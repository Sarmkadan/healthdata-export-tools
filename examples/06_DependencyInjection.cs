// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using HealthDataExportTools;
using HealthDataExportTools.Configuration;
using HealthDataExportTools.Domain.Enums;
using HealthDataExportTools.Services;

namespace HealthDataExportTools.Examples;

/// Example 6: Dependency Injection Configuration
/// Demonstrates proper DI setup for production applications using
/// Microsoft.Extensions.DependencyInjection for IoC container management.
class DependencyInjectionExample
{
    static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("═══════════════════════════════════════════");
            Console.WriteLine("  Example 6: Dependency Injection");
            Console.WriteLine("═══════════════════════════════════════════\n");

            // Build service collection
            Console.WriteLine("🔧 Configuring services...");
            var services = new ServiceCollection();

            // Register health data export tools with options
            services.AddHealthDataExportTools(options =>
            {
                options.InputPath = "./exports/";
                options.OutputPath = "./output/";
                options.DatabasePath = "./healthdata.db";
                options.ExportFormat = ExportFormat.All;
                options.ValidateData = true;
                options.PerformAnalysis = true;
                options.VerboseLogging = false;
                options.CacheEnabled = true;
                options.CacheDurationSeconds = 3600;
            });

            // Build service provider
            var serviceProvider = services.BuildServiceProvider();
            Console.WriteLine("✓ Services configured\n");

            // Get services from DI container
            Console.WriteLine("📦 Resolving services from DI container...");
            var parser = serviceProvider.GetRequiredService<HealthDataParserService>();
            var exporter = serviceProvider.GetRequiredService<ExportService>();
            var analytics = serviceProvider.GetRequiredService<AnalyticsService>();
            var validator = serviceProvider.GetRequiredService<ValidationService>();
            var cacheService = serviceProvider.GetRequiredService<CacheService>();
            Console.WriteLine("✓ All services resolved successfully\n");

            // Parse health data
            Console.WriteLine("📂 Parsing health data using injected parser...");
            var healthData = await parser.ParseHealthDataAsync("./exports/export.zip");
            Console.WriteLine($"✓ Parsed {healthData.GetTotalRecordCount()} records\n");

            // Validate using injected validator
            Console.WriteLine("🔍 Validating data using injected validator...");
            var validation = await validator.ValidateAllAsync(healthData);
            Console.WriteLine($"✓ Validation complete: {(validation.IsValid ? "VALID" : "INVALID")}\n");

            // Analyze using injected analytics
            Console.WriteLine("📊 Analyzing data using injected analytics service...");
            var healthScore = analytics.CalculateHealthScore(healthData);
            Console.WriteLine($"✓ Health Score: {healthScore}/100\n");

            // Cache data using injected cache service
            Console.WriteLine("💾 Caching data using injected cache service...");
            await cacheService.SetAsync("current_health_data", healthData, TimeSpan.FromHours(1));
            Console.WriteLine("✓ Data cached for 1 hour\n");

            // Retrieve from cache
            Console.WriteLine("📥 Retrieving from cache...");
            var cachedData = await cacheService.GetAsync<HealthDataExportDto>("current_health_data");
            if (cachedData != null)
            {
                Console.WriteLine($"✓ Retrieved {cachedData.GetTotalRecordCount()} records from cache\n");
            }

            // Export using injected exporter
            Console.WriteLine("💾 Exporting data using injected exporter...");
            Directory.CreateDirectory("./output");
            await exporter.ExportCompleteAsync(healthData, "./output/", ExportFormat.Json);
            Console.WriteLine("✓ Export completed\n");

            // Display DI container status
            Console.WriteLine("━━━ Dependency Injection Container Status ━━━\n");
            Console.WriteLine("  Registered Services:");
            Console.WriteLine("    ✓ HealthDataParserService");
            Console.WriteLine("    ✓ ExportService");
            Console.WriteLine("    ✓ AnalyticsService");
            Console.WriteLine("    ✓ ValidationService");
            Console.WriteLine("    ✓ CacheService");
            Console.WriteLine("    ✓ BatchProcessingService");
            Console.WriteLine("    ✓ NotificationService");
            Console.WriteLine("    ✓ IHealthDataRepository (InMemory)");
            Console.WriteLine("    ✓ SqliteConnectionManager\n");

            // Best practices
            Console.WriteLine("━━━ Dependency Injection Best Practices ━━━\n");
            Console.WriteLine("  1. Register services in Startup/Configuration");
            Console.WriteLine("  2. Use GetRequiredService for required dependencies");
            Console.WriteLine("  3. Use GetService for optional dependencies");
            Console.WriteLine("  4. Keep service lifetime appropriate:");
            Console.WriteLine("     - Singleton: Stateless, thread-safe services");
            Console.WriteLine("     - Transient: New instance each time");
            Console.WriteLine("     - Scoped: One instance per request/scope\n");

            Console.WriteLine("✅ DI example completed successfully!");
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
