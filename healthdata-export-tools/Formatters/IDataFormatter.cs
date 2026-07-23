#nullable enable
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

    /// <summary>
    /// Write a collection of health data records to a stream asynchronously
    /// </summary>
    /// <param name="records">The records to format; must not be <c>null</c>.</param>
    /// <param name="writer">The text writer to write to; must not be <c>null</c>.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="records"/> or <paramref name="writer"/> is <c>null</c>.</exception>
    Task WriteAsync(IEnumerable<HealthDataRecord> records, TextWriter writer, CancellationToken cancellationToken = default);

    /// <summary>
    /// Write sleep data to a stream asynchronously
    /// </summary>
    /// <param name="sleepRecords">The sleep records to format; must not be <c>null</c>.</param>
    /// <param name="writer">The text writer to write to; must not be <c>null</c>.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepRecords"/> or <paramref name="writer"/> is <c>null</c>.</exception>
    Task WriteSleepDataAsync(IEnumerable<SleepData> sleepRecords, TextWriter writer, CancellationToken cancellationToken = default);

    /// <summary>
    /// Write heart rate data to a stream asynchronously
    /// </summary>
    /// <param name="heartRateRecords">The heart-rate records to format; must not be <c>null</c>.</param>
    /// <param name="writer">The text writer to write to; must not be <c>null</c>.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="heartRateRecords"/> or <paramref name="writer"/> is <c>null</c>.</exception>
    Task WriteHeartRateDataAsync(IEnumerable<HeartRateData> heartRateRecords, TextWriter writer, CancellationToken cancellationToken = default);

    /// <summary>
    /// Write SpO2 data to a stream asynchronously
    /// </summary>
    /// <param name="spo2Records">The SpO2 records to format; must not be <c>null</c>.</param>
    /// <param name="writer">The text writer to write to; must not be <c>null</c>.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="spo2Records"/> or <paramref name="writer"/> is <c>null</c>.</exception>
    Task WriteSpO2DataAsync(IEnumerable<SpO2Data> spo2Records, TextWriter writer, CancellationToken cancellationToken = default);

    /// <summary>
    /// Write steps data to a stream asynchronously
    /// </summary>
    /// <param name="stepsRecords">The steps records to format; must not be <c>null</c>.</param>
    /// <param name="writer">The text writer to write to; must not be <c>null</c>.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="stepsRecords"/> or <paramref name="writer"/> is <c>null</c>.</exception>
    Task WriteStepsDataAsync(IEnumerable<StepsData> stepsRecords, TextWriter writer, CancellationToken cancellationToken = default);
}

/// <summary>
/// Formatter metadata and factory information
/// </summary>
public sealed class FormatterInfo
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;
    public bool IsCompressible { get; set; }
    public int MaxRecordsPerFile { get; set; } = 10000;
}
