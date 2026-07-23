#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using HealthDataExportTools.Domain.Models;
using Microsoft.Extensions.Logging;

namespace HealthDataExportTools.Formatters;

public sealed partial class JsonFormatter
{
    /// <summary>
    /// Writes a collection of health data records to a stream as JSON array asynchronously.
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

        await writer.WriteAsync("[").ConfigureAwait(false);

        bool first = true;
        foreach (var record in records)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!first)
            {
                await writer.WriteAsync(",").ConfigureAwait(false);
            }
            first = false;

            var jsonObject = new
            {
                RecordDate = record.RecordDate,
                MetricType = record.GetType().Name,
                DeviceType = record.DeviceId,
                Value = string.Empty
            };

            var json = JsonSerializer.Serialize(jsonObject, _jsonOptions);
            await writer.WriteAsync(json).ConfigureAwait(false);
        }

        await writer.WriteAsync("]").ConfigureAwait(false);
        await writer.FlushAsync().ConfigureAwait(false);

        _logger.LogInformation("Streamed {Count} records to JSON", records.Count());
    }

    /// <summary>
    /// Writes sleep data to a stream as JSON array asynchronously.
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

        await writer.WriteAsync("[").ConfigureAwait(false);

        bool first = true;
        foreach (var record in sleepRecords)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!first)
            {
                await writer.WriteAsync(",").ConfigureAwait(false);
            }
            first = false;

            var jsonObject = new
            {
                Date = record.RecordDate,
                record.DurationMinutes,
                record.Quality,
                record.DeepSleepMinutes,
                record.RemSleepMinutes,
                record.AwakeMinutes,
                DeviceType = record.DeviceId
            };

            var json = JsonSerializer.Serialize(jsonObject, _jsonOptions);
            await writer.WriteAsync(json).ConfigureAwait(false);
        }

        await writer.WriteAsync("]").ConfigureAwait(false);
        await writer.FlushAsync().ConfigureAwait(false);

        _logger.LogInformation("Streamed {Count} sleep records to JSON", sleepRecords.Count());
    }

    /// <summary>
    /// Writes heart rate data to a stream as JSON array asynchronously.
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

        await writer.WriteAsync("[").ConfigureAwait(false);

        bool first = true;
        foreach (var record in heartRateRecords)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!first)
            {
                await writer.WriteAsync(",").ConfigureAwait(false);
            }
            first = false;

            var jsonObject = new
            {
                Timestamp = record.RecordDate,
                HeartRate = record.AverageBpm,
                HeartRateZone = string.Empty,
                DeviceType = record.DeviceId
            };

            var json = JsonSerializer.Serialize(jsonObject, _jsonOptions);
            await writer.WriteAsync(json).ConfigureAwait(false);
        }

        await writer.WriteAsync("]").ConfigureAwait(false);
        await writer.FlushAsync().ConfigureAwait(false);

        _logger.LogInformation("Streamed {Count} heart rate records to JSON", heartRateRecords.Count());
    }

    /// <summary>
    /// Writes SpO2 data to a stream as JSON array asynchronously.
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

        await writer.WriteAsync("[").ConfigureAwait(false);

        bool first = true;
        foreach (var record in spo2Records)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!first)
            {
                await writer.WriteAsync(",").ConfigureAwait(false);
            }
            first = false;

            var jsonObject = new
            {
                Timestamp = record.RecordDate,
                SpO2 = record.AveragePercentage,
                IsLowOxygen = record.HasConcerningLevels(),
                DeviceType = record.DeviceId
            };

            var json = JsonSerializer.Serialize(jsonObject, _jsonOptions);
            await writer.WriteAsync(json).ConfigureAwait(false);
        }

        await writer.WriteAsync("]").ConfigureAwait(false);
        await writer.FlushAsync().ConfigureAwait(false);

        _logger.LogInformation("Streamed {Count} SpO2 records to JSON", spo2Records.Count());
    }

    /// <summary>
    /// Writes steps data to a stream as JSON array asynchronously.
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

        await writer.WriteAsync("[").ConfigureAwait(false);

        bool first = true;
        foreach (var record in stepsRecords)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!first)
            {
                await writer.WriteAsync(",").ConfigureAwait(false);
            }
            first = false;

            var jsonObject = new
            {
                Date = record.RecordDate,
                StepCount = record.TotalSteps,
                Distance = record.DistanceKm,
                Calories = record.CaloriesBurned,
                DeviceType = record.DeviceId
            };

            var json = JsonSerializer.Serialize(jsonObject, _jsonOptions);
            await writer.WriteAsync(json).ConfigureAwait(false);
        }

        await writer.WriteAsync("]").ConfigureAwait(false);
        await writer.FlushAsync().ConfigureAwait(false);

        _logger.LogInformation("Streamed {Count} steps records to JSON", stepsRecords.Count());
    }
}