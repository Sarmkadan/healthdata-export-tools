#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using Microsoft.Extensions.Logging;
using HealthDataExportTools.Domain.Models;

namespace HealthDataExportTools.Services;

/// <summary>
/// Converts health data records to FHIR R4-compatible JSON resources
/// (Fast Healthcare Interoperability Resources)
/// </summary>
public sealed class FhirConverter
{
    private readonly ILogger<FhirConverter> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Initializes a new instance of <see cref="FhirConverter"/>
    /// </summary>
    public FhirConverter(ILogger<FhirConverter> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Convert a heart rate record to a FHIR Observation resource
    /// </summary>
    /// <param name="data">Heart rate data</param>
    /// <param name="patientReference">FHIR patient reference (e.g. "Patient/123")</param>
    /// <returns>FHIR Observation as a dictionary</returns>
    public Dictionary<string, object> ConvertHeartRate(HeartRateData data, string patientReference)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentException.ThrowIfNullOrWhiteSpace(patientReference);

        return BuildObservation(
            recordId: data.Id,
            patientReference: patientReference,
            effectiveDateTime: data.RecordDate,
            loincCode: "8867-4",
            loincDisplay: "Heart rate",
            value: data.AverageBpm,
            unit: "/min",
            ucumCode: "/min");
    }

    /// <summary>
    /// Convert an SpO2 record to a FHIR Observation resource
    /// </summary>
    /// <param name="data">SpO2 data</param>
    /// <param name="patientReference">FHIR patient reference</param>
    /// <returns>FHIR Observation as a dictionary</returns>
    public Dictionary<string, object> ConvertSpO2(SpO2Data data, string patientReference)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentException.ThrowIfNullOrWhiteSpace(patientReference);

        return BuildObservation(
            recordId: data.Id,
            patientReference: patientReference,
            effectiveDateTime: data.RecordDate,
            loincCode: "2708-6",
            loincDisplay: "Oxygen saturation in Arterial blood",
            value: data.AveragePercentage,
            unit: "%",
            ucumCode: "%");
    }

    /// <summary>
    /// Convert a steps record to a FHIR Observation resource
    /// </summary>
    /// <param name="data">Steps data</param>
    /// <param name="patientReference">FHIR patient reference</param>
    /// <returns>FHIR Observation as a dictionary</returns>
    public Dictionary<string, object> ConvertSteps(StepsData data, string patientReference)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentException.ThrowIfNullOrWhiteSpace(patientReference);

        return BuildObservation(
            recordId: data.Id,
            patientReference: patientReference,
            effectiveDateTime: data.RecordDate,
            loincCode: "55423-8",
            loincDisplay: "Number of steps in unspecified time Pedometer",
            value: data.TotalSteps,
            unit: "steps",
            ucumCode: "{steps}");
    }

    /// <summary>
    /// Convert an entire collection to a FHIR Bundle resource
    /// </summary>
    /// <param name="collection">Health data collection</param>
    /// <param name="patientReference">FHIR patient reference</param>
    /// <returns>FHIR Bundle as a dictionary</returns>
    public Dictionary<string, object> ConvertToBundle(
        HealthDataCollection collection, string patientReference)
    {
        ArgumentNullException.ThrowIfNull(collection);
        ArgumentException.ThrowIfNullOrWhiteSpace(patientReference);

        var entries = new List<Dictionary<string, object>>();

        foreach (var hr in collection.HeartRateRecords)
            entries.Add(WrapEntry(ConvertHeartRate(hr, patientReference)));

        foreach (var spo2 in collection.SpO2Records)
            entries.Add(WrapEntry(ConvertSpO2(spo2, patientReference)));

        foreach (var steps in collection.StepsRecords)
            entries.Add(WrapEntry(ConvertSteps(steps, patientReference)));

        _logger.LogInformation("Converted {Count} records to FHIR Bundle", entries.Count);

        return new Dictionary<string, object>
        {
            ["resourceType"] = "Bundle",
            ["type"] = "collection",
            ["timestamp"] = DateTime.UtcNow.ToString("o"),
            ["total"] = entries.Count,
            ["entry"] = entries
        };
    }

    /// <summary>
    /// Serialize a FHIR resource to JSON string
    /// </summary>
    /// <param name="resource">FHIR resource dictionary</param>
    /// <returns>JSON string</returns>
    public string ToJson(Dictionary<string, object> resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        return JsonSerializer.Serialize(resource, JsonOptions);
    }

    private static Dictionary<string, object> BuildObservation(
        string recordId,
        string patientReference,
        DateTime effectiveDateTime,
        string loincCode,
        string loincDisplay,
        double value,
        string unit,
        string ucumCode)
    {
        return new Dictionary<string, object>
        {
            ["resourceType"] = "Observation",
            ["id"] = recordId,
            ["status"] = "final",
            ["category"] = new[]
            {
                new Dictionary<string, object>
                {
                    ["coding"] = new[]
                    {
                        new Dictionary<string, string>
                        {
                            ["system"] = "http://terminology.hl7.org/CodeSystem/observation-category",
                            ["code"] = "vital-signs",
                            ["display"] = "Vital Signs"
                        }
                    }
                }
            },
            ["code"] = new Dictionary<string, object>
            {
                ["coding"] = new[]
                {
                    new Dictionary<string, string>
                    {
                        ["system"] = "http://loinc.org",
                        ["code"] = loincCode,
                        ["display"] = loincDisplay
                    }
                }
            },
            ["subject"] = new Dictionary<string, string>
            {
                ["reference"] = patientReference
            },
            ["effectiveDateTime"] = effectiveDateTime.ToString("o"),
            ["valueQuantity"] = new Dictionary<string, object>
            {
                ["value"] = value,
                ["unit"] = unit,
                ["system"] = "http://unitsofmeasure.org",
                ["code"] = ucumCode
            }
        };
    }

    private static Dictionary<string, object> WrapEntry(Dictionary<string, object> resource)
    {
        return new Dictionary<string, object>
        {
            ["resource"] = resource
        };
    }
}
