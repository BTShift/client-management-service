# Use the official .NET 8 SDK image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set build arguments for package registry access
ARG GITHUB_TOKEN
ARG GITHUB_ACTOR

# Set working directory
WORKDIR /app

# Copy csproj files and restore dependencies
COPY src/ClientManagement.Api/ClientManagement.Api.csproj src/ClientManagement.Api/
COPY src/ClientManagement.Application/ClientManagement.Application.csproj src/ClientManagement.Application/
COPY src/ClientManagement.Contract/ClientManagement.Contract.csproj src/ClientManagement.Contract/
COPY src/ClientManagement.Domain/ClientManagement.Domain.csproj src/ClientManagement.Domain/
COPY src/ClientManagement.Infrastructure/ClientManagement.Infrastructure.csproj src/ClientManagement.Infrastructure/
COPY ClientManagement.sln .

# Copy NuGet configuration if it exists
COPY NuGet.Config* ./

# Restore dependencies
RUN dotnet restore

# Copy the remaining source code
COPY src/ src/

# Build the application
RUN dotnet build -c Release --no-restore

# Publish the application
RUN dotnet publish src/ClientManagement.Api/ClientManagement.Api.csproj -c Release -o /app/publish --no-build

# Use the official .NET 8 runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Set working directory
WORKDIR /app

# Copy the published application
COPY --from=build /app/publish .

# Expose ports
EXPOSE 8080
EXPOSE 5001

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Run the application
ENTRYPOINT ["dotnet", "ClientManagement.Api.dll"]