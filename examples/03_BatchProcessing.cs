// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using HealthDataExportTools;
using HealthDataExportTools.Domain.Enums;
using HealthDataExportTools.Services;

namespace HealthDataExportTools.Examples;

/// Example 3: Batch Processing Multiple Files
/// Demonstrates how to process multiple health data files in parallel
/// for improved throughput and efficiency.
class BatchProcessingExample
{
    static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("═══════════════════════════════════════════");
            Console.WriteLine("  Example 3: Batch Processing");
            Console.WriteLine("═══════════════════════════════════════════\n");

            // Create sample export files
            Console.WriteLine("📁 Preparing test data...");
            var exportsDir = "./exports";
            Directory.CreateDirectory(exportsDir);

            // List existing export files
            var exportFiles = Directory.GetFiles(exportsDir, "*.zip");
            Console.WriteLine($"Found {exportFiles.Length} export files:\n");

            foreach (var file in exportFiles)
            {
                var fileInfo = new FileInfo(file);
                Console.WriteLine($"  • {Path.GetFileName(file)} ({fileInfo.Length / 1024.0:F2} KB)");
            }
            Console.WriteLine();

            if (exportFiles.Length == 0)
            {
                Console.WriteLine("⚠ No export files found in ./exports/");
                Console.WriteLine("Please add .zip export files and try again.\n");
                return;
            }

            // Create batch processor
            var batchProcessor = new BatchProcessingService();

            // Process all files
            Console.WriteLine("🔄 Processing files in batch...\n");
            var startTime = DateTime.UtcNow;

            var results = await batchProcessor.ProcessDirectoryAsync(
                exportsDir,
                ExportFormat.All
            );

            var processingDuration = DateTime.UtcNow - startTime;

            // Display results
            Console.WriteLine("\n━━━ Batch Processing Results ━━━\n");

            var successCount = 0;
            var failureCount = 0;
            var totalRecords = 0;

            foreach (var result in results)
            {
                Console.WriteLine($"  📄 {result.FileName}");

                if (result.Success)
                {
                    successCount++;
                    Console.WriteLine($"     Status:      ✓ Successful");
                    Console.WriteLine($"     Records:     {result.RecordsProcessed:N0}");
                    Console.WriteLine($"     Duration:    {result.DurationMs} ms");
                    totalRecords += result.RecordsProcessed;
                }
                else
                {
                    failureCount++;
                    Console.WriteLine($"     Status:      ✗ Failed");
                    Console.WriteLine($"     Error:       {result.ErrorMessage}");
                }
                Console.WriteLine();
            }

            // Summary statistics
            Console.WriteLine("━━━ Summary ━━━");
            Console.WriteLine($"  Total Files:       {exportFiles.Length}");
            Console.WriteLine($"  Successful:        {successCount}");
            Console.WriteLine($"  Failed:            {failureCount}");
            Console.WriteLine($"  Total Records:     {totalRecords:N0}");
            Console.WriteLine($"  Total Duration:    {processingDuration.TotalSeconds:F2}s");
            Console.WriteLine($"  Throughput:        {totalRecords / processingDuration.TotalSeconds:F0} records/sec\n");

            // Check output
            var outputDir = "./output";
            if (Directory.Exists(outputDir))
            {
                var outputFiles = Directory.GetFiles(outputDir);
                Console.WriteLine($"📦 Output files generated: {outputFiles.Length}\n");

                foreach (var file in outputFiles.Take(5))
                {
                    var fileInfo = new FileInfo(file);
                    Console.WriteLine($"  • {Path.GetFileName(file)} ({fileInfo.Length / 1024.0:F2} KB)");
                }

                if (outputFiles.Length > 5)
                    Console.WriteLine($"  ... and {outputFiles.Length - 5} more files");
            }

            Console.WriteLine("\n✅ Batch processing completed successfully!");
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
