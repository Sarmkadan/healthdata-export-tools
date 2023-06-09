# Configuration Guide

Complete guide to configuring Health Data Export Tools for your environment.

## Configuration Methods

Health Data Export Tools can be configured in three ways:

1. **Code Configuration** (Programmatic)
2. **Configuration Files** (JSON/appsettings.json)
3. **Environment Variables** (System/Docker)

## 1. Code Configuration

Configure directly in your application:

```csharp
var options = new HealthDataExportOptions
{
    InputPath = "./exports/",
    OutputPath = "./output/",
    DatabasePath = "./healthdata.db",
    ExportFormat = ExportFormat.Json,
    ValidateData = true,
    PerformAnalysis = true,
    VerboseLogging = false,
    CacheEnabled = true,
    CacheDurationSeconds = 3600,
    MaxRetries = 3,
    TimeoutSeconds = 300
};
```

**Pros**: Type-safe, compile-time checking, IDE support
**Cons**: Requires code changes for different configurations

## 2. Configuration Files (Recommended)

### appsettings.json

Create an `appsettings.json` file in your project root:

```json
{
  "HealthDataExport": {
    "InputPath": "./exports/",
    "OutputPath": "./output/",
    "DatabasePath": "./healthdata.db",
    "ExportFormat": "All",
    "ValidateData": true,
    "PerformAnalysis": true,
    "VerboseLogging": false,
    "CacheEnabled": true,
    "CacheDurationSeconds": 3600,
    "MaxRetries": 3,
    "TimeoutSeconds": 300
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  }
}
```

### Load Configuration

```csharp
var configBuilder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("appsettings.Development.json", optional: true)
    .AddEnvironmentVariables();

var configuration = configBuilder.Build();
var options = configuration.GetSection("HealthDataExport")
    .Get<HealthDataExportOptions>()!;
```

## 3. Environment Variables

### Set Environment Variables

**Windows (PowerShell)**:
```powershell
$env:HealthDataExport__InputPath = "C:\health\exports\"
$env:HealthDataExport__OutputPath = "C:\health\output\"
$env:HealthDataExport__ExportFormat = "Json"
```

**Linux/macOS (Bash)**:
```bash
export HealthDataExport__InputPath=/data/exports/
export HealthDataExport__OutputPath=/data/output/
export HealthDataExport__ExportFormat=Json
```

**Docker**:
```yaml
environment:
  - HealthDataExport__InputPath=/data/exports
  - HealthDataExport__OutputPath=/data/output
  - HealthDataExport__ExportFormat=All
```

### Load from Environment Variables

```csharp
var configBuilder = new ConfigurationBuilder()
    .AddEnvironmentVariables("HealthDataExport__");

var configuration = configBuilder.Build();
```

## Configuration Options Reference

### Paths

#### InputPath
- **Type**: `string`
- **Default**: `./exports/`
- **Description**: Directory containing health export ZIP files
- **Example**: `"./exports/"` or `/var/lib/healthdata/exports/`

#### OutputPath
- **Type**: `string`
- **Default**: `./output/`
- **Description**: Directory for exported files
- **Example**: `"./output/"` or `/var/lib/healthdata/output/`

#### DatabasePath
- **Type**: `string`
- **Default**: `./healthdata.db`
- **Description**: SQLite database file location
- **Example**: `./healthdata.db` or `/var/lib/healthdata/healthdata.db`

### Export Settings

#### ExportFormat
- **Type**: `ExportFormat` enum
- **Default**: `Json`
- **Values**:
  - `Csv` - Export to CSV format only
  - `Json` - Export to JSON format only
  - `Xml` - Export to XML format only
  - `All` - Export to all formats
- **Example**: `"All"`

#### ValidateData
- **Type**: `bool`
- **Default**: `true`
- **Description**: Validate health data before processing
- **Impact**: Slower processing, more reliable results

#### PerformAnalysis
- **Type**: `bool`
- **Default**: `true`
- **Description**: Run analytics and generate reports
- **Impact**: Slight performance overhead, generates insights

### Caching

#### CacheEnabled
- **Type**: `bool`
- **Default**: `true`
- **Description**: Enable in-memory caching of parsed data
- **Impact**: Faster repeated access, uses more memory

#### CacheDurationSeconds
- **Type**: `int`
- **Default**: `3600` (1 hour)
- **Description**: How long to keep cached data
- **Valid Range**: 60-86400 (1 minute to 1 day)

### Performance

#### MaxRetries
- **Type**: `int`
- **Default**: `3`
- **Description**: Retry failed operations
- **Valid Range**: 1-10

#### TimeoutSeconds
- **Type**: `int`
- **Default**: `300` (5 minutes)
- **Description**: Operation timeout
- **Valid Range**: 10-3600

### Logging

#### VerboseLogging
- **Type**: `bool`
- **Default**: `false`
- **Description**: Enable detailed logging output
- **When to Enable**: Debugging, troubleshooting

## Complete appsettings.json Example

```json
{
  "HealthDataExport": {
    "InputPath": "./exports/",
    "OutputPath": "./output/",
    "DatabasePath": "./healthdata.db",
    "ExportFormat": "All",
    "ValidateData": true,
    "PerformAnalysis": true,
    "VerboseLogging": false,
    "CacheEnabled": true,
    "CacheDurationSeconds": 3600,
    "MaxRetries": 3,
    "TimeoutSeconds": 300
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    },
    "Console": {
      "IncludeScopes": true
    }
  },
  "AllowedHosts": "*"
}
```

## Environment-Specific Configuration

### Development Configuration

**appsettings.Development.json**:
```json
{
  "HealthDataExport": {
    "InputPath": "./test-data/exports/",
    "OutputPath": "./test-data/output/",
    "VerboseLogging": true,
    "CacheDurationSeconds": 60
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug"
    }
  }
}
```

### Production Configuration

**appsettings.Production.json**:
```json
{
  "HealthDataExport": {
    "InputPath": "/var/lib/healthdata/exports/",
    "OutputPath": "/var/lib/healthdata/output/",
    "DatabasePath": "/var/lib/healthdata/healthdata.db",
    "VerboseLogging": false,
    "CacheDurationSeconds": 7200,
    "MaxRetries": 5,
    "TimeoutSeconds": 600
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  }
}
```

## Configuration Validation

### Validate Configuration at Startup

```csharp
var options = configuration.GetSection("HealthDataExport")
    .Get<HealthDataExportOptions>()!;

var errors = options.Validate();
if (errors.Count > 0)
{
    Console.WriteLine("Configuration errors:");
    foreach (var error in errors)
        Console.WriteLine($"  • {error}");
    Environment.Exit(1);
}
```

### Common Validation Errors

**"InputPath does not exist"**
- Solution: Create the directory or correct the path

**"OutputPath is not writable"**
- Solution: Check file permissions
- Linux: `chmod 755 /output/`

**"DatabasePath is invalid"**
- Solution: Verify parent directory exists

## Docker Configuration

### Using Environment Variables with Docker

```bash
docker run \
  -e HealthDataExport__InputPath=/data/exports \
  -e HealthDataExport__OutputPath=/data/output \
  -e HealthDataExport__ExportFormat=All \
  -v $(pwd)/exports:/data/exports \
  -v $(pwd)/output:/data/output \
  healthdata-export-tools:latest
```

### Using .env File with Docker Compose

**.env**:
```env
INPUT_PATH=./exports/
OUTPUT_PATH=./output/
DATABASE_PATH=./healthdata.db
EXPORT_FORMAT=All
```

**docker-compose.yml**:
```yaml
services:
  healthdata-exporter:
    environment:
      - HealthDataExport__InputPath=${INPUT_PATH}
      - HealthDataExport__OutputPath=${OUTPUT_PATH}
      - HealthDataExport__ExportFormat=${EXPORT_FORMAT}
```

## Performance Tuning

### For Large Datasets

```json
{
  "HealthDataExport": {
    "CacheEnabled": true,
    "CacheDurationSeconds": 7200,
    "TimeoutSeconds": 600,
    "MaxRetries": 5
  }
}
```

### For Rapid Processing

```json
{
  "HealthDataExport": {
    "CacheEnabled": true,
    "CacheDurationSeconds": 300,
    "TimeoutSeconds": 60,
    "MaxRetries": 1,
    "ValidateData": false
  }
}
```

### For Reliable Processing

```json
{
  "HealthDataExport": {
    "ValidateData": true,
    "TimeoutSeconds": 300,
    "MaxRetries": 5,
    "CacheEnabled": true
  }
}
```

## Configuration Priority

When multiple configuration sources are specified, the priority (highest to lowest) is:

1. **Environment Variables** (override everything)
2. **appsettings.{Environment}.json** (overrides appsettings.json)
3. **appsettings.json** (default values)
4. **Code Configuration** (fallback)

Example with Environment: Development

```
appsettings.json (base)
  ↓
appsettings.Development.json (override)
  ↓
Environment Variables (final override)
```

## Troubleshooting Configuration

### Configuration Not Applied

1. Verify correct JSON syntax in appsettings.json
2. Check configuration keys match exactly (case-sensitive)
3. Ensure file is in correct directory
4. Check environment-specific file is loaded

### Validation Fails at Startup

```csharp
try
{
    var options = configuration.GetSection("HealthDataExport")
        .Get<HealthDataExportOptions>()!;
    
    var errors = options.Validate();
    if (errors.Count > 0)
        throw new InvalidOperationException($"Configuration error: {string.Join(", ", errors)}");
}
catch (Exception ex)
{
    logger.LogError(ex, "Configuration validation failed");
    throw;
}
```

## Configuration Examples

See [examples/](../examples/) directory for complete working examples using different configuration methods.

## Next Steps

- Review [Getting Started](getting-started.md) for setup
- Check [API Reference](api-reference.md) for available options
- See [Deployment Guide](deployment.md) for production setup
