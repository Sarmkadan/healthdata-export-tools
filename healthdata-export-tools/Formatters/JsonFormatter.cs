#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace HealthDataExportTools.Formatters;

/// <summary>
/// Formats health data into JSON format with proper serialization
/// Supports pretty-printing and compact modes
/// </summary>
public sealed partial class JsonFormatter : IDataFormatter
{
    private readonly ILogger<JsonFormatter> _logger;
    private readonly int _maxRecordCount;
    private readonly JsonSerializerOptions _jsonOptions;

    public string FileExtension => ".json";
    public string FormatName => "JSON";

    /// <summary>
    /// Initializes a new instance of <see cref="JsonFormatter"/>.
    /// </summary>
    /// <param name="logger">Logger instance; must not be <c>null</c>.</param>
    /// <param name="maxRecordCount">
    /// Maximum number of records that can be formatted in a single operation.
    /// Defaults to <c>100_000</c>.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="maxRecordCount"/> is less than or equal to zero.
    /// </exception>
    public JsonFormatter(ILogger<JsonFormatter> logger, int maxRecordCount = 100_000)
    {
        ArgumentNullException.ThrowIfNull(logger);
        if (maxRecordCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxRecordCount), "Maximum record count must be greater than zero.");

        _logger = logger;
        _maxRecordCount = maxRecordCount;

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    /// <summary>
    /// Determines whether the supplied <paramref name="dataType"/> can be formatted as JSON.
    /// </summary>
    /// <param name="dataType">The type to evaluate; must not be <c>null</c>.</param>
    /// <returns><c>true</c> if the type is supported; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dataType"/> is <c>null</c>.</exception>
    public bool CanFormat(Type dataType)
    {
        ArgumentNullException.ThrowIfNull(dataType);
        return true; // JSON can format any type
    }

    /// <summary>
    /// Formats a single <see cref="HealthDataRecord"/> as JSON.
    /// </summary>
    /// <param name="record">The record to format; must not be <c>null</c>.</param>
    /// <returns>A JSON string containing the serialized record.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="record"/> is <c>null</c>.</exception>
    public async Task<string> FormatAsync(HealthDataRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        var jsonObject = new
        {
            record.RecordDate,
            MetricType = record.GetType().Name,
            DeviceType = record.DeviceId,
            Value = string.Empty
        };

        _logger.LogDebug("Formatted single record to JSON");
        return await Task.FromResult(JsonSerializer.Serialize(jsonObject, _jsonOptions)).ConfigureAwait(false);
    }

    /// <summary>
    /// Formats a collection of <see cref="HealthDataRecord"/> instances as JSON array with metadata.
    /// </summary>
    /// <param name="records">The collection to format; must not be <c>null</c>.</param>
    /// <returns>JSON representation of the collection.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="records"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the number of records exceeds the configured maximum.
    /// </exception>
    public async Task<string> FormatCollectionAsync(List<HealthDataRecord> records)
    {
        ArgumentNullException.ThrowIfNull(records);
        if (records.Count > _maxRecordCount)
            throw new ArgumentException($"Record collection exceeds the maximum allowed count of {_maxRecordCount}.", nameof(records));

        if (records.Count == 0)
        {
            _logger.LogWarning("Empty record collection provided to JSON formatter");
            return string.Empty;
        }

        var output = new
        {
            ExportDate = DateTime.UtcNow,
            TotalRecords = records.Count,
            Records = records.Select(r => new
            {
                r.RecordDate,
                MetricType = r.GetType().Name,
                DeviceType = r.DeviceId,
                Value = string.Empty
            }).ToList()
        };

        _logger.LogInformation("Formatted {Count} records to JSON", records.Count);
        return await Task.FromResult(JsonSerializer.Serialize(output, _jsonOptions)).ConfigureAwait(false);
    }

    /// <summary>
    /// Formats a list of <see cref="SleepData"/> records as JSON with detailed sleep metrics.
    /// </summary>
    /// <param name="sleepRecords">The sleep records to format; must not be <c>null</c>.</param>
    /// <returns>JSON string for the supplied sleep data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepRecords"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the number of records exceeds the configured maximum.
    /// </exception>
    public async Task<string> FormatSleepDataAsync(List<SleepData> sleepRecords)
    {
        ArgumentNullException.ThrowIfNull(sleepRecords);
        if (sleepRecords.Count > _maxRecordCount)
            throw new ArgumentException($"Sleep record collection exceeds the maximum allowed count of {_maxRecordCount}.", nameof(sleepRecords));

        if (sleepRecords.Count == 0)
            return string.Empty;

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
                Date = s.RecordDate,
                s.DurationMinutes,
                s.Quality,
                s.DeepSleepMinutes,
                s.RemSleepMinutes,
                s.AwakeMinutes,
                DeviceType = s.DeviceId
            }).ToList()
        };

        _logger.LogInformation("Formatted {Count} sleep records to JSON", sleepRecords.Count);
        return await Task.FromResult(JsonSerializer.Serialize(output, _jsonOptions)).ConfigureAwait(false);
    }

    /// <summary>
    /// Formats a list of <see cref="HeartRateData"/> records as JSON with HR statistics.
    /// </summary>
    /// <param name="heartRateRecords">The heart-rate records to format; must not be <c>null</c>.</param>
    /// <returns>JSON string for the supplied heart-rate data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="heartRateRecords"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the number of records exceeds the configured maximum.
    /// </exception>
    public async Task<string> FormatHeartRateDataAsync(List<HeartRateData> heartRateRecords)
    {
        ArgumentNullException.ThrowIfNull(heartRateRecords);
        if (heartRateRecords.Count > _maxRecordCount)
            throw new ArgumentException($"Heart-rate record collection exceeds the maximum allowed count of {_maxRecordCount}.", nameof(heartRateRecords));

        if (heartRateRecords.Count == 0)
            return string.Empty;

        var output = new
        {
            DataType = "HeartRate",
            ExportDate = DateTime.UtcNow,
            RecordCount = heartRateRecords.Count,
            Statistics = new
            {
                AverageHeartRate = heartRateRecords.Average(h => h.AverageBpm),
                MaxHeartRate = heartRateRecords.Max(h => h.MaximumBpm),
                MinHeartRate = heartRateRecords.Min(h => h.MinimumBpm)
            },
            Records = heartRateRecords.Select(h => new
            {
                Timestamp = h.RecordDate,
                HeartRate = h.AverageBpm,
                HeartRateZone = string.Empty,
                DeviceType = h.DeviceId
            }).ToList()
        };

        _logger.LogInformation("Formatted {Count} heart rate records to JSON", heartRateRecords.Count);
        return await Task.FromResult(JsonSerializer.Serialize(output, _jsonOptions)).ConfigureAwait(false);
    }

    /// <summary>
    /// Formats a list of <see cref="SpO2Data"/> records as JSON with oxygen level statistics.
    /// </summary>
    /// <param name="spo2Records">The SpO2 records to format; must not be <c>null</c>.</param>
    /// <returns>JSON string for the supplied SpO2 data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="spo2Records"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the number of records exceeds the configured maximum.
    /// </exception>
    public async Task<string> FormatSpO2DataAsync(List<SpO2Data> spo2Records)
    {
        ArgumentNullException.ThrowIfNull(spo2Records);
        if (spo2Records.Count > _maxRecordCount)
            throw new ArgumentException($"SpO2 record collection exceeds the maximum allowed count of {_maxRecordCount}.", nameof(spo2Records));

        if (spo2Records.Count == 0)
            return string.Empty;

        var output = new
        {
            DataType = "SpO2",
            ExportDate = DateTime.UtcNow,
            RecordCount = spo2Records.Count,
            Statistics = new
            {
                AverageSpO2 = spo2Records.Average(s => s.AveragePercentage),
                MinSpO2 = spo2Records.Min(s => s.MinimumPercentage),
                LowOxygenEvents = spo2Records.Count(s => s.HasConcerningLevels())
            },
            Records = spo2Records.Select(s => new
            {
                Timestamp = s.RecordDate,
                SpO2 = s.AveragePercentage,
                IsLowOxygen = s.HasConcerningLevels(),
                DeviceType = s.DeviceId
            }).ToList()
        };

        _logger.LogInformation("Formatted {Count} SpO2 records to JSON", spo2Records.Count);
        return await Task.FromResult(JsonSerializer.Serialize(output, _jsonOptions)).ConfigureAwait(false);
    }

    /// <summary>
    /// Formats a list of <see cref="StepsData"/> records as JSON with activity statistics.
    /// </summary>
    /// <param name="stepsRecords">The steps records to format; must not be <c>null</c>.</param>
    /// <returns>JSON string for the supplied steps data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="stepsRecords"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the number of records exceeds the configured maximum.
    /// </exception>
    public async Task<string> FormatStepsDataAsync(List<StepsData> stepsRecords)
    {
        ArgumentNullException.ThrowIfNull(stepsRecords);
        if (stepsRecords.Count > _maxRecordCount)
            throw new ArgumentException($"Steps record collection exceeds the maximum allowed count of {_maxRecordCount}.", nameof(stepsRecords));

        if (stepsRecords.Count == 0)
            return string.Empty;

        var output = new
        {
            DataType = "Steps",
            ExportDate = DateTime.UtcNow,
            RecordCount = stepsRecords.Count,
            Statistics = new
            {
                TotalSteps = stepsRecords.Sum(s => s.TotalSteps),
                AverageStepsPerDay = stepsRecords.Average(s => s.TotalSteps),
                TotalDistance = stepsRecords.Sum(s => s.DistanceKm),
                TotalCalories = stepsRecords.Sum(s => s.CaloriesBurned)
            },
            Records = stepsRecords.Select(s => new
            {
                Date = s.RecordDate,
                StepCount = s.TotalSteps,
                Distance = s.DistanceKm,
                Calories = s.CaloriesBurned,
                DeviceType = s.DeviceId
            }).ToList()
        };

        _logger.LogInformation("Formatted {Count} steps records to JSON", stepsRecords.Count);
        return await Task.FromResult(JsonSerializer.Serialize(output, _jsonOptions)).ConfigureAwait(false);
    }

    /// <summary>
    /// Validates a collection of <see cref="HealthDataRecord"/> before JSON serialization.
    /// </summary>
    /// <param name="records">The records to validate; must not be <c>null</c>.</param>
    /// <returns>A list of validation error messages; empty if no errors were found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="records"/> is <c>null</c>.</exception>
    public async Task<List<string>> ValidateAsync(List<HealthDataRecord> records)
    {
        ArgumentNullException.ThrowIfNull(records);

        var errors = new List<string>();

        if (records.Count == 0)
        {
            errors.Add("Record collection is empty");
            return await Task.FromResult(errors).ConfigureAwait(false);
        }

        if (records.Count > _maxRecordCount)
        {
            errors.Add($"Record collection exceeds the maximum allowed count of {_maxRecordCount}");
            return await Task.FromResult(errors).ConfigureAwait(false);
        }

        for (int i = 0; i < records.Count; i++)
        {
            var record = records[i];
            if (record.RecordDate == default)
                errors.Add($"Record {i}: RecordDate is not set");
        }

        _logger.LogInformation("Validation complete: {ErrorCount} errors found", errors.Count);
        return await Task.FromResult(errors).ConfigureAwait(false);
    }
}
