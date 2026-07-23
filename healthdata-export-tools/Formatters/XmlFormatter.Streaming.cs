#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using HealthDataExportTools.Domain.Models;
using Microsoft.Extensions.Logging;

namespace HealthDataExportTools.Formatters;

public sealed partial class XmlFormatter
{
    /// <summary>
    /// Writes a collection of health data records to a stream as XML asynchronously.
    /// This streaming approach avoids building the entire payload in memory, making it suitable for large datasets.
    /// </summary>
    /// <param name="records">The records to format; must not be <c>null</c>.</param>
    /// <param name="writer">The text writer to write to; must not be <c>null</c>.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="records"/> or <paramref name="writer"/> is <c>null</c>.</exception>
    public async Task WriteAsync(
        IEnumerable<HealthDataRecord> records,
        TextWriter writer,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(records);
        ArgumentNullException.ThrowIfNull(writer);

        var doc = new XmlDocument();
        var declaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
        doc.AppendChild(declaration);

        var root = doc.CreateElement("HealthDataExport");
        root.SetAttribute("xmlns", "urn:healthdata-export");
        doc.AppendChild(root);

        var recordsElement = doc.CreateElement("Records");
        root.AppendChild(recordsElement);

        int count = 0;
        foreach (var record in records)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var recordElement = doc.CreateElement("Record");

            AddElement(doc, recordElement, "RecordDate", record.RecordDate.ToString("yyyy-MM-ddTHH:mm:ss"));
            AddElement(doc, recordElement, "MetricType", record.GetType().Name);
            AddElement(doc, recordElement, "DeviceType", record.DeviceId);
            AddElement(doc, recordElement, "Value", string.Empty);

            recordsElement.AppendChild(recordElement);
            count++;
        }

        var sb = new StringBuilder();
        var xmlSettings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = " ",
            Encoding = Encoding.UTF8
        };

        using (var xmlWriter = XmlWriter.Create(sb, xmlSettings))
        {
            doc.WriteTo(xmlWriter);
        }

        await writer.WriteAsync(sb.ToString()).ConfigureAwait(false);
        await writer.FlushAsync().ConfigureAwait(false);

        _logger.LogInformation("Streamed {Count} records to XML", count);
    }

    /// <summary>
    /// Writes sleep data to a stream as XML asynchronously.
    /// </summary>
    /// <param name="sleepRecords">The sleep records to format; must not be <c>null</c>.</param>
    /// <param name="writer">The text writer to write to; must not be <c>null</c>.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sleepRecords"/> or <paramref name="writer"/> is <c>null</c>.</exception>
    public async Task WriteSleepDataAsync(
        IEnumerable<SleepData> sleepRecords,
        TextWriter writer,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sleepRecords);
        ArgumentNullException.ThrowIfNull(writer);

        var doc = new XmlDocument();
        var declaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
        doc.AppendChild(declaration);

        var root = doc.CreateElement("SleepDataExport");
        doc.AppendChild(root);

        int count = 0;
        foreach (var record in sleepRecords)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var recordElement = doc.CreateElement("SleepRecord");

            AddElement(doc, recordElement, "Date", record.RecordDate.ToString("yyyy-MM-dd"));
            AddElement(doc, recordElement, "DurationMinutes", record.DurationMinutes.ToString());
            AddElement(doc, recordElement, "Quality", record.Quality.ToString());
            AddElement(doc, recordElement, "DeepSleepMinutes", record.DeepSleepMinutes.ToString());
            AddElement(doc, recordElement, "RemSleepMinutes", record.RemSleepMinutes.ToString());
            AddElement(doc, recordElement, "AwakeMinutes", record.AwakeMinutes.ToString());
            AddElement(doc, recordElement, "DeviceType", record.DeviceId);

            root.AppendChild(recordElement);
            count++;
        }

        var sb = new StringBuilder();
        var xmlSettings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = " "
        };

        using (var xmlWriter = XmlWriter.Create(sb, xmlSettings))
        {
            doc.WriteTo(xmlWriter);
        }

        await writer.WriteAsync(sb.ToString()).ConfigureAwait(false);
        await writer.FlushAsync().ConfigureAwait(false);

        _logger.LogInformation("Streamed {Count} sleep records to XML", count);
    }

    /// <summary>
    /// Writes heart rate data to a stream as XML asynchronously.
    /// </summary>
    /// <param name="heartRateRecords">The heart-rate records to format; must not be <c>null</c>.</param>
    /// <param name="writer">The text writer to write to; must not be <c>null</c>.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="heartRateRecords"/> or <paramref name="writer"/> is <c>null</c>.</exception>
    public async Task WriteHeartRateDataAsync(
        IEnumerable<HeartRateData> heartRateRecords,
        TextWriter writer,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(heartRateRecords);
        ArgumentNullException.ThrowIfNull(writer);

        var doc = new XmlDocument();
        var declaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
        doc.AppendChild(declaration);

        var root = doc.CreateElement("HeartRateDataExport");
        doc.AppendChild(root);

        int count = 0;
        foreach (var record in heartRateRecords)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var recordElement = doc.CreateElement("HeartRateRecord");

            AddElement(doc, recordElement, "Timestamp", record.RecordDate.ToString("yyyy-MM-ddTHH:mm:ss"));
            AddElement(doc, recordElement, "HeartRate", record.AverageBpm.ToString());
            AddElement(doc, recordElement, "Zone", string.Empty);

            root.AppendChild(recordElement);
            count++;
        }

        var sb = new StringBuilder();
        using (var writerSb = new StringWriter(sb))
        {
            doc.Save(writerSb);
        }

        await writer.WriteAsync(sb.ToString()).ConfigureAwait(false);
        await writer.FlushAsync().ConfigureAwait(false);

        _logger.LogInformation("Streamed {Count} heart rate records to XML", count);
    }

    /// <summary>
    /// Writes SpO2 data to a stream as XML asynchronously.
    /// </summary>
    /// <param name="spo2Records">The SpO2 records to format; must not be <c>null</c>.</param>
    /// <param name="writer">The text writer to write to; must not be <c>null</c>.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="spo2Records"/> or <paramref name="writer"/> is <c>null</c>.</exception>
    public async Task WriteSpO2DataAsync(
        IEnumerable<SpO2Data> spo2Records,
        TextWriter writer,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(spo2Records);
        ArgumentNullException.ThrowIfNull(writer);

        var doc = new XmlDocument();
        var declaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
        doc.AppendChild(declaration);

        var root = doc.CreateElement("SpO2DataExport");
        doc.AppendChild(root);

        int count = 0;
        foreach (var record in spo2Records)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var recordElement = doc.CreateElement("SpO2Record");

            AddElement(doc, recordElement, "Timestamp", record.RecordDate.ToString("yyyy-MM-ddTHH:mm:ss"));
            AddElement(doc, recordElement, "SpO2", record.AveragePercentage.ToString("F1"));
            AddElement(doc, recordElement, "IsLowOxygen", record.HasConcerningLevels().ToString());

            root.AppendChild(recordElement);
            count++;
        }

        var sb = new StringBuilder();
        using (var writerSb = new StringWriter(sb))
        {
            doc.Save(writerSb);
        }

        await writer.WriteAsync(sb.ToString()).ConfigureAwait(false);
        await writer.FlushAsync().ConfigureAwait(false);

        _logger.LogInformation("Streamed {Count} SpO2 records to XML", count);
    }

    /// <summary>
    /// Writes steps data to a stream as XML asynchronously.
    /// </summary>
    /// <param name="stepsRecords">The steps records to format; must not be <c>null</c>.</param>
    /// <param name="writer">The text writer to write to; must not be <c>null</c>.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="stepsRecords"/> or <paramref name="writer"/> is <c>null</c>.</exception>
    public async Task WriteStepsDataAsync(
        IEnumerable<StepsData> stepsRecords,
        TextWriter writer,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stepsRecords);
        ArgumentNullException.ThrowIfNull(writer);

        var doc = new XmlDocument();
        var declaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
        doc.AppendChild(declaration);

        var root = doc.CreateElement("StepsDataExport");
        doc.AppendChild(root);

        int count = 0;
        foreach (var record in stepsRecords)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var recordElement = doc.CreateElement("StepsRecord");

            AddElement(doc, recordElement, "Date", record.RecordDate.ToString("yyyy-MM-dd"));
            AddElement(doc, recordElement, "StepCount", record.TotalSteps.ToString());
            AddElement(doc, recordElement, "Distance", record.DistanceKm.ToString("F2"));
            AddElement(doc, recordElement, "Calories", record.CaloriesBurned.ToString("F1"));

            root.AppendChild(recordElement);
            count++;
        }

        var sb = new StringBuilder();
        using (var writerSb = new StringWriter(sb))
        {
            doc.Save(writerSb);
        }

        await writer.WriteAsync(sb.ToString()).ConfigureAwait(false);
        await writer.FlushAsync().ConfigureAwait(false);

        _logger.LogInformation("Streamed {Count} steps records to XML", count);
    }
}