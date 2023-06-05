// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace HealthDataExportTools.Events;

/// <summary>
/// Event raised when health data export is successfully completed
/// Contains export metadata and outcome information
/// </summary>
public class ExportCompletedEvent : EventBase
{
    public ExportFormat ExportFormat { get; }
    public int RecordsExported { get; }
    public string OutputPath { get; }
    public long OutputSizeBytes { get; }
    public DateTime ExportStartTime { get; }
    public DateTime ExportEndTime { get; }
    public bool WasCompressed { get; }
    public List<string> GeneratedFiles { get; }
    public List<string> Warnings { get; }

    public ExportCompletedEvent(
        string exportId,
        ExportFormat exportFormat,
        int recordsExported,
        string outputPath,
        long outputSizeBytes,
        DateTime exportStartTime,
        DateTime exportEndTime,
        bool wasCompressed,
        List<string> generatedFiles,
        List<string> warnings = null)
        : base(exportId)
    {
        ExportFormat = exportFormat;
        RecordsExported = recordsExported;
        OutputPath = outputPath;
        OutputSizeBytes = outputSizeBytes;
        ExportStartTime = exportStartTime;
        ExportEndTime = exportEndTime;
        WasCompressed = wasCompressed;
        GeneratedFiles = generatedFiles ?? new List<string>();
        Warnings = warnings ?? new List<string>();
    }

    /// <summary>
    /// Get the duration of the export operation
    /// </summary>
    public TimeSpan GetExportDuration() => ExportEndTime - ExportStartTime;

    /// <summary>
    /// Get records per second throughput
    /// </summary>
    public double GetThroughput()
    {
        var duration = GetExportDuration();
        if (duration.TotalSeconds == 0)
            return 0;

        return RecordsExported / duration.TotalSeconds;
    }

    /// <summary>
    /// Get human-readable size
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

    /// <summary>
    /// Check if export had any warnings
    /// </summary>
    public bool HasWarnings => Warnings.Count > 0;

    public override string ToString()
    {
        return $"ExportCompleted: {RecordsExported} records to {ExportFormat} " +
               $"({GetHumanReadableSize()}) in {GetExportDuration().TotalSeconds:F2}s";
    }
}
