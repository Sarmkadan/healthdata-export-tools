# CorrelationEngineOptions
The `CorrelationEngineOptions` type in the `healthdata-export-tools` project provides a set of configuration options for a correlation engine, allowing users to customize the analysis of health data. This type enables fine-grained control over the correlation analysis process, including the window of analysis, significance thresholds, and parallel computation settings.

## API
The `CorrelationEngineOptions` type exposes the following public members:
* `AnalysisWindowDays`: An integer specifying the number of days to consider for the analysis window.
* `SignificanceThreshold`: A double representing the minimum significance required for a correlation to be considered significant.
* `MinimumSampleCount`: An integer specifying the minimum number of samples required for a correlation to be considered valid.
* `IncludeWeakCorrelations`: A boolean indicating whether to include weak correlations in the analysis results.
* `ComputeAllPairs`: A boolean indicating whether to compute correlations for all pairs of metrics.
* `EnableParallelComputation`: A boolean indicating whether to enable parallel computation for the correlation analysis.
* `MaxDegreeOfParallelism`: An integer specifying the maximum degree of parallelism to use when computing correlations in parallel.
* `InsightMode`: An `InsightGenerationMode` enum value specifying the mode for generating insights from the correlation analysis.
* `AdditionalMetricPairs`: A list of `CorrelationPair` objects specifying additional metric pairs to consider in the analysis.
* `MaxLagDays`: An integer specifying the maximum number of days to consider for lagged correlations.
* `Validate`: An enumerable of strings representing validation errors or warnings.
* `IsValid`: A boolean indicating whether the options are valid.

## Usage
Here are two examples of using the `CorrelationEngineOptions` type:
```csharp
// Example 1: Basic usage
var options = new CorrelationEngineOptions
{
    AnalysisWindowDays = 30,
    SignificanceThreshold = 0.05,
    MinimumSampleCount = 10,
    IncludeWeakCorrelations = false,
    ComputeAllPairs = true,
    EnableParallelComputation = true,
    MaxDegreeOfParallelism = 4,
    InsightMode = InsightGenerationMode.Auto,
    AdditionalMetricPairs = new List<CorrelationPair>(),
    MaxLagDays = 7
};

// Example 2: Advanced usage with custom metric pairs
var customPairs = new List<CorrelationPair>
{
    new CorrelationPair("Metric1", "Metric2"),
    new CorrelationPair("Metric3", "Metric4")
};

var advancedOptions = new CorrelationEngineOptions
{
    AnalysisWindowDays = 60,
    SignificanceThreshold = 0.01,
    MinimumSampleCount = 20,
    IncludeWeakCorrelations = true,
    ComputeAllPairs = false,
    EnableParallelComputation = true,
    MaxDegreeOfParallelism = 8,
    InsightMode = InsightGenerationMode.Manual,
    AdditionalMetricPairs = customPairs,
    MaxLagDays = 14
};
```

## Notes
When using the `CorrelationEngineOptions` type, consider the following edge cases and thread-safety remarks:
* The `AnalysisWindowDays` and `MaxLagDays` properties must be non-negative integers.
* The `SignificanceThreshold` property must be a value between 0 and 1.
* The `MinimumSampleCount` property must be a positive integer.
* The `EnableParallelComputation` property may impact performance and should be used judiciously.
* The `MaxDegreeOfParallelism` property should be set based on the available computational resources.
* The `InsightMode` property determines the mode for generating insights and should be chosen based on the specific use case.
* The `AdditionalMetricPairs` property allows for custom metric pairs to be considered in the analysis.
* The `Validate` property returns an enumerable of validation errors or warnings, which should be checked to ensure the options are valid.
* The `IsValid` property indicates whether the options are valid and should be checked before using the correlation engine.
* The `CorrelationEngineOptions` type is not thread-safe and should not be shared across multiple threads.
