# Frequently Asked Questions

Common questions and answers about Health Data Export Tools.

## General Questions

### Q: What devices are supported?

**A**: The library supports health exports from:
- **Zepp/Amazfit**: Amazfit Band, Amazfit Watch, Zepp devices
- **Garmin**: Garmin Connect exported data
- **Generic**: Any device exporting standard health data formats

### Q: What's the license?

**A**: MIT License - completely free for commercial and personal use.

### Q: Can I contribute?

**A**: Yes! Fork the repository and submit pull requests. See [Contributing Guidelines](../README.md#contributing).

### Q: Is there a commercial version?

**A**: No, this is open-source. However, custom development and consulting services are available.

## Installation & Setup

### Q: I get "command not found" error after installation

**A**: Ensure dotnet is installed and in PATH:
```bash
dotnet --version
echo $PATH
```

If using NuGet package, ensure your project references it:
```bash
dotnet add package healthdata-export-tools
```

### Q: How do I install an older version?

**A**: Specify the version:
```bash
dotnet add package healthdata-export-tools --version 1.0.0
```

### Q: Can I use it with .NET Framework instead of .NET Core?

**A**: No, the library requires .NET 10.0+. The codebase uses modern C# features not available in .NET Framework.

### Q: Does it work on macOS/Linux?

**A**: Yes, fully compatible with:
- Windows 10+
- macOS 10.14+
- Linux (all distributions with .NET 10 support)

## Data & Parsing

### Q: What if my export file is corrupted?

**A**: The parser will throw a `HealthDataException` with details. Check:
1. File is not truncated
2. File is actually a ZIP archive
3. File is from a supported device
4. File is not encrypted

### Q: Can I parse multiple files at once?

**A**: Yes, use `BatchProcessingService`:
```csharp
var batchService = new BatchProcessingService();
var results = await batchService.ProcessDirectoryAsync("./exports/", ExportFormat.All);
```

### Q: How large can export files be?

**A**: No hard limit, but processing depends on available RAM. For files > 1 GB:
1. Use batch processing to split into smaller chunks
2. Increase available RAM
3. Close other applications

### Q: Can I filter data by date range?

**A**: Yes, after parsing:
```csharp
var filteredData = data.SleepRecords
    .Where(s => s.RecordDate >= startDate && s.RecordDate <= endDate)
    .ToList();
```

### Q: What happens to invalid records?

**A**: By default, they're skipped with a warning. To fail on invalid data:
```csharp
var options = new HealthDataExportOptions { ValidateData = true };
```

## Export & Storage

### Q: What format should I use for export?

**A**: 
- **JSON**: Best for hierarchical data and APIs
- **CSV**: Best for Excel/spreadsheet analysis
- **SQLite**: Best for querying and long-term storage

### Q: How do I access exported data in Excel?

**A**: 
1. Export to CSV format
2. Open with Excel or LibreOffice Calc
3. Or use the SQLite database with third-party tools

### Q: Can I export to a custom format?

**A**: Yes, implement `IDataFormatter`:
```csharp
public class CustomFormatter : IDataFormatter
{
    public async Task<string> FormatAsync<T>(T data) where T : class
    {
        // Custom formatting logic
        return formatted;
    }
}
```

### Q: How do I store data persistently?

**A**: Use SQLite repository:
```csharp
var connectionManager = new SqliteConnectionManager("./healthdata.db");
await connectionManager.InitializeDatabaseAsync();

var repository = new InMemoryHealthDataRepository();
await repository.SaveHealthDataAsync(data);
```

## Configuration

### Q: How do I change default directories?

**A**: Modify `appsettings.json`:
```json
{
  "HealthDataExport": {
    "InputPath": "/custom/exports/",
    "OutputPath": "/custom/output/",
    "DatabasePath": "/custom/healthdata.db"
  }
}
```

### Q: What does the health score calculation include?

**A**: 
- Sleep quality: 30%
- Heart rate: 25%
- SpO2 levels: 20%
- Activity: 25%

You can customize weights in `AnalyticsService`.

### Q: Can I disable data validation?

**A**: Yes, though not recommended:
```csharp
var options = new HealthDataExportOptions
{
    ValidateData = false
};
```

### Q: What's the default cache duration?

**A**: 3600 seconds (1 hour). Change with:
```csharp
options.CacheDurationSeconds = 7200; // 2 hours
```

## Performance

### Q: Processing is slow. How can I speed it up?

**A**: 
1. Enable caching: `options.CacheEnabled = true`
2. Use batch processing for multiple files
3. Increase available RAM
4. Run on faster storage (SSD vs HDD)
5. Close other applications

### Q: How much memory does it use?

**A**: Depends on data size:
- Small export (1 week): ~10-50 MB
- Large export (1 year): ~200-500 MB
- Multiple years: > 1 GB

Monitor with system tools.

### Q: Can I process in the background?

**A**: Yes, use `BackgroundTaskScheduler`:
```csharp
var scheduler = new BackgroundTaskScheduler();
scheduler.ScheduleTask("export", TimeSpan.FromHours(1), async () =>
{
    await exporter.ExportCompleteAsync(data, "./output/", ExportFormat.Json);
});
```

## Analytics

### Q: How accurate are the health calculations?

**A**: Based on the raw data from your device:
- Sleep metrics: Device-accurate
- Heart rate: Device-accurate
- SpO2: Device-accurate
- Composite score: Mathematical model, not medical diagnosis

### Q: Can I customize analytics?

**A**: Yes, extend `AnalyticsService` or create custom analyzers:
```csharp
public class CustomAnalytics
{
    public CustomReport Analyze(IEnumerable<SleepData> records)
    {
        // Custom logic
    }
}
```

### Q: What does "trend direction" mean?

**A**: 
- **Improving**: Metric getting better over time
- **Declining**: Metric getting worse
- **Stable**: No significant change

Based on week-over-week comparison.

## Integration

### Q: Can I integrate with other tools?

**A**: Yes, the library provides multiple integration points:
1. **CSV Export**: Import into any spreadsheet tool
2. **JSON Export**: Consume via API or scripts
3. **SQLite**: Query with any database tool
4. **Custom**: Implement interfaces for custom storage

### Q: Can I use it in a web application?

**A**: Yes, create an ASP.NET Core controller:
```csharp
[HttpPost("export")]
public async Task<IActionResult> ExportData([FromBody] ExportRequest request)
{
    var exporter = new ExportService();
    await exporter.ExportAsync(request.Data, options);
    return Ok();
}
```

### Q: How do I send data to an API?

**A**: Export to JSON and POST:
```csharp
var json = await exporter.ExportToJsonAsync(data, "data.json");
var content = new StringContent(json, Encoding.UTF8, "application/json");
await httpClient.PostAsync("https://api.example.com/health", content);
```

## Troubleshooting

### Q: I get "Unsupported file format" error

**A**: 
1. Verify file is from Zepp/Amazfit/Garmin
2. Check file is not corrupted
3. Ensure file extension is `.zip`
4. Try re-exporting from device

### Q: "Database locked" error

**A**: 
1. Ensure only one application accesses database
2. Check file permissions
3. Close other processes
4. Try restarting application

### Q: "Out of memory" error

**A**: 
1. Close other applications
2. Process files in batches
3. Reduce cache size
4. Use streaming parsing for large files

### Q: Export produces empty files

**A**: 
1. Verify health data contains records
2. Check export format matches expected output
3. Ensure no validation errors excluded all records
4. Check output directory permissions

### Q: Data validation fails

**A**: Common reasons:
1. Heart rate > 220 bpm (invalid)
2. SpO2 < 70% (invalid)
3. Negative duration (invalid)
4. Future dates

Review error messages in validation result.

## Advanced

### Q: Can I use dependency injection?

**A**: Yes, fully supported:
```csharp
var services = new ServiceCollection();
services.AddHealthDataExportTools();
var provider = services.BuildServiceProvider();
```

### Q: How do I implement custom storage?

**A**: Implement `IHealthDataRepository`:
```csharp
public class CustomRepository : IHealthDataRepository
{
    public async Task SaveHealthDataAsync(HealthDataExportDto data)
    {
        // Custom save logic
    }
}
```

### Q: Can I extend the library?

**A**: Yes, the architecture is extensible:
1. Implement existing interfaces
2. Extend service classes
3. Create custom middleware
4. Add event subscribers

### Q: Is there a TypeScript/JavaScript version?

**A**: Not officially, but JSON export can be consumed by any language/platform.

### Q: What's the roadmap?

**A**: Future planned features:
- Direct cloud storage integration (Azure, AWS)
- Web dashboard for analytics
- Mobile app support
- Additional device types
- Real-time synchronization

## Getting Help

### Q: Where can I report bugs?

**A**: Report issues on [GitHub Issues](https://github.com/Sarmkadan/healthdata-export-tools/issues)

### Q: How do I suggest features?

**A**: Create a [GitHub Discussion](https://github.com/Sarmkadan/healthdata-export-tools/discussions) or Issue

### Q: Is there documentation?

**A**: Yes:
- [README.md](../README.md) - Comprehensive overview
- [Getting Started](getting-started.md) - Setup guide
- [Architecture](architecture.md) - Design overview
- [API Reference](api-reference.md) - Complete API docs

### Q: How can I contact the author?

**A**: 
- GitHub: [@Sarmkadan](https://github.com/Sarmkadan)
- Website: [sarmkadan.com](https://sarmkadan.com)
- Telegram: [@sarmkadan](https://t.me/sarmkadan)

## License & Terms

### Q: Can I use this commercially?

**A**: Yes, MIT license allows commercial use.

### Q: Do I need to attribute the author?

**A**: License attribution is appreciated but not required. MIT license is permissive.

### Q: Can I modify and redistribute?

**A**: Yes, as long as you include the original MIT license.

### Q: Is there liability?

**A**: No. See LICENSE file for complete terms.

## Still Have Questions?

- Check the [README.md](../README.md) for detailed documentation
- Browse [examples/](../examples/) for code samples
- Search [GitHub Issues](https://github.com/Sarmkadan/healthdata-export-tools/issues)
- Read the [API Reference](api-reference.md)
- Open a new [GitHub Discussion](https://github.com/Sarmkadan/healthdata-export-tools/discussions)
