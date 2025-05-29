// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace HealthDataExportTools.Formatters;

/// <summary>
/// Formats health data into XML (Extensible Markup Language) format
/// Generates well-formed, indented XML with proper document structure
/// </summary>
public class XmlFormatter : IDataFormatter
{
    private readonly ILogger<XmlFormatter> _logger;

    public string FileExtension => ".xml";
    public string FormatName => "XML";

    public XmlFormatter(ILogger<XmlFormatter> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Check if data can be formatted as XML
    /// </summary>
    public bool CanFormat(Type dataType)
    {
        return typeof(HealthDataRecord).IsAssignableFrom(dataType) ||
               typeof(SleepData).IsAssignableFrom(dataType) ||
               typeof(HeartRateData).IsAssignableFrom(dataType) ||
               typeof(SpO2Data).IsAssignableFrom(dataType) ||
               typeof(StepsData).IsAssignableFrom(dataType) ||
               typeof(ActivityData).IsAssignableFrom(dataType);
    }

    /// <summary>
    /// Format single record as XML element
    /// </summary>
    public async Task<string> FormatAsync(HealthDataRecord record)
    {
        var doc = new XmlDocument();
        var root = doc.CreateElement("HealthDataRecord");

        AddElement(doc, root, "RecordDate", record.RecordDate.ToString("yyyy-MM-ddTHH:mm:ss"));
        AddElement(doc, root, "MetricType", record.GetType().Name);
        AddElement(doc, root, "DeviceType", record.DeviceId);
        AddElement(doc, root, "Value", string.Empty);

        doc.AppendChild(root);

        var sb = new StringBuilder();
        using (var writer = new StringWriter(sb))
        {
            doc.Save(writer);
        }

        _logger.LogInformation("Formatted single record to XML");
        return await Task.FromResult(sb.ToString());
    }

    /// <summary>
    /// Format collection of records as XML document
    /// </summary>
    public async Task<string> FormatCollectionAsync(List<HealthDataRecord> records)
    {
        try
        {
            if (records == null || records.Count == 0)
            {
                _logger.LogWarning("Empty record collection provided to XML formatter");
                return await Task.FromResult(GenerateEmptyXmlDocument());
            }

            var doc = new XmlDocument();
            var declaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(declaration);

            var root = doc.CreateElement("HealthDataExport");
            root.SetAttribute("ExportDate", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss"));
            root.SetAttribute("TotalRecords", records.Count.ToString());
            root.SetAttribute("xmlns", "urn:healthdata-export");

            var recordsElement = doc.CreateElement("Records");

            foreach (var record in records)
            {
                var recordElement = doc.CreateElement("Record");

                AddElement(doc, recordElement, "RecordDate", record.RecordDate.ToString("yyyy-MM-ddTHH:mm:ss"));
                AddElement(doc, recordElement, "MetricType", record.GetType().Name);
                AddElement(doc, recordElement, "DeviceType", record.DeviceId);
                AddElement(doc, recordElement, "Value", string.Empty);

                recordsElement.AppendChild(recordElement);
            }

            root.AppendChild(recordsElement);
            doc.AppendChild(root);

            var sb = new StringBuilder();
            var xmlSettings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                Encoding = Encoding.UTF8
            };

            using (var writer = XmlWriter.Create(sb, xmlSettings))
            {
                doc.WriteTo(writer);
            }

            _logger.LogInformation("Formatted {Count} records to XML", records.Count);
            return await Task.FromResult(sb.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error formatting records collection to XML");
            throw;
        }
    }

    /// <summary>
    /// Format sleep data as XML with sleep-specific structure
    /// </summary>
    public async Task<string> FormatSleepDataAsync(List<SleepData> sleepRecords)
    {
        var doc = new XmlDocument();
        var declaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
        doc.AppendChild(declaration);

        var root = doc.CreateElement("SleepDataExport");
        root.SetAttribute("ExportDate", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss"));
        root.SetAttribute("TotalRecords", (sleepRecords?.Count ?? 0).ToString());

        if (sleepRecords != null && sleepRecords.Count > 0)
        {
            // Add statistics
            var statsElement = doc.CreateElement("Statistics");
            AddElement(doc, statsElement, "AverageDurationMinutes",
                sleepRecords.Average(s => s.DurationMinutes).ToString("F1"));
            AddElement(doc, statsElement, "AverageDeepSleepMinutes",
                sleepRecords.Average(s => s.DeepSleepMinutes).ToString("F1"));
            root.AppendChild(statsElement);

            // Add records
            var recordsElement = doc.CreateElement("Records");
            foreach (var record in sleepRecords)
            {
                var recordElement = doc.CreateElement("SleepRecord");

                AddElement(doc, recordElement, "Date", record.RecordDate.ToString("yyyy-MM-dd"));
                AddElement(doc, recordElement, "DurationMinutes", record.DurationMinutes.ToString());
                AddElement(doc, recordElement, "Quality", record.Quality.ToString());
                AddElement(doc, recordElement, "DeepSleepMinutes", record.DeepSleepMinutes.ToString());
                AddElement(doc, recordElement, "RemSleepMinutes", record.RemSleepMinutes.ToString());
                AddElement(doc, recordElement, "AwakeMinutes", record.AwakeMinutes.ToString());
                AddElement(doc, recordElement, "DeviceType", record.DeviceId);

                recordsElement.AppendChild(recordElement);
            }
            root.AppendChild(recordsElement);
        }

        doc.AppendChild(root);

        var sb = new StringBuilder();
        var xmlSettings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "  "
        };

        using (var writer = XmlWriter.Create(sb, xmlSettings))
        {
            doc.WriteTo(writer);
        }

        _logger.LogInformation("Formatted {Count} sleep records to XML", sleepRecords?.Count ?? 0);
        return await Task.FromResult(sb.ToString());
    }

    /// <summary>
    /// Format heart rate data as XML
    /// </summary>
    public async Task<string> FormatHeartRateDataAsync(List<HeartRateData> heartRateRecords)
    {
        var doc = new XmlDocument();
        var root = doc.CreateElement("HeartRateDataExport");

        if (heartRateRecords != null && heartRateRecords.Count > 0)
        {
            var recordsElement = doc.CreateElement("Records");
            foreach (var record in heartRateRecords)
            {
                var recordElement = doc.CreateElement("HeartRateRecord");
                AddElement(doc, recordElement, "Timestamp", record.RecordDate.ToString("yyyy-MM-ddTHH:mm:ss"));
                AddElement(doc, recordElement, "HeartRate", record.AverageBpm.ToString());
                AddElement(doc, recordElement, "Zone", string.Empty);
                recordsElement.AppendChild(recordElement);
            }
            root.AppendChild(recordsElement);
        }

        doc.AppendChild(root);

        var sb = new StringBuilder();
        using (var writer = new StringWriter(sb))
        {
            doc.Save(writer);
        }

        return await Task.FromResult(sb.ToString());
    }

    /// <summary>
    /// Format SpO2 data as XML
    /// </summary>
    public async Task<string> FormatSpO2DataAsync(List<SpO2Data> spo2Records)
    {
        var doc = new XmlDocument();
        var root = doc.CreateElement("SpO2DataExport");

        if (spo2Records != null && spo2Records.Count > 0)
        {
            var recordsElement = doc.CreateElement("Records");
            foreach (var record in spo2Records)
            {
                var recordElement = doc.CreateElement("SpO2Record");
                AddElement(doc, recordElement, "Timestamp", record.RecordDate.ToString("yyyy-MM-ddTHH:mm:ss"));
                AddElement(doc, recordElement, "SpO2", record.AveragePercentage.ToString("F1"));
                AddElement(doc, recordElement, "IsLowOxygen", record.HasConcerningLevels().ToString());
                recordsElement.AppendChild(recordElement);
            }
            root.AppendChild(recordsElement);
        }

        doc.AppendChild(root);

        var sb = new StringBuilder();
        using (var writer = new StringWriter(sb))
        {
            doc.Save(writer);
        }

        return await Task.FromResult(sb.ToString());
    }

    /// <summary>
    /// Format steps data as XML
    /// </summary>
    public async Task<string> FormatStepsDataAsync(List<StepsData> stepsRecords)
    {
        var doc = new XmlDocument();
        var root = doc.CreateElement("StepsDataExport");

        if (stepsRecords != null && stepsRecords.Count > 0)
        {
            var recordsElement = doc.CreateElement("Records");
            foreach (var record in stepsRecords)
            {
                var recordElement = doc.CreateElement("StepsRecord");
                AddElement(doc, recordElement, "Date", record.RecordDate.ToString("yyyy-MM-dd"));
                AddElement(doc, recordElement, "StepCount", record.TotalSteps.ToString());
                AddElement(doc, recordElement, "Distance", record.DistanceKm.ToString("F2"));
                AddElement(doc, recordElement, "Calories", record.CaloriesBurned.ToString("F1"));
                recordsElement.AppendChild(recordElement);
            }
            root.AppendChild(recordsElement);
        }

        doc.AppendChild(root);

        var sb = new StringBuilder();
        using (var writer = new StringWriter(sb))
        {
            doc.Save(writer);
        }

        return await Task.FromResult(sb.ToString());
    }

    /// <summary>
    /// Validate records before XML serialization
    /// </summary>
    public async Task<List<string>> ValidateAsync(List<HealthDataRecord> records)
    {
        var errors = new List<string>();

        if (records == null)
        {
            errors.Add("Record collection is null");
            return await Task.FromResult(errors);
        }

        for (int i = 0; i < records.Count; i++)
        {
            var record = records[i];

            if (ContainsInvalidXmlChars(record.GetType().Name))
                errors.Add($"Record {i}: Invalid XML characters in type name");
        }

        return await Task.FromResult(errors);
    }

    private void AddElement(XmlDocument doc, XmlElement parent, string elementName, string elementValue)
    {
        var element = doc.CreateElement(elementName);
        element.InnerText = elementValue ?? string.Empty;
        parent.AppendChild(element);
    }

    private bool ContainsInvalidXmlChars(string text)
    {
        return text?.Any(c => c < 32 && c != 9 && c != 10 && c != 13) ?? false;
    }

    private string GenerateEmptyXmlDocument()
    {
        var doc = new XmlDocument();
        var declaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
        doc.AppendChild(declaration);

        var root = doc.CreateElement("HealthDataExport");
        root.SetAttribute("TotalRecords", "0");
        root.SetAttribute("ExportDate", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss"));

        doc.AppendChild(root);

        var sb = new StringBuilder();
        using (var writer = new StringWriter(sb))
        {
            doc.Save(writer);
        }

        return sb.ToString();
    }
}
