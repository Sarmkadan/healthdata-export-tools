#nullable enable

namespace HealthDataExportTools.Services;

/// <summary>
/// Extension methods for <see cref="CsvExportOptions"/> that provide convenient configuration helpers.
/// </summary>
public static class CsvExportOptionsExtensions
{
    /// <summary>
    /// Enables all data types for export (sleep, heart rate, SpO2, steps, activity).
    /// </summary>
    /// <param name="options">The export options to configure.</param>
    /// <returns>The configured <see cref="CsvExportOptions"/> for method chaining.</returns>
    public static CsvExportOptions EnableAllDataTypes(this CsvExportOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.IncludeSleep = true;
        options.IncludeHeartRate = true;
        options.IncludeSpO2 = true;
        options.IncludeSteps = true;
        options.IncludeActivity = true;

        return options;
    }

    /// <summary>
    /// Disables all data types for export.
    /// </summary>
    /// <param name="options">The export options to configure.</param>
    /// <returns>The configured <see cref="CsvExportOptions"/> for method chaining.</returns>
    public static CsvExportOptions DisableAllDataTypes(this CsvExportOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.IncludeSleep = false;
        options.IncludeHeartRate = false;
        options.IncludeSpO2 = false;
        options.IncludeSteps = false;
        options.IncludeActivity = false;

        return options;
    }

    /// <summary>
    /// Sets the date format for all date columns.
    /// </summary>
    /// <param name="options">The export options to configure.</param>
    /// <param name="format">The date format string (e.g., "yyyy-MM-dd", "MM/dd/yyyy").</param>
    /// <returns>The configured <see cref="CsvExportOptions"/> for method chaining.</returns>
    public static CsvExportOptions WithDateFormat(this CsvExportOptions options, string format)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(format);

        options.DateFormat = format;
        return options;
    }

    /// <summary>
    /// Configures the export to include only specific column groups.
    /// </summary>
    /// <param name="options">The export options to configure.</param>
    /// <param name="includeSleep">Whether to include sleep columns.</param>
    /// <param name="includeHeartRate">Whether to include heart rate columns.</param>
    /// <param name="includeSpO2">Whether to include SpO2 columns.</param>
    /// <param name="includeSteps">Whether to include steps columns.</param>
    /// <param name="includeActivity">Whether to include activity columns.</param>
    /// <returns>The configured <see cref="CsvExportOptions"/> for method chaining.</returns>
    public static CsvExportOptions WithColumns(
        this CsvExportOptions options,
        bool includeSleep = true,
        bool includeHeartRate = true,
        bool includeSpO2 = true,
        bool includeSteps = true,
        bool includeActivity = true)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.IncludeSleep = includeSleep;
        options.IncludeHeartRate = includeHeartRate;
        options.IncludeSpO2 = includeSpO2;
        options.IncludeSteps = includeSteps;
        options.IncludeActivity = includeActivity;

        return options;
    }
}
