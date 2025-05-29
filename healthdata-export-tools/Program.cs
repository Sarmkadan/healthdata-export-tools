// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using HealthDataExportTools.Configuration;
using HealthDataExportTools.Domain.Enums;
using HealthDataExportTools.Domain.Models;
using HealthDataExportTools.Services;

namespace HealthDataExportTools;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║         Health Data Export Tools v1.0.0                    ║");
            Console.WriteLine("║     Parse & Analyze Zepp/Amazfit/Garmin Health Data        ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            var options = new HealthDataExportOptions
            {
                InputPath = args.Length > 0 ? args[0] : "./exports",
                OutputPath = args.Length > 1 ? args[1] : "./output",
                DatabasePath = args.Length > 2 ? args[2] : "./healthdata.db",
                ExportFormat = ExportFormat.All,
                ValidateData = true,
                PerformAnalysis = true,
                VerboseLogging = false
            };

            Console.WriteLine($"Configuration:");
            Console.WriteLine($"  • Input Path: {options.InputPath}");
            Console.WriteLine($"  • Output Path: {options.OutputPath}");
            Console.WriteLine($"  • Database: {options.DatabasePath}");
            Console.WriteLine($"  • Export Format: {options.ExportFormat}");
            Console.WriteLine();

            var errors = options.Validate();
            if (errors.Count > 0)
            {
                Console.WriteLine("⚠ Configuration errors:");
                foreach (var error in errors)
                    Console.WriteLine($"  • {error}");
                Console.WriteLine();
                return;
            }

            var serviceProvider = ServiceCollectionExtensions.CreateTestServiceProvider(options.DatabasePath);
            var parser = (HealthDataParserService?)serviceProvider.GetService(typeof(HealthDataParserService));
            var analytics = (AnalyticsService?)serviceProvider.GetService(typeof(AnalyticsService));
            var exporter = (ExportService?)serviceProvider.GetService(typeof(ExportService));

            if (parser == null || analytics == null || exporter == null)
            {
                Console.WriteLine("❌ Failed to initialize services");
                return;
            }

            // Create sample data for demonstration
            Console.WriteLine("📊 Creating sample health data...");
            var sampleData = CreateSampleData();
            Console.WriteLine($"✓ Created {sampleData.GetTotalRecordCount()} sample records");
            Console.WriteLine();

            // Perform analysis
            Console.WriteLine("🔍 Performing analysis...");
            var sleepReport = analytics.AnalyzeSleepQuality(sampleData.SleepRecords);
            var spO2Report = analytics.AnalyzeSpO2Health(sampleData.SpO2Records);
            var healthScore = analytics.CalculateHealthScore(sampleData);

            Console.WriteLine($"  • Sleep Quality: {sleepReport.Description}");
            Console.WriteLine($"    - Average Duration: {sleepReport.AverageDuration:F1} minutes");
            Console.WriteLine($"    - Deep Sleep: {sleepReport.AverageDeepSleep:F1} minutes");
            Console.WriteLine($"    - REM Sleep: {sleepReport.AverageRemSleep:F1} minutes");
            Console.WriteLine($"    - Excellent Nights: {sleepReport.ExcellentNights}/{sleepReport.TotalNights}");
            Console.WriteLine();
            Console.WriteLine($"  • SpO2 Status: {spO2Report.Status}");
            Console.WriteLine($"    - Average: {spO2Report.AverageSpO2}%");
            Console.WriteLine($"    - Minimum: {spO2Report.MinimumSpO2}%");
            Console.WriteLine($"    - Low Events: {spO2Report.TotalLowEvents}");
            Console.WriteLine();
            Console.WriteLine($"  • Overall Health Score: {healthScore}/100");
            Console.WriteLine();

            // Export data
            Console.WriteLine("💾 Exporting data...");
            Directory.CreateDirectory(options.OutputPath);

            switch (options.ExportFormat)
            {
                case ExportFormat.Json:
                    await exporter.ExportToJsonAsync(sampleData,
                        Path.Combine(options.OutputPath, "health_data.json"));
                    Console.WriteLine("  ✓ Exported to JSON");
                    break;

                case ExportFormat.Csv:
                    if (sampleData.SleepRecords.Any())
                        await exporter.ExportSleepToCsvAsync(sampleData.SleepRecords,
                            Path.Combine(options.OutputPath, "sleep.csv"));

                    if (sampleData.HeartRateRecords.Any())
                        await exporter.ExportHeartRateToCsvAsync(sampleData.HeartRateRecords,
                            Path.Combine(options.OutputPath, "heart_rate.csv"));

                    if (sampleData.StepsRecords.Any())
                        await exporter.ExportStepsToCsvAsync(sampleData.StepsRecords,
                            Path.Combine(options.OutputPath, "steps.csv"));

                    Console.WriteLine("  ✓ Exported to CSV");
                    break;

                case ExportFormat.All:
                    await exporter.ExportCompleteAsync(sampleData, options.OutputPath, ExportFormat.Json);
                    await exporter.ExportCompleteAsync(sampleData, options.OutputPath, ExportFormat.Csv);
                    Console.WriteLine("  ✓ Exported to all formats");
                    break;
            }

            Console.WriteLine();
            Console.WriteLine("✅ Export completed successfully!");
            Console.WriteLine($"📂 Output location: {Path.GetFullPath(options.OutputPath)}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"   Details: {ex.InnerException.Message}");
            Environment.Exit(1);
        }
    }

    static HealthDataCollection CreateSampleData()
    {
        var collection = new HealthDataCollection();
        var today = DateTime.UtcNow;

        // Create 7 days of sleep data
        for (int i = 0; i < 7; i++)
        {
            var date = today.AddDays(-i);
            var sleep = new Domain.Models.SleepData
            {
                RecordDate = date,
                DeviceId = "Amazfit-12345",
                SleepStart = date.AddHours(-8).AddMinutes(-30),
                SleepEnd = date,
                DurationMinutes = 510, // 8.5 hours
                DeepSleepMinutes = 90,
                LightSleepMinutes = 300,
                RemSleepMinutes = 90,
                AwakeMinutes = 30,
                Score = 82 + i,
                AverageHeartRate = 55 - i
            };
            sleep.Quality = sleep.CalculateQuality();
            collection.SleepRecords.Add(sleep);
        }

        // Create 7 days of heart rate data
        for (int i = 0; i < 7; i++)
        {
            var date = today.AddDays(-i);
            var hr = new Domain.Models.HeartRateData
            {
                RecordDate = date,
                DeviceId = "Amazfit-12345",
                MinimumBpm = 52,
                MaximumBpm = 140 - i,
                AverageBpm = 75 - i,
                RestingBpm = 58,
                MeasurementCount = 1440,
                StressLevel = 35 + i
            };
            collection.HeartRateRecords.Add(hr);
        }

        // Create 7 days of SpO2 data
        for (int i = 0; i < 7; i++)
        {
            var date = today.AddDays(-i);
            var spo2 = new Domain.Models.SpO2Data
            {
                RecordDate = date,
                DeviceId = "Amazfit-12345",
                MinimumPercentage = 96,
                MaximumPercentage = 99,
                AveragePercentage = 97 + i,
                RestingPercentage = 98,
                MeasurementCount = 50,
                LowSpO2Events = 0
            };
            collection.SpO2Records.Add(spo2);
        }

        // Create 7 days of steps data
        for (int i = 0; i < 7; i++)
        {
            var date = today.AddDays(-i);
            var steps = new Domain.Models.StepsData
            {
                RecordDate = date,
                DeviceId = "Amazfit-12345",
                TotalSteps = 8500 + (i * 1000),
                DistanceKm = 6.5 + (i * 0.5),
                CaloriesBurned = 350 + (i * 50),
                DailyGoal = 10000,
                ActiveMinutes = 45 + (i * 5),
                WalkingMinutes = 40 + (i * 5),
                RunningMinutes = 5
            };
            steps.UpdateGoalAchievement();
            collection.StepsRecords.Add(steps);
        }

        return collection;
    }
}
