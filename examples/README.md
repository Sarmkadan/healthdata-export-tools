# Health Data Export Tools - Examples

Complete working examples demonstrating different features and use cases of the Health Data Export Tools library.

## Examples Overview

### 1. Basic Export (01_BasicExport.cs)

**Purpose**: Simplest way to parse and export health data.

**Topics Covered**:
- Creating export options
- Parsing health data from ZIP file
- Exporting to JSON format
- Error handling

**Key Classes**:
- `HealthDataParserService`
- `ExportService`
- `HealthDataExportOptions`

### 2. Health Analytics (02_HealthAnalytics.cs)

**Purpose**: Analyze health data and generate reports.

**Topics Covered**:
- Sleep quality analysis
- Heart rate analysis
- SpO2 health analysis
- Activity/steps analysis
- Overall health score calculation

**Key Classes**:
- `AnalyticsService`
- Sleep/Heart/Activity report classes

### 3. Batch Processing (03_BatchProcessing.cs)

**Purpose**: Process multiple health files in parallel for efficiency.

**Topics Covered**:
- Directory scanning
- Parallel file processing
- Performance metrics
- Error handling in batch operations

**Key Classes**:
- `BatchProcessingService`
- Result aggregation

### 4. Database Storage (04_DatabaseStorage.cs)

**Purpose**: Persist health data to SQLite database for long-term storage.

**Topics Covered**:
- Database initialization
- Saving data to repository
- Querying data by date range
- Data retrieval and statistics

**Key Classes**:
- `SqliteConnectionManager`
- `IHealthDataRepository`
- `InMemoryHealthDataRepository`

### 5. Data Validation (05_DataValidation.cs)

**Purpose**: Validate health data quality and integrity before processing.

**Topics Covered**:
- Comprehensive validation rules
- Error and warning reporting
- Per-data-type validation
- Validation recommendations

**Key Classes**:
- `ValidationService`
- `ValidationResultDto`

### 6. Dependency Injection (06_DependencyInjection.cs)

**Purpose**: Production-grade DI setup using Microsoft.Extensions.

**Topics Covered**:
- Service registration
- ServiceCollection configuration
- Resolving services from DI container
- Dependency injection best practices

**Key Classes**:
- `ServiceCollection`
- `IServiceProvider`
- Service registration extensions

### 7. Multi-Format Export (07_MultiFormatExport.cs)

**Purpose**: Export to multiple formats with date range filtering.

**Topics Covered**:
- Date range filtering
- Selective data type export
- CSV export
- JSON export
- File size tracking

**Key Classes**:
- `ExportService`
- Format-specific export methods

## Running the Examples

### Prerequisites

- .NET 10 SDK installed
- Visual Studio, VS Code, or command-line terminal
- Sample health data export files (ZIP format)

### Setup

1. **Clone Repository**
```bash
git clone https://github.com/Sarmkadan/healthdata-export-tools.git
cd healthdata-export-tools
```

2. **Prepare Input Data**
```bash
# Create exports directory
mkdir exports

# Copy your health data ZIP files to exports/
cp ~/Downloads/health_export.zip ./exports/
```

3. **Build Project**
```bash
dotnet build -c Release
```

### Running Individual Examples

**Run Example 1: Basic Export**
```bash
dotnet run --project healthdata-export-tools -- BasicExport
# Or directly:
dotnet run --no-build --project healthdata-export-tools/healthdata-export-tools.csproj
```

**Run Example 2: Health Analytics**
```bash
dotnet run --project healthdata-export-tools -- HealthAnalytics
```

**Run All Examples**
```bash
for i in {1..7}; do
  echo "Running Example $i..."
  # Run each example
done
```

## Example Output Directory Structure

```
output/
├── 2024-05-04/
│   ├── sleep_last_30_days.csv
│   ├── heart_rate_last_30_days.csv
│   ├── steps_last_30_days.csv
│   ├── spo2_last_30_days.csv
│   └── health_data_last_30_days.json
├── health_data.json
└── health_data.csv
```

## Common Tasks

### Viewing Example Code

Each example is a complete, self-contained application. Review the source code:

```bash
# View Example 1
cat examples/01_BasicExport.cs

# View Example 2
cat examples/02_HealthAnalytics.cs
```

### Modifying Examples

Each example demonstrates specific functionality. Modify them to:

1. **Change Input Path**
```csharp
InputPath = "path/to/your/exports/",
```

2. **Change Output Format**
```csharp
ExportFormat = ExportFormat.Csv,  // Change to CSV
```

3. **Add Custom Logic**
```csharp
// Add your custom processing after export
var metrics = /* calculate */;
```

### Creating New Examples

To create a new example based on existing ones:

1. Copy an existing example file
2. Rename (e.g., `08_CustomExample.cs`)
3. Modify the class name and Main method
4. Implement your custom logic

## Learning Path

**Recommended order for learning**:

1. **Start**: Example 1 (Basic Export) - Understand core functionality
2. **Next**: Example 6 (Dependency Injection) - Learn proper architecture
3. **Then**: Example 2 (Analytics) - Understand data analysis features
4. **Explore**: Example 5 (Validation) - Learn data quality checks
5. **Practice**: Example 3 (Batch Processing) - Handle multiple files
6. **Integrate**: Example 4 (Database) - Persistent storage
7. **Polish**: Example 7 (Multi-Format) - Professional output

## Troubleshooting Examples

### "File not found" Error

**Solution**: Ensure health data ZIP files exist:
```bash
ls -la exports/
# Should show: health_export.zip (or similar)
```

### "Permission denied" Error

**Solution**: Check directory permissions:
```bash
chmod 755 exports/
chmod 644 exports/*.zip
```

### "No export files found"

**Solution**: Add sample health data to exports/

### Example Compilation Errors

**Solution**: Ensure dependencies are restored:
```bash
dotnet restore
dotnet build
```

## Integration with Your Project

To use these examples in your own project:

1. **Copy Core Logic**
```csharp
// From Example 1
var parser = new HealthDataParserService();
var data = await parser.ParseHealthDataAsync("export.zip");
```

2. **Adapt for Your Needs**
- Modify file paths for your environment
- Adjust export formats as needed
- Add custom validation or analytics

3. **Handle Errors**
- Wrap in try-catch blocks
- Log errors appropriately
- Provide user feedback

## Performance Tips from Examples

From **Example 3 (Batch Processing)**:
- Process multiple files in parallel for better throughput
- Monitor processing time and optimize accordingly

From **Example 4 (Database Storage)**:
- Use databases for efficient querying
- Create appropriate indexes for common queries

From **Example 5 (Validation)**:
- Validate early to catch data issues
- Review validation errors carefully

From **Example 6 (DI)**:
- Use dependency injection for testability
- Keep services loosely coupled

## Next Steps

After reviewing the examples:

1. Read [Getting Started Guide](../docs/getting-started.md)
2. Study [Architecture Documentation](../docs/architecture.md)
3. Reference [API Documentation](../docs/api-reference.md)
4. Explore [Configuration Options](../docs/deployment.md)
5. Check [FAQ](../docs/faq.md) for common questions

## Contributing Examples

Want to add more examples? Follow these guidelines:

1. Create a new file: `NN_ExampleName.cs`
2. Use the author header and namespace convention
3. Include XML documentation comments
4. Keep example focused on single feature
5. Provide clear output and error handling
6. Submit pull request with description

## Support

For questions about examples:

- Review the code comments
- Check [FAQ](../docs/faq.md)
- Open [GitHub Issue](https://github.com/Sarmkadan/healthdata-export-tools/issues)
- Check [Discussions](https://github.com/Sarmkadan/healthdata-export-tools/discussions)
