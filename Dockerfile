# 1) Build stage - Changed to 9.0
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["HellavorX/HellavorX.csproj", "HellavorX/"]
RUN dotnet restore "HellavorX/HellavorX.csproj"

# Copy everything else and publish
COPY . .
WORKDIR /src/HellavorX
RUN dotnet publish "HellavorX.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 2) Runtime stage - Changed to 9.0
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Copy published output
COPY --from=build /app/publish .

# Copy entrypoint helper
COPY entrypoint.sh /app/entrypoint.sh
RUN chmod +x /app/entrypoint.sh

# Expose standard HTTP/HTTPS ports
EXPOSE 80
EXPOSE 443

# Set a sane default
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["/app/entrypoint.sh"]
