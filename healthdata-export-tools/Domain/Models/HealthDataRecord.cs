// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace HealthDataExportTools.Domain.Models;

/// <summary>
/// Base class for all health data records with common properties
/// </summary>
public abstract class HealthDataRecord
{
    /// <summary>
    /// Unique identifier for the record
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Record creation timestamp in UTC
    /// </summary>
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Record modification timestamp in UTC
    /// </summary>
    public DateTime ModifiedUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date this health record pertains to
    /// </summary>
    public DateTime RecordDate { get; set; }

    /// <summary>
    /// Source device that recorded this data
    /// </summary>
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>
    /// Firmware version of the recording device
    /// </summary>
    public string? FirmwareVersion { get; set; }

    /// <summary>
    /// Indicates if record data has been validated
    /// </summary>
    public bool IsValidated { get; private set; }

    /// <summary>
    /// Notes or additional metadata about the record
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Mark this record as validated after passing quality checks
    /// </summary>
    public virtual void MarkAsValidated()
    {
        IsValidated = true;
        ModifiedUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Check if the record contains valid data for analysis
    /// </summary>
    public abstract bool IsValid();

    /// <summary>
    /// Get a summary of the record's key metrics
    /// </summary>
    public abstract Dictionary<string, object> GetSummary();

    /// <summary>
    /// Update the modification timestamp to now
    /// </summary>
    public virtual void Touch()
    {
        ModifiedUtc = DateTime.UtcNow;
    }
}
