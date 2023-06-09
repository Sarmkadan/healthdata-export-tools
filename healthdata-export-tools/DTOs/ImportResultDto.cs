// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace HealthDataExportTools.DTOs;

/// <summary>
/// Data Transfer Object for import operation results
/// </summary>
[JsonSerializable]
public class ImportResultDto
{
    [JsonPropertyName("importId")]
    public string ImportId { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("status")]
    public string Status { get; set; } = "Success";

    [JsonPropertyName("recordsImported")]
    public int RecordsImported { get; set; }

    [JsonPropertyName("recordsValidated")]
    public int RecordsValidated { get; set; }

    [JsonPropertyName("recordsRejected")]
    public int RecordsRejected { get; set; }

    [JsonPropertyName("importSource")]
    public string ImportSource { get; set; } = string.Empty;

    [JsonPropertyName("deviceType")]
    public string DeviceType { get; set; } = string.Empty;

    [JsonPropertyName("dataTypes")]
    public List<string> DataTypes { get; set; } = new();

    [JsonPropertyName("startTime")]
    public DateTime StartTime { get; set; }

    [JsonPropertyName("endTime")]
    public DateTime EndTime { get; set; }

    [JsonPropertyName("duration")]
    public double DurationSeconds => (EndTime - StartTime).TotalSeconds;

    [JsonPropertyName("throughput")]
    public double RecordsPerSecond
    {
        get
        {
            var seconds = (EndTime - StartTime).TotalSeconds;
            return seconds > 0 ? RecordsImported / seconds : 0;
        }
    }

    [JsonPropertyName("validationErrors")]
    public List<ValidationError> ValidationErrors { get; set; } = new();

    [JsonPropertyName("warnings")]
    public List<string> Warnings { get; set; } = new();

    [JsonPropertyName("dateRange")]
    public DateRangeInfo DateRange { get; set; } = new();

    [JsonPropertyName("statistics")]
    public ImportStatistics Statistics { get; set; } = new();

    public bool HasErrors => ValidationErrors.Count > 0;
    public bool HasWarnings => Warnings.Count > 0;

    public double GetSuccessRate()
    {
        var total = RecordsImported + RecordsRejected;
        return total > 0 ? (RecordsImported * 100.0 / total) : 0;
    }
}

/// <summary>
/// Information about validation errors during import
/// </summary>
public class ValidationError
{
    [JsonPropertyName("recordIndex")]
    public int RecordIndex { get; set; }

    [JsonPropertyName("errorType")]
    public string ErrorType { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("severity")]
    public string Severity { get; set; } = "Error"; // Error, Warning
}

/// <summary>
/// Date range information
/// </summary>
public class DateRangeInfo
{
    [JsonPropertyName("startDate")]
    public DateTime? StartDate { get; set; }

    [JsonPropertyName("endDate")]
    public DateTime? EndDate { get; set; }

    [JsonPropertyName("daysCovered")]
    public int DaysCovered => EndDate != null && StartDate != null
        ? (int)(EndDate - StartDate)?.TotalDays
        : 0;
}

/// <summary>
/// Statistics about imported data
/// </summary>
public class ImportStatistics
{
    [JsonPropertyName("sleepRecords")]
    public int SleepRecords { get; set; }

    [JsonPropertyName("heartRateRecords")]
    public int HeartRateRecords { get; set; }

    [JsonPropertyName("spo2Records")]
    public int SpO2Records { get; set; }

    [JsonPropertyName("stepsRecords")]
    public int StepsRecords { get; set; }

    [JsonPropertyName("activityRecords")]
    public int ActivityRecords { get; set; }

    [JsonPropertyName("uniqueDevices")]
    public int UniqueDevices { get; set; }

    public int GetTotalMetrics() =>
        SleepRecords + HeartRateRecords + SpO2Records + StepsRecords + ActivityRecords;
}
