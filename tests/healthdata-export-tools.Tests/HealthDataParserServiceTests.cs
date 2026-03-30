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

public sealed class HealthDataParserServiceTests
{
    private readonly HealthDataParserService _parserService;
    private readonly IValidationService _mockValidationService;

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
                    ""quality"": ""1"",
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
        collection.SleepRecords.First().Quality.Should().Be(SleepQuality.VeryPoor); // Default enum value

        collection.HeartRateRecords.Should().HaveCount(1);
        collection.HeartRateRecords.First().RestingBpm.Should().BeNull();
    }

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

    [Fact]
    public void DetectDeviceType_ShouldReturnUnknownForUnknownKeywords()
    {
        // Arrange
        var service = new HealthDataParserService(_mockValidationService);

        // Act & Assert
        service.DetectDeviceType("apple_watch").Should().Be(DeviceType.Unknown);
        service.DetectDeviceType("fitbit").Should().Be(DeviceType.Unknown);
    }

    [Fact]
    public void DetectDeviceType_ShouldBeCaseInsensitive()
    {
        // Arrange
        var service = new HealthDataParserService(_mockValidationService);

        // Act & Assert
        service.DetectDeviceType("My_ZePp_Watch").Should().Be(DeviceType.Zepp);
        service.DetectDeviceType("amazFIt").Should().Be(DeviceType.Amazfit);
    }

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