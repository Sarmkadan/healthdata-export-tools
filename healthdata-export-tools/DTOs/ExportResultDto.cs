// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace HealthDataExportTools.DTOs;

/// <summary>
/// Data Transfer Object for export operation results
/// </summary>
public class ExportResultDto
{
    [JsonPropertyName("exportId")]
    public string ExportId { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("status")]
    public string Status { get; set; } = "Success";

    [JsonPropertyName("recordsExported")]
    public int RecordsExported { get; set; }

    [JsonPropertyName("recordsFiltered")]
    public int RecordsFiltered { get; set; }

    [JsonPropertyName("outputPath")]
    public string OutputPath { get; set; } = string.Empty;

    [JsonPropertyName("outputSize")]
    public long OutputSizeBytes { get; set; }

    [JsonPropertyName("generatedFiles")]
    public List<ExportedFile> GeneratedFiles { get; set; } = new();

    [JsonPropertyName("exportedFormats")]
    public List<string> ExportedFormats { get; set; } = new();

    [JsonPropertyName("startTime")]
    public DateTime StartTime { get; set; }

    [JsonPropertyName("endTime")]
    public DateTime EndTime { get; set; }

    [JsonPropertyName("duration")]
    public double DurationSeconds => (EndTime - StartTime).TotalSeconds;

    [JsonPropertyName("deviceTypes")]
    public List<string> DeviceTypes { get; set; } = new();

    [JsonPropertyName("dataTypes")]
    public List<string> DataTypes { get; set; } = new();

    [JsonPropertyName("warnings")]
    public List<string> Warnings { get; set; } = new();

    [JsonPropertyName("errors")]
    public List<string> Errors { get; set; } = new();

    [JsonPropertyName("isCompressed")]
    public bool IsCompressed { get; set; }

    [JsonPropertyName("compressionRatio")]
    public double? CompressionRatio { get; set; }

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

    public bool HasErrors => Errors.Count > 0;
    public bool HasWarnings => Warnings.Count > 0;
}

/// <summary>
/// Information about an exported file
/// </summary>
public class ExportedFile
{
    [JsonPropertyName("fileName")]
    public string FileName { get; set; } = string.Empty;

    [JsonPropertyName("filePath")]
    public string FilePath { get; set; } = string.Empty;

    [JsonPropertyName("format")]
    public string Format { get; set; } = string.Empty;

    [JsonPropertyName("recordCount")]
    public int RecordCount { get; set; }

    [JsonPropertyName("fileSize")]
    public long FileSizeBytes { get; set; }

    [JsonPropertyName("hash")]
    public string? Hash { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
}
