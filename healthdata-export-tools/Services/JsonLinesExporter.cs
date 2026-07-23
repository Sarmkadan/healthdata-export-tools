#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text;
using System.Text.Json;
using HealthDataExportTools.Domain.Models;
using HealthDataExportTools.Exceptions;

namespace HealthDataExportTools.Services;

/// <summary>
/// Exports a <see cref="HealthDataCollection"/> to JSON Lines format.
/// Each record is written as a separate line in the output file, enabling
/// efficient streaming and processing of large datasets.
/// </summary>
public sealed class JsonLinesExporter : IDataExporter
{
    private readonly ILogger<JsonLinesExporter> _logger;

    public JsonLinesExporter(ILogger<JsonLinesExporter> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Export all enabled data types from <paramref name="collection"/> to a single JSON Lines file
    /// at <paramref name="outputPath"/>.
    /// </summary>
    /// <param name="collection">The health data to export.</param>
    /// <param name="outputPath">Path to the output JSON Lines file.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    public async Task ExportToJsonLinesAsync(
        HealthDataCollection collection,
        string outputPath,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(collection);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        try
        {
            using var fs = File.Create(outputPath);
            using var writer = new StreamWriter(fs, Encoding.UTF8);
            var options = new JsonSerializerOptions
            {
                WriteIndented = false,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            // Export sleep records
            foreach (var record in collection.SleepRecords)
            {
                var json = JsonSerializer.Serialize(record, options);
                await writer.WriteLineAsync(json).ConfigureAwait(false);
            }

            // Export heart rate records
            foreach (var record in collection.HeartRateRecords)
            {
                var json = JsonSerializer.Serialize(record, options);
                await writer.WriteLineAsync(json).ConfigureAwait(false);
            }

            // Export SpO2 records
            foreach (var record in collection.SpO2Records)
            {
                var json = JsonSerializer.Serialize(record, options);
                await writer.WriteLineAsync(json).ConfigureAwait(false);
            }

            // Export steps records
            foreach (var record in collection.StepsRecords)
            {
                var json = JsonSerializer.Serialize(record, options);
                await writer.WriteLineAsync(json).ConfigureAwait(false);
            }

            // Export activity records
            foreach (var record in collection.ActivityRecords)
            {
                var json = JsonSerializer.Serialize(record, options);
                await writer.WriteLineAsync(json).ConfigureAwait(false);
            }

            await writer.FlushAsync().ConfigureAwait(false);

            _logger.LogInformation(
                "JSON Lines export complete: {RecordCount} records written to {OutputPath}",
                collection.GetTotalRecordCount(),
                outputPath);
        }
        catch (IOException ex)
        {
            throw new ExportException("Failed to write JSON Lines file", outputPath, "JSONL", ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new ExportException("Access denied when writing JSON Lines file", outputPath, "JSONL", ex);
        }
    }

    /// <inheritdoc />
    public async Task ExportAsync(
        HealthDataCollection collection,
        string destination,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(collection);
        ArgumentException.ThrowIfNullOrWhiteSpace(destination);

        await ExportToJsonLinesAsync(collection, destination, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the file extension for JSON Lines format.
    /// </summary>
    public string FileExtension => ".jsonl";

    /// <summary>
    /// Gets a human-readable description of the JSON Lines export format.
    /// </summary>
    public string FormatDescription => "JSON Lines format (one JSON object per line)";
}
