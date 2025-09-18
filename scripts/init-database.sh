#!/bin/bash

# Database initialization script for production
# This script should be run once after deployment to initialize the database

echo "🚀 Starting database initialization..."

# Set environment variables
export ASPNETCORE_ENVIRONMENT=Production
export INITIALIZE_DATABASE=true

# Run the application with database initialization
dotnet Esportify.dll

echo "✅ Database initialization completed!"