#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text;
using HealthDataExportTools.Domain.Models;

namespace HealthDataExportTools.Services;

/// <summary>
/// Service for exporting health data to HTML charts and graphs.
/// </summary>
public sealed class ChartExportService
{
    /// <summary>
    /// Exports health data records into an interactive HTML chart file.
    /// </summary>
    /// <param name="collection">The collected health data to export.</param>
    /// <param name="outputPath">The file path where the HTML report will be saved.</param>
    public async Task ExportToHtmlChartsAsync(HealthDataCollection collection, string outputPath)
    {
        var htmlBuilder = new StringBuilder();
        htmlBuilder.AppendLine("<!DOCTYPE html>");
        htmlBuilder.AppendLine("<html lang=\"en\">");
        htmlBuilder.AppendLine("<head>");
        htmlBuilder.AppendLine("    <meta charset=\"UTF-8\">");
        htmlBuilder.AppendLine("    <title>Health Data Charts</title>");
        htmlBuilder.AppendLine("    <script src=\"https://cdn.jsdelivr.net/npm/chart.js\"></script>");
        htmlBuilder.AppendLine("    <style>");
        htmlBuilder.AppendLine("        body { font-family: Arial, sans-serif; margin: 20px; background-color: #f4f7f6; }");
        htmlBuilder.AppendLine("        .chart-container { width: 80%; margin: auto; background: white; padding: 20px; border-radius: 8px; box-shadow: 0 4px 8px rgba(0,0,0,0.1); margin-bottom: 30px; }");
        htmlBuilder.AppendLine("        h1 { text-align: center; color: #333; }");
        htmlBuilder.AppendLine("    </style>");
        htmlBuilder.AppendLine("</head>");
        htmlBuilder.AppendLine("<body>");
        htmlBuilder.AppendLine("    <h1>Health Data Export Charts</h1>");

        if (collection.HeartRateRecords.Any())
        {
            htmlBuilder.AppendLine("    <div class=\"chart-container\">");
            htmlBuilder.AppendLine("        <canvas id=\"heartRateChart\"></canvas>");
            htmlBuilder.AppendLine("    </div>");
        }

        if (collection.StepsRecords.Any())
        {
            htmlBuilder.AppendLine("    <div class=\"chart-container\">");
            htmlBuilder.AppendLine("        <canvas id=\"stepsChart\"></canvas>");
            htmlBuilder.AppendLine("    </div>");
        }

        if (collection.SleepRecords.Any())
        {
            htmlBuilder.AppendLine("    <div class=\"chart-container\">");
            htmlBuilder.AppendLine("        <canvas id=\"sleepChart\"></canvas>");
            htmlBuilder.AppendLine("    </div>");
        }

        htmlBuilder.AppendLine("    <script>");

        if (collection.HeartRateRecords.Any())
        {
            var labels = string.Join(",", collection.HeartRateRecords.Select(r => $"'{r.RecordDate:MMM dd}'"));
            var avgData = string.Join(",", collection.HeartRateRecords.Select(r => r.AverageBpm));
            htmlBuilder.AppendLine($"        new Chart(document.getElementById('heartRateChart'), {{ type: 'line', data: {{ labels: [{labels}], datasets: [{{ label: 'Avg Heart Rate (BPM)', data: [{avgData}], borderColor: 'rgba(255, 99, 132, 1)', backgroundColor: 'rgba(255, 99, 132, 0.2)', fill: true }}] }}, options: {{ responsive: true }} }});");
        }

        if (collection.StepsRecords.Any())
        {
            var labels = string.Join(",", collection.StepsRecords.Select(r => $"'{r.RecordDate:MMM dd}'"));
            var stepsData = string.Join(",", collection.StepsRecords.Select(r => r.TotalSteps));
            htmlBuilder.AppendLine($"        new Chart(document.getElementById('stepsChart'), {{ type: 'bar', data: {{ labels: [{labels}], datasets: [{{ label: 'Daily Steps', data: [{stepsData}], backgroundColor: 'rgba(54, 162, 235, 0.6)' }}] }}, options: {{ responsive: true }} }});");
        }

        if (collection.SleepRecords.Any())
        {
            var labels = string.Join(",", collection.SleepRecords.Select(r => $"'{r.RecordDate:MMM dd}'"));
            var sleepData = string.Join(",", collection.SleepRecords.Select(r => Math.Round(r.DurationMinutes / 60.0, 1)));
            htmlBuilder.AppendLine($"        new Chart(document.getElementById('sleepChart'), {{ type: 'bar', data: {{ labels: [{labels}], datasets: [{{ label: 'Sleep Duration (Hours)', data: [{sleepData}], backgroundColor: 'rgba(75, 192, 192, 0.6)' }}] }}, options: {{ responsive: true }} }});");
        }

        htmlBuilder.AppendLine("    </script>");
        htmlBuilder.AppendLine("</body>");
        htmlBuilder.AppendLine("</html>");

        await File.WriteAllTextAsync(outputPath, htmlBuilder.ToString(), Encoding.UTF8).ConfigureAwait(false);
    }
}
