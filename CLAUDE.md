# CLAUDE.md

.NET 10 library + CLI (`HealthDataExportTools`) for parsing, validating, analyzing and exporting personal health data (sleep, heart rate, SpO2, steps, activity) from Zepp/Amazfit/Garmin exports to CSV/JSON/JSON Lines/XML and SQLite.

## Build

```bash
dotnet restore healthdata-export-tools.sln
dotnet build healthdata-export-tools.sln -c Debug      # or: make build
dotnet build healthdata-export-tools.sln -c Release    # or: make release
dotnet publish healthdata-export-tools/healthdata-export-tools.csproj -c Release -o publish   # make publish
dotnet pack healthdata-export-tools/healthdata-export-tools.csproj -c Release -o packages     # make pack
dotnet run --project healthdata-export-tools -- <inputPath> <outputPath> <dbPath>
docker build -t healthdata-export-tools .              # docker-compose.yml also present
```

SDK pinned in `global.json` (10.0.100, rollForward latestMinor). Shared props in `Directory.Build.props` (Nullable + ImplicitUsings on, LangVersion latest, warnings not errors).

## Test

```bash
dotnet test healthdata-export-tools.sln                             # or: make test
dotnet test healthdata-export-tools.sln --filter "FullyQualifiedName~CacheServiceTests"
dotnet test healthdata-export-tools.sln -c Release --collect:"XPlat Code Coverage"   # make coverage
dotnet run -c Release --project benchmarks/healthdata-export-tools.Benchmarks        # BenchmarkDotNet
```

Stack: xUnit 2.9, FluentAssertions 7, NSubstitute 5. CI (`.github/workflows/`) runs restore/build/test in Release on push/PR to `main`; CodeQL, Docker (ghcr.io), NuGet publish and GitHub Release workflows exist too.

## Lint / format

```bash
dotnet format healthdata-export-tools.sln --verify-no-changes   # make format-check
dotnet format healthdata-export-tools.sln                        # make format
dotnet build healthdata-export-tools.sln /p:EnforceCodeStyleInBuild=true   # make lint
```

Style rules live in `.editorconfig` (4-space indent, PascalCase types/members, `I` prefix for interfaces, `var` when type is apparent, braces preferred).

## Layout

- `healthdata-export-tools.sln` - main project + test project only (benchmarks are not in the solution).
- `healthdata-export-tools/` - library + CLI. `Program.cs` is the entry point (demo run over sample data; `verify` subcommand).
  - `Cli/` - `CliArgumentParser`, `CliOptions`, `CommandHandler`
  - `Configuration/` - `HealthDataExportOptions`, `ServiceCollectionExtensions.AddHealthDataExportServices()` (DI registration)
  - `Domain/Models`, `Domain/Enums` - `HealthDataRecord` base + `SleepData`, `HeartRateData`, `SpO2Data`, `StepsData`, `ActivityData`; `DeviceType`, `ExportFormat`
  - `DTOs/` - `HealthDataExportDto`, `ExportResultDto`, `ImportResultDto`, `ValidationResultDto`
  - `Services/` - `HealthDataParserService`, `ValidationService`, `AnalyticsService`, `ExportService`, `BatchProcessingService`, `CacheService`, `TrendAnomalyDetectionService`, `ReportGenerationService`, `DataComparisonService`, `CsvExporter`, `JsonLinesExporter`
  - `Formatters/` - `IDataFormatter` + `Csv/Json/JsonLines/XmlFormatter`, `FormatterFactory`; `*.Streaming.cs` partials for large inputs
  - `Data/` - `IHealthDataRepository`, `InMemoryHealthDataRepository`, `SqliteConnectionManager`
  - `Events/`, `Middleware/`, `Interceptors/` (`RateLimiter`, `MetricsCollector`), `Integration/` (`WebhookService`, `RetryHandler`), `Tasks/` (`BackgroundTaskScheduler`), `Cache/`, `Correlation/`, `Exceptions/` (`HealthDataException`), `Utilities/` (`CompressionUtility`, `PathTraversalValidator`, `CsvUtility`, `JsonUtility`)
  - `GlobalUsings.cs` - System.*, `Microsoft.Extensions.Logging`, and Domain/Services/Exceptions namespaces are global; do not re-import them.
- `tests/healthdata-export-tools.Tests/` - xUnit tests; `BaseTest` + `TestFixture` (`IClassFixture`) give a DI `ServiceProvider`; `TestConstants.cs` holds shared data.
- `benchmarks/healthdata-export-tools.Benchmarks/` - BenchmarkDotNet suite.
- `examples/` - numbered usage samples (`01_BasicExport.cs` ...), not compiled into the solution.
- `docs/` - per-class markdown docs, `architecture.md`, `configuration.md`, `faq.md`.
- Root-level `Cache/`, `Services/`, `DTOs/`, `Domain/`, etc. and `CSVExporter.cs`, `test_*.cs*` are stray tracked copies outside any csproj; they are not compiled. Edit the copies under `healthdata-export-tools/`. Untracked `build/` and root `healthdata-export-tools.Tests/` are local leftovers.

## Conventions

- File header: `#nullable enable` + author banner comment (Vladyslav Zaiets) on every `.cs`; file-scoped namespaces (`namespace HealthDataExportTools.X;`) in library code, block namespaces in tests.
- Root namespace `HealthDataExportTools`; folder = namespace segment. Tests use `HealthDataExportTools.Tests`.
- One type per file, named after the type. Related helpers split into partial/extension files with suffixes: `XExtensions.cs`, `XJsonExtensions.cs`, `XValidation.cs`, `X.Streaming.cs`.
- Public members have XML doc comments (`GenerateDocumentationFile` is on). Guard args with `ArgumentNullException.ThrowIfNull`.
- Services take `ILogger<T>` via constructor and are registered in `ServiceCollectionExtensions`; async methods end in `Async` and accept `CancellationToken`.
- Domain-specific failures throw `HealthDataException`; user-supplied paths go through `PathTraversalValidator`; CSV output goes through `CsvUtility` (injection escaping).
- Test naming: `Method_Scenario_ExpectedResult` with `[Fact]`/`[Theory]`, FluentAssertions `.Should()`, NSubstitute for mocks; test files mirror source class names (`XTests.cs`, `XTestsExtensions.cs`).
- Commits: conventional prefixes (`feat(Scope):`, `fix:`, `docs:`, `chore:`).
