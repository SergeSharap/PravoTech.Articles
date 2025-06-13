# Build stage for the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0.0 AS base
WORKDIR /app
EXPOSE 8080

# Build stage for SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG BUILD_VERSION=1.0.0
WORKDIR /src
COPY . .
RUN dotnet restore "./PravoTech.Articles.csproj"

# Install EF Core tools
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

# Build the application
RUN dotnet build "./PravoTech.Articles.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/build \
    /p:Version=$BUILD_VERSION

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./PravoTech.Articles.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false \
    /p:Version=$BUILD_VERSION

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PravoTech.Articles.dll"]