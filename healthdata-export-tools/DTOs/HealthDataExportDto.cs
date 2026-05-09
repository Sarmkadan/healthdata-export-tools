// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace HealthDataExportTools.DTOs;

/// <summary>
/// Data Transfer Object for comprehensive health data export
/// </summary>
public class HealthDataExportDto
{
    [JsonPropertyName("exportId")]
    public string ExportId { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("exportDate")]
    public DateTime ExportDate { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("summary")]
    public ExportSummary Summary { get; set; } = new();

    [JsonPropertyName("sleepData")]
    public List<SleepExportDto> SleepData { get; set; } = new();

    [JsonPropertyName("heartRateData")]
    public List<HeartRateExportDto> HeartRateData { get; set; } = new();

    [JsonPropertyName("spo2Data")]
    public List<SpO2ExportDto> SpO2Data { get; set; } = new();

    [JsonPropertyName("stepsData")]
    public List<StepsExportDto> StepsData { get; set; } = new();

    [JsonPropertyName("metadata")]
    public ExportMetadata Metadata { get; set; } = new();
}

/// <summary>
/// Export summary information
/// </summary>
public class ExportSummary
{
    [JsonPropertyName("totalRecords")]
    public int TotalRecords { get; set; }

    [JsonPropertyName("dateRange")]
    public DateRangeDto DateRange { get; set; } = new();

    [JsonPropertyName("recordCounts")]
    public RecordCountSummary RecordCounts { get; set; } = new();

    [JsonPropertyName("deviceTypes")]
    public List<string> DeviceTypes { get; set; } = new();
}

/// <summary>
/// Record count summary
/// </summary>
public class RecordCountSummary
{
    [JsonPropertyName("sleep")]
    public int Sleep { get; set; }

    [JsonPropertyName("heartRate")]
    public int HeartRate { get; set; }

    [JsonPropertyName("spo2")]
    public int SpO2 { get; set; }

    [JsonPropertyName("steps")]
    public int Steps { get; set; }

    [JsonPropertyName("activity")]
    public int Activity { get; set; }
}

/// <summary>
/// Date range DTO
/// </summary>
public class DateRangeDto
{
    [JsonPropertyName("start")]
    public DateTime Start { get; set; }

    [JsonPropertyName("end")]
    public DateTime End { get; set; }

    [JsonPropertyName("days")]
    public int DaysCovered => (End - Start).Days;
}

/// <summary>
/// Sleep data export DTO
/// </summary>
public class SleepExportDto
{
    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    [JsonPropertyName("durationMinutes")]
    public int DurationMinutes { get; set; }

    [JsonPropertyName("quality")]
    public int Quality { get; set; }

    [JsonPropertyName("deepSleepMinutes")]
    public int DeepSleepMinutes { get; set; }

    [JsonPropertyName("remSleepMinutes")]
    public int RemSleepMinutes { get; set; }

    [JsonPropertyName("awakeMinutes")]
    public int AwakeMinutes { get; set; }

    [JsonPropertyName("deviceType")]
    public string DeviceType { get; set; } = string.Empty;

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }
}

/// <summary>
/// Heart rate data export DTO
/// </summary>
public class HeartRateExportDto
{
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("heartRate")]
    public int HeartRate { get; set; }

    [JsonPropertyName("zone")]
    public string Zone { get; set; } = string.Empty;

    [JsonPropertyName("deviceType")]
    public string DeviceType { get; set; } = string.Empty;
}

/// <summary>
/// SpO2 data export DTO
/// </summary>
public class SpO2ExportDto
{
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("spo2")]
    public double SpO2 { get; set; }

    [JsonPropertyName("isLowOxygen")]
    public bool IsLowOxygen { get; set; }

    [JsonPropertyName("deviceType")]
    public string DeviceType { get; set; } = string.Empty;
}

/// <summary>
/// Steps data export DTO
/// </summary>
public class StepsExportDto
{
    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    [JsonPropertyName("steps")]
    public long Steps { get; set; }

    [JsonPropertyName("distance")]
    public double Distance { get; set; }

    [JsonPropertyName("calories")]
    public double Calories { get; set; }

    [JsonPropertyName("deviceType")]
    public string DeviceType { get; set; } = string.Empty;
}

/// <summary>
/// Export metadata
/// </summary>
public class ExportMetadata
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0.0";

    [JsonPropertyName("schema")]
    public string Schema { get; set; } = "HealthDataExport";

    [JsonPropertyName("timezone")]
    public string Timezone { get; set; } = TimeZoneInfo.Local.Id;

    [JsonPropertyName("compression")]
    public bool IsCompressed { get; set; }

    [JsonPropertyName("encryptionEnabled")]
    public bool EncryptionEnabled { get; set; }

    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;

    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = new();
}
