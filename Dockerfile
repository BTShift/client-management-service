# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Accept build arguments for GitHub Package Registry authentication
ARG GITHUB_TOKEN
ARG GITHUB_ACTOR

# Generate NuGet.Config for accessing private packages
RUN echo '<?xml version="1.0" encoding="utf-8"?>' > NuGet.Config && \
    echo '<configuration>' >> NuGet.Config && \
    echo '  <packageSources>' >> NuGet.Config && \
    echo '    <clear />' >> NuGet.Config && \
    echo '    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />' >> NuGet.Config && \
    echo '    <add key="github-btshift" value="https://nuget.pkg.github.com/BTShift/index.json" />' >> NuGet.Config && \
    echo '  </packageSources>' >> NuGet.Config && \
    echo '  <packageSourceCredentials>' >> NuGet.Config && \
    echo '    <github-btshift>' >> NuGet.Config && \
    echo "      <add key=\"Username\" value=\"${GITHUB_ACTOR}\" />" >> NuGet.Config && \
    echo "      <add key=\"ClearTextPassword\" value=\"${GITHUB_TOKEN}\" />" >> NuGet.Config && \
    echo '    </github-btshift>' >> NuGet.Config && \
    echo '  </packageSourceCredentials>' >> NuGet.Config && \
    echo '</configuration>' >> NuGet.Config

# Copy solution and project files
COPY ClientManagement.sln ./
COPY src/ClientManagement.Api/ClientManagement.Api.csproj ./src/ClientManagement.Api/
COPY src/ClientManagement.Application/ClientManagement.Application.csproj ./src/ClientManagement.Application/
COPY src/ClientManagement.Contract/ClientManagement.Contract.csproj ./src/ClientManagement.Contract/
COPY src/ClientManagement.Domain/ClientManagement.Domain.csproj ./src/ClientManagement.Domain/
COPY src/ClientManagement.Infrastructure/ClientManagement.Infrastructure.csproj ./src/ClientManagement.Infrastructure/
COPY tests/ClientManagement.IntegrationTests/ClientManagement.IntegrationTests.csproj ./tests/ClientManagement.IntegrationTests/
COPY tests/ClientManagement.UnitTests/ClientManagement.UnitTests.csproj ./tests/ClientManagement.UnitTests/

# Restore dependencies
RUN dotnet restore

# Copy everything else
COPY . ./

# Build and publish
RUN dotnet publish src/ClientManagement.Api/ClientManagement.Api.csproj -c Release -o out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

# Set default environment variables
ENV PORT=8080
ENV GRPC_PORT=5000

# Expose both ports
EXPOSE 8080
EXPOSE 5000

ENTRYPOINT ["dotnet", "ClientManagement.Api.dll"]