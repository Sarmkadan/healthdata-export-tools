#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text;
using System.Text.Json;
using HealthDataExportTools.Domain.Models;

namespace HealthDataExportTools.Services;

/// <summary>
/// Service for comparing two distinct sets of health data records.
/// </summary>
public sealed class DataComparisonService
{
    /// <summary>
    /// Compare two pre-built <see cref="HealthDataCollection"/> periods.
    /// Calculates percentage changes for sleep, heart rate, steps, SpO2, and activity metrics.
    /// </summary>
    /// <param name="period1">Reference (baseline) period.</param>
    /// <param name="period2">Comparison period.</param>
    /// <returns>A <see cref="DataComparisonResult"/> containing per-metric changes and a narrative summary.</returns>
    public Task<DataComparisonResult> ComparePeriodsAsync(
        HealthDataCollection period1,
        HealthDataCollection period2)
    {
        ArgumentNullException.ThrowIfNull(period1);
        ArgumentNullException.ThrowIfNull(period2);

        var result = new DataComparisonResult
        {
            Period1RecordCount = period1.GetTotalRecordCount(),
            Period2RecordCount = period2.GetTotalRecordCount(),
            GeneratedAt = DateTime.UtcNow
        };

        // Sleep
        if (period1.SleepRecords.Count > 0 && period2.SleepRecords.Count > 0)
        {
            result.Period1AverageSleepMinutes = period1.SleepRecords.Average(s => s.DurationMinutes);
            result.Period2AverageSleepMinutes = period2.SleepRecords.Average(s => s.DurationMinutes);
            result.SleepDurationChangePercentage = CalculatePercentageChange(
                result.Period1AverageSleepMinutes,
                result.Period2AverageSleepMinutes);

            result.Period1AverageDeepSleepMinutes = period1.SleepRecords.Average(s => s.DeepSleepMinutes);
            result.Period2AverageDeepSleepMinutes = period2.SleepRecords.Average(s => s.DeepSleepMinutes);
            result.DeepSleepChangePercentage = CalculatePercentageChange(
                result.Period1AverageDeepSleepMinutes,
                result.Period2AverageDeepSleepMinutes);
        }

        // Heart Rate
        if (period1.HeartRateRecords.Count > 0 && period2.HeartRateRecords.Count > 0)
        {
            result.Period1AverageHeartRate = period1.HeartRateRecords.Average(h => h.AverageBpm);
            result.Period2AverageHeartRate = period2.HeartRateRecords.Average(h => h.AverageBpm);
            result.HeartRateChangePercentage = CalculatePercentageChange(
                result.Period1AverageHeartRate,
                result.Period2AverageHeartRate);
        }

        // Steps
        if (period1.StepsRecords.Count > 0 && period2.StepsRecords.Count > 0)
        {
            result.Period1AverageSteps = period1.StepsRecords.Average(st => st.TotalSteps);
            result.Period2AverageSteps = period2.StepsRecords.Average(st => st.TotalSteps);
            result.StepsChangePercentage = CalculatePercentageChange(
                result.Period1AverageSteps,
                result.Period2AverageSteps);
        }

        // SpO2
        if (period1.SpO2Records.Count > 0 && period2.SpO2Records.Count > 0)
        {
            result.Period1AverageSpO2 = period1.SpO2Records.Average(sp => sp.AveragePercentage);
            result.Period2AverageSpO2 = period2.SpO2Records.Average(sp => sp.AveragePercentage);
            result.SpO2ChangePercentage = CalculatePercentageChange(
                result.Period1AverageSpO2,
                result.Period2AverageSpO2);
        }

        // Activity
        if (period1.ActivityRecords.Count > 0 && period2.ActivityRecords.Count > 0)
        {
            result.Period1TotalActivityMinutes = period1.ActivityRecords.Sum(a => a.DurationMinutes);
            result.Period2TotalActivityMinutes = period2.ActivityRecords.Sum(a => a.DurationMinutes);
            result.ActivityMinutesChangePercentage = CalculatePercentageChange(
                result.Period1TotalActivityMinutes,
                result.Period2TotalActivityMinutes);

            result.Period1TotalCalories = period1.ActivityRecords.Sum(a => a.CaloriesBurned);
            result.Period2TotalCalories = period2.ActivityRecords.Sum(a => a.CaloriesBurned);
            result.CaloriesChangePercentage = CalculatePercentageChange(
                result.Period1TotalCalories,
                result.Period2TotalCalories);
        }

        result.NarrativeSummary = BuildNarrative(result);
        return Task.FromResult(result);
    }

    /// <summary>
    /// Compare two date ranges extracted from a single <see cref="HealthDataCollection"/>.
    /// Records are partitioned into Period 1 and Period 2 based on the supplied date boundaries.
    /// </summary>
    /// <param name="collection">The full collection containing all records.</param>
    /// <param name="period1Start">Inclusive start of the baseline period.</param>
    /// <param name="period1End">Inclusive end of the baseline period.</param>
    /// <param name="period2Start">Inclusive start of the comparison period.</param>
    /// <param name="period2End">Inclusive end of the comparison period.</param>
    public Task<DataComparisonResult> CompareByDateRangeAsync(
        HealthDataCollection collection,
        DateTime period1Start,
        DateTime period1End,
        DateTime period2Start,
        DateTime period2End)
    {
        ArgumentNullException.ThrowIfNull(collection);

        var p1 = new HealthDataCollection
        {
            SleepRecords      = collection.SleepRecords.Where(r => r.RecordDate.Date >= period1Start.Date && r.RecordDate.Date <= period1End.Date).ToList(),
            HeartRateRecords  = collection.HeartRateRecords.Where(r => r.RecordDate.Date >= period1Start.Date && r.RecordDate.Date <= period1End.Date).ToList(),
            StepsRecords      = collection.StepsRecords.Where(r => r.RecordDate.Date >= period1Start.Date && r.RecordDate.Date <= period1End.Date).ToList(),
            SpO2Records       = collection.SpO2Records.Where(r => r.RecordDate.Date >= period1Start.Date && r.RecordDate.Date <= period1End.Date).ToList(),
            ActivityRecords   = collection.ActivityRecords.Where(r => r.RecordDate.Date >= period1Start.Date && r.RecordDate.Date <= period1End.Date).ToList()
        };

        var p2 = new HealthDataCollection
        {
            SleepRecords      = collection.SleepRecords.Where(r => r.RecordDate.Date >= period2Start.Date && r.RecordDate.Date <= period2End.Date).ToList(),
            HeartRateRecords  = collection.HeartRateRecords.Where(r => r.RecordDate.Date >= period2Start.Date && r.RecordDate.Date <= period2End.Date).ToList(),
            StepsRecords      = collection.StepsRecords.Where(r => r.RecordDate.Date >= period2Start.Date && r.RecordDate.Date <= period2End.Date).ToList(),
            SpO2Records       = collection.SpO2Records.Where(r => r.RecordDate.Date >= period2Start.Date && r.RecordDate.Date <= period2End.Date).ToList(),
            ActivityRecords   = collection.ActivityRecords.Where(r => r.RecordDate.Date >= period2Start.Date && r.RecordDate.Date <= period2End.Date).ToList()
        };

        return ComparePeriodsAsync(p1, p2);
    }

    /// <summary>
    /// Export a <see cref="DataComparisonResult"/> to a JSON file.
    /// </summary>
    /// <param name="result">The comparison result to serialize.</param>
    /// <param name="outputPath">Destination file path.</param>
    public async Task ExportToJsonAsync(DataComparisonResult result, string outputPath)
    {
        ArgumentNullException.ThrowIfNull(result);

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        var json = JsonSerializer.Serialize(result, options);
        await File.WriteAllTextAsync(outputPath, json, Encoding.UTF8).ConfigureAwait(false);
    }

    /// <summary>
    /// Renders a plain-text diff summary.
    /// </summary>
    /// <param name="result">The comparison result to render.</param>
    /// <returns>A formatted plain-text diff summary.</returns>
    public string GenerateTextReport(DataComparisonResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var sb = new StringBuilder();
        sb.AppendLine("Diff Summary:");
        sb.AppendLine($"Added: {result.AddedCount}");
        sb.AppendLine($"Removed: {result.RemovedCount}");
        sb.AppendLine($"Changed: {result.ChangedCount}");
        sb.AppendLine();
        sb.AppendLine("Top 10 Changed Entries:");
        
        if (result.TopChangedEntries.Count == 0)
        {
            sb.AppendLine("None.");
        }
        else
        {
            foreach (var entry in result.TopChangedEntries.Take(10))
            {
                sb.AppendLine($"- {entry}");
            }
        }
        return sb.ToString();
    }

    /// <summary>
    /// Compares two record sets with tolerance-aware numeric matching and time-alignment,
    /// producing a structured added/removed/changed diff instead of a boolean verdict.
    /// </summary>
    /// <typeparam name="T">The health record type being compared.</typeparam>
    /// <param name="baseline">The reference (older / source) record set.</param>
    /// <param name="candidate">The record set being compared against the baseline.</param>
    /// <param name="options">
    /// Tolerance and alignment options. When <c>null</c>, <see cref="ComparisonOptions.Default"/> is used.
    /// </param>
    /// <returns>A <see cref="RecordSetDiff{T}"/> describing added, removed, and changed records.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="baseline"/> or <paramref name="candidate"/> is <c>null</c>.</exception>
    public RecordSetDiff<T> CompareRecordSet<T>(
        IEnumerable<T> baseline,
        IEnumerable<T> candidate,
        ComparisonOptions? options = null)
        where T : HealthDataRecord
    {
        ArgumentNullException.ThrowIfNull(baseline);
        ArgumentNullException.ThrowIfNull(candidate);

        var opts = options ?? ComparisonOptions.Default;
        string KeySelector(T record) => opts.KeySelector(record);

        var baselineByKey = baseline.ToLookup(KeySelector);
        var candidateByKey = candidate.ToLookup(KeySelector);

        var diff = new RecordSetDiff<T>();

        foreach (var group in baselineByKey)
        {
            var baseRecord = group.First();
            var matchGroup = candidateByKey[group.Key];
            var candidateRecord = matchGroup.FirstOrDefault();

            if (candidateRecord is null)
            {
                diff.Removed.Add(baseRecord);
                continue;
            }

            var fieldDeltas = ComputeFieldDeltas(baseRecord, candidateRecord, opts);
            if (fieldDeltas.Count > 0)
            {
                diff.Changed.Add(new RecordChange<T>(group.Key, baseRecord, candidateRecord, fieldDeltas));
            }
        }

        var baselineKeys = new HashSet<string>(baselineByKey.Select(g => g.Key));
        foreach (var group in candidateByKey)
        {
            if (!baselineKeys.Contains(group.Key))
            {
                diff.Added.Add(group.First());
            }
        }

        return diff;
    }

    /// <summary>
    /// Compares the field-level summary of two same-key records, reporting only fields whose
    /// values differ beyond the configured tolerance.
    /// </summary>
    /// <param name="baseRecord">The baseline record.</param>
    /// <param name="candidateRecord">The candidate record matched to the baseline by key.</param>
    /// <param name="options">Tolerance and alignment options driving the comparison.</param>
    /// <returns>A list of field-level deltas; empty when the records are equivalent within tolerance.</returns>
    private static List<FieldDelta> ComputeFieldDeltas(
        HealthDataRecord baseRecord,
        HealthDataRecord candidateRecord,
        ComparisonOptions options)
    {
        var baseFields = baseRecord.GetSummary();
        var candidateFields = candidateRecord.GetSummary();
        var fieldNames = new SortedSet<string>(baseFields.Keys, StringComparer.Ordinal);
        fieldNames.UnionWith(candidateFields.Keys);

        var deltas = new List<FieldDelta>();

        foreach (var fieldName in fieldNames)
        {
            var hasBase = baseFields.TryGetValue(fieldName, out var baseValue);
            var hasCandidate = candidateFields.TryGetValue(fieldName, out var candidateValue);

            if (!hasBase || !hasCandidate)
            {
                deltas.Add(new FieldDelta(fieldName, baseValue, candidateValue));
                continue;
            }

            if (!ValuesAreEquivalent(fieldName, baseValue, candidateValue, options))
            {
                deltas.Add(new FieldDelta(fieldName, baseValue, candidateValue));
            }
        }

        return deltas;
    }

    /// <summary>
    /// Determines whether two summary field values are equivalent, applying per-metric epsilon
    /// tolerance to numeric values and time-granularity normalization to <see cref="DateTime"/> values.
    /// </summary>
    /// <param name="fieldName">The summary field name, used to look up a per-metric epsilon.</param>
    /// <param name="baseValue">The baseline field value.</param>
    /// <param name="candidateValue">The candidate field value.</param>
    /// <param name="options">Tolerance and alignment options driving the comparison.</param>
    /// <returns><c>true</c> when the values are equivalent within tolerance; otherwise <c>false</c>.</returns>
    private static bool ValuesAreEquivalent(string fieldName, object? baseValue, object? candidateValue, ComparisonOptions options)
    {
        if (baseValue is null || candidateValue is null)
        {
            return baseValue is null && candidateValue is null;
        }

        if (baseValue is DateTime baseDate && candidateValue is DateTime candidateDate)
        {
            return options.NormalizeTimestamp(baseDate) == options.NormalizeTimestamp(candidateDate);
        }

        if (TryToDouble(baseValue, out var baseNum) && TryToDouble(candidateValue, out var candidateNum))
        {
            var epsilon = options.GetEpsilon(fieldName);
            return Math.Abs(baseNum - candidateNum) <= epsilon;
        }

        return Equals(baseValue, candidateValue);
    }

    /// <summary>
    /// Attempts to convert a boxed numeric value to <see cref="double"/> for tolerance comparison.
    /// </summary>
    /// <param name="value">The boxed value to convert.</param>
    /// <param name="result">The converted value when successful; otherwise zero.</param>
    /// <returns><c>true</c> when <paramref name="value"/> is a supported numeric type.</returns>
    private static bool TryToDouble(object value, out double result)
    {
        switch (value)
        {
            case double d: result = d; return true;
            case float f: result = f; return true;
            case int i: result = i; return true;
            case long l: result = l; return true;
            case short s: result = s; return true;
            case decimal m: result = (double)m; return true;
            default: result = 0; return false;
        }
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private static double CalculatePercentageChange(double oldVal, double newVal)
    {
        if (Math.Abs(oldVal) < 0.001) return newVal > 0 ? 100.0 : 0.0;
        return Math.Round((newVal - oldVal) / oldVal * 100.0, 2);
    }

    private static string BuildNarrative(DataComparisonResult r)
    {
        var parts = new List<string>();

        if (r.Period1AverageSleepMinutes > 0)
            parts.Add(FormatChange("Sleep duration", r.SleepDurationChangePercentage, "min/night"));

        if (r.Period1AverageHeartRate > 0)
            parts.Add(FormatChange("Resting heart rate", r.HeartRateChangePercentage, "BPM"));

        if (r.Period1AverageSteps > 0)
            parts.Add(FormatChange("Daily steps", r.StepsChangePercentage, "steps"));

        if (r.Period1AverageSpO2 > 0)
            parts.Add(FormatChange("SpO2", r.SpO2ChangePercentage, "%"));

        if (r.Period1TotalActivityMinutes > 0)
            parts.Add(FormatChange("Activity minutes", r.ActivityMinutesChangePercentage, "min/period"));

        return parts.Count == 0
            ? "No comparable data found between the two periods."
            : string.Join(" | ", parts);
    }

    private static string FormatChange(string metric, double pct, string unit)
    {
        var direction = pct > 0 ? "▲" : pct < 0 ? "▼" : "→";
        var absStr    = Math.Abs(pct).ToString("F1");
        return $"{metric}: {direction} {absStr}% ({unit})";
    }

    /// <summary>
    /// Fast pre-check that compares two export manifests' record counts and time ranges
    /// without reading or parsing the underlying export files. Intended to be run before
    /// a deep, record-by-record comparison so that identical exports can be skipped cheaply.
    /// </summary>
    /// <param name="manifest1">Manifest of the first export. Must not be null.</param>
    /// <param name="manifest2">Manifest of the second export. Must not be null.</param>
    /// <returns>
    /// True when both manifests report the same total record count, the same per-data-type
    /// record counts, and the same covered time range, meaning a deep comparison is unlikely
    /// to find differences. False otherwise.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="manifest1"/> or <paramref name="manifest2"/> is null.</exception>
    public bool AreManifestsLikelyEquivalent(
        Exporters.ExportManifest manifest1,
        Exporters.ExportManifest manifest2)
    {
        ArgumentNullException.ThrowIfNull(manifest1);
        ArgumentNullException.ThrowIfNull(manifest2);

        if (manifest1.TotalRecordCount != manifest2.TotalRecordCount)
            return false;

        if (manifest1.TimeRangeStartUtc != manifest2.TimeRangeStartUtc ||
            manifest1.TimeRangeEndUtc != manifest2.TimeRangeEndUtc)
            return false;

        if (manifest1.RecordCountsByDataType.Count != manifest2.RecordCountsByDataType.Count)
            return false;

        foreach (var (dataType, count) in manifest1.RecordCountsByDataType)
        {
            if (!manifest2.RecordCountsByDataType.TryGetValue(dataType, out var otherCount) || otherCount != count)
                return false;
        }

        return true;
    }
}

/// <summary>
/// Contains the result of comparing two data periods.
/// </summary>
public sealed class DataComparisonResult
{
    /// <summary>Timestamp when this comparison was generated.</summary>
    public DateTime GeneratedAt { get; set; }

    /// <summary>Total record count in Period 1.</summary>
    public int Period1RecordCount { get; set; }
    /// <summary>Total record count in Period 2.</summary>
    public int Period2RecordCount { get; set; }

    // Diff metrics
    public int AddedCount { get; set; }
    public int RemovedCount { get; set; }
    public int ChangedCount { get; set; }
    public List<string> TopChangedEntries { get; set; } = [];

    // Sleep
    /// <summary>Average nightly sleep duration in Period 1 (minutes).</summary>
    public double Period1AverageSleepMinutes { get; set; }
    /// <summary>Average nightly sleep duration in Period 2 (minutes).</summary>
    public double Period2AverageSleepMinutes { get; set; }
    /// <summary>Percentage change in average sleep duration (Period 2 vs Period 1).</summary>
    public double SleepDurationChangePercentage { get; set; }

    /// <summary>Average deep-sleep duration per night in Period 1 (minutes).</summary>
    public double Period1AverageDeepSleepMinutes { get; set; }
    /// <summary>Average deep-sleep duration per night in Period 2 (minutes).</summary>
    public double Period2AverageDeepSleepMinutes { get; set; }
    /// <summary>Percentage change in average deep-sleep duration.</summary>
    public double DeepSleepChangePercentage { get; set; }

    // Heart rate
    /// <summary>Average heart rate in Period 1 (BPM).</summary>
    public double Period1AverageHeartRate { get; set; }
    /// <summary>Average heart rate in Period 2 (BPM).</summary>
    public double Period2AverageHeartRate { get; set; }
    /// <summary>Percentage change in average heart rate.</summary>
    public double HeartRateChangePercentage { get; set; }

    // Steps
    /// <summary>Average daily steps in Period 1.</summary>
    public double Period1AverageSteps { get; set; }
    /// <summary>Average daily steps in Period 2.</summary>
    public double Period2AverageSteps { get; set; }
    /// <summary>Percentage change in average daily steps.</summary>
    public double StepsChangePercentage { get; set; }

    // SpO2
    /// <summary>Average SpO2 percentage in Period 1.</summary>
    public double Period1AverageSpO2 { get; set; }
    /// <summary>Average SpO2 percentage in Period 2.</summary>
    public double Period2AverageSpO2 { get; set; }
    /// <summary>Percentage change in average SpO2.</summary>
    public double SpO2ChangePercentage { get; set; }

    // Activity
    /// <summary>Total activity minutes in Period 1.</summary>
    public int Period1TotalActivityMinutes { get; set; }
    /// <summary>Total activity minutes in Period 2.</summary>
    public int Period2TotalActivityMinutes { get; set; }
    /// <summary>Percentage change in total activity minutes.</summary>
    public double ActivityMinutesChangePercentage { get; set; }

    /// <summary>Total calories burned via activity in Period 1.</summary>
    public int Period1TotalCalories { get; set; }
    /// <summary>Total calories burned via activity in Period 2.</summary>
    public int Period2TotalCalories { get; set; }
    /// <summary>Percentage change in total calories burned.</summary>
    public double CaloriesChangePercentage { get; set; }

    /// <summary>
    /// Human-readable narrative summarising the key changes across all metrics.
    /// </summary>
    public string NarrativeSummary { get; set; } = string.Empty;
}

/// <summary>
/// Configures tolerance and time-alignment semantics used by <see cref="DataComparisonService.CompareRecordSet{T}"/>.
/// </summary>
public sealed class ComparisonOptions
{
    /// <summary>
    /// Default numeric tolerance applied to any summary field without a more specific entry
    /// in <see cref="MetricEpsilons"/>.
    /// </summary>
    public double DefaultEpsilon { get; init; } = 0.001;

    /// <summary>
    /// Per-metric numeric tolerance overrides, keyed by the field name as reported by
    /// <see cref="HealthDataRecord.GetSummary"/> (e.g. "AveragePercentage", "AverageBpm").
    /// </summary>
    public IReadOnlyDictionary<string, double> MetricEpsilons { get; init; } =
        new Dictionary<string, double>(StringComparer.Ordinal);

    /// <summary>
    /// Granularity that timestamps are truncated to before being compared or used for keying.
    /// Defaults to one second, so offset/precision-only differences do not register as mismatches.
    /// </summary>
    public TimeSpan TimeGranularity { get; init; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Produces the matching key for a record. Defaults to the record's <see cref="HealthDataRecord.Id"/>
    /// when set, falling back to a normalized-timestamp key derived from <see cref="HealthDataRecord.RecordDate"/>
    /// and <see cref="HealthDataRecord.DeviceId"/>.
    /// </summary>
    public Func<HealthDataRecord, string> KeySelector { get; init; } = DefaultKeySelector;

    /// <summary>
    /// The default options instance: one-second time granularity, empty per-metric overrides,
    /// and the default identity-or-timestamp key selector.
    /// </summary>
    public static ComparisonOptions Default { get; } = new();

    /// <summary>
    /// Resolves the numeric tolerance for a given summary field name, falling back to
    /// <see cref="DefaultEpsilon"/> when no per-metric override is configured.
    /// </summary>
    /// <param name="fieldName">The summary field name.</param>
    /// <returns>The epsilon to use when comparing that field's numeric values.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="fieldName"/> is null or empty.</exception>
    public double GetEpsilon(string fieldName)
    {
        ArgumentException.ThrowIfNullOrEmpty(fieldName);
        return MetricEpsilons.TryGetValue(fieldName, out var epsilon) ? epsilon : DefaultEpsilon;
    }

    /// <summary>
    /// Normalizes a timestamp to UTC and truncates it to <see cref="TimeGranularity"/>, so that
    /// values differing only by offset or sub-granularity precision compare equal.
    /// </summary>
    /// <param name="value">The timestamp to normalize.</param>
    /// <returns>The UTC timestamp truncated to the configured granularity.</returns>
    public DateTime NormalizeTimestamp(DateTime value)
    {
        var utc = value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };

        var granularity = TimeGranularity <= TimeSpan.Zero ? TimeSpan.FromTicks(1) : TimeGranularity;
        var ticks = utc.Ticks - (utc.Ticks % granularity.Ticks);
        return new DateTime(ticks, DateTimeKind.Utc);
    }

    /// <summary>
    /// Default key selector: uses <see cref="HealthDataRecord.Id"/> when non-empty, otherwise
    /// falls back to a key built from the normalized record date and device id.
    /// </summary>
    /// <param name="record">The record to key.</param>
    /// <returns>A stable matching key for the record.</returns>
    private static string DefaultKeySelector(HealthDataRecord record) =>
        !string.IsNullOrEmpty(record.Id)
            ? record.Id
            : $"{Default.NormalizeTimestamp(record.RecordDate):O}|{record.DeviceId}";
}

/// <summary>
/// A single field-level difference between a baseline and candidate record.
/// </summary>
/// <param name="FieldName">The summary field name that differs.</param>
/// <param name="BaselineValue">The value from the baseline record, or <c>null</c> if absent.</param>
/// <param name="CandidateValue">The value from the candidate record, or <c>null</c> if absent.</param>
public sealed record FieldDelta(string FieldName, object? BaselineValue, object? CandidateValue);

/// <summary>
/// Represents a record present in both sets under the same key, but with one or more
/// field-level differences beyond configured tolerance.
/// </summary>
/// <typeparam name="T">The health record type.</typeparam>
/// <param name="Key">The matching key shared by both records.</param>
/// <param name="Baseline">The baseline record.</param>
/// <param name="Candidate">The candidate record.</param>
/// <param name="FieldDeltas">The field-level differences that exceeded tolerance.</param>
public sealed record RecordChange<T>(string Key, T Baseline, T Candidate, IReadOnlyList<FieldDelta> FieldDeltas)
    where T : HealthDataRecord;

/// <summary>
/// Structured result of comparing two record sets: which records were added, removed,
/// or changed, with field-level deltas for changes.
/// </summary>
/// <typeparam name="T">The health record type.</typeparam>
public sealed class RecordSetDiff<T>
    where T : HealthDataRecord
{
    /// <summary>Records present only in the candidate set (not matched to any baseline record).</summary>
    public List<T> Added { get; } = [];

    /// <summary>Records present only in the baseline set (not matched to any candidate record).</summary>
    public List<T> Removed { get; } = [];

    /// <summary>Records present in both sets whose field values differ beyond tolerance.</summary>
    public List<RecordChange<T>> Changed { get; } = [];

    /// <summary>Indicates whether the two sets are equivalent within tolerance (no additions, removals, or changes).</summary>
    public bool AreEquivalent => Added.Count == 0 && Removed.Count == 0 && Changed.Count == 0;
}
