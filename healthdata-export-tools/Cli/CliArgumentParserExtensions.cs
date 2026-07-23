#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ====================================================================

namespace HealthDataExportTools.Cli;

/// <summary>
/// Extension methods for <see cref="CliArgumentParser"/> providing additional functionality
/// </summary>
public static class CliArgumentParserExtensions
{
    /// <summary>
    /// Parses command-line arguments with default validation enabled
    /// </summary>
    /// <param name="parser">The parser instance. Cannot be null.</param>
    /// <param name="args">Command-line arguments. Cannot be null.</param>
    /// <returns>Parse result</returns>
    /// <exception cref="ArgumentNullException"><paramref name="parser"/> is null.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="args"/> is null.</exception>
    public static ParseResult<CliOptions> ParseWithValidation(this CliArgumentParser parser, string[] args)
    {
        ArgumentNullException.ThrowIfNull(parser);
        ArgumentNullException.ThrowIfNull(args);

        return parser.Parse(args);
    }

    /// <summary>
    /// Parses command-line arguments and returns whether validation passed
    /// </summary>
    /// <param name="parser">The parser instance. Cannot be null.</param>
    /// <param name="args">Command-line arguments. Cannot be null.</param>
    /// <param name="options">Parsed options if successful; otherwise null.</param>
    /// <returns>True if parsing and validation succeeded; false otherwise.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="parser"/> is null.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="args"/> is null.</exception>
    public static bool TryParse(this CliArgumentParser parser, string[] args, out CliOptions? options)
    {
        ArgumentNullException.ThrowIfNull(parser);
        ArgumentNullException.ThrowIfNull(args);

        try
        {
            var result = parser.Parse(args);
            options = result.Success ? result.Options : null;
            return result.Success;
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
    /// <param name="parser">The parser instance. Cannot be null.</param>
    /// <param name="options">Parsed options. Cannot be null.</param>
    /// <returns>Formatted summary string.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="parser"/> is null.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is null.</exception>
    public static string GetSummary(this CliArgumentParser parser, CliOptions options)
    {
        ArgumentNullException.ThrowIfNull(parser);
        ArgumentNullException.ThrowIfNull(options);

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
        summary.AppendLine($"Compare: {options.Compare}");
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
    /// <param name="parser">The parser instance. Cannot be null.</param>
    /// <param name="options">Parsed options. Cannot be null.</param>
    /// <returns>True if help or version should be displayed; otherwise false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="parser"/> is null.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is null.</exception>
    public static bool ShouldShowHelpOrVersion(this CliArgumentParser parser, CliOptions options)
    {
        ArgumentNullException.ThrowIfNull(parser);
        ArgumentNullException.ThrowIfNull(options);

        return options.Help || options.Version;
    }
}