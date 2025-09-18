# Use the official .NET 8.0 SDK image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copy project file and restore
COPY ["Esportify.csproj", "./"]
RUN dotnet restore "Esportify.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "Esportify.csproj" -c Release -o /app/build
RUN dotnet publish "Esportify.csproj" -c Release -o /app/publish --no-restore

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy the app
COPY --from=build /app/publish .

# Create upload directories
RUN mkdir -p wwwroot/images/games wwwroot/images/profile wwwroot/images/teams wwwroot/images/tournaments

# Set the environment
ENV ASPNETCORE_ENVIRONMENT=Production

# Use the PORT environment variable
CMD ASPNETCORE_URLS=http://*:$PORT dotnet Esportify.dll
