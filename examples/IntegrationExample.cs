#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using Microsoft.Extensions.DependencyInjection;
using HealthDataExportTools;
using HealthDataExportTools.Configuration;
using HealthDataExportTools.Domain.Enums;
using HealthDataExportTools.Services;

namespace HealthDataExportTools.Examples;

/// Example: Integration with ASP.NET Dependency Injection
/// Demonstrates production-grade integration using Microsoft.Extensions.DependencyInjection.
/// Shows how to wire the library into ASP.NET Core applications.
class IntegrationExample
{
    static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("═══════════════════════════════════════════");
            Console.WriteLine(" ASP.NET Dependency Injection Integration");
            Console.WriteLine("═══════════════════════════════════════════\n");

            // Configure services (typically done in Startup.cs or Program.cs)
            Console.WriteLine("🔧 Configuring services...");
            var services = new ServiceCollection();

            services.AddHealthDataExportTools(options =>
            {
                options.InputPath = "./exports/";
                options.OutputPath = "./output/";
                options.DatabasePath = "./healthdata.db";
                options.ExportFormat = ExportFormat.All;
                options.ValidateData = true;
                options.PerformAnalysis = true;
                options.CacheEnabled = true;
                options.CacheDurationSeconds = 3600;
            });

            // Build service provider
            var serviceProvider = services.BuildServiceProvider();
            Console.WriteLine("✓ Services configured\n");

            // Resolve services from DI container
            Console.WriteLine("📦 Resolving services...");
            var parser = serviceProvider.GetRequiredService<HealthDataParserService>();
            var exporter = serviceProvider.GetRequiredService<ExportService>();
            var analytics = serviceProvider.GetRequiredService<AnalyticsService>();
            var validator = serviceProvider.GetRequiredService<ValidationService>();
            var cache = serviceProvider.GetRequiredService<CacheService>();
            var batchProcessor = serviceProvider.GetRequiredService<BatchProcessingService>();
            Console.WriteLine("✓ All services resolved\n");

            // Use services
            Console.WriteLine("📂 Parsing health data...");
            var healthData = await parser.ParseHealthDataAsync("./exports/export.zip").ConfigureAwait(false);
            Console.WriteLine($"✓ Parsed {healthData.GetTotalRecordCount()} records\n");

            // Validate using injected service
            Console.WriteLine("🔍 Validating data...");
            var validation = await validator.ValidateAllAsync(healthData).ConfigureAwait(false);
            Console.WriteLine($"✓ Validation: {(validation.IsValid ? "VALID" : "INVALID")}\n");

            // Analyze using injected service
            Console.WriteLine("📊 Analyzing data...");
            var healthScore = analytics.CalculateHealthScore(healthData);
            Console.WriteLine($"✓ Health Score: {healthScore}/100\n");

            // Cache using injected service
            Console.WriteLine("💾 Caching data...");
            await cache.SetAsync("health_data_current", healthData, TimeSpan.FromHours(1)).ConfigureAwait(false);
            Console.WriteLine("✓ Data cached\n");

            // Export using injected service
            Console.WriteLine("💾 Exporting data...");
            Directory.CreateDirectory("./output");
            await exporter.ExportCompleteAsync(healthData, "./output/", ExportFormat.Json).ConfigureAwait(false);
            Console.WriteLine("✓ Export completed\n");

            // Display DI container information
            Console.WriteLine("━━━ Registered Services ━━━\n");
            Console.WriteLine(" HealthDataParserService");
            Console.WriteLine(" ExportService");
            Console.WriteLine(" AnalyticsService");
            Console.WriteLine(" ValidationService");
            Console.WriteLine(" CacheService");
            Console.WriteLine(" BatchProcessingService");
            Console.WriteLine(" NotificationService");
            Console.WriteLine(" IHealthDataRepository\n");

            Console.WriteLine("━━━ Best Practices ━━━\n");
            Console.WriteLine(" 1. Register services in Startup/Program configuration");
            Console.WriteLine(" 2. Use GetRequiredService for mandatory dependencies");
            Console.WriteLine(" 3. Use GetService for optional dependencies");
            Console.WriteLine(" 4. Keep services loosely coupled");
            Console.WriteLine(" 5. Use scoped lifetime for web applications\n");

            Console.WriteLine("✅ Integration example completed successfully!");
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