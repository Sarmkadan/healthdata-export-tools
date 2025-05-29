# Changelog

All notable changes to Health Data Export Tools are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2025-08-04

### Added
- Complete documentation suite: getting-started, architecture, API reference, deployment, FAQ
- 7 comprehensive example applications covering all major use cases
- Docker support with Dockerfile and docker-compose configuration
- GitHub Actions CI/CD workflow for automated testing and NuGet publishing
- CodeQL security scanning workflow
- Makefile for build automation

### Changed
- Finalized public API surface — no further breaking changes planned for the 1.x line
- Updated README with full usage guide, benchmark table, and troubleshooting section
- Improved error messages across all services

### Fixed
- Memory leak in `BatchProcessingService` when processing directories with many small files
- `SqliteConnectionManager` did not dispose connections on timeout

## [0.9.0] - 2025-07-07

### Added
- `WebhookService` for push notifications on export completion
- `RetryHandler` with configurable back-off for transient failures
- `RateLimiter` interceptor to protect against accidental API flooding
- `MetricsCollector` interceptor for per-operation timing and counters
- `NotificationService` wiring for email/webhook delivery

### Changed
- `EventBus` now supports multiple subscribers per event type
- Improved `BackgroundTaskScheduler` shutdown to drain queued work before exit

### Fixed
- Race condition in `InMemoryCacheProvider` under high-concurrency reads
- `ExportCompletedEvent` carried stale duration when export was retried

## [0.8.0] - 2025-06-16

### Added
- BenchmarkDotNet benchmark suite: `JsonParsingBenchmarks`, `CsvFormattingBenchmarks`, `AnalyticsBenchmarks`
- `PerformanceUtility` helpers for measuring hot-path allocations
- `ArrayPool<char>` backing buffer in `CsvUtility.ParseCsvLine` — eliminates per-line `StringBuilder` allocation
- `FrozenDictionary` lookup for device-type resolution in the parser

### Changed
- `AnalyticsService` methods (`CalculateHealthScore`, `AnalyzeSleepQuality`, `AnalyzeSpO2Health`, `AnalyzeActivityIntensity`) each replaced multiple LINQ passes with a single `foreach` loop
- `CsvFormatter` date formatting switched from `ToString("yyyy-MM-dd")` to `TryFormat` on `Span<char>`

### Fixed
- `CompressionUtility` did not flush the final block on non-seekable streams

## [0.7.0] - 2025-05-26

### Added
- `BackgroundTaskScheduler` for recurring and scheduled data-processing jobs
- `EventBus` publish/subscribe system with `ExportCompletedEvent` and `HealthDataImportedEvent`
- `IEventPublisher` interface for testable event dispatch
- Dependency injection extension method `AddHealthDataExportTools` for one-call service registration
- `ServiceCollectionExtensions` with sensible defaults for all services

### Changed
- All services now resolve their dependencies from the DI container rather than constructing them internally
- `HealthDataExportOptions` properties are now validated on startup when DI is used

## [0.6.0] - 2025-05-05

### Added
- `ICacheProvider` / `InMemoryCacheProvider` interfaces
- `CacheService` wrapping the provider with TTL support
- `BatchProcessingService` for processing entire directories of export files in parallel
- `ReportGenerationService` for multi-metric summary reports

### Changed
- `ExportService.ExportCompleteAsync` now delegates format selection to `FormatterFactory`
- `IHealthDataRepository` extended with `DeleteAsync` and `ExistsAsync`

### Fixed
- `InMemoryHealthDataRepository` threw `InvalidOperationException` on concurrent writes

## [0.5.0] - 2025-04-21

### Added
- `SqliteConnectionManager` for database initialisation and schema migration
- `IHealthDataRepository` / `InMemoryHealthDataRepository` for storage abstraction
- Full SQLite persistence path: parse → validate → store → export

### Changed
- `ExportService` accepts a repository instance for persistence before formatting
- `HealthDataExportDto` renamed from `HealthDataDto` for clarity

### Fixed
- `CsvFormatter` double-quoted fields that contained only whitespace

## [0.4.0] - 2025-04-07

### Added
- `AnalyticsService` with health-score calculation and per-metric analysis
- `TrendAnomalyDetectionService` for detecting out-of-range and trending anomalies
- `AnomalyDetectionResultDto` and related report DTOs
- `ValidationService` with rules for all four metric types (sleep, heart rate, SpO2, steps)
- `ValidationResultDto` and `ImportResultDto`

### Changed
- `HealthDataParserService` now returns a structured `HealthDataExportDto` instead of raw lists
- `SleepData.Quality` is set automatically by the parser based on score ranges

## [0.3.0] - 2025-03-17

### Added
- JSON export via `JsonFormatter` using `System.Text.Json`
- XML export via `XmlFormatter`
- `FormatterFactory` for runtime format selection
- `IDataFormatter` interface for pluggable formatters
- `ErrorHandlingMiddleware` and `LoggingMiddleware` pipeline
- `HealthDataException` hierarchy for structured error propagation

### Changed
- `ExportFormat` enum extended with `Xml` and `All` values
- `CliOptions` updated to accept `--format` flag

### Fixed
- Parser did not handle missing optional fields in Garmin JSON exports

## [0.2.0] - 2025-02-28

### Added
- CSV export via `CsvFormatter` and `CsvUtility` helpers
- `DateTimeExtensions` for normalising device-local timestamps to UTC
- `DataTransformationUtility` for unit conversions (km↔mi, kg↔lbs)
- `FileUtility` for safe path handling and temp-file cleanup
- `CompressionUtility` for extracting ZIP health export archives

### Changed
- Domain models (`SleepData`, `HeartRateData`, `SpO2Data`, `StepsData`) inherit from `HealthDataRecord`
- `DeviceType` and `SleepQuality` enums extracted to `Domain/Enums/`

### Fixed
- Step-count parser miscounted when the daily file contained multiple activity blocks

## [0.1.0] - 2025-02-10

### Added
- Initial release
- `HealthDataParserService` — parse Zepp/Amazfit/Garmin JSON health exports
- Domain models: `SleepData`, `HeartRateData`, `SpO2Data`, `StepsData`, `ActivityData`
- `CliArgumentParser` and `CliOptions` for command-line usage
- `JsonUtility` wrapper around `System.Text.Json`
- `ValidationHelper` with common guard methods
- `Constants` for shared thresholds and defaults
- MIT licence, basic README

---

## Contributors

- **Vladyslav Zaiets** — Author & Maintainer
