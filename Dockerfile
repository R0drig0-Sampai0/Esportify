# Use the official .NET 8.0 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory
WORKDIR /app

# Copy the project file and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy the rest of the application files
COPY . ./

# Build the application
RUN dotnet publish -c Release -o /app/publish

# Use the official .NET 8.0 runtime image for running the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Set the working directory
WORKDIR /app

# Copy the published application from the build stage
COPY --from=build /app/publish .

# Create directories for uploaded images
RUN mkdir -p /app/wwwroot/images/games && \
    mkdir -p /app/wwwroot/images/profile && \
    mkdir -p /app/wwwroot/images/teams && \
    mkdir -p /app/wwwroot/images/tournaments

# Set permissions for uploaded images directories
RUN chmod -R 755 /app/wwwroot/images

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production

# Create startup script
RUN echo '#!/bin/bash' > /app/start.sh && \
    echo 'export ASPNETCORE_URLS="http://+:${PORT:-10000}"' >> /app/start.sh && \
    echo 'exec dotnet Esportify.dll' >> /app/start.sh && \
    chmod +x /app/start.sh

# Run the application
CMD ["/app/start.sh"]
