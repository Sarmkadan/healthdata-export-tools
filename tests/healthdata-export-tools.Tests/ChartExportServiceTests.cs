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

public sealed class ChartExportServiceTests : IDisposable
{
    private readonly ChartExportService _chartExportService;
    private readonly string _tempDirectory;

    public ChartExportServiceTests()
    {
        _chartExportService = new ChartExportService();
        _tempDirectory = Path.Combine(Path.GetTempPath(), "ChartExportServiceTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }

    private HealthDataCollection CreateSampleCollection()
    {
        var collection = new HealthDataCollection();
        collection.HeartRateRecords.Add(new HeartRateData { RecordDate = new DateTime(2024, 1, 1), AverageBpm = 72 });
        collection.StepsRecords.Add(new StepsData { RecordDate = new DateTime(2024, 1, 1), TotalSteps = 8500 });
        collection.SleepRecords.Add(new SleepData { RecordDate = new DateTime(2024, 1, 1), DurationMinutes = 480 });
        return collection;
    }

    [Fact]
    public async Task ExportToHtmlChartsAsync_ShouldGenerateValidHtml()
    {
        // Arrange
        var collection = CreateSampleCollection();
        var outputPath = Path.Combine(_tempDirectory, "test_charts.html");

        // Act
        await _chartExportService.ExportToHtmlChartsAsync(collection, outputPath).ConfigureAwait(false);

        // Assert
        File.Exists(outputPath).Should().BeTrue();
        var content = await File.ReadAllTextAsync(outputPath).ConfigureAwait(false);
        content.Should().Contain("<!DOCTYPE html>");
        content.Should().Contain("<canvas id=\"heartRateChart\"></canvas>");
        content.Should().Contain("<canvas id=\"stepsChart\"></canvas>");
        content.Should().Contain("<canvas id=\"sleepChart\"></canvas>");
        content.Should().Contain("new Chart(");
        content.Should().Contain("72"); // Heart rate
        content.Should().Contain("8500"); // Steps
        content.Should().Contain("8"); // Sleep (480 mins = 8 hours)
    }

    [Fact]
    public async Task ExportToHtmlChartsAsync_ShouldHandleEmptyCollection()
    {
        // Arrange
        var collection = new HealthDataCollection();
        var outputPath = Path.Combine(_tempDirectory, "empty_charts.html");

        // Act
        await _chartExportService.ExportToHtmlChartsAsync(collection, outputPath).ConfigureAwait(false);

        // Assert
        File.Exists(outputPath).Should().BeTrue();
        var content = await File.ReadAllTextAsync(outputPath).ConfigureAwait(false);
        content.Should().Contain("<!DOCTYPE html>");
        content.Should().NotContain("<canvas id=\"heartRateChart\"></canvas>");
        content.Should().NotContain("<canvas id=\"stepsChart\"></canvas>");
        content.Should().NotContain("<canvas id=\"sleepChart\"></canvas>");
        content.Should().NotContain("new Chart(");
    }

    [Fact]
    public async Task ExportToHtmlChartsAsync_WithSpO2Records_ShouldIncludeSpO2Chart()
    {
        // Arrange
        var collection = new HealthDataCollection();
        collection.SpO2Records.Add(new SpO2Data
        {
            RecordDate = new DateTime(2024, 3, 1),
            AveragePercentage = 97,
            MinimumPercentage = 94,
            MaximumPercentage = 99
        });
        var outputPath = Path.Combine(_tempDirectory, "spo2_charts.html");

        // Act
        await _chartExportService.ExportToHtmlChartsAsync(collection, outputPath).ConfigureAwait(false);

        // Assert
        File.Exists(outputPath).Should().BeTrue();
        var content = await File.ReadAllTextAsync(outputPath).ConfigureAwait(false);
        content.Should().Contain("<canvas id=\"spo2Chart\"></canvas>");
        content.Should().Contain("Blood Oxygen Saturation");
        content.Should().Contain("97"); // avg SpO2 value
        content.Should().Contain("94"); // min SpO2 value
    }

    [Fact]
    public async Task ExportToHtmlChartsAsync_WithOptions_ShouldRespectDisabledCharts()
    {
        // Arrange
        var collection = CreateSampleCollection();
        collection.SpO2Records.Add(new SpO2Data
        {
            RecordDate = new DateTime(2024, 1, 1),
            AveragePercentage = 98,
            MinimumPercentage = 95,
            MaximumPercentage = 99
        });
        var options = new ChartExportOptions
        {
            IncludeSpO2Chart = false,
            IncludeSummaryTable = false,
            IncludeSleepCompositionChart = false,
            Title = "Custom Title"
        };
        var outputPath = Path.Combine(_tempDirectory, "options_charts.html");

        // Act
        await _chartExportService.ExportToHtmlChartsAsync(collection, outputPath, options).ConfigureAwait(false);

        // Assert
        var content = await File.ReadAllTextAsync(outputPath).ConfigureAwait(false);
        content.Should().Contain("Custom Title");
        content.Should().NotContain("<canvas id=\"spo2Chart\"></canvas>");
        content.Should().NotContain("<canvas id=\"sleepCompositionChart\"></canvas>");
        content.Should().NotContain("<table class=\"stats-table\">");
    }

    [Fact]
    public async Task ExportToHtmlChartsAsync_WithSleepData_ShouldIncludeSleepCompositionChart()
    {
        // Arrange
        var collection = new HealthDataCollection();
        collection.SleepRecords.Add(new SleepData
        {
            RecordDate = new DateTime(2024, 2, 1),
            DurationMinutes = 480,
            DeepSleepMinutes = 96,
            RemSleepMinutes = 72,
            LightSleepMinutes = 270,
            AwakeMinutes = 42
        });
        var outputPath = Path.Combine(_tempDirectory, "sleep_composition.html");

        // Act
        await _chartExportService.ExportToHtmlChartsAsync(collection, outputPath).ConfigureAwait(false);

        // Assert
        var content = await File.ReadAllTextAsync(outputPath).ConfigureAwait(false);
        content.Should().Contain("<canvas id=\"sleepChart\"></canvas>");
        content.Should().Contain("<canvas id=\"sleepCompositionChart\"></canvas>");
        content.Should().Contain("Sleep Composition");
        content.Should().Contain("96");  // deep sleep minutes
        content.Should().Contain("72");  // REM minutes
    }

    [Fact]
    public async Task ExportToHtmlChartsAsync_WithAllData_ShouldIncludeSummaryTable()
    {
        // Arrange
        var collection = CreateSampleCollection();
        collection.SpO2Records.Add(new SpO2Data
        {
            RecordDate = new DateTime(2024, 1, 1),
            AveragePercentage = 97,
            MinimumPercentage = 94,
            MaximumPercentage = 99
        });
        collection.ActivityRecords.Add(new ActivityData
        {
            RecordDate = new DateTime(2024, 1, 1),
            DurationMinutes = 45,
            CaloriesBurned = 350,
            StartTime = new DateTime(2024, 1, 1, 7, 0, 0),
            EndTime = new DateTime(2024, 1, 1, 7, 45, 0)
        });
        var outputPath = Path.Combine(_tempDirectory, "full_charts.html");

        // Act
        await _chartExportService.ExportToHtmlChartsAsync(collection, outputPath).ConfigureAwait(false);

        // Assert
        var content = await File.ReadAllTextAsync(outputPath).ConfigureAwait(false);
        content.Should().Contain("stats-table");
        content.Should().Contain("Heart Rate");
        content.Should().Contain("SpO2");
        content.Should().Contain("Activity");
        content.Should().Contain("<canvas id=\"activityChart\"></canvas>");
    }
}
