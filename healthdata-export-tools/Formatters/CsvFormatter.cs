#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using HealthDataExportTools.Domain.Models;
using Microsoft.Extensions.Logging;

namespace HealthDataExportTools.Formatters;

/// <summary>
/// Formats health data into CSV (Comma-Separated Values) format.
/// Provides separate CSV files for each data type to maintain clean structure.
/// This implementation hardens CSV output against locale‑dependent formatting and
/// CSV injection attacks (OWASP). All numeric and date values are rendered using
/// <see cref="CultureInfo.InvariantCulture"/> and ISO‑8601 date strings. Fields that
/// start with '=', '+', '-', '@', '\t', '\r' are prefixed with a single quote to prevent
/// Excel formula injection. RFC 4180 quoting rules are delegated to <c>CsvHelper</c>.
/// </summary>
public sealed partial class CsvFormatter : IDataFormatter
{
    private readonly ILogger<CsvFormatter> _logger;
    private readonly int _maxRecordCount;
    private readonly int _maxFieldLength;
    private static readonly CsvConfiguration InvariantConfig = new(CultureInfo.InvariantCulture);

    /// <summary>
    /// File extension for CSV files.
    /// </summary>
    public string FileExtension => ".csv";

    /// <summary>
    /// Human‑readable name of the format.
    /// </summary>
    public string FormatName => "CSV";

    /// <summary>
    /// Initializes a new instance of <see cref="CsvFormatter"/>.
    /// </summary>
    /// <param name="logger">Logger instance; must not be <c>null</c>.</param>
    /// <param name="maxRecordCount">
    /// Maximum number of records that can be formatted in a single operation.
    /// Defaults to <c>100_000</c>.
    /// </param>
    /// <param name="maxFieldLength">
    /// Maximum length (in characters) allowed for any individual CSV field after sanitisation.
    /// Defaults to <c>10_000</c>.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="maxRecordCount"/> or <paramref name="maxFieldLength"/> is less than or equal to zero.
    /// </exception>
    public CsvFormatter(ILogger<CsvFormatter> logger, int maxRecordCount = 100_000, int maxFieldLength = 10_000)
    {
        ArgumentNullException.ThrowIfNull(logger);
        if (maxRecordCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxRecordCount), "Maximum record count must be greater than zero.");
        if (maxFieldLength <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxFieldLength), "Maximum field length must be greater than zero.");

        _logger = logger;
        _maxRecordCount = maxRecordCount;
        _maxFieldLength = maxFieldLength;
    }

    /// <summary>
    /// Determines whether the supplied <paramref name="dataType"/> can be formatted as CSV.
    /// </summary>
    /// <param name="dataType">The type to evaluate; must not be <c>null</c>.</param>
    /// <returns><c>true</c> if the type is a health‑data record; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dataType"/> is <c>null</c>.</exception>
    public bool CanFormat(Type dataType)
    {
        ArgumentNullException.ThrowIfNull(dataType);
        return dataType switch
        {
            _ when typeof(HealthDataRecord).IsAssignableFrom(dataType) => true,
            _ when typeof(SleepData).IsAssignableFrom(dataType) => true,
            _ when typeof(HeartRateData).IsAssignableFrom(dataType) => true,
            _ when typeof(SpO2Data).IsAssignableFrom(dataType) => true,
            _ when typeof(StepsData).IsAssignableFrom(dataType) => true,
            _ when typeof(ActivityData).IsAssignableFrom(dataType) => true,
            _ => false
        };
    }

    /// <summary>
    /// Formats a single <see cref="HealthDataRecord"/> as a CSV row.
    /// </summary>
    /// <param name="record">The record to format; must not be <c>null</c>.</param>
    /// <returns>A CSV string containing a header row followed by the record.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="record"/> is <c>null</c>.</exception>
    public async Task<string> FormatAsync(HealthDataRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        var sb = new StringBuilder();
        await using var writer = new StringWriter(sb);
        await using var csv = new CsvWriter(writer, InvariantConfig);

        // Header
        csv.WriteField("RecordDate");
        csv.WriteField("MetricType");
        csv.WriteField("DeviceType");
        csv.WriteField("Value");
        await csv.NextRecordAsync().ConfigureAwait(false);

        // Data
        csv.WriteField(FormatDate(record.RecordDate));
        csv.WriteField(record.GetType().Name);
        csv.WriteField(ValidateFieldLength(Sanitize(record.DeviceId), nameof(record.DeviceId)));
        csv.WriteField(ValidateFieldLength(Sanitize(record.Notes), nameof(record.Notes)));
        await csv.NextRecordAsync().ConfigureAwait(false);

        return sb.ToString();
    }

    /// <summary>
    /// Formats a collection of <see cref="HealthDataRecord"/> instances as CSV with a header row.
    /// </summary>
    /// <param name="records">The collection to format; must not be <c>null</c> or empty.</param>
    /// <returns>CSV representation of the collection.</returns>
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
            _logger.LogWarning("Empty record collection provided to CSV formatter");
            return string.Empty;
        }

        await using var writer = new StringWriter();
        await using var csv = new CsvWriter(writer, InvariantConfig);

        // Header
        csv.WriteField("RecordDate");
        csv.WriteField("MetricType");
        csv.WriteField("DeviceType");
        csv.WriteField("Value");
        await csv.NextRecordAsync().ConfigureAwait(false);

        // Rows
        foreach (var record in records)
        {
            csv.WriteField(FormatDate(record.RecordDate));
            csv.WriteField(record.GetType().Name);
            csv.WriteField(ValidateFieldLength(Sanitize(record.DeviceId), nameof(record.DeviceId)));
            csv.WriteField(ValidateFieldLength(Sanitize(record.Notes), nameof(record.Notes)));
            await csv.NextRecordAsync().ConfigureAwait(false);
        }

        _logger.LogInformation("Formatted {Count} records to CSV", records.Count);
        return writer.ToString();
    }

    /// <summary>
    /// Formats a list of <see cref="SleepData"/> records into CSV with sleep‑specific columns.
    /// </summary>
    /// <param name="sleepRecords">The sleep records to format; must not be <c>null</c> or empty.</param>
    /// <returns>CSV string for the supplied sleep data.</returns>
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

        await using var writer = new StringWriter();
        await using var csv = new CsvWriter(writer, InvariantConfig);

        // Header
        csv.WriteField("Date");
        csv.WriteField("DurationMinutes");
        csv.WriteField("Quality");
        csv.WriteField("DeepSleepMinutes");
        csv.WriteField("RemSleepMinutes");
        csv.WriteField("AwakeMinutes");
        csv.WriteField("DeviceType");
        await csv.NextRecordAsync().ConfigureAwait(false);

        // Rows
        foreach (var record in sleepRecords)
        {
            csv.WriteField(FormatDate(record.RecordDate));
            csv.WriteField(record.DurationMinutes);
            csv.WriteField(record.Quality);
            csv.WriteField(record.DeepSleepMinutes);
            csv.WriteField(record.RemSleepMinutes);
            csv.WriteField(record.AwakeMinutes);
            csv.WriteField(ValidateFieldLength(Sanitize(record.DeviceId), nameof(record.DeviceId)));
            await csv.NextRecordAsync().ConfigureAwait(false);
        }

        _logger.LogInformation("Formatted {Count} sleep records to CSV", sleepRecords.Count);
        return writer.ToString();
    }

    /// <summary>
    /// Formats a list of <see cref="HeartRateData"/> records into CSV with heart‑rate‑specific columns.
    /// </summary>
    /// <param name="heartRateRecords">The heart‑rate records to format; must not be <c>null</c> or empty.</param>
    /// <returns>CSV string for the supplied heart‑rate data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="heartRateRecords"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the number of records exceeds the configured maximum.
    /// </exception>
    public async Task<string> FormatHeartRateDataAsync(List<HeartRateData> heartRateRecords)
    {
        ArgumentNullException.ThrowIfNull(heartRateRecords);
        if (heartRateRecords.Count > _maxRecordCount)
            throw new ArgumentException($"Heart‑rate record collection exceeds the maximum allowed count of {_maxRecordCount}.", nameof(heartRateRecords));

        if (heartRateRecords.Count == 0)
            return string.Empty;

        await using var writer = new StringWriter();
        await using var csv = new CsvWriter(writer, InvariantConfig);

        // Header
        csv.WriteField("Timestamp");
        csv.WriteField("HeartRate");
        csv.WriteField("HeartRateZone");
        csv.WriteField("DeviceType");
        await csv.NextRecordAsync().ConfigureAwait(false);

        // Rows
        foreach (var record in heartRateRecords)
        {
            csv.WriteField(FormatDate(record.RecordDate));
            csv.WriteField(record.AverageBpm);
            csv.WriteField(string.Empty);
            csv.WriteField(ValidateFieldLength(Sanitize(record.DeviceId), nameof(record.DeviceId)));
            await csv.NextRecordAsync().ConfigureAwait(false);
        }

        _logger.LogInformation("Formatted {Count} heart rate records to CSV", heartRateRecords.Count);
        return writer.ToString();
    }

    /// <summary>
    /// Formats a list of <see cref="SpO2Data"/> records into CSV with oxygen‑specific columns.
    /// </summary>
    /// <param name="spo2Records">The SpO2 records to format; must not be <c>null</c> or empty.</param>
    /// <returns>CSV string for the supplied SpO2 data.</returns>
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

        await using var writer = new StringWriter();
        await using var csv = new CsvWriter(writer, InvariantConfig);

        // Header
        csv.WriteField("Timestamp");
        csv.WriteField("SpO2");
        csv.WriteField("IsLowOxygen");
        csv.WriteField("DeviceType");
        await csv.NextRecordAsync().ConfigureAwait(false);

        // Rows
        foreach (var record in spo2Records)
        {
            csv.WriteField(FormatDate(record.RecordDate));
            csv.WriteField(record.AveragePercentage);
            csv.WriteField(record.HasConcerningLevels());
            csv.WriteField(ValidateFieldLength(Sanitize(record.DeviceId), nameof(record.DeviceId)));
            await csv.NextRecordAsync().ConfigureAwait(false);
        }

        _logger.LogInformation("Formatted {Count} SpO2 records to CSV", spo2Records.Count);
        return writer.ToString();
    }

    /// <summary>
    /// Formats a list of <see cref="StepsData"/> records into CSV with activity‑specific columns.
    /// </summary>
    /// <param name="stepsRecords">The steps records to format; must not be <c>null</c> or empty.</param>
    /// <returns>CSV string for the supplied steps data.</returns>
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

        await using var writer = new StringWriter();
        await using var csv = new CsvWriter(writer, InvariantConfig);

        // Header
        csv.WriteField("Date");
        csv.WriteField("StepCount");
        csv.WriteField("Distance");
        csv.WriteField("Calories");
        csv.WriteField("DeviceType");
        await csv.NextRecordAsync().ConfigureAwait(false);

        // Rows
        foreach (var record in stepsRecords)
        {
            csv.WriteField(FormatDate(record.RecordDate));
            csv.WriteField(record.TotalSteps);
            csv.WriteField(record.DistanceKm);
            csv.WriteField(record.CaloriesBurned);
            csv.WriteField(ValidateFieldLength(Sanitize(record.DeviceId), nameof(record.DeviceId)));
            await csv.NextRecordAsync().ConfigureAwait(false);
        }

        _logger.LogInformation("Formatted {Count} steps records to CSV", stepsRecords.Count);
        return writer.ToString();
    }

    /// <summary>
    /// Validates a collection of <see cref="HealthDataRecord"/> before CSV export.
    /// </summary>
    /// <param name="records">The records to format; must not be <c>null</c>.</param>
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

    // ------------------------------------------------------------------------
    // Private helpers
    // ------------------------------------------------------------------------

    /// <summary>
    /// Formats a <see cref="DateTime"/> as an ISO‑8601 string using invariant culture.
    /// </summary>
    private static string FormatDate(DateTime date) => date.ToString("o", CultureInfo.InvariantCulture);

    /// <summary>
    /// Sanitises a string field to mitigate CSV injection attacks.
    ///
    /// <para>Protects against:</para>
    /// <list type="bullet">
    /// <item>Excel formula injection (values starting with =, +, -)</item>
    /// <item>DDE injection (values starting with @)</item>
    /// <item>Tab character injection (values starting with tab)</item>
    /// <item>Carriage return injection (values starting with \r)</item>
    /// <item>Row splitting via embedded newlines (\r\n, \n, \r)</item>
    /// <item>Embedded double quotes that could break CSV quoting</item>
    /// </list>
    ///
    /// <para>Sanitization rules:</para>
    /// <list type="bullet">
    /// <item>Values starting with =, +, -, @, or tab are prefixed with a single quote</item>
    /// <item>Values containing embedded newlines (\r\n, \n, \r) have newlines replaced with spaces</item>
    /// <item>Values containing embedded double quotes have them escaped per RFC 4180</item>
    /// <item>Empty or null values return empty string</item>
    /// </list>
    /// </summary>
    /// <param name="value">The value to sanitize; can be null.</param>
    /// <returns>The sanitized value safe for CSV export.</returns>
    private static string Sanitize(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        // Check if value starts with dangerous characters that could trigger formula execution
        bool startsWithDangerousChar = value.StartsWith('=') ||
                                      value.StartsWith('+') ||
                                      value.StartsWith('-') ||
                                      value.StartsWith('@') ||
                                      value.StartsWith('\t') ||
                                      value.StartsWith('\r');

        // Replace embedded newlines with spaces to prevent row splitting
        // Handle all common newline sequences: \r\n (Windows), \n (Unix), \r (old Mac)
        string sanitized = value
            .Replace("\r\n", " ")
            .Replace('\n', ' ')
            .Replace('\r', ' ');

        // Escape embedded double quotes per RFC 4180
        sanitized = sanitized.Replace("\"", "\"\"");

        // Apply prefix if value starts with dangerous character
        return startsWithDangerousChar ? $"'{sanitized}" : sanitized;
    }

    /// <summary>
    /// Ensures that a CSV field does not exceed the configured maximum length.
    /// </summary>
    /// <param name="field">The field value after sanitisation.</param>
    /// <param name="fieldName">The name of the field (used for exception messages).</param>
    /// <returns>The original <paramref name="field"/> if it is within limits.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the field length exceeds <see cref="_maxFieldLength"/>.
    /// </exception>
    private string ValidateFieldLength(string field, string fieldName)
    {
        if (field.Length > _maxFieldLength)
            throw new ArgumentException($"Field '{fieldName}' exceeds the maximum allowed length of {_maxFieldLength} characters.", fieldName);
        return field;
    }
}
