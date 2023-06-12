// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Frozen;
using System.Text.Json;
using HealthDataExportTools.Domain.Enums;
using HealthDataExportTools.Domain.Models;
using HealthDataExportTools.Exceptions;

namespace HealthDataExportTools.Services;

/// <summary>
/// Service for parsing health data from various formats and devices
/// </summary>
public class HealthDataParserService
{
    private readonly ValidationService _validationService = new();

    // FrozenDictionary is read-only after construction and uses optimized lookup structures
    private static readonly FrozenDictionary<string, DeviceType> _deviceKeywords =
        new Dictionary<string, DeviceType>(StringComparer.OrdinalIgnoreCase)
        {
            ["zepp"]    = DeviceType.Zepp,
            ["amazfit"] = DeviceType.Amazfit,
            ["garmin"]  = DeviceType.Garmin,
        }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Parse health data from a JSON string
    /// </summary>
    public Task<HealthDataCollection> ParseJsonAsync(string jsonContent)
    {
        try
        {
            using var doc = JsonDocument.Parse(jsonContent);
            var root = doc.RootElement;

            var collection = new HealthDataCollection();

            if (root.TryGetProperty("sleep", out var sleepArray))
            {
                foreach (var sleepElement in sleepArray.EnumerateArray())
                {
                    var sleep = ParseSleepDataFromJson(sleepElement);
                    if (_validationService.ValidateSleepData(sleep).IsValid)
                        collection.SleepRecords.Add(sleep);
                }
            }

            if (root.TryGetProperty("heartRate", out var hrArray))
            {
                foreach (var hrElement in hrArray.EnumerateArray())
                {
                    var hr = ParseHeartRateDataFromJson(hrElement);
                    if (_validationService.ValidateHeartRateData(hr).IsValid)
                        collection.HeartRateRecords.Add(hr);
                }
            }

            if (root.TryGetProperty("spO2", out var spo2Array))
            {
                foreach (var spo2Element in spo2Array.EnumerateArray())
                {
                    var spo2 = ParseSpO2DataFromJson(spo2Element);
                    if (_validationService.ValidateSpO2Data(spo2).IsValid)
                        collection.SpO2Records.Add(spo2);
                }
            }

            if (root.TryGetProperty("steps", out var stepsArray))
            {
                foreach (var stepsElement in stepsArray.EnumerateArray())
                {
                    var steps = ParseStepsDataFromJson(stepsElement);
                    if (_validationService.ValidateStepsData(steps).IsValid)
                        collection.StepsRecords.Add(steps);
                }
            }

            return Task.FromResult(collection);
        }
        catch (JsonException ex)
        {
            throw new ParsingException("Failed to parse JSON content", ex.Message, 0, ex);
        }
    }

    /// <summary>
    /// Parse sleep data from JSON element
    /// </summary>
    private SleepData ParseSleepDataFromJson(JsonElement element)
    {
        var sleep = new SleepData
        {
            RecordDate  = element.GetProperty("recordDate").GetDateTime(),
            DeviceId    = element.GetProperty("deviceId").GetString() ?? "",
            SleepStart  = element.GetProperty("sleepStart").GetDateTime(),
            SleepEnd    = element.GetProperty("sleepEnd").GetDateTime(),
            DurationMinutes  = element.GetProperty("durationMinutes").GetInt32(),
            DeepSleepMinutes = element.GetProperty("deepSleepMinutes").GetInt32(),
            LightSleepMinutes = element.GetProperty("lightSleepMinutes").GetInt32(),
            RemSleepMinutes  = element.GetProperty("remSleepMinutes").GetInt32(),
            AwakeMinutes     = element.GetProperty("awakeMinutes").GetInt32()
        };

        if (element.TryGetProperty("score", out var score))
            sleep.Score = score.GetInt32();

        if (element.TryGetProperty("quality", out var quality) && int.TryParse(quality.GetString(), out var q))
            sleep.Quality = (SleepQuality)q;

        return sleep;
    }

    /// <summary>
    /// Parse heart rate data from JSON element
    /// </summary>
    private HeartRateData ParseHeartRateDataFromJson(JsonElement element)
    {
        var hr = new HeartRateData
        {
            RecordDate       = element.GetProperty("recordDate").GetDateTime(),
            DeviceId         = element.GetProperty("deviceId").GetString() ?? "",
            MinimumBpm       = element.GetProperty("minimumBpm").GetInt32(),
            MaximumBpm       = element.GetProperty("maximumBpm").GetInt32(),
            AverageBpm       = element.GetProperty("averageBpm").GetInt32(),
            MeasurementCount = element.GetProperty("measurementCount").GetInt32()
        };

        if (element.TryGetProperty("restingBpm", out var resting))
            hr.RestingBpm = resting.GetInt32();

        if (element.TryGetProperty("stressLevel", out var stress))
            hr.StressLevel = stress.GetInt32();

        return hr;
    }

    /// <summary>
    /// Parse SpO2 data from JSON element
    /// </summary>
    private SpO2Data ParseSpO2DataFromJson(JsonElement element)
    {
        var spo2 = new SpO2Data
        {
            RecordDate         = element.GetProperty("recordDate").GetDateTime(),
            DeviceId           = element.GetProperty("deviceId").GetString() ?? "",
            MinimumPercentage  = element.GetProperty("minimumPercentage").GetInt32(),
            MaximumPercentage  = element.GetProperty("maximumPercentage").GetInt32(),
            AveragePercentage  = element.GetProperty("averagePercentage").GetInt32(),
            MeasurementCount   = element.GetProperty("measurementCount").GetInt32()
        };

        if (element.TryGetProperty("restingPercentage", out var resting))
            spo2.RestingPercentage = resting.GetInt32();

        if (element.TryGetProperty("lowSpO2Events", out var events))
            spo2.LowSpO2Events = events.GetInt32();

        return spo2;
    }

    /// <summary>
    /// Parse steps data from JSON element
    /// </summary>
    private StepsData ParseStepsDataFromJson(JsonElement element)
    {
        var steps = new StepsData
        {
            RecordDate     = element.GetProperty("recordDate").GetDateTime(),
            DeviceId       = element.GetProperty("deviceId").GetString() ?? "",
            TotalSteps     = element.GetProperty("totalSteps").GetInt32(),
            DistanceKm     = element.GetProperty("distanceKm").GetDouble(),
            CaloriesBurned = element.GetProperty("caloriesBurned").GetInt32(),
            DailyGoal      = element.GetProperty("dailyGoal").GetInt32(),
            ActiveMinutes  = element.GetProperty("activeMinutes").GetInt32()
        };

        steps.UpdateGoalAchievement();
        return steps;
    }

    /// <summary>
    /// Detect device type from data
    /// </summary>
    public DeviceType DetectDeviceType(string deviceIdentifier)
    {
        foreach (var kvp in _deviceKeywords)
        {
            if (deviceIdentifier.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
                return kvp.Value;
        }
        return DeviceType.Unknown;
    }

    /// <summary>
    /// Parse CSV health data (basic implementation)
    /// </summary>
    public async Task<HealthDataCollection> ParseCsvAsync(string filePath)
    {
        if (!File.Exists(filePath))
            throw new ParsingException("CSV file not found", filePath);

        var collection = new HealthDataCollection();

        try
        {
            var lines = await File.ReadAllLinesAsync(filePath);
            if (lines.Length < 2)
                throw new ParsingException("CSV file has no data rows", filePath);

            var header = lines[0].Split(',');

            for (int i = 1; i < lines.Length; i++)
            {
                var values = lines[i].Split(',');
                // CSV parsing logic would go here based on header
            }

            return collection;
        }
        catch (IOException ex)
        {
            throw new ParsingException("Failed to read CSV file", filePath, -1, ex);
        }
    }

    /// <summary>
    /// Merge multiple health data collections
    /// </summary>
    public HealthDataCollection MergeCollections(params HealthDataCollection[] collections)
    {
        var merged = new HealthDataCollection();

        foreach (var collection in collections)
        {
            merged.SleepRecords.AddRange(collection.SleepRecords);
            merged.HeartRateRecords.AddRange(collection.HeartRateRecords);
            merged.SpO2Records.AddRange(collection.SpO2Records);
            merged.StepsRecords.AddRange(collection.StepsRecords);
            merged.ActivityRecords.AddRange(collection.ActivityRecords);
        }

        return merged;
    }
}

/// <summary>
/// Container for parsed health data records
/// </summary>
public class HealthDataCollection
{
    /// <summary>
    /// Sleep records
    /// </summary>
    public List<SleepData> SleepRecords { get; set; } = [];

    /// <summary>
    /// Heart rate records
    /// </summary>
    public List<HeartRateData> HeartRateRecords { get; set; } = [];

    /// <summary>
    /// SpO2 records
    /// </summary>
    public List<SpO2Data> SpO2Records { get; set; } = [];

    /// <summary>
    /// Steps records
    /// </summary>
    public List<StepsData> StepsRecords { get; set; } = [];

    /// <summary>
    /// Activity records
    /// </summary>
    public List<ActivityData> ActivityRecords { get; set; } = [];

    /// <summary>
    /// Health metrics
    /// </summary>
    public List<HealthMetric> Metrics { get; set; } = [];

    /// <summary>
    /// Get total record count
    /// </summary>
    public int GetTotalRecordCount()
    {
        return SleepRecords.Count + HeartRateRecords.Count + SpO2Records.Count +
               StepsRecords.Count + ActivityRecords.Count;
    }
}
