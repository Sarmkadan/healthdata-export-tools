#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Reflection;
using Microsoft.Extensions.Logging;
using HealthDataExportTools.Domain.Models;

namespace HealthDataExportTools.Services;

/// <summary>
/// Maps health data record properties to output schema columns/fields,
/// supporting custom field mappings and transformations for different export targets
/// </summary>
public sealed class SchemaMapper
{
    private readonly ILogger<SchemaMapper> _logger;
    private readonly Dictionary<string, Dictionary<string, FieldMapping>> _mappings = new();

    /// <summary>
    /// Initializes a new instance of <see cref="SchemaMapper"/>
    /// </summary>
    public SchemaMapper(ILogger<SchemaMapper> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        RegisterDefaults();
    }

    /// <summary>
    /// Register a custom field mapping for a record type
    /// </summary>
    /// <param name="recordType">Type name (e.g. "SleepData")</param>
    /// <param name="sourceProperty">Source property name on the record</param>
    /// <param name="targetField">Target column/field name in the output</param>
    /// <param name="transform">Optional value transform function</param>
    public void RegisterMapping(
        string recordType,
        string sourceProperty,
        string targetField,
        Func<object?, object?>? transform = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(recordType);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceProperty);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetField);

        if (!_mappings.ContainsKey(recordType))
            _mappings[recordType] = new Dictionary<string, FieldMapping>();

        _mappings[recordType][sourceProperty] = new FieldMapping(targetField, transform);
        _logger.LogDebug("Registered mapping {Type}.{Source} -> {Target}",
            recordType, sourceProperty, targetField);
    }

    /// <summary>
    /// Apply mappings to a health data record, producing a flat dictionary
    /// </summary>
    /// <typeparam name="T">Record type</typeparam>
    /// <param name="record">The source record</param>
    /// <returns>Dictionary with mapped field names and transformed values</returns>
    public Dictionary<string, object?> ApplyMapping<T>(T record) where T : HealthDataRecord
    {
        ArgumentNullException.ThrowIfNull(record);

        var typeName = typeof(T).Name;
        var result = new Dictionary<string, object?>();
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in properties)
        {
            var value = prop.GetValue(record);
            string targetName = prop.Name;
            Func<object?, object?>? transform = null;

            if (_mappings.TryGetValue(typeName, out var typeMap) &&
                typeMap.TryGetValue(prop.Name, out var mapping))
            {
                targetName = mapping.TargetField;
                transform = mapping.Transform;
            }

            result[targetName] = transform != null ? transform(value) : value;
        }

        return result;
    }

    /// <summary>
    /// Get the output column names for a record type in mapping order
    /// </summary>
    /// <typeparam name="T">Record type</typeparam>
    /// <returns>Ordered list of output field names</returns>
    public List<string> GetOutputColumns<T>() where T : HealthDataRecord
    {
        var typeName = typeof(T).Name;
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var columns = new List<string>();

        foreach (var prop in properties)
        {
            if (_mappings.TryGetValue(typeName, out var typeMap) &&
                typeMap.TryGetValue(prop.Name, out var mapping))
            {
                columns.Add(mapping.TargetField);
            }
            else
            {
                columns.Add(prop.Name);
            }
        }

        return columns;
    }

    /// <summary>
    /// Apply mappings to a collection of records
    /// </summary>
    /// <typeparam name="T">Record type</typeparam>
    /// <param name="records">Source records</param>
    /// <returns>List of mapped dictionaries</returns>
    public List<Dictionary<string, object?>> ApplyMappings<T>(IEnumerable<T> records)
        where T : HealthDataRecord
    {
        ArgumentNullException.ThrowIfNull(records);
        return records.Select(ApplyMapping).ToList();
    }

    /// <summary>
    /// Check if a mapping is registered for a given record type and property
    /// </summary>
    /// <param name="recordType">Type name</param>
    /// <param name="sourceProperty">Property name</param>
    /// <returns>True if a custom mapping exists</returns>
    public bool HasMapping(string recordType, string sourceProperty)
    {
        return _mappings.TryGetValue(recordType, out var typeMap) &&
               typeMap.ContainsKey(sourceProperty);
    }

    private void RegisterDefaults()
    {
        // Standard date formatting for all types
        RegisterMapping("SleepData", "RecordDate", "date",
            v => v is DateTime dt ? dt.ToString("yyyy-MM-dd") : v);
        RegisterMapping("HeartRateData", "RecordDate", "date",
            v => v is DateTime dt ? dt.ToString("yyyy-MM-dd") : v);
        RegisterMapping("SpO2Data", "RecordDate", "date",
            v => v is DateTime dt ? dt.ToString("yyyy-MM-dd") : v);
        RegisterMapping("StepsData", "RecordDate", "date",
            v => v is DateTime dt ? dt.ToString("yyyy-MM-dd") : v);
        RegisterMapping("ActivityData", "RecordDate", "date",
            v => v is DateTime dt ? dt.ToString("yyyy-MM-dd") : v);
    }
}

/// <summary>
/// Describes how a source property maps to a target field
/// </summary>
internal sealed class FieldMapping
{
    /// <summary>Target field/column name</summary>
    public string TargetField { get; }

    /// <summary>Optional value transformation</summary>
    public Func<object?, object?>? Transform { get; }

    public FieldMapping(string targetField, Func<object?, object?>? transform = null)
    {
        TargetField = targetField;
        Transform = transform;
    }
}
