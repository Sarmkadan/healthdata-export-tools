#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace HealthDataExportTools.Events;

/// <summary>
/// Provides validation helpers for <see cref="EventBus"/> instances
/// </summary>
public static class EventBusValidation
{
    /// <summary>
    /// Validates an EventBus instance for common issues
    /// </summary>
    /// <param name="value">The EventBus instance to validate</param>
    /// <returns>List of validation problems; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    public static IReadOnlyList<string> Validate(this EventBus value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // EventBus itself doesn't have validation-worthy state beyond its constructor parameters
        // The handlers dictionary is internal and managed by the class
        // The logger is validated in constructor and assumed to be valid

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if an EventBus instance is valid
    /// </summary>
    /// <param name="value">The EventBus instance to check</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValid(this EventBus value)
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
    /// Ensures an EventBus instance is valid, throwing if not
    /// </summary>
    /// <param name="value">The EventBus instance to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    /// <exception cref="ArgumentException">Thrown if value is invalid with problem details</exception>
    public static void EnsureValid(this EventBus value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"EventBus is invalid. Problems: {string.Join("; ", problems)}");
        }
    }
}