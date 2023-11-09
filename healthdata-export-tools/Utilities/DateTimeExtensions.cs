#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Runtime.CompilerServices;

namespace HealthDataExportTools.Utilities;

/// <summary>
/// Extension methods for DateTime operations
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Gets the start of the day (00:00:00) for the specified date.
    /// </summary>
    /// <param name="dateTime">The date and time value.</param>
    /// <returns>A <see cref="DateTime"/> value representing the start of the day.</returns>
    public static DateTime StartOfDay(this DateTime dateTime)
        => dateTime.Date;

    /// <summary>
    /// Gets the end of the day (23:59:59) for the specified date.
    /// </summary>
    /// <param name="dateTime">The date and time value.</param>
    /// <returns>A <see cref="DateTime"/> value representing the end of the day.</returns>
    public static DateTime EndOfDay(this DateTime dateTime)
        => dateTime.Date.AddDays(1).AddSeconds(-1);

    /// <summary>
    /// Gets the start of the week (Monday) for the specified date.
    /// </summary>
    /// <param name="dateTime">The date and time value.</param>
    /// <returns>A <see cref="DateTime"/> value representing the start of the week (Monday).</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the result would be outside the valid DateTime range.</exception>
    public static DateTime StartOfWeek(this DateTime dateTime)
    {
        var dayOfWeek = (int)dateTime.DayOfWeek;
        var daysToMonday = dayOfWeek == 0 ? 6 : dayOfWeek - 1;
        return dateTime.AddDays(-daysToMonday).StartOfDay();
    }

    /// <summary>
    /// Gets the end of the week (Sunday) for the specified date.
    /// </summary>
    /// <param name="dateTime">The date and time value.</param>
    /// <returns>A <see cref="DateTime"/> value representing the end of the week (Sunday).</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the result would be outside the valid DateTime range.</exception>
    public static DateTime EndOfWeek(this DateTime dateTime)
        => dateTime.StartOfWeek().AddDays(7).AddSeconds(-1);

    /// <summary>
    /// Gets the start of the month for the specified date.
    /// </summary>
    /// <param name="dateTime">The date and time value.</param>
    /// <returns>A <see cref="DateTime"/> value representing the start of the month (1st day).</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the result would be outside the valid DateTime range.</exception>
    public static DateTime StartOfMonth(this DateTime dateTime)
        => new DateTime(dateTime.Year, dateTime.Month, 1);

    /// <summary>
    /// Gets the end of the month for the specified date.
    /// </summary>
    /// <param name="dateTime">The date and time value.</param>
    /// <returns>A <see cref="DateTime"/> value representing the end of the month (last day at 23:59:59).</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the result would be outside the valid DateTime range.</exception>
    public static DateTime EndOfMonth(this DateTime dateTime)
        => dateTime.StartOfMonth().AddMonths(1).AddSeconds(-1);

    /// <summary>
    /// Gets the start of the year for the specified date.
    /// </summary>
    /// <param name="dateTime">The date and time value.</param>
    /// <returns>A <see cref="DateTime"/> value representing the start of the year (January 1st).</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the result would be outside the valid DateTime range.</exception>
    public static DateTime StartOfYear(this DateTime dateTime)
        => new DateTime(dateTime.Year, 1, 1);

    /// <summary>
    /// Gets the end of the year for the specified date.
    /// </summary>
    /// <param name="dateTime">The date and time value.</param>
    /// <returns>A <see cref="DateTime"/> value representing the end of the year (December 31st at 23:59:59).</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the result would be outside the valid DateTime range.</exception>
    public static DateTime EndOfYear(this DateTime dateTime)
        => new DateTime(dateTime.Year, 12, 31, 23, 59, 59);

    /// <summary>
    /// Checks whether the specified date is in the past (earlier than current UTC time).
    /// </summary>
    /// <param name="dateTime">The date and time value to check.</param>
    /// <returns><see langword="true"/> if the date is in the past; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPast(this DateTime dateTime)
        => dateTime < DateTime.UtcNow;

    /// <summary>
    /// Checks whether the specified date is in the future (later than current UTC time).
    /// </summary>
    /// <param name="dateTime">The date and time value to check.</param>
    /// <returns><see langword="true"/> if the date is in the future; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsFuture(this DateTime dateTime)
        => dateTime > DateTime.UtcNow;

    /// <summary>
    /// Checks whether the specified date represents today (same date as current UTC date).
    /// </summary>
    /// <param name="dateTime">The date and time value to check.</param>
    /// <returns><see langword="true"/> if the date is today; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsToday(this DateTime dateTime)
        => dateTime.Date == DateTime.UtcNow.Date;

    /// <summary>
    /// Calculates the age in years from a birth date.
    /// </summary>
    /// <param name="birthDate">The birth date.</param>
    /// <returns>The age in years.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the calculated age would be negative.</exception>
    public static int GetAge(this DateTime birthDate)
    {
        var today = DateTime.Today;
        var age = today.Year - birthDate.Year;
        if (birthDate.Date > today.AddYears(-age))
        {
            age--;
        }
        return age;
    }

    /// <summary>
    /// Calculates the absolute number of days between two dates.
    /// </summary>
    /// <param name="startDate">The start date.</param>
    /// <param name="endDate">The end date.</param>
    /// <returns>The absolute number of days between the two dates.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the date difference calculation overflows.</exception>
    public static int DaysBetween(this DateTime startDate, DateTime endDate)
    {
        var timeSpan = endDate > startDate
            ? endDate - startDate
            : startDate - endDate;
        return (int)timeSpan.TotalDays;
    }

    /// <summary>
    /// Gets a human-readable string representing the time elapsed since the specified date.
    /// </summary>
    /// <param name="dateTime">The date and time value.</param>
    /// <returns>A formatted string describing the elapsed time (e.g., "2 hours ago", "just now").</returns>
    public static string GetTimeElapsed(this DateTime dateTime)
    {
        var elapsed = DateTime.UtcNow - dateTime;
        var totalSeconds = elapsed.TotalSeconds;
        var mins = (int)elapsed.TotalMinutes;
        var hours = (int)elapsed.TotalHours;
        var days = (int)elapsed.TotalDays;

        return totalSeconds switch
        {
            < 60 => "just now",
            < 3600 => $"{mins} minute{(mins != 1 ? "s" : "")} ago",
            < 86400 => $"{hours} hour{(hours != 1 ? "s" : "")} ago",
            < 604800 => $"{days} day{(days != 1 ? "s" : "")} ago",
            _ => dateTime.ToString(Constants.DateTime.DateFormat)
        };
    }

    /// <summary>
    /// Rounds the specified date and time to the nearest interval.
    /// </summary>
    /// <param name="dateTime">The date and time value to round.</param>
    /// <param name="interval">The time interval to round to.</param>
    /// <returns>A <see cref="DateTime"/> value rounded to the nearest interval.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when interval is zero or negative, or when rounding would result in an invalid DateTime.
    /// </exception>
    public static DateTime RoundToNearest(this DateTime dateTime, TimeSpan interval)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(interval, TimeSpan.Zero);

        var halfInterval = new TimeSpan(interval.Ticks / 2);
        var difference = dateTime.Ticks % interval.Ticks;

        return difference < halfInterval.Ticks
            ? dateTime.AddTicks(-difference)
            : dateTime.AddTicks(interval.Ticks - difference);
    }

    /// <summary>
    /// Converts the specified date and time to an ISO 8601 formatted string.
    /// </summary>
    /// <param name="dateTime">The date and time value to format.</param>
    /// <returns>An ISO 8601 formatted string representation of the date and time.</returns>
    public static string ToIso8601(this DateTime dateTime)
        => dateTime.ToString(Constants.DateTime.Iso8601Format);

    /// <summary>
    /// Converts the specified date to a formatted date string (yyyy-MM-dd).
    /// </summary>
    /// <param name="dateTime">The date value to format.</param>
    /// <returns>A formatted date string in yyyy-MM-dd format.</returns>
    public static string ToDateString(this DateTime dateTime)
        => dateTime.ToString(Constants.DateTime.DateFormat);

    /// <summary>
    /// Converts the specified date and time to a formatted date-time string (yyyy-MM-dd HH:mm:ss).
    /// </summary>
    /// <param name="dateTime">The date and time value to format.</param>
    /// <returns>A formatted date-time string in yyyy-MM-dd HH:mm:ss format.</returns>
    public static string ToDateTimeString(this DateTime dateTime)
        => dateTime.ToString(Constants.DateTime.DateTimeFormat);
}
