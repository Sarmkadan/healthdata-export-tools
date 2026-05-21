#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace HealthDataExportTools.DTOs;

/// <summary>
/// Data Transfer Object representing the result of an export operation.
/// Contains statistics about exported records, output files, timing, and any warnings or errors.
/// </summary>
public sealed class ExportResultDto
{
    /// <summary>Unique identifier for this export operation.</summary>
    [JsonPropertyName("exportId")]
    public string ExportId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Overall status of the export ("Success", "PartialSuccess", "Failed").</summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = "Success";

    /// <summary>Total number of health data records included in the export.</summary>
    [JsonPropertyName("recordsExported")]
    public int RecordsExported { get; set; }

    /// <summary>Number of records excluded by date range or type filters.</summary>
    [JsonPropertyName("recordsFiltered")]
    public int RecordsFiltered { get; set; }

    /// <summary>Directory or file path where output was written.</summary>
    [JsonPropertyName("outputPath")]
    public string OutputPath { get; set; } = string.Empty;

    /// <summary>Total output size in bytes across all generated files.</summary>
    [JsonPropertyName("outputSize")]
    public long OutputSizeBytes { get; set; }

    /// <summary>List of individual files generated during the export.</summary>
    [JsonPropertyName("generatedFiles")]
    public List<ExportedFile> GeneratedFiles { get; set; } = new();

    /// <summary>Formats used in this export (e.g., "csv", "json", "xml").</summary>
    [JsonPropertyName("exportedFormats")]
    public List<string> ExportedFormats { get; set; } = new();

    /// <summary>UTC timestamp when the export operation started.</summary>
    [JsonPropertyName("startTime")]
    public DateTime StartTime { get; set; }

    /// <summary>UTC timestamp when the export operation completed.</summary>
    [JsonPropertyName("endTime")]
    public DateTime EndTime { get; set; }

    /// <summary>Export duration in seconds, computed from StartTime and EndTime.</summary>
    [JsonPropertyName("duration")]
    public double DurationSeconds => (EndTime - StartTime).TotalSeconds;

    /// <summary>Device types found in the exported data (e.g., "Apple Watch", "Oura Ring").</summary>
    [JsonPropertyName("deviceTypes")]
    public List<string> DeviceTypes { get; set; } = new();

    /// <summary>Health data types included (e.g., "HeartRate", "Sleep", "Steps").</summary>
    [JsonPropertyName("dataTypes")]
    public List<string> DataTypes { get; set; } = new();

    /// <summary>Non-fatal issues encountered during export.</summary>
    [JsonPropertyName("warnings")]
    public List<string> Warnings { get; set; } = new();

    /// <summary>Fatal errors that prevented complete export.</summary>
    [JsonPropertyName("errors")]
    public List<string> Errors { get; set; } = new();

    /// <summary>Whether the output files were compressed (gzip).</summary>
    [JsonPropertyName("isCompressed")]
    public bool IsCompressed { get; set; }

    /// <summary>Compression ratio (original/compressed), or null if not compressed.</summary>
    [JsonPropertyName("compressionRatio")]
    public double? CompressionRatio { get; set; }

    /// <summary>
    /// Returns the output size formatted in human-readable units (B, KB, MB, GB).
    /// </summary>
    public string GetHumanReadableSize()
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double size = OutputSizeBytes;
        int order = 0;

        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }

        return $"{size:F2} {sizes[order]}";
    }

    /// <summary>Indicates whether any errors occurred during export.</summary>
    public bool HasErrors => Errors.Count > 0;
    /// <summary>Indicates whether any warnings were generated during export.</summary>
    public bool HasWarnings => Warnings.Count > 0;
}

/// <summary>
/// Metadata about a single file produced during an export operation.
/// </summary>
public sealed class ExportedFile
{
    /// <summary>Name of the generated file (without directory path).</summary>
    [JsonPropertyName("fileName")]
    public string FileName { get; set; } = string.Empty;

    /// <summary>Full filesystem path to the generated file.</summary>
    [JsonPropertyName("filePath")]
    public string FilePath { get; set; } = string.Empty;

    /// <summary>Output format of this file (e.g., "csv", "json", "xml").</summary>
    [JsonPropertyName("format")]
    public string Format { get; set; } = string.Empty;

    /// <summary>Number of health data records contained in this file.</summary>
    [JsonPropertyName("recordCount")]
    public int RecordCount { get; set; }

    /// <summary>File size in bytes.</summary>
    [JsonPropertyName("fileSize")]
    public long FileSizeBytes { get; set; }

    /// <summary>SHA-256 hash of the file contents for integrity verification, or null if not computed.</summary>
    [JsonPropertyName("hash")]
    public string? Hash { get; set; }

    /// <summary>UTC timestamp when the file was created.</summary>
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
}
