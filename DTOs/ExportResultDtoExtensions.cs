using System;
using System.Linq;

namespace HealthDataExportTools.DTOs;

/// <summary>
/// Extension methods for <see cref="ExportResultDto"/> providing convenience functionality
/// </summary>
public static class ExportResultDtoExtensions
{
    /// <summary>
    /// Determines whether the export operation was successful (status is "Success" and no errors)
    /// </summary>
    /// <param name="dto">The export result DTO</param>
    /// <returns>True if the export was successful; otherwise, false</returns>
    /// <exception cref="ArgumentNullException">Thrown if dto is null</exception>
    public static bool IsSuccess(this ExportResultDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return string.Equals(dto.Status, "Success", StringComparison.OrdinalIgnoreCase) && !dto.HasErrors;
    }

    /// <summary>
    /// Creates a concise summary string for the export result
    /// </summary>
    /// <param name="dto">The export result DTO</param>
    /// <returns>A formatted summary string</returns>
    /// <exception cref="ArgumentNullException">Thrown if dto is null</exception>
    public static string ToSummaryString(this ExportResultDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var format = dto.ExportedFormats.FirstOrDefault() ?? "unknown format";
        var recordCount = dto.GeneratedFiles.Sum(f => f.RecordCount);
        var duration = dto.DurationSeconds;
        var size = dto.GetHumanReadableSize();

        return $"Exported {recordCount} records to {format} in {duration:F1}s ({size})";
    }

    /// <summary>
    /// Throws an exception if the export operation failed
    /// </summary>
    /// <param name="dto">The export result DTO</param>
    /// <exception cref="ArgumentNullException">Thrown if dto is null</exception>
    /// <exception cref="InvalidOperationException">Thrown if the export failed</exception>
    public static void ThrowIfFailed(this ExportResultDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        if (!dto.IsSuccess())
        {
            var errorMessage = dto.Errors.FirstOrDefault() ?? "Export operation failed";
            throw new InvalidOperationException(errorMessage);
        }
    }
}