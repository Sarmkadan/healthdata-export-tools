#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Xunit;
using FluentAssertions;
using HealthDataExportTools.Data;
using HealthDataExportTools.Domain.Models;
using HealthDataExportTools.Domain.Enums;

namespace HealthDataExportTools.Tests;

public sealed class InMemoryHealthDataRepositoryTests
{
    private readonly InMemoryHealthDataRepository _sut;

    public InMemoryHealthDataRepositoryTests()
    {
        _sut = new InMemoryHealthDataRepository();
    }

    [Fact]
    public async Task AddAndGetSleepData_ReturnsCorrectData()
    {
        // Arrange
        var sleepData = new SleepData
        {
            Id = Guid.NewGuid().ToString(),
            RecordDate = DateTime.UtcNow.Date.AddHours(1),
            DeviceId = "TestDevice",
            DurationMinutes = 480,
            Quality = SleepQuality.Good,
            DeepSleepMinutes = 100,
            RemSleepMinutes = 80
        };

        // Act
        await _sut.AddSleepAsync(sleepData);
        var retrievedData = await _sut.GetSleepByIdAsync(sleepData.Id);

        // Assert
        retrievedData.Should().NotBeNull();
        retrievedData.Should().BeEquivalentTo(sleepData);
    }

    [Fact]
    public async Task UpdateSleepData_ReflectsChanges()
    {
        // Arrange
        var sleepData = new SleepData
        {
            Id = Guid.NewGuid().ToString(),
            RecordDate = DateTime.UtcNow.Date,
            DeviceId = "TestDevice",
            DurationMinutes = 480,
            Quality = SleepQuality.Good
        };
        await _sut.AddSleepAsync(sleepData);

        sleepData.DurationMinutes = 500;
        sleepData.Quality = SleepQuality.Excellent;

        // Act
        await _sut.UpdateSleepAsync(sleepData);
        var retrievedData = await _sut.GetSleepByIdAsync(sleepData.Id);

        // Assert
        retrievedData.Should().NotBeNull();
        retrievedData!.DurationMinutes.Should().Be(500);
        retrievedData.Quality.Should().Be(SleepQuality.Excellent);
    }

    [Fact]
    public async Task DeleteSleepData_RemovesData()
    {
        // Arrange
        var sleepData = new SleepData
        {
            Id = Guid.NewGuid().ToString(),
            RecordDate = DateTime.UtcNow.Date,
            DeviceId = "TestDevice",
            DurationMinutes = 480
        };
        await _sut.AddSleepAsync(sleepData);

        // Act
        await _sut.DeleteSleepAsync(sleepData.Id);
        var retrievedData = await _sut.GetSleepByIdAsync(sleepData.Id);

        // Assert
        retrievedData.Should().BeNull();
    }

    [Fact]
    public async Task GetSleepRange_ReturnsCorrectData()
    {
        // Arrange
        var date1 = DateTime.UtcNow.Date.AddDays(-2);
        var date2 = DateTime.UtcNow.Date.AddDays(-1);
        var date3 = DateTime.UtcNow.Date;

        var sleep1 = new SleepData { Id = Guid.NewGuid().ToString(), RecordDate = date1 };
        var sleep2 = new SleepData { Id = Guid.NewGuid().ToString(), RecordDate = date2 };
        var sleep3 = new SleepData { Id = Guid.NewGuid().ToString(), RecordDate = date3 };

        await _sut.AddSleepAsync(sleep1);
        await _sut.AddSleepAsync(sleep2);
        await _sut.AddSleepAsync(sleep3);

        // Act
        var results = await _sut.GetSleepRangeAsync(date1, date2);

        // Assert
        results.Should().HaveCount(2);
        results.Should().Contain(s => s.Id == sleep1.Id);
        results.Should().Contain(s => s.Id == sleep2.Id);
        results.Should().NotContain(s => s.Id == sleep3.Id);
    }

    [Fact]
    public async Task AddAndGetHeartRateData_ReturnsCorrectData()
    {
        // Arrange
        var hrData = new HeartRateData
        {
            Id = Guid.NewGuid().ToString(),
            RecordDate = DateTime.UtcNow.Date.AddHours(2),
            DeviceId = "TestDevice",
            AverageBpm = 70,
            MinimumBpm = 60,
            MaximumBpm = 80
        };

        // Act
        await _sut.AddHeartRateAsync(hrData);
        var retrievedData = await _sut.GetHeartRateByIdAsync(hrData.Id);

        // Assert
        retrievedData.Should().NotBeNull();
        retrievedData.Should().BeEquivalentTo(hrData);
    }

    [Fact]
    public async Task GetTotalRecordCount_ReturnsCorrectCount()
    {
        // Arrange
        await _sut.AddSleepAsync(new SleepData { Id = Guid.NewGuid().ToString(), RecordDate = DateTime.UtcNow.Date });
        await _sut.AddHeartRateAsync(new HeartRateData { Id = Guid.NewGuid().ToString(), RecordDate = DateTime.UtcNow.Date });
        await _sut.AddStepsAsync(new StepsData { Id = Guid.NewGuid().ToString(), RecordDate = DateTime.UtcNow.Date });

        // Act
        var count = await _sut.GetTotalRecordCountAsync();

        // Assert
        count.Should().Be(3);
    }

    [Fact]
    public async Task DeleteOldRecords_RemovesRecordsBeforeDate()
    {
        // Arrange
        var oldSleep = new SleepData { Id = Guid.NewGuid().ToString(), RecordDate = DateTime.UtcNow.AddDays(-10) };
        var recentSleep = new SleepData { Id = Guid.NewGuid().ToString(), RecordDate = DateTime.UtcNow.AddDays(-1) };
        await _sut.AddSleepAsync(oldSleep);
        await _sut.AddSleepAsync(recentSleep);

        var cutOffDate = DateTime.UtcNow.AddDays(-5);

        // Act
        await _sut.DeleteOldRecordsAsync(cutOffDate);
        var remainingRecords = await _sut.GetSleepRangeAsync(DateTime.MinValue, DateTime.MaxValue);

        // Assert
        remainingRecords.Should().ContainSingle(s => s.Id == recentSleep.Id);
        remainingRecords.Should().NotContain(s => s.Id == oldSleep.Id);
    }
}
