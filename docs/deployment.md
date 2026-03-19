# Deployment Guide

Instructions for deploying Health Data Export Tools to production environments.

## Deployment Strategies

### Strategy 1: Command-Line Tool Deployment

Deploy as a standalone CLI application for scheduled batch processing.

#### On Windows

1. **Build Release Binary**
```bash
dotnet publish -c Release -o ./publish
```

2. **Create Windows Service**
```batch
@echo off
set APP_PATH=C:\HealthDataTools\healthdata-export-tools.exe
set DB_PATH=C:\HealthDataTools\healthdata.db
set INPUT_PATH=C:\HealthDataTools\exports\
set OUTPUT_PATH=C:\HealthDataTools\output\

%APP_PATH% %INPUT_PATH% %OUTPUT_PATH% %DB_PATH%
```

3. **Schedule with Task Scheduler**
- Open Task Scheduler
- Create Basic Task > Daily > 2:00 AM
- Action: Start program
- Program: `C:\HealthDataTools\healthdata-export-tools.exe`
- Arguments: `C:\HealthDataTools\exports\ C:\HealthDataTools\output\ C:\HealthDataTools\healthdata.db`

#### On Linux/macOS

1. **Build and Install**
```bash
dotnet publish -c Release -o ./publish
sudo cp -r ./publish/* /opt/healthdata-tools/
sudo chmod +x /opt/healthdata-tools/healthdata-export-tools
```

2. **Create Systemd Service**
```ini
[Unit]
Description=Health Data Export Tools
After=network.target

[Service]
Type=oneshot
User=healthdata
ExecStart=/opt/healthdata-tools/healthdata-export-tools /var/lib/healthdata/exports /var/lib/healthdata/output
WorkingDirectory=/var/lib/healthdata

[Install]
WantedBy=multi-user.target
```

3. **Create Cron Schedule**
```bash
# Daily at 2 AM
0 2 * * * /opt/healthdata-tools/healthdata-export-tools /var/lib/healthdata/exports /var/lib/healthdata/output
```

### Strategy 2: Docker Container Deployment

Deploy as Docker container for containerized environments.

#### Dockerfile

```dockerfile
FROM mcr.microsoft.com/dotnet/runtime:10.0

WORKDIR /app

COPY healthdata-export-tools/bin/Release/net10.0/publish/ .

# Create directories
RUN mkdir -p /data/exports /data/output

VOLUME ["/data/exports", "/data/output"]

ENTRYPOINT ["./healthdata-export-tools", "/data/exports", "/data/output"]
```

#### Docker Compose

```yaml
version: '3.8'

services:
  healthdata-exporter:
    build:
      context: .
      dockerfile: Dockerfile
    volumes:
      - ./exports:/data/exports:ro
      - ./output:/data/output
      - ./healthdata.db:/data/healthdata.db
    environment:
      - DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
    restart: on-failure
```

#### Run Docker Container

```bash
# Build image
docker build -t healthdata-export-tools:latest .

# Run container
docker run \
  -v $(pwd)/exports:/data/exports:ro \
  -v $(pwd)/output:/data/output \
  -v $(pwd)/healthdata.db:/data/healthdata.db \
  healthdata-export-tools:latest
```

### Strategy 3: Web API Deployment

Deploy as ASP.NET Core web service for API-based access.

#### API Controller Example

```csharp
using Microsoft.AspNetCore.Mvc;
using HealthDataExportTools.Services;

[ApiController]
[Route("api/[controller]")]
public class HealthDataController : ControllerBase
{
    private readonly HealthDataParserService _parser;
    private readonly ExportService _exporter;
    private readonly AnalyticsService _analytics;
    
    public HealthDataController(
        HealthDataParserService parser,
        ExportService exporter,
        AnalyticsService analytics)
    {
        _parser = parser;
        _exporter = exporter;
        _analytics = analytics;
    }
    
    [HttpPost("parse")]
    public async Task<IActionResult> ParseHealthData(IFormFile file)
    {
        try
        {
            var tempPath = Path.GetTempFileName();
            await using var stream = System.IO.File.Create(tempPath);
            await file.CopyToAsync(stream);
            
            var data = await _parser.ParseHealthDataAsync(tempPath);
            System.IO.File.Delete(tempPath);
            
            return Ok(data);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    [HttpPost("export")]
    public async Task<IActionResult> ExportData([FromBody] ExportRequest request)
    {
        try
        {
            var options = new HealthDataExportOptions
            {
                ExportFormat = request.Format,
                ValidateData = true
            };
            
            await _exporter.ExportAsync(request.Data, options);
            return Ok(new { message = "Export completed" });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    [HttpPost("analytics")]
    public IActionResult AnalyzeHealth([FromBody] HealthDataExportDto data)
    {
        try
        {
            var score = _analytics.CalculateHealthScore(data);
            var sleepReport = _analytics.AnalyzeSleepQuality(data.SleepRecords);
            
            return Ok(new
            {
                healthScore = score,
                sleepQuality = sleepReport
            });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
```

#### Startup Configuration

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHealthDataExportTools(builder.Configuration.GetSection("HealthDataExport"));
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors();
app.MapControllers();
app.Run();
```

## Cloud Deployment

### AWS Lambda

Deploy as serverless function:

```csharp
using Amazon.Lambda.Core;
using HealthDataExportTools.Services;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

public class Function
{
    private readonly HealthDataParserService _parser;
    private readonly ExportService _exporter;
    
    public async Task<string> FunctionHandler(S3Event s3Event, ILambdaContext context)
    {
        var s3Client = new AmazonS3Client();
        var parser = new HealthDataParserService();
        var exporter = new ExportService();
        
        foreach (var record in s3Event.Records)
        {
            var bucket = record.S3.Bucket.Name;
            var key = record.S3.Object.Key;
            
            var obj = await s3Client.GetObjectAsync(bucket, key);
            var tempFile = Path.GetTempFileName();
            
            await obj.ResponseStream.CopyToAsync(System.IO.File.Create(tempFile));
            
            var data = await parser.ParseHealthDataAsync(tempFile);
            var options = new HealthDataExportOptions { OutputPath = "/tmp/" };
            await exporter.ExportAsync(data, options);
            
            System.IO.File.Delete(tempFile);
        }
        
        return "Success";
    }
}
```

### Azure Function

Deploy as Azure Function:

```csharp
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using HealthDataExportTools.Services;

public static class HealthDataFunction
{
    [FunctionName("ProcessHealthData")]
    public static async Task Run(
        [BlobTrigger("exports/{name}", Connection = "AzureWebJobsStorage")] 
        Stream myBlob,
        string name,
        ILogger log)
    {
        try
        {
            var tempFile = Path.GetTempFileName();
            using var file = System.IO.File.Create(tempFile);
            await myBlob.CopyToAsync(file);
            
            var parser = new HealthDataParserService();
            var data = await parser.ParseHealthDataAsync(tempFile);
            
            var exporter = new ExportService();
            var options = new HealthDataExportOptions { OutputPath = "/tmp/" };
            await exporter.ExportAsync(data, options);
            
            System.IO.File.Delete(tempFile);
            log.LogInformation($"Successfully processed {name}");
        }
        catch (Exception ex)
        {
            log.LogError(ex, $"Error processing {name}");
            throw;
        }
    }
}
```

## Kubernetes Deployment

Deploy on Kubernetes cluster:

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: healthdata-exporter
  labels:
    app: healthdata-exporter
spec:
  replicas: 1
  selector:
    matchLabels:
      app: healthdata-exporter
  template:
    metadata:
      labels:
        app: healthdata-exporter
    spec:
      containers:
      - name: healthdata-exporter
        image: healthdata-export-tools:latest
        volumeMounts:
        - name: exports
          mountPath: /data/exports
        - name: output
          mountPath: /data/output
        - name: database
          mountPath: /data
        resources:
          requests:
            memory: "512Mi"
            cpu: "250m"
          limits:
            memory: "1Gi"
            cpu: "500m"
      volumes:
      - name: exports
        persistentVolumeClaim:
          claimName: exports-pvc
      - name: output
        persistentVolumeClaim:
          claimName: output-pvc
      - name: database
        persistentVolumeClaim:
          claimName: database-pvc
---
apiVersion: v1
kind: PersistentVolumeClaim
metadata:
  name: exports-pvc
spec:
  accessModes:
    - ReadWriteOnce
  resources:
    requests:
      storage: 10Gi
---
apiVersion: v1
kind: PersistentVolumeClaim
metadata:
  name: output-pvc
spec:
  accessModes:
    - ReadWriteOnce
  resources:
    requests:
      storage: 10Gi
```

## Database Setup

### SQLite (Default)

Database automatically initializes on first run:

```bash
./healthdata-export-tools ./exports ./output
# Creates ./healthdata.db automatically
```

### PostgreSQL (Optional)

For production PostgreSQL support:

1. **Create Database**
```sql
CREATE DATABASE healthdata;
CREATE USER healthdata_user WITH PASSWORD 'secure_password';
GRANT ALL PRIVILEGES ON DATABASE healthdata TO healthdata_user;
```

2. **Connection String**
```json
{
  "HealthDataExport": {
    "DatabasePath": "Host=localhost;Database=healthdata;Username=healthdata_user;Password=secure_password"
  }
}
```

## Performance Tuning

### Memory Configuration

```bash
# Set .NET heap size (MB)
export DOTNET_GCHeapHardLimit=1073741824  # 1 GB

# Enable server GC
export DOTNET_gcServer=1

# Enable concurrent GC
export DOTNET_gcConcurrent=1

./healthdata-export-tools
```

### Database Indexing

Create indexes for frequently queried fields:

```sql
CREATE INDEX idx_sleep_date ON SleepData(RecordDate);
CREATE INDEX idx_heartrate_date ON HeartRateData(RecordDate);
CREATE INDEX idx_spo2_date ON SpO2Data(RecordDate);
CREATE INDEX idx_steps_date ON StepsData(RecordDate);
```

### Batch Size Tuning

Adjust batch sizes in options:

```json
{
  "HealthDataExport": {
    "BatchSize": 5000,
    "MaxDegreeOfParallelism": 4
  }
}
```

## Monitoring & Logging

### Enable Verbose Logging

```csharp
var options = new HealthDataExportOptions
{
    VerboseLogging = true
};
```

### Structured Logging Setup

```csharp
var services = new ServiceCollection();
services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.AddFile("logs/healthdata-{Date}.log");
});
```

### Health Checks

```csharp
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = WriteResponse
});

services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database")
    .AddCheck<FilesystemHealthCheck>("filesystem");
```

## Security Considerations

### File Permissions

```bash
# Linux/macOS
sudo chmod 700 /var/lib/healthdata
sudo chmod 600 /var/lib/healthdata/healthdata.db
```

### Environment Variables

Use environment variables for sensitive data:

```bash
export DATABASE_PASSWORD=secure_password
export API_KEY=your_api_key
```

Access in code:

```csharp
var dbPassword = Environment.GetEnvironmentVariable("DATABASE_PASSWORD");
```

### HTTPS Configuration

```json
{
  "Kestrel": {
    "Endpoints": {
      "HttpsInlineCertAndKey": {
        "Url": "https://localhost:443",
        "Certificate": {
          "Path": "/etc/ssl/certs/certificate.pfx",
          "Password": "cert_password"
        }
      }
    }
  }
}
```

## Backup Strategy

### Automated Database Backups

```bash
#!/bin/bash
# backup.sh

BACKUP_DIR="/backups/healthdata"
DB_PATH="/var/lib/healthdata/healthdata.db"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)

mkdir -p $BACKUP_DIR
cp $DB_PATH $BACKUP_DIR/healthdata_$TIMESTAMP.db.backup
gzip $BACKUP_DIR/healthdata_$TIMESTAMP.db.backup

# Keep last 30 days
find $BACKUP_DIR -mtime +30 -delete
```

Schedule with cron:

```bash
0 3 * * * /scripts/backup.sh
```

## Rollback Procedure

```bash
# Restore from backup
cp /backups/healthdata/healthdata_20240101_030000.db.backup.gz /var/lib/healthdata/
gunzip /var/lib/healthdata/healthdata_20240101_030000.db.backup.gz
mv /var/lib/healthdata/healthdata_20240101_030000.db.backup /var/lib/healthdata/healthdata.db

# Restart application
systemctl restart healthdata-exporter
```

## Troubleshooting Deployment

### Issue: Permission Denied

**Solution**: Check file/directory permissions:
```bash
ls -la /var/lib/healthdata/
chmod 755 /var/lib/healthdata
```

### Issue: Out of Disk Space

**Solution**: Check disk usage and clean old files:
```bash
df -h
find /var/lib/healthdata -mtime +90 -delete
```

### Issue: Database Locked

**Solution**: Ensure single process access:
```bash
lsof | grep healthdata.db
```

## Next Steps

- See [Configuration Guide](configuration.md) for detailed settings
- Check [API Reference](api-reference.md) for API details
- Read [Getting Started](getting-started.md) for basic setup
