#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace HealthDataExportTools.Cli;

/// <summary>
/// Parses command-line arguments into structured options
/// </summary>
public sealed class CliArgumentParser
{
    private readonly Dictionary<string, Action<string>> _argumentMap;
    private readonly HashSet<string> _knownFlags;
    private readonly CliOptions _options;
    private readonly List<string> _validationErrors;

    public CliArgumentParser()
    {
        _options = new CliOptions();
        _validationErrors = [];
        _argumentMap = InitializeArgumentMap();
        _knownFlags = new HashSet<string>(_argumentMap.Keys, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Calculate Levenshtein distance between two strings
    /// </summary>
    /// <param name="a">First string</param>
    /// <param name="b">Second string</param>
    /// <returns>Levenshtein distance</returns>
    private static int LevenshteinDistance(string? a, string? b)
    {
        if (string.IsNullOrEmpty(a)) return string.IsNullOrEmpty(b) ? 0 : b!.Length;
        if (string.IsNullOrEmpty(b)) return a!.Length;

        int[,] distance = new int[a.Length + 1, b.Length + 1];

        for (int i = 0; i <= a.Length; i++)
            distance[i, 0] = i;
        for (int j = 0; j <= b.Length; j++)
            distance[0, j] = j;

        for (int i = 1; i <= a.Length; i++)
        {
            for (int j = 1; j <= b.Length; j++)
            {
                int cost = (a[i - 1] == b[j - 1]) ? 0 : 1;
                distance[i, j] = Math.Min(
                    Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                    distance[i - 1, j - 1] + cost);
            }
        }

        return distance[a.Length, b.Length];
    }

    /// <summary>
    /// Get suggestion for unknown flag using Levenshtein distance
    /// </summary>
    /// <param name="unknownFlag">Unknown flag to find suggestion for</param>
    /// <returns>Suggested known flag or null if no good match found</returns>
    private string? GetSuggestion(string unknownFlag)
    {
        const int maxDistance = 2;
        string? bestMatch = null;
        int minDistance = int.MaxValue;

        foreach (var knownFlag in _knownFlags)
        {
            int distance = LevenshteinDistance(unknownFlag, knownFlag);
            if (distance < minDistance && distance <= maxDistance)
            {
                minDistance = distance;
                bestMatch = knownFlag;
            }
        }

        return bestMatch;
    }

    /// <summary>
    /// Parse command-line arguments and return parse result
    /// </summary>
    /// <param name="args">Command-line arguments to parse</param>
    /// <returns>Parse result containing options or errors</returns>
    /// <exception cref="ArgumentNullException"><paramref name="args"/> is null.</exception>
    public ParseResult<CliOptions> Parse(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        _validationErrors.Clear();

        // Process each argument as key-value pairs
        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];

            // Handle flags like --verbose, --help
            if (arg.StartsWith("--"))
            {
                string key = arg[2..];

                if (!_knownFlags.Contains(key))
                {
                    // Unknown flag - suggest similar known flags
                    string? suggestion = GetSuggestion(key);
                    _validationErrors.Add(suggestion != null
                        ? $"Unknown option '--{key}'. Did you mean '--{suggestion}'?"
                        : $"Unknown option '--{key}'");
                    continue;
                }

                if (_argumentMap.TryGetValue(key, out var handler))
                {
                    // Check if this flag expects a value
                    if (RequiresValue(key) && i + 1 < args.Length)
                    {
                        string value = args[++i];
                        try
                        {
                            handler(value);
                        }
                        catch (Exception ex)
                        {
                            _validationErrors.Add($"Invalid value for '--{key}': {ex.Message}");
                        }
                    }
                    else if (!RequiresValue(key))
                    {
                        handler("");
                    }
                    else
                    {
                        _validationErrors.Add($"Option '--{key}' requires a value but none was provided");
                    }
                }
            }
            // Handle short options like -v, -h
            else if (arg.StartsWith("-") && arg.Length == 2)
            {
                string shortFlag = arg[1].ToString();
                ProcessShortFlag(shortFlag, args, ref i);
            }
            else if (i == 0 && string.Equals(arg, "verify", StringComparison.OrdinalIgnoreCase))
            {
                // "verify" as the first positional token selects the manifest verification subcommand
                _options.Command = "verify";
            }
            else
            {
                // Positional argument - treat as input path
                _options.InputPath = arg;
            }
        }

        // Validate at parse time
        ValidateAtParseTime();

        if (_validationErrors.Count > 0)
        {
            return ParseResult<CliOptions>.FailureResult(_validationErrors, GetHelpText());
        }

        return ParseResult<CliOptions>.SuccessResult(_options, GetHelpText());
    }

    /// <summary>
    /// Validate parsed options for correctness and consistency at parse time
    /// </summary>
    private void ValidateAtParseTime()
    {
        // The "verify" subcommand checks a manifest file instead of processing an input directory
        if (string.Equals(_options.Command, "verify", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrEmpty(_options.ManifestPath))
            {
                _validationErrors.Add("The 'verify' command requires --manifest <path>");
            }
            else if (!File.Exists(_options.ManifestPath))
            {
                _validationErrors.Add($"Manifest file does not exist: {_options.ManifestPath}");
            }

            return;
        }

        // Validate input path exists
        if (!string.IsNullOrEmpty(_options.InputPath) && !Directory.Exists(_options.InputPath))
        {
            _validationErrors.Add($"Input path does not exist: {_options.InputPath}");
        }

        // Validate date format if provided (using invariant culture for consistent parsing)
        // Accept various date formats but ensure they parse correctly
        if (!string.IsNullOrEmpty(_options.StartDate))
        {
            if (!DateTime.TryParse(_options.StartDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            {
                _validationErrors.Add($"Invalid start date format: {_options.StartDate}. Expected format: yyyy-MM-dd");
            }
        }

        if (!string.IsNullOrEmpty(_options.EndDate))
        {
            if (!DateTime.TryParse(_options.EndDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            {
                _validationErrors.Add($"Invalid end date format: {_options.EndDate}. Expected format: yyyy-MM-dd");
            }
        }

        // Validate date range
        if (!string.IsNullOrEmpty(_options.StartDate) && !string.IsNullOrEmpty(_options.EndDate))
        {
            if (DateTime.TryParse(_options.StartDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var start) &&
                DateTime.TryParse(_options.EndDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var end))
            {
                if (start > end)
                {
                    _validationErrors.Add("Start date cannot be after end date");
                }
            }
        }

        // Validate format option - use formats from CliOptions
        var validFormats = new[] { "json", "csv", "sqlite", "xml", "all" };
        if (!validFormats.Contains(_options.Format.ToLower()))
        {
            _validationErrors.Add($"Invalid format: {_options.Format}. Valid options: {string.Join(", ", validFormats)}");
        }

        // Validate parallelism
        if (_options.MaxParallelism < 1 || _options.MaxParallelism > Environment.ProcessorCount)
        {
            _validationErrors.Add($"Max parallelism must be between 1 and {Environment.ProcessorCount}");
        }

        // Validate cache duration
        if (_options.CacheDurationMinutes < 0)
        {
            _validationErrors.Add("Cache duration cannot be negative");
        }
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
            if (!DateTime.TryParse(_options.StartDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                errors.Add($"Invalid start date format: {_options.StartDate}");
        }

        if (!string.IsNullOrEmpty(_options.EndDate))
        {
            if (!DateTime.TryParse(_options.EndDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                errors.Add($"Invalid end date format: {_options.EndDate}");
        }

        // Validate date range
        if (!string.IsNullOrEmpty(_options.StartDate) && !string.IsNullOrEmpty(_options.EndDate))
        {
            if (DateTime.TryParse(_options.StartDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var start) &&
                DateTime.TryParse(_options.EndDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var end))
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
                   healthdata-export-tools verify --manifest <path>

            Commands:
              verify --manifest <path>    Recompute checksums/record counts for an export and
                                           compare them against a previously written manifest.json

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
              healthdata-export-tools verify --manifest ./output/manifest.json
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
            { "max-parallelism", v => _options.MaxParallelism = int.Parse(v, CultureInfo.InvariantCulture) },
            { "cache", v => _options.EnableCache = true },
            { "no-cache", v => _options.EnableCache = false },
            { "cache-duration", v => _options.CacheDurationMinutes = int.Parse(v, CultureInfo.InvariantCulture) },
            { "manifest", v => _options.ManifestPath = v },
        };
    }

    private bool RequiresValue(string key)
    {
        var flagsRequiringValue = new[]
        {
            "input", "output", "database", "format", "device",
            "data-type", "start-date", "end-date", "max-parallelism", "cache-duration", "manifest"
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
