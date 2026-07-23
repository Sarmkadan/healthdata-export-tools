#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using HealthDataExportTools.Domain.Models;
using Microsoft.Extensions.Logging;

namespace HealthDataExportTools.Formatters;

public sealed partial class CsvFormatter
{
    /// <summary>
    /// Writes a collection of health data records to a stream as CSV asynchronously.
    /// This streaming approach avoids building the entire payload in memory, making it suitable for large datasets.
    /// </summary>
    /// <param name="records">The records to format; must not be <c>null</c>.</param>
    /// <param name="writer">The text writer to write to; must not be <c>null</c>.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="records"/> or <paramref name="writer"/> is <c>null</c>.</exception>
    public async Task WriteAsync(
        IEnumerable<HealthDataRecord> records,
        TextWriter writer,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(records);
        ArgumentNullException.ThrowIfNull(writer);

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
            cancellationToken.ThrowIfCancellationRequested();

            csv.WriteField(FormatDate(record.RecordDate));
            csv.WriteField(record.GetType().Name);
            csv.WriteField(Sanitize(record.DeviceId));
            csv.WriteField(string.Empty);
            await csv.NextRecordAsync().ConfigureAwait(false);
        }

        _logger.LogInformation("Streamed {Count} records to CSV", records.Count());
    }

    /// <summary>
    /// Writes sleep data to a stream as CSV asynchronously.
    /// </summary>
    /// <param name="sleepRecords">The sleep records to format; must not be <c>null</c>.</param>
    /// <param name="writer">The text writer to write to; must not be <c>null</c>.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepRecords"/> or <paramref name="writer"/> is <c>null</c>.</exception>
    public async Task WriteSleepDataAsync(
        IEnumerable<SleepData> sleepRecords,
        TextWriter writer,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sleepRecords);
        ArgumentNullException.ThrowIfNull(writer);

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
            cancellationToken.ThrowIfCancellationRequested();

            csv.WriteField(FormatDate(record.RecordDate));
            csv.WriteField(record.DurationMinutes);
            csv.WriteField(record.Quality);
            csv.WriteField(record.DeepSleepMinutes);
            csv.WriteField(record.RemSleepMinutes);
            csv.WriteField(record.AwakeMinutes);
            csv.WriteField(Sanitize(record.DeviceId));
            await csv.NextRecordAsync().ConfigureAwait(false);
        }

        _logger.LogInformation("Streamed {Count} sleep records to CSV", sleepRecords.Count());
    }

    /// <summary>
    /// Writes heart rate data to a stream as CSV asynchronously.
    /// </summary>
    /// <param name="heartRateRecords">The heart-rate records to format; must not be <c>null</c>.</param>
    /// <param name="writer">The text writer to write to; must not be <c>null</c>.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="heartRateRecords"/> or <paramref name="writer"/> is <c>null</c>.</exception>
    public async Task WriteHeartRateDataAsync(
        IEnumerable<HeartRateData> heartRateRecords,
        TextWriter writer,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(heartRateRecords);
        ArgumentNullException.ThrowIfNull(writer);

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
            cancellationToken.ThrowIfCancellationRequested();

            csv.WriteField(FormatDate(record.RecordDate));
            csv.WriteField(record.AverageBpm);
            csv.WriteField(string.Empty);
            csv.WriteField(Sanitize(record.DeviceId));
            await csv.NextRecordAsync().ConfigureAwait(false);
        }

        _logger.LogInformation("Streamed {Count} heart rate records to CSV", heartRateRecords.Count());
    }

    /// <summary>
    /// Writes SpO2 data to a stream as CSV asynchronously.
    /// </summary>
    /// <param name="spo2Records">The SpO2 records to format; must not be <c>null</c>.</param>
    /// <param name="writer">The text writer to write to; must not be <c>null</c>.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="spo2Records"/> or <paramref name="writer"/> is <c>null</c>.</exception>
    public async Task WriteSpO2DataAsync(
        IEnumerable<SpO2Data> spo2Records,
        TextWriter writer,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(spo2Records);
        ArgumentNullException.ThrowIfNull(writer);

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
            cancellationToken.ThrowIfCancellationRequested();

            csv.WriteField(FormatDate(record.RecordDate));
            csv.WriteField(record.AveragePercentage);
            csv.WriteField(record.HasConcerningLevels());
            csv.WriteField(Sanitize(record.DeviceId));
            await csv.NextRecordAsync().ConfigureAwait(false);
        }

        _logger.LogInformation("Streamed {Count} SpO2 records to CSV", spo2Records.Count());
    }

    /// <summary>
    /// Writes steps data to a stream as CSV asynchronously.
    /// </summary>
    /// <param name="stepsRecords">The steps records to format; must not be <c>null</c>.</param>
    /// <param name="writer">The text writer to write to; must not be <c>null</c>.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="stepsRecords"/> or <paramref name="writer"/> is <c>null</c>.</exception>
    public async Task WriteStepsDataAsync(
        IEnumerable<StepsData> stepsRecords,
        TextWriter writer,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stepsRecords);
        ArgumentNullException.ThrowIfNull(writer);

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
            cancellationToken.ThrowIfCancellationRequested();

            csv.WriteField(FormatDate(record.RecordDate));
            csv.WriteField(record.TotalSteps);
            csv.WriteField(record.DistanceKm);
            csv.WriteField(record.CaloriesBurned);
            csv.WriteField(Sanitize(record.DeviceId));
            await csv.NextRecordAsync().ConfigureAwait(false);
        }

        _logger.LogInformation("Streamed {Count} steps records to CSV", stepsRecords.Count());
    }
}