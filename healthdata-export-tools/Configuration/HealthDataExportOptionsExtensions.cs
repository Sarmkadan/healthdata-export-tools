#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace HealthDataExportTools.Configuration;

/// <summary>
/// Extension methods for <see cref="HealthDataExportOptions"/> providing common operations and validations
/// </summary>
public static class HealthDataExportOptionsExtensions
{
    /// <summary>
    /// Creates a deep copy of the <see cref="HealthDataExportOptions"/> instance
    /// </summary>
    /// <param name="options">The options to copy</param>
    /// <returns>A new instance with the same values</returns>
    /// <exception cref="ArgumentNullException">Thrown when options is null</exception>
    public static HealthDataExportOptions Clone(this HealthDataExportOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return new HealthDataExportOptions
        {
            InputPath = options.InputPath,
            OutputPath = options.OutputPath,
            DatabasePath = options.DatabasePath,
            ExportFormat = options.ExportFormat,
            ValidateData = options.ValidateData,
            PerformAnalysis = options.PerformAnalysis,
            TrendAnalysisDays = options.TrendAnalysisDays,
            MaxRecordAgeDays = options.MaxRecordAgeDays,
            CompressOutput = options.CompressOutput,
            TargetDeviceType = options.TargetDeviceType,
            TargetDeviceId = options.TargetDeviceId,
            StartDate = options.StartDate,
            EndDate = options.EndDate,
            NotificationEmail = options.NotificationEmail,
            VerboseLogging = options.VerboseLogging
        };
    }

    /// <summary>
    /// Gets the effective input path - returns DatabasePath if InputPath is empty, otherwise InputPath
    /// </summary>
    /// <param name="options">The options instance</param>
    /// <returns>The effective input path</returns>
    /// <exception cref="ArgumentNullException">Thrown when options is null</exception>
    public static string GetEffectiveInputPath(this HealthDataExportOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return string.IsNullOrWhiteSpace(options.InputPath)
            ? options.DatabasePath
            : options.InputPath;
    }

    /// <summary>
    /// Gets the effective output directory - ensures it exists by creating if necessary
    /// </summary>
    /// <param name="options">The options instance</param>
    /// <returns>The effective output directory path</returns>
    /// <exception cref="ArgumentNullException">Thrown when options is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when OutputPath cannot be created</exception>
    public static string EnsureOutputDirectoryExists(this HealthDataExportOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var outputPath = options.OutputPath;
        if (string.IsNullOrWhiteSpace(outputPath))
        {
            throw new InvalidOperationException("OutputPath must be specified to ensure directory exists");
        }

        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }

        return outputPath;
    }

    /// <summary>
    /// Gets the date range as a tuple for easy consumption
    /// </summary>
    /// <param name="options">The options instance</param>
    /// <returns>A tuple containing (StartDate, EndDate) where null values are preserved</returns>
    /// <exception cref="ArgumentNullException">Thrown when options is null</exception>
    public static (DateTime? StartDate, DateTime? EndDate) GetDateRange(this HealthDataExportOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return (options.StartDate, options.EndDate);
    }

    /// <summary>
    /// Gets the effective file extension for the configured export format
    /// </summary>
    /// <param name="options">The options instance</param>
    /// <returns>The file extension including dot (e.g., ".json", ".csv")</returns>
    /// <exception cref="ArgumentNullException">Thrown when options is null</exception>
    public static string GetFileExtension(this HealthDataExportOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return options.ExportFormat switch
        {
            ExportFormat.Json => ".json",
            ExportFormat.Csv => ".csv",
            ExportFormat.Sqlite => ".db",
            ExportFormat.Html => ".html",
            ExportFormat.All => ".all",
            _ => ".json"
        };
    }

    /// <summary>
    /// Gets the device filter as a formatted string for logging purposes
    /// </summary>
    /// <param name="options">The options instance</param>
    /// <returns>A formatted string representing the device filter</returns>
    /// <exception cref="ArgumentNullException">Thrown when options is null</exception>
    public static string GetDeviceFilterDescription(this HealthDataExportOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(options.TargetDeviceType) && string.IsNullOrWhiteSpace(options.TargetDeviceId))
        {
            return "All devices";
        }

        if (!string.IsNullOrWhiteSpace(options.TargetDeviceType) && !string.IsNullOrWhiteSpace(options.TargetDeviceId))
        {
            return $"Device Type: {options.TargetDeviceType}, Device ID: {options.TargetDeviceId}";
        }

        if (!string.IsNullOrWhiteSpace(options.TargetDeviceType))
        {
            return $"Device Type: {options.TargetDeviceType}";
        }

        return $"Device ID: {options.TargetDeviceId}";
    }

    /// <summary>
    /// Gets the configured analysis period as a TimeSpan
    /// </summary>
    /// <param name="options">The options instance</param>
    /// <returns>The analysis period as TimeSpan</returns>
    /// <exception cref="ArgumentNullException">Thrown when options is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when TrendAnalysisDays is negative</exception>
    public static TimeSpan GetAnalysisPeriod(this HealthDataExportOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (options.TrendAnalysisDays < 0)
        {
            throw new InvalidOperationException("TrendAnalysisDays must be non-negative");
        }

        return TimeSpan.FromDays(options.TrendAnalysisDays);
    }

    /// <summary>
    /// Gets the configured maximum record age as a TimeSpan
    /// </summary>
    /// <param name="options">The options instance</param>
    /// <returns>The maximum record age as TimeSpan</returns>
    /// <exception cref="ArgumentNullException">Thrown when options is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when MaxRecordAgeDays is negative</exception>
    public static TimeSpan GetMaxRecordAge(this HealthDataExportOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (options.MaxRecordAgeDays < 0)
        {
            throw new InvalidOperationException("MaxRecordAgeDays must be non-negative");
        }

        return TimeSpan.FromDays(options.MaxRecordAgeDays);
    }

    /// <summary>
    /// Gets the output file path for a given base filename using the configured format
    /// </summary>
    /// <param name="options">The options instance</param>
    /// <param name="baseFilename">The base filename without extension</param>
    /// <returns>The full output file path with appropriate extension</returns>
    /// <exception cref="ArgumentNullException">Thrown when options or baseFilename is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when OutputPath is not specified</exception>
    public static string GetOutputFilePath(this HealthDataExportOptions options, string baseFilename)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrEmpty(baseFilename);

        var outputPath = options.EnsureOutputDirectoryExists();
        var extension = options.GetFileExtension();
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);

        return Path.Combine(outputPath, $"{baseFilename}_{timestamp}{extension}");
    }

    /// <summary>
    /// Gets all configured validation rules as an enumerable of strings
    /// </summary>
    /// <param name="options">The options instance</param>
    /// <returns>An enumerable of validation rule descriptions</returns>
    /// <exception cref="ArgumentNullException">Thrown when options is null</exception>
    public static IEnumerable<string> GetValidationRules(this HealthDataExportOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        yield return "InputPath or DatabasePath must be specified";
        yield return "OutputPath must be specified";
        yield return "MaxRecordAgeDays must be non-negative";
        yield return "TrendAnalysisDays must be positive";
        yield return "StartDate cannot be after EndDate";
        yield return "NotificationEmail must be a valid email if specified";

        if (!string.IsNullOrWhiteSpace(options.TargetDeviceType))
        {
            yield return $"TargetDeviceType is set to: {options.TargetDeviceType}";
        }

        if (!string.IsNullOrWhiteSpace(options.TargetDeviceId))
        {
            yield return $"TargetDeviceId is set to: {options.TargetDeviceId}";
        }

        if (options.ValidateData)
        {
            yield return "Data validation is enabled";
        }

        if (options.PerformAnalysis)
        {
            yield return $"Analysis is enabled for {options.TrendAnalysisDays} days";
        }

        if (options.CompressOutput)
        {
            yield return "Output compression is enabled";
        }
    }
}