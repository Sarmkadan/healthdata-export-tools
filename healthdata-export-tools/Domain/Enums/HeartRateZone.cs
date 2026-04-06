#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace HealthDataExportTools.Domain.Enums;

/// <summary>
/// Heart rate training zones based on percentage of maximum heart rate.
/// Zone boundaries follow the standard five-zone model:
/// Zone 1 ≤ 60 %, Zone 2 60-70 %, Zone 3 70-80 %, Zone 4 80-90 %, Zone 5 > 90 %.
/// </summary>
public enum HeartRateZone
{
    /// <summary>Zone 1: up to 60 % max HR – recovery / warm-up</summary>
    Zone1 = 1,

    /// <summary>Zone 2: 60-70 % max HR – fat-burn / base endurance</summary>
    Zone2 = 2,

    /// <summary>Zone 3: 70-80 % max HR – aerobic / cardio fitness</summary>
    Zone3 = 3,

    /// <summary>Zone 4: 80-90 % max HR – lactate threshold</summary>
    Zone4 = 4,

    /// <summary>Zone 5: above 90 % max HR – anaerobic / maximum effort</summary>
    Zone5 = 5
}
