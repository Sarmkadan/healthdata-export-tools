#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using HealthDataExportTools.Domain.Models;

namespace HealthDataExportTools.Data;

/// <summary>
/// Extension methods for InMemoryHealthDataRepository providing additional utility functionality
/// </summary>
public static class InMemoryHealthDataRepositoryExtensions
{
    /// <summary>
    /// Gets the most recent sleep record by date
    /// </summary>
    /// <param name="repository">The repository instance</param>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is null</exception>
    /// <param name="date">The date to search</param>
    /// <returns>The most recent sleep record for the specified date, or null if not found</returns>
    public static async Task<SleepData?> GetMostRecentSleepAsync(this InMemoryHealthDataRepository repository, DateTime date)
    {
        ArgumentNullException.ThrowIfNull(repository);

        var records = await repository.GetSleepByDateAsync(date);
        return records.OrderByDescending(r => r.RecordDate).FirstOrDefault();
    }

    /// <summary>
    /// Gets the most recent heart rate record by date
    /// </summary>
    /// <param name="repository">The repository instance</param>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is null</exception>
    /// <param name="date">The date to search</param>
    /// <returns>The most recent heart rate record for the specified date, or null if not found</returns>
    public static async Task<HeartRateData?> GetMostRecentHeartRateAsync(this InMemoryHealthDataRepository repository, DateTime date)
    {
        ArgumentNullException.ThrowIfNull(repository);

        var record = await repository.GetHeartRateByDateAsync(date);
        return record;
    }

    /// <summary>
    /// Gets the most recent SpO2 record by date
    /// </summary>
    /// <param name="repository">The repository instance</param>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is null</exception>
    /// <param name="date">The date to search</param>
    /// <returns>The most recent SpO2 record for the specified date, or null if not found</returns>
    public static async Task<SpO2Data?> GetMostRecentSpO2Async(this InMemoryHealthDataRepository repository, DateTime date)
    {
        ArgumentNullException.ThrowIfNull(repository);

        var record = await repository.GetSpO2ByDateAsync(date);
        return record;
    }

    /// <summary>
    /// Gets the most recent steps record by date
    /// </summary>
    /// <param name="repository">The repository instance</param>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is null</exception>
    /// <param name="date">The date to search</param>
    /// <returns>The most recent steps record for the specified date, or null if not found</returns>
    public static async Task<StepsData?> GetMostRecentStepsAsync(this InMemoryHealthDataRepository repository, DateTime date)
    {
        ArgumentNullException.ThrowIfNull(repository);

        var record = await repository.GetStepsByDateAsync(date);
        return record;
    }

    /// <summary>
    /// Gets average heart rate for a specific date
    /// </summary>
    /// <param name="repository">The repository instance</param>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is null</exception>
    /// <param name="date">The date to calculate average for</param>
    /// <returns>Average heart rate in beats per minute, or null if no data exists</returns>
    public static async Task<int?> GetAverageHeartRateAsync(this InMemoryHealthDataRepository repository, DateTime date)
    {
        ArgumentNullException.ThrowIfNull(repository);

        var records = await repository.GetHeartRateRangeAsync(date.Date, date.Date.AddDays(1).AddTicks(-1));

        return records.Count == 0
            ? null
            : (int)Math.Round(records.Average(r => r.AverageBpm));
    }

    /// <summary>
    /// Gets total steps for a specific date
    /// </summary>
    /// <param name="repository">The repository instance</param>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is null</exception>
    /// <param name="date">The date to calculate total for</param>
    /// <returns>Total steps for the date, or null if no data exists</returns>
    public static async Task<int?> GetTotalStepsAsync(this InMemoryHealthDataRepository repository, DateTime date)
    {
        ArgumentNullException.ThrowIfNull(repository);

        var record = await repository.GetStepsByDateAsync(date);
        return record?.TotalSteps;
    }

    /// <summary>
    /// Gets average SpO2 percentage for a specific date
    /// </summary>
    /// <param name="repository">The repository instance</param>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is null</exception>
    /// <param name="date">The date to calculate average for</param>
    /// <returns>Average SpO2 percentage, or null if no data exists</returns>
    public static async Task<int?> GetAverageSpO2Async(this InMemoryHealthDataRepository repository, DateTime date)
    {
        ArgumentNullException.ThrowIfNull(repository);

        var records = await repository.GetSpO2RangeAsync(date.Date, date.Date.AddDays(1).AddTicks(-1));

        return records.Count == 0
            ? null
            : (int)Math.Round(records.Average(r => r.AveragePercentage));
    }

    /// <summary>
    /// Checks if any health data exists for a specific date across all data types
    /// </summary>
    /// <param name="repository">The repository instance</param>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is null</exception>
    /// <param name="date">The date to check</param>
    /// <returns>True if any data exists for the date, false otherwise</returns>
    public static async Task<bool> HasDataForDateAsync(this InMemoryHealthDataRepository repository, DateTime date)
    {
        ArgumentNullException.ThrowIfNull(repository);

        return await repository.AnyRecordsExistAsync(date);
    }

    /// <summary>
    /// Gets the latest record across all data types for a specific date
    /// </summary>
    /// <param name="repository">The repository instance</param>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is null</exception>
    /// <param name="date">The date to retrieve the latest record for</param>
    /// <returns>Tuple containing the data type and record, or null if no data exists</returns>
    public static async Task<(HealthDataType DataType, object? Record)?> GetLatestRecordAsync(this InMemoryHealthDataRepository repository, DateTime date)
    {
        ArgumentNullException.ThrowIfNull(repository);

        var hasData = await repository.HasDataForDateAsync(date);

        if (!hasData)
        {
            return null;
        }

        // Check each data type for the most recent record
        var sleepTask = repository.GetMostRecentSleepAsync(date);
        var heartRateTask = repository.GetMostRecentHeartRateAsync(date);
        var spo2Task = repository.GetMostRecentSpO2Async(date);
        var stepsTask = repository.GetMostRecentStepsAsync(date);

        await Task.WhenAll(sleepTask, heartRateTask, spo2Task, stepsTask);

        var sleepRecord = await sleepTask;
        var heartRateRecord = await heartRateTask;
        var spo2Record = await spo2Task;
        var stepsRecord = await stepsTask;

        // Find the record with the latest timestamp
        var candidates = new List<(HealthDataType, DateTime, object?)>
        {
            (HealthDataType.Sleep, sleepRecord?.RecordDate ?? DateTime.MinValue, sleepRecord),
            (HealthDataType.HeartRate, heartRateRecord?.RecordDate ?? DateTime.MinValue, heartRateRecord),
            (HealthDataType.SpO2, spo2Record?.RecordDate ?? DateTime.MinValue, spo2Record),
            (HealthDataType.Steps, stepsRecord?.RecordDate ?? DateTime.MinValue, stepsRecord)
        };

        var latest = candidates
            .Where(c => c.Item3 != null)
            .OrderByDescending(c => c.Item2)
            .FirstOrDefault();

        return latest != default
            ? (latest.Item1, latest.Item3)
            : null;
    }

    /// <summary>
    /// Gets all records for a specific date grouped by data type
    /// </summary>
    /// <param name="repository">The repository instance</param>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is null</exception>
    /// <param name="date">The date to retrieve records for</param>
    /// <returns>Dictionary mapping data types to their records for the specified date</returns>
    public static async Task<Dictionary<HealthDataType, List<object>>> GetRecordsByDateGroupedAsync(
        this InMemoryHealthDataRepository repository,
        DateTime date)
    {
        ArgumentNullException.ThrowIfNull(repository);

        var result = new Dictionary<HealthDataType, List<object>>
        {
            { HealthDataType.Sleep, [] },
            { HealthDataType.HeartRate, [] },
            { HealthDataType.SpO2, [] },
            { HealthDataType.Steps, [] }
        };

        var sleepRecords = await repository.GetSleepByDateAsync(date);
        var heartRateRecords = await repository.GetHeartRateRangeAsync(date.Date, date.Date.AddDays(1).AddTicks(-1));
        var spo2Records = await repository.GetSpO2RangeAsync(date.Date, date.Date.AddDays(1).AddTicks(-1));
        var stepsRecords = await repository.GetStepsRangeAsync(date.Date, date.Date.AddDays(1).AddTicks(-1));

        result[HealthDataType.Sleep].AddRange(sleepRecords.Cast<object>());
        result[HealthDataType.HeartRate].AddRange(heartRateRecords.Cast<object>());
        result[HealthDataType.SpO2].AddRange(spo2Records.Cast<object>());
        result[HealthDataType.Steps].AddRange(stepsRecords.Cast<object>());

        return result;
    }
}

/// <summary>
/// Enum representing health data types
/// </summary>
public enum HealthDataType
{
    Sleep,
    HeartRate,
    SpO2,
    Steps
}