// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace HealthDataExportTools.Domain.Enums;

/// <summary>
/// Supported export formats for health data output
/// </summary>
public enum ExportFormat
{
    /// <summary>Comma-separated values format</summary>
    Csv,

    /// <summary>JavaScript Object Notation format</summary>
    Json,

    /// <summary>SQLite database format</summary>
    Sqlite,

    /// <summary>All formats combined</summary>
    All
}
