// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using HealthDataExportTools;
using HealthDataExportTools.Services;

namespace HealthDataExportTools.Examples;

/// Example 5: Data Validation
/// Demonstrates comprehensive data validation to ensure health data
/// quality and integrity before processing or export.
class DataValidationExample
{
    static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("═══════════════════════════════════════════");
            Console.WriteLine("  Example 5: Data Validation");
            Console.WriteLine("═══════════════════════════════════════════\n");

            // Parse health data
            Console.WriteLine("📂 Parsing health data...");
            var parser = new HealthDataParserService();
            var healthData = await parser.ParseHealthDataAsync("./exports/export.zip");
            Console.WriteLine($"✓ Parsed {healthData.GetTotalRecordCount()} records\n");

            // Create validator
            var validator = new ValidationService();

            // Validate all data
            Console.WriteLine("🔍 Validating health data...\n");
            var validationResult = await validator.ValidateAllAsync(healthData);

            // Display validation summary
            Console.WriteLine("━━━ Validation Summary ━━━\n");
            Console.WriteLine($"  Overall Status:  {(validationResult.IsValid ? "✓ Valid" : "✗ Invalid")}");
            Console.WriteLine($"  Records Checked: {validationResult.RecordsValidated}");
            Console.WriteLine($"  Invalid Records: {validationResult.RecordsInvalid}");
            Console.WriteLine($"  Errors Found:    {validationResult.Errors.Count}");
            Console.WriteLine($"  Warnings Found:  {validationResult.Warnings.Count}\n");

            // Display errors
            if (validationResult.Errors.Count > 0)
            {
                Console.WriteLine("━━━ Validation Errors ━━━\n");
                foreach (var error in validationResult.Errors.Take(10))
                {
                    Console.WriteLine($"  ✗ {error}");
                }

                if (validationResult.Errors.Count > 10)
                    Console.WriteLine($"  ... and {validationResult.Errors.Count - 10} more errors");

                Console.WriteLine();
            }

            // Display warnings
            if (validationResult.Warnings.Count > 0)
            {
                Console.WriteLine("━━━ Validation Warnings ━━━\n");
                foreach (var warning in validationResult.Warnings.Take(10))
                {
                    Console.WriteLine($"  ⚠ {warning}");
                }

                if (validationResult.Warnings.Count > 10)
                    Console.WriteLine($"  ... and {validationResult.Warnings.Count - 10} more warnings");

                Console.WriteLine();
            }

            // Detailed per-data-type validation
            Console.WriteLine("━━━ Per-Data-Type Validation ━━━\n");

            // Validate sleep records
            Console.WriteLine("  Sleep Records:");
            if (healthData.SleepRecords.Count > 0)
            {
                var validSleepCount = 0;
                var invalidSleepCount = 0;

                foreach (var record in healthData.SleepRecords)
                {
                    var result = validator.ValidateSleepData(record);
                    if (result.IsValid)
                        validSleepCount++;
                    else
                        invalidSleepCount++;
                }

                Console.WriteLine($"    Valid:   {validSleepCount}");
                Console.WriteLine($"    Invalid: {invalidSleepCount}");
            }
            else
            {
                Console.WriteLine($"    Count: 0 (no data)");
            }

            // Validate heart rate records
            Console.WriteLine("\n  Heart Rate Records:");
            if (healthData.HeartRateRecords.Count > 0)
            {
                var validHrCount = 0;
                var invalidHrCount = 0;

                foreach (var record in healthData.HeartRateRecords)
                {
                    var result = validator.ValidateHeartRateData(record);
                    if (result.IsValid)
                        validHrCount++;
                    else
                        invalidHrCount++;
                }

                Console.WriteLine($"    Valid:   {validHrCount}");
                Console.WriteLine($"    Invalid: {invalidHrCount}");
            }
            else
            {
                Console.WriteLine($"    Count: 0 (no data)");
            }

            // Validate SpO2 records
            Console.WriteLine("\n  SpO2 Records:");
            if (healthData.SpO2Records.Count > 0)
            {
                var validSpo2Count = 0;
                var invalidSpo2Count = 0;

                foreach (var record in healthData.SpO2Records)
                {
                    var result = validator.ValidateSpO2Data(record);
                    if (result.IsValid)
                        validSpo2Count++;
                    else
                        invalidSpo2Count++;
                }

                Console.WriteLine($"    Valid:   {validSpo2Count}");
                Console.WriteLine($"    Invalid: {invalidSpo2Count}");
            }
            else
            {
                Console.WriteLine($"    Count: 0 (no data)");
            }

            // Validate steps records
            Console.WriteLine("\n  Steps Records:");
            if (healthData.StepsRecords.Count > 0)
            {
                var validStepsCount = 0;
                var invalidStepsCount = 0;

                foreach (var record in healthData.StepsRecords)
                {
                    var result = validator.ValidateStepsData(record);
                    if (result.IsValid)
                        validStepsCount++;
                    else
                        invalidStepsCount++;
                }

                Console.WriteLine($"    Valid:   {validStepsCount}");
                Console.WriteLine($"    Invalid: {invalidStepsCount}");
            }
            else
            {
                Console.WriteLine($"    Count: 0 (no data)");
            }

            Console.WriteLine();

            // Recommendation
            Console.WriteLine("━━━ Validation Recommendation ━━━\n");
            if (validationResult.IsValid)
            {
                Console.WriteLine("  ✓ All data is valid and ready for processing/export.");
            }
            else
            {
                Console.WriteLine("  ⚠ Data contains errors. Review the errors above before processing.");
                Console.WriteLine("  You can:");
                Console.WriteLine("    1. Fix the source data and re-export");
                Console.WriteLine("    2. Manually correct individual records");
                Console.WriteLine("    3. Use ValidateData: false to skip validation (not recommended)");
            }

            Console.WriteLine("\n✅ Validation example completed!");
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
