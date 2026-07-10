#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace HealthDataExportTools.Cli;

/// <summary>
/// Extension methods for CliArgumentParser providing additional functionality
/// </summary>
public static class CliArgumentParserExtensions
{
    /// <summary>
    /// Parses command-line arguments with default validation enabled
    /// </summary>
    /// <param name="parser">The parser instance</param>
    /// <param name="args">Command-line arguments</param>
    /// <returns>Parsed options</returns>
    /// <exception cref="ArgumentException">Thrown if validation fails</exception>
    public static CliOptions ParseWithValidation(this CliArgumentParser parser, string[] args)
    {
        var options = parser.Parse(args);
        var errors = parser.Validate();

        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"Validation failed:\n{string.Join("\n", errors)}");
        }

        return options;
    }

    /// <summary>
    /// Parses command-line arguments and returns whether validation passed
    /// </summary>
    /// <param name="parser">The parser instance</param>
    /// <param name="args">Command-line arguments</param>
    /// <param name="options">Parsed options if successful</param>
    /// <returns>True if parsing and validation succeeded; false otherwise</returns>
    public static bool TryParse(this CliArgumentParser parser, string[] args, out CliOptions? options)
    {
        try
        {
            options = parser.Parse(args);
            var errors = parser.Validate();

            if (errors.Count > 0)
            {
                options = null;
                return false;
            }

            return true;
        }
        catch
        {
            options = null;
            return false;
        }
    }

    /// <summary>
    /// Gets a summary of the parsed options for display purposes
    /// </summary>
    /// <param name="parser">The parser instance</param>
    /// <param name="options">Parsed options</param>
    /// <returns>Formatted summary string</returns>
    public static string GetSummary(this CliArgumentParser parser, CliOptions options)
    {
        var summary = new System.Text.StringBuilder();
        summary.AppendLine("=== CLI Options Summary ===");
        summary.AppendLine($"Input Path: {options.InputPath}");
        summary.AppendLine($"Output Path: {options.OutputPath}");
        summary.AppendLine($"Database Path: {options.DatabasePath}");
        summary.AppendLine($"Format: {options.Format}");
        summary.AppendLine($"Device: {options.Device}");
        summary.AppendLine($"Data Type: {options.DataType}");
        summary.AppendLine($"Start Date: {options.StartDate ?? "Not specified"}");
        summary.AppendLine($"End Date: {options.EndDate ?? "Not specified"}");
        summary.AppendLine($"Validate: {options.Validate}");
        summary.AppendLine($"Analyze: {options.Analyze}");
        summary.AppendLine($"Compress: {options.Compress}");
        summary.AppendLine($"Parallel: {options.Parallel}");
        summary.AppendLine($"Max Parallelism: {options.MaxParallelism}");
        summary.AppendLine($"Enable Cache: {options.EnableCache}");
        summary.AppendLine($"Cache Duration: {options.CacheDurationMinutes} minutes");
        summary.AppendLine($"Verbose: {options.Verbose}");
        summary.AppendLine("========================");

        return summary.ToString();
    }

    /// <summary>
    /// Checks if the parsed options indicate a help or version request
    /// </summary>
    /// <param name="parser">The parser instance</param>
    /// <param name="options">Parsed options</param>
    /// <returns>True if help or version should be displayed</returns>
    public static bool ShouldShowHelpOrVersion(this CliArgumentParser parser, CliOptions options)
    {
        return options.Help || options.Version;
    }
}