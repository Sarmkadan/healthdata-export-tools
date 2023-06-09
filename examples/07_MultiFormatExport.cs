// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using HealthDataExportTools;
using HealthDataExportTools.Configuration;
using HealthDataExportTools.Domain.Enums;
using HealthDataExportTools.Services;

namespace HealthDataExportTools.Examples;

/// Example 7: Multi-Format Export with Filtering
/// Demonstrates how to export health data to multiple formats (CSV, JSON, XML)
/// with date range filtering and selective data type export.
class MultiFormatExportExample
{
    static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("═══════════════════════════════════════════");
            Console.WriteLine("  Example 7: Multi-Format Export");
            Console.WriteLine("═══════════════════════════════════════════\n");

            // Parse health data
            Console.WriteLine("📂 Parsing health data...");
            var parser = new HealthDataParserService();
            var healthData = await parser.ParseHealthDataAsync("./exports/export.zip");
            Console.WriteLine($"✓ Parsed {healthData.GetTotalRecordCount()} records\n");

            // Define date range for filtering
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            var today = DateTime.UtcNow;

            Console.WriteLine($"📅 Filtering data for last 30 days: {thirtyDaysAgo:yyyy-MM-dd} to {today:yyyy-MM-dd}\n");

            // Filter data by date range
            var filteredData = new HealthDataExportDto
            {
                SleepRecords = healthData.SleepRecords
                    .Where(s => s.RecordDate >= thirtyDaysAgo && s.RecordDate <= today)
                    .ToList(),
                HeartRateRecords = healthData.HeartRateRecords
                    .Where(h => h.RecordDate >= thirtyDaysAgo && h.RecordDate <= today)
                    .ToList(),
                SpO2Records = healthData.SpO2Records
                    .Where(s => s.RecordDate >= thirtyDaysAgo && s.RecordDate <= today)
                    .ToList(),
                StepsRecords = healthData.StepsRecords
                    .Where(s => s.RecordDate >= thirtyDaysAgo && s.RecordDate <= today)
                    .ToList()
            };

            Console.WriteLine($"✓ Filtered to {filteredData.GetTotalRecordCount()} records:\n");
            Console.WriteLine($"  • Sleep Records:      {filteredData.SleepRecords.Count}");
            Console.WriteLine($"  • Heart Rate Records: {filteredData.HeartRateRecords.Count}");
            Console.WriteLine($"  • SpO2 Records:       {filteredData.SpO2Records.Count}");
            Console.WriteLine($"  • Steps Records:      {filteredData.StepsRecords.Count}\n");

            // Create output directory
            var outputDir = Path.Combine("./output", DateTime.UtcNow.ToString("yyyy-MM-dd"));
            Directory.CreateDirectory(outputDir);

            var exporter = new ExportService();

            // Export to CSV format
            Console.WriteLine("━━━ Exporting to CSV ━━━\n");
            if (filteredData.SleepRecords.Count > 0)
            {
                Console.WriteLine("  Exporting sleep data...");
                await exporter.ExportSleepToCsvAsync(
                    filteredData.SleepRecords,
                    Path.Combine(outputDir, "sleep_last_30_days.csv")
                );
                Console.WriteLine("  ✓ sleep_last_30_days.csv");
            }

            if (filteredData.HeartRateRecords.Count > 0)
            {
                Console.WriteLine("  Exporting heart rate data...");
                await exporter.ExportHeartRateToCsvAsync(
                    filteredData.HeartRateRecords,
                    Path.Combine(outputDir, "heart_rate_last_30_days.csv")
                );
                Console.WriteLine("  ✓ heart_rate_last_30_days.csv");
            }

            if (filteredData.StepsRecords.Count > 0)
            {
                Console.WriteLine("  Exporting steps data...");
                await exporter.ExportStepsToCsvAsync(
                    filteredData.StepsRecords,
                    Path.Combine(outputDir, "steps_last_30_days.csv")
                );
                Console.WriteLine("  ✓ steps_last_30_days.csv");
            }

            if (filteredData.SpO2Records.Count > 0)
            {
                Console.WriteLine("  Exporting SpO2 data...");
                await exporter.ExportSpO2ToCsvAsync(
                    filteredData.SpO2Records,
                    Path.Combine(outputDir, "spo2_last_30_days.csv")
                );
                Console.WriteLine("  ✓ spo2_last_30_days.csv");
            }

            Console.WriteLine();

            // Export to JSON format
            Console.WriteLine("━━━ Exporting to JSON ━━━\n");
            Console.WriteLine("  Exporting all data...");
            await exporter.ExportToJsonAsync(
                filteredData,
                Path.Combine(outputDir, "health_data_last_30_days.json")
            );
            Console.WriteLine("  ✓ health_data_last_30_days.json\n");

            // Export all formats at once
            Console.WriteLine("━━━ Export Summary ━━━\n");

            var csvFiles = Directory.GetFiles(outputDir, "*.csv");
            var jsonFiles = Directory.GetFiles(outputDir, "*.json");

            Console.WriteLine($"  CSV Files Generated:  {csvFiles.Length}");
            foreach (var file in csvFiles)
            {
                var fileInfo = new FileInfo(file);
                Console.WriteLine($"    • {Path.GetFileName(file)} ({fileInfo.Length / 1024.0:F2} KB)");
            }

            Console.WriteLine($"\n  JSON Files Generated: {jsonFiles.Length}");
            foreach (var file in jsonFiles)
            {
                var fileInfo = new FileInfo(file);
                Console.WriteLine($"    • {Path.GetFileName(file)} ({fileInfo.Length / 1024.0:F2} KB)");
            }

            // Calculate totals
            var totalSize = csvFiles.Concat(jsonFiles)
                .Sum(f => new FileInfo(f).Length) / 1024.0;

            Console.WriteLine($"\n  Total Output Size:    {totalSize:F2} KB");
            Console.WriteLine($"  Output Location:      {Path.GetFullPath(outputDir)}\n");

            // Provide usage recommendations
            Console.WriteLine("━━━ Usage Recommendations ━━━\n");
            Console.WriteLine("  CSV Files:");
            Console.WriteLine("    • Import into Excel/Google Sheets for analysis");
            Console.WriteLine("    • Load into business intelligence tools");
            Console.WriteLine("    • Process with data analysis scripts\n");

            Console.WriteLine("  JSON Files:");
            Console.WriteLine("    • Consume via REST APIs");
            Console.WriteLine("    • Process with Node.js/Python scripts");
            Console.WriteLine("    • Store in NoSQL databases\n");

            Console.WriteLine("✅ Multi-format export completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"   Details: {ex.InnerException.Message}");
            Environment.Exit(1);
        }
    }
}
