using System;
using System.Collections.Generic;

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

        // Validate that the test class has test methods (Xunit Fact/Theory attributes)
        // This is a marker validation - DataTransformationUtilityTests is a test class container
        // and should have at least one test method to be considered valid
        var testMethods = value.GetType().GetMethods(
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.DeclaredOnly);

        if (testMethods.Length == 0)
        {
            problems.Add("Test class must contain at least one test method.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="DataTransformationUtilityTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The test instance to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this DataTransformationUtilityTests value)
    {
        if (value is null)
        {
            return false;
        }

        try
        {
            var problems = value.Validate();
            return problems.Count == 0;
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