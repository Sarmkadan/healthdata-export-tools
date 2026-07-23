#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace HealthDataExportTools.Cli;

/// <summary>
/// Represents the result of command-line argument parsing
/// </summary>
/// <typeparam name="T">The type of options parsed</typeparam>
public sealed class ParseResult<T> where T : class
{
    /// <summary>
    /// Gets a value indicating whether parsing was successful
    /// </summary>
    public bool Success { get; }

    /// <summary>
    /// Gets the parsed options if successful; otherwise null
    /// </summary>
    public T? Options { get; }

    /// <summary>
    /// Gets the list of validation errors if parsing failed
    /// </summary>
    public IReadOnlyList<string> Errors { get; }

    /// <summary>
    /// Gets the help text to display when parsing fails or help is requested
    /// </summary>
    public string HelpText { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ParseResult{T}"/> class
    /// </summary>
    /// <param name="success">Whether parsing was successful</param>
    /// <param name="options">The parsed options if successful</param>
    /// <param name="errors">List of validation errors</param>
    /// <param name="helpText">Help text to display</param>
    public ParseResult(bool success, T? options, List<string> errors, string helpText)
    {
        Success = success;
        Options = options;
        Errors = errors.AsReadOnly();
        HelpText = helpText;
    }

    /// <summary>
    /// Creates a successful parse result
    /// </summary>
    /// <param name="options">The parsed options</param>
    /// <param name="helpText">Help text to display</param>
    public static ParseResult<T> SuccessResult(T options, string helpText)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrEmpty(helpText);

        return new ParseResult<T>(true, options, [], helpText);
    }

    /// <summary>
    /// Creates a failed parse result with validation errors
    /// </summary>
    /// <param name="errors">List of validation errors</param>
    /// <param name="helpText">Help text to display</param>
    public static ParseResult<T> FailureResult(List<string> errors, string helpText)
    {
        ArgumentException.ThrowIfNullOrEmpty(helpText);
        ArgumentNullException.ThrowIfNull(errors);

        return new ParseResult<T>(false, null, errors, helpText);
    }
}