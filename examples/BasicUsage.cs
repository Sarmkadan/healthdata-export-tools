#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using HealthDataExportTools;
using HealthDataExportTools.Configuration;
using HealthDataExportTools.Domain.Enums;

namespace HealthDataExportTools.Examples;

/// Example: Basic Usage - Minimal Setup
/// Demonstrates the simplest possible integration with Health Data Export Tools.
/// This is the recommended starting point for new users.
class BasicUsage
{
    static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("═══════════════════════════════════════════");
            Console.WriteLine(" Basic Usage Example");
            Console.WriteLine("═══════════════════════════════════════════\n");

            // Minimal setup - just parse and export
            var parser = new HealthDataParserService();
            var exporter = new ExportService();

            Console.WriteLine("📂 Parsing health data...");
            var healthData = await parser.ParseHealthDataAsync("export.zip").ConfigureAwait(false);

            Console.WriteLine($"✓ Parsed {healthData.GetTotalRecordCount()} records");
            Console.WriteLine("💾 Exporting to JSON...");
            await exporter.ExportCompleteAsync(healthData, "./output/", ExportFormat.Json).ConfigureAwait(false);

            Console.WriteLine("✅ Success!\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($" Details: {ex.InnerException.Message}");
            Environment.Exit(1);
        }
    }
}