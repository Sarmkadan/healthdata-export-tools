# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder

WORKDIR /src

COPY ["healthdata-export-tools/healthdata-export-tools.csproj", "healthdata-export-tools/"]
RUN dotnet restore "healthdata-export-tools/healthdata-export-tools.csproj"

COPY . .
RUN dotnet build "healthdata-export-tools/healthdata-export-tools.csproj" -c Release -o /app/build

RUN dotnet publish "healthdata-export-tools/healthdata-export-tools.csproj" -c Release -o /app/publish

# Final runtime stage
FROM mcr.microsoft.com/dotnet/runtime:10.0

WORKDIR /app

COPY --from=builder /app/publish .

# Create directories for input/output
RUN mkdir -p /data/exports /data/output

# Set permissions
RUN chmod 755 /data/exports /data/output

VOLUME ["/data/exports", "/data/output"]

# Set environment variables
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD dotnet /app/healthdata-export-tools.dll --help > /dev/null || exit 1

ENTRYPOINT ["./healthdata-export-tools", "/data/exports", "/data/output", "/data/healthdata.db"]
