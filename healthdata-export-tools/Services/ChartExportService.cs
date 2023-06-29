#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text;
using HealthDataExportTools.Domain.Models;

namespace HealthDataExportTools.Services;

/// <summary>
/// Options that control how the HTML chart report is rendered.
/// </summary>
public sealed class ChartExportOptions
{
    /// <summary>Title displayed at the top of the HTML report.</summary>
    public string Title { get; set; } = "Health Data Charts";

    /// <summary>Whether to include a statistics summary table above the charts.</summary>
    public bool IncludeSummaryTable { get; set; } = true;

    /// <summary>Whether to include the SpO2 chart when SpO2 records are available.</summary>
    public bool IncludeSpO2Chart { get; set; } = true;

    /// <summary>Whether to include the activity intensity chart when activity records are available.</summary>
    public bool IncludeActivityChart { get; set; } = true;

    /// <summary>Whether to include the sleep composition stacked-bar chart.</summary>
    public bool IncludeSleepCompositionChart { get; set; } = true;
}

/// <summary>
/// Service for exporting health data to interactive HTML charts and graphs.
/// </summary>
public sealed class ChartExportService
{
    /// <summary>
    /// Exports health data records into an interactive HTML chart file using default options.
    /// </summary>
    /// <param name="collection">The collected health data to export.</param>
    /// <param name="outputPath">The file path where the HTML report will be saved.</param>
    public Task ExportToHtmlChartsAsync(HealthDataCollection collection, string outputPath)
        => ExportToHtmlChartsAsync(collection, outputPath, new ChartExportOptions());

    /// <summary>
    /// Exports health data records into an interactive HTML chart file with configurable options.
    /// </summary>
    /// <param name="collection">The collected health data to export.</param>
    /// <param name="outputPath">The file path where the HTML report will be saved.</param>
    /// <param name="options">Rendering options that control which charts and sections are included.</param>
    public async Task ExportToHtmlChartsAsync(
        HealthDataCollection collection,
        string outputPath,
        ChartExportOptions options)
    {
        ArgumentNullException.ThrowIfNull(collection);
        ArgumentNullException.ThrowIfNull(options);

        var html = BuildHtml(collection, options);
        await File.WriteAllTextAsync(outputPath, html, Encoding.UTF8).ConfigureAwait(false);
    }

    // ── HTML assembly ──────────────────────────────────────────────────────────

    private static string BuildHtml(HealthDataCollection collection, ChartExportOptions options)
    {
        var sb = new StringBuilder();

        AppendHead(sb, options.Title);
        sb.AppendLine("<body>");
        sb.AppendLine($"    <h1>{System.Web.HttpUtility.HtmlEncode(options.Title)}</h1>");
        sb.AppendLine($"    <p class=\"generated\">Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC</p>");

        if (options.IncludeSummaryTable)
            AppendSummaryTable(sb, collection);

        // Canvas placeholders
        if (collection.HeartRateRecords.Any())
            AppendCanvas(sb, "heartRateChart");

        if (collection.StepsRecords.Any())
            AppendCanvas(sb, "stepsChart");

        if (collection.SleepRecords.Any())
        {
            AppendCanvas(sb, "sleepChart");
            if (options.IncludeSleepCompositionChart)
                AppendCanvas(sb, "sleepCompositionChart");
        }

        if (options.IncludeSpO2Chart && collection.SpO2Records.Any())
            AppendCanvas(sb, "spo2Chart");

        if (options.IncludeActivityChart && collection.ActivityRecords.Any())
            AppendCanvas(sb, "activityChart");

        // Chart.js scripts
        sb.AppendLine("    <script>");

        if (collection.HeartRateRecords.Any())
            AppendHeartRateChart(sb, collection.HeartRateRecords);

        if (collection.StepsRecords.Any())
            AppendStepsChart(sb, collection.StepsRecords);

        if (collection.SleepRecords.Any())
        {
            AppendSleepDurationChart(sb, collection.SleepRecords);
            if (options.IncludeSleepCompositionChart)
                AppendSleepCompositionChart(sb, collection.SleepRecords);
        }

        if (options.IncludeSpO2Chart && collection.SpO2Records.Any())
            AppendSpO2Chart(sb, collection.SpO2Records);

        if (options.IncludeActivityChart && collection.ActivityRecords.Any())
            AppendActivityChart(sb, collection.ActivityRecords);

        sb.AppendLine("    </script>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    // ── Head / Style ───────────────────────────────────────────────────────────

    private static void AppendHead(StringBuilder sb, string title)
    {
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"en\">");
        sb.AppendLine("<head>");
        sb.AppendLine("    <meta charset=\"UTF-8\">");
        sb.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
        sb.AppendLine($"    <title>{System.Web.HttpUtility.HtmlEncode(title)}</title>");
        sb.AppendLine("    <script src=\"https://cdn.jsdelivr.net/npm/chart.js\"></script>");
        sb.AppendLine("    <style>");
        sb.AppendLine("        body { font-family: 'Segoe UI', Arial, sans-serif; margin: 24px; background: #f0f4f8; color: #222; }");
        sb.AppendLine("        h1 { text-align: center; color: #2c3e50; margin-bottom: 4px; }");
        sb.AppendLine("        .generated { text-align: center; color: #888; font-size: 0.85em; margin-bottom: 24px; }");
        sb.AppendLine("        .chart-container { width: 90%; max-width: 900px; margin: 0 auto 32px; background: #fff; padding: 24px; border-radius: 10px; box-shadow: 0 2px 12px rgba(0,0,0,0.08); }");
        sb.AppendLine("        .stats-table { width: 90%; max-width: 900px; margin: 0 auto 32px; border-collapse: collapse; background: #fff; border-radius: 10px; overflow: hidden; box-shadow: 0 2px 12px rgba(0,0,0,0.08); }");
        sb.AppendLine("        .stats-table th { background: #2c3e50; color: #fff; padding: 10px 14px; text-align: left; }");
        sb.AppendLine("        .stats-table td { padding: 8px 14px; border-bottom: 1px solid #eee; }");
        sb.AppendLine("        .stats-table tr:last-child td { border-bottom: none; }");
        sb.AppendLine("        .stats-table tr:nth-child(even) td { background: #f8fafc; }");
        sb.AppendLine("    </style>");
        sb.AppendLine("</head>");
    }

    // ── Summary statistics table ───────────────────────────────────────────────

    private static void AppendSummaryTable(StringBuilder sb, HealthDataCollection collection)
    {
        sb.AppendLine("    <table class=\"stats-table\">");
        sb.AppendLine("        <thead><tr><th>Metric</th><th>Records</th><th>Average</th><th>Min</th><th>Max</th></tr></thead>");
        sb.AppendLine("        <tbody>");

        if (collection.HeartRateRecords.Any())
        {
            var avg = (int)collection.HeartRateRecords.Average(r => r.AverageBpm);
            var min = collection.HeartRateRecords.Min(r => r.MinimumBpm);
            var max = collection.HeartRateRecords.Max(r => r.MaximumBpm);
            sb.AppendLine($"            <tr><td>Heart Rate (BPM)</td><td>{collection.HeartRateRecords.Count}</td><td>{avg}</td><td>{min}</td><td>{max}</td></tr>");
        }

        if (collection.StepsRecords.Any())
        {
            var avg = (int)collection.StepsRecords.Average(r => r.TotalSteps);
            var min = collection.StepsRecords.Min(r => r.TotalSteps);
            var max = collection.StepsRecords.Max(r => r.TotalSteps);
            sb.AppendLine($"            <tr><td>Steps</td><td>{collection.StepsRecords.Count}</td><td>{avg:N0}</td><td>{min:N0}</td><td>{max:N0}</td></tr>");
        }

        if (collection.SleepRecords.Any())
        {
            var avgH = Math.Round(collection.SleepRecords.Average(r => r.DurationMinutes) / 60.0, 1);
            var minH = Math.Round(collection.SleepRecords.Min(r => r.DurationMinutes) / 60.0, 1);
            var maxH = Math.Round(collection.SleepRecords.Max(r => r.DurationMinutes) / 60.0, 1);
            sb.AppendLine($"            <tr><td>Sleep (hours)</td><td>{collection.SleepRecords.Count}</td><td>{avgH}</td><td>{minH}</td><td>{maxH}</td></tr>");
        }

        if (collection.SpO2Records.Any())
        {
            var avg = (int)collection.SpO2Records.Average(r => r.AveragePercentage);
            var min = collection.SpO2Records.Min(r => r.MinimumPercentage);
            var max = collection.SpO2Records.Max(r => r.MaximumPercentage);
            sb.AppendLine($"            <tr><td>SpO2 (%)</td><td>{collection.SpO2Records.Count}</td><td>{avg}</td><td>{min}</td><td>{max}</td></tr>");
        }

        if (collection.ActivityRecords.Any())
        {
            var totalCal = collection.ActivityRecords.Sum(r => r.CaloriesBurned);
            var avgDur = (int)collection.ActivityRecords.Average(r => r.DurationMinutes);
            sb.AppendLine($"            <tr><td>Activity (sessions)</td><td>{collection.ActivityRecords.Count}</td><td>{avgDur} min avg</td><td>{totalCal:N0} kcal total</td><td>—</td></tr>");
        }

        sb.AppendLine("        </tbody>");
        sb.AppendLine("    </table>");
    }

    // ── Canvas helpers ─────────────────────────────────────────────────────────

    private static void AppendCanvas(StringBuilder sb, string id)
    {
        sb.AppendLine("    <div class=\"chart-container\">");
        sb.AppendLine($"        <canvas id=\"{id}\"></canvas>");
        sb.AppendLine("    </div>");
    }

    // ── Individual chart builders ──────────────────────────────────────────────

    private static void AppendHeartRateChart(StringBuilder sb, List<HeartRateData> records)
    {
        var sorted = records.OrderBy(r => r.RecordDate).ToList();
        var labels = string.Join(",", sorted.Select(r => $"'{r.RecordDate:MMM dd}'"));
        var avgData = string.Join(",", sorted.Select(r => r.AverageBpm));
        var minData = string.Join(",", sorted.Select(r => r.MinimumBpm));
        var maxData = string.Join(",", sorted.Select(r => r.MaximumBpm));
        sb.AppendLine($"        new Chart(document.getElementById('heartRateChart'), {{ type: 'line', data: {{ labels: [{labels}], datasets: [" +
            $"{{ label: 'Avg BPM', data: [{avgData}], borderColor: 'rgba(255,99,132,1)', backgroundColor: 'rgba(255,99,132,0.1)', fill: true, tension: 0.3 }}," +
            $"{{ label: 'Min BPM', data: [{minData}], borderColor: 'rgba(255,159,64,0.7)', borderDash: [5,5], fill: false, tension: 0.3 }}," +
            $"{{ label: 'Max BPM', data: [{maxData}], borderColor: 'rgba(255,205,86,0.7)', borderDash: [5,5], fill: false, tension: 0.3 }}" +
            $"] }}, options: {{ responsive: true, plugins: {{ title: {{ display: true, text: 'Heart Rate (BPM)' }} }} }} }});");
    }

    private static void AppendStepsChart(StringBuilder sb, List<StepsData> records)
    {
        var sorted = records.OrderBy(r => r.RecordDate).ToList();
        var labels = string.Join(",", sorted.Select(r => $"'{r.RecordDate:MMM dd}'"));
        var stepsData = string.Join(",", sorted.Select(r => r.TotalSteps));
        sb.AppendLine($"        new Chart(document.getElementById('stepsChart'), {{ type: 'bar', data: {{ labels: [{labels}], datasets: [" +
            $"{{ label: 'Daily Steps', data: [{stepsData}], backgroundColor: 'rgba(54,162,235,0.6)', borderColor: 'rgba(54,162,235,1)', borderWidth: 1 }}" +
            $"] }}, options: {{ responsive: true, plugins: {{ title: {{ display: true, text: 'Daily Steps' }} }} }} }});");
    }

    private static void AppendSleepDurationChart(StringBuilder sb, List<SleepData> records)
    {
        var sorted = records.OrderBy(r => r.RecordDate).ToList();
        var labels = string.Join(",", sorted.Select(r => $"'{r.RecordDate:MMM dd}'"));
        var sleepHours = string.Join(",", sorted.Select(r => Math.Round(r.DurationMinutes / 60.0, 1)));
        sb.AppendLine($"        new Chart(document.getElementById('sleepChart'), {{ type: 'bar', data: {{ labels: [{labels}], datasets: [" +
            $"{{ label: 'Sleep Duration (hours)', data: [{sleepHours}], backgroundColor: 'rgba(75,192,192,0.6)', borderColor: 'rgba(75,192,192,1)', borderWidth: 1 }}" +
            $"] }}, options: {{ responsive: true, plugins: {{ title: {{ display: true, text: 'Sleep Duration' }} }} }} }});");
    }

    private static void AppendSleepCompositionChart(StringBuilder sb, List<SleepData> records)
    {
        var sorted = records.OrderBy(r => r.RecordDate).ToList();
        var labels = string.Join(",", sorted.Select(r => $"'{r.RecordDate:MMM dd}'"));
        var deep = string.Join(",", sorted.Select(r => r.DeepSleepMinutes));
        var rem = string.Join(",", sorted.Select(r => r.RemSleepMinutes));
        var light = string.Join(",", sorted.Select(r => r.LightSleepMinutes));
        var awake = string.Join(",", sorted.Select(r => r.AwakeMinutes));
        sb.AppendLine($"        new Chart(document.getElementById('sleepCompositionChart'), {{ type: 'bar', data: {{ labels: [{labels}], datasets: [" +
            $"{{ label: 'Deep (min)', data: [{deep}], backgroundColor: 'rgba(54,162,235,0.8)' }}," +
            $"{{ label: 'REM (min)', data: [{rem}], backgroundColor: 'rgba(153,102,255,0.8)' }}," +
            $"{{ label: 'Light (min)', data: [{light}], backgroundColor: 'rgba(75,192,192,0.6)' }}," +
            $"{{ label: 'Awake (min)', data: [{awake}], backgroundColor: 'rgba(255,99,132,0.5)' }}" +
            $"] }}, options: {{ responsive: true, scales: {{ x: {{ stacked: true }}, y: {{ stacked: true }} }}, plugins: {{ title: {{ display: true, text: 'Sleep Composition (minutes)' }} }} }} }});");
    }

    private static void AppendSpO2Chart(StringBuilder sb, List<SpO2Data> records)
    {
        var sorted = records.OrderBy(r => r.RecordDate).ToList();
        var labels = string.Join(",", sorted.Select(r => $"'{r.RecordDate:MMM dd}'"));
        var avgData = string.Join(",", sorted.Select(r => r.AveragePercentage));
        var minData = string.Join(",", sorted.Select(r => r.MinimumPercentage));
        sb.AppendLine($"        new Chart(document.getElementById('spo2Chart'), {{ type: 'line', data: {{ labels: [{labels}], datasets: [" +
            $"{{ label: 'Avg SpO2 (%)', data: [{avgData}], borderColor: 'rgba(46,204,113,1)', backgroundColor: 'rgba(46,204,113,0.15)', fill: true, tension: 0.3 }}," +
            $"{{ label: 'Min SpO2 (%)', data: [{minData}], borderColor: 'rgba(231,76,60,0.8)', borderDash: [5,5], fill: false, tension: 0.3 }}" +
            $"] }}, options: {{ responsive: true, scales: {{ y: {{ min: 85, max: 100 }} }}, plugins: {{ title: {{ display: true, text: 'Blood Oxygen Saturation (SpO2)' }} }} }} }});");
    }

    private static void AppendActivityChart(StringBuilder sb, List<ActivityData> records)
    {
        var sorted = records.OrderBy(r => r.RecordDate).ToList();
        var labels = string.Join(",", sorted.Select(r => $"'{r.RecordDate:MMM dd}'"));
        var calories = string.Join(",", sorted.Select(r => r.CaloriesBurned));
        var duration = string.Join(",", sorted.Select(r => r.DurationMinutes));
        sb.AppendLine($"        new Chart(document.getElementById('activityChart'), {{ type: 'bar', data: {{ labels: [{labels}], datasets: [" +
            $"{{ label: 'Calories Burned', data: [{calories}], backgroundColor: 'rgba(255,159,64,0.7)', yAxisID: 'yCalories' }}," +
            $"{{ label: 'Duration (min)', data: [{duration}], backgroundColor: 'rgba(201,203,207,0.7)', type: 'line', yAxisID: 'yDuration', tension: 0.3 }}" +
            $"] }}, options: {{ responsive: true, scales: {{ yCalories: {{ type: 'linear', position: 'left' }}, yDuration: {{ type: 'linear', position: 'right', grid: {{ drawOnChartArea: false }} }} }}, plugins: {{ title: {{ display: true, text: 'Activity Summary' }} }} }} }});");
    }
}
