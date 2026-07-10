#nullable enable

namespace HealthDataExportTools.DTOs;

/// <summary>
/// Extension methods for ValidationResultDto providing additional functionality
/// </summary>
public static class ValidationResultDtoExtensions
{
    /// <summary>
    /// Gets the percentage of invalid records relative to total records
    /// </summary>
    /// <param name="dto">The validation result</param>
    /// <returns>Percentage of invalid records (0-100)</returns>
    public static double GetInvalidRate(this ValidationResultDto dto)
    {
        if (dto.TotalRecords <= 0)
            return 0;

        return (dto.InvalidRecords * 100.0) / dto.TotalRecords;
    }

    /// <summary>
    /// Gets the percentage of valid records relative to total records
    /// </summary>
    /// <param name="dto">The validation result</param>
    /// <returns>Percentage of valid records (0-100)</returns>
    public static double GetValidRate(this ValidationResultDto dto)
    {
        if (dto.TotalRecords <= 0)
            return 100;

        return (dto.ValidRecords * 100.0) / dto.TotalRecords;
    }

    /// <summary>
    /// Determines if the validation has critical errors (severity = "Critical")
    /// </summary>
    /// <param name="dto">The validation result</param>
    /// <returns>True if any critical errors exist, false otherwise</returns>
    public static bool HasCriticalErrors(this ValidationResultDto dto)
    {
        return dto.ValidationErrors.Any(error => string.Equals(error.Severity, "Critical", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets the most severe error from the validation result
    /// </summary>
    /// <param name="dto">The validation result</param>
    /// <returns>The most severe error detail, or null if no errors exist</returns>
    public static ValidationErrorDetail? GetMostSevereError(this ValidationResultDto dto)
    {
        if (dto.ValidationErrors.Count == 0)
            return null;

        return dto.ValidationErrors
            .OrderByDescending(error => error.Severity, StringComparer.OrdinalIgnoreCase)
            .ThenByDescending(error => error.Message.Length)
            .First();
    }

    /// <summary>
    /// Gets the count of errors by a specific error code
    /// </summary>
    /// <param name="dto">The validation result</param>
    /// <param name="errorCode">The error code to count</param>
    /// <returns>Number of errors with the specified code</returns>
    public static int GetErrorCountByCode(this ValidationResultDto dto, string errorCode)
    {
        if (string.IsNullOrWhiteSpace(errorCode))
            return 0;

        return dto.ValidationErrors.Count(error => string.Equals(error.ErrorCode, errorCode, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets a summary string of the validation result
    /// </summary>
    /// <param name="dto">The validation result</param>
    /// <returns>Formatted summary string</returns>
    public static string GetSummary(this ValidationResultDto dto)
    {
        var successRate = dto.GetSuccessRate();
        var invalidRate = dto.GetInvalidRate();
        var errorCount = dto.GetErrorCount();
        var warningCount = dto.GetWarningCount();

        return $"Validation [{dto.ValidationId}] - " +
               $"Total: {dto.TotalRecords} | " +
               $"Valid: {dto.ValidRecords} ({successRate:F2}%) | " +
               $"Invalid: {dto.InvalidRecords} ({invalidRate:F2}%) | " +
               $"Errors: {errorCount} | " +
               $"Warnings: {warningCount} | " +
               $"Duration: {dto.DurationMs:F0}ms";
    }

    /// <summary>
    /// Determines if the validation result has any warnings
    /// </summary>
    /// <param name="dto">The validation result</param>
    /// <returns>True if warnings exist, false otherwise</returns>
    public static bool HasWarnings(this ValidationResultDto dto)
    {
        return dto.Warnings.Count > 0;
    }

    /// <summary>
    /// Gets the first warning from the validation result
    /// </summary>
    /// <param name="dto">The validation result</param>
    /// <returns>The first warning, or null if no warnings exist</returns>
    public static ValidationWarning? GetFirstWarning(this ValidationResultDto dto)
    {
        return dto.Warnings.Count > 0 ? dto.Warnings[0] : null;
    }

    /// <summary>
    /// Calculates the ratio of valid to invalid records
    /// </summary>
    /// <param name="dto">The validation result</param>
    /// <returns>Ratio of valid to invalid records (e.g., 5.2 means 5.2 valid per 1 invalid)</returns>
    public static double GetValidToInvalidRatio(this ValidationResultDto dto)
    {
        if (dto.InvalidRecords <= 0)
            return dto.TotalRecords > 0 ? double.PositiveInfinity : 1.0;

        return (double)dto.ValidRecords / dto.InvalidRecords;
    }
}