// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace HealthDataExportTools.Events;

/// <summary>
/// Event raised when health data is successfully imported
/// Used to trigger downstream processing like caching and indexing
/// </summary>
public class HealthDataImportedEvent : EventBase
{
    public int RecordCount { get; }
    public DateTime ImportStartTime { get; }
    public DateTime ImportEndTime { get; }
    public string ImportSource { get; }
    public DeviceType DeviceType { get; }
    public List<string> ImportedMetricTypes { get; }

    public HealthDataImportedEvent(
        string importId,
        int recordCount,
        DateTime importStartTime,
        DateTime importEndTime,
        string importSource,
        DeviceType deviceType,
        List<string> importedMetricTypes)
        : base(importId)
    {
        RecordCount = recordCount;
        ImportStartTime = importStartTime;
        ImportEndTime = importEndTime;
        ImportSource = importSource;
        DeviceType = deviceType;
        ImportedMetricTypes = importedMetricTypes ?? new List<string>();
    }

    /// <summary>
    /// Calculate duration of the import operation
    /// </summary>
    public TimeSpan GetImportDuration() => ImportEndTime - ImportStartTime;

    /// <summary>
    /// Get records per second throughput
    /// </summary>
    public double GetThroughput()
    {
        var duration = GetImportDuration();
        if (duration.TotalSeconds == 0)
            return 0;

        return RecordCount / duration.TotalSeconds;
    }

    public override string ToString()
    {
        return $"HealthDataImported: {RecordCount} records from {ImportSource} ({DeviceType}) " +
               $"in {GetImportDuration().TotalSeconds:F2}s";
    }
}
