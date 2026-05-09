// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using CsvHelper;
using CsvHelper.Configuration;

namespace HealthDataExportTools.Formatters;

/// <summary>
/// Formats health data into CSV (Comma-Separated Values) format
/// Provides separate CSV files for each data type to maintain clean structure
/// </summary>
public class CsvFormatter : IDataFormatter
{
    private readonly ILogger<CsvFormatter> _logger;

    public string FileExtension => ".csv";
    public string FormatName => "CSV";

    public CsvFormatter(ILogger<CsvFormatter> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Check if data can be formatted as CSV (most types can be)
    /// </summary>
    public bool CanFormat(Type dataType)
    {
        return typeof(HealthDataRecord).IsAssignableFrom(dataType) ||
               typeof(SleepData).IsAssignableFrom(dataType) ||
               typeof(HeartRateData).IsAssignableFrom(dataType) ||
               typeof(SpO2Data).IsAssignableFrom(dataType) ||
               typeof(StepsData).IsAssignableFrom(dataType) ||
               typeof(ActivityData).IsAssignableFrom(dataType);
    }

    /// <summary>
    /// Format single record as CSV row (not typically used, but implements interface)
    /// </summary>
    public async Task<string> FormatAsync(HealthDataRecord record)
    {
        var sb = new StringBuilder();
        using (var writer = new StringWriter(sb))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            // Write header
            csv.WriteField("RecordDate");
            csv.WriteField("MetricType");
            csv.WriteField("DeviceType");
            csv.WriteField("Value");
            csv.NextRecord();

            // Write data
            csv.WriteField(record.RecordDate);
            csv.WriteField(record.GetType().Name);
            csv.WriteField(record.DeviceId);
            csv.WriteField(string.Empty);
            csv.NextRecord();
        }

        return await Task.FromResult(sb.ToString());
    }

    /// <summary>
    /// Format collection of records as CSV with headers
    /// </summary>
    public async Task<string> FormatCollectionAsync(List<HealthDataRecord> records)
    {
        if (records == null || records.Count == 0)
        {
            _logger.LogWarning("Empty record collection provided to CSV formatter");
            return string.Empty;
        }

        var sb = new StringBuilder();
        using (var writer = new StringWriter(sb))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            // Write headers
            csv.WriteField("RecordDate");
            csv.WriteField("MetricType");
            csv.WriteField("DeviceType");
            csv.WriteField("Value");
            csv.NextRecord();

            // Write all records
            foreach (var record in records)
            {
                csv.WriteField(record.RecordDate);
                csv.WriteField(record.GetType().Name);
                csv.WriteField(record.DeviceId);
                csv.WriteField(string.Empty);
                csv.NextRecord();
            }
        }

        _logger.LogInformation("Formatted {Count} records to CSV", records.Count);
        return await Task.FromResult(sb.ToString());
    }

    /// <summary>
    /// Format sleep data into CSV with sleep-specific columns
    /// </summary>
    public async Task<string> FormatSleepDataAsync(List<SleepData> sleepRecords)
    {
        if (sleepRecords == null || sleepRecords.Count == 0)
            return string.Empty;

        var sb = new StringBuilder();
        using (var writer = new StringWriter(sb))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            // Write headers
            csv.WriteField("Date");
            csv.WriteField("DurationMinutes");
            csv.WriteField("Quality");
            csv.WriteField("DeepSleepMinutes");
            csv.WriteField("RemoSleepMinutes");
            csv.WriteField("AwakeMinutes");
            csv.WriteField("DeviceType");
            csv.NextRecord();

            // Write sleep records
            foreach (var record in sleepRecords)
            {
                csv.WriteField(record.RecordDate);
                csv.WriteField(record.DurationMinutes);
                csv.WriteField(record.Quality);
                csv.WriteField(record.DeepSleepMinutes);
                csv.WriteField(record.RemSleepMinutes);
                csv.WriteField(record.AwakeMinutes);
                csv.WriteField(record.DeviceId);
                csv.NextRecord();
            }
        }

        _logger.LogInformation("Formatted {Count} sleep records to CSV", sleepRecords.Count);
        return await Task.FromResult(sb.ToString());
    }

    /// <summary>
    /// Format heart rate data into CSV with HR-specific columns
    /// </summary>
    public async Task<string> FormatHeartRateDataAsync(List<HeartRateData> heartRateRecords)
    {
        if (heartRateRecords == null || heartRateRecords.Count == 0)
            return string.Empty;

        var sb = new StringBuilder();
        using (var writer = new StringWriter(sb))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            // Write headers
            csv.WriteField("Timestamp");
            csv.WriteField("HeartRate");
            csv.WriteField("HeartRateZone");
            csv.WriteField("DeviceType");
            csv.NextRecord();

            // Write heart rate records
            foreach (var record in heartRateRecords)
            {
                csv.WriteField(record.RecordDate);
                csv.WriteField(record.AverageBpm);
                csv.WriteField(string.Empty);
                csv.WriteField(record.DeviceId);
                csv.NextRecord();
            }
        }

        _logger.LogInformation("Formatted {Count} heart rate records to CSV", heartRateRecords.Count);
        return await Task.FromResult(sb.ToString());
    }

    /// <summary>
    /// Format SpO2 data into CSV with oxygen-specific columns
    /// </summary>
    public async Task<string> FormatSpO2DataAsync(List<SpO2Data> spo2Records)
    {
        if (spo2Records == null || spo2Records.Count == 0)
            return string.Empty;

        var sb = new StringBuilder();
        using (var writer = new StringWriter(sb))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.WriteField("Timestamp");
            csv.WriteField("SpO2");
            csv.WriteField("IsLowOxygen");
            csv.WriteField("DeviceType");
            csv.NextRecord();

            foreach (var record in spo2Records)
            {
                csv.WriteField(record.RecordDate);
                csv.WriteField(record.AveragePercentage);
                csv.WriteField(record.HasConcerningLevels());
                csv.WriteField(record.DeviceId);
                csv.NextRecord();
            }
        }

        _logger.LogInformation("Formatted {Count} SpO2 records to CSV", spo2Records.Count);
        return await Task.FromResult(sb.ToString());
    }

    /// <summary>
    /// Format steps data into CSV with activity-specific columns
    /// </summary>
    public async Task<string> FormatStepsDataAsync(List<StepsData> stepsRecords)
    {
        if (stepsRecords == null || stepsRecords.Count == 0)
            return string.Empty;

        var sb = new StringBuilder();
        using (var writer = new StringWriter(sb))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.WriteField("Date");
            csv.WriteField("StepCount");
            csv.WriteField("Distance");
            csv.WriteField("Calories");
            csv.WriteField("DeviceType");
            csv.NextRecord();

            foreach (var record in stepsRecords)
            {
                csv.WriteField(record.RecordDate);
                csv.WriteField(record.TotalSteps);
                csv.WriteField(record.DistanceKm);
                csv.WriteField(record.CaloriesBurned);
                csv.WriteField(record.DeviceId);
                csv.NextRecord();
            }
        }

        _logger.LogInformation("Formatted {Count} steps records to CSV", stepsRecords.Count);
        return await Task.FromResult(sb.ToString());
    }

    /// <summary>
    /// Validate health records before CSV export
    /// Checks for required fields and data consistency
    /// </summary>
    public async Task<List<string>> ValidateAsync(List<HealthDataRecord> records)
    {
        var errors = new List<string>();

        if (records == null)
        {
            errors.Add("Record collection is null");
            return await Task.FromResult(errors);
        }

        if (records.Count == 0)
        {
            errors.Add("Record collection is empty");
            return await Task.FromResult(errors);
        }

        // Validate each record
        for (int i = 0; i < records.Count; i++)
        {
            var record = records[i];

            if (record.RecordDate == default)
                errors.Add($"Record {i}: RecordDate is not set");
        }

        _logger.LogInformation("Validation complete: {ErrorCount} errors found", errors.Count);
        return await Task.FromResult(errors);
    }
}
