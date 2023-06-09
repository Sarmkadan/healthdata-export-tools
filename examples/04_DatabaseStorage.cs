// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using HealthDataExportTools;
using HealthDataExportTools.Data;
using HealthDataExportTools.Domain.Enums;

namespace HealthDataExportTools.Examples;

/// Example 4: SQLite Database Storage
/// Demonstrates how to parse health data and persist it to SQLite database
/// for long-term storage, querying, and analysis.
class DatabaseStorageExample
{
    static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("═══════════════════════════════════════════");
            Console.WriteLine("  Example 4: Database Storage");
            Console.WriteLine("═══════════════════════════════════════════\n");

            const string dbPath = "./health_data.db";

            // Initialize database
            Console.WriteLine("🗄 Initializing SQLite database...");
            var connectionManager = new SqliteConnectionManager(dbPath);
            await connectionManager.InitializeDatabaseAsync();
            Console.WriteLine("✓ Database initialized\n");

            // Parse health data
            Console.WriteLine("📂 Parsing health data...");
            var parser = new HealthDataParserService();
            var healthData = await parser.ParseHealthDataAsync("./exports/export.zip");
            Console.WriteLine($"✓ Parsed {healthData.GetTotalRecordCount()} records\n");

            // Create repository
            var repository = new InMemoryHealthDataRepository();

            // Save data to repository
            Console.WriteLine("💾 Saving data to storage...");
            await repository.SaveHealthDataAsync(healthData);
            Console.WriteLine("✓ Data saved successfully\n");

            // Retrieve and display statistics
            Console.WriteLine("━━━ Stored Data Statistics ━━━\n");

            var storedData = await repository.GetHealthDataAsync();
            if (storedData != null)
            {
                Console.WriteLine($"  Sleep Records:      {storedData.SleepRecords.Count}");
                Console.WriteLine($"  Heart Rate Records: {storedData.HeartRateRecords.Count}");
                Console.WriteLine($"  SpO2 Records:       {storedData.SpO2Records.Count}");
                Console.WriteLine($"  Steps Records:      {storedData.StepsRecords.Count}");
                Console.WriteLine($"  Total:              {storedData.GetTotalRecordCount()}\n");
            }

            // Query data by date range
            Console.WriteLine("━━━ Querying by Date Range ━━━\n");

            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            var today = DateTime.UtcNow;

            Console.WriteLine($"  Query Range: {thirtyDaysAgo:yyyy-MM-dd} to {today:yyyy-MM-dd}\n");

            var sleepRecords = await repository.GetSleepRecordsAsync(thirtyDaysAgo, today);
            var hrRecords = await repository.GetHeartRateRecordsAsync(thirtyDaysAgo, today);

            Console.WriteLine($"  Sleep Records in Range:      {sleepRecords.Count()}");
            Console.WriteLine($"  Heart Rate Records in Range: {hrRecords.Count()}\n");

            // Display sample records
            if (sleepRecords.Any())
            {
                Console.WriteLine("━━━ Sample Sleep Records ━━━\n");
                var samples = sleepRecords.Take(3).ToList();

                for (int i = 0; i < samples.Count; i++)
                {
                    var sleep = samples[i];
                    Console.WriteLine($"  Record {i + 1}:");
                    Console.WriteLine($"    Date:       {sleep.RecordDate:yyyy-MM-dd}");
                    Console.WriteLine($"    Duration:   {sleep.DurationMinutes} minutes");
                    Console.WriteLine($"    Quality:    {sleep.Quality}");
                    Console.WriteLine($"    Score:      {sleep.Score}");
                    Console.WriteLine($"    Deep Sleep: {sleep.DeepSleepMinutes} minutes");
                    Console.WriteLine();
                }
            }

            if (hrRecords.Any())
            {
                Console.WriteLine("━━━ Sample Heart Rate Records ━━━\n");
                var samples = hrRecords.Take(3).ToList();

                for (int i = 0; i < samples.Count; i++)
                {
                    var hr = samples[i];
                    Console.WriteLine($"  Record {i + 1}:");
                    Console.WriteLine($"    Date:      {hr.RecordDate:yyyy-MM-dd}");
                    Console.WriteLine($"    Avg BPM:   {hr.AverageBpm}");
                    Console.WriteLine($"    Min BPM:   {hr.MinimumBpm}");
                    Console.WriteLine($"    Max BPM:   {hr.MaximumBpm}");
                    Console.WriteLine($"    Stress:    {hr.StressLevel}/100");
                    Console.WriteLine();
                }
            }

            // Database file information
            Console.WriteLine("━━━ Database Information ━━━\n");
            if (File.Exists(dbPath))
            {
                var dbInfo = new FileInfo(dbPath);
                Console.WriteLine($"  Location: {Path.GetFullPath(dbPath)}");
                Console.WriteLine($"  Size:     {dbInfo.Length / 1024.0:F2} KB");
                Console.WriteLine($"  Modified: {dbInfo.LastWriteTime:yyyy-MM-dd HH:mm:ss}\n");
            }

            Console.WriteLine("✅ Database storage example completed successfully!");
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
