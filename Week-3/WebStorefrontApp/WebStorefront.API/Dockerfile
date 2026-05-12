# Multi-stage Dockerfile for the WebStorefront Web API (ASP.NET Core 10).
# Build stage compiles + publishes; runtime stage holds only the published output.
# Build context = WebStorefront.API/ (run `docker build .` from this folder).

# syntax=docker/dockerfile:1.7

# ---- Stage 1: build ---------------------------------------------------------
# SDK image has the dotnet CLI, MSBuild, and analyzers. Heavy, so we discard it
# after publishing.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj first and restore so source edits don't bust the restore layer.
COPY TechSupportSystem.csproj ./
RUN dotnet restore TechSupportSystem.csproj

# Copy the rest of the source and publish in Release mode.
# UseAppHost=false skips the native bootstrapper -- the runtime image runs
# `dotnet TechSupportSystem.dll` directly, which is smaller.
COPY . .
RUN dotnet publish TechSupportSystem.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# ---- Stage 2: runtime -------------------------------------------------------
# aspnet image has only the ASP.NET Core runtime -- no SDK, no compilers.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Bind Kestrel to 8080 (non-privileged Linux convention). Map with -p locally:
#   docker run --rm -p 8080:8080 webstorefront-api:ci
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Swagger is gated on Development in Program.cs. The official aspnet image
# defaults ASPNETCORE_ENVIRONMENT to Production; override at run time to see
# Swagger:
#   docker run --rm -p 8080:8080 -e ASPNETCORE_ENVIRONMENT=Development webstorefront-api:ci

ENTRYPOINT ["dotnet", "TechSupportSystem.dll"]
