#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using HealthDataExportTools.Domain.Models;
using HealthDataExportTools.Exceptions;

namespace HealthDataExportTools.Exporters;

/// <summary>
/// Describes a single discrepancy found while verifying an export against its manifest.
/// </summary>
/// <param name="FileName">Name of the file the mismatch was found in.</param>
/// <param name="Description">Human-readable description of what did not match.</param>
public sealed record ExportManifestMismatch(string FileName, string Description);

/// <summary>
/// Outcome of running <see cref="ExportManifestWriter.VerifyAsync"/> against a previously written manifest.
/// </summary>
public sealed class ExportManifestVerificationResult
{
    /// <summary>
    /// True when every file listed in the manifest is present and its checksum, size, and line
    /// count still match what was recorded at export time.
    /// </summary>
    public bool IsValid => Mismatches.Count == 0;

    /// <summary>
    /// The manifest that was verified.
    /// </summary>
    public required ExportManifest Manifest { get; init; }

    /// <summary>
    /// All discrepancies found. Empty when verification succeeded.
    /// </summary>
    public List<ExportManifestMismatch> Mismatches { get; init; } = [];
}

/// <summary>
/// Writes and verifies "manifest.json" sidecar files that accompany health data exports.
/// The manifest records per-file SHA-256 checksums, per-data-type record counts, the covered
/// time range, tool version, and export timestamp, enabling later integrity verification of
/// archived or shared medical data without needing the original source files.
/// </summary>
public sealed class ExportManifestWriter
{
    /// <summary>
    /// Default file name used for the manifest sidecar.
    /// </summary>
    public const string ManifestFileName = "manifest.json";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
    };

    /// <summary>
    /// Build a manifest describing <paramref name="exportedFilePaths"/> and the records in
    /// <paramref name="collection"/>, then write it as "manifest.json" inside <paramref name="outputDirectory"/>.
    /// </summary>
    /// <param name="collection">The health data collection that was exported. Must not be null.</param>
    /// <param name="outputDirectory">Directory containing the exported files and where the manifest will be written. Must not be null or whitespace.</param>
    /// <param name="exportedFilePaths">Paths of the files produced by the export. Must not be null or empty.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The manifest that was written.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="collection"/> or <paramref name="exportedFilePaths"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="outputDirectory"/> is null or whitespace, or <paramref name="exportedFilePaths"/> is empty.</exception>
    /// <exception cref="ExportException">Thrown when the manifest cannot be written due to an I/O or access error.</exception>
    public async Task<ExportManifest> WriteAsync(
        HealthDataCollection collection,
        string outputDirectory,
        IReadOnlyCollection<string> exportedFilePaths,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(collection);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);
        ArgumentNullException.ThrowIfNull(exportedFilePaths);
        if (exportedFilePaths.Count == 0)
            throw new ArgumentException("At least one exported file path must be provided.", nameof(exportedFilePaths));

        var manifest = new ExportManifest
        {
            ToolVersion = GetToolVersion(),
            ExportedAtUtc = DateTime.UtcNow,
            RecordCountsByDataType = BuildRecordCounts(collection),
            TotalRecordCount = collection.GetTotalRecordCount(),
        };

        (manifest.TimeRangeStartUtc, manifest.TimeRangeEndUtc) = ComputeTimeRange(collection);

        foreach (var filePath in exportedFilePaths)
        {
            cancellationToken.ThrowIfCancellationRequested();
            manifest.Files.Add(await BuildFileEntryAsync(filePath, cancellationToken).ConfigureAwait(false));
        }

        var manifestPath = Path.Combine(outputDirectory, ManifestFileName);
        try
        {
            Directory.CreateDirectory(outputDirectory);
            var json = JsonSerializer.Serialize(manifest, SerializerOptions);
            await File.WriteAllTextAsync(manifestPath, json, cancellationToken).ConfigureAwait(false);
        }
        catch (IOException ex)
        {
            throw new ExportException("Failed to write export manifest", manifestPath, "manifest", ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new ExportException("Access denied when writing export manifest", manifestPath, "manifest", ex);
        }

        return manifest;
    }

    /// <summary>
    /// Read a previously written manifest and recompute the checksum, size, and line count of
    /// each referenced file to confirm the export has not been corrupted or tampered with since
    /// it was archived.
    /// </summary>
    /// <param name="manifestPath">Path to the "manifest.json" file to verify. Must not be null or whitespace.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A verification result listing any mismatches found; <see cref="ExportManifestVerificationResult.IsValid"/> is true when none were found.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="manifestPath"/> is null or whitespace.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the manifest file does not exist.</exception>
    /// <exception cref="ExportException">Thrown when the manifest cannot be read or does not contain valid JSON.</exception>
    public async Task<ExportManifestVerificationResult> VerifyAsync(
        string manifestPath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(manifestPath);

        if (!File.Exists(manifestPath))
            throw new FileNotFoundException("Manifest file not found", manifestPath);

        ExportManifest? manifest;
        try
        {
            var json = await File.ReadAllTextAsync(manifestPath, cancellationToken).ConfigureAwait(false);
            manifest = JsonSerializer.Deserialize<ExportManifest>(json, SerializerOptions);
        }
        catch (IOException ex)
        {
            throw new ExportException("Failed to read export manifest", manifestPath, "manifest", ex);
        }
        catch (JsonException ex)
        {
            throw new ExportException("Export manifest is not valid JSON", manifestPath, "manifest", ex);
        }

        if (manifest is null)
            throw new ExportException($"Export manifest deserialized to null: {manifestPath}");

        var manifestDirectory = Path.GetDirectoryName(Path.GetFullPath(manifestPath)) ?? ".";
        var result = new ExportManifestVerificationResult { Manifest = manifest };

        foreach (var expected in manifest.Files)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var filePath = Path.Combine(manifestDirectory, expected.FileName);

            if (!File.Exists(filePath))
            {
                result.Mismatches.Add(new ExportManifestMismatch(expected.FileName, "File is missing"));
                continue;
            }

            var actual = await BuildFileEntryAsync(filePath, cancellationToken).ConfigureAwait(false);

            if (!string.Equals(actual.Sha256, expected.Sha256, StringComparison.OrdinalIgnoreCase))
            {
                result.Mismatches.Add(new ExportManifestMismatch(
                    expected.FileName,
                    $"SHA-256 mismatch: expected {expected.Sha256}, found {actual.Sha256}"));
            }

            if (actual.SizeBytes != expected.SizeBytes)
            {
                result.Mismatches.Add(new ExportManifestMismatch(
                    expected.FileName,
                    $"Size mismatch: expected {expected.SizeBytes} bytes, found {actual.SizeBytes} bytes"));
            }

            if (expected.LineCount.HasValue && actual.LineCount != expected.LineCount)
            {
                result.Mismatches.Add(new ExportManifestMismatch(
                    expected.FileName,
                    $"Line/record count mismatch: expected {expected.LineCount}, found {actual.LineCount}"));
            }
        }

        return result;
    }

    /// <summary>
    /// Compute a checksum-bearing manifest entry for a single file on disk.
    /// </summary>
    private static async Task<ExportManifestFileEntry> BuildFileEntryAsync(string filePath, CancellationToken cancellationToken)
    {
        var fileInfo = new FileInfo(filePath);

        long lineCount = 0;
        using (var sha256 = SHA256.Create())
        await using (var stream = File.OpenRead(filePath))
        {
            var hashBytes = await sha256.ComputeHashAsync(stream, cancellationToken).ConfigureAwait(false);
            var hash = Convert.ToHexString(hashBytes).ToLowerInvariant();

            stream.Position = 0;
            using var reader = new StreamReader(stream, leaveOpen: true);
            while (await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false) is { } line)
            {
                if (!string.IsNullOrWhiteSpace(line))
                    lineCount++;
            }

            return new ExportManifestFileEntry
            {
                FileName = Path.GetFileName(filePath),
                Sha256 = hash,
                SizeBytes = fileInfo.Length,
                LineCount = lineCount,
            };
        }
    }

    /// <summary>
    /// Build the per-data-type record counts for a health data collection.
    /// </summary>
    private static Dictionary<string, int> BuildRecordCounts(HealthDataCollection collection) =>
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["Sleep"] = collection.SleepRecords.Count,
            ["HeartRate"] = collection.HeartRateRecords.Count,
            ["SpO2"] = collection.SpO2Records.Count,
            ["Steps"] = collection.StepsRecords.Count,
            ["Activity"] = collection.ActivityRecords.Count,
        };

    /// <summary>
    /// Compute the min/max <c>RecordDate</c> across every record type in the collection.
    /// </summary>
    private static (DateTime? Start, DateTime? End) ComputeTimeRange(HealthDataCollection collection)
    {
        var dates = collection.SleepRecords.Select(r => r.RecordDate)
            .Concat(collection.HeartRateRecords.Select(r => r.RecordDate))
            .Concat(collection.SpO2Records.Select(r => r.RecordDate))
            .Concat(collection.StepsRecords.Select(r => r.RecordDate))
            .Concat(collection.ActivityRecords.Select(r => r.RecordDate))
            .ToList();

        return dates.Count == 0 ? (null, null) : (dates.Min(), dates.Max());
    }

    /// <summary>
    /// Resolve the running assembly's informational version for inclusion in the manifest.
    /// </summary>
    private static string GetToolVersion() =>
        Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        ?? Assembly.GetExecutingAssembly().GetName().Version?.ToString()
        ?? "unknown";
}
