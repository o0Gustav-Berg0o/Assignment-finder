FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY AssignmentFinder.sln .
COPY src/AssignmentFinder.Domain/AssignmentFinder.Domain.csproj src/AssignmentFinder.Domain/
COPY src/AssignmentFinder.Infrastructure/AssignmentFinder.Infrastructure.csproj src/AssignmentFinder.Infrastructure/
COPY src/AssignmentFinder.Scraping/AssignmentFinder.Scraping.csproj src/AssignmentFinder.Scraping/
COPY src/AssignmentFinder.Api/AssignmentFinder.Api.csproj src/AssignmentFinder.Api/
COPY src/AssignmentFinder.Worker/AssignmentFinder.Worker.csproj src/AssignmentFinder.Worker/
COPY src/AssignmentFinder.Tests/AssignmentFinder.Tests.csproj src/AssignmentFinder.Tests/
RUN dotnet restore src/AssignmentFinder.Worker/AssignmentFinder.Worker.csproj

COPY src/ src/
RUN dotnet publish src/AssignmentFinder.Worker/AssignmentFinder.Worker.csproj -c Release -o /app --no-restore

# Playwright's own image ships Chromium + all native deps already installed/verified for this
# Playwright version, which is far less brittle than apt-get'ing deps into a plain aspnet image.
FROM mcr.microsoft.com/playwright/dotnet:v1.49.0-noble AS runtime
WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ["dotnet", "AssignmentFinder.Worker.dll"]
