#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using HealthDataExportTools.Domain.Enums;
using HealthDataExportTools.Domain.Models;
using HealthDataExportTools.Formatters;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace HealthDataExportTools.Tests;

/// <summary>
/// Tests for CSV injection protection in CsvFormatter.
/// Verifies that dangerous characters and sequences are properly sanitized.
/// </summary>
public sealed class CsvInjectionTests : IDisposable
{
    private readonly CsvFormatter _formatter;
    private readonly Mock<ILogger<CsvFormatter>> _loggerMock;
    private readonly string _tempDir;

    public CsvInjectionTests()
    {
        _loggerMock = new Mock<ILogger<CsvFormatter>>();
        _formatter = new CsvFormatter(_loggerMock.Object);
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
    public async Task FormatAsync_ExcelFormulaInjection_ShouldBePrefixedWithQuote()
    {
        // Arrange
        var record = new SleepData
        {
            RecordDate = new DateTime(2024, 1, 1, 22, 0, 0, DateTimeKind.Utc),
            DurationMinutes = 480,
            Quality = SleepQuality.Good,
            DeviceId = "=1+1",
            Notes = "Test note"
        };

        // Act
        var csv = await _formatter.FormatAsync(record);

        // Assert
        Assert.Contains("'=1+1", csv);
        Assert.DoesNotContain("=1+1,", csv);
        Assert.DoesNotContain("=1+1\"", csv);
    }

    [Fact]
    public async Task FormatAsync_DdeInjection_ShouldBePrefixedWithQuote()
    {
        // Arrange
        var record = new HeartRateData
        {
            RecordDate = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            AverageBpm = 70,
            DeviceId = "@SUM(A1:A10)",
            Notes = "Device note"
        };

        // Act
        var csv = await _formatter.FormatAsync(record);

        // Assert
        Assert.Contains("'@SUM(A1:A10)", csv);
        Assert.DoesNotContain("@SUM(A1:A10,", csv);
    }

    [Fact]
    public async Task FormatAsync_PositiveFormulaInjection_ShouldBePrefixedWithQuote()
    {
        // Arrange
        var record = new SpO2Data
        {
            RecordDate = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            AveragePercentage = 98,
            DeviceId = "+100",
            Notes = "Test"
        };

        // Act
        var csv = await _formatter.FormatAsync(record);

        // Assert
        Assert.Contains("'+100", csv);
        Assert.DoesNotContain("+100,", csv);
    }

    [Fact]
    public async Task FormatAsync_NegativeFormulaInjection_ShouldBePrefixedWithQuote()
    {
        // Arrange
        var record = new StepsData
        {
            RecordDate = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            TotalSteps = 5000,
            DeviceId = "-500",
            Notes = "Test"
        };

        // Act
        var csv = await _formatter.FormatAsync(record);

        // Assert
        Assert.Contains("'-500", csv);
        Assert.DoesNotContain("-500,", csv);
    }

    [Fact]
    public async Task FormatAsync_TabCharacterInjection_ShouldBePrefixedWithQuote()
    {
        // Arrange
        var record = new ActivityData
        {
            RecordDate = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            DeviceId = "\t=1+1",
            Notes = "Test"
        };

        // Act
        var csv = await _formatter.FormatAsync(record);

        // Assert
        Assert.Contains("'\t=1+1", csv);
        Assert.DoesNotContain("\t=1+1,", csv);
    }

    [Fact]
    public async Task FormatAsync_CarriageReturnInjection_ShouldBePrefixedWithQuote()
    {
        // Arrange
        var record = new SleepData
        {
            RecordDate = new DateTime(2024, 1, 1, 22, 0, 0, DateTimeKind.Utc),
            DurationMinutes = 480,
            Quality = SleepQuality.Good,
            DeviceId = "\r=1+1",
            Notes = "Test note"
        };

        // Act
        var csv = await _formatter.FormatAsync(record);

        // Assert
        Assert.Contains("'\r=1+1", csv);
        Assert.DoesNotContain("\r=1+1,", csv);
    }

    [Fact]
    public async Task FormatAsync_EmbeddedCrlf_ShouldBeReplacedWithSpaces()
    {
        // Arrange
        var record = new HeartRateData
        {
            RecordDate = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            AverageBpm = 70,
            DeviceId = "Line1\r\nLine2",
            Notes = "Multi\r\nline note"
        };

        // Act
        var csv = await _formatter.FormatAsync(record);

        // Assert - CRLF should be replaced with spaces
        Assert.Contains("Line1 Line2", csv);
        Assert.DoesNotContain("Line1\r\nLine2", csv);
        Assert.DoesNotContain("Line1\nLine2", csv);
    }

    [Fact]
    public async Task FormatAsync_EmbeddedLf_ShouldBeReplacedWithSpaces()
    {
        // Arrange
        var record = new SpO2Data
        {
            RecordDate = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            AveragePercentage = 98,
            DeviceId = "Line1\nLine2",
            Notes = "Multi\nline note"
        };

        // Act
        var csv = await _formatter.FormatAsync(record);

        // Assert
        Assert.Contains("Line1 Line2", csv);
        Assert.DoesNotContain("Line1\nLine2", csv);
    }

    [Fact]
    public async Task FormatAsync_EmbeddedCr_ShouldBeReplacedWithSpaces()
    {
        // Arrange
        var record = new StepsData
        {
            RecordDate = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            TotalSteps = 5000,
            DeviceId = "Line1\rLine2",
            Notes = "Multi\rline note"
        };

        // Act
        var csv = await _formatter.FormatAsync(record);

        // Assert
        Assert.Contains("Line1 Line2", csv);
        Assert.DoesNotContain("Line1\rLine2", csv);
    }

    [Fact]
    public async Task FormatAsync_EmbeddedDoubleQuotes_ShouldBeEscaped()
    {
        // Arrange
        var record = new ActivityData
        {
            RecordDate = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            DeviceId = "He said \"Hello\"",
            Notes = "Test with \"quotes\""
        };

        // Act
        var csv = await _formatter.FormatAsync(record);

        // Assert - double quotes should be escaped per RFC 4180
        Assert.Contains("\"\"", csv); // Should contain escaped quotes
        Assert.DoesNotContain("\"He said", csv); // Should not have unescaped quote before text
    }

    [Fact]
    public async Task FormatAsync_NormalText_ShouldPassThroughUnchanged()
    {
        // Arrange
        var record = new SleepData
        {
            RecordDate = new DateTime(2024, 1, 1, 22, 0, 0, DateTimeKind.Utc),
            DurationMinutes = 480,
            Quality = SleepQuality.Good,
            DeviceId = "NormalDevice123",
            Notes = "This is a normal health data note"
        };

        // Act
        var csv = await _formatter.FormatAsync(record);

        // Assert
        Assert.Contains("NormalDevice123", csv);
        Assert.Contains("This is a normal health data note", csv);
    }

    [Fact]
    public async Task FormatAsync_NullOrEmptyValues_ShouldBeHandled()
    {
        // Arrange
        var record = new HeartRateData
        {
            RecordDate = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            AverageBpm = 70,
            DeviceId = null,
            Notes = string.Empty
        };

        // Act
        var csv = await _formatter.FormatAsync(record);

        // Assert - should not throw and should contain empty fields
        Assert.NotNull(csv);
        Assert.NotEmpty(csv);
    }

    [Fact]
    public async Task FormatCollectionAsync_AllRecords_ShouldSanitizeAllFields()
    {
        // Arrange
        var records = new List<SleepData>
        {
            new SleepData
            {
                RecordDate = new DateTime(2024, 1, 1, 22, 0, 0, DateTimeKind.Utc),
                DurationMinutes = 480,
                Quality = SleepQuality.Good,
                DeviceId = "=1+1",
                Notes = "Test note with\r\nnewline"
            },
            new SleepData
            {
                RecordDate = new DateTime(2024, 1, 2, 22, 0, 0, DateTimeKind.Utc),
                DurationMinutes = 420,
                Quality = SleepQuality.Fair,
                DeviceId = "@SUM(A1:A10)",
                Notes = "Another test"
            }
        };

        // Act
        var csv = await _formatter.FormatCollectionAsync(records);

        // Assert
        Assert.Contains("'=1+1", csv);
        Assert.Contains("'@SUM(A1:A10)", csv);
        Assert.Contains("Test note with newline", csv);
        Assert.DoesNotContain("=1+1,", csv);
        Assert.DoesNotContain("@SUM(A1:A10,", csv);
        Assert.DoesNotContain("\r\n", csv);
    }

    [Fact]
    public async Task FormatSleepDataAsync_ShouldSanitizeDeviceId()
    {
        // Arrange
        var records = new List<SleepData>
        {
            new SleepData
            {
                RecordDate = new DateTime(2024, 1, 1, 22, 0, 0, DateTimeKind.Utc),
                DurationMinutes = 480,
                Quality = SleepQuality.Good,
                DeviceId = "=DANGEROUS"
            }
        };

        // Act
        var csv = await _formatter.FormatSleepDataAsync(records);

        // Assert
        Assert.Contains("'=DANGEROUS", csv);
        Assert.DoesNotContain("=DANGEROUS,", csv);
    }

    [Fact]
    public async Task FormatHeartRateDataAsync_ShouldSanitizeDeviceId()
    {
        // Arrange
        var records = new List<HeartRateData>
        {
            new HeartRateData
            {
                RecordDate = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc),
                AverageBpm = 70,
                DeviceId = "+1000"
            }
        };

        // Act
        var csv = await _formatter.FormatHeartRateDataAsync(records);

        // Assert
        Assert.Contains("'+1000", csv);
        Assert.DoesNotContain("+1000,", csv);
    }

    [Fact]
    public async Task FormatSpO2DataAsync_ShouldSanitizeDeviceId()
    {
        // Arrange
        var records = new List<SpO2Data>
        {
            new SpO2Data
            {
                RecordDate = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc),
                AveragePercentage = 98,
                DeviceId = "@EVIL"
            }
        };

        // Act
        var csv = await _formatter.FormatSpO2DataAsync(records);

        // Assert
        Assert.Contains("'@EVIL", csv);
        Assert.DoesNotContain("@EVIL,", csv);
    }

    [Fact]
    public async Task FormatStepsDataAsync_ShouldSanitizeDeviceId()
    {
        // Arrange
        var records = new List<StepsData>
        {
            new StepsData
            {
                RecordDate = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc),
                TotalSteps = 5000,
                DeviceId = "-5000"
            }
        };

        // Act
        var csv = await _formatter.FormatStepsDataAsync(records);

        // Assert
        Assert.Contains("'-5000", csv);
        Assert.DoesNotContain("-5000,", csv);
    }

    [Fact]
    public async Task FormatAsync_NotesFieldWithDangerousContent_ShouldBeSanitized()
    {
        // Arrange - Notes field is user-provided free text
        var record = new SleepData
        {
            RecordDate = new DateTime(2024, 1, 1, 22, 0, 0, DateTimeKind.Utc),
            DurationMinutes = 480,
            Quality = SleepQuality.Good,
            DeviceId = "NormalDevice",
            Notes = "=1+1 This is a malicious formula in notes field"
        };

        // Act
        var csv = await _formatter.FormatAsync(record);

        // Assert - Notes field should be sanitized
        Assert.Contains("NormalDevice", csv);
        Assert.Contains("'=1+1 This is a malicious formula in notes field", csv);
    }
}