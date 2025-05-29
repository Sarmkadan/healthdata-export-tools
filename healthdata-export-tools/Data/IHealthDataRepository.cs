// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using HealthDataExportTools.Domain.Models;

namespace HealthDataExportTools.Data;

/// <summary>
/// Repository interface for health data persistence operations
/// </summary>
public interface IHealthDataRepository : IDisposable
{
    // Sleep Data Operations
    Task<SleepData?> GetSleepByIdAsync(string id);
    Task<List<SleepData>> GetSleepByDateAsync(DateTime date);
    Task<List<SleepData>> GetSleepRangeAsync(DateTime startDate, DateTime endDate);
    Task AddSleepAsync(SleepData data);
    Task UpdateSleepAsync(SleepData data);
    Task DeleteSleepAsync(string id);

    // Heart Rate Operations
    Task<HeartRateData?> GetHeartRateByIdAsync(string id);
    Task<HeartRateData?> GetHeartRateByDateAsync(DateTime date);
    Task<List<HeartRateData>> GetHeartRateRangeAsync(DateTime startDate, DateTime endDate);
    Task AddHeartRateAsync(HeartRateData data);
    Task UpdateHeartRateAsync(HeartRateData data);
    Task DeleteHeartRateAsync(string id);

    // SpO2 Operations
    Task<SpO2Data?> GetSpO2ByIdAsync(string id);
    Task<SpO2Data?> GetSpO2ByDateAsync(DateTime date);
    Task<List<SpO2Data>> GetSpO2RangeAsync(DateTime startDate, DateTime endDate);
    Task AddSpO2Async(SpO2Data data);
    Task UpdateSpO2Async(SpO2Data data);
    Task DeleteSpO2Async(string id);

    // Steps Operations
    Task<StepsData?> GetStepsByIdAsync(string id);
    Task<StepsData?> GetStepsByDateAsync(DateTime date);
    Task<List<StepsData>> GetStepsRangeAsync(DateTime startDate, DateTime endDate);
    Task AddStepsAsync(StepsData data);
    Task UpdateStepsAsync(StepsData data);
    Task DeleteStepsAsync(string id);

    // Activity Operations
    Task<ActivityData?> GetActivityByIdAsync(string id);
    Task<List<ActivityData>> GetActivitiesByDateAsync(DateTime date);
    Task<List<ActivityData>> GetActivitiesRangeAsync(DateTime startDate, DateTime endDate);
    Task AddActivityAsync(ActivityData data);
    Task UpdateActivityAsync(ActivityData data);
    Task DeleteActivityAsync(string id);

    // Health Metric Operations
    Task<HealthMetric?> GetMetricByIdAsync(string id);
    Task<List<HealthMetric>> GetMetricsByNameAsync(string metricName);
    Task AddMetricAsync(HealthMetric metric);
    Task UpdateMetricAsync(HealthMetric metric);
    Task DeleteMetricAsync(string id);

    // Bulk Operations
    Task<int> GetTotalRecordCountAsync();
    Task<bool> AnyRecordsExistAsync(DateTime date);
    Task DeleteOldRecordsAsync(DateTime beforeDate);
    Task<HealthDataCollection> GetAllRecordsAsync(DateTime startDate, DateTime endDate);
}
