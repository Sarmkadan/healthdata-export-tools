# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =====================================================================

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder
WORKDIR /src

# Copy project files and restore dependencies
COPY ["healthdata-export-tools/healthdata-export-tools.csproj", "healthdata-export-tools/"]
RUN dotnet restore "healthdata-export-tools/healthdata-export-tools.csproj"

# Copy source code and build
COPY . .
RUN dotnet build "healthdata-export-tools/healthdata-export-tools.csproj" -c Release -o /app/build

# Publish the application
RUN dotnet publish "healthdata-export-tools/healthdata-export-tools.csproj" -c Release -o /app/publish \
    --no-restore \
    -p:PublishSingleFile=false \
    -p:PublishTrimmed=false

# Final runtime stage
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app

# Copy published application from build stage
COPY --from=builder /app/publish .

# Create directories for input/output with proper permissions
RUN mkdir -p /data/exports /data/output /data/test-input /data/test-output && \
    chown -R 1001:1001 /data && \
    chmod -R 755 /data

# Set non-root user for security (UID 1001 matches runtime image default)
USER 1001

# Set environment variables
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    DOTNET_gcServer=1 \
    DOTNET_gcConcurrent=1 \
    ASPNETCORE_ENVIRONMENT=Production

# Health check - verify the application can run and complete successfully
HEALTHCHECK --interval=30s --timeout=30s --start-period=15s --retries=3 \
    CMD dotnet /app/healthdata-export-tools.dll /data/test-input /data/test-output /data/test.db 2>&1 | grep -q "✅ Export completed successfully" || exit 1

# Application configuration via environment variables
ENV INPUT_PATH=/data/exports \
    OUTPUT_PATH=/data/output \
    DATABASE_PATH=/data/healthdata.db \
    EXPORT_FORMAT=All \
    VERBOSE_LOGGING=false

# Entrypoint with default arguments
ENTRYPOINT ["dotnet"]

# Default command - run the application with default paths
CMD ["healthdata-export-tools.dll", "/data/exports", "/data/output", "/data/healthdata.db"]