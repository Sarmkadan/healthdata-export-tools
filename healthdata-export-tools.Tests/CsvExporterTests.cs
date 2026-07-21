#nullable enable
// =============================================================================
// Author: Automated Test Generation
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using HealthDataExportTools.Domain.Models;
using HealthDataExportTools.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace HealthDataExportTools.Tests;

/// <summary>
/// Tests for <see cref="CsvExporter"/> covering header generation,
/// column selection, empty collections and multi‑type export behavior.
/// </summary>
public sealed class CsvExporterTests : IDisposable
{
    private readonly CsvExporter _exporter;
    private readonly Mock<ILogger<CsvExporter>> _loggerMock;
    private readonly string _tempDir;

    public CsvExporterTests()
    {
        _loggerMock = new Mock<ILogger<CsvExporter>>();
        _exporter = new CsvExporter(_loggerMock.Object);
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
    }

    [Fact]
    public async Task ExportSleep_AllColumns_HeaderAndValues_AreCorrect()
    {
        // Arrange
        var collection = new HealthDataCollection
        {
            SleepRecords = new List<SleepData>
            {
                new SleepData
                {
                    RecordDate = new DateTime(2024, 1, 1, 22, 0, 0, DateTimeKind.Utc),
                    DurationMinutes = 480,
                    DeepSleepMinutes = 180,
                    LightSleepMinutes = 240,
                    RemSleepMinutes = 60,
                    AwakeMinutes = 30,
                    Quality = SleepQuality.Good,
                    Score = 85,
                    AverageHeartRate = 60
                }
            }
        };

        var options = new CsvExportOptions
        {
            IncludeSleep = true               // ensure the exporter creates the file
            // No column filter – all columns should be written
        };

        // Act
        await _exporter.ExportToCsvAsync(collection, _tempDir, options);

        // Assert
        var sleepPath = Path.Combine(_tempDir, "sleep.csv");
        Assert.True(File.Exists(sleepPath));

        var lines = await File.ReadAllLinesAsync(sleepPath);
        Assert.Equal(2, lines.Length); // header + one data line

        var expectedHeader = "Date,Duration,DeepSleep,LightSleep,REM,Awake,Quality,Score,AvgHeartRate";
        Assert.Equal(expectedHeader, lines[0]);

        var fields = lines[1].Split(',');
        Assert.Equal(9, fields.Length);
        Assert.Equal(collection.SleepRecords[0].RecordDate.ToString(options.DateFormat, CultureInfo.InvariantCulture), fields[0]);
        Assert.Equal("480", fields[1]);
        Assert.Equal("180", fields[2]);
        Assert.Equal("240", fields[3]);
        Assert.Equal("60", fields[4]);
        Assert.Equal("30", fields[5]);
        Assert.Equal("Good", fields[6]);
        Assert.Equal("85", fields[7]);
        Assert.Equal("60", fields[8]);
    }

    [Fact]
    public async Task ExportSleep_SelectedColumns_HeaderSubset_IsRespected()
    {
        // Arrange
        var collection = new HealthDataCollection
        {
            SleepRecords = new List<SleepData>
            {
                new SleepData
                {
                    RecordDate = new DateTime(2024, 2, 2, 22, 0, 0, DateTimeKind.Utc),
                    DurationMinutes = 300,
                    Quality = SleepQuality.Fair,
                    Score = 70
                }
            }
        };

        var options = new CsvExportOptions
        {
            IncludeSleep = true,
            SleepColumns = new[] { "Date", "Duration", "Score" } // only these columns should appear
        };

        // Act
        await _exporter.ExportToCsvAsync(collection, _tempDir, options);

        // Assert
        var sleepPath = Path.Combine(_tempDir, "sleep.csv");
        Assert.True(File.Exists(sleepPath));

        var lines = await File.ReadAllLinesAsync(sleepPath);
        Assert.Equal(2, lines.Length); // header + data

        var expectedHeader = "Date,Duration,Score";
        Assert.Equal(expectedHeader, lines[0]);

        var fields = lines[1].Split(',');
        Assert.Equal(3, fields.Length);
        Assert.Equal(collection.SleepRecords[0].RecordDate.ToString(options.DateFormat, CultureInfo.InvariantCulture), fields[0]);
        Assert.Equal("300", fields[1]);
        Assert.Equal("70", fields[2]);
    }

    [Fact]
    public async Task ExportEmptyCollection_NoFilesAreCreated()
    {
        // Arrange
        var emptyCollection = new HealthDataCollection(); // all record lists are empty
        var options = new CsvExportOptions
        {
            IncludeSleep = true,
            IncludeHeartRate = true,
            IncludeSpO2 = true,
            IncludeSteps = true
        };

        // Act
        await _exporter.ExportToCsvAsync(emptyCollection, _tempDir, options);

        // Assert
        var files = Directory.GetFiles(_tempDir);
        Assert.Empty(files); // exporter should not create any CSV files when there is no data
    }

    [Fact]
    public async Task ExportMultipleRecordTypes_AllRequestedFilesAreCreated()
    {
        // Arrange
        var collection = new HealthDataCollection
        {
            SleepRecords = new List<SleepData>
            {
                new SleepData { RecordDate = DateTime.UtcNow, DurationMinutes = 100 }
            },
            HeartRateRecords = new List<HeartRateData>
            {
                new HeartRateData { RecordDate = DateTime.UtcNow, AverageBpm = 70, MinimumBpm = 60, MaximumBpm = 80 }
            },
            StepsRecords = new List<StepsData>
            {
                new StepsData { RecordDate = DateTime.UtcNow, TotalSteps = 5000 }
            }
        };

        var options = new CsvExportOptions
        {
            IncludeSleep = true,
            IncludeHeartRate = true,
            IncludeSpO2 = false,
            IncludeSteps = true
        };

        // Act
        await _exporter.ExportToCsvAsync(collection, _tempDir, options);

        // Assert
        var expectedFiles = new[] { "sleep.csv", "heart_rate.csv", "steps.csv" };
        foreach (var fileName in expectedFiles)
        {
            var path = Path.Combine(_tempDir, fileName);
            Assert.True(File.Exists(path), $"Expected file {fileName} to exist.");
        }

        // Ensure SpO2 file was NOT created
        var spo2Path = Path.Combine(_tempDir, "spo2.csv");
        Assert.False(File.Exists(spo2Path));
    }
}
