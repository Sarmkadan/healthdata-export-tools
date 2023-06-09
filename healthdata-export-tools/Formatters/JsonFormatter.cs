// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace HealthDataExportTools.Formatters;

/// <summary>
/// Formats health data into JSON format with proper serialization
/// Supports pretty-printing and compact modes
/// </summary>
public class JsonFormatter : IDataFormatter
{
    private readonly ILogger<JsonFormatter> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public string FileExtension => ".json";
    public string FormatName => "JSON";

    public JsonFormatter(ILogger<JsonFormatter> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    /// <summary>
    /// Check if data can be formatted as JSON (all types supported)
    /// </summary>
    public bool CanFormat(Type dataType)
    {
        return dataType != null;
    }

    /// <summary>
    /// Format single health record as JSON
    /// </summary>
    public async Task<string> FormatAsync(HealthDataRecord record)
    {
        try
        {
            var jsonObject = new
            {
                record.RecordDate,
                record.MetricType,
                record.DeviceType,
                record.Value
            };

            return await Task.FromResult(JsonSerializer.Serialize(jsonObject, _jsonOptions));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error formatting single record to JSON");
            throw;
        }
    }

    /// <summary>
    /// Format collection of records as JSON array with metadata
    /// </summary>
    public async Task<string> FormatCollectionAsync(List<HealthDataRecord> records)
    {
        try
        {
            if (records == null || records.Count == 0)
            {
                _logger.LogWarning("Empty record collection provided to JSON formatter");
                return await Task.FromResult("[]");
            }

            var output = new
            {
                ExportDate = DateTime.UtcNow,
                TotalRecords = records.Count,
                Records = records.Select(r => new
                {
                    r.RecordDate,
                    r.MetricType,
                    r.DeviceType,
                    r.Value
                }).ToList()
            };

            _logger.LogInformation("Formatted {Count} records to JSON", records.Count);
            return await Task.FromResult(JsonSerializer.Serialize(output, _jsonOptions));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error formatting records collection to JSON");
            throw;
        }
    }

    /// <summary>
    /// Format sleep data as JSON with detailed sleep metrics
    /// </summary>
    public async Task<string> FormatSleepDataAsync(List<SleepData> sleepRecords)
    {
        try
        {
            if (sleepRecords == null || sleepRecords.Count == 0)
                return await Task.FromResult("[]");

            var output = new
            {
                DataType = "Sleep",
                ExportDate = DateTime.UtcNow,
                RecordCount = sleepRecords.Count,
                Statistics = new
                {
                    AverageDurationMinutes = sleepRecords.Average(s => s.DurationMinutes),
                    AverageDeepSleep = sleepRecords.Average(s => s.DeepSleepMinutes),
                    AverageQualityScore = sleepRecords.Average(s => (double)s.Quality)
                },
                Records = sleepRecords.Select(s => new
                {
                    s.SleepDate,
                    s.DurationMinutes,
                    s.Quality,
                    s.DeepSleepMinutes,
                    s.RemSleepMinutes,
                    s.AwakeMinutes,
                    s.DeviceType
                }).ToList()
            };

            _logger.LogInformation("Formatted {Count} sleep records to JSON", sleepRecords.Count);
            return await Task.FromResult(JsonSerializer.Serialize(output, _jsonOptions));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error formatting sleep data to JSON");
            throw;
        }
    }

    /// <summary>
    /// Format heart rate data as JSON with HR statistics
    /// </summary>
    public async Task<string> FormatHeartRateDataAsync(List<HeartRateData> heartRateRecords)
    {
        try
        {
            if (heartRateRecords == null || heartRateRecords.Count == 0)
                return await Task.FromResult("[]");

            var output = new
            {
                DataType = "HeartRate",
                ExportDate = DateTime.UtcNow,
                RecordCount = heartRateRecords.Count,
                Statistics = new
                {
                    AverageHeartRate = heartRateRecords.Average(h => h.HeartRate),
                    MaxHeartRate = heartRateRecords.Max(h => h.HeartRate),
                    MinHeartRate = heartRateRecords.Min(h => h.HeartRate)
                },
                Records = heartRateRecords.Select(h => new
                {
                    h.Timestamp,
                    h.HeartRate,
                    h.HeartRateZone,
                    h.DeviceType
                }).ToList()
            };

            _logger.LogInformation("Formatted {Count} heart rate records to JSON", heartRateRecords.Count);
            return await Task.FromResult(JsonSerializer.Serialize(output, _jsonOptions));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error formatting heart rate data to JSON");
            throw;
        }
    }

    /// <summary>
    /// Format SpO2 data as JSON with oxygen level statistics
    /// </summary>
    public async Task<string> FormatSpO2DataAsync(List<SpO2Data> spo2Records)
    {
        try
        {
            if (spo2Records == null || spo2Records.Count == 0)
                return await Task.FromResult("[]");

            var output = new
            {
                DataType = "SpO2",
                ExportDate = DateTime.UtcNow,
                RecordCount = spo2Records.Count,
                Statistics = new
                {
                    AverageSpO2 = spo2Records.Average(s => s.SpO2),
                    MinSpO2 = spo2Records.Min(s => s.SpO2),
                    LowOxygenEvents = spo2Records.Count(s => s.IsLowOxygen)
                },
                Records = spo2Records.Select(s => new
                {
                    s.Timestamp,
                    s.SpO2,
                    s.IsLowOxygen,
                    s.DeviceType
                }).ToList()
            };

            _logger.LogInformation("Formatted {Count} SpO2 records to JSON", spo2Records.Count);
            return await Task.FromResult(JsonSerializer.Serialize(output, _jsonOptions));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error formatting SpO2 data to JSON");
            throw;
        }
    }

    /// <summary>
    /// Format steps data as JSON with activity statistics
    /// </summary>
    public async Task<string> FormatStepsDataAsync(List<StepsData> stepsRecords)
    {
        try
        {
            if (stepsRecords == null || stepsRecords.Count == 0)
                return await Task.FromResult("[]");

            var output = new
            {
                DataType = "Steps",
                ExportDate = DateTime.UtcNow,
                RecordCount = stepsRecords.Count,
                Statistics = new
                {
                    TotalSteps = stepsRecords.Sum(s => s.StepCount),
                    AverageStepsPerDay = stepsRecords.Average(s => s.StepCount),
                    TotalDistance = stepsRecords.Sum(s => s.Distance),
                    TotalCalories = stepsRecords.Sum(s => s.Calories)
                },
                Records = stepsRecords.Select(s => new
                {
                    s.StepsDate,
                    s.StepCount,
                    s.Distance,
                    s.Calories,
                    s.DeviceType
                }).ToList()
            };

            _logger.LogInformation("Formatted {Count} steps records to JSON", stepsRecords.Count);
            return await Task.FromResult(JsonSerializer.Serialize(output, _jsonOptions));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error formatting steps data to JSON");
            throw;
        }
    }

    /// <summary>
    /// Validate records before JSON serialization
    /// </summary>
    public async Task<List<string>> ValidateAsync(List<HealthDataRecord> records)
    {
        var errors = new List<string>();

        if (records == null)
        {
            errors.Add("Record collection is null");
            return await Task.FromResult(errors);
        }

        // JSON can technically handle empty collections, but log it
        if (records.Count == 0)
        {
            _logger.LogWarning("Empty record collection provided for JSON validation");
        }

        // Basic validation
        for (int i = 0; i < records.Count; i++)
        {
            var record = records[i];

            if (double.IsNaN(record.Value) || double.IsInfinity(record.Value))
                errors.Add($"Record {i}: Invalid numeric value (NaN or Infinity)");
        }

        return await Task.FromResult(errors);
    }
}
