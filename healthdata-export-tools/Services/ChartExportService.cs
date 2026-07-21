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

        await using var fs = File.Create(outputPath);
        await using var writer = new StreamWriter(fs, Encoding.UTF8);
        await BuildHtmlAsync(writer, collection, options).ConfigureAwait(false);
    }

    // ── HTML assembly ──────────────────────────────────────────────────────────

    private static async Task BuildHtmlAsync(
        StreamWriter writer,
        HealthDataCollection collection,
        ChartExportOptions options)
    {
        AppendHead(writer, options.Title);
        await writer.WriteLineAsync("<body>").ConfigureAwait(false);
        await writer.WriteLineAsync($" <h1>{System.Web.HttpUtility.HtmlEncode(options.Title)}</h1>").ConfigureAwait(false);
        await writer.WriteLineAsync($" <p class=\"generated\">Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC</p>").ConfigureAwait(false);

        if (options.IncludeSummaryTable)
            await AppendSummaryTableAsync(writer, collection).ConfigureAwait(false);

        // Canvas placeholders
        if (collection.HeartRateRecords.Any())
            await AppendCanvasAsync(writer, "heartRateChart").ConfigureAwait(false);

        if (collection.StepsRecords.Any())
            await AppendCanvasAsync(writer, "stepsChart").ConfigureAwait(false);

        if (collection.SleepRecords.Any())
        {
            await AppendCanvasAsync(writer, "sleepChart").ConfigureAwait(false);
            if (options.IncludeSleepCompositionChart)
                await AppendCanvasAsync(writer, "sleepCompositionChart").ConfigureAwait(false);
        }

        if (options.IncludeSpO2Chart && collection.SpO2Records.Any())
            await AppendCanvasAsync(writer, "spo2Chart").ConfigureAwait(false);

        if (options.IncludeActivityChart && collection.ActivityRecords.Any())
            await AppendCanvasAsync(writer, "activityChart").ConfigureAwait(false);

        // Chart.js scripts
        await writer.WriteLineAsync(" <script>").ConfigureAwait(false);

        if (collection.HeartRateRecords.Any())
            await AppendHeartRateChartAsync(writer, collection.HeartRateRecords).ConfigureAwait(false);

        if (collection.StepsRecords.Any())
            await AppendStepsChartAsync(writer, collection.StepsRecords).ConfigureAwait(false);

        if (collection.SleepRecords.Any())
        {
            await AppendSleepDurationChartAsync(writer, collection.SleepRecords).ConfigureAwait(false);
            if (options.IncludeSleepCompositionChart)
                await AppendSleepCompositionChartAsync(writer, collection.SleepRecords).ConfigureAwait(false);
        }

        if (options.IncludeSpO2Chart && collection.SpO2Records.Any())
            await AppendSpO2ChartAsync(writer, collection.SpO2Records).ConfigureAwait(false);

        if (options.IncludeActivityChart && collection.ActivityRecords.Any())
            await AppendActivityChartAsync(writer, collection.ActivityRecords).ConfigureAwait(false);

        await writer.WriteLineAsync(" </script>").ConfigureAwait(false);
        await writer.WriteLineAsync("</body>").ConfigureAwait(false);
        await writer.WriteLineAsync("</html>").ConfigureAwait(false);
    }

    // ── Head / Style ───────────────────────────────────────────────────────────

    private static void AppendHead(StreamWriter writer, string title)
    {
        writer.WriteLine("<!DOCTYPE html>");
        writer.WriteLine("<html lang=\"en\">");
        writer.WriteLine("<head>");
        writer.WriteLine(" <meta charset=\"UTF-8\">");
        writer.WriteLine(" <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
        writer.WriteLine($" <title>{System.Web.HttpUtility.HtmlEncode(title)}</title>");
        writer.WriteLine(" <script src=\"https://cdn.jsdelivr.net/npm/chart.js\"></script>");
        writer.WriteLine(" <style>");
        writer.WriteLine(" body { font-family: 'Segoe UI', Arial, sans-serif; margin: 24px; background: #f0f4f8; color: #222; }");
        writer.WriteLine(" h1 { text-align: center; color: #2c3e50; margin-bottom: 4px; }");
        writer.WriteLine(" .generated { text-align: center; color: #888; font-size: 0.85em; margin-bottom: 24px; }");
        writer.WriteLine(" .chart-container { width: 90%; max-width: 900px; margin: 0 auto 32px; background: #fff; padding: 24px; border-radius: 10px; box-shadow: 0 2px 12px 0.08); }");
        writer.WriteLine(" .stats-table { width: 90%; max-width: 900px; margin: 0 auto 32px; border-collapse: collapse; background: #fff; border-radius: 0 2px 12px rgba(0,0,0,0.08); }");
        writer.WriteLine(" .stats-table { width: 90%; max-width: 900px; margin: 0 auto 32px; border-collapse: collapse; background: #fff; border-radius: 10px; overflow: hidden; box-shadow: 0 2px 12px rgba(0,0,0,0.08); }");
        writer.WriteLine(" .stats-table th { background: #2c3e50; color: #fff; padding: 10px 14px; text-align: left; }");
        writer.WriteLine(" .stats-table td { padding: 8px 14px; border-bottom: 1px solid #eee; }");
        writer.WriteLine(" .stats-table tr:last-child td { border-bottom: none; }");
        writer.WriteLine(" .stats-table tr:nth-child(even) td { background: #f8fafc; }");
        writer.WriteLine(" </style>");
        writer.WriteLine("</head>");
    }

    // ── Summary statistics table ───────────────────────────────────────────────

    private static async Task AppendSummaryTableAsync(StreamWriter writer, HealthDataCollection collection)
    {
        await writer.WriteLineAsync(" <table class=\"stats-table\">").ConfigureAwait(false);
        await writer.WriteLineAsync(" <thead><tr><th>Metric</th><th>Records</th><th>Average</th><th>Min</th><th>Max</th></tr></thead>").ConfigureAwait(false);
        await writer.WriteLineAsync(" <tbody>").ConfigureAwait(false);

        if (collection.HeartRateRecords.Any())
        {
            var avg = (int)collection.HeartRateRecords.Average(r => r.AverageBpm);
            var min = collection.HeartRateRecords.Min(r => r.MinimumBpm);
            var max = collection.HeartRateRecords.Max(r => r.MaximumBpm);
            await writer.WriteLineAsync($" <tr><td>Heart Rate (BPM)</td><td>{collection.HeartRateRecords.Count}</td><td>{avg}</td><td>{min}</td><td>{max}</td></tr>").ConfigureAwait(false);
        }

        if (collection.StepsRecords.Any())
        {
            var avg = (int)collection.StepsRecords.Average(r => r.TotalSteps);
            var min = collection.StepsRecords.Min(r => r.TotalSteps);
            var max = collection.StepsRecords.Max(r => r.TotalSteps);
            await writer.WriteLineAsync($" <tr><td>Steps</td><td>{collection.StepsRecords.Count}</td><td>{avg:N0}</td><td>{min:N0}</td><td>{max:N0}</td></tr>").ConfigureAwait(false);
        }

        if (collection.SleepRecords.Any())
        {
            var avgH = Math.Round(collection.SleepRecords.Average(r => r.DurationMinutes) / 60.0, 1);
            var minH = Math.Round(collection.SleepRecords.Min(r => r.DurationMinutes) / 60.0, 1);
            var maxH = Math.Round(collection.SleepRecords.Max(r => r.DurationMinutes) / 60.0, 1);
            await writer.WriteLineAsync($" <tr><td>Sleep (hours)</td><td>{collection.SleepRecords.Count}</td><td>{avgH}</td><td>{minH}</td><td>{maxH}</td></tr>").ConfigureAwait(false);
        }

        if (collection.SpO2Records.Any())
        {
            var avg = (int)collection.SpO2Records.Average(r => r.AveragePercentage);
            var min = collection.SpO2Records.Min(r => r.MinimumPercentage);
            var max = collection.SpO2Records.Max(r => r.MaximumPercentage);
            await writer.WriteLineAsync($" <tr><td>SpO2 (%)</td><td>{collection.SpO2Records.Count}</td><td>{avg}</td><td>{min}</td><td>{max}</td></tr>").ConfigureAwait(false);
        }

        if (collection.ActivityRecords.Any())
        {
            var totalCal = collection.ActivityRecords.Sum(r => r.CaloriesBurned);
            var avgDur = (int)collection.ActivityRecords.Average(r => r.DurationMinutes);
            await writer.WriteLineAsync($" <tr><td>Activity (sessions)</td><td>{collection.ActivityRecords.Count}</td><td>{avgDur} min avg</td><td>{totalCal:N0} kcal total</td><td>—</td></tr>").ConfigureAwait(false);
        }

        await writer.WriteLineAsync(" </tbody>").ConfigureAwait(false);
        await writer.WriteLineAsync(" </table>").ConfigureAwait(false);
    }

    // ── Canvas helpers ─────────────────────────────────────────────────────────

    private static async Task AppendCanvasAsync(StreamWriter writer, string id)
    {
        await writer.WriteLineAsync(" <div class=\"chart-container\">").ConfigureAwait(false);
        await writer.WriteLineAsync($" <canvas id=\"{id}\"></canvas>").ConfigureAwait(false);
        await writer.WriteLineAsync(" </div>").ConfigureAwait(false);
    }

    // ── Individual chart builders ──────────────────────────────────────────────

    private static async Task AppendHeartRateChartAsync(StreamWriter writer, List<HeartRateData> records)
    {
        var sorted = records.OrderBy(r => r.RecordDate).ToList();
        var labels = string.Join(",", sorted.Select(r => $"'{r.RecordDate:MMM dd}'"));
        var avgData = string.Join(",", sorted.Select(r => r.AverageBpm));
        var minData = string.Join(",", sorted.Select(r => r.MinimumBpm));
        var maxData = string.Join(",", sorted.Select(r => r.MaximumBpm));

        var js = "new Chart(document.getElementById('heartRateChart'), { type: 'line', data: { labels: [" + labels + "], datasets: [";
        js += "{ label: 'Avg BPM', data: [" + avgData + "], borderColor: 'rgba(255,99,132,1)', backgroundColor: 'rgba(255,99,132,0.1)', fill: true, tension: 0.3 },";
        js += "{ label: 'Min BPM', data: [" + minData + "], borderColor: 'rgba(255,159,64,0.7)', borderDash: [5,5], fill: false, tension: 0.3 },";
        js += "{ label: 'Max BPM', data: [" + maxData + "], borderColor: 'rgba(255,205,86,0.7)', borderDash: [5,5], fill: false, tension: 0.3 }";
        js += "] }, options: { responsive: true, plugins: { title: { display: true, text: 'Heart Rate (BPM)' } } } });";
        await writer.WriteLineAsync(js).ConfigureAwait(false);
    }

    private static async Task AppendStepsChartAsync(StreamWriter writer, List<StepsData> records)
    {
        var sorted = records.OrderBy(r => r.RecordDate).ToList();
        var labels = string.Join(",", sorted.Select(r => $"'{r.RecordDate:MMM dd}'"));
        var stepsData = string.Join(",", sorted.Select(r => r.TotalSteps));

        var js = "new Chart(document.getElementById('stepsChart'), { type: 'bar', data: { labels: [" + labels + "], datasets: [";
        js += "{ label: 'Daily Steps', data: [" + stepsData + "], backgroundColor: 'rgba(54,162,235,0.6)', borderColor: 'rgba(54,162,235,1)', borderWidth: 1 }";
        js += "] }, options: { responsive: true, plugins: { title: { display: true, text: 'Daily Steps' } } } });";
        await writer.WriteLineAsync(js).ConfigureAwait(false);
    }

    private static async Task AppendSleepDurationChartAsync(StreamWriter writer, List<SleepData> records)
    {
        var sorted = records.OrderBy(r => r.RecordDate).ToList();
        var labels = string.Join(",", sorted.Select(r => $"'{r.RecordDate:MMM dd}'"));
        var sleepHours = string.Join(",", sorted.Select(r => Math.Round(r.DurationMinutes / 60.0, 1)));

        var js = "new Chart(document.getElementById('sleepChart'), { type: 'bar', data: { labels: [" + labels + "], datasets: [";
        js += "{ label: 'Sleep Duration (hours)', data: [" + sleepHours + "], backgroundColor: 'rgba(75,192,192,0.6)', borderColor: 'rgba(75,192,192,1)', borderWidth: 1 }";
        js += "] }, options: { responsive: true, plugins: { title: { display: true, text: 'Sleep Duration' } } } });";
        await writer.WriteLineAsync(js).ConfigureAwait(false);
    }

    private static async Task AppendSleepCompositionChartAsync(StreamWriter writer, List<SleepData> records)
    {
        var sorted = records.OrderBy(r => r.RecordDate).ToList();
        var labels = string.Join(",", sorted.Select(r => $"'{r.RecordDate:MMM dd}'"));
        var deep = string.Join(",", sorted.Select(r => r.DeepSleepMinutes));
        var rem = string.Join(",", sorted.Select(r => r.RemSleepMinutes));
        var light = string.Join(",", sorted.Select(r => r.LightSleepMinutes));
        var awake = string.Join(",", sorted.Select(r => r.AwakeMinutes));

        var js = "new Chart(document.getElementById('sleepCompositionChart'), { type: 'bar', data: { labels: [" + labels + "], datasets: [";
        js += "{ label: 'Deep (min)', data: [" + deep + "], backgroundColor: 'rgba(54,162,235,0.8)' },";
        js += "{ label: 'REM (min)', data: [" + rem + "], backgroundColor: 'rgba(153,102,255,0.8)' },";
        js += "{ label: 'Light (min)', data: [" + light + "], backgroundColor: 'rgba(75,192,192,0.6)' },";
        js += "{ label: 'Awake (min)', data: [" + awake + "], backgroundColor: 'rgba(255,99,132,0.5)' }";
        js += "] }, options: { responsive: true, scales: { x: { stacked: true }, y: { stacked: true } }, plugins: { title: { display: true, text: 'Sleep Composition (minutes)' } } } });";
        await writer.WriteLineAsync(js).ConfigureAwait(false);
    }

    private static async Task AppendSpO2ChartAsync(StreamWriter writer, List<SpO2Data> records)
    {
        var sorted = records.OrderBy(r => r.RecordDate).ToList();
        var labels = string.Join(",", sorted.Select(r => $"'{r.RecordDate:MMM dd}'"));
        var avgData = string.Join(",", sorted.Select(r => r.AveragePercentage));
        var minData = string.Join(",", sorted.Select(r => r.MinimumPercentage));

        var js = "new Chart(document.getElementById('spo2Chart'), { type: 'line', data: { labels: [" + labels + "], datasets: [";
        js += "{ label: 'Avg SpO2 (%)', data: [" + avgData + "], borderColor: 'rgba(46,204,113,1)', backgroundColor: 'rgba(46,204,113,0.15)', fill: true, tension: 0.3 },";
        js += "{ label: 'Min SpO2 (%)', data: [" + minData + "], borderColor: 'rgba(231,76,60,0.8)', borderDash: [5,5], fill: false, tension: 0.3 }";
        js += "] }, options: { responsive: true, scales: { y: { min: 85, max: 100 } }, plugins: { title: { display: true, text: 'Blood Oxygen Saturation (SpO2)' } } } });";
        await writer.WriteLineAsync(js).ConfigureAwait(false);
    }

    private static async Task AppendActivityChartAsync(StreamWriter writer, List<ActivityData> records)
    {
        var sorted = records.OrderBy(r => r.RecordDate).ToList();
        var labels = string.Join(",", sorted.Select(r => $"'{r.RecordDate:MMM dd}'"));
        var calories = string.Join(",", sorted.Select(r => r.CaloriesBurned));
        var duration = string.Join(",", sorted.Select(r => r.DurationMinutes));

        var js = "new Chart(document.getElementById('activityChart'), { type: 'bar', data: { labels: [" + labels + "], datasets: [";
        js += "{ label: 'Calories Burned', data: [" + calories + "], backgroundColor: 'rgba(255,159,64,0.7)', yAxisID: 'yCalories' },";
        js += "{ label: 'Duration (min)', data: [" + duration + "], backgroundColor: 'rgba(201,203,207,0.7)', type: 'line', yAxisID: 'yDuration', tension: 0.3 }";
        js += "] }, options: { responsive: true, scales: { yCalories: { type: 'linear', position: 'left' }, yDuration: { type: 'linear', position: 'right', grid: { drawOnChartArea: false } } }, plugins: { title: { display: true, text: 'Activity Summary' } } } });";
        await writer.WriteLineAsync(js).ConfigureAwait(false);
    }
}