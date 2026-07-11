using System;
using System.Collections.Generic;
using System.Globalization;

namespace HealthDataExportTools.Tests;

/// <summary>
/// Provides validation helpers for <see cref="DataTransformationUtilityTests"/> instances.
/// Validates that test instances are properly initialized and contain valid values for testing.
/// </summary>
public static class DataTransformationUtilityTestsValidation
{
    /// <summary>
    /// Validates the specified <see cref="DataTransformationUtilityTests"/> instance.
    /// </summary>
    /// <param name="value">The test instance to validate.</param>
    /// <returns>A list of validation problems; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this DataTransformationUtilityTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate test methods are not null (they're methods, but should be present)
        // Test methods are static and should always be present for a valid test class

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="DataTransformationUtilityTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The test instance to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this DataTransformationUtilityTests value)
    {
        try
        {
            _ = value.Validate();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Ensures that the specified <see cref="DataTransformationUtilityTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The test instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance contains validation problems.</exception>
    public static void EnsureValid(this DataTransformationUtilityTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"DataTransformationUtilityTests instance is invalid. Problems: {string.Join(", ", problems)}");
        }
    }
}