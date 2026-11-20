# Deploying to Azure (cheapest viable setup)

This is a runbook for moving the app from local docker-compose to Azure, optimized for lowest
cost. It assumes you (not an agent) run the `az` commands — nothing here executes automatically.

## Architecture

| Today (docker-compose) | Azure |
|---|---|
| `db` (SQL Server container) | **Azure SQL Database, Serverless** (General Purpose, Gen5, 0.5 vCore min, auto-pause after 60 min idle) |
| `api` container | **Container App**, 0.25 vCPU / 0.5 GiB, min replicas **0** (scales to zero between requests) |
| `worker` container | **Azure Function (Consumption plan), Timer trigger** — recommended over an always-on Container App; see below |
| `web` container | **Container App**, 0.25 vCPU / 0.5 GiB, min replicas **0**, external ingress (the public entry point) |
| local image builds | **ghcr.io** (GitHub Container Registry, free) via `.github/workflows/build-and-push.yml` |

**Why `worker` isn't a Container App:** it runs its own in-process Quartz cron
(`Scan__CronSchedule`, every 3 hours by default) rather than being woken up externally, so an
always-on Container App is the only way to run it as-is (~$12-15/mo, since min replicas 0 would
never fire it). The actually-cheapest option is an **Azure Function on the Consumption plan**
with a `TimerTrigger` matching the cron schedule: Consumption gives 1M free executions + 400,000
GB-s/month, and running a few-minutes scan every 3 hours (~240 runs/month) stays comfortably
inside that free grant — realistically **$0/month** for the worker, instead of ~$12-15/mo for an
always-on Container App or a Container Apps Job (which bills per-run, cheaper than always-on but
not free).

This needs the same code change either way: swap the Worker's hosted Quartz scheduler for a
single triggered entrypoint (`TimerTrigger` for Functions, Cron trigger for a Container Apps Job).
**One real constraint with Functions Consumption:** it's a sandboxed environment with no control
over OS-level dependencies, so Playwright/headless Chromium won't reliably run there (that needs
Premium/Elastic Premium or a custom container). None of the 3 currently-enabled brokers
(Brainville, Cinode Market, TED) use Playwright, so this doesn't block today's setup — it would
only matter if a future login-gated SPA connector (the `PlaywrightConnectorBase.LoginConfig`
mechanism added this session) gets enabled; that broker would need a different hosting plan.

This refactor (and the Functions project it implies) is **not yet done** — this doc records the
recommendation for when it's picked up. Until then, deploy `worker` as an always-on Container App
per the steps below.

**Networking:** `web`'s nginx proxies `/api/` to the `api` app. Locally this is
`http://api:8080` (docker-compose's internal DNS); in Container Apps it's `api`'s internal FQDN
within the same Container Apps Environment. This is now configurable via the `API_INTERNAL_HOST`
env var (see `web/assignment-finder-ui/nginx.conf.template` and the `Dockerfile`'s
`ENV API_INTERNAL_HOST=api:8080` default) — override it per-environment, no image rebuild needed
for a different target.

## Estimated cost (West Europe, approximate — confirm with the Azure Pricing Calculator)

| Item | Estimate |
|---|---|
| Azure SQL Database Serverless (0.5 vCore min, ~5 GB storage) | ~$5–15/mo |
| `worker` as always-on Container App (current plan below) | ~$12–15/mo |
| `worker` as Azure Function, Consumption plan (recommended, not yet implemented) | ~$0/mo |
| `api` + `web` Container Apps (scale-to-zero) | ~$0–5/mo combined (mostly inside the free monthly grant) |
| Container Apps Environment | $0 |
| ghcr.io | $0 |
| **Total** | **~$20–30/month now, ~$5–20/month once `worker` moves to Functions** |

## Steps

### 1. Build and push images

Push to `main` (or run the workflow manually) to build and publish:
- `ghcr.io/<owner>/assignment-finder-api:latest`
- `ghcr.io/<owner>/assignment-finder-worker:latest`
- `ghcr.io/<owner>/assignment-finder-web:latest`

### 2. Create the Azure SQL Database

```powershell
az group create --name assignment-finder --location westeurope

az sql server create `
  --name <unique-server-name> `
  --resource-group assignment-finder `
  --location westeurope `
  --admin-user sqladmin `
  --admin-password "<strong-password>"

az sql db create `
  --resource-group assignment-finder `
  --server <unique-server-name> `
  --name AssignmentFinder `
  --edition GeneralPurpose `
  --family Gen5 `
  --capacity 1 `
  --compute-model Serverless `
  --min-capacity 0.5 `
  --auto-pause-delay 60

# Allow Azure services (Container Apps) to reach it
az sql server firewall-rule create `
  --resource-group assignment-finder `
  --server <unique-server-name> `
  --name AllowAzureServices `
  --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0
```

EF Core migrations apply automatically on API startup (same as today), so no separate migration
step is needed — just make sure the connection string below is correct before the `api` app first
starts.

### 3. Migrate Brokers / Saved searches / Data Protection keys

Run the exporter built for exactly this (see `seed-data/README.md`), against the *current* local
database, then apply the resulting script to the *new* Azure SQL database (after step 2's EF
migrations have run at least once against it, so the tables exist):

```bash
docker run --rm --network <compose-project>_default -v "$PWD:/src" -w /src \
  mcr.microsoft.com/dotnet/sdk:10.0 \
  dotnet run --project src/AssignmentFinder.SeedExporter -- \
  "Server=db,1433;Database=AssignmentFinder;User Id=sa;Password=<local-sa-password>;TrustServerCertificate=True;"
```

Then run the generated `seed-data/seed-brokers-and-searches.sql` against the Azure SQL database
(e.g. via `sqlcmd` or the Azure Portal's Query Editor). Note: `SearchCriteria` rows reference
`AspNetUsers(Id)` — register the same admin user in the new environment first (or delete those
rows from the script) or the MERGE will fail its foreign-key check.

### 4. Create the Container Apps Environment and apps

```powershell
az extension add --name containerapp
az provider register --namespace Microsoft.App

az containerapp env create `
  --name assignment-finder-env `
  --resource-group assignment-finder `
  --location westeurope

# api - internal only, scales to zero
az containerapp create `
  --name api `
  --resource-group assignment-finder `
  --environment assignment-finder-env `
  --image ghcr.io/<owner>/assignment-finder-api:latest `
  --cpu 0.25 --memory 0.5Gi `
  --min-replicas 0 --max-replicas 1 `
  --ingress internal --target-port 8080 `
  --secrets connection-string="Server=tcp:<unique-server-name>.database.windows.net,1433;Database=AssignmentFinder;User Id=sqladmin;Password=<strong-password>;TrustServerCertificate=False;Encrypt=True;" jwt-key="<jwt-signing-key>" admin-password="<admin-password>" `
  --env-vars ASPNETCORE_ENVIRONMENT=Production ConnectionStrings__Default=secretref:connection-string Jwt__SigningKey=secretref:jwt-key Admin__Email="<admin-email>" Admin__Password=secretref:admin-password

# worker - always-on (see note above on why it can't scale to zero yet)
az containerapp create `
  --name worker `
  --resource-group assignment-finder `
  --environment assignment-finder-env `
  --image ghcr.io/<owner>/assignment-finder-worker:latest `
  --cpu 0.25 --memory 0.5Gi `
  --min-replicas 1 --max-replicas 1 `
  --ingress disabled `
  --secrets connection-string="<same as api>" smtp-password="<smtp-password>" `
  --env-vars DOTNET_ENVIRONMENT=Production ConnectionStrings__Default=secretref:connection-string Scan__CronSchedule="0 0 */3 * * ?" Email__Host="<smtp-host>" Email__Port=587 Email__FromAddress="<smtp-from>" Email__Username="<smtp-username>" Email__Password=secretref:smtp-password Email__UseSsl=true

# web - public entry point, scales to zero
az containerapp create `
  --name web `
  --resource-group assignment-finder `
  --environment assignment-finder-env `
  --image ghcr.io/<owner>/assignment-finder-web:latest `
  --cpu 0.25 --memory 0.5Gi `
  --min-replicas 0 --max-replicas 2 `
  --ingress external --target-port 80
```

### 5. Point `web` at `api`'s internal address

```powershell
az containerapp show --name api --resource-group assignment-finder --query "properties.configuration.ingress.fqdn" -o tsv
```

Set that FQDN (it looks like `api.internal.<env-id>.westeurope.azurecontainerapps.io`) as the
`web` app's `API_INTERNAL_HOST` env var:

```powershell
az containerapp update --name web --resource-group assignment-finder --set-env-vars API_INTERNAL_HOST="<fqdn-from-above>"
```

### 6. Verify

- Open the `web` app's public FQDN (`az containerapp show --name web ... --query "properties.configuration.ingress.fqdn"`).
- Log in with the existing admin account.
- Check the Brokers page shows all 8 brokers with their current `IsEnabled`/login config intact
  (confirms step 3's data migration worked).
- Click "Scan all now" and confirm new assignments appear (confirms `api`/`worker` can both reach
  the new Azure SQL database, and that migrated Data Protection keys correctly decrypt the broker
  passwords).

## Follow-up optimization (not done here): move `worker` to Azure Functions

Recommended: convert `worker` from an always-on Container App to an **Azure Function
(Consumption plan)** with a `TimerTrigger` matching `Scan__CronSchedule`, removing the in-process
Quartz scheduler in favor of Azure's own trigger. This is the single biggest remaining cost lever
(~$12–15/mo → ~$0/mo) but requires a Worker code change:

- New/repurposed project using the .NET isolated worker Functions model
  (`FunctionsApplication.CreateBuilder(args)`), registering the same DI services
  (`AppDbContext`, `ScanPipeline`, connectors, `BrokerCredentialProtector`) the Worker already
  wires up today.
- Replace the hosted Quartz scheduler with one `[Function("ScanTimer")]` method using
  `[TimerTrigger("0 0 */3 * * *")]` (NCRONTAB, 6-field like Quartz's own cron but not identical
  syntax — double-check the translation) that calls `scanPipeline.RunAsync(ct)` once per
  invocation.
- **Constraint to accept:** Functions Consumption can't run Playwright/headless Chromium (no
  control over OS dependencies; needs Premium/Elastic Premium or a custom container for that).
  Fine today since no enabled broker uses `PlaywrightConnectorBase` — would need revisiting if a
  login-gated SPA connector (Verama/Onsiter/etc., using the `LoginConfig` mechanism added this
  session) is ever enabled.

(Alternative, not recommended: a **Container Apps Job** with a Cron trigger — same code-shape
change, but bills per-run instead of being free, so cheaper than always-on but not free like
Functions Consumption.)
