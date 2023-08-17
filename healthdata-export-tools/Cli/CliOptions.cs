// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace HealthDataExportTools.Cli;

/// <summary>
/// Encapsulates command-line options and arguments
/// </summary>
public class CliOptions
{
    /// <summary>
    /// Input directory containing health data exports
    /// </summary>
    public string InputPath { get; set; } = "./exports";

    /// <summary>
    /// Output directory for exported files
    /// </summary>
    public string OutputPath { get; set; } = "./output";

    /// <summary>
    /// Path to SQLite database file
    /// </summary>
    public string DatabasePath { get; set; } = "./healthdata.db";

    /// <summary>
    /// Export format: json, csv, sqlite, all
    /// </summary>
    public string Format { get; set; } = "all";

    /// <summary>
    /// Device type filter: zepp, amazfit, garmin, all
    /// </summary>
    public string Device { get; set; } = "all";

    /// <summary>
    /// Data type to process: sleep, heart, spo2, steps, activity, all
    /// </summary>
    public string DataType { get; set; } = "all";

    /// <summary>
    /// Start date for data filter (yyyy-MM-dd)
    /// </summary>
    public string? StartDate { get; set; }

    /// <summary>
    /// End date for data filter (yyyy-MM-dd)
    /// </summary>
    public string? EndDate { get; set; }

    /// <summary>
    /// Enable validation of input data
    /// </summary>
    public bool Validate { get; set; } = true;

    /// <summary>
    /// Perform analytics on exported data
    /// </summary>
    public bool Analyze { get; set; } = false;

    /// <summary>
    /// Enable verbose console output
    /// </summary>
    public bool Verbose { get; set; } = false;

    /// <summary>
    /// Compress output files
    /// </summary>
    public bool Compress { get; set; } = false;

    /// <summary>
    /// Enable concurrent processing for large datasets
    /// </summary>
    public bool Parallel { get; set; } = true;

    /// <summary>
    /// Show help information
    /// </summary>
    public bool Help { get; set; } = false;

    /// <summary>
    /// Show version information
    /// </summary>
    public bool Version { get; set; } = false;

    /// <summary>
    /// Maximum number of parallel tasks for concurrent processing
    /// </summary>
    public int MaxParallelism { get; set; } = Environment.ProcessorCount;

    /// <summary>
    /// Cache results to improve performance
    /// </summary>
    public bool EnableCache { get; set; } = true;

    /// <summary>
    /// Cache duration in minutes
    /// </summary>
    public int CacheDurationMinutes { get; set; } = 60;
}
