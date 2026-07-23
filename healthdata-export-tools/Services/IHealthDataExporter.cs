#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using HealthDataExportTools.Domain.Models;

namespace HealthDataExportTools.Services;

/// <summary>
/// Defines the contract for exporting a <see cref="HealthDataCollection"/> to CSV files.
/// </summary>
public interface IHealthDataExporter
{
    /// <summary>
    /// Export all enabled data types from <paramref name="collection"/> to separate CSV files
    /// inside <paramref name="outputDirectory"/>.
    /// </summary>
    /// <param name="collection">The health data to export.</param>
    /// <param name="outputDirectory">Directory where the CSV files will be written.</param>
    /// <param name="options">
    /// Optional export options controlling which data types and columns to include,
    /// and the date format to use. When <c>null</c>, all data types are exported
    /// with ISO 8601 date formatting.
    /// </param>
    Task ExportToCsvAsync(HealthDataCollection collection, string outputDirectory, CsvExportOptions? options = null);

    /// <summary>
    /// Export all enabled data types from <paramref name="collection"/> to a single CSV stream asynchronously.
    /// This streaming approach avoids building the entire payload in memory, making it suitable for large datasets.
    /// </summary>
    /// <param name="collection">The health data to export.</param>
    /// <param name="writer">The text writer to write to; must not be <c>null</c>.</param>
    /// <param name="options">
    /// Optional export options controlling which data types and columns to include,
    /// and the date format to use. When <c>null</c>, all data types are exported
    /// with ISO 8601 date formatting.
    /// </param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer"/> is <c>null</c>.</exception>
    Task ExportToCsvStreamAsync(
        HealthDataCollection collection,
        TextWriter writer,
        CsvExportOptions? options = null,
        CancellationToken cancellationToken = default);
}
