#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace HealthDataExportTools.DTOs
{
    /// <summary>
    /// Provides extension methods for <see cref="CorrelationAnalysisResultDto"/>.
    /// </summary>
    public static class CorrelationAnalysisResultDtoExtensions
    {
        /// <summary>
        /// Returns the strongest correlation by absolute coefficient value, or null if no correlations exist.
        /// This is a more explicit version of the existing StrongestCorrelation property.
        /// </summary>
        /// <param name="result">The correlation analysis result.</param>
        /// <returns>The strongest correlation, or null if no correlations exist.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is null.</exception>
        public static MetricCorrelationDto? GetStrongestCorrelation(this CorrelationAnalysisResultDto result)
        {
            ArgumentNullException.ThrowIfNull(result);
            return result.Correlations.Count > 0
                ? result.Correlations.MaxBy(c => c.AbsoluteCoefficient)
                : null;
        }

        /// <summary>
        /// Filters correlations by a custom significance threshold.
        /// </summary>
        /// <param name="result">The correlation analysis result.</param>
        /// <param name="threshold">The minimum absolute coefficient value to consider significant (0 to 1).</param>
        /// <returns>A list of correlations meeting the threshold, ordered by absolute coefficient descending.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="threshold"/> is less than 0 or greater than 1.</exception>
        public static IReadOnlyList<MetricCorrelationDto> GetCorrelationsAboveThreshold(this CorrelationAnalysisResultDto result, double threshold)
        {
            ArgumentNullException.ThrowIfNull(result);

            if (threshold < 0 || threshold > 1)
                throw new ArgumentOutOfRangeException(nameof(threshold), "Threshold must be between 0 and 1.");

            return result.Correlations
                .Where(c => c.AbsoluteCoefficient >= threshold)
                .OrderByDescending(c => c.AbsoluteCoefficient)
                .ToArray();
        }

        /// <summary>
        /// Gets the count of correlations that are positive (direct relationship).
        /// </summary>
        /// <param name="result">The correlation analysis result.</param>
        /// <returns>The number of correlations with positive coefficients.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is null.</exception>
        public static int GetPositiveCorrelationsCount(this CorrelationAnalysisResultDto result)
        {
            ArgumentNullException.ThrowIfNull(result);
            return result.Correlations.Count(c => c.Coefficient > 0);
        }

        /// <summary>
        /// Gets the count of correlations that are negative (inverse relationship).
        /// </summary>
        /// <param name="result">The correlation analysis result.</param>
        /// <returns>The number of correlations with negative coefficients.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is null.</exception>
        public static int GetNegativeCorrelationsCount(this CorrelationAnalysisResultDto result)
        {
            ArgumentNullException.ThrowIfNull(result);
            return result.Correlations.Count(c => c.Coefficient < 0);
        }

        /// <summary>
        /// Gets the average coefficient (not absolute) across all correlations.
        /// Returns 0 when no correlations exist.
        /// </summary>
        /// <param name="result">The correlation analysis result.</param>
        /// <returns>The average coefficient value.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is null.</exception>
        public static double GetAverageCoefficient(this CorrelationAnalysisResultDto result)
        {
            ArgumentNullException.ThrowIfNull(result);
            return result.Correlations.Count > 0
                ? result.Correlations.Average(c => c.Coefficient)
                : 0.0;
        }
    }
}