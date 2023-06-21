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

public sealed class ExportServiceTests
{
    private readonly ExportService _exportService;
    private readonly ILogger<ExportService> _mockLogger;
    private readonly string _tempDirectory;

    public ExportServiceTests()
    {
        _mockLogger = Substitute.For<ILogger<ExportService>>();
        _exportService = new ExportService(); // ExportService currently doesn't take logger, so instantiate directly
        _tempDirectory = Path.Combine(Path.GetTempPath(), "ExportServiceTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);
    }

    // Since XUnit doesn't have a direct "AfterAll" equivalent for a class,
    // we can implement IDisposable to clean up the temporary directory.
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

    [Fact]
    public async Task ExportToJsonAsync_ShouldCreateValidJsonFile()
    {
        // Arrange
        var collection = CreateSampleHealthDataCollection();
        var outputPath = Path.Combine(_tempDirectory, "test_export.json");

        // Act
        await _exportService.ExportToJsonAsync(collection, outputPath);

        // Assert
        File.Exists(outputPath).Should().BeTrue();
        var jsonContent = await File.ReadAllTextAsync(outputPath);
        jsonContent.Should().Contain("\"TotalRecords\": 6");
        jsonContent.Should().Contain("\"SleepRecords\":");
        jsonContent.Should().Contain("\"HeartRateRecords\":");
        jsonContent.Should().Contain("\"SpO2Records\":");
        jsonContent.Should().Contain("\"StepsRecords\":");
        jsonContent.Should().Contain("\"ActivityRecords\":");
        jsonContent.Should().Contain("\"Metrics\":");
    }

    [Fact]
    public async Task ExportToJsonAsync_ShouldHandleEmptyCollection()
    {
        // Arrange
        var collection = new HealthDataCollection();
        var outputPath = Path.Combine(_tempDirectory, "empty_export.json");

        // Act
        await _exportService.ExportToJsonAsync(collection, outputPath);

        // Assert
        File.Exists(outputPath).Should().BeTrue();
        var jsonContent = await File.ReadAllTextAsync(outputPath);
        jsonContent.Should().Contain("\"TotalRecords\": 0");
        jsonContent.Should().Contain("\"SleepRecords\": []");
    }

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
                LightSleepMinutes = 270, RemSleepMinutes = 60, AwakeMinutes = 60, Score = 85, AverageHeartRate = 60
            }
        };
        var outputPath = Path.Combine(_tempDirectory, "sleep_export.csv");

        // Act
        await _exportService.ExportSleepToCsvAsync(sleepRecords, outputPath);

        // Assert
        File.Exists(outputPath).Should().BeTrue();
        var csvContent = await File.ReadAllTextAsync(outputPath);
        csvContent.Should().Contain("Date,Duration,DeepSleep,LightSleep,REM,Awake,Quality,Score,AvgHeartRate");
        csvContent.Should().Contain("2024-01-01,480,90,270,60,60,Good,85,60");
    }

    [Fact]
    public async Task ExportCompleteAsync_ShouldExportAllFormatsWhenSpecified()
    {
        // Arrange
        var collection = CreateSampleHealthDataCollection();
        var outputDir = Path.Combine(_tempDirectory, "complete_export_all");
        
        // Act
        await _exportService.ExportCompleteAsync(collection, outputDir, ExportFormat.All);

        // Assert
        Directory.Exists(outputDir).Should().BeTrue();
        File.Exists(Path.Combine(outputDir, "health_data.json")).Should().BeTrue();
        File.Exists(Path.Combine(outputDir, "sleep.csv")).Should().BeTrue();
        File.Exists(Path.Combine(outputDir, "heart_rate.csv")).Should().BeTrue();
        File.Exists(Path.Combine(outputDir, "steps.csv")).Should().BeTrue();
    }

    [Fact]
    public async Task ExportCompleteAsync_ShouldCreateOutputDirectoryIfNotExists()
    {
        // Arrange
        var collection = CreateSampleHealthDataCollection();
        var nonExistentDir = Path.Combine(_tempDirectory, "non_existent_output");
        
        // Act
        await _exportService.ExportCompleteAsync(collection, nonExistentDir, ExportFormat.Json);

        // Assert
        Directory.Exists(nonExistentDir).Should().BeTrue();
        File.Exists(Path.Combine(nonExistentDir, "health_data.json")).Should().BeTrue();
    }
    
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
        await _exportService.ExportHeartRateToCsvAsync(hrRecords, outputPath);

        // Assert
        File.Exists(outputPath).Should().BeTrue();
        var csvContent = await File.ReadAllTextAsync(outputPath);
        csvContent.Should().Contain("Date,MinBpm,MaxBpm,AvgBpm,RestingBpm,Measurements,StressLevel,CardioZone");
        csvContent.Should().Contain("2024-01-01,50,120,70,60,100,5,30");
    }

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
        await _exportService.ExportStepsToCsvAsync(stepsRecords, outputPath);

        // Assert
        File.Exists(outputPath).Should().BeTrue();
        var csvContent = await File.ReadAllTextAsync(outputPath);
        csvContent.Should().Contain("Date,Steps,DistanceKm,Calories,GoalAchievement,ActiveMinutes,Walking,Running");
        csvContent.Should().Contain("2024-01-01,10000,7.5,500,100,120,90,30");
    }
}