#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using HealthDataExportTools;
using HealthDataExportTools.Configuration;
using HealthDataExportTools.Domain.Enums;
using HealthDataExportTools.Services;

namespace HealthDataExportTools.Examples;

/// Example: Advanced Usage - Configuration and Error Handling
/// Demonstrates advanced configuration options, custom settings, and robust error handling.
class AdvancedUsage
{
    static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("═══════════════════════════════════════════");
            Console.WriteLine(" Advanced Usage Example");
            Console.WriteLine("═══════════════════════════════════════════\n");

            // Advanced configuration with all options
            var options = new HealthDataExportOptions
            {
                InputPath = "./exports/",
                OutputPath = "./output/",
                DatabasePath = "./healthdata.db",
                ExportFormat = ExportFormat.All,
                ValidateData = true,
                PerformAnalysis = true,
                VerboseLogging = true,
                CacheEnabled = true,
                CacheDurationSeconds = 3600,
                MaxRetries = 3,
                TimeoutSeconds = 300
            };

            Console.WriteLine("📋 Configuration:");
            Console.WriteLine($" • Input: {options.InputPath}");
            Console.WriteLine($" • Output: {options.OutputPath}");
            Console.WriteLine($" • Validate: {options.ValidateData}");
            Console.WriteLine($" • Analysis: {options.PerformAnalysis}");
            Console.WriteLine($" • Cache: {options.CacheEnabled}");
            Console.WriteLine();

            // Validate configuration
            var validationErrors = options.Validate();
            if (validationErrors.Count > 0)
            {
                Console.WriteLine("⚠ Configuration errors:");
                foreach (var error in validationErrors)
                    Console.WriteLine($" • {error}");
                return;
            }

            // Create services with configuration
            var parser = new HealthDataParserService();
            var exporter = new ExportService();
            var analytics = new AnalyticsService();
            var validator = new ValidationService();
            var cache = new CacheService(new InMemoryCacheProvider());

            Console.WriteLine("📂 Parsing health data...");
            var healthData = await parser.ParseHealthDataAsync(
                Path.Combine(options.InputPath, "export.zip"))
                .ConfigureAwait(false);

            Console.WriteLine($"✓ Parsed {healthData.GetTotalRecordCount()} records");

            // Validate data
            Console.WriteLine("🔍 Validating data...");
            var validation = await validator.ValidateAllAsync(healthData).ConfigureAwait(false);
            if (!validation.IsValid)
            {
                Console.WriteLine("⚠ Validation warnings:");
                foreach (var warning in validation.Warnings)
                    Console.WriteLine($" • {warning}");
            }

            // Perform analysis
            Console.WriteLine("📊 Analyzing data...");
            var healthScore = analytics.CalculateHealthScore(healthData);
            Console.WriteLine($"✓ Health Score: {healthScore}/100");

            // Cache results
            Console.WriteLine("💾 Caching data...");
            await cache.SetAsync("health_data", healthData, TimeSpan.FromHours(1)).ConfigureAwait(false);

            // Export in multiple formats
            Console.WriteLine("💾 Exporting data...");
            Directory.CreateDirectory(options.OutputPath);
            await exporter.ExportCompleteAsync(healthData, options.OutputPath, options.ExportFormat).ConfigureAwait(false);

            Console.WriteLine("✅ Advanced usage completed successfully!\n");
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