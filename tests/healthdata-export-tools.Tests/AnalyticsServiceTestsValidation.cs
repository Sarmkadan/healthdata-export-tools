#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;
using HealthDataExportTools.Services;

namespace HealthDataExportTools.Tests;

/// <summary>
/// Provides validation helpers for <see cref="AnalyticsServiceTests"/> instances.
/// Validates that test data follows expected patterns and constraints for health analytics testing.
/// </summary>
public static class AnalyticsServiceTestsValidation
{
    /// <summary>
    /// Validates an <see cref="AnalyticsServiceTests"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The <see cref="AnalyticsServiceTests"/> instance to validate.</param>
    /// <returns>An immutable list of validation problems; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this AnalyticsServiceTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate private field _analyticsService
        if (value.GetType().GetField("_analyticsService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(value) is not AnalyticsService analyticsService)
        {
            problems.Add("Private field '_analyticsService' is null or not of type AnalyticsService");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="AnalyticsServiceTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The <see cref="AnalyticsServiceTests"/> instance to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this AnalyticsServiceTests value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="AnalyticsServiceTests"/> instance is valid.
    /// Throws an <see cref="ArgumentException"/> with a detailed message listing all validation problems.
    /// </summary>
    /// <param name="value">The <see cref="AnalyticsServiceTests"/> instance to validate.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is invalid.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static void EnsureValid(this AnalyticsServiceTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"AnalyticsServiceTests instance is invalid. Problems: {string.Join("; ", problems)}",
            nameof(value));
    }
}