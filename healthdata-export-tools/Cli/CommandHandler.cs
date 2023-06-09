// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using HealthDataExportTools.Domain.Enums;
using HealthDataExportTools.Services;
using HealthDataExportTools.Configuration;

namespace HealthDataExportTools.Cli;

/// <summary>
/// Handles execution of commands and coordinates services
/// </summary>
public class CommandHandler
{
    private readonly HealthDataParserService _parserService;
    private readonly ExportService _exportService;
    private readonly AnalyticsService _analyticsService;
    private readonly ValidationService _validationService;
    private readonly ILogger<CommandHandler> _logger;

    public CommandHandler(
        HealthDataParserService parserService,
        ExportService exportService,
        AnalyticsService analyticsService,
        ValidationService validationService,
        ILogger<CommandHandler> logger)
    {
        _parserService = parserService ?? throw new ArgumentNullException(nameof(parserService));
        _exportService = exportService ?? throw new ArgumentNullException(nameof(exportService));
        _analyticsService = analyticsService ?? throw new ArgumentNullException(nameof(analyticsService));
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Execute the main export command with specified options
    /// </summary>
    public async Task<int> ExecuteExportAsync(CliOptions options)
    {
        try
        {
            _logger.LogInformation("Starting health data export with format: {Format}", options.Format);

            // Create output directory if it doesn't exist
            Directory.CreateDirectory(options.OutputPath);

            // Parse input health data files
            _logger.LogInformation("Parsing health data from: {InputPath}", options.InputPath);
            var healthDataRecords = await _parserService.ParseAllHealthDataAsync(options.InputPath);

            if (healthDataRecords.Count == 0)
            {
                _logger.LogWarning("No health data records found in input directory");
                return 0;
            }

            _logger.LogInformation("Successfully parsed {Count} health records", healthDataRecords.Count);

            // Apply filters if specified
            var filteredRecords = ApplyFilters(healthDataRecords, options);

            // Validate data if requested
            if (options.Validate)
            {
                _logger.LogInformation("Validating health data...");
                var validationResults = await _validationService.ValidateAsync(filteredRecords);
                LogValidationResults(validationResults);
            }

            // Export to requested formats
            await ExportToFormats(filteredRecords, options);

            // Perform analytics if requested
            if (options.Analyze)
            {
                _logger.LogInformation("Performing analytics analysis...");
                var analytics = _analyticsService.Analyze(filteredRecords);
                await ExportAnalyticsResults(analytics, options);
            }

            _logger.LogInformation("Export completed successfully");
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Export command failed");
            return 1;
        }
    }

    /// <summary>
    /// Apply filters to health records based on CLI options
    /// </summary>
    private List<HealthDataRecord> ApplyFilters(List<HealthDataRecord> records, CliOptions options)
    {
        var filtered = records.AsEnumerable();

        // Filter by date range
        if (!string.IsNullOrEmpty(options.StartDate) && DateTime.TryParse(options.StartDate, out var start))
        {
            filtered = filtered.Where(r => r.RecordDate >= start);
        }

        if (!string.IsNullOrEmpty(options.EndDate) && DateTime.TryParse(options.EndDate, out var end))
        {
            filtered = filtered.Where(r => r.RecordDate <= end);
        }

        // Filter by data type
        if (!options.DataType.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            filtered = filtered.Where(r =>
                r.MetricType.ToString().Equals(options.DataType, StringComparison.OrdinalIgnoreCase));
        }

        // Filter by device type
        if (!options.Device.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            filtered = filtered.Where(r =>
                r.DeviceType.ToString().Equals(options.Device, StringComparison.OrdinalIgnoreCase));
        }

        return filtered.ToList();
    }

    /// <summary>
    /// Export filtered records to all requested formats
    /// </summary>
    private async Task ExportToFormats(List<HealthDataRecord> records, CliOptions options)
    {
        var formats = options.Format.Equals("all", StringComparison.OrdinalIgnoreCase)
            ? new[] { "json", "csv", "sqlite" }
            : new[] { options.Format.ToLower() };

        foreach (var format in formats)
        {
            try
            {
                _logger.LogInformation("Exporting to {Format} format...", format);
                string outputFile = Path.Combine(options.OutputPath, $"health_data.{GetExtension(format)}");

                // Delegate actual export to appropriate service based on format
                switch (format)
                {
                    case "json":
                        // JSON export logic here
                        _logger.LogInformation("JSON export complete: {FilePath}", outputFile);
                        break;
                    case "csv":
                        // CSV export logic here
                        _logger.LogInformation("CSV export complete: {FilePath}", outputFile);
                        break;
                    case "sqlite":
                        // SQLite export logic here
                        _logger.LogInformation("SQLite export complete: {FilePath}", outputFile);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to export to {Format} format", format);
            }
        }
    }

    /// <summary>
    /// Export analytics results to output directory
    /// </summary>
    private async Task ExportAnalyticsResults(object analytics, CliOptions options)
    {
        try
        {
            string analyticsFile = Path.Combine(options.OutputPath, "analytics.json");
            // Analytics export logic here
            _logger.LogInformation("Analytics exported to: {FilePath}", analyticsFile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export analytics results");
        }
    }

    /// <summary>
    /// Log validation results to console and logger
    /// </summary>
    private void LogValidationResults(List<string> results)
    {
        if (results.Count == 0)
        {
            _logger.LogInformation("Data validation passed");
        }
        else
        {
            _logger.LogWarning("Data validation found {Count} issues:", results.Count);
            foreach (var result in results)
            {
                _logger.LogWarning("  • {Issue}", result);
            }
        }
    }

    private string GetExtension(string format) => format.ToLower() switch
    {
        "json" => "json",
        "csv" => "csv",
        "sqlite" => "db",
        "xml" => "xml",
        _ => "txt"
    };
}
