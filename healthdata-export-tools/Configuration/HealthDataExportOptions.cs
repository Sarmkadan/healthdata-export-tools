// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using HealthDataExportTools.Domain.Enums;

namespace HealthDataExportTools.Configuration;

/// <summary>
/// Configuration options for health data export
/// </summary>
public class HealthDataExportOptions
{
    /// <summary>
    /// Input directory path for health data files
    /// </summary>
    public string InputPath { get; set; } = string.Empty;

    /// <summary>
    /// Output directory path for exported files
    /// </summary>
    public string OutputPath { get; set; } = string.Empty;

    /// <summary>
    /// Database file path for SQLite storage
    /// </summary>
    public string DatabasePath { get; set; } = "healthdata.db";

    /// <summary>
    /// Export format(s) to use
    /// </summary>
    public ExportFormat ExportFormat { get; set; } = ExportFormat.Json;

    /// <summary>
    /// Enable automatic validation of parsed data
    /// </summary>
    public bool ValidateData { get; set; } = true;

    /// <summary>
    /// Enable automatic analysis after parsing
    /// </summary>
    public bool PerformAnalysis { get; set; } = true;

    /// <summary>
    /// Number of days to use for trend analysis
    /// </summary>
    public int TrendAnalysisDays { get; set; } = 7;

    /// <summary>
    /// Maximum age of records to keep in days
    /// </summary>
    public int MaxRecordAgeDays { get; set; } = 365;

    /// <summary>
    /// Enable compression of export files
    /// </summary>
    public bool CompressOutput { get; set; } = false;

    /// <summary>
    /// Device type to filter for (null = all devices)
    /// </summary>
    public string? TargetDeviceType { get; set; }

    /// <summary>
    /// Device ID to filter for (null = all devices)
    /// </summary>
    public string? TargetDeviceId { get; set; }

    /// <summary>
    /// Start date for data range (null = all data)
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// End date for data range (null = all data)
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Email address for result notifications
    /// </summary>
    public string? NotificationEmail { get; set; }

    /// <summary>
    /// Enable detailed logging
    /// </summary>
    public bool VerboseLogging { get; set; } = false;

    /// <summary>
    /// Validate the configuration options
    /// </summary>
    public List<string> Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(InputPath) && string.IsNullOrWhiteSpace(DatabasePath))
            errors.Add("Either InputPath or DatabasePath must be specified");

        if (!string.IsNullOrWhiteSpace(InputPath) && !Directory.Exists(InputPath))
            errors.Add($"InputPath '{InputPath}' does not exist");

        if (string.IsNullOrWhiteSpace(OutputPath))
            errors.Add("OutputPath must be specified");

        if (!string.IsNullOrWhiteSpace(DatabasePath) && !string.IsNullOrWhiteSpace(InputPath))
        {
            var dbDir = Path.GetDirectoryName(DatabasePath);
            if (!string.IsNullOrEmpty(dbDir) && !Directory.Exists(dbDir))
                errors.Add($"Database directory '{dbDir}' does not exist");
        }

        if (MaxRecordAgeDays < 0)
            errors.Add("MaxRecordAgeDays must be non-negative");

        if (TrendAnalysisDays <= 0)
            errors.Add("TrendAnalysisDays must be positive");

        if (StartDate.HasValue && EndDate.HasValue && StartDate > EndDate)
            errors.Add("StartDate cannot be after EndDate");

        if (!string.IsNullOrWhiteSpace(NotificationEmail) && !IsValidEmail(NotificationEmail))
            errors.Add($"NotificationEmail '{NotificationEmail}' is not valid");

        return errors;
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Get validation errors with descriptions
    /// </summary>
    public string GetValidationErrors()
    {
        var errors = Validate();
        if (errors.Count == 0) return "Configuration is valid";
        return "Configuration validation failed:\n" + string.Join("\n", errors.Select(e => $"  • {e}"));
    }

    /// <summary>
    /// Check if configuration is valid
    /// </summary>
    public bool IsValid() => Validate().Count == 0;
}
