#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using HealthDataExportTools.Domain.Models;
using Microsoft.Extensions.Logging;

namespace HealthDataExportTools.Formatters;

/// <summary>
/// Formats health data into JSON Lines format (one JSON object per line).
/// This implementation provides consistent error handling with other formatters
/// (CsvFormatter, JsonFormatter) by using guard clauses and consistent behavior
/// for edge cases like null collections, empty collections, and record limits.
/// </summary>
public sealed partial class JsonLinesFormatter : IDataFormatter
{
    private readonly ILogger<JsonLinesFormatter> _logger;
    private readonly int _maxRecordCount;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    /// <summary>
    /// File extension for JSON Lines files (.jsonl).
    /// </summary>
    public string FileExtension => ".jsonl";

    /// <summary>
    /// Human-readable name of the format.
    /// </summary>
    public string FormatName => "JSON Lines";

    /// <summary>
    /// Initializes a new instance of <see cref="JsonLinesFormatter"/>.
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
    public JsonLinesFormatter(ILogger<JsonLinesFormatter> logger, int maxRecordCount = 100_000)
    {
        ArgumentNullException.ThrowIfNull(logger);
        if (maxRecordCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxRecordCount), "Maximum record count must be greater than zero.");

        _logger = logger;
        _maxRecordCount = maxRecordCount;
    }

    /// <summary>
    /// Determines whether the supplied <paramref name="dataType"/> can be formatted as JSON Lines.
    /// </summary>
    /// <param name="dataType">The type to evaluate; must not be <c>null</c>.</param>
    /// <returns><c>true</c> if the type is a health-data record; otherwise <c>false</c>.</returns>
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
    /// Formats a single <see cref="HealthDataRecord"/> as a JSON Lines record.
    /// </summary>
    /// <param name="record">The record to format; must not be <c>null</c>.</param>
    /// <returns>A JSON string containing the serialized record.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="record"/> is <c>null</c>.</exception>
    public async Task<string> FormatAsync(HealthDataRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        var json = JsonSerializer.Serialize(record, JsonOptions);
        _logger.LogDebug("Formatted single record to JSON Lines");
        return json;
    }

    /// <summary>
    /// Formats a collection of <see cref="HealthDataRecord"/> instances as JSON Lines.
    /// </summary>
    /// <param name="records">The collection to format; must not be <c>null</c>.</param>
    /// <returns>JSON Lines representation of the collection.</returns>
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
            _logger.LogWarning("Empty record collection provided to JSON Lines formatter");
            return string.Empty;
        }

        var sb = new System.Text.StringBuilder();
        foreach (var record in records)
        {
            var json = JsonSerializer.Serialize(record, JsonOptions);
            sb.AppendLine(json);
        }

        _logger.LogInformation("Formatted {Count} records to JSON Lines", records.Count);
        return sb.ToString();
    }

    /// <summary>
    /// Formats a list of <see cref="SleepData"/> records into JSON Lines format.
    /// </summary>
    /// <param name="sleepRecords">The sleep records to format; must not be <c>null</c>.</param>
    /// <returns>JSON Lines string for the supplied sleep data.</returns>
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

        var sb = new System.Text.StringBuilder();
        foreach (var record in sleepRecords)
        {
            var json = JsonSerializer.Serialize(record, JsonOptions);
            sb.AppendLine(json);
        }

        _logger.LogInformation("Formatted {Count} sleep records to JSON Lines", sleepRecords.Count);
        return sb.ToString();
    }

    /// <summary>
    /// Formats a list of <see cref="HeartRateData"/> records into JSON Lines format.
    /// </summary>
    /// <param name="heartRateRecords">The heart-rate records to format; must not be <c>null</c>.</param>
    /// <returns>JSON Lines string for the supplied heart-rate data.</returns>
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

        var sb = new System.Text.StringBuilder();
        foreach (var record in heartRateRecords)
        {
            var json = JsonSerializer.Serialize(record, JsonOptions);
            sb.AppendLine(json);
        }

        _logger.LogInformation("Formatted {Count} heart rate records to JSON Lines", heartRateRecords.Count);
        return sb.ToString();
    }

    /// <summary>
    /// Formats a list of <see cref="SpO2Data"/> records into JSON Lines format.
    /// </summary>
    /// <param name="spo2Records">The SpO2 records to format; must not be <c>null</c>.</param>
    /// <returns>JSON Lines string for the supplied SpO2 data.</returns>
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

        var sb = new System.Text.StringBuilder();
        foreach (var record in spo2Records)
        {
            var json = JsonSerializer.Serialize(record, JsonOptions);
            sb.AppendLine(json);
        }

        _logger.LogInformation("Formatted {Count} SpO2 records to JSON Lines", spo2Records.Count);
        return sb.ToString();
    }

    /// <summary>
    /// Formats a list of <see cref="StepsData"/> records into JSON Lines format.
    /// </summary>
    /// <param name="stepsRecords">The steps records to format; must not be <c>null</c>.</param>
    /// <returns>JSON Lines string for the supplied steps data.</returns>
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

        var sb = new System.Text.StringBuilder();
        foreach (var record in stepsRecords)
        {
            var json = JsonSerializer.Serialize(record, JsonOptions);
            sb.AppendLine(json);
        }

        _logger.LogInformation("Formatted {Count} steps records to JSON Lines", stepsRecords.Count);
        return sb.ToString();
    }

    /// <summary>
    /// Validates a collection of <see cref="HealthDataRecord"/> before JSON Lines export.
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

    /// <summary>
    /// Write a collection of health data records to a stream as JSON Lines asynchronously.
    /// </summary>
    /// <param name="records">The records to format; must not be <c>null</c>.</param>
    /// <param name="writer">The text writer to write to; must not be <c>null</c>.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="records"/> or <paramref name="writer"/> is <c>null</c>.</exception>
    public async Task WriteAsync(IEnumerable<HealthDataRecord> records, TextWriter writer, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(records);
        ArgumentNullException.ThrowIfNull(writer);

        var recordList = records as List<HealthDataRecord> ?? new List<HealthDataRecord>(records);

        if (recordList.Count > _maxRecordCount)
            throw new ArgumentException($"Record collection exceeds the maximum allowed count of {_maxRecordCount}.", nameof(records));

        foreach (var record in recordList)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var json = JsonSerializer.Serialize(record, JsonOptions);
            await writer.WriteLineAsync(json).ConfigureAwait(false);
        }

        _logger.LogInformation("Wrote {Count} records to JSON Lines stream", recordList.Count);
    }

    /// <summary>
    /// Write sleep data to a stream as JSON Lines asynchronously.
    /// </summary>
    /// <param name="sleepRecords">The sleep records to format; must not be <c>null</c>.</param>
    /// <param name="writer">The text writer to write to; must not be <c>null</c>.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepRecords"/> or <paramref name="writer"/> is <c>null</c>.</exception>
    public async Task WriteSleepDataAsync(IEnumerable<SleepData> sleepRecords, TextWriter writer, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sleepRecords);
        ArgumentNullException.ThrowIfNull(writer);

        var recordList = sleepRecords as List<SleepData> ?? new List<SleepData>(sleepRecords);

        if (recordList.Count > _maxRecordCount)
            throw new ArgumentException($"Sleep record collection exceeds the maximum allowed count of {_maxRecordCount}.", nameof(sleepRecords));

        foreach (var record in recordList)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var json = JsonSerializer.Serialize(record, JsonOptions);
            await writer.WriteLineAsync(json).ConfigureAwait(false);
        }

        _logger.LogInformation("Wrote {Count} sleep records to JSON Lines stream", recordList.Count);
    }

    /// <summary>
    /// Write heart rate data to a stream as JSON Lines asynchronously.
    /// </summary>
    /// <param name="heartRateRecords">The heart-rate records to format; must not be <c>null</c>.</param>
    /// <param name="writer">The text writer to write to; must not be <c>null</c>.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="heartRateRecords"/> or <paramref name="writer"/> is <c>null</c>.</exception>
    public async Task WriteHeartRateDataAsync(IEnumerable<HeartRateData> heartRateRecords, TextWriter writer, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(heartRateRecords);
        ArgumentNullException.ThrowIfNull(writer);

        var recordList = heartRateRecords as List<HeartRateData> ?? new List<HeartRateData>(heartRateRecords);

        if (recordList.Count > _maxRecordCount)
            throw new ArgumentException($"Heart-rate record collection exceeds the maximum allowed count of {_maxRecordCount}.", nameof(heartRateRecords));

        foreach (var record in recordList)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var json = JsonSerializer.Serialize(record, JsonOptions);
            await writer.WriteLineAsync(json).ConfigureAwait(false);
        }

        _logger.LogInformation("Wrote {Count} heart rate records to JSON Lines stream", recordList.Count);
    }

    /// <summary>
    /// Write SpO2 data to a stream as JSON Lines asynchronously.
    /// </summary>
    /// <param name="spo2Records">The SpO2 records to format; must not be <c>null</c>.</param>
    /// <param name="writer">The text writer to write to; must not be <c>null</c>.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="spo2Records"/> or <paramref name="writer"/> is <c>null</c>.</exception>
    public async Task WriteSpO2DataAsync(IEnumerable<SpO2Data> spo2Records, TextWriter writer, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(spo2Records);
        ArgumentNullException.ThrowIfNull(writer);

        var recordList = spo2Records as List<SpO2Data> ?? new List<SpO2Data>(spo2Records);

        if (recordList.Count > _maxRecordCount)
            throw new ArgumentException($"SpO2 record collection exceeds the maximum allowed count of {_maxRecordCount}.", nameof(spo2Records));

        foreach (var record in recordList)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var json = JsonSerializer.Serialize(record, JsonOptions);
            await writer.WriteLineAsync(json).ConfigureAwait(false);
        }

        _logger.LogInformation("Wrote {Count} SpO2 records to JSON Lines stream", recordList.Count);
    }

    /// <summary>
    /// Write steps data to a stream as JSON Lines asynchronously.
    /// </summary>
    /// <param name="stepsRecords">The steps records to format; must not be <c>null</c>.</param>
    /// <param name="writer">The text writer to write to; must not be <c>null</c>.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="stepsRecords"/> or <paramref name="writer"/> is <c>null</c>.</exception>
    public async Task WriteStepsDataAsync(IEnumerable<StepsData> stepsRecords, TextWriter writer, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stepsRecords);
        ArgumentNullException.ThrowIfNull(writer);

        var recordList = stepsRecords as List<StepsData> ?? new List<StepsData>(stepsRecords);

        if (recordList.Count > _maxRecordCount)
            throw new ArgumentException($"Steps record collection exceeds the maximum allowed count of {_maxRecordCount}.", nameof(stepsRecords));

        foreach (var record in recordList)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var json = JsonSerializer.Serialize(record, JsonOptions);
            await writer.WriteLineAsync(json).ConfigureAwait(false);
        }

        _logger.LogInformation("Wrote {Count} steps records to JSON Lines stream", recordList.Count);
    }
}
