// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using HealthDataExportTools.Configuration;
using HealthDataExportTools.DTOs;

namespace HealthDataExportTools.Correlation;

/// <summary>
/// Default implementation of <see cref="ICorrelationEngine"/>.
/// Extracts per-metric time series from a <see cref="HealthDataCollection"/>,
/// computes Pearson r for every metric pair, and maps significant correlations
/// to a curated set of health insights with actionable recommendations.
/// </summary>
/// <remarks>
/// Designed for singleton registration. All public methods are stateless and
/// safe to call concurrently from multiple threads.
/// </remarks>
public sealed class CorrelationEngine(ILogger<CorrelationEngine> logger) : ICorrelationEngine
{
    // Curated pairs prioritised by physiological relevance.
    // Used when ComputeAllPairs is false and no custom pairs are supplied.
    private static readonly CorrelationPair[] _defaultPairs =
    [
        new("SleepDuration",    "RestingHeartRate"),
        new("SleepDuration",    "AverageHeartRate"),
        new("SleepDuration",    "Steps"),
        new("SleepScore",       "HeartRateVariability"),
        new("SleepScore",       "SpO2Average"),
        new("DeepSleepPercent", "RestingHeartRate"),
        new("DeepSleepPercent", "HeartRateVariability"),
        new("SpO2Average",      "AverageHeartRate"),
        new("SpO2Average",      "SleepScore"),
        new("Steps",            "RestingHeartRate"),
        new("Steps",            "CaloriesBurned"),
        new("Steps",            "HeartRateVariability"),
        new("ActiveMinutes",    "RestingHeartRate"),
        new("ActiveMinutes",    "HeartRateVariability"),
    ];

    // ── Public API ────────────────────────────────────────────────────────────

    /// <inheritdoc />
    public async Task<CorrelationAnalysisResultDto> AnalyzeAsync(
        HealthDataCollection collection,
        CorrelationEngineOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= CorrelationEngineOptions.Default;

        var timeSeries = ExtractTimeSeries(collection, options.AnalysisWindowDays);
        var seriesMap = timeSeries.ToDictionary(t => t.MetricName, StringComparer.Ordinal);

        List<CorrelationPair> pairs = options.ComputeAllPairs
            ? GenerateAllPairs(timeSeries.Select(t => t.MetricName).ToArray())
            : [.. _defaultPairs, .. options.AdditionalMetricPairs];

        var correlations = options.EnableParallelComputation
            ? await ComputeInParallelAsync(pairs, seriesMap, options, cancellationToken)
            : ComputeSequentially(pairs, seriesMap, options);

        if (!options.IncludeWeakCorrelations)
            correlations = [.. correlations.Where(c => c.IsSignificant(options.SignificanceThreshold))];

        var insights = GenerateInsights(correlations, options);
        var significantCount = correlations.Count(c => c.IsSignificant(options.SignificanceThreshold));

        logger.LogDebug(
            "Correlation analysis complete — {Total} pairs, {Significant} significant, {Insights} insights (window={Days}d)",
            correlations.Count, significantCount, insights.Count, options.AnalysisWindowDays);

        return new CorrelationAnalysisResultDto
        {
            AnalysisId                = Guid.NewGuid().ToString("N")[..12],
            GeneratedAt               = DateTimeOffset.UtcNow,
            WindowDays                = options.AnalysisWindowDays,
            Correlations              = correlations,
            Insights                  = insights,
            TotalMetricPairsAnalyzed  = correlations.Count,
            SignificantCorrelationsFound = significantCount,
        };
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CrossMetricInsightDto>> GetInsightsAsync(
        HealthDataCollection collection,
        int windowDays = 30,
        CancellationToken cancellationToken = default)
    {
        var result = await AnalyzeAsync(
            collection,
            new CorrelationEngineOptions { AnalysisWindowDays = windowDays },
            cancellationToken);
        return result.Insights;
    }

    /// <inheritdoc />
    public IReadOnlyList<MetricTimeSeries> ExtractTimeSeries(HealthDataCollection collection, int windowDays)
    {
        var cutoff = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-windowDays));
        var series = new List<MetricTimeSeries>(10);

        ExtractSleepSeries(collection.SleepRecords, cutoff, series);
        ExtractHeartRateSeries(collection.HeartRateRecords, cutoff, series);
        ExtractSpO2Series(collection.SpO2Records, cutoff, series);
        ExtractStepsSeries(collection.StepsRecords, cutoff, series);

        return series;
    }

    /// <inheritdoc />
    public double ComputePearsonCorrelation(IReadOnlyList<double> x, IReadOnlyList<double> y)
    {
        if (x.Count != y.Count || x.Count < 2)
            return double.NaN;

        var n = x.Count;
        var meanX = x.Average();
        var meanY = y.Average();
        double sumXY = 0, sumX2 = 0, sumY2 = 0;

        for (var i = 0; i < n; i++)
        {
            var dx = x[i] - meanX;
            var dy = y[i] - meanY;
            sumXY += dx * dy;
            sumX2 += dx * dx;
            sumY2 += dy * dy;
        }

        var denom = Math.Sqrt(sumX2 * sumY2);
        return denom < double.Epsilon ? 0.0 : sumXY / denom;
    }

    /// <inheritdoc />
    public double ComputeLaggedCorrelation(IReadOnlyList<double> x, IReadOnlyList<double> y, int lagDays)
    {
        if (lagDays < 0 || lagDays >= x.Count || x.Count != y.Count)
            return double.NaN;

        var xLeading = x.Take(x.Count - lagDays).ToArray();
        var yLagging = y.Skip(lagDays).ToArray();
        return ComputePearsonCorrelation(xLeading, yLagging);
    }

    /// <inheritdoc />
    public IReadOnlyList<LaggedCorrelationResult> AnalyzeLag(
        MetricTimeSeries a, MetricTimeSeries b, int maxLagDays = 7)
    {
        var commonDates = a.DataPoints.Select(p => p.Date)
            .Intersect(b.DataPoints.Select(p => p.Date))
            .Order()
            .ToArray();

        if (commonDates.Length < maxLagDays + 2)
            return [];

        var xValues = a.DataPoints
            .Where(p => commonDates.Contains(p.Date))
            .OrderBy(p => p.Date)
            .Select(p => p.Value)
            .ToArray();

        var yValues = b.DataPoints
            .Where(p => commonDates.Contains(p.Date))
            .OrderBy(p => p.Date)
            .Select(p => p.Value)
            .ToArray();

        var results = new List<LaggedCorrelationResult>(maxLagDays + 1);

        for (var lag = 0; lag <= maxLagDays; lag++)
        {
            var r = ComputeLaggedCorrelation(xValues, yValues, lag);
            if (double.IsNaN(r)) continue;

            var interpretation = lag == 0
                ? $"Same-day: {a.MetricName} ↔ {b.MetricName} r={r:F3} (n={xValues.Length})"
                : $"{a.MetricName} on day N predicts {b.MetricName} {lag}d later: r={r:F3} (n={xValues.Length - lag})";

            results.Add(new LaggedCorrelationResult(
                a.MetricName, b.MetricName, lag,
                Math.Round(r, 4), xValues.Length - lag, interpretation));
        }

        return [.. results.OrderByDescending(r => r.AbsoluteCoefficient)];
    }

    // ── Time-series extraction ────────────────────────────────────────────────

    private static void ExtractSleepSeries(
        IReadOnlyList<SleepData> records, DateOnly cutoff, List<MetricTimeSeries> out_)
    {
        var inWindow = records
            .Where(r => DateOnly.FromDateTime(r.RecordDate) >= cutoff && !r.IsNap)
            .ToList();
        if (inWindow.Count == 0) return;

        out_.Add(new MetricTimeSeries("SleepDuration",
            inWindow.GroupBy(r => DateOnly.FromDateTime(r.RecordDate))
                .Select(g => (g.Key, (double)g.Average(r => r.DurationMinutes)))
                .OrderBy(p => p.Key).ToArray()));

        var withScore = inWindow.Where(r => r.Score.HasValue).ToList();
        if (withScore.Count > 0)
            out_.Add(new MetricTimeSeries("SleepScore",
                withScore.GroupBy(r => DateOnly.FromDateTime(r.RecordDate))
                    .Select(g => (g.Key, g.Average(r => (double)r.Score!.Value)))
                    .OrderBy(p => p.Key).ToArray()));

        var withDuration = inWindow.Where(r => r.DurationMinutes > 0).ToList();
        if (withDuration.Count > 0)
            out_.Add(new MetricTimeSeries("DeepSleepPercent",
                withDuration.GroupBy(r => DateOnly.FromDateTime(r.RecordDate))
                    .Select(g => (g.Key, g.Average(r => r.GetDeepSleepPercentage())))
                    .OrderBy(p => p.Key).ToArray()));
    }

    private static void ExtractHeartRateSeries(
        IReadOnlyList<HeartRateData> records, DateOnly cutoff, List<MetricTimeSeries> out_)
    {
        var inWindow = records.Where(r => DateOnly.FromDateTime(r.RecordDate) >= cutoff).ToList();
        if (inWindow.Count == 0) return;

        out_.Add(new MetricTimeSeries("AverageHeartRate",
            inWindow.GroupBy(r => DateOnly.FromDateTime(r.RecordDate))
                .Select(g => (g.Key, (double)g.Average(r => r.AverageBpm)))
                .OrderBy(p => p.Key).ToArray()));

        var withResting = inWindow.Where(r => r.RestingBpm.HasValue).ToList();
        if (withResting.Count > 0)
            out_.Add(new MetricTimeSeries("RestingHeartRate",
                withResting.GroupBy(r => DateOnly.FromDateTime(r.RecordDate))
                    .Select(g => (g.Key, g.Average(r => (double)r.RestingBpm!.Value)))
                    .OrderBy(p => p.Key).ToArray()));

        var withHrv = inWindow.Where(r => r.HeartRateVariability.HasValue).ToList();
        if (withHrv.Count > 0)
            out_.Add(new MetricTimeSeries("HeartRateVariability",
                withHrv.GroupBy(r => DateOnly.FromDateTime(r.RecordDate))
                    .Select(g => (g.Key, g.Average(r => r.HeartRateVariability!.Value)))
                    .OrderBy(p => p.Key).ToArray()));
    }

    private static void ExtractSpO2Series(
        IReadOnlyList<SpO2Data> records, DateOnly cutoff, List<MetricTimeSeries> out_)
    {
        var inWindow = records
            .Where(r => DateOnly.FromDateTime(r.RecordDate) >= cutoff && r.AveragePercentage > 0)
            .ToList();
        if (inWindow.Count == 0) return;

        out_.Add(new MetricTimeSeries("SpO2Average",
            inWindow.GroupBy(r => DateOnly.FromDateTime(r.RecordDate))
                .Select(g => (g.Key, (double)g.Average(r => r.AveragePercentage)))
                .OrderBy(p => p.Key).ToArray()));
    }

    private static void ExtractStepsSeries(
        IReadOnlyList<StepsData> records, DateOnly cutoff, List<MetricTimeSeries> out_)
    {
        var inWindow = records.Where(r => DateOnly.FromDateTime(r.RecordDate) >= cutoff).ToList();
        if (inWindow.Count == 0) return;

        out_.Add(new MetricTimeSeries("Steps",
            inWindow.GroupBy(r => DateOnly.FromDateTime(r.RecordDate))
                .Select(g => (g.Key, (double)g.Sum(r => r.TotalSteps)))
                .OrderBy(p => p.Key).ToArray()));

        var withActive = inWindow.Where(r => r.ActiveMinutes > 0).ToList();
        if (withActive.Count > 0)
            out_.Add(new MetricTimeSeries("ActiveMinutes",
                withActive.GroupBy(r => DateOnly.FromDateTime(r.RecordDate))
                    .Select(g => (g.Key, (double)g.Sum(r => r.ActiveMinutes)))
                    .OrderBy(p => p.Key).ToArray()));

        var withCalories = inWindow.Where(r => r.CaloriesBurned > 0).ToList();
        if (withCalories.Count > 0)
            out_.Add(new MetricTimeSeries("CaloriesBurned",
                withCalories.GroupBy(r => DateOnly.FromDateTime(r.RecordDate))
                    .Select(g => (g.Key, (double)g.Sum(r => r.CaloriesBurned)))
                    .OrderBy(p => p.Key).ToArray()));
    }

    // ── Correlation computation ───────────────────────────────────────────────

    private async Task<List<MetricCorrelationDto>> ComputeInParallelAsync(
        IReadOnlyList<CorrelationPair> pairs,
        Dictionary<string, MetricTimeSeries> seriesMap,
        CorrelationEngineOptions options,
        CancellationToken cancellationToken)
    {
        var bag = new ConcurrentBag<MetricCorrelationDto>();

        await Parallel.ForEachAsync(
            pairs,
            new ParallelOptions
            {
                MaxDegreeOfParallelism = options.MaxDegreeOfParallelism,
                CancellationToken      = cancellationToken,
            },
            (pair, _) =>
            {
                var dto = ComputePairCorrelation(pair, seriesMap, options.MinimumSampleCount);
                if (dto is not null) bag.Add(dto);
                return ValueTask.CompletedTask;
            });

        return [.. bag.OrderByDescending(c => c.AbsoluteCoefficient)];
    }

    private List<MetricCorrelationDto> ComputeSequentially(
        IReadOnlyList<CorrelationPair> pairs,
        Dictionary<string, MetricTimeSeries> seriesMap,
        CorrelationEngineOptions options)
    {
        var results = new List<MetricCorrelationDto>(pairs.Count);
        foreach (var pair in pairs)
        {
            var dto = ComputePairCorrelation(pair, seriesMap, options.MinimumSampleCount);
            if (dto is not null) results.Add(dto);
        }
        results.Sort((a, b) => b.AbsoluteCoefficient.CompareTo(a.AbsoluteCoefficient));
        return results;
    }

    private MetricCorrelationDto? ComputePairCorrelation(
        CorrelationPair pair,
        Dictionary<string, MetricTimeSeries> seriesMap,
        int minimumSamples)
    {
        if (!seriesMap.TryGetValue(pair.MetricA, out var seriesA) ||
            !seriesMap.TryGetValue(pair.MetricB, out var seriesB))
            return null;

        var commonDates = seriesA.DataPoints.Select(p => p.Date)
            .Intersect(seriesB.DataPoints.Select(p => p.Date))
            .ToHashSet();

        if (commonDates.Count < minimumSamples) return null;

        var xValues = seriesA.DataPoints
            .Where(p => commonDates.Contains(p.Date))
            .OrderBy(p => p.Date).Select(p => p.Value).ToArray();

        var yValues = seriesB.DataPoints
            .Where(p => commonDates.Contains(p.Date))
            .OrderBy(p => p.Date).Select(p => p.Value).ToArray();

        var r = ComputePearsonCorrelation(xValues, yValues);
        if (double.IsNaN(r)) return null;

        var strength  = ClassifyStrength(r);
        var direction = r > 0.01 ? CorrelationDirection.Positive
                      : r < -0.01 ? CorrelationDirection.Negative
                      : CorrelationDirection.None;

        var sortedDates = commonDates.Order().ToArray();

        return new MetricCorrelationDto
        {
            Pair               = pair,
            Coefficient        = Math.Round(r, 4),
            Strength           = strength,
            Direction          = direction,
            SampleCount        = commonDates.Count,
            Interpretation     = BuildInterpretation(pair, r, strength, direction),
            AnalysisPeriodStart = sortedDates[0],
            AnalysisPeriodEnd   = sortedDates[^1],
        };
    }

    private static CorrelationStrength ClassifyStrength(double r) => Math.Abs(r) switch
    {
        < 0.10 => CorrelationStrength.Negligible,
        < 0.30 => CorrelationStrength.Weak,
        < 0.50 => CorrelationStrength.Moderate,
        < 0.70 => CorrelationStrength.Strong,
        _      => CorrelationStrength.VeryStrong,
    };

    private static string BuildInterpretation(
        CorrelationPair pair, double r, CorrelationStrength strength, CorrelationDirection direction)
    {
        var dir = direction switch
        {
            CorrelationDirection.Positive => "positively correlated",
            CorrelationDirection.Negative => "negatively correlated",
            _                             => "not meaningfully correlated",
        };
        var mag = strength switch
        {
            CorrelationStrength.VeryStrong => "very strongly",
            CorrelationStrength.Strong     => "strongly",
            CorrelationStrength.Moderate   => "moderately",
            CorrelationStrength.Weak       => "weakly",
            _                              => "negligibly",
        };
        return $"{pair.MetricA} and {pair.MetricB} are {mag} {dir} (r={r:F3}).";
    }

    // ── Insight generation ────────────────────────────────────────────────────

    private static List<CrossMetricInsightDto> GenerateInsights(
        IReadOnlyList<MetricCorrelationDto> correlations, CorrelationEngineOptions options)
    {
        var insights = new List<CrossMetricInsightDto>();

        foreach (var corr in correlations.Where(c => c.IsSignificant(options.SignificanceThreshold)))
        {
            var (title, description, recommendation, severity) = ResolveInsight(corr, options.InsightMode);
            if (title is null) continue;

            insights.Add(new CrossMetricInsightDto
            {
                Title          = title,
                Description    = description!,
                RelatedPair    = corr.Pair,
                Coefficient    = corr.Coefficient,
                Severity       = severity,
                Recommendation = recommendation!,
            });
        }

        return [.. insights.OrderByDescending(i => i.Severity).ThenByDescending(i => i.AbsoluteCoefficient)];
    }

    private static (string? Title, string? Description, string? Recommendation, InsightSeverity Severity)
        ResolveInsight(MetricCorrelationDto corr, InsightGenerationMode mode)
    {
        return (corr.Pair.MetricA, corr.Pair.MetricB, corr.Direction) switch
        {
            ("SleepDuration", "RestingHeartRate", CorrelationDirection.Negative) => (
                "Sleep Duration Reduces Resting Heart Rate",
                $"Longer sleep is associated with a lower resting heart rate (r={corr.Coefficient:F2}), " +
                "reflecting improved overnight cardiac recovery.",
                "Aim for 7–9 hours of sleep nightly to sustain cardiovascular conditioning.",
                InsightSeverity.Significant),

            ("SleepDuration", "RestingHeartRate", CorrelationDirection.Positive) => (
                "Prolonged Sleep Linked to Elevated Resting Heart Rate",
                $"Longer sleep correlates with a higher resting heart rate (r={corr.Coefficient:F2}), " +
                "which can indicate poor sleep quality or recovery from illness.",
                "Track sleep efficiency alongside duration; consider a consistent wake time to improve sleep architecture.",
                InsightSeverity.Moderate),

            ("DeepSleepPercent", "RestingHeartRate", CorrelationDirection.Negative) => (
                "Deep Sleep Drives Cardiac Recovery",
                $"Higher deep-sleep percentage is associated with a lower resting heart rate (r={corr.Coefficient:F2}), " +
                "indicating effective overnight autonomic restoration.",
                "Optimise sleep environment (cool temperature, darkness, minimal noise) to extend deep-sleep stages.",
                InsightSeverity.Significant),

            ("DeepSleepPercent", "HeartRateVariability", CorrelationDirection.Positive) => (
                "Deep Sleep Enhances Heart Rate Variability",
                $"More deep sleep correlates with higher HRV (r={corr.Coefficient:F2}), " +
                "a well-established marker of autonomic flexibility and recovery capacity.",
                "Maintain a consistent sleep schedule and limit alcohol, which preferentially suppresses deep sleep.",
                InsightSeverity.Significant),

            ("SleepScore", "HeartRateVariability", CorrelationDirection.Positive) => (
                "Sleep Quality Boosts Autonomic Recovery",
                $"Higher sleep quality scores correlate with improved HRV (r={corr.Coefficient:F2}), " +
                "reflecting superior overnight nervous-system restoration.",
                "Consistent bed and wake times are the single most impactful lever for improving sleep score and HRV simultaneously.",
                InsightSeverity.Significant),

            ("SpO2Average", "SleepScore", CorrelationDirection.Positive) or
            ("SleepScore", "SpO2Average", CorrelationDirection.Positive) => (
                "Blood Oxygen Saturation Shapes Sleep Quality",
                $"SpO₂ levels positively correlate with sleep quality score (r={corr.Coefficient:F2}), " +
                "suggesting that respiratory function directly affects restorative sleep.",
                "If SpO₂ dips below 95% during sleep, seek evaluation for sleep-disordered breathing.",
                InsightSeverity.Significant),

            ("Steps", "RestingHeartRate", CorrelationDirection.Negative) => (
                "Daily Step Count Lowers Resting Heart Rate",
                $"Higher daily step counts correlate with a lower resting heart rate (r={corr.Coefficient:F2}), " +
                "reflecting progressive cardiovascular fitness.",
                "Aim for 8,000–10,000 steps daily; even a 1,000-step increase per day has a measurable effect.",
                InsightSeverity.Significant),

            ("Steps", "HeartRateVariability", CorrelationDirection.Positive) or
            ("HeartRateVariability", "Steps", CorrelationDirection.Positive) => (
                "Physical Activity Improves Autonomic Balance",
                $"Higher step counts correlate with improved HRV (r={corr.Coefficient:F2}), " +
                "indicating better autonomic nervous system adaptability.",
                "Regular low-to-moderate daily activity improves HRV more reliably than infrequent high-intensity sessions.",
                InsightSeverity.Moderate),

            ("ActiveMinutes", "RestingHeartRate", CorrelationDirection.Negative) => (
                "Active Minutes Reduce Resting Heart Rate",
                $"More active minutes per day are associated with a lower resting heart rate (r={corr.Coefficient:F2}), " +
                "reflecting cumulative cardiovascular adaptation.",
                "Distribute activity throughout the day with short movement breaks rather than one long session.",
                InsightSeverity.Moderate),

            ("SleepDuration", "Steps", CorrelationDirection.Positive) or
            ("Steps", "SleepDuration", CorrelationDirection.Positive) => (
                "Activity and Sleep Reinforce Each Other",
                $"Daily step count and sleep duration are positively correlated (r={corr.Coefficient:F2}), " +
                "suggesting a beneficial lifestyle feedback loop.",
                "Physical activity earlier in the day increases sleep drive; use this synergy intentionally.",
                InsightSeverity.Moderate),

            ("SpO2Average", "AverageHeartRate", CorrelationDirection.Negative) => (
                "Lower SpO₂ Associated with Higher Average Heart Rate",
                $"Oxygen saturation and average heart rate are inversely correlated (r={corr.Coefficient:F2}), " +
                "consistent with the cardiovascular response to mild hypoxia.",
                "Monitor trends: persistent low SpO₂ with elevated HR may warrant clinical review.",
                InsightSeverity.Significant),

            _ when corr.Strength >= CorrelationStrength.Strong =>
                BuildGenericStrongInsight(corr),

            _ when mode == InsightGenerationMode.Comprehensive =>
                BuildGenericInsight(corr),

            _ => (null, null, null, InsightSeverity.Informational),
        };
    }

    private static (string Title, string Description, string Recommendation, InsightSeverity Severity)
        BuildGenericStrongInsight(MetricCorrelationDto corr)
    {
        var dirLabel = corr.Direction == CorrelationDirection.Positive ? "increases as" : "decreases as";
        return (
            $"Strong {corr.Direction} Link: {corr.Pair}",
            $"{corr.Pair.MetricA} {dirLabel} {corr.Pair.MetricB} changes " +
            $"(r={corr.Coefficient:F2}, n={corr.SampleCount} days). This strong relationship may have health implications.",
            $"Investigate whether deliberate changes in {corr.Pair.MetricB} can be used to influence {corr.Pair.MetricA}.",
            InsightSeverity.Moderate);
    }

    private static (string Title, string Description, string Recommendation, InsightSeverity Severity)
        BuildGenericInsight(MetricCorrelationDto corr)
    {
        return (
            $"{corr.Strength} Correlation: {corr.Pair}",
            corr.Interpretation,
            "Monitor both metrics over the coming weeks to confirm or refute this pattern.",
            InsightSeverity.Informational);
    }

    private static List<CorrelationPair> GenerateAllPairs(IReadOnlyList<string> metricNames)
    {
        var pairs = new List<CorrelationPair>(metricNames.Count * (metricNames.Count - 1) / 2);
        for (var i = 0; i < metricNames.Count; i++)
            for (var j = i + 1; j < metricNames.Count; j++)
                pairs.Add(new CorrelationPair(metricNames[i], metricNames[j]));
        return pairs;
    }
}
