#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;
using HealthDataExportTools.Domain.Enums;
using HealthDataExportTools.Domain.Models;

namespace HealthDataExportTools.Services;

/// <summary>
/// Orchestrates the full export pipeline: validate, transform, export
/// </summary>
public sealed class ExportPipeline
{
    private readonly ILogger<ExportPipeline> _logger;
    private readonly DataValidator _validator;
    private readonly ExportService _exportService;
    private readonly HealthDataParserService _parser;

    /// <summary>
    /// Initializes a new instance of <see cref="ExportPipeline"/>
    /// </summary>
    public ExportPipeline(
        ILogger<ExportPipeline> logger,
        DataValidator validator,
        ExportService exportService,
        HealthDataParserService parser)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _exportService = exportService ?? throw new ArgumentNullException(nameof(exportService));
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
    }

    /// <summary>
    /// Run the full pipeline on an already-parsed collection: validate and export
    /// </summary>
    /// <param name="collection">Parsed health data collection</param>
    /// <param name="outputDirectory">Directory for exported files</param>
    /// <param name="format">Target export format</param>
    /// <returns>Pipeline execution result</returns>
    public async Task<PipelineResult> ExecuteAsync(
        HealthDataCollection collection,
        string outputDirectory,
        ExportFormat format = ExportFormat.Json)
    {
        ArgumentNullException.ThrowIfNull(collection);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);

        var result = new PipelineResult { StartedUtc = DateTime.UtcNow };
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Pipeline started: {Count} records -> {Output} ({Format})",
                collection.GetTotalRecordCount(), outputDirectory, format);

            result.RecordsParsed = collection.GetTotalRecordCount();

            // Step 1: Validate
            var issues = _validator.ValidateCollection(collection);
            result.ValidationIssues = issues.Count;
            if (issues.Count > 0)
                _logger.LogWarning("{Count} validation issues found", issues.Count);

            // Step 2: Deduplicate
            var duplicates = _validator.FindDuplicates(collection);
            result.DuplicatesRemoved = duplicates.Count;

            // Step 3: Export
            Directory.CreateDirectory(outputDirectory);
            var outputPath = Path.Combine(outputDirectory,
                $"health-export-{DateTime.UtcNow:yyyyMMdd-HHmmss}");

            await _exportService.ExportToJsonAsync(collection, outputPath + ".json")
                .ConfigureAwait(false);

            result.OutputPath = outputPath;
            result.Success = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Pipeline failed");
            result.ErrorMessage = ex.Message;
            result.Success = false;
        }
        finally
        {
            stopwatch.Stop();
            result.Duration = stopwatch.Elapsed;
            result.CompletedUtc = DateTime.UtcNow;
        }

        return result;
    }

    /// <summary>
    /// Parse a JSON string and run the full pipeline
    /// </summary>
    /// <param name="jsonContent">Raw JSON health data</param>
    /// <param name="outputDirectory">Directory for exported files</param>
    /// <param name="format">Target export format</param>
    /// <returns>Pipeline execution result</returns>
    public async Task<PipelineResult> ExecuteFromJsonAsync(
        string jsonContent,
        string outputDirectory,
        ExportFormat format = ExportFormat.Json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jsonContent);
        var collection = await _parser.ParseJsonAsync(jsonContent).ConfigureAwait(false);
        return await ExecuteAsync(collection, outputDirectory, format).ConfigureAwait(false);
    }

    /// <summary>
    /// Run validation-only pass without exporting
    /// </summary>
    /// <param name="collection">Parsed health data collection</param>
    /// <returns>Validation issues found</returns>
    public List<DataValidationIssue> ValidateOnly(HealthDataCollection collection)
    {
        ArgumentNullException.ThrowIfNull(collection);
        return _validator.ValidateCollection(collection);
    }

    /// <summary>
    /// Get a summary of record counts by type
    /// </summary>
    /// <param name="collection">The collection to summarize</param>
    /// <returns>Record counts by type</returns>
    public Dictionary<string, int> Preview(HealthDataCollection collection)
    {
        ArgumentNullException.ThrowIfNull(collection);

        return new Dictionary<string, int>
        {
            ["Sleep"] = collection.SleepRecords.Count,
            ["HeartRate"] = collection.HeartRateRecords.Count,
            ["SpO2"] = collection.SpO2Records.Count,
            ["Steps"] = collection.StepsRecords.Count,
            ["Activity"] = collection.ActivityRecords.Count,
            ["Metrics"] = collection.Metrics.Count
        };
    }
}

/// <summary>
/// Result of a pipeline execution
/// </summary>
public sealed class PipelineResult
{
    /// <summary>Whether the pipeline completed successfully</summary>
    public bool Success { get; set; }

    /// <summary>Total records parsed from source</summary>
    public int RecordsParsed { get; set; }

    /// <summary>Number of validation issues found</summary>
    public int ValidationIssues { get; set; }

    /// <summary>Number of duplicate records removed</summary>
    public int DuplicatesRemoved { get; set; }

    /// <summary>Output file path (without extension)</summary>
    public string? OutputPath { get; set; }

    /// <summary>Error message if the pipeline failed</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Pipeline start time</summary>
    public DateTime StartedUtc { get; set; }

    /// <summary>Pipeline completion time</summary>
    public DateTime? CompletedUtc { get; set; }

    /// <summary>Total pipeline duration</summary>
    public TimeSpan Duration { get; set; }
}
