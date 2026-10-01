#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;
using HealthDataExportTools.Domain.Models;

namespace HealthDataExportTools.Services;

/// <summary>
/// Validates health data records for completeness, consistency, and plausible ranges
/// </summary>
public sealed class DataValidator
{
    private readonly ILogger<DataValidator> _logger;
    private readonly IValidationService _validationService;

    /// <summary>
    /// Initializes a new instance of <see cref="DataValidator"/>
    /// </summary>
    public DataValidator(ILogger<DataValidator> logger, IValidationService validationService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
    }

    /// <summary>
    /// Validate an entire health data collection and return aggregated results
    /// </summary>
    /// <param name="collection">The collection to validate</param>
    /// <returns>A list of validation issues found, empty if all data is valid</returns>
    public List<DataValidationIssue> ValidateCollection(HealthDataCollection collection)
    {
        ArgumentNullException.ThrowIfNull(collection);
        var issues = new List<DataValidationIssue>();

        foreach (var sleep in collection.SleepRecords)
        {
            var result = _validationService.ValidateSleepData(sleep);
            if (!result.IsValid)
                issues.Add(new DataValidationIssue(sleep.Id, "SleepData", result.Errors));
        }

        foreach (var hr in collection.HeartRateRecords)
        {
            var result = _validationService.ValidateHeartRateData(hr);
            if (!result.IsValid)
                issues.Add(new DataValidationIssue(hr.Id, "HeartRateData", result.Errors));
        }

        foreach (var spo2 in collection.SpO2Records)
        {
            var result = _validationService.ValidateSpO2Data(spo2);
            if (!result.IsValid)
                issues.Add(new DataValidationIssue(spo2.Id, "SpO2Data", result.Errors));
        }

        foreach (var steps in collection.StepsRecords)
        {
            var result = _validationService.ValidateStepsData(steps);
            if (!result.IsValid)
                issues.Add(new DataValidationIssue(steps.Id, "StepsData", result.Errors));
        }

        foreach (var activity in collection.ActivityRecords)
        {
            var result = _validationService.ValidateActivityData(activity);
            if (!result.IsValid)
                issues.Add(new DataValidationIssue(activity.Id, "ActivityData", result.Errors));
        }

        _logger.LogInformation("Validated {Total} records, found {Issues} issues",
            collection.GetTotalRecordCount(), issues.Count);

        return issues;
    }

    /// <summary>
    /// Check for duplicate records within a collection by date and type
    /// </summary>
    /// <param name="collection">The collection to check</param>
    /// <returns>Record IDs that appear to be duplicates</returns>
    public List<string> FindDuplicates(HealthDataCollection collection)
    {
        ArgumentNullException.ThrowIfNull(collection);
        var duplicates = new List<string>();

        duplicates.AddRange(FindDuplicatesInList(collection.SleepRecords));
        duplicates.AddRange(FindDuplicatesInList(collection.HeartRateRecords));
        duplicates.AddRange(FindDuplicatesInList(collection.SpO2Records));
        duplicates.AddRange(FindDuplicatesInList(collection.StepsRecords));
        duplicates.AddRange(FindDuplicatesInList(collection.ActivityRecords));

        if (duplicates.Count > 0)
            _logger.LogWarning("Found {Count} duplicate records", duplicates.Count);

        return duplicates;
    }

    /// <summary>
    /// Detect temporal gaps in a record sequence (missing days)
    /// </summary>
    /// <param name="records">Records sorted by date</param>
    /// <param name="maxGapDays">Maximum acceptable gap between consecutive records</param>
    /// <returns>Date ranges where data is missing</returns>
    public List<(DateTime Start, DateTime End)> DetectGaps<T>(
        IEnumerable<T> records, int maxGapDays = 1) where T : HealthDataRecord
    {
        ArgumentNullException.ThrowIfNull(records);
        if (maxGapDays < 1)
            throw new ArgumentOutOfRangeException(nameof(maxGapDays), "Must be at least 1");

        var gaps = new List<(DateTime Start, DateTime End)>();
        var sorted = records.OrderBy(r => r.RecordDate).ToList();

        for (int i = 1; i < sorted.Count; i++)
        {
            var daysBetween = (sorted[i].RecordDate - sorted[i - 1].RecordDate).TotalDays;
            if (daysBetween > maxGapDays)
            {
                gaps.Add((sorted[i - 1].RecordDate.AddDays(1), sorted[i].RecordDate.AddDays(-1)));
            }
        }

        return gaps;
    }

    /// <summary>
    /// Validate that all required fields are present on a record
    /// </summary>
    /// <param name="record">The record to check</param>
    /// <returns>True if the record has all required base fields populated</returns>
    public bool HasRequiredFields(HealthDataRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        if (string.IsNullOrWhiteSpace(record.Id)) return false;
        if (record.RecordDate == default) return false;
        if (record.CreatedUtc == default) return false;

        return record.IsValid();
    }

    private static List<string> FindDuplicatesInList<T>(List<T> records) where T : HealthDataRecord
    {
        return records
            .GroupBy(r => r.RecordDate.Date)
            .Where(g => g.Count() > 1)
            .SelectMany(g => g.Skip(1).Select(r => r.Id))
            .ToList();
    }
}

/// <summary>
/// Represents a validation issue found in a health data record
/// </summary>
public sealed class DataValidationIssue
{
    /// <summary>Record ID that has the issue</summary>
    public string RecordId { get; }

    /// <summary>Type of the record</summary>
    public string RecordType { get; }

    /// <summary>Validation error messages</summary>
    public IReadOnlyList<string> Errors { get; }

    /// <summary>
    /// Creates a new validation issue
    /// </summary>
    public DataValidationIssue(string recordId, string recordType, IReadOnlyList<string> errors)
    {
        RecordId = recordId;
        RecordType = recordType;
        Errors = errors;
    }
}
