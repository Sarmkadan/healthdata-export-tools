#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Threading;
using HealthDataExportTools.Domain.Models;

namespace HealthDataExportTools.Services;

/// <summary>
/// Defines the contract for exporting health data to files.
/// Implementations should handle file operations including atomic writes via temporary files.
/// </summary>
public interface IDataExporter
{
    /// <summary>
    /// Export health data records to the specified destination file.
    /// </summary>
    /// <param name="collection">The health data collection to export. Must not be null.</param>
    /// <param name="destination">The output file path. Must not be null or whitespace.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="collection"/> or <paramref name="destination"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="destination"/> is empty or whitespace.</exception>
    Task ExportAsync(HealthDataCollection collection, string destination, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the file extension for this exporter's output format.
    /// </summary>
    string FileExtension { get; }

    /// <summary>
    /// Gets a human-readable description of the export format.
    /// </summary>
    string FormatDescription { get; }
}
