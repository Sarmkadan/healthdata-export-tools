# Architecture Guide

Detailed overview of Health Data Export Tools architecture, design patterns, and component interactions.

## System Overview

The library is organized into distinct layers:

```
┌─────────────────────────────────────────────────────┐
│         Presentation Layer (CLI/API)                │
│  ┌──────────────────────────────────────────────┐  │
│  │  CliArgumentParser  CommandHandler           │  │
│  └──────────────────────────────────────────────┘  │
├─────────────────────────────────────────────────────┤
│         Business Logic Layer (Services)             │
│  ┌───────────────────────────────────────────────┐ │
│  │ Parser │ Validator │ Analytics │ Exporter    │ │
│  │ Batch  │ Cache     │ EventBus  │ Scheduler   │ │
│  └───────────────────────────────────────────────┘ │
├─────────────────────────────────────────────────────┤
│         Data Access Layer (Repository)              │
│  ┌───────────────────────────────────────────────┐ │
│  │  IHealthDataRepository Implementation         │ │
│  │  (InMemory / SQLite)                          │ │
│  └───────────────────────────────────────────────┘ │
├─────────────────────────────────────────────────────┤
│         Data Layer (Domain Models)                  │
│  ┌───────────────────────────────────────────────┐ │
│  │ SleepData │ HeartRateData │ SpO2Data │ Steps  │ │
│  │ ActivityData │ HealthMetric │ Enums   │      │ │
│  └───────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────┘
```

## Core Components

### 1. Parsing Layer

**Responsible for**: Deserializing health export files from various device formats.

**Key Classes**:
- `HealthDataParserService`: Main parsing orchestrator
- Device-specific parsers (Zepp, Amazfit, Garmin)
- `CompressionUtility`: Handle ZIP and compressed archives

**Flow**:
```
Input File (ZIP)
    ↓
Identify Format (Zepp/Amazfit/Garmin)
    ↓
Extract from Archive
    ↓
Parse JSON/XML/Binary
    ↓
Create Domain Models
    ↓
Return HealthDataExportDto
```

**Example Implementation**:
```csharp
public async Task<HealthDataExportDto> ParseHealthDataAsync(string zipFilePath)
{
    var archive = ZipFile.OpenRead(zipFilePath);
    var manifest = await ReadManifestAsync(archive);
    var deviceType = IdentifyDevice(manifest);
    
    var sleepData = await ParseSleepDataAsync(archive, deviceType);
    var heartRateData = await ParseHeartRateDataAsync(archive, deviceType);
    var spO2Data = await ParseSpO2DataAsync(archive, deviceType);
    var stepsData = await ParseStepsDataAsync(archive, deviceType);
    
    return new HealthDataExportDto
    {
        SleepRecords = sleepData,
        HeartRateRecords = heartRateData,
        SpO2Records = spO2Data,
        StepsRecords = stepsData
    };
}
```

### 2. Validation Layer

**Responsible for**: Ensuring data integrity and consistency.

**Rules Enforced**:
- Heart rate ranges (30-220 bpm)
- SpO2 ranges (70-100%)
- Sleep duration (0-12 hours)
- Temporal consistency (end > start times)
- No future dates
- No negative values

**Validation Results**:
```csharp
public class ValidationResultDto
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; }
    public List<string> Warnings { get; set; }
    public int RecordsValidated { get; set; }
    public int RecordsInvalid { get; set; }
}
```

### 3. Analytics Layer

**Responsible for**: Calculate metrics, trends, and health insights.

**Analytics Performed**:

#### Sleep Quality Analysis
- Average sleep duration
- Deep/REM sleep percentages
- Sleep consistency score
- Quality classification (Poor/Fair/Good/Excellent)
- Trend direction (improving/declining/stable)

#### Heart Rate Analysis
- Min/max/average heart rates
- Resting heart rate trends
- Heart rate variability
- Fitness indicators
- Stress level patterns

#### SpO2 Analysis
- Average oxygen saturation
- Low SpO2 event detection
- Trend analysis
- Health status classification

#### Activity Analysis
- Daily step averages
- Activity trends
- Goal achievement rates
- Weekly patterns

#### Overall Health Score
Composite score (0-100) based on:
- Sleep quality (weight: 30%)
- Heart rate (weight: 25%)
- SpO2 levels (weight: 20%)
- Activity levels (weight: 25%)

**Example Report**:
```csharp
public class SleepQualityReport
{
    public double AverageDuration { get; set; }      // Minutes
    public double AverageDeepSleep { get; set; }     // Minutes
    public double AverageRemSleep { get; set; }      // Minutes
    public int ExcellentNights { get; set; }         // Count
    public int TotalNights { get; set; }             // Count
    public string TrendDirection { get; set; }       // "Improving"
    public string Description { get; set; }          // Human readable
}
```

### 4. Export Layer

**Responsible for**: Formatting data for output in various formats.

**Supported Formats**:
- **CSV**: Tabular format, one file per data type
- **JSON**: Nested structure, all data in one file
- **SQLite**: Relational database format

**Export Process**:
```csharp
public async Task ExportAsync(HealthDataExportDto data, HealthDataExportOptions options)
{
    var formatter = FormatterFactory.CreateFormatter(options.ExportFormat);
    var formatted = await formatter.FormatAsync(data);
    
    var path = Path.Combine(options.OutputPath, GenerateFileName(options.ExportFormat));
    await File.WriteAllTextAsync(path, formatted);
}
```

**Formatter Interface**:
```csharp
public interface IDataFormatter
{
    Task<string> FormatAsync<T>(T data) where T : class;
}
```

### 5. Data Access Layer

**Pattern**: Repository Pattern

**Implementations**:
- `InMemoryHealthDataRepository`: Fast in-memory storage
- `SqliteHealthDataRepository`: Persistent SQLite storage

**Repository Interface**:
```csharp
public interface IHealthDataRepository
{
    Task SaveHealthDataAsync(HealthDataExportDto data);
    Task<HealthDataExportDto?> GetHealthDataAsync();
    Task<IEnumerable<SleepData>> GetSleepRecordsAsync(DateTime? from, DateTime? to);
    Task<IEnumerable<HeartRateData>> GetHeartRateRecordsAsync(DateTime? from, DateTime? to);
    Task DeleteOldRecordsAsync(int daysToKeep);
}
```

### 6. Caching Layer

**Purpose**: Improve performance by caching frequently accessed data.

**Cache Providers**:
- `InMemoryCacheProvider`: In-memory cache
- Configurable TTL (time-to-live)

**Cache Strategy**:
```csharp
public class CacheService
{
    public async Task<T?> GetAsync<T>(string key)
    {
        if (_cacheProvider.TryGet(key, out var cached))
            return cached as T;
        return null;
    }
    
    public async Task SetAsync<T>(string key, T value, TimeSpan? duration = null)
    {
        _cacheProvider.Set(key, value, duration ?? _defaultDuration);
    }
}
```

### 7. Event System

**Pattern**: Observer/Pub-Sub

**Events Published**:
- `HealthDataImportedEvent`: When data is imported
- `ExportCompletedEvent`: When export finishes
- `ValidationFailedEvent`: When validation errors occur

**Event Handler**:
```csharp
public class EventBus : IEventPublisher
{
    public void Subscribe<T>(Func<T, Task> handler) where T : IEvent
    {
        // Register subscriber
    }
    
    public async Task PublishAsync<T>(T evt) where T : IEvent
    {
        // Invoke all subscribers
    }
}
```

### 8. Batch Processing

**Purpose**: Efficiently process multiple files in parallel.

**Implementation**:
```csharp
public class BatchProcessingService
{
    public async Task<IEnumerable<BatchResultDto>> ProcessDirectoryAsync(
        string directory, 
        ExportFormat format)
    {
        var files = Directory.GetFiles(directory, "*.zip");
        
        var results = await Task.WhenAll(
            files.Select(file => ProcessSingleAsync(file, format))
        );
        
        return results;
    }
}
```

### 9. Background Scheduling

**Purpose**: Schedule periodic health data synchronization and analysis.

**Implementation**:
```csharp
public class BackgroundTaskScheduler
{
    public void ScheduleTask(string name, TimeSpan interval, Func<Task> action)
    {
        var timer = new System.Timers.Timer(interval.TotalMilliseconds);
        timer.Elapsed += async (_, _) => await action();
        timer.Start();
    }
}
```

## Design Patterns Used

### 1. Dependency Injection

Services are loosely coupled through constructor injection:

```csharp
public class ExportService
{
    private readonly IHealthDataRepository _repository;
    private readonly IDataFormatter _formatter;
    
    public ExportService(IHealthDataRepository repository, IDataFormatter formatter)
    {
        _repository = repository;
        _formatter = formatter;
    }
}
```

### 2. Repository Pattern

Abstract data access behind interfaces:

```csharp
public interface IHealthDataRepository
{
    Task SaveAsync(HealthData data);
    Task<HealthData?> GetAsync(int id);
}
```

### 3. Factory Pattern

Create formatters dynamically:

```csharp
public class FormatterFactory
{
    public static IDataFormatter CreateFormatter(ExportFormat format)
    {
        return format switch
        {
            ExportFormat.Json => new JsonFormatter(),
            ExportFormat.Csv => new CsvFormatter(),
            ExportFormat.Xml => new XmlFormatter(),
            _ => throw new ArgumentException()
        };
    }
}
```

### 4. Strategy Pattern

Different export strategies:

```csharp
public interface IExportStrategy
{
    Task ExportAsync(HealthDataExportDto data, string path);
}

public class JsonExportStrategy : IExportStrategy { /* ... */ }
public class CsvExportStrategy : IExportStrategy { /* ... */ }
```

### 5. Observer Pattern

Event-driven notifications:

```csharp
public interface IEventPublisher
{
    void Subscribe<T>(Func<T, Task> handler) where T : IEvent;
    Task PublishAsync<T>(T evt) where T : IEvent;
}
```

### 6. Builder Pattern

Complex object construction:

```csharp
var options = new HealthDataExportOptionsBuilder()
    .WithInputPath("./exports/")
    .WithOutputPath("./output/")
    .WithFormat(ExportFormat.Json)
    .ValidateData()
    .Build();
```

## Data Flow Diagram

```
User Request
    ↓
┌───────────────────────┐
│ CommandHandler / CLI  │  Parse command-line arguments
└───────────────────────┘
    ↓
┌───────────────────────────────────────────┐
│ HealthDataParserService                   │  Read and deserialize
│ • Identify device format                  │  health export file
│ • Extract from ZIP archive                │
│ • Parse device-specific format            │
└───────────────────────────────────────────┘
    ↓
┌───────────────────────────────────────────┐
│ ValidationService                         │  Check data integrity
│ • Validate each record                    │  and consistency
│ • Check constraints                       │
└───────────────────────────────────────────┘
    ↓
┌───────────────────────────────────────────┐
│ AnalyticsService                          │  Calculate metrics
│ • Analyze sleep quality                   │  and insights
│ • Calculate health scores                 │
│ • Generate reports                        │
└───────────────────────────────────────────┘
    ↓
┌───────────────────────────────────────────┐
│ ExportService                             │  Format data for output
│ • Create formatter (CSV/JSON/XML)         │
│ • Serialize data                          │
│ • Write to file                           │
└───────────────────────────────────────────┘
    ↓
┌───────────────────────────────────────────┐
│ IHealthDataRepository                     │  Persist to storage
│ • Save to SQLite database                 │
│ • Store in memory cache                   │
└───────────────────────────────────────────┘
    ↓
┌───────────────────────────────────────────┐
│ EventBus                                  │  Notify subscribers
│ • Publish ExportCompletedEvent            │  of completion
│ • Trigger post-export actions             │
└───────────────────────────────────────────┘
    ↓
User Output
(Exported files, reports, analytics)
```

## Extension Points

### 1. Custom Formatters

Implement `IDataFormatter` for new export formats:

```csharp
public class YamlFormatter : IDataFormatter
{
    public async Task<string> FormatAsync<T>(T data) where T : class
    {
        // Custom YAML serialization
        return await Task.FromResult(serialized);
    }
}
```

### 2. Custom Validators

Extend `ValidationService`:

```csharp
public class CustomValidator
{
    public ValidationResultDto ValidateCustomRule(HealthDataRecord record)
    {
        // Implement custom validation logic
    }
}
```

### 3. Custom Analytics

Extend `AnalyticsService`:

```csharp
public class CustomAnalytics
{
    public CustomReport AnalyzeCustomMetric(IEnumerable<HealthDataRecord> records)
    {
        // Implement custom analysis
    }
}
```

### 4. Custom Storage

Implement `IHealthDataRepository`:

```csharp
public class PostgresRepository : IHealthDataRepository
{
    public async Task SaveHealthDataAsync(HealthDataExportDto data)
    {
        // Persist to PostgreSQL
    }
}
```

## Performance Considerations

### 1. Lazy Loading
Load data only when needed:

```csharp
public lazy<IEnumerable<SleepData>> SleepRecords => 
    new(() => repository.GetSleepRecordsAsync());
```

### 2. Batch Operations
Process multiple records efficiently:

```csharp
await repository.SaveBatchAsync(records, batchSize: 1000);
```

### 3. Asynchronous Processing
Use async/await throughout:

```csharp
var results = await Task.WhenAll(
    ProcessData1Async(),
    ProcessData2Async(),
    ProcessData3Async()
);
```

### 4. Memory Management
Stream large datasets:

```csharp
using var reader = File.OpenText(path);
foreach (var line in ReadLines(reader))
{
    ProcessLine(line);
}
```

## Testing Strategy

### Unit Tests
Test individual services in isolation:

```csharp
[Fact]
public async Task ParseHealthDataAsync_ValidZip_ReturnsCorrectData()
{
    var parser = new HealthDataParserService();
    var result = await parser.ParseHealthDataAsync("test.zip");
    Assert.NotNull(result);
    Assert.NotEmpty(result.SleepRecords);
}
```

### Integration Tests
Test component interactions:

```csharp
[Fact]
public async Task FullWorkflow_ParseValidateExport_Succeeds()
{
    var parser = new HealthDataParserService();
    var data = await parser.ParseHealthDataAsync("test.zip");
    
    var validator = new ValidationService();
    var validation = validator.ValidateAll(data);
    Assert.True(validation.IsValid);
    
    var exporter = new ExportService();
    await exporter.ExportAsync(data, options);
}
```

## Next Steps

For more details, see:
- [API Reference](api-reference.md) - Complete API documentation
- [Configuration Guide](configuration.md) - Configuration options
- [Deployment Guide](deployment.md) - Production deployment
