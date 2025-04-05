// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Runtime.CompilerServices;

namespace HealthDataExportTools.Utilities;

/// <summary>
/// Extension methods for DateTime operations
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Get start of the day (00:00:00)
    /// </summary>
    public static DateTime StartOfDay(this DateTime dateTime)
    {
        return dateTime.Date;
    }

    /// <summary>
    /// Get end of the day (23:59:59)
    /// </summary>
    public static DateTime EndOfDay(this DateTime dateTime)
    {
        return dateTime.Date.AddDays(1).AddSeconds(-1);
    }

    /// <summary>
    /// Get start of the week (Monday)
    /// </summary>
    public static DateTime StartOfWeek(this DateTime dateTime)
    {
        var dayOfWeek = (int)dateTime.DayOfWeek;
        var daysToMonday = dayOfWeek == 0 ? 6 : dayOfWeek - 1;
        return dateTime.AddDays(-daysToMonday).StartOfDay();
    }

    /// <summary>
    /// Get end of the week (Sunday)
    /// </summary>
    public static DateTime EndOfWeek(this DateTime dateTime)
    {
        return dateTime.StartOfWeek().AddDays(7).AddSeconds(-1);
    }

    /// <summary>
    /// Get start of the month
    /// </summary>
    public static DateTime StartOfMonth(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, 1);
    }

    /// <summary>
    /// Get end of the month
    /// </summary>
    public static DateTime EndOfMonth(this DateTime dateTime)
    {
        return dateTime.StartOfMonth().AddMonths(1).AddSeconds(-1);
    }

    /// <summary>
    /// Get start of the year
    /// </summary>
    public static DateTime StartOfYear(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, 1, 1);
    }

    /// <summary>
    /// Get end of the year
    /// </summary>
    public static DateTime EndOfYear(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, 12, 31, 23, 59, 59);
    }

    /// <summary>
    /// Check if a date is in the past
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPast(this DateTime dateTime)
    {
        return dateTime < DateTime.UtcNow;
    }

    /// <summary>
    /// Check if a date is in the future
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsFuture(this DateTime dateTime)
    {
        return dateTime > DateTime.UtcNow;
    }

    /// <summary>
    /// Check if a date is today
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsToday(this DateTime dateTime)
    {
        return dateTime.Date == DateTime.UtcNow.Date;
    }

    /// <summary>
    /// Calculate age from birthdate
    /// </summary>
    public static int GetAge(this DateTime birthDate)
    {
        var today = DateTime.Today;
        var age = today.Year - birthDate.Year;
        if (birthDate.Date > today.AddYears(-age)) age--;
        return age;
    }

    /// <summary>
    /// Get number of days between two dates
    /// </summary>
    public static int DaysBetween(this DateTime startDate, DateTime endDate)
    {
        return (int)Math.Abs((endDate - startDate).TotalDays);
    }

    /// <summary>
    /// Get human-readable time elapsed (e.g., "2 hours ago")
    /// </summary>
    public static string GetTimeElapsed(this DateTime dateTime)
    {
        var elapsed = DateTime.UtcNow - dateTime;
        var mins = (int)elapsed.TotalMinutes;
        var hours = (int)elapsed.TotalHours;
        var days = (int)elapsed.TotalDays;

        return elapsed.TotalSeconds switch
        {
            < 60 => "just now",
            < 3600 => $"{mins} minute{(mins != 1 ? "s" : "")} ago",
            < 86400 => $"{hours} hour{(hours != 1 ? "s" : "")} ago",
            < 604800 => $"{days} day{(days != 1 ? "s" : "")} ago",
            _ => dateTime.ToString(Constants.DateTime.DateFormat)
        };
    }

    /// <summary>
    /// Round time to nearest interval
    /// </summary>
    public static DateTime RoundToNearest(this DateTime dateTime, TimeSpan interval)
    {
        var halfInterval = new TimeSpan(interval.Ticks / 2);
        var difference = dateTime.Ticks % interval.Ticks;

        return difference < halfInterval.Ticks
            ? dateTime.AddTicks(-difference)
            : dateTime.AddTicks(interval.Ticks - difference);
    }

    /// <summary>
    /// Convert DateTime to ISO 8601 string
    /// </summary>
    public static string ToIso8601(this DateTime dateTime)
    {
        return dateTime.ToString(Constants.DateTime.Iso8601Format);
    }

    /// <summary>
    /// Convert DateTime to formatted date string
    /// </summary>
    public static string ToDateString(this DateTime dateTime)
    {
        return dateTime.ToString(Constants.DateTime.DateFormat);
    }

    /// <summary>
    /// Convert DateTime to formatted date-time string
    /// </summary>
    public static string ToDateTimeString(this DateTime dateTime)
    {
        return dateTime.ToString(Constants.DateTime.DateTimeFormat);
    }
}
