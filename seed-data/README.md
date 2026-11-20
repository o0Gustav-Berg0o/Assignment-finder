# seed-data

Output folder for `AssignmentFinder.SeedExporter` (`src/AssignmentFinder.SeedExporter`). Generated
`.sql` files here are gitignored - they contain encrypted broker passwords and the Data Protection
key that decrypts them, so treat them as secrets (copy over a secure channel, delete when done).

## Exporting from a running environment

```
dotnet run --project src/AssignmentFinder.SeedExporter -- "<connection-string>"
```

If the target SQL Server isn't reachable from your shell directly (e.g. only via a Docker network),
run it inside that network instead, same as the API/Worker containers do:

```
docker run --rm --network <compose-project>_default -v "$PWD:/src" -w /src \
  mcr.microsoft.com/dotnet/sdk:10.0 \
  dotnet run --project src/AssignmentFinder.SeedExporter -- "Server=db,1433;Database=AssignmentFinder;User Id=sa;Password=<sa-password>;TrustServerCertificate=True;"
```

This writes `seed-data/seed-brokers-and-searches.sql`.

## Applying to a new (cloud) database

1. Apply EF Core migrations to the target database first (`dotnet ef database update`, or let the
   API apply them on startup as it already does).
2. Make sure any `SearchCriterion` rows' `UserId` already exists in the target's `AspNetUsers` table -
   migrating/seeding users is a separate step, not covered by this script.
3. Run the generated `.sql` file against the target database.
