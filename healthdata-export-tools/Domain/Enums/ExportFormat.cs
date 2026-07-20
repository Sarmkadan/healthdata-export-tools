#nullable enable
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

    /// <summary>HTML chart/graph export format</summary>
    Html,

    /// <summary>All formats combined</summary>
    All,

    /// <summary>JSON Lines format - one JSON object per line</summary>
    JsonLines
}
