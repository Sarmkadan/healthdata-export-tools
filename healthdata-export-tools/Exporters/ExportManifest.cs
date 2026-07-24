#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace HealthDataExportTools.Exporters;

/// <summary>
/// Describes a single output file produced by an export, together with the
/// information needed to later verify its integrity and shape without
/// re-parsing the original source data.
/// </summary>
public sealed class ExportManifestFileEntry
{
    /// <summary>
    /// File name of the exported artifact, relative to the manifest's own directory.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Lowercase hexadecimal SHA-256 digest of the file content.
    /// </summary>
    public string Sha256 { get; set; } = string.Empty;

    /// <summary>
    /// Size of the file in bytes at the time the manifest was written.
    /// </summary>
    public long SizeBytes { get; set; }

    /// <summary>
    /// Count of non-empty lines in the file. Used as a cheap, format-agnostic
    /// proxy for the number of records the file contains (one record per line
    /// for JSON Lines exports, one row per data record plus a header for CSV).
    /// Null for files where a line count is not a meaningful measure (e.g. binary formats).
    /// </summary>
    public long? LineCount { get; set; }
}

/// <summary>
/// A sidecar manifest describing the outcome of an export: which files were
/// produced, their checksums, how many records of each health data type were
/// exported, the time range those records cover, and when/with what tool
/// version the export ran. Written as "manifest.json" next to the export output
/// and used for later integrity verification via the CLI 'verify' subcommand.
/// </summary>
public sealed class ExportManifest
{
    /// <summary>
    /// Version of the manifest schema itself, independent of the tool version.
    /// Bump when the shape of this class changes in a breaking way.
    /// </summary>
    public int SchemaVersion { get; set; } = 1;

    /// <summary>
    /// Version of healthdata-export-tools that produced this export.
    /// </summary>
    public string ToolVersion { get; set; } = string.Empty;

    /// <summary>
    /// UTC timestamp at which the export completed and the manifest was generated.
    /// </summary>
    public DateTime ExportedAtUtc { get; set; }

    /// <summary>
    /// Earliest record timestamp (<c>RecordDate</c>) covered by the export, across all data types.
    /// Null when the export contains no records.
    /// </summary>
    public DateTime? TimeRangeStartUtc { get; set; }

    /// <summary>
    /// Latest record timestamp (<c>RecordDate</c>) covered by the export, across all data types.
    /// Null when the export contains no records.
    /// </summary>
    public DateTime? TimeRangeEndUtc { get; set; }

    /// <summary>
    /// Number of exported records per health data type (e.g. "Sleep", "HeartRate", "SpO2", "Steps", "Activity").
    /// </summary>
    public Dictionary<string, int> RecordCountsByDataType { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Per-file checksum and size entries for every output file this export produced.
    /// </summary>
    public List<ExportManifestFileEntry> Files { get; set; } = [];

    /// <summary>
    /// Total record count across all data types, provided as a convenience for fast pre-checks.
    /// </summary>
    public int TotalRecordCount { get; set; }
}
