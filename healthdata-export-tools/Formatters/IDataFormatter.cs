// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace HealthDataExportTools.Formatters;

/// <summary>
/// Defines the contract for formatting health data into specific output formats
/// </summary>
public interface IDataFormatter
{
    /// <summary>
    /// Get the file extension for this format
    /// </summary>
    string FileExtension { get; }

    /// <summary>
    /// Get human-readable name of the format
    /// </summary>
    string FormatName { get; }

    /// <summary>
    /// Check if formatter can handle the given data type
    /// </summary>
    bool CanFormat(Type dataType);

    /// <summary>
    /// Format a single health data record
    /// </summary>
    Task<string> FormatAsync(HealthDataRecord record);

    /// <summary>
    /// Format a collection of health data records
    /// </summary>
    Task<string> FormatCollectionAsync(List<HealthDataRecord> records);

    /// <summary>
    /// Format sleep data specifically
    /// </summary>
    Task<string> FormatSleepDataAsync(List<SleepData> sleepRecords);

    /// <summary>
    /// Format heart rate data specifically
    /// </summary>
    Task<string> FormatHeartRateDataAsync(List<HeartRateData> heartRateRecords);

    /// <summary>
    /// Format SpO2 data specifically
    /// </summary>
    Task<string> FormatSpO2DataAsync(List<SpO2Data> spo2Records);

    /// <summary>
    /// Format steps data specifically
    /// </summary>
    Task<string> FormatStepsDataAsync(List<StepsData> stepsRecords);

    /// <summary>
    /// Validate data before formatting
    /// </summary>
    Task<List<string>> ValidateAsync(List<HealthDataRecord> records);
}

/// <summary>
/// Formatter metadata and factory information
/// </summary>
public class FormatterInfo
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;
    public bool IsCompressible { get; set; }
    public int MaxRecordsPerFile { get; set; } = 10000;
}
