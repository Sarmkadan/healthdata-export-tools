# ReportGenerationServiceTests

Unit test suite for the `ReportGenerationService` class, validating report generation functionality across summary, daily, trend, and weekly report types. Tests cover normal operation, edge cases, and data completeness scenarios to ensure correct report structure and metrics population.

## API

### `ReportGenerationServiceTests`
Public test class containing test cases for report generation scenarios. Serves as the entry point for all report validation tests.

### `Task GenerateSummaryReportAsync_WithValidRecords_ReturnsCorrectReport()`
Verifies that a summary report is generated correctly when valid input records are provided. Ensures all expected metrics are populated and values are calculated accurately.

### `Task GenerateSummaryReportAsync_WithNoRecords_ReturnsEmptyReport()`
Validates behavior when no records are provided. Confirms that an empty report is returned without errors and maintains expected structure.

### `Task GenerateDailyReportAsync_WithValidData_ReturnsCorrectReport()`
Tests daily report generation with complete input data. Ensures all daily metrics (e.g., sleep duration, heart rate trends) are present and correctly computed.

### `Task GenerateDailyReportAsync_WithNoSleepData_ReturnsReportWithoutSleepMetrics()`
Checks daily report generation when sleep data is missing. Ensures the report is generated without sleep-related metrics and does not fail.

### `Task GenerateDailyReportAsync_WithNoHeartRateData_ReturnsReportWithoutHeartRateMetrics()`
Validates daily report generation when heart rate data is absent. Confirms the report omits heart rate metrics while remaining structurally valid.

### `Task GenerateTrendReportAsync_WithSufficientData_CalculatesTrend()`
Tests trend report generation with adequate historical data. Ensures trend calculations (e.g., moving averages, deltas) are performed and included in the output.

### `Task GenerateTrendReportAsync_WithInsufficientData_ReturnsEmptyMetricTrends()`
Validates trend report behavior when insufficient data is available. Confirms that metric trends are omitted rather than populated with unreliable values.

### `Task GenerateWeeklySummaryReportAsync_WithValidData_ReturnsWeeklyReports()`
Tests weekly summary report generation with valid multi-day data. Ensures all weekly aggregates and summaries are computed and included.

### `Task GenerateWeeklySummaryReportAsync_WithEmptyData_ReturnsEmptyList()`
Verifies behavior when no weekly data is available. Confirms an empty list is returned without errors.

### `Task GenerateWeeklySummaryReportAsync_WithSpO2AndActivity_PopulatesNewFields()`
Ensures new SpO2 and activity-related fields are correctly populated in weekly reports when corresponding data exists.

### `Task GenerateWeeklySummaryReportAsync_MultipleWeeks_AttachesWeekOverWeekChanges()`
Tests weekly report generation across multiple weeks. Validates that week-over-week change metrics (e.g., delta values) are attached correctly.

### `Task GenerateWeeklySummaryReportAsync_CalculatesHealthScore()`
Confirms that a composite health score is calculated and included in weekly reports based on underlying metrics.

### `Task ExportWeeklySummaryToJsonAsync_WritesValidJsonFile()`
Validates JSON export functionality for weekly summary reports. Ensures the generated file is valid JSON and contains all expected data.

## Usage
