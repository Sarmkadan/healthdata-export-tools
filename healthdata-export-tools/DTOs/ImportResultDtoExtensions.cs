#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace HealthDataExportTools.DTOs;

/// <summary>
/// Extension methods for <see cref="ImportResultDto"/> providing common
/// diagnostics such as success rate, failure detection and a concise log
/// representation. The implementation uses reflection to remain tolerant to
/// variations in the DTO's property names that may evolve across versions.
/// </summary>
public static class ImportResultDtoExtensions
{
    /// <summary>
    /// Calculates the success rate as a value between 0.0 and 1.0.
    /// If the DTO does not expose recognizable total or success properties,
    /// the method returns 0.0.
    /// </summary>
    public static double SuccessRate(this ImportResultDto dto)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));

        // Try to locate a total count property
        var totalProp = FindProperty(dto, "Total", "TotalCount", "RecordCount", "Count");
        // Try to locate a successful count property
        var successProp = FindProperty(dto, "Successful", "SuccessCount", "SuccessfulCount", "Succeeded");

        if (totalProp == null || successProp == null)
            return 0.0;

        var totalObj = totalProp.GetValue(dto);
        var successObj = successProp.GetValue(dto);

        if (totalObj is null || successObj is null)
            return 0.0;

        double total = Convert.ToDouble(totalObj);
        double success = Convert.ToDouble(successObj);

        if (total <= 0) return 0.0;
        return success / total;
    }

    /// <summary>
    /// Determines whether the import operation reported any failures.
    /// Checks for a failure count property or a non‑empty collection of errors.
    /// </summary>
    public static bool HasFailures(this ImportResultDto dto)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));

        // Look for a failure count property
        var failureProp = FindProperty(dto, "Failed", "FailureCount", "FailedCount", "ErrorsCount");
        if (failureProp != null)
        {
            var value = failureProp.GetValue(dto);
            if (value != null && Convert.ToInt32(value) > 0)
                return true;
        }

        // Look for an enumerable of errors
        var errorsProp = FindProperty(dto, "Errors", "ErrorMessages", "ErrorList");
        if (errorsProp != null)
        {
            var value = errorsProp.GetValue(dto);
            if (value is IEnumerable enumerable)
            {
                foreach (var _ in enumerable)
                    return true; // at least one element
            }
        }

        return false;
    }

    /// <summary>
    /// Generates a short, human‑readable log string summarising the import result.
    /// Includes total, successful, failure counts (when available) and the first
    /// few error messages if any.
    /// </summary>
    public static string ToLogString(this ImportResultDto dto)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));

        var sb = new StringBuilder();

        // Total / Success
        var totalProp = FindProperty(dto, "Total", "TotalCount", "RecordCount", "Count");
        var successProp = FindProperty(dto, "Successful", "SuccessCount", "SuccessfulCount", "Succeeded");
        var failureProp = FindProperty(dto, "Failed", "FailureCount", "FailedCount", "ErrorsCount");

        if (totalProp != null)
        {
            sb.Append($"Total={totalProp.GetValue(dto)}");
        }

        if (successProp != null)
        {
            if (sb.Length > 0) sb.Append(' ');
            sb.Append($"Success={successProp.GetValue(dto)}");
        }

        if (failureProp != null)
        {
            if (sb.Length > 0) sb.Append(' ');
            sb.Append($"Failed={failureProp.GetValue(dto)}");
        }

        // Append a brief error summary if there are errors
        var errorsProp = FindProperty(dto, "Errors", "ErrorMessages", "ErrorList");
        if (errorsProp != null)
        {
            var value = errorsProp.GetValue(dto);
            if (value is IEnumerable enumerable)
            {
                var errors = enumerable.Cast<object>()
                                      .Select(o => o?.ToString() ?? string.Empty)
                                      .Where(s => !string.IsNullOrEmpty(s))
                                      .Take(3)
                                      .ToArray();

                if (errors.Length > 0)
                {
                    sb.Append(" Errors=[");
                    sb.Append(string.Join("; ", errors));
                    if (enumerable.Cast<object>().Count() > errors.Length)
                        sb.Append("; ...");
                    sb.Append(']');
                }
            }
        }

        // Fallback if nothing could be extracted
        if (sb.Length == 0)
        {
            sb.Append(dto.ToString());
        }

        return sb.ToString();
    }

    // --------------------------------------------------------------------
    // Helper: locate a property by trying several possible names (case‑insensitive)
    // --------------------------------------------------------------------
    private static PropertyInfo? FindProperty(ImportResultDto dto, params string[] possibleNames)
    {
        var type = dto.GetType();
        foreach (var name in possibleNames)
        {
            var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop != null) return prop;
        }
        return null;
    }
}
