# ServiceCollectionExtensions

Provides extension methods for `IServiceCollection` to register all services required by the Health Data Export Tools pipeline, along with a nested configuration builder that constructs a configured `IServiceProvider`. The class centralizes dependency injection setup for both production and test scenarios, supporting in-memory and SQLite repository backends.

## API

### `AddHealthDataExportServices`

```csharp
public static IServiceCollection AddHealthDataExportServices(
    this IServiceCollection services,
    Action<HealthDataExportConfigurationBuilder> configure)
```

Registers the core export pipeline services into the service collection. The `configure` delegate receives a `HealthDataExportConfigurationBuilder` that must be used to specify all required settings before the provider is built.

- **Parameters:**
  - `services`: The `IServiceCollection` to augment.
  - `configure`: A delegate that populates the configuration builder.
- **Returns:** The same `IServiceCollection` for chaining.
- **Throws:** `ArgumentNullException` if `services` or `configure` is `null`.

### `AddInMemoryRepository`

```csharp
public static IServiceCollection AddInMemoryRepository(
    this IServiceCollection services)
```

Registers an in-memory implementation of the health data repository. Suitable for testing or transient workloads where persistence is not required.

- **Parameters:**
  - `services`: The `IServiceCollection` to augment.
- **Returns:** The same `IServiceCollection` for chaining.
- **Throws:** `ArgumentNullException` if `services` is `null`.

### `AddSqliteRepository`

```csharp
public static IServiceCollection AddSqliteRepository(
    this IServiceCollection services)
```

Registers a SQLite-backed implementation of the health data repository. The database path must be configured separately via the configuration builder.

- **Parameters:**
  - `services`: The `IServiceCollection` to augment.
- **Returns:** The same `IServiceCollection` for chaining.
- **Throws:** `ArgumentNullException` if `services` is `null`.

### `CreateTestServiceProvider`

```csharp
public static IServiceProvider CreateTestServiceProvider(
    Action<HealthDataExportConfigurationBuilder>? configure = null)
```

Creates a fully configured `IServiceProvider` pre-loaded with in-memory repository services and default test settings. If `configure` is provided, it overrides the defaults.

- **Parameters:**
  - `configure`: Optional delegate to customize test configuration.
- **Returns:** A ready-to-use `IServiceProvider`.
- **Throws:** Nothing directly; exceptions may propagate from the configuration delegate.

### `HealthDataExportConfigurationBuilder`

A nested class that accumulates configuration for the export pipeline and ultimately produces an `IServiceProvider`.

#### `WithInputPath`

```csharp
public HealthDataExportConfigurationBuilder WithInputPath(string path)
```

Sets the file system path from which health data files are read.

- **Parameters:**
  - `path`: A valid directory or file path.
- **Returns:** The builder instance for chaining.
- **Throws:** `ArgumentException` if `path` is null or empty.

#### `WithOutputPath`

```csharp
public HealthDataExportConfigurationBuilder WithOutputPath(string path)
```

Sets the directory where exported files are written.

- **Parameters:**
  - `path`: A valid directory path.
- **Returns:** The builder instance for chaining.
- **Throws:** `ArgumentException` if `path` is null or empty.

#### `WithDatabasePath`

```csharp
public HealthDataExportConfigurationBuilder WithDatabasePath(string path)
```

Sets the file path for the SQLite database when `AddSqliteRepository` is used. Ignored for in-memory repositories.

- **Parameters:**
  - `path`: A valid file path for the SQLite database file.
- **Returns:** The builder instance for chaining.
- **Throws:** `ArgumentException` if `path` is null or empty.

#### `WithExportFormat`

```csharp
public HealthDataExportConfigurationBuilder WithExportFormat(ExportFormat format)
```

Specifies the output format for exported data.

- **Parameters:**
  - `format`: A member of the `ExportFormat` enumeration.
- **Returns:** The builder instance for chaining.

#### `WithDataValidation`

```csharp
public HealthDataExportConfigurationBuilder WithDataValidation(bool enable)
```

Enables or disables data validation during the export pipeline.

- **Parameters:**
  - `enable`: `true` to perform validation; `false` to skip it.
- **Returns:** The builder instance for chaining.

#### `WithAnalysis`

```csharp
public HealthDataExportConfigurationBuilder WithAnalysis(bool enable)
```

Enables or disables analytical processing of the exported data.

- **Parameters:**
  - `enable`: `true` to run analysis; `false` to skip it.
- **Returns:** The builder instance for chaining.

#### `WithTrendAnalysisDays`

```csharp
public HealthDataExportConfigurationBuilder WithTrendAnalysisDays(int days)
```

Sets the number of days to include in trend analysis when analysis is enabled.

- **Parameters:**
  - `days`: A positive integer representing the lookback window.
- **Returns:** The builder instance for chaining.
- **Throws:** `ArgumentOutOfRangeException` if `days` is less than 1.

#### `WithDateRange`

```csharp
public HealthDataExportConfigurationBuilder WithDateRange(DateTime start, DateTime end)
```

Restricts exported data to records falling within the specified inclusive date range.

- **Parameters:**
  - `start`: The inclusive start date.
  - `end`: The inclusive end date.
- **Returns:** The builder instance for chaining.
- **Throws:** `ArgumentException` if `end` is earlier than `start`.

#### `WithVerboseLogging`

```csharp
public HealthDataExportConfigurationBuilder WithVerboseLogging(bool enable)
```

Enables or disables verbose diagnostic logging throughout the pipeline.

- **Parameters:**
  - `enable`: `true` for detailed logging; `false` for minimal output.
- **Returns:** The builder instance for chaining.

#### `Build`

```csharp
public IServiceProvider Build()
```

Finalizes the configuration and constructs the `IServiceProvider` with all registered services.

- **Returns:** A configured `IServiceProvider` ready to resolve pipeline components.
- **Throws:** `InvalidOperationException` if required configuration values (input path, output path) have not been set.

## Usage

### Example 1: Production pipeline with SQLite persistence

```csharp
var services = new ServiceCollection();

services
    .AddHealthDataExportServices(builder => builder
        .WithInputPath("/data/health/input")
        .WithOutputPath("/data/health/output")
        .WithDatabasePath("/data/health/export.db")
        .WithExportFormat(ExportFormat.Csv)
        .WithDataValidation(true)
        .WithAnalysis(true)
        .WithTrendAnalysisDays(30)
        .WithDateRange(new DateTime(2024, 1, 1), new DateTime(2024, 12, 31))
        .WithVerboseLogging(false))
    .AddSqliteRepository();

var provider = services.BuildServiceProvider();
var exporter = provider.GetRequiredService<IHealthDataExporter>();
await exporter.RunAsync();
```

### Example 2: Unit test setup with in-memory repository

```csharp
var provider = ServiceCollectionExtensions.CreateTestServiceProvider(builder =>
    builder
        .WithInputPath("TestData/Input")
        .WithOutputPath("TestData/Output")
        .WithExportFormat(ExportFormat.Json)
        .WithDataValidation(false)
        .WithAnalysis(false)
        .WithDateRange(DateTime.Today.AddDays(-7), DateTime.Today));

var exporter = provider.GetRequiredService<IHealthDataExporter>();
var result = await exporter.RunAsync();
Assert.True(result.FilesExported > 0);
```

## Notes

- `AddHealthDataExportServices` must be called before `AddSqliteRepository` or `AddInMemoryRepository`; otherwise the repository registration will have no effect on the pipeline.
- `CreateTestServiceProvider` internally calls `AddInMemoryRepository` and applies safe defaults. Any `WithDatabasePath` call in the configuration delegate is ignored.
- The builder methods return the same builder instance, enabling fluent chaining. All validation of values (e.g., path existence, date logic) is deferred until `Build` is called, except for argument null/empty checks which occur immediately.
- This class is not designed for concurrent use. Builder instances are intended to be configured on a single thread before `Build` is invoked. The resulting `IServiceProvider` follows standard Microsoft.Extensions.DependencyInjection thread-safety rules: resolution is safe across threads, but modification of the collection after the provider is built is not supported.
