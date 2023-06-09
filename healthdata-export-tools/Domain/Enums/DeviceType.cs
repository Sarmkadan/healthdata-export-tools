// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace HealthDataExportTools.Domain.Enums;

/// <summary>
/// Supported wearable device types
/// </summary>
public enum DeviceType
{
    /// <summary>Zepp or Amazfit branded devices</summary>
    Zepp,

    /// <summary>Amazfit specific devices (alternative branding)</summary>
    Amazfit,

    /// <summary>Garmin wearable devices</summary>
    Garmin,

    /// <summary>Unknown or unsupported device type</summary>
    Unknown
}
