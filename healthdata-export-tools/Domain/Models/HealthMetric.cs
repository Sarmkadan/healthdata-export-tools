// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace HealthDataExportTools.Domain.Models;

/// <summary>
/// Computed health metrics derived from multiple data sources
/// </summary>
public class HealthMetric : HealthDataRecord
{
    /// <summary>
    /// Name of the metric (e.g., "VO2Max", "RestingHeartRate", "SleepDebt")
    /// </summary>
    public string MetricName { get; set; } = string.Empty;

    /// <summary>
    /// Current value of the metric
    /// </summary>
    public double Value { get; set; }

    /// <summary>
    /// Unit of measurement (BPM, %, ml/kg/min, hours, etc.)
    /// </summary>
    public string Unit { get; set; } = string.Empty;

    /// <summary>
    /// Lower bound of normal/healthy range
    /// </summary>
    public double? NormalRangeLow { get; set; }

    /// <summary>
    /// Upper bound of normal/healthy range
    /// </summary>
    public double? NormalRangeHigh { get; set; }

    /// <summary>
    /// Previous value for trend analysis
    /// </summary>
    public double? PreviousValue { get; set; }

    /// <summary>
    /// Trend indicator: -1 (worsening), 0 (stable), 1 (improving)
    /// </summary>
    public int? Trend { get; set; }

    /// <summary>
    /// Percentage change from previous value
    /// </summary>
    public double? PercentageChange { get; set; }

    /// <summary>
    /// Health status: "Optimal", "Normal", "Caution", "Alert"
    /// </summary>
    public string HealthStatus { get; set; } = "Normal";

    /// <summary>
    /// Confidence score for the metric calculation (0-100)
    /// </summary>
    public int? ConfidenceScore { get; set; }

    /// <summary>
    /// Components/data sources used in calculation
    /// </summary>
    public List<string> DataSources { get; set; } = [];

    /// <summary>
    /// Last date this metric was updated
    /// </summary>
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Number of days used in calculation/averaging
    /// </summary>
    public int? SampleDays { get; set; }

    /// <summary>
    /// Validate metric data
    /// </summary>
    public override bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(MetricName)) return false;
        if (string.IsNullOrWhiteSpace(Unit)) return false;
        if (Value < 0) return false;
        if (NormalRangeLow.HasValue && NormalRangeHigh.HasValue && NormalRangeLow > NormalRangeHigh) return false;

        return true;
    }

    /// <summary>
    /// Get metric summary
    /// </summary>
    public override Dictionary<string, object> GetSummary()
    {
        var summary = new Dictionary<string, object>
        {
            { "Metric", MetricName },
            { "Date", RecordDate.ToString("yyyy-MM-dd") },
            { "Value", $"{Value} {Unit}" },
            { "HealthStatus", HealthStatus },
            { "Confidence", ConfidenceScore },
            { "DataSources", string.Join(", ", DataSources) }
        };

        if (PreviousValue.HasValue && PercentageChange.HasValue)
        {
            summary.Add("Previous", $"{PreviousValue} {Unit}");
            var changeStr = PercentageChange >= 0 ? $"+{PercentageChange:F1}" : $"{PercentageChange:F1}";
            summary.Add("Change", $"{changeStr}%");
        }

        return summary;
    }

    /// <summary>
    /// Check if current value is within normal range
    /// </summary>
    public bool IsInNormalRange()
    {
        if (!NormalRangeLow.HasValue || !NormalRangeHigh.HasValue)
            return true;

        return Value >= NormalRangeLow.Value && Value <= NormalRangeHigh.Value;
    }

    /// <summary>
    /// Assess health status based on value and ranges
    /// </summary>
    public void AssessHealthStatus()
    {
        if (!NormalRangeLow.HasValue || !NormalRangeHigh.HasValue)
        {
            HealthStatus = "Normal";
            return;
        }

        var rangeSize = NormalRangeHigh.Value - NormalRangeLow.Value;
        var cautionLow = NormalRangeLow.Value - (rangeSize * 0.2);
        var cautionHigh = NormalRangeHigh.Value + (rangeSize * 0.2);

        if (Value >= NormalRangeLow.Value && Value <= NormalRangeHigh.Value)
            HealthStatus = "Optimal";
        else if (Value >= cautionLow && Value <= cautionHigh)
            HealthStatus = "Caution";
        else
            HealthStatus = "Alert";
    }

    /// <summary>
    /// Update metric with new value and calculate trend
    /// </summary>
    public void UpdateValue(double newValue)
    {
        PreviousValue = Value;
        Value = newValue;

        if (PreviousValue.HasValue)
        {
            PercentageChange = ((Value - PreviousValue.Value) / PreviousValue.Value) * 100;
            Trend = Value > PreviousValue.Value ? 1 : (Value < PreviousValue.Value ? -1 : 0);
        }

        AssessHealthStatus();
        LastUpdated = DateTime.UtcNow;
        Touch();
    }

    /// <summary>
    /// Add a data source to the components list
    /// </summary>
    public void AddDataSource(string source)
    {
        if (!DataSources.Contains(source))
            DataSources.Add(source);
    }
}
