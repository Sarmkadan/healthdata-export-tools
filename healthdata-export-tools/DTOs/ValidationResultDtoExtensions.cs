#nullable enable

namespace HealthDataExportTools.DTOs;

/// <summary>
/// Extension methods for <see cref="ValidationResultDto"/> providing additional validation result analysis functionality.
/// </summary>
public static class ValidationResultDtoExtensions
{
    /// <summary>
    /// Gets the percentage of invalid records relative to total records.
    /// </summary>
    /// <param name="dto">The validation result. Cannot be <see langword="null"/>.</param>
    /// <returns>Percentage of invalid records (0-100).</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dto"/> is <see langword="null"/>.</exception>
    public static double GetInvalidRate(this ValidationResultDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return dto.TotalRecords > 0
            ? (dto.InvalidRecords * 100.0) / dto.TotalRecords
            : 0.0;
    }

    /// <summary>
    /// Gets the percentage of valid records relative to total records.
    /// </summary>
    /// <param name="dto">The validation result. Cannot be <see langword="null"/>.</param>
    /// <returns>Percentage of valid records (0-100).</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dto"/> is <see langword="null"/>.</exception>
    public static double GetValidRate(this ValidationResultDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return dto.TotalRecords > 0
            ? (dto.ValidRecords * 100.0) / dto.TotalRecords
            : 100.0;
    }

    /// <summary>
    /// Determines whether the validation has critical errors (severity = "Critical").
    /// </summary>
    /// <param name="dto">The validation result. Cannot be <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if any critical errors exist; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dto"/> is <see langword="null"/>.</exception>
    public static bool HasCriticalErrors(this ValidationResultDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return dto.ValidationErrors.Count > 0 &&
               dto.ValidationErrors.Any(error => string.Equals(error.Severity, "Critical", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets the most severe error from the validation result.
    /// </summary>
    /// <param name="dto">The validation result. Cannot be <see langword="null"/>.</param>
    /// <returns>The most severe error detail, or <see langword="null"/> if no errors exist.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dto"/> is <see langword="null"/>.</exception>
    public static ValidationErrorDetail? GetMostSevereError(this ValidationResultDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return dto.ValidationErrors.Count == 0
            ? null
            : dto.ValidationErrors
                .OrderByDescending(error => error.Severity, StringComparer.OrdinalIgnoreCase)
                .ThenByDescending(error => error.Message.Length)
                .First();
    }

    /// <summary>
    /// Gets the count of errors by a specific error code.
    /// </summary>
    /// <param name="dto">The validation result. Cannot be <see langword="null"/>.</param>
    /// <param name="errorCode">The error code to count. Cannot be <see langword="null"/>, <see langword="string.Empty"/>, or whitespace.</param>
    /// <returns>Number of errors with the specified code.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dto"/> or <paramref name="errorCode"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="errorCode"/> is <see langword="string.Empty"/> or consists only of whitespace.</exception>
    public static int GetErrorCountByCode(this ValidationResultDto dto, string errorCode)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ArgumentException.ThrowIfNullOrEmpty(errorCode);

        return dto.ValidationErrors.Count(error => string.Equals(error.ErrorCode, errorCode, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets a summary string of the validation result.
    /// </summary>
    /// <param name="dto">The validation result. Cannot be <see langword="null"/>.</param>
    /// <returns>Formatted summary string containing validation statistics.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dto"/> is <see langword="null"/>.</exception>
    public static string GetSummary(this ValidationResultDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

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
    /// Determines whether the validation result has any warnings.
    /// </summary>
    /// <param name="dto">The validation result. Cannot be <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if warnings exist; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dto"/> is <see langword="null"/>.</exception>
    public static bool HasWarnings(this ValidationResultDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return dto.Warnings.Count > 0;
    }

    /// <summary>
    /// Gets the first warning from the validation result.
    /// </summary>
    /// <param name="dto">The validation result. Cannot be <see langword="null"/>.</param>
    /// <returns>The first warning, or <see langword="null"/> if no warnings exist.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dto"/> is <see langword="null"/>.</exception>
    public static ValidationWarning? GetFirstWarning(this ValidationResultDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return dto.Warnings.Count > 0 ? dto.Warnings[0] : null;
    }

    /// <summary>
    /// Calculates the ratio of valid to invalid records.
    /// </summary>
    /// <param name="dto">The validation result. Cannot be <see langword="null"/>.</param>
    /// <returns>
    /// Ratio of valid to invalid records.
    /// Returns <see cref="double.PositiveInfinity"/> when there are no invalid records but at least one total record.
    /// Returns 1.0 when there are no records at all.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dto"/> is <see langword="null"/>.</exception>
    public static double GetValidToInvalidRatio(this ValidationResultDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return dto.InvalidRecords <= 0
            ? dto.TotalRecords > 0
                ? double.PositiveInfinity
                : 1.0
            : (double)dto.ValidRecords / dto.InvalidRecords;
    }
}