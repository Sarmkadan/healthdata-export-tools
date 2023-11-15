// Health Data Export Tools

...

## DataComparisonServiceTestsExtensions

The `DataComparisonServiceTestsExtensions` class provides extension methods for testing data comparison scenarios. These extensions simplify testing by creating mock data collections and asserting expected percentage changes.

### Usage Example

```csharp
using HealthDataExportTools.Tests;

var test = new DataComparisonServiceTests();
var period1Data = test.WithSleepData(new[]
{
    new SleepData { SleepStart = DateTime.Today.AddDays(-1), DurationMinutes = 480 },
    new SleepData { SleepStart = DateTime.Today.AddDays(-2), DurationMinutes = 420 },
});

var period2Data = test.WithSleepData(new[]
{
    new SleepData { SleepStart = DateTime.Today.AddDays(-8), DurationMinutes = 380 },
    new SleepData { SleepStart = DateTime.Today.AddDays(-9), DurationMinutes = 400 },
});

var result = await DataComparisonServiceTestsExtensions.CreateComparisonResultAsync(period1Data, period2Data);

DataComparisonServiceTestsExtensions.ShouldHavePercentageChanges(result, 15, -5);
DataComparisonServiceTestsExtensions.ShouldHaveSpO2Change(result, 0, 0);
DataComparisonServiceTestsExtensions.ShouldHaveActivityChange(result, 0, 0);
DataComparisonServiceTestsExtensions.ShouldHaveDeepSleepChange(result, 10, -5);
```

## ChartExportServiceTestsExtensions

The `ChartExportServiceTestsExtensions` class provides extension methods for testing chart export scenarios. These extensions simplify testing by creating mock data collections and asserting expected chart data.

### Usage Example

```csharp
using HealthDataExportTools.Tests;

var test = new ChartExportServiceTests();
var data = test.WithMultiDayData(new[]
{
    new HealthDataRecord { Date = DateTime.Today.AddDays(-1), HeartRate = 60 },
    new HealthDataRecord { Date = DateTime.Today.AddDays(-2), HeartRate = 70 },
});

await ChartExportServiceTestsExtensions.ShouldContainHeartRateChartDataAsync(data);
await ChartExportServiceTestsExtensions.ShouldContainStepsChartDataAsync(data);
await ChartExportServiceTestsExtensions.ShouldContainSleepChartDataAsync(data);
await ChartExportServiceTestsExtensions.ShouldContainSpO2ChartDataAsync(data);
await ChartExportServiceTestsExtensions.ShouldContainActivityChartDataAsync(data);
await ChartExportServiceTestsExtensions.ShouldContainTitleAsync(data);
await ChartExportServiceTestsExtensions.ShouldContainSummaryTableWithHeadersAsync(data);
```