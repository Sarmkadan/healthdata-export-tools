// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using HealthDataExportTools;
using HealthDataExportTools.Configuration;
using HealthDataExportTools.Domain.Enums;

namespace HealthDataExportTools.Examples;

/// Example 1: Basic Health Data Export
/// Demonstrates the simplest way to parse and export health data to JSON format.
/// This is the recommended starting point for new users.
class BasicExportExample
{
    static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("═══════════════════════════════════════════");
            Console.WriteLine("  Example 1: Basic Health Data Export");
            Console.WriteLine("═══════════════════════════════════════════\n");

            // Configure export options
            var options = new HealthDataExportOptions
            {
                InputPath = "./exports/",
                OutputPath = "./output/",
                ExportFormat = ExportFormat.Json,
                ValidateData = true,
                PerformAnalysis = true,
                VerboseLogging = true
            };

            Console.WriteLine("📋 Configuration:");
            Console.WriteLine($"   • Input Directory:  {options.InputPath}");
            Console.WriteLine($"   • Output Directory: {options.OutputPath}");
            Console.WriteLine($"   • Export Format:    {options.ExportFormat}\n");

            // Validate configuration
            var validationErrors = options.Validate();
            if (validationErrors.Count > 0)
            {
                Console.WriteLine("❌ Configuration errors:");
                foreach (var error in validationErrors)
                    Console.WriteLine($"   • {error}");
                return;
            }

            // Create parser instance
            var parser = new HealthDataParserService();

            // Parse health data from ZIP file
            Console.WriteLine("📂 Parsing health data from export.zip...");
            var healthData = await parser.ParseHealthDataAsync(Path.Combine(options.InputPath, "export.zip"));

            Console.WriteLine($"✓ Successfully parsed health data:");
            Console.WriteLine($"   • Sleep records:      {healthData.SleepRecords.Count}");
            Console.WriteLine($"   • Heart rate records: {healthData.HeartRateRecords.Count}");
            Console.WriteLine($"   • SpO2 records:       {healthData.SpO2Records.Count}");
            Console.WriteLine($"   • Steps records:      {healthData.StepsRecords.Count}\n");

            // Create exporter instance
            var exporter = new ExportService();

            // Ensure output directory exists
            Directory.CreateDirectory(options.OutputPath);

            // Export data to JSON
            Console.WriteLine("💾 Exporting data to JSON format...");
            await exporter.ExportCompleteAsync(
                healthData,
                options.OutputPath,
                ExportFormat.Json
            );

            Console.WriteLine("✓ Export completed successfully!\n");

            // Display output file information
            var outputFile = Path.Combine(options.OutputPath, "health_data.json");
            if (File.Exists(outputFile))
            {
                var fileInfo = new FileInfo(outputFile);
                Console.WriteLine($"📁 Output File:");
                Console.WriteLine($"   • Path: {Path.GetFullPath(outputFile)}");
                Console.WriteLine($"   • Size: {fileInfo.Length / 1024.0:F2} KB\n");
            }

            Console.WriteLine("✅ Example completed successfully!");
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
