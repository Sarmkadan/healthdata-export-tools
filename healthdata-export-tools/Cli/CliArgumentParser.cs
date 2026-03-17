// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace HealthDataExportTools.Cli;

/// <summary>
/// Parses command-line arguments into structured options
/// </summary>
public class CliArgumentParser
{
    private readonly Dictionary<string, Action<string>> _argumentMap;
    private readonly CliOptions _options;

    public CliArgumentParser()
    {
        _options = new CliOptions();
        _argumentMap = InitializeArgumentMap();
    }

    /// <summary>
    /// Parse command-line arguments and return options object
    /// </summary>
    public CliOptions Parse(string[] args)
    {
        // Process each argument as key-value pairs
        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];

            // Handle flags like --verbose, --help
            if (arg.StartsWith("--"))
            {
                string key = arg[2..];
                if (_argumentMap.TryGetValue(key, out var handler))
                {
                    // Check if this flag expects a value
                    if (RequiresValue(key) && i + 1 < args.Length)
                    {
                        handler(args[++i]);
                    }
                    else if (!RequiresValue(key))
                    {
                        handler("");
                    }
                }
            }
            // Handle short options like -v, -h
            else if (arg.StartsWith("-") && arg.Length == 2)
            {
                string shortFlag = arg[1].ToString();
                ProcessShortFlag(shortFlag, args, ref i);
            }
        }

        return _options;
    }

    /// <summary>
    /// Validate parsed options for correctness and consistency
    /// </summary>
    public List<string> Validate()
    {
        var errors = new List<string>();

        // Validate input path exists
        if (!Directory.Exists(_options.InputPath))
            errors.Add($"Input path does not exist: {_options.InputPath}");

        // Validate date format if provided
        if (!string.IsNullOrEmpty(_options.StartDate))
        {
            if (!DateTime.TryParse(_options.StartDate, out _))
                errors.Add($"Invalid start date format: {_options.StartDate}");
        }

        if (!string.IsNullOrEmpty(_options.EndDate))
        {
            if (!DateTime.TryParse(_options.EndDate, out _))
                errors.Add($"Invalid end date format: {_options.EndDate}");
        }

        // Validate date range
        if (!string.IsNullOrEmpty(_options.StartDate) && !string.IsNullOrEmpty(_options.EndDate))
        {
            if (DateTime.TryParse(_options.StartDate, out var start) &&
                DateTime.TryParse(_options.EndDate, out var end))
            {
                if (start > end)
                    errors.Add("Start date cannot be after end date");
            }
        }

        // Validate format option
        var validFormats = new[] { "json", "csv", "sqlite", "xml", "all" };
        if (!validFormats.Contains(_options.Format.ToLower()))
            errors.Add($"Invalid format: {_options.Format}. Valid options: {string.Join(", ", validFormats)}");

        // Validate parallelism
        if (_options.MaxParallelism < 1 || _options.MaxParallelism > Environment.ProcessorCount)
            errors.Add($"Max parallelism must be between 1 and {Environment.ProcessorCount}");

        return errors;
    }

    /// <summary>
    /// Get help text for CLI usage
    /// </summary>
    public static string GetHelpText()
    {
        return """
            Health Data Export Tools v1.0.0

            Usage: healthdata-export-tools [OPTIONS]

            Options:
              --input <path>              Input directory containing health exports (default: ./exports)
              --output <path>             Output directory for exports (default: ./output)
              --database <path>           SQLite database path (default: ./healthdata.db)
              --format <format>           Export format: json, csv, sqlite, xml, all (default: all)
              --device <type>             Device type: zepp, amazfit, garmin, all (default: all)
              --data-type <type>          Data type: sleep, heart, spo2, steps, activity, all (default: all)
              --start-date <date>         Filter start date (yyyy-MM-dd)
              --end-date <date>           Filter end date (yyyy-MM-dd)
              --validate                  Enable data validation (default: enabled)
              --analyze                   Perform analytics on data
              --compress                  Compress output files
              --no-parallel               Disable parallel processing
              --max-parallelism <n>       Set maximum parallel tasks
              --cache                     Enable result caching (default: enabled)
              --cache-duration <min>      Cache duration in minutes (default: 60)
              --verbose                   Enable verbose logging
              -v, --version               Show version information
              -h, --help                  Show this help message

            Examples:
              healthdata-export-tools --input ./exports --format json --analyze
              healthdata-export-tools --input ./exports --device amazfit --start-date 2025-01-01
              healthdata-export-tools --input ./exports --format csv --compress --validate
            """;
    }

    private Dictionary<string, Action<string>> InitializeArgumentMap()
    {
        return new Dictionary<string, Action<string>>(StringComparer.OrdinalIgnoreCase)
        {
            { "input", v => _options.InputPath = v },
            { "output", v => _options.OutputPath = v },
            { "database", v => _options.DatabasePath = v },
            { "format", v => _options.Format = v },
            { "device", v => _options.Device = v },
            { "data-type", v => _options.DataType = v },
            { "start-date", v => _options.StartDate = v },
            { "end-date", v => _options.EndDate = v },
            { "validate", v => _options.Validate = true },
            { "analyze", v => _options.Analyze = true },
            { "compress", v => _options.Compress = true },
            { "verbose", v => _options.Verbose = true },
            { "help", v => _options.Help = true },
            { "version", v => _options.Version = true },
            { "no-parallel", v => _options.Parallel = false },
            { "max-parallelism", v => _options.MaxParallelism = int.Parse(v) },
            { "cache", v => _options.EnableCache = true },
            { "no-cache", v => _options.EnableCache = false },
            { "cache-duration", v => _options.CacheDurationMinutes = int.Parse(v) },
        };
    }

    private bool RequiresValue(string key)
    {
        var flagsRequiringValue = new[]
        {
            "input", "output", "database", "format", "device",
            "data-type", "start-date", "end-date", "max-parallelism", "cache-duration"
        };
        return flagsRequiringValue.Contains(key);
    }

    private void ProcessShortFlag(string flag, string[] args, ref int index)
    {
        switch (flag)
        {
            case "h":
                _options.Help = true;
                break;
            case "v":
                _options.Version = true;
                break;
        }
    }
}
