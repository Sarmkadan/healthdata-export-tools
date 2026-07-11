#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using HealthDataExportTools.Domain.Models;
using HealthDataExportTools.Services;
using Xunit;

namespace HealthDataExportTools.Tests;

public static class ChartExportServiceTestsExtensions
{
    /// <summary>
    /// Asserts that the generated HTML file contains valid chart data for heart rate records.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <param name="outputPath">Path to the generated HTML file.</param>
    /// <param name="expectedHeartRateValues">Expected heart rate values to verify.</param>
    /// <exception cref="ArgumentNullException"><paramref name="outputPath"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="outputPath"/> is empty or whitespace.</exception>
    public static async Task ShouldContainHeartRateChartDataAsync(
        this ChartExportServiceTests test,
        string outputPath,
        params int[] expectedHeartRateValues)
    {
        ArgumentNullException.ThrowIfNull(outputPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        // Act
        var content = await File.ReadAllTextAsync(outputPath).ConfigureAwait(false);

        // Assert
        content.Should().Contain("<canvas id=\"heartRateChart\"></canvas>");
        content.Should().Contain("new Chart(");

        foreach (var expectedValue in expectedHeartRateValues)
        {
            content.Should().Contain(expectedValue.ToString());
        }
    }

    /// <summary>
    /// Asserts that the generated HTML file contains valid chart data for steps records.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <param name="outputPath">Path to the generated HTML file.</param>
    /// <param name="expectedStepValues">Expected step values to verify.</param>
    /// <exception cref="ArgumentNullException"><paramref name="outputPath"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="outputPath"/> is empty or whitespace.</exception>
    public static async Task ShouldContainStepsChartDataAsync(
        this ChartExportServiceTests test,
        string outputPath,
        params int[] expectedStepValues)
    {
        ArgumentNullException.ThrowIfNull(outputPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        // Act
        var content = await File.ReadAllTextAsync(outputPath).ConfigureAwait(false);

        // Assert
        content.Should().Contain("<canvas id=\"stepsChart\"></canvas>");
        content.Should().Contain("new Chart(");

        foreach (var expectedValue in expectedStepValues)
        {
            content.Should().Contain(expectedValue.ToString());
        }
    }

    /// <summary>
    /// Asserts that the generated HTML file contains valid chart data for sleep records.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <param name="outputPath">Path to the generated HTML file.</param>
    /// <param name="expectedSleepDurations">Expected sleep durations in hours.</param>
    /// <exception cref="ArgumentNullException"><paramref name="outputPath"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="outputPath"/> is empty or whitespace.</exception>
    public static async Task ShouldContainSleepChartDataAsync(
        this ChartExportServiceTests test,
        string outputPath,
        params int[] expectedSleepDurations)
    {
        ArgumentNullException.ThrowIfNull(outputPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        // Act
        var content = await File.ReadAllTextAsync(outputPath).ConfigureAwait(false);

        // Assert
        content.Should().Contain("<canvas id=\"sleepChart\"></canvas>");
        content.Should().Contain("new Chart(");

        foreach (var expectedDuration in expectedSleepDurations)
        {
            content.Should().Contain(expectedDuration.ToString());
        }
    }

    /// <summary>
    /// Asserts that the generated HTML file contains valid chart data for SpO2 records.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <param name="outputPath">Path to the generated HTML file.</param>
    /// <param name="expectedAverageSpO2">Expected average SpO2 values.</param>
    /// <param name="expectedMinSpO2">Expected minimum SpO2 values.</param>
    /// <param name="expectedMaxSpO2">Expected maximum SpO2 values.</param>
    /// <exception cref="ArgumentNullException"><paramref name="outputPath"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="outputPath"/> is empty or whitespace.</exception>
    public static async Task ShouldContainSpO2ChartDataAsync(
        this ChartExportServiceTests test,
        string outputPath,
        int[] expectedAverageSpO2,
        int[] expectedMinSpO2,
        int[] expectedMaxSpO2)
    {
        ArgumentNullException.ThrowIfNull(outputPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        // Act
        var content = await File.ReadAllTextAsync(outputPath).ConfigureAwait(false);

        // Assert
        content.Should().Contain("<canvas id=\"spo2Chart\"></canvas>");
        content.Should().Contain("Blood Oxygen Saturation");
        content.Should().Contain("new Chart(");

        foreach (var avg in expectedAverageSpO2)
        {
            content.Should().Contain(avg.ToString());
        }

        foreach (var min in expectedMinSpO2)
        {
            content.Should().Contain(min.ToString());
        }

        foreach (var max in expectedMaxSpO2)
        {
            content.Should().Contain(max.ToString());
        }
    }

    /// <summary>
    /// Asserts that the generated HTML file contains valid chart data for activity records.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <param name="outputPath">Path to the generated HTML file.</param>
    /// <param name="expectedCalories">Expected calories burned values.</param>
    /// <exception cref="ArgumentNullException"><paramref name="outputPath"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="outputPath"/> is empty or whitespace.</exception>
    public static async Task ShouldContainActivityChartDataAsync(
        this ChartExportServiceTests test,
        string outputPath,
        params int[] expectedCalories)
    {
        ArgumentNullException.ThrowIfNull(outputPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        // Act
        var content = await File.ReadAllTextAsync(outputPath).ConfigureAwait(false);

        // Assert
        content.Should().Contain("<canvas id=\"activityChart\"></canvas>");
        content.Should().Contain("new Chart(");

        foreach (var calories in expectedCalories)
        {
            content.Should().Contain(calories.ToString());
        }
    }

    /// <summary>
    /// Asserts that the generated HTML file contains the specified title.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <param name="outputPath">Path to the generated HTML file.</param>
    /// <param name="expectedTitle">Expected title text.</param>
    /// <exception cref="ArgumentNullException"><paramref name="outputPath"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="outputPath"/> is empty or whitespace.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="expectedTitle"/> is <see langword="null"/>.</exception>
    public static async Task ShouldContainTitleAsync(
        this ChartExportServiceTests test,
        string outputPath,
        string expectedTitle)
    {
        ArgumentNullException.ThrowIfNull(outputPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        ArgumentNullException.ThrowIfNull(expectedTitle);
        ArgumentException.ThrowIfNullOrWhiteSpace(expectedTitle);

        // Act
        var content = await File.ReadAllTextAsync(outputPath).ConfigureAwait(false);

        // Assert
        content.Should().Contain(expectedTitle);
    }

    /// <summary>
    /// Asserts that the generated HTML file contains a summary table with the specified headers.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <param name="outputPath">Path to the generated HTML file.</param>
    /// <param name="expectedHeaders">Expected table header texts.</param>
    /// <exception cref="ArgumentNullException"><paramref name="outputPath"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="outputPath"/> is empty or whitespace.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="expectedHeaders"/> is <see langword="null"/>.</exception>
    public static async Task ShouldContainSummaryTableWithHeadersAsync(
        this ChartExportServiceTests test,
        string outputPath,
        params string[] expectedHeaders)
    {
        ArgumentNullException.ThrowIfNull(outputPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        ArgumentNullException.ThrowIfNull(expectedHeaders);

        // Act
        var content = await File.ReadAllTextAsync(outputPath).ConfigureAwait(false);

        // Assert
        content.Should().Contain("stats-table");
        content.Should().Contain("<table");

        foreach (var header in expectedHeaders)
        {
            content.Should().Contain(header);
        }
    }

    /// <summary>
    /// Creates a health data collection with multiple records for comprehensive testing.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <param name="heartRateRecords">Heart rate records to add.</param>
    /// <param name="stepsRecords">Steps records to add.</param>
    /// <param name="sleepRecords">Sleep records to add.</param>
    /// <param name="spO2Records">SpO2 records to add.</param>
    /// <param name="activityRecords">Activity records to add.</param>
    /// <returns>Configured HealthDataCollection.</returns>
    public static HealthDataCollection WithRecords(
        this ChartExportServiceTests test,
        List<HeartRateData>? heartRateRecords = null,
        List<StepsData>? stepsRecords = null,
        List<SleepData>? sleepRecords = null,
        List<SpO2Data>? spO2Records = null,
        List<ActivityData>? activityRecords = null)
    {
        var collection = new HealthDataCollection();

        if (heartRateRecords is not null)
        {
            foreach (var record in heartRateRecords)
            {
                collection.HeartRateRecords.Add(record);
            }
        }

        if (stepsRecords is not null)
        {
            foreach (var record in stepsRecords)
            {
                collection.StepsRecords.Add(record);
            }
        }

        if (sleepRecords is not null)
        {
            foreach (var record in sleepRecords)
            {
                collection.SleepRecords.Add(record);
            }
        }

        if (spO2Records is not null)
        {
            foreach (var record in spO2Records)
            {
                collection.SpO2Records.Add(record);
            }
        }

        if (activityRecords is not null)
        {
            foreach (var record in activityRecords)
            {
                collection.ActivityRecords.Add(record);
            }
        }

        return collection;
    }

    /// <summary>
    /// Creates a health data collection with records spanning multiple days.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <returns>Configured HealthDataCollection with multi-day data.</returns>
    public static HealthDataCollection WithMultiDayData(
        this ChartExportServiceTests test)
    {
        var collection = new HealthDataCollection();

        // Day 1
        collection.HeartRateRecords.Add(new HeartRateData { RecordDate = new DateTime(2024, 1, 1), AverageBpm = 72 });
        collection.StepsRecords.Add(new StepsData { RecordDate = new DateTime(2024, 1, 1), TotalSteps = 8500 });
        collection.SleepRecords.Add(new SleepData { RecordDate = new DateTime(2024, 1, 1), DurationMinutes = 480 });

        // Day 2
        collection.HeartRateRecords.Add(new HeartRateData { RecordDate = new DateTime(2024, 1, 2), AverageBpm = 75 });
        collection.StepsRecords.Add(new StepsData { RecordDate = new DateTime(2024, 1, 2), TotalSteps = 10200 });
        collection.SleepRecords.Add(new SleepData { RecordDate = new DateTime(2024, 1, 2), DurationMinutes = 450 });

        // Day 3
        collection.HeartRateRecords.Add(new HeartRateData { RecordDate = new DateTime(2024, 1, 3), AverageBpm = 68 });
        collection.StepsRecords.Add(new StepsData { RecordDate = new DateTime(2024, 1, 3), TotalSteps = 6800 });
        collection.SleepRecords.Add(new SleepData { RecordDate = new DateTime(2024, 1, 3), DurationMinutes = 510 });

        return collection;
    }

    /// <summary>
    /// Creates a health data collection with extreme values for boundary testing.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <returns>Configured HealthDataCollection with extreme values.</returns>
    public static HealthDataCollection WithExtremeValues(
        this ChartExportServiceTests test)
    {
        var collection = new HealthDataCollection();

        // Extreme heart rate
        collection.HeartRateRecords.Add(new HeartRateData { RecordDate = new DateTime(2024, 1, 1), AverageBpm = 220 });
        collection.HeartRateRecords.Add(new HeartRateData { RecordDate = new DateTime(2024, 1, 2), AverageBpm = 30 });

        // Extreme steps
        collection.StepsRecords.Add(new StepsData { RecordDate = new DateTime(2024, 1, 1), TotalSteps = 0 });
        collection.StepsRecords.Add(new StepsData { RecordDate = new DateTime(2024, 1, 2), TotalSteps = 25000 });

        // Extreme sleep
        collection.SleepRecords.Add(new SleepData { RecordDate = new DateTime(2024, 1, 1), DurationMinutes = 0 });
        collection.SleepRecords.Add(new SleepData { RecordDate = new DateTime(2024, 1, 2), DurationMinutes = 1440 }); // 24 hours

        // Extreme SpO2
        collection.SpO2Records.Add(new SpO2Data { RecordDate = new DateTime(2024, 1, 1), AveragePercentage = 100, MinimumPercentage = 90, MaximumPercentage = 100 });
        collection.SpO2Records.Add(new SpO2Data { RecordDate = new DateTime(2024, 1, 2), AveragePercentage = 85, MinimumPercentage = 80, MaximumPercentage = 88 });

        return collection;
    }
}