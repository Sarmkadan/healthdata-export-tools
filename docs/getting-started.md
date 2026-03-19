# Getting Started with Health Data Export Tools

A step-by-step guide to install, configure, and use Health Data Export Tools for the first time.

## Prerequisites

Before you begin, ensure you have:

- **Operating System**: Windows 10+, macOS 10.14+, or Linux
- **.NET 10 Runtime**: [Download from microsoft.com](https://dotnet.microsoft.com/download)
- **Git**: For cloning the repository (optional)
- **Text Editor**: Visual Studio Code, Visual Studio, or any text editor

Verify your .NET installation:

```bash
dotnet --version
```

Output should show version 10.0.0 or later.

## Installation Methods

### Option 1: NuGet Package (Recommended for Library Usage)

Install via command line in your project:

```bash
cd your-project
dotnet add package healthdata-export-tools
```

Or via Visual Studio Package Manager:

1. Open Package Manager Console
2. Run: `Install-Package healthdata-export-tools`

### Option 2: Clone from GitHub (Development/Contribution)

```bash
git clone https://github.com/Sarmkadan/healthdata-export-tools.git
cd healthdata-export-tools
dotnet build -c Release
```

This creates a Release build in `/bin/Release/net10.0/`.

### Option 3: Docker Container

Use pre-built Docker image:

```bash
docker pull healthdata-export-tools:latest
docker run -v $(pwd)/exports:/app/exports healthdata-export-tools
```

Or build locally:

```bash
docker build -t healthdata-export-tools .
docker run -v $(pwd)/exports:/app/exports healthdata-export-tools
```

## Creating Your First Project

### Step 1: Create a New Console Application

```bash
dotnet new console -n MyHealthApp
cd MyHealthApp
dotnet add package healthdata-export-tools
```

### Step 2: Basic Setup

Edit `Program.cs`:

```csharp
using HealthDataExportTools;
using HealthDataExportTools.Configuration;
using HealthDataExportTools.Domain.Enums;

// Create output directory
Directory.CreateDirectory("./output");

// Configure options
var options = new HealthDataExportOptions
{
    InputPath = "./exports/",
    OutputPath = "./output/",
    ExportFormat = ExportFormat.Json,
    ValidateData = true,
    PerformAnalysis = true
};

try
{
    var parser = new HealthDataParserService();
    Console.WriteLine("📂 Parsing health data...");
    var data = await parser.ParseHealthDataAsync("health_export.zip");
    
    var exporter = new ExportService();
    Console.WriteLine("💾 Exporting data...");
    await exporter.ExportAsync(data, options);
    
    Console.WriteLine("✓ Export completed successfully!");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error: {ex.Message}");
}
```

### Step 3: Prepare Health Data

Export your health data from your wearable:

**Amazfit/Zepp**:
1. Open Zepp app
2. Navigate to Settings > Data Management
3. Export your health data as ZIP

**Garmin**:
1. Go to Connect.Garmin.com
2. Settings > Data Export
3. Download health data ZIP

**Place exported ZIP file** in the `./exports/` directory.

### Step 4: Run Your Application

```bash
dotnet run
```

You should see:
```
📂 Parsing health data...
💾 Exporting data...
✓ Export completed successfully!
```

Check the `./output/` directory for exported files.

## Project Structure

Organize your project like this:

```
MyHealthApp/
├── Program.cs                    # Main entry point
├── appsettings.json             # Configuration
├── MyHealthApp.csproj           # Project file
├── Services/                    # Custom services
│   └── ReportService.cs
├── Models/                      # Custom models
│   └── HealthReport.cs
├── exports/                     # Input health data
│   └── health_export.zip
└── output/                      # Exported results
    ├── health_data.json
    └── health_data.csv
```

## Configuration

### appsettings.json

Create an `appsettings.json` file:

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
  }
}
```

Load in your application:

```csharp
var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var options = config.GetSection("HealthDataExport")
    .Get<HealthDataExportOptions>()!;
```

## Using Dependency Injection

For more sophisticated applications, use dependency injection:

```csharp
using Microsoft.Extensions.DependencyInjection;
using HealthDataExportTools.Configuration;

var services = new ServiceCollection();

services.AddHealthDataExportTools(options =>
{
    options.InputPath = "./exports/";
    options.OutputPath = "./output/";
    options.ValidateData = true;
});

var provider = services.BuildServiceProvider();

var parser = provider.GetRequiredService<HealthDataParserService>();
var exporter = provider.GetRequiredService<ExportService>();
var analytics = provider.GetRequiredService<AnalyticsService>();

var data = await parser.ParseHealthDataAsync("export.zip");
var healthScore = analytics.CalculateHealthScore(data);

Console.WriteLine($"Health Score: {healthScore}/100");

await exporter.ExportAsync(data, options);
```

## Common Tasks

### Export to Specific Format

```csharp
var exporter = new ExportService();

// JSON only
await exporter.ExportCompleteAsync(data, "./output/", ExportFormat.Json);

// CSV only
await exporter.ExportSleepToCsvAsync(data.SleepRecords, "sleep.csv");
await exporter.ExportHeartRateToCsvAsync(data.HeartRateRecords, "heart_rate.csv");
await exporter.ExportStepsToCsvAsync(data.StepsRecords, "steps.csv");
```

### Analyze Your Data

```csharp
var analytics = new AnalyticsService();

// Sleep analysis
var sleepReport = analytics.AnalyzeSleepQuality(data.SleepRecords);
Console.WriteLine($"Sleep Quality: {sleepReport.Description}");
Console.WriteLine($"Average Sleep: {sleepReport.AverageDuration} minutes");

// Heart rate analysis
var hrReport = analytics.AnalyzeHeartRate(data.HeartRateRecords);
Console.WriteLine($"Average HR: {hrReport.AverageHeartRate} bpm");

// Overall health
var score = analytics.CalculateHealthScore(data);
Console.WriteLine($"Overall Health: {score}/100");
```

### Store in Database

```csharp
var connectionManager = new SqliteConnectionManager("./healthdata.db");
await connectionManager.InitializeDatabaseAsync();

var repository = new InMemoryHealthDataRepository();
await repository.SaveHealthDataAsync(data);

var retrieved = await repository.GetHealthDataAsync();
```

### Validate Data

```csharp
var validator = new ValidationService();

var results = new List<ValidationResultDto>();
foreach (var record in data.SleepRecords)
{
    var result = validator.ValidateSleepData(record);
    results.Add(result);
}

var invalid = results.Where(r => !r.IsValid).ToList();
Console.WriteLine($"Valid: {results.Count - invalid.Count}, Invalid: {invalid.Count}");
```

## Next Steps

- Read the [Architecture Guide](architecture.md) for deeper understanding
- Check [API Reference](api-reference.md) for complete API documentation
- Explore [examples/](../examples/) for ready-to-run code samples
- Review [Configuration Guide](configuration.md) for advanced options
- Visit [Deployment Guide](deployment.md) for production deployment

## Troubleshooting

### Issue: "The type or namespace name ... could not be found"

**Solution**: Ensure the package is installed:
```bash
dotnet add package healthdata-export-tools
```

### Issue: "File not found" when parsing

**Solution**: Verify the export ZIP file path:
```csharp
var filePath = "./exports/health_export.zip";
if (!File.Exists(filePath))
    Console.WriteLine($"File not found: {Path.GetFullPath(filePath)}");
else
    var data = await parser.ParseHealthDataAsync(filePath);
```

### Issue: Async errors

**Solution**: Ensure Main is async:
```csharp
static async Task Main(string[] args)  // ← "Task" not "void"
{
    await DoAsync();
}
```

## Getting Help

- **Documentation**: See [README.md](../README.md) for comprehensive documentation
- **Issues**: Report bugs at [GitHub Issues](https://github.com/Sarmkadan/healthdata-export-tools/issues)
- **Discussions**: Ask questions in [GitHub Discussions](https://github.com/Sarmkadan/healthdata-export-tools/discussions)

## Next: Learn the Architecture

Continue with the [Architecture Guide](architecture.md) to understand how the library works internally.
