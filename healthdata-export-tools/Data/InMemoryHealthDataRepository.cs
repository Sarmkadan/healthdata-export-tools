// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using HealthDataExportTools.Domain.Models;

namespace HealthDataExportTools.Data;

/// <summary>
/// In-memory implementation of health data repository for testing
/// </summary>
public class InMemoryHealthDataRepository : IHealthDataRepository
{
    // Fix: Use ConcurrentDictionary instead of Dictionary to resolve thread safety issues
    private readonly ConcurrentDictionary<string, SleepData> _sleepData = [];
    private readonly ConcurrentDictionary<string, HeartRateData> _heartRateData = [];
    private readonly ConcurrentDictionary<string, SpO2Data> _spO2Data = [];
    private readonly ConcurrentDictionary<string, StepsData> _stepsData = [];
    private readonly ConcurrentDictionary<string, ActivityData> _activityData = [];
    private readonly ConcurrentDictionary<string, HealthMetric> _metrics = [];

    // Sleep Data Operations
    public Task<SleepData?> GetSleepByIdAsync(string id)
    {
        _sleepData.TryGetValue(id, out var data);
        return Task.FromResult(data);
    }

    public Task<List<SleepData>> GetSleepByDateAsync(DateTime date)
    {
        var result = _sleepData.Values.Where(x => x.RecordDate.Date == date.Date).ToList();
        return Task.FromResult(result);
    }

    public Task<List<SleepData>> GetSleepRangeAsync(DateTime startDate, DateTime endDate)
    {
        var result = _sleepData.Values
            .Where(x => x.RecordDate.Date >= startDate.Date && x.RecordDate.Date <= endDate.Date)
            .ToList();
        return Task.FromResult(result);
    }

    public Task AddSleepAsync(SleepData data)
    {
        _sleepData[data.Id] = data;
        return Task.CompletedTask;
    }

    public Task UpdateSleepAsync(SleepData data)
    {
        _sleepData[data.Id] = data;
        return Task.CompletedTask;
    }

    public Task DeleteSleepAsync(string id)
    {
        _sleepData.TryRemove(id, out _);
        return Task.CompletedTask;
    }

    // Heart Rate Operations
    public Task<HeartRateData?> GetHeartRateByIdAsync(string id)
    {
        _heartRateData.TryGetValue(id, out var data);
        return Task.FromResult(data);
    }

    public Task<HeartRateData?> GetHeartRateByDateAsync(DateTime date)
    {
        var result = _heartRateData.Values.FirstOrDefault(x => x.RecordDate.Date == date.Date);
        return Task.FromResult(result);
    }

    public Task<List<HeartRateData>> GetHeartRateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var result = _heartRateData.Values
            .Where(x => x.RecordDate.Date >= startDate.Date && x.RecordDate.Date <= endDate.Date)
            .ToList();
        return Task.FromResult(result);
    }

    public Task AddHeartRateAsync(HeartRateData data)
    {
        _heartRateData[data.Id] = data;
        return Task.CompletedTask;
    }

    public Task UpdateHeartRateAsync(HeartRateData data)
    {
        _heartRateData[data.Id] = data;
        return Task.CompletedTask;
    }

    public Task DeleteHeartRateAsync(string id)
    {
        _heartRateData.TryRemove(id, out _);
        return Task.CompletedTask;
    }

    // SpO2 Operations
    public Task<SpO2Data?> GetSpO2ByIdAsync(string id)
    {
        _spO2Data.TryGetValue(id, out var data);
        return Task.FromResult(data);
    }

    public Task<SpO2Data?> GetSpO2ByDateAsync(DateTime date)
    {
        var result = _spO2Data.Values.FirstOrDefault(x => x.RecordDate.Date == date.Date);
        return Task.FromResult(result);
    }

    public Task<List<SpO2Data>> GetSpO2RangeAsync(DateTime startDate, DateTime endDate)
    {
        var result = _spO2Data.Values
            .Where(x => x.RecordDate.Date >= startDate.Date && x.RecordDate.Date <= endDate.Date)
            .ToList();
        return Task.FromResult(result);
    }

    public Task AddSpO2Async(SpO2Data data)
    {
        _spO2Data[data.Id] = data;
        return Task.CompletedTask;
    }

    public Task UpdateSpO2Async(SpO2Data data)
    {
        _spO2Data[data.Id] = data;
        return Task.CompletedTask;
    }

    public Task DeleteSpO2Async(string id)
    {
        _spO2Data.TryRemove(id, out _);
        return Task.CompletedTask;
    }

    // Steps Operations
    public Task<StepsData?> GetStepsByIdAsync(string id)
    {
        _stepsData.TryGetValue(id, out var data);
        return Task.FromResult(data);
    }

    public Task<StepsData?> GetStepsByDateAsync(DateTime date)
    {
        var result = _stepsData.Values.FirstOrDefault(x => x.RecordDate.Date == date.Date);
        return Task.FromResult(result);
    }

    public Task<List<StepsData>> GetStepsRangeAsync(DateTime startDate, DateTime endDate)
    {
        var result = _stepsData.Values
            .Where(x => x.RecordDate.Date >= startDate.Date && x.RecordDate.Date <= endDate.Date)
            .ToList();
        return Task.FromResult(result);
    }

    public Task AddStepsAsync(StepsData data)
    {
        _stepsData[data.Id] = data;
        return Task.CompletedTask;
    }

    public Task UpdateStepsAsync(StepsData data)
    {
        _stepsData[data.Id] = data;
        return Task.CompletedTask;
    }

    public Task DeleteStepsAsync(string id)
    {
        _stepsData.TryRemove(id, out _);
        return Task.CompletedTask;
    }

    // Activity Operations
    public Task<ActivityData?> GetActivityByIdAsync(string id)
    {
        _activityData.TryGetValue(id, out var data);
        return Task.FromResult(data);
    }

    public Task<List<ActivityData>> GetActivitiesByDateAsync(DateTime date)
    {
        var result = _activityData.Values.Where(x => x.RecordDate.Date == date.Date).ToList();
        return Task.FromResult(result);
    }

    public Task<List<ActivityData>> GetActivitiesRangeAsync(DateTime startDate, DateTime endDate)
    {
        var result = _activityData.Values
            .Where(x => x.RecordDate.Date >= startDate.Date && x.RecordDate.Date <= endDate.Date)
            .ToList();
        return Task.FromResult(result);
    }

    public Task AddActivityAsync(ActivityData data)
    {
        _activityData[data.Id] = data;
        return Task.CompletedTask;
    }

    public Task UpdateActivityAsync(ActivityData data)
    {
        _activityData[data.Id] = data;
        return Task.CompletedTask;
    }

    public Task DeleteActivityAsync(string id)
    {
        _activityData.TryRemove(id, out _);
        return Task.CompletedTask;
    }

    // Health Metric Operations
    public Task<HealthMetric?> GetMetricByIdAsync(string id)
    {
        _metrics.TryGetValue(id, out var metric);
        return Task.FromResult(metric);
    }

    public Task<List<HealthMetric>> GetMetricsByNameAsync(string metricName)
    {
        var result = _metrics.Values.Where(x => x.MetricName == metricName).ToList();
        return Task.FromResult(result);
    }

    public Task AddMetricAsync(HealthMetric metric)
    {
        _metrics[metric.Id] = metric;
        return Task.CompletedTask;
    }

    public Task UpdateMetricAsync(HealthMetric metric)
    {
        _metrics[metric.Id] = metric;
        return Task.CompletedTask;
    }

    public Task DeleteMetricAsync(string id)
    {
        _metrics.TryRemove(id, out _);
        return Task.CompletedTask;
    }

    // Bulk Operations
    public Task<int> GetTotalRecordCountAsync()
    {
        var count = _sleepData.Count + _heartRateData.Count + _spO2Data.Count +
                   _stepsData.Count + _activityData.Count;
        return Task.FromResult(count);
    }

    public Task<bool> AnyRecordsExistAsync(DateTime date)
    {
        return Task.FromResult(
            _sleepData.Values.Any(x => x.RecordDate.Date == date.Date) ||
            _heartRateData.Values.Any(x => x.RecordDate.Date == date.Date) ||
            _spO2Data.Values.Any(x => x.RecordDate.Date == date.Date) ||
            _stepsData.Values.Any(x => x.RecordDate.Date == date.Date) ||
            _activityData.Values.Any(x => x.RecordDate.Date == date.Date)
        );
    }

    public Task DeleteOldRecordsAsync(DateTime beforeDate)
    {
        var sleepKeys = _sleepData.Where(x => x.Value.RecordDate < beforeDate).Select(x => x.Key).ToList();
        var hrKeys = _heartRateData.Where(x => x.Value.RecordDate < beforeDate).Select(x => x.Key).ToList();
        var spo2Keys = _spO2Data.Where(x => x.Value.RecordDate < beforeDate).Select(x => x.Key).ToList();
        var stepsKeys = _stepsData.Where(x => x.Value.RecordDate < beforeDate).Select(x => x.Key).ToList();
        var activityKeys = _activityData.Where(x => x.Value.RecordDate < beforeDate).Select(x => x.Key).ToList();

        foreach (var key in sleepKeys) _sleepData.TryRemove(key, out _);
        foreach (var key in hrKeys) _heartRateData.TryRemove(key, out _);
        foreach (var key in spo2Keys) _spO2Data.TryRemove(key, out _);
        foreach (var key in stepsKeys) _stepsData.TryRemove(key, out _);
        foreach (var key in activityKeys) _activityData.TryRemove(key, out _);

        return Task.CompletedTask;
    }

    public async Task<HealthDataCollection> GetAllRecordsAsync(DateTime startDate, DateTime endDate)
    {
        var collection = new HealthDataCollection
        {
            SleepRecords = await GetSleepRangeAsync(startDate, endDate),
            HeartRateRecords = await GetHeartRateRangeAsync(startDate, endDate),
            SpO2Records = await GetSpO2RangeAsync(startDate, endDate),
            StepsRecords = await GetStepsRangeAsync(startDate, endDate),
            ActivityRecords = await GetActivitiesRangeAsync(startDate, endDate),
            Metrics = _metrics.Values.Where(x => x.RecordDate >= startDate && x.RecordDate <= endDate).ToList()
        };

        return collection;
    }

    public void Dispose() { }
}
