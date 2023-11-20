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
using NSubstitute;
using Xunit;
using ValidationResult = HealthDataExportTools.DTOs.ValidationResultDto;

namespace HealthDataExportTools.Tests;

/// <summary>
/// Contains unit tests for the <see cref="HealthDataParserService"/> class.
/// Tests various parsing scenarios including JSON parsing, device type detection,
/// and collection merging functionality.
/// </summary>
public sealed class HealthDataParserServiceTests
{
    private readonly HealthDataParserService _parserService;
    private readonly IValidationService _mockValidationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="HealthDataParserServiceTests"/> class.
    /// Sets up mock validation service and creates the parser service instance for testing.
    /// </summary>
    public HealthDataParserServiceTests()
    {
        _mockValidationService = Substitute.For<IValidationService>();
        // Ensure validation always returns true for valid data in these tests
        _mockValidationService.ValidateSleepData(Arg.Any<SleepData>()).Returns(new HealthDataExportTools.Services.ValidationResult());
        _mockValidationService.ValidateHeartRateData(Arg.Any<HeartRateData>()).Returns(new HealthDataExportTools.Services.ValidationResult());
        _mockValidationService.ValidateSpO2Data(Arg.Any<SpO2Data>()).Returns(new HealthDataExportTools.Services.ValidationResult());
        _mockValidationService.ValidateStepsData(Arg.Any<StepsData>()).Returns(new HealthDataExportTools.Services.ValidationResult());
        
        _parserService = new HealthDataParserService(_mockValidationService);
    }

    /// <summary>
    /// Tests that the parser correctly extracts and converts all health data types from JSON.
    /// Verifies that sleep, heart rate, SpO2, and steps data are properly parsed and
    /// converted into their respective domain models.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task ParseJsonAsync_ShouldParseAllHealthDataTypesSuccessfully()
    {
        // Arrange
        var jsonContent = @"
        {
            ""sleep"": [
                {
                    ""recordDate"": ""2024-01-01T00:00:00Z"",
                    ""deviceId"": ""device1"",
                    ""sleepStart"": ""2024-01-01T22:00:00Z"",
                    ""sleepEnd"": ""2024-01-02T06:00:00Z"",
                    ""durationMinutes"": 480,
                    ""deepSleepMinutes"": 90,
                    ""lightSleepMinutes"": 270,
                    ""remSleepMinutes"": 60,
                    ""awakeMinutes"": 60,
                    ""quality"": ""3"",
                    ""score"": 85
                }
            ],
            ""heartRate"": [
                {
                    ""recordDate"": ""2024-01-01T12:00:00Z"",
                    ""deviceId"": ""device1"",
                    ""minimumBpm"": 50,
                    ""maximumBpm"": 120,
                    ""averageBpm"": 70,
                    ""restingBpm"": 60,
                    ""measurementCount"": 100
                }
            ],
            ""spO2"": [
                {
                    ""recordDate"": ""2024-01-01T03:00:00Z"",
                    ""deviceId"": ""device1"",
                    ""minimumPercentage"": 95,
                    ""maximumPercentage"": 99,
                    ""averagePercentage"": 97,
                    ""restingPercentage"": 98,
                    ""measurementCount"": 50
                }
            ],
            ""steps"": [
                {
                    ""recordDate"": ""2024-01-01T23:59:59Z"",
                    ""deviceId"": ""device1"",
                    ""totalSteps"": 10000,
                    ""distanceKm"": 7.5,
                    ""caloriesBurned"": 500,
                    ""dailyGoal"": 10000,
                    ""activeMinutes"": 120
                }
            ]
        }";

        // Act
        var collection = await _parserService.ParseJsonAsync(jsonContent).ConfigureAwait(false);

        // Assert
        collection.Should().NotBeNull();
        collection.SleepRecords.Should().HaveCount(1);
        collection.HeartRateRecords.Should().HaveCount(1);
        collection.SpO2Records.Should().HaveCount(1);
        collection.StepsRecords.Should().HaveCount(1);

        var sleep = collection.SleepRecords.First();
        sleep.DeviceId.Should().Be("device1");
        sleep.DurationMinutes.Should().Be(480);
        sleep.Quality.Should().Be(SleepQuality.Good);

        var hr = collection.HeartRateRecords.First();
        hr.AverageBpm.Should().Be(70);
        hr.RestingBpm.Should().Be(60);

        var spo2 = collection.SpO2Records.First();
        spo2.AveragePercentage.Should().Be(97);
        spo2.LowSpO2Events.Should().Be(0); // Not provided in JSON example, should be 0

        var steps = collection.StepsRecords.First();
        steps.TotalSteps.Should().Be(10000);
        steps.DistanceKm.Should().Be(7.5);
        steps.GoalAchievementPercentage.Should().Be(100);
    }

    /// <summary>
    /// Tests that the parser handles missing optional fields gracefully.
    /// Verifies that when optional fields like Score and RestingBpm are not provided,
    /// they are set to appropriate default values rather than causing parsing failures.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task ParseJsonAsync_ShouldHandleMissingOptionalFields()
    {
        // Arrange
        var jsonContent = @"
        {
            ""sleep"": [
                {
                    ""recordDate"": ""2024-01-01T00:00:00Z"",
                    ""deviceId"": ""device1"",
                    ""sleepStart"": ""2024-01-01T22:00:00Z"",
                    ""sleepEnd"": ""2024-01-02T06:00:00Z"",
                    ""durationMinutes"": 480,
                    ""deepSleepMinutes"": 90,
                    ""lightSleepMinutes"": 270,
                    ""remSleepMinutes"": 60,
                    ""awakeMinutes"": 60
                }
            ],
            ""heartRate"": [
                {
                    ""recordDate"": ""2024-01-01T12:00:00Z"",
                    ""deviceId"": ""device1"",
                    ""minimumBpm"": 50,
                    ""maximumBpm"": 120,
                    ""averageBpm"": 70,
                    ""measurementCount"": 100
                }
            ]
        }";

        // Act
        var collection = await _parserService.ParseJsonAsync(jsonContent).ConfigureAwait(false);

        // Assert
        collection.SleepRecords.Should().HaveCount(1);
        collection.SleepRecords.First().Score.Should().BeNull();
        collection.SleepRecords.First().Quality.Should().Be(SleepQuality.Average); // SleepData.Quality's declared default

        collection.HeartRateRecords.Should().HaveCount(1);
        collection.HeartRateRecords.First().RestingBpm.Should().BeNull();
    }

    /// <summary>
    /// Tests that the parser throws a <see cref="ParsingException"/> when provided with invalid JSON.
    /// Verifies that malformed JSON content results in appropriate exception handling.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task ParseJsonAsync_ShouldThrowParsingExceptionForInvalidJson()
    {
        // Arrange
        var invalidJson = "{ \"sleep\": [ { \"recordDate\": \"invalid-date\" } ] }";

        // Act
        Func<Task> act = async () => await _parserService.ParseJsonAsync(invalidJson).ConfigureAwait(false);

        // Assert
        await act.Should().ThrowAsync<ParsingException>()
            .WithMessage("Failed to parse JSON content*");
    }

    /// <summary>
    /// Tests that the parser returns empty collections when provided with empty arrays.
    /// Verifies that empty arrays in JSON result in empty collections rather than null collections.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task ParseJsonAsync_ShouldReturnEmptyCollectionsForEmptyArrays()
    {
        // Arrange
        var jsonContent = @"
        {
            ""sleep"": [],
            ""heartRate"": [],
            ""spO2"": [],
            ""steps"": []
        }";

        // Act
        var collection = await _parserService.ParseJsonAsync(jsonContent).ConfigureAwait(false);

        // Assert
        collection.Should().NotBeNull();
        collection.SleepRecords.Should().BeEmpty();
        collection.HeartRateRecords.Should().BeEmpty();
        collection.SpO2Records.Should().BeEmpty();
        collection.StepsRecords.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that the device type detection correctly identifies known device types.
    /// Verifies that specific device name patterns map to the correct <see cref="DeviceType"/> enum values.
    /// </summary>
    [Fact]
    public void DetectDeviceType_ShouldReturnCorrectDeviceTypeForKnownKeywords()
    {
        // Arrange
        var service = new HealthDataParserService(_mockValidationService);

        // Act & Assert
        service.DetectDeviceType("my_zepp_watch").Should().Be(DeviceType.Zepp);
        service.DetectDeviceType("AMAZFIT_band").Should().Be(DeviceType.Amazfit);
        service.DetectDeviceType("Garmin_fenix_6").Should().Be(DeviceType.Garmin);
    }

    /// <summary>
    /// Tests that the device type detection returns Unknown for unrecognized device names.
    /// Verifies that device names without known patterns default to <see cref="DeviceType.Unknown"/>.
    /// </summary>
    [Fact]
    public void DetectDeviceType_ShouldReturnUnknownForUnknownKeywords()
    {
        // Arrange
        var service = new HealthDataParserService(_mockValidationService);

        // Act & Assert
        service.DetectDeviceType("apple_watch").Should().Be(DeviceType.Unknown);
        service.DetectDeviceType("fitbit").Should().Be(DeviceType.Unknown);
    }

    /// <summary>
    /// Tests that the device type detection is case-insensitive.
    /// Verifies that device type detection works regardless of the casing in device names.
    /// </summary>
    [Fact]
    public void DetectDeviceType_ShouldBeCaseInsensitive()
    {
        // Arrange
        var service = new HealthDataParserService(_mockValidationService);

        // Act & Assert
        service.DetectDeviceType("My_ZePp_Watch").Should().Be(DeviceType.Zepp);
        service.DetectDeviceType("amazFIt").Should().Be(DeviceType.Amazfit);
    }

    /// <summary>
    /// Tests that the collection merging combines all records from multiple collections.
    /// Verifies that merging multiple <see cref="HealthDataCollection"/> instances correctly combines
    /// sleep, heart rate, SpO2, and steps records into a single collection.
    /// </summary>
    [Fact]
    public void MergeCollections_ShouldCombineAllRecordsFromMultipleCollections()
    {
        // Arrange
        var collection1 = new HealthDataCollection
        {
            SleepRecords = { new SleepData { DeviceId = "d1" } },
            HeartRateRecords = { new HeartRateData { DeviceId = "d1" }, new HeartRateData { DeviceId = "d1" } }
        };

        var collection2 = new HealthDataCollection
        {
            SleepRecords = { new SleepData { DeviceId = "d2" } },
            StepsRecords = { new StepsData { DeviceId = "d2" } }
        };

        var collection3 = new HealthDataCollection
        {
            SpO2Records = { new SpO2Data { DeviceId = "d3" } }
        };

        // Act
        var mergedCollection = _parserService.MergeCollections(collection1, collection2, collection3);

        // Assert
        mergedCollection.SleepRecords.Should().HaveCount(2);
        mergedCollection.HeartRateRecords.Should().HaveCount(2);
        mergedCollection.SpO2Records.Should().HaveCount(1);
        mergedCollection.StepsRecords.Should().HaveCount(1);
        mergedCollection.ActivityRecords.Should().BeEmpty(); // No activity records added
    }

    /// <summary>
    /// Tests that the collection merging handles empty collections gracefully.
    /// Verifies that merging with empty collections does not cause errors and maintains
    /// the existing records in the merged result.
    /// </summary>
    [Fact]
    public void MergeCollections_ShouldHandleEmptyCollectionsGracefully()
    {
        // Arrange
        var collection1 = new HealthDataCollection
        {
            SleepRecords = { new SleepData { DeviceId = "d1" } }
        };
        var emptyCollection = new HealthDataCollection();

        // Act
        var mergedCollection = _parserService.MergeCollections(collection1, emptyCollection);

        // Assert
        mergedCollection.SleepRecords.Should().HaveCount(1);
        mergedCollection.HeartRateRecords.Should().BeEmpty();
    }
}