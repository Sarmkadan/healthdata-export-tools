// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace HealthDataExportTools.Utilities;

/// <summary>
/// Utility for CSV operations beyond basic formatting
/// Handles parsing, validation, and batch operations
/// </summary>
public static class CsvUtility
{
    /// <summary>
    /// Parse CSV file into records
    /// </summary>
    public static async Task<List<Dictionary<string, string>>> ParseCsvFileAsync(string filePath)
    {
        var records = new List<Dictionary<string, string>>();

        try
        {
            var lines = await FileUtility.ReadLinesAsync(filePath);

            if (lines.Count == 0)
                return records;

            var headers = ParseCsvLine(lines[0]);

            for (int i = 1; i < lines.Count; i++)
            {
                var values = ParseCsvLine(lines[i]);

                if (values.Count != headers.Count)
                    continue; // Skip malformed lines

                var record = new Dictionary<string, string>();

                for (int j = 0; j < headers.Count; j++)
                {
                    record[headers[j]] = values[j];
                }

                records.Add(record);
            }

            return records;
        }
        catch (Exception ex)
        {
            throw new HealthDataException($"Failed to parse CSV file: {filePath}", ex);
        }
    }

    /// <summary>
    /// Parse a single CSV line respecting quotes
    /// </summary>
    public static List<string> ParseCsvLine(string line)
    {
        var fields = new List<string>();
        var currentField = new StringBuilder();
        var inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            var c = line[i];

            if (c == '"')
            {
                // Check for escaped quote
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    currentField.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                fields.Add(currentField.ToString());
                currentField.Clear();
            }
            else
            {
                currentField.Append(c);
            }
        }

        fields.Add(currentField.ToString());

        return fields.Select(f => f.Trim()).ToList();
    }

    /// <summary>
    /// Escape CSV field value
    /// </summary>
    public static string EscapeCsvField(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }

    /// <summary>
    /// Validate CSV structure
    /// </summary>
    public static List<string> ValidateCsvStructure(string filePath, string[] expectedHeaders)
    {
        var errors = new List<string>();

        try
        {
            using (var reader = new StreamReader(filePath))
            {
                var firstLine = reader.ReadLine();

                if (string.IsNullOrEmpty(firstLine))
                {
                    errors.Add("CSV file is empty");
                    return errors;
                }

                var headers = ParseCsvLine(firstLine);

                if (headers.Count != expectedHeaders.Length)
                {
                    errors.Add($"Expected {expectedHeaders.Length} columns, found {headers.Count}");
                }

                for (int i = 0; i < Math.Min(headers.Count, expectedHeaders.Length); i++)
                {
                    if (!headers[i].Equals(expectedHeaders[i], StringComparison.OrdinalIgnoreCase))
                    {
                        errors.Add($"Column {i}: Expected '{expectedHeaders[i]}', found '{headers[i]}'");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            errors.Add($"Error validating CSV: {ex.Message}");
        }

        return errors;
    }

    /// <summary>
    /// Count rows in CSV file (excluding header)
    /// </summary>
    public static async Task<int> CountRowsAsync(string filePath)
    {
        try
        {
            var lines = await FileUtility.ReadLinesAsync(filePath);
            return Math.Max(0, lines.Count - 1); // Subtract header
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// Merge multiple CSV files
    /// </summary>
    public static async Task<string> MergeCsvFilesAsync(
        List<string> filePaths,
        string outputPath,
        bool includeSourceColumn = false)
    {
        try
        {
            var allRecords = new List<Dictionary<string, string>>();
            var headers = new List<string>();

            foreach (var filePath in filePaths)
            {
                var records = await ParseCsvFileAsync(filePath);

                if (headers.Count == 0 && records.Count > 0)
                {
                    headers.AddRange(records[0].Keys);

                    if (includeSourceColumn && !headers.Contains("Source"))
                    {
                        headers.Add("Source");
                    }
                }

                // Add source column if requested
                if (includeSourceColumn)
                {
                    foreach (var record in records)
                    {
                        record["Source"] = Path.GetFileName(filePath);
                    }
                }

                allRecords.AddRange(records);
            }

            // Write merged file
            var lines = new List<string>();
            lines.Add(string.Join(",", headers.Select(EscapeCsvField)));

            foreach (var record in allRecords)
            {
                var values = headers.Select(h => record.TryGetValue(h, out var v) ? v : "");
                lines.Add(string.Join(",", values.Select(EscapeCsvField)));
            }

            await FileUtility.WriteLinesAsync(outputPath, lines);

            return outputPath;
        }
        catch (Exception ex)
        {
            throw new HealthDataException("Failed to merge CSV files", ex);
        }
    }

    /// <summary>
    /// Convert CSV to JSON
    /// </summary>
    public static async Task<string> CsvToJsonAsync(string csvFilePath)
    {
        try
        {
            var records = await ParseCsvFileAsync(csvFilePath);
            return JsonSerializer.Serialize(records, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            throw new HealthDataException("Failed to convert CSV to JSON", ex);
        }
    }

    /// <summary>
    /// Get CSV statistics
    /// </summary>
    public static async Task<CsvStatistics> GetCsvStatisticsAsync(string filePath)
    {
        try
        {
            var lines = await FileUtility.ReadLinesAsync(filePath);
            var fileInfo = new FileInfo(filePath);

            return new CsvStatistics
            {
                RowCount = Math.Max(0, lines.Count - 1),
                HeaderCount = lines.Count > 0 ? ParseCsvLine(lines[0]).Count : 0,
                FileSizeBytes = fileInfo.Length,
                LastModified = fileInfo.LastWriteTimeUtc
            };
        }
        catch (Exception ex)
        {
            throw new HealthDataException($"Failed to get CSV statistics: {filePath}", ex);
        }
    }
}

/// <summary>
/// CSV file statistics
/// </summary>
public class CsvStatistics
{
    public int RowCount { get; set; }
    public int HeaderCount { get; set; }
    public long FileSizeBytes { get; set; }
    public DateTime LastModified { get; set; }
}
