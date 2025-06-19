// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace HealthDataExportTools.Formatters;

/// <summary>
/// Factory for creating and managing data formatters
/// Provides centralized access to all available formatters
/// </summary>
public class FormatterFactory
{
    private readonly Dictionary<string, IDataFormatter> _formatters;
    private readonly ILogger<FormatterFactory> _logger;

    public FormatterFactory(ILogger<FormatterFactory> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _formatters = new Dictionary<string, IDataFormatter>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Register a formatter
    /// </summary>
    public void RegisterFormatter(string formatName, IDataFormatter formatter)
    {
        // Fix: Use IsNullOrWhiteSpace for better boundary checking and improve exception message with actual value
        if (string.IsNullOrWhiteSpace(formatName))
            throw new ArgumentException($"Format name cannot be null or empty. Received: '{formatName}'", nameof(formatName));

        if (formatter == null)
            throw new ArgumentNullException(nameof(formatter), "Formatter instance cannot be null when registering a new format.");

        _formatters[formatName] = formatter;
        _logger.LogInformation("Formatter registered: {Format}", formatName);
    }

    /// <summary>
    /// Get formatter by format name
    /// </summary>
    public IDataFormatter? GetFormatter(string formatName)
    {
        if (string.IsNullOrWhiteSpace(formatName))
            return null;

        if (_formatters.TryGetValue(formatName, out var formatter))
        {
            return formatter;
        }

        _logger.LogWarning("Formatter not found: {Format}", formatName);
        return null;
    }

    /// <summary>
    /// Get all registered formatters
    /// </summary>
    public List<IDataFormatter> GetAllFormatters()
    {
        return _formatters.Values.ToList();
    }

    /// <summary>
    /// Get list of supported formats
    /// </summary>
    public List<string> GetSupportedFormats()
    {
        return _formatters.Keys.ToList();
    }

    /// <summary>
    /// Check if format is supported
    /// </summary>
    public bool IsSupportedFormat(string formatName)
    {
        return _formatters.ContainsKey(formatName);
    }

    /// <summary>
    /// Create default factory with standard formatters
    /// </summary>
    public static FormatterFactory CreateDefault(ILogger<FormatterFactory> logger, ILoggerFactory loggerFactory)
    {
        var factory = new FormatterFactory(logger);

        // Register standard formatters
        factory.RegisterFormatter("json", new JsonFormatter(loggerFactory.CreateLogger<JsonFormatter>()));
        factory.RegisterFormatter("csv", new CsvFormatter(loggerFactory.CreateLogger<CsvFormatter>()));
        factory.RegisterFormatter("xml", new XmlFormatter(loggerFactory.CreateLogger<XmlFormatter>()));

        return factory;
    }

    /// <summary>
    /// Create formatter based on file extension
    /// </summary>
    public IDataFormatter? GetFormatterByExtension(string extension)
    {
        if (string.IsNullOrEmpty(extension))
            return null;

        // Normalize extension
        var cleanExtension = extension.TrimStart('.').ToLower();

        var formatter = _formatters.Values.FirstOrDefault(f =>
            f.FileExtension.TrimStart('.').Equals(cleanExtension, StringComparison.OrdinalIgnoreCase));

        return formatter;
    }

    /// <summary>
    /// Format data to all supported formats
    /// </summary>
    public async Task<Dictionary<string, string>> FormatToAllAsync(List<HealthDataRecord> records)
    {
        var results = new Dictionary<string, string>();

        foreach (var formatter in _formatters.Values)
        {
            try
            {
                var formatted = await formatter.FormatCollectionAsync(records);
                results[formatter.FormatName] = formatted;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error formatting to {Format}", formatter.FormatName);
            }
        }

        return results;
    }

    /// <summary>
    /// Get formatter information
    /// </summary>
    public List<FormatterInfo> GetFormatterInfo()
    {
        return _formatters.Values.Select(f => new FormatterInfo
        {
            Name = f.FormatName,
            FileExtension = f.FileExtension,
            Description = GetFormatterDescription(f.FormatName),
            IsCompressible = f.FormatName.Equals("json", StringComparison.OrdinalIgnoreCase) ||
                           f.FormatName.Equals("xml", StringComparison.OrdinalIgnoreCase)
        }).ToList();
    }

    /// <summary>
    /// Get formatter description
    /// </summary>
    private string GetFormatterDescription(string formatName)
    {
        return formatName.ToLower() switch
        {
            "json" => "JavaScript Object Notation - human readable, structured format",
            "csv" => "Comma-Separated Values - spreadsheet compatible format",
            "xml" => "Extensible Markup Language - hierarchical structured format",
            _ => "Custom format"
        };
    }
}
