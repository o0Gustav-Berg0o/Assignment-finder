FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/AssignmentFinder.Domain/AssignmentFinder.Domain.csproj src/AssignmentFinder.Domain/
COPY src/AssignmentFinder.Infrastructure/AssignmentFinder.Infrastructure.csproj src/AssignmentFinder.Infrastructure/
COPY src/AssignmentFinder.Scraping/AssignmentFinder.Scraping.csproj src/AssignmentFinder.Scraping/
COPY src/AssignmentFinder.Api/AssignmentFinder.Api.csproj src/AssignmentFinder.Api/
COPY src/AssignmentFinder.Worker/AssignmentFinder.Worker.csproj src/AssignmentFinder.Worker/
COPY src/AssignmentFinder.Tests/AssignmentFinder.Tests.csproj src/AssignmentFinder.Tests/
RUN dotnet restore src/AssignmentFinder.Api/AssignmentFinder.Api.csproj

COPY src/ src/
RUN dotnet publish src/AssignmentFinder.Api/AssignmentFinder.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "AssignmentFinder.Api.dll"]
