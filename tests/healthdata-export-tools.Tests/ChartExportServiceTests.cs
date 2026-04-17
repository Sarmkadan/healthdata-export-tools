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
}
