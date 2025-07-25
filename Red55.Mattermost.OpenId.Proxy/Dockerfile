# syntax=docker/dockerfile:1.7-labs
# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0-noble AS base
USER root
ENV DEBIAN_FRONTEND=noniteractive
RUN apt update && apt install -y --no-install-recommends \
    curl \ 
    && rm -rf /var/lib/apt/lists/*
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8443



# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0-noble AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Red55.Mattermost.OpenId.Proxy/Directory.Packages.props", "Red55.Mattermost.OpenId.Proxy/"]
COPY ["Red55.Mattermost.OpenId.Proxy/Directory.Build.props", "Red55.Mattermost.OpenId.Proxy/"]
COPY ["Red55.Mattermost.OpenId.Proxy/Red55.Mattermost.OpenId.Proxy.csproj", "Red55.Mattermost.OpenId.Proxy/"]
RUN dotnet restore "./Red55.Mattermost.OpenId.Proxy/Red55.Mattermost.OpenId.Proxy.csproj"
COPY --exclude=./.git/* . .

WORKDIR "/src/Red55.Mattermost.OpenId.Proxy"
RUN --mount=type=bind,source=./.git,target=/src/Red55.Mattermost.OpenId.Proxy/.git/ dotnet build "Red55.Mattermost.OpenId.Proxy.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN --mount=type=bind,source=./.git,target=/src/Red55.Mattermost.OpenId.Proxy/.git/ dotnet publish "Red55.Mattermost.OpenId.Proxy.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY localhost.* /home/app/
ENTRYPOINT ["dotnet", "Red55.Mattermost.OpenId.Proxy.dll"]