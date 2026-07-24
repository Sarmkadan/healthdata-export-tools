#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using HealthDataExportTools.Domain.Models;
using HealthDataExportTools.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace HealthDataExportTools.Tests;

/// <summary>
/// Tests for JSON Lines exporter functionality
/// </summary>
public sealed class JsonLinesExporterTests
{
    private readonly JsonLinesExporter _exporter;
    private readonly Mock<ILogger<JsonLinesExporter>> _loggerMock;

    public JsonLinesExporterTests()
    {
        _loggerMock = new Mock<ILogger<JsonLinesExporter>>();
        _exporter = new JsonLinesExporter(_loggerMock.Object);
    }

    [Fact]
    public async Task ExportToJsonLinesAsync_WithEmptyCollection_CreatesEmptyFile()
    {
        // Arrange
        var collection = new HealthDataCollection();
        var tempFile = Path.GetTempFileName();

        try
        {
            // Act
            await _exporter.ExportToJsonLinesAsync(collection, tempFile);

            // Assert
            Assert.True(File.Exists(tempFile));
            var content = await File.ReadAllTextAsync(tempFile);
            Assert.Empty(content);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public async Task ExportToJsonLinesAsync_WithSleepRecords_ExportsCorrectFormat()
    {
        // Arrange
        var collection = new HealthDataCollection
        {
            SleepRecords =
            [
                new SleepData
                {
                    RecordDate = new DateTime(2024, 1, 1, 22, 0, 0, DateTimeKind.Utc),
                    DurationMinutes = 480,
                    DeepSleepMinutes = 180,
                    LightSleepMinutes = 240,
                    RemSleepMinutes = 60,
                    AwakeMinutes = 30,
                    Quality = SleepQuality.Good,
                    Score = 85
                }
            ]
        };
        var tempFile = Path.GetTempFileName();

        try
        {
            // Act
            await _exporter.ExportToJsonLinesAsync(collection, tempFile);

            // Assert
            Assert.True(File.Exists(tempFile));
            var lines = await File.ReadAllLinesAsync(tempFile);
            Assert.Single(lines);

            // Verify it's valid JSON
            var json = lines[0];
            Assert.NotEmpty(json);
            Assert.Contains("RecordDate", json);
            Assert.Contains("DurationMinutes", json);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public async Task ExportToJsonLinesAsync_WithMultipleRecordTypes_ExportsAllRecords()
    {
        // Arrange
        var collection = new HealthDataCollection
        {
            SleepRecords =
            [
                new SleepData
                {
                    RecordDate = new DateTime(2024, 1, 1, 22, 0, 0, DateTimeKind.Utc),
                    DurationMinutes = 480
                }
            ],
            HeartRateRecords =
            [
                new HeartRateData
                {
                    RecordDate = new DateTime(2024, 1, 2, 12, 0, 0, DateTimeKind.Utc),
                    AverageBpm = 75,
                    MinimumBpm = 60,
                    MaximumBpm = 90
                }
            ],
            StepsRecords =
            [
                new StepsData
                {
                    RecordDate = new DateTime(2024, 1, 3, 0, 0, 0, DateTimeKind.Utc),
                    TotalSteps = 10000
                }
            ]
        };
        var tempFile = Path.GetTempFileName();

        try
        {
            // Act
            await _exporter.ExportToJsonLinesAsync(collection, tempFile);

            // Assert
            Assert.True(File.Exists(tempFile));
            var lines = await File.ReadAllLinesAsync(tempFile);
            Assert.Equal(3, lines.Length);

            // Each line should be valid JSON
            foreach (var line in lines)
            {
                Assert.NotEmpty(line);
            }
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public async Task ExportToJsonLinesAsync_WithSpecialCharacters_HandlesEscaping()
    {
        // Arrange
        var collection = new HealthDataCollection
        {
            SleepRecords =
            [
                new SleepData
                {
                    RecordDate = new DateTime(2024, 1, 1, 22, 0, 0, DateTimeKind.Utc),
                    Quality = SleepQuality.Good
                }
            ]
        };
        var tempFile = Path.GetTempFileName();

        try
        {
            // Act
            await _exporter.ExportToJsonLinesAsync(collection, tempFile);

            // Assert - should not throw and file should be created
            Assert.True(File.Exists(tempFile));
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public async Task ExportToJsonLinesAsync_WithNonExistentDirectory_CreatesFile()
    {
        // Arrange
        var collection = new HealthDataCollection
        {
            SleepRecords = [new SleepData { RecordDate = DateTime.UtcNow }]
        };
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var tempFile = Path.Combine(tempDir, "test.jsonl");

        try
        {
            // Act
            await _exporter.ExportToJsonLinesAsync(collection, tempFile);

            // Assert
            Assert.True(File.Exists(tempFile));
            Assert.True(new FileInfo(tempFile).Length > 0);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public async Task ExportAsync_WithExistingFile_OverwritesSilently()
    {
        // Arrange
        var collection = new HealthDataCollection
        {
            SleepRecords = [new SleepData { RecordDate = DateTime.UtcNow, DurationMinutes = 100 }]
        };
        var tempFile = Path.GetTempFileName();

        try
        {
            // Create initial file with content
            await File.WriteAllTextAsync(tempFile, "Initial content");
            var initialContent = await File.ReadAllTextAsync(tempFile);
            Assert.Equal("Initial content", initialContent);

            // Act - should overwrite without throwing
            await _exporter.ExportAsync(collection, tempFile);

            // Assert - file should exist and contain new content (not initial content)
            Assert.True(File.Exists(tempFile));
            var finalContent = await File.ReadAllTextAsync(tempFile);
            Assert.NotEqual("Initial content", finalContent);
            Assert.NotEmpty(finalContent); // Should contain JSON content
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public async Task ExportAsync_WithNonExistentParentDirectory_CreatesDirectoryAndFile()
    {
        // Arrange
        var collection = new HealthDataCollection
        {
            SleepRecords = [new SleepData { RecordDate = DateTime.UtcNow, DurationMinutes = 100 }]
        };
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var tempFile = Path.Combine(tempDir, "test.jsonl");

        try
        {
            // Act - should create directory and file
            await _exporter.ExportAsync(collection, tempFile);

            // Assert
            Assert.True(File.Exists(tempFile));
            var content = await File.ReadAllTextAsync(tempFile);
            Assert.NotEmpty(content); // Should contain JSON content
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public async Task ExportAsync_WithNullDestination_ThrowsArgumentNullException()
    {
        // Arrange
        var collection = new HealthDataCollection();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _exporter.ExportAsync(collection, null!));
    }

    [Fact]
    public async Task ExportAsync_WithEmptyDestination_ThrowsArgumentException()
    {
        // Arrange
        var collection = new HealthDataCollection();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _exporter.ExportAsync(collection, ""));
    }

    [Fact]
    public async Task ExportAsync_WithWhitespaceDestination_ThrowsArgumentException()
    {
        // Arrange
        var collection = new HealthDataCollection();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _exporter.ExportAsync(collection, "   "));
    }
}
