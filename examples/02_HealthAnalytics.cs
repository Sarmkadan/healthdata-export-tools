// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using HealthDataExportTools;
using HealthDataExportTools.Services;

namespace HealthDataExportTools.Examples;

/// Example 2: Health Data Analytics
/// Demonstrates how to analyze parsed health data and generate reports.
/// Shows sleep quality, heart rate, SpO2 analysis and overall health scoring.
class HealthAnalyticsExample
{
    static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("═══════════════════════════════════════════");
            Console.WriteLine("  Example 2: Health Data Analytics");
            Console.WriteLine("═══════════════════════════════════════════\n");

            // Parse health data
            var parser = new HealthDataParserService();
            Console.WriteLine("📂 Parsing health data...");
            var healthData = await parser.ParseHealthDataAsync("./exports/export.zip");
            Console.WriteLine("✓ Data parsed successfully\n");

            // Initialize analytics service
            var analytics = new AnalyticsService();

            // Analyze Sleep Quality
            Console.WriteLine("━━━ Sleep Quality Analysis ━━━");
            if (healthData.SleepRecords.Count > 0)
            {
                var sleepReport = analytics.AnalyzeSleepQuality(healthData.SleepRecords);

                Console.WriteLine($"  Quality Level:     {sleepReport.Description}");
                Console.WriteLine($"  Avg Duration:      {sleepReport.AverageDuration:F1} minutes ({sleepReport.AverageDuration / 60:F1} hours)");
                Console.WriteLine($"  Deep Sleep:        {sleepReport.AverageDeepSleep:F1} minutes");
                Console.WriteLine($"  REM Sleep:         {sleepReport.AverageRemSleep:F1} minutes");
                Console.WriteLine($"  Light Sleep:       {sleepReport.AverageLightSleep:F1} minutes");
                Console.WriteLine($"  Excellent Nights:  {sleepReport.ExcellentNights}/{sleepReport.TotalNights}");
                Console.WriteLine($"  Trend:             {sleepReport.TrendDirection}\n");
            }
            else
            {
                Console.WriteLine("  No sleep data available\n");
            }

            // Analyze Heart Rate
            Console.WriteLine("━━━ Heart Rate Analysis ━━━");
            if (healthData.HeartRateRecords.Count > 0)
            {
                var hrReport = analytics.AnalyzeHeartRate(healthData.HeartRateRecords);

                Console.WriteLine($"  Avg Heart Rate:    {hrReport.AverageHeartRate} bpm");
                Console.WriteLine($"  Resting HR:        {hrReport.RestingHeartRate} bpm");
                Console.WriteLine($"  Min HR:            {hrReport.MinimumHeartRate} bpm");
                Console.WriteLine($"  Max HR:            {hrReport.MaximumHeartRate} bpm");
                Console.WriteLine($"  HR Variability:    {hrReport.Variability:F2}");
                Console.WriteLine($"  Avg Stress Level:  {hrReport.AverageStressLevel}/100");
                Console.WriteLine($"  Trend:             {hrReport.TrendDirection}\n");
            }
            else
            {
                Console.WriteLine("  No heart rate data available\n");
            }

            // Analyze SpO2
            Console.WriteLine("━━━ SpO2 (Blood Oxygen) Analysis ━━━");
            if (healthData.SpO2Records.Count > 0)
            {
                var spO2Report = analytics.AnalyzeSpO2Health(healthData.SpO2Records);

                Console.WriteLine($"  Status:            {spO2Report.Status}");
                Console.WriteLine($"  Avg SpO2:          {spO2Report.AverageSpO2}%");
                Console.WriteLine($"  Min SpO2:          {spO2Report.MinimumSpO2}%");
                Console.WriteLine($"  Max SpO2:          {spO2Report.MaximumSpO2}%");
                Console.WriteLine($"  Low SpO2 Events:   {spO2Report.TotalLowEvents}");
                Console.WriteLine($"  Risk Level:        {spO2Report.RiskLevel}\n");
            }
            else
            {
                Console.WriteLine("  No SpO2 data available\n");
            }

            // Analyze Activity/Steps
            Console.WriteLine("━━━ Activity & Steps Analysis ━━━");
            if (healthData.StepsRecords.Count > 0)
            {
                var activityReport = analytics.AnalyzeActivityTrends(healthData.StepsRecords);

                Console.WriteLine($"  Avg Daily Steps:   {activityReport.AverageDailySteps:N0}");
                Console.WriteLine($"  Avg Distance:      {activityReport.AverageDailyDistance:F2} km");
                Console.WriteLine($"  Avg Calories:      {activityReport.AverageDailyCalories:N0} kcal");
                Console.WriteLine($"  Goal Achievement:  {activityReport.GoalAchievementRate:F1}%");
                Console.WriteLine($"  Active Days:       {activityReport.ActiveDays}/{activityReport.TotalDays}");
                Console.WriteLine($"  Trend:             {activityReport.TrendDirection}\n");
            }
            else
            {
                Console.WriteLine("  No steps data available\n");
            }

            // Calculate Overall Health Score
            Console.WriteLine("━━━ Overall Health Score ━━━");
            var healthScore = analytics.CalculateHealthScore(healthData);
            Console.WriteLine($"  Health Score:      {healthScore}/100");
            Console.WriteLine($"  Rating:            {GetHealthRating(healthScore)}\n");

            Console.WriteLine("✅ Analysis completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"   Details: {ex.InnerException.Message}");
            Environment.Exit(1);
        }
    }

    static string GetHealthRating(int score)
    {
        return score switch
        {
            >= 85 => "🟢 Excellent",
            >= 70 => "🟢 Good",
            >= 55 => "🟡 Fair",
            >= 40 => "🟠 Poor",
            _ => "🔴 Critical"
        };
    }
}
