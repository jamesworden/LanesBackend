FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
WORKDIR /src/LanesBackendLauncher

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /src/LanesBackendLauncher
COPY --from=build-env /src/LanesBackendLauncher/out .
ENTRYPOINT ["dotnet", "LanesBackendLauncher.dll"]