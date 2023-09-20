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

/// <summary>
/// Tests for the InMemoryHealthDataRepository class.
/// </summary>
public sealed class InMemoryHealthDataRepositoryTests
{
    private readonly InMemoryHealthDataRepository _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryHealthDataRepositoryTests"/> class.
    /// </summary>
    public InMemoryHealthDataRepositoryTests()
    {
        _sut = new InMemoryHealthDataRepository();
    }

    [Fact]
    public async Task AddAndGetSleepData_ReturnsCorrectData()
    {
        /// <summary>
        /// Tests that adding and retrieving sleep data returns the correct data.
        /// </summary>
        /// <param name="sleepData">The sleep data to add.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
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
        await _sut.AddSleepAsync(sleepData).ConfigureAwait(false);
        var retrievedData = await _sut.GetSleepByIdAsync(sleepData.Id).ConfigureAwait(false);

        // Assert
        retrievedData.Should().NotBeNull();
        retrievedData.Should().BeEquivalentTo(sleepData);
    }

    [Fact]
    public async Task UpdateSleepData_ReflectsChanges()
    {
        /// <summary>
        /// Tests that updating sleep data reflects the changes.
        /// </summary>
        /// <param name="sleepData">The sleep data to add and update.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        // Arrange
        var sleepData = new SleepData
        {
            Id = Guid.NewGuid().ToString(),
            RecordDate = DateTime.UtcNow.Date,
            DeviceId = "TestDevice",
            DurationMinutes = 480,
            Quality = SleepQuality.Good
        };
        await _sut.AddSleepAsync(sleepData).ConfigureAwait(false);

        sleepData.DurationMinutes = 500;
        sleepData.Quality = SleepQuality.Excellent;

        // Act
        await _sut.UpdateSleepAsync(sleepData).ConfigureAwait(false);
        var retrievedData = await _sut.GetSleepByIdAsync(sleepData.Id).ConfigureAwait(false);

        // Assert
        retrievedData.Should().NotBeNull();
        retrievedData!.DurationMinutes.Should().Be(500);
        retrievedData.Quality.Should().Be(SleepQuality.Excellent);
    }

    [Fact]
    public async Task DeleteSleepData_RemovesData()
    {
        /// <summary>
        /// Tests that deleting sleep data removes the data.
        /// </summary>
        /// <param name="sleepData">The sleep data to add and delete.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        // Arrange
        var sleepData = new SleepData
        {
            Id = Guid.NewGuid().ToString(),
            RecordDate = DateTime.UtcNow.Date,
            DeviceId = "TestDevice",
            DurationMinutes = 480
        };
        await _sut.AddSleepAsync(sleepData).ConfigureAwait(false);

        // Act
        await _sut.DeleteSleepAsync(sleepData.Id).ConfigureAwait(false);
        var retrievedData = await _sut.GetSleepByIdAsync(sleepData.Id).ConfigureAwait(false);

        // Assert
        retrievedData.Should().BeNull();
    }

    [Fact]
    public async Task GetSleepRange_ReturnsCorrectData()
    {
        /// <summary>
        /// Tests that getting sleep data within a range returns the correct data.
        /// </summary>
        /// <param name="date1">The start date of the range.</param>
        /// <param name="date2">The end date of the range.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        // Arrange
        var date1 = DateTime.UtcNow.Date.AddDays(-2);
        var date2 = DateTime.UtcNow.Date.AddDays(-1);
        var date3 = DateTime.UtcNow.Date;

        var sleep1 = new SleepData { Id = Guid.NewGuid().ToString(), RecordDate = date1 };
        var sleep2 = new SleepData { Id = Guid.NewGuid().ToString(), RecordDate = date2 };
        var sleep3 = new SleepData { Id = Guid.NewGuid().ToString(), RecordDate = date3 };

        await _sut.AddSleepAsync(sleep1).ConfigureAwait(false);
        await _sut.AddSleepAsync(sleep2).ConfigureAwait(false);
        await _sut.AddSleepAsync(sleep3).ConfigureAwait(false);

        // Act
        var results = await _sut.GetSleepRangeAsync(date1, date2).ConfigureAwait(false);

        // Assert
        results.Should().HaveCount(2);
        results.Should().Contain(s => s.Id == sleep1.Id);
        results.Should().Contain(s => s.Id == sleep2.Id);
        results.Should().NotContain(s => s.Id == sleep3.Id);
    }

    [Fact]
    public async Task AddAndGetHeartRateData_ReturnsCorrectData()
    {
        /// <summary>
        /// Tests that adding and retrieving heart rate data returns the correct data.
        /// </summary>
        /// <param name="hrData">The heart rate data to add.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
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
        await _sut.AddHeartRateAsync(hrData).ConfigureAwait(false);
        var retrievedData = await _sut.GetHeartRateByIdAsync(hrData.Id).ConfigureAwait(false);

        // Assert
        retrievedData.Should().NotBeNull();
        retrievedData.Should().BeEquivalentTo(hrData);
    }

    [Fact]
    public async Task GetTotalRecordCount_ReturnsCorrectCount()
    {
        /// <summary>
        /// Tests that getting the total record count returns the correct count.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        // Arrange
        await _sut.AddSleepAsync(new SleepData { Id = Guid.NewGuid().ToString(), RecordDate = DateTime.UtcNow.Date }).ConfigureAwait(false);
        await _sut.AddHeartRateAsync(new HeartRateData { Id = Guid.NewGuid().ToString(), RecordDate = DateTime.UtcNow.Date }).ConfigureAwait(false);
        await _sut.AddStepsAsync(new StepsData { Id = Guid.NewGuid().ToString(), RecordDate = DateTime.UtcNow.Date }).ConfigureAwait(false);

        // Act
        var count = await _sut.GetTotalRecordCountAsync().ConfigureAwait(false);

        // Assert
        count.Should().Be(3);
    }

    [Fact]
    public async Task DeleteOldRecords_RemovesRecordsBeforeDate()
    {
        /// <summary>
        /// Tests that deleting old records removes records before the specified date.
        /// </summary>
        /// <param name="cutOffDate">The date before which records should be deleted.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        // Arrange
        var oldSleep = new SleepData { Id = Guid.NewGuid().ToString(), RecordDate = DateTime.UtcNow.AddDays(-10) };
        var recentSleep = new SleepData { Id = Guid.NewGuid().ToString(), RecordDate = DateTime.UtcNow.AddDays(-1) };
        await _sut.AddSleepAsync(oldSleep).ConfigureAwait(false);
        await _sut.AddSleepAsync(recentSleep).ConfigureAwait(false);

        var cutOffDate = DateTime.UtcNow.AddDays(-5);

        // Act
        await _sut.DeleteOldRecordsAsync(cutOffDate).ConfigureAwait(false);
        var remainingRecords = await _sut.GetSleepRangeAsync(DateTime.MinValue, DateTime.MaxValue).ConfigureAwait(false);

        // Assert
        remainingRecords.Should().ContainSingle(s => s.Id == recentSleep.Id);
        remainingRecords.Should().NotContain(s => s.Id == oldSleep.Id);
    }
}
