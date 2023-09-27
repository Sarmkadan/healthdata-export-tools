# CliOptions

`CliOptions` is a configuration class used to encapsulate command-line arguments and settings for the health data export process. It centralizes input/output paths, database connections, export formats, and operational flags to control validation, analysis, comparison, and performance behaviors.

## API

### `public string InputPath`
Specifies the input file or directory path containing health data to be processed. Must be a valid filesystem path. No validation is performed by the property itself; invalid paths may cause failures during subsequent operations.

### `public string OutputPath`
Defines the target directory where processed outputs (e.g., exported files, reports) will be written. The directory must exist or be creatable by the application; otherwise, operations using this path will fail.

### `public string DatabasePath`
Path to the database file or connection string used for storing or retrieving health data. Required for operations involving data persistence or queries. Invalid or inaccessible paths will result in runtime exceptions during database operations.

### `public string Format`
Determines the output format for exported data (e.g., CSV, JSON, XML). Accepted values depend on application-specific formatters. Unsupported formats may cause export failures or warnings.

### `public string Device`
Identifier for the source device or system generating the health data. Used to filter or tag data during processing. No format constraints are enforced by the property.

### `public string DataType`
Specifies the type of health data being processed (e.g., heart rate, blood pressure). Used to select appropriate processing pipelines or validations. Case sensitivity depends on downstream logic.

### `public string? StartDate`
Optional start date (inclusive) for filtering data records. Format is application-defined (e.g., ISO 8601). When `null`, no lower bound is applied. Invalid formats may cause parsing errors.

### `public string? EndDate`
Optional end date (inclusive) for filtering data records. Format is application-defined. When `null`, no upper bound is applied. Invalid formats may cause parsing errors.

### `public bool Validate`
Enables or disables data validation during processing. When `true`, the application performs schema, integrity, and business rule checks. When `false`, validation is skipped, potentially improving performance but risking data quality issues.

### `public bool Analyze`
Toggles analytical processing (e.g., aggregations, trend detection) on the input data. When `true`, additional computations are performed and results may be included in outputs. When `false`, analysis is omitted.

### `public bool Compare`
Controls whether comparative operations (e.g., against baseline data or previous exports) are executed. When `true`, the application performs comparisons and may generate difference reports or metrics.

### `public bool Verbose`
Enables detailed logging output during execution. When `true`, the application emits progress, warnings, and diagnostic information to the console or log. When `false`, only essential messages are shown.

### `public bool Compress`
Determines whether output files should be compressed (e.g., using ZIP or GZIP). When `true`, exported files are compressed into archives. When `false`, files are written uncompressed.

### `public bool Parallel`
Enables parallel processing of independent data chunks or operations. When `true`, the application uses multiple threads to improve throughput. When `false`, operations run sequentially. Affects performance and resource usage.

### `public bool Help`
When `true`, triggers the display of usage instructions and available options. Typically used in CLI entry points to show help text. Setting this does not perform other operations.

### `public bool Version`
When `true`, triggers the display of the application version. Typically used in CLI entry points to show version information. Setting this does not perform other operations.

### `public int MaxParallelism`
Limits the maximum number of concurrent threads or tasks used during parallel processing. Must be a positive integer. Values greater than the system’s logical processor count may not improve performance and could degrade responsiveness.

### `public bool EnableCache`
Enables or disables internal caching of intermediate results (e.g., parsed data, computed metrics). When `true`, repeated operations may benefit from reduced I/O or recomputation. When `false`, cache is disabled.

### `public int CacheDurationMinutes`
Specifies the duration (in minutes) for which cached entries remain valid. Must be a non-negative integer. A value of `0` implies no expiration. Longer durations reduce recomputation but risk stale data.

## Usage
