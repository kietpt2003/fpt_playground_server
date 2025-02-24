# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["FPTPlaygroundServer/FPTPlaygroundServer.csproj", "FPTPlaygroundServer/"]
RUN dotnet restore "./FPTPlaygroundServer/FPTPlaygroundServer.csproj"
COPY . .
WORKDIR "/src/FPTPlaygroundServer"
RUN dotnet build "./FPTPlaygroundServer.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./FPTPlaygroundServer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Define environment variables (these will be overridden at runtime)
ARG DB_POSTGRES_CONNECTION_STRING
ARG DB_REDIS_CONNECTION_STRING
ARG JWT_ISSUER
ARG JWT_AUDIENCE
ARG JWT_SIGNING_KEY
ARG SMTP_MAIL
ARG SMTP_PASSWORD
ARG PAYOS_CLIENT_ID
ARG PAYOS_API_KEY
ARG PAYOS_CHECKSUM_KEY
ARG GOOGLE_STORAGE_BUCKET

# Persist ARG values as ENV so they remain available at runtime
ENV DB_POSTGRES_CONNECTION_STRING=$DB_POSTGRES_CONNECTION_STRING
ENV DB_REDIS_CONNECTION_STRING=$DB_REDIS_CONNECTION_STRING
ENV JWT_ISSUER=$JWT_ISSUER
ENV JWT_AUDIENCE=$JWT_AUDIENCE
ENV JWT_SIGNING_KEY=$JWT_SIGNING_KEY
ENV SMTP_MAIL=$SMTP_MAIL
ENV SMTP_PASSWORD=$SMTP_PASSWORD
ENV PAYOS_CLIENT_ID=$PAYOS_CLIENT_ID
ENV PAYOS_API_KEY=$PAYOS_API_KEY
ENV PAYOS_CHECKSUM_KEY=$PAYOS_CHECKSUM_KEY
ENV GOOGLE_STORAGE_BUCKET=$GOOGLE_STORAGE_BUCKET

ENTRYPOINT ["dotnet", "FPTPlaygroundServer.dll"]