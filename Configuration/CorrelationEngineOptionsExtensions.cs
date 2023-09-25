using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace HealthData.Export.Configuration
{
    /// <summary>
    /// Provides extension methods for <see cref="CorrelationEngineOptions"/> to enhance configuration and validation scenarios.
    /// </summary>
    public static class CorrelationEngineOptionsExtensions
    {
        /// <summary>
        /// Validates that the analysis window is within acceptable bounds for meaningful correlation analysis.
        /// </summary>
        /// <param name="options">The correlation engine options to validate.</param>
        /// <exception cref="ArgumentNullException"><paramref name="options"/> is null.</exception>
        /// <exception cref="ArgumentException">Analysis window is invalid.</exception>
        /// <returns>True if the analysis window is valid; otherwise, false.</returns>
        public static bool IsAnalysisWindowValid(this CorrelationEngineOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            return options.AnalysisWindowDays >= 7 && options.AnalysisWindowDays <= 365;
        }

        /// <summary>
        /// Gets the effective maximum lag days based on the configured analysis window.
        /// The effective lag is capped at 50% of the analysis window to ensure statistical significance.
        /// </summary>
        /// <param name="options">The correlation engine options.</param>
        /// <exception cref="ArgumentNullException"><paramref name="options"/> is null.</exception>
        /// <returns>The effective maximum lag days, never exceeding 50% of analysis window.</returns>
        public static int GetEffectiveMaxLagDays(this CorrelationEngineOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            var maxAllowedLag = options.AnalysisWindowDays / 2;
            return Math.Min(options.MaxLagDays, maxAllowedLag);
        }

        /// <summary>
        /// Determines whether parallel computation should be enabled based on the configured degree of parallelism.
        /// </summary>
        /// <param name="options">The correlation engine options.</param>
        /// <exception cref="ArgumentNullException"><paramref name="options"/> is null.</exception>
        /// <returns>True if parallel computation is enabled and has sufficient parallelism configured; otherwise, false.</returns>
        public static bool ShouldUseParallelComputation(this CorrelationEngineOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            return options.EnableParallelComputation && options.MaxDegreeOfParallelism > 1;
        }

        /// <summary>
        /// Creates a read-only list of correlation pairs that includes both the configured additional pairs
        /// and any default pairs that should be computed based on the insight generation mode.
        /// </summary>
        /// <param name="options">The correlation engine options.</param>
        /// <exception cref="ArgumentNullException"><paramref name="options"/> is null.</exception>
        /// <returns>A read-only list of correlation pairs to compute.</returns>
        public static IReadOnlyList<CorrelationPair> GetComputationPairs(this CorrelationEngineOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            var pairs = new List<CorrelationPair>();

            // Always include explicitly configured pairs
            if (options.AdditionalMetricPairs != null)
            {
                pairs.AddRange(options.AdditionalMetricPairs);
            }

            // Include default pairs based on insight mode
            if (options.InsightMode == InsightGenerationMode.Comprehensive)
            {
                // In comprehensive mode, we compute all possible pairs unless explicitly excluded
                // This is handled by the ComputeAllPairs flag in the engine itself
            }
            else if (options.InsightMode == InsightGenerationMode.Targeted)
            {
                // In targeted mode, we might want to add some standard pairs
                // For now, we rely on AdditionalMetricPairs for targeted scenarios
            }

            return pairs.AsReadOnly();
        }

        /// <summary>
        /// Validates that the significance threshold is within a reasonable range for correlation analysis.
        /// </summary>
        /// <param name="options">The correlation engine options to validate.</param>
        /// <exception cref="ArgumentNullException"><paramref name="options"/> is null.</exception>
        /// <exception cref="ArgumentException">Significance threshold is invalid.</exception>
        /// <returns>A collection of validation messages; empty if valid.</returns>
        public static IEnumerable<string> ValidateSignificanceThreshold(this CorrelationEngineOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            var messages = new List<string>();

            if (options.SignificanceThreshold <= 0)
            {
                messages.Add("SignificanceThreshold must be greater than 0.");
            }
            else if (options.SignificanceThreshold > 1.0)
            {
                messages.Add("SignificanceThreshold should not exceed 1.0 (100%).");
            }

            if (double.IsNaN(options.SignificanceThreshold))
            {
                messages.Add("SignificanceThreshold must be a valid number.");
            }

            if (double.IsInfinity(options.SignificanceThreshold))
            {
                messages.Add("SignificanceThreshold must be finite.");
            }

            return messages;
        }

        /// <summary>
        /// Gets the minimum sample count required for reliable correlation detection.
        /// Returns the configured value or a sensible default if not set.
        /// </summary>
        /// <param name="options">The correlation engine options.</param>
        /// <exception cref="ArgumentNullException"><paramref name="options"/> is null.</exception>
        /// <returns>The minimum sample count, never less than 10.</returns>
        public static int GetMinimumSampleCount(this CorrelationEngineOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            // Ensure minimum sample count is never less than 10 for statistical significance
            return Math.Max(options.MinimumSampleCount, 10);
        }

        /// <summary>
        /// Determines whether weak correlations should be included based on the configured threshold.
        /// </summary>
        /// <param name="options">The correlation engine options.</param>
        /// <exception cref="ArgumentNullException"><paramref name="options"/> is null.</exception>
        /// <returns>True if weak correlations should be included; otherwise, false.</returns>
        public static bool ShouldIncludeWeakCorrelations(this CorrelationEngineOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            return options.IncludeWeakCorrelations && options.SignificanceThreshold < 0.3;
        }
    }
}