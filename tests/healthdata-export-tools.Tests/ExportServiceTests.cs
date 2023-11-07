#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using HealthDataExportTools.Domain.Enums;
using HealthDataExportTools.Domain.Models;
using HealthDataExportTools.Exceptions;
using HealthDataExportTools.Services;
using HealthDataExportTools.Utilities;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace HealthDataExportTools.Tests;

/// <summary>
/// Contains unit tests for <see cref="ExportService"/> verifying its export
/// functionality for JSON and CSV formats, handling of empty collections,
/// directory creation, and various data types.
/// </summary>
public sealed class ExportServiceTests
{
    private readonly ExportService _exportService;
    private readonly ILogger<ExportService> _mockLogger;
    private readonly string _tempDirectory;

    /// <summary>
    /// Initializes a new instance of <see cref="ExportServiceTests"/>.
    /// Sets up a mock logger, creates an <see cref="ExportService"/> instance,
    /// and prepares a temporary directory for test output files.
    /// </summary>
    public ExportServiceTests()
    {
        _mockLogger = Substitute.For<ILogger<ExportService>>();
        _exportService = new ExportService(); // ExportService currently doesn't take logger, so instantiate directly
        _tempDirectory = Path.Combine(Path.GetTempPath(), "ExportServiceTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);
    }

    /// <summary>
    /// Cleans up the temporary directory created for the tests.
    /// </summary>
    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }

    private HealthDataCollection CreateSampleHealthDataCollection()
    {
        var collection = new HealthDataCollection();
        collection.SleepRecords.Add(new SleepData
        {
            RecordDate = new DateTime(2024, 1, 1), DeviceId = "dev1", SleepStart = new DateTime(2024, 1, 1, 22, 0, 0),
            SleepEnd = new DateTime(2024, 1, 2, 6, 0, 0), DurationMinutes = 480, DeepSleepMinutes = 90,
            LightSleepMinutes = 270, RemSleepMinutes = 60, AwakeMinutes = 60
        });
        collection.HeartRateRecords.Add(new HeartRateData
        {
            RecordDate = new DateTime(2024, 1, 1), DeviceId = "dev1", MinimumBpm = 50, MaximumBpm = 120, AverageBpm = 70
        });
        collection.SpO2Records.Add(new SpO2Data
        {
            RecordDate = new DateTime(2024, 1, 1), DeviceId = "dev1", MinimumPercentage = 95, MaximumPercentage = 99, AveragePercentage = 97
        });
        collection.StepsRecords.Add(new StepsData
        {
            RecordDate = new DateTime(2024, 1, 1), DeviceId = "dev1", TotalSteps = 10000, DistanceKm = 7.5, CaloriesBurned = 500, DailyGoal = 10000
        });
        collection.ActivityRecords.Add(new ActivityData
        {
            RecordDate = new DateTime(2024, 1, 1), DeviceId = "dev1", ActivityType = "Running", StartTime = new DateTime(2024, 1, 1, 10, 0, 0), EndTime = new DateTime(2024, 1, 1, 11, 0, 0), DurationMinutes = 60, DistanceKm = 10, CaloriesBurned = 600
        });
        collection.Metrics.Add(new HealthMetric
        {
            RecordDate = new DateTime(2024, 1, 1), MetricName = "Weight", Value = 70.5, Unit = "kg"
        });
        return collection;
    }

    /// <summary>
    /// Verifies that <see cref="ExportService.ExportToJsonAsync"/> creates a valid JSON
    /// file containing all expected sections and a correct total record count.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task ExportToJsonAsync_ShouldCreateValidJsonFile()
    {
        // Arrange
        var collection = CreateSampleHealthDataCollection();
        var outputPath = Path.Combine(_tempDirectory, "test_export.json");

        // Act
        await _exportService.ExportToJsonAsync(collection, outputPath).ConfigureAwait(false);

        // Assert
        File.Exists(outputPath).Should().BeTrue();
        var jsonContent = await File.ReadAllTextAsync(outputPath).ConfigureAwait(false);
        jsonContent.Should().Contain("\"TotalRecords\": 6");
        jsonContent.Should().Contain("\"SleepRecords\":");
        jsonContent.Should().Contain("\"HeartRateRecords\":");
        jsonContent.Should().Contain("\"SpO2Records\":");
        jsonContent.Should().Contain("\"StepsRecords\":");
        jsonContent.Should().Contain("\"ActivityRecords\":");
        jsonContent.Should().Contain("\"Metrics\":");
    }

    /// <summary>
    /// Ensures that exporting an empty <see cref="HealthDataCollection"/> still
    /// produces a JSON file with a total record count of zero and empty arrays.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task ExportToJsonAsync_ShouldHandleEmptyCollection()
    {
        // Arrange
        var collection = new HealthDataCollection();
        var outputPath = Path.Combine(_tempDirectory, "empty_export.json");

        // Act
        await _exportService.ExportToJsonAsync(collection, outputPath).ConfigureAwait(false);

        // Assert
        File.Exists(outputPath).Should().BeTrue();
        var jsonContent = await File.ReadAllTextAsync(outputPath).ConfigureAwait(false);
        jsonContent.Should().Contain("\"TotalRecords\": 0");
        jsonContent.Should().Contain("\"SleepRecords\": []");
    }

    /// <summary>
    /// Checks that <see cref="ExportService.ExportSleepToCsvAsync"/> creates a CSV file
    /// with the correct header and a row representing the provided sleep record.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task ExportSleepToCsvAsync_ShouldCreateValidCsvFile()
    {
        // Arrange
        var sleepRecords = new List<SleepData>
        {
            new SleepData
            {
                RecordDate = new DateTime(2024, 1, 1), DeviceId = "dev1", SleepStart = new DateTime(2024, 1, 1, 22, 0, 0),
                SleepEnd = new DateTime(2024, 1, 2, 6, 0, 0), DurationMinutes = 480, DeepSleepMinutes = 90,
                LightSleepMinutes = 270, RemSleepMinutes = 60, AwakeMinutes = 60, Score = 85, AverageHeartRate = 60, Quality = SleepQuality.Good
            }
        };
        var outputPath = Path.Combine(_tempDirectory, "sleep_export.csv");

        // Act
        await _exportService.ExportSleepToCsvAsync(sleepRecords, outputPath).ConfigureAwait(false);

        // Assert
        File.Exists(outputPath).Should().BeTrue();
        var csvContent = await File.ReadAllTextAsync(outputPath).ConfigureAwait(false);
        csvContent.Should().Contain("Date,Duration,DeepSleep,LightSleep,REM,Awake,Quality,Score,AvgHeartRate");
        csvContent.Should().Contain("2024-01-01,480,90,270,60,60,Good,85,60");
    }

    /// <summary>
    /// Validates that <see cref="ExportService.ExportCompleteAsync"/> exports all
    /// supported formats when <see cref="ExportFormat.All"/> is specified.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task ExportCompleteAsync_ShouldExportAllFormatsWhenSpecified()
    {
        // Arrange
        var collection = CreateSampleHealthDataCollection();
        var outputDir = Path.Combine(_tempDirectory, "complete_export_all");
        
        // Act
        await _exportService.ExportCompleteAsync(collection, outputDir, ExportFormat.All).ConfigureAwait(false);

        // Assert
        Directory.Exists(outputDir).Should().BeTrue();
        File.Exists(Path.Combine(outputDir, "health_data.json")).Should().BeTrue();
        File.Exists(Path.Combine(outputDir, "sleep.csv")).Should().BeTrue();
        File.Exists(Path.Combine(outputDir, "heart_rate.csv")).Should().BeTrue();
        File.Exists(Path.Combine(outputDir, "steps.csv")).Should().BeTrue();
    }

    /// <summary>
    /// Confirms that <see cref="ExportService.ExportCompleteAsync"/> creates the
    /// output directory if it does not already exist when exporting JSON only.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task ExportCompleteAsync_ShouldCreateOutputDirectoryIfNotExists()
    {
        // Arrange
        var collection = CreateSampleHealthDataCollection();
        var nonExistentDir = Path.Combine(_tempDirectory, "non_existent_output");
        
        // Act
        await _exportService.ExportCompleteAsync(collection, nonExistentDir, ExportFormat.Json).ConfigureAwait(false);

        // Assert
        Directory.Exists(nonExistentDir).Should().BeTrue();
        File.Exists(Path.Combine(nonExistentDir, "health_data.json")).Should().BeTrue();
    }
    
    /// <summary>
    /// Ensures that <see cref="ExportService.ExportHeartRateToCsvAsync"/> produces a CSV
    /// file with the correct header and data row for a heart rate record.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task ExportHeartRateToCsvAsync_ShouldCreateValidCsvFile()
    {
        // Arrange
        var hrRecords = new List<HeartRateData>
        {
            new HeartRateData
            {
                RecordDate = new DateTime(2024, 1, 1), DeviceId = "dev1", MinimumBpm = 50, MaximumBpm = 120, AverageBpm = 70, RestingBpm = 60, MeasurementCount = 100, StressLevel = 5, CardioZoneMinutes = 30
            }
        };
        var outputPath = Path.Combine(_tempDirectory, "heart_rate_export.csv");

        // Act
        await _exportService.ExportHeartRateToCsvAsync(hrRecords, outputPath).ConfigureAwait(false);

        // Assert
        File.Exists(outputPath).Should().BeTrue();
        var csvContent = await File.ReadAllTextAsync(outputPath).ConfigureAwait(false);
        csvContent.Should().Contain("Date,MinBpm,MaxBpm,AvgBpm,RestingBpm,Measurements,StressLevel,CardioZone");
        csvContent.Should().Contain("2024-01-01,50,120,70,60,100,5,30");
    }

    /// <summary>
    /// Verifies that <see cref="ExportService.ExportStepsToCsvAsync"/> creates a CSV file
    /// with the appropriate header and a row representing a steps record.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task ExportStepsToCsvAsync_ShouldCreateValidCsvFile()
    {
        // Arrange
        var stepsRecords = new List<StepsData>
        {
            new StepsData
            {
                RecordDate = new DateTime(2024, 1, 1), DeviceId = "dev1", TotalSteps = 10000, DistanceKm = 7.5, CaloriesBurned = 500, DailyGoal = 10000, ActiveMinutes = 120, WalkingMinutes = 90, RunningMinutes = 30
            }
        };
        var outputPath = Path.Combine(_tempDirectory, "steps_export.csv");

        // Act
        await _exportService.ExportStepsToCsvAsync(stepsRecords, outputPath).ConfigureAwait(false);

        // Assert
        File.Exists(outputPath).Should().BeTrue();
        var csvContent = await File.ReadAllTextAsync(outputPath).ConfigureAwait(false);
        csvContent.Should().Contain("Date,Steps,DistanceKm,Calories,GoalAchievement,ActiveMinutes,Walking,Running");
        csvContent.Should().Contain("2024-01-01,10000,7.5,500,100,120,90,30");
    }
}
