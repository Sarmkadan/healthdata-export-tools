// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using HealthDataExportTools.Exceptions;

namespace HealthDataExportTools.Utilities;

/// <summary>
/// Validation helper methods for health data
/// </summary>
public static class ValidationHelper
{
    /// <summary>
    /// Validate heart rate value
    /// </summary>
    public static bool IsValidHeartRate(int bpm)
    {
        return bpm >= Constants.HealthData.MinHeartRate && bpm <= Constants.HealthData.MaxHeartRate;
    }

    /// <summary>
    /// Validate resting heart rate
    /// </summary>
    public static bool IsValidRestingHeartRate(int bpm)
    {
        return bpm >= Constants.HealthData.RestingHeartRateMin &&
               bpm <= Constants.HealthData.RestingHeartRateMax;
    }

    /// <summary>
    /// Validate SpO2 percentage
    /// </summary>
    public static bool IsValidSpO2(int percentage)
    {
        return percentage >= 0 && percentage <= 100;
    }

    /// <summary>
    /// Validate sleep duration in minutes
    /// </summary>
    public static bool IsValidSleepDuration(int minutes)
    {
        return minutes >= Constants.HealthData.MinSleepDuration &&
               minutes <= Constants.HealthData.MaxSleepDuration;
    }

    /// <summary>
    /// Validate activity duration
    /// </summary>
    public static bool IsValidActivityDuration(int minutes)
    {
        return minutes >= Constants.HealthData.MinActivityDuration &&
               minutes <= Constants.HealthData.MaxActivityDuration;
    }

    /// <summary>
    /// Validate date is not in the future (for health records)
    /// </summary>
    public static bool IsValidRecordDate(DateTime recordDate)
    {
        return recordDate.Date <= DateTime.UtcNow.Date;
    }

    /// <summary>
    /// Validate email address format
    /// </summary>
    public static bool IsValidEmail(string email)
    {
        try
        {
            var address = new System.Net.Mail.MailAddress(email);
            return address.Address == email;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validate file path exists and is readable
    /// </summary>
    public static bool IsValidFilePath(string filePath)
    {
        try
        {
            return !string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validate directory path exists and is writable
    /// </summary>
    public static bool IsValidDirectoryPath(string dirPath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dirPath)) return false;

            var dir = new DirectoryInfo(dirPath);
            return dir.Exists;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validate GPS coordinates
    /// </summary>
    public static bool IsValidGpsCoordinates(double latitude, double longitude)
    {
        return latitude >= -90 && latitude <= 90 && longitude >= -180 && longitude <= 180;
    }

    /// <summary>
    /// Validate distance value in kilometers
    /// </summary>
    public static bool IsValidDistance(double distanceKm)
    {
        return !double.IsNaN(distanceKm) && !double.IsInfinity(distanceKm) && distanceKm >= 0;
    }

    /// <summary>
    /// Validate percentage value (0-100)
    /// </summary>
    public static bool IsValidPercentage(int percentage)
    {
        return percentage >= 0 && percentage <= 100;
    }

    /// <summary>
    /// Validate percentage value (0-100) with extended range
    /// </summary>
    public static bool IsValidPercentageExtended(int percentage)
    {
        return percentage >= 0 && percentage <= 200;
    }

    /// <summary>
    /// Validate elevation gain/loss in meters
    /// </summary>
    public static bool IsValidElevation(int meters)
    {
        return meters >= 0 && meters <= 9000;
    }

    /// <summary>
    /// Validate temperature value in Celsius
    /// </summary>
    public static bool IsValidTemperature(double celsius)
    {
        return celsius >= -50 && celsius <= 60;
    }

    /// <summary>
    /// Validate calories value
    /// </summary>
    public static bool IsValidCalories(int calories)
    {
        return calories >= 0 && calories <= 100000;
    }

    /// <summary>
    /// Ensure value is within range, throw if not
    /// </summary>
    public static void EnsureInRange(int value, int min, int max, string fieldName)
    {
        if (value < min || value > max)
        {
            throw new ValidationException(
                $"{fieldName} must be between {min} and {max}",
                fieldName,
                $"Range: [{min}, {max}]"
            );
        }
    }

    /// <summary>
    /// Ensure value is not null, throw if null
    /// </summary>
    public static void EnsureNotNull<T>(T? value, string fieldName) where T : class
    {
        if (value == null)
        {
            throw new ValidationException(
                $"{fieldName} cannot be null",
                fieldName,
                "NotNull"
            );
        }
    }

    /// <summary>
    /// Ensure string is not empty, throw if empty
    /// </summary>
    public static void EnsureNotEmpty(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ValidationException(
                $"{fieldName} cannot be empty",
                fieldName,
                "NotEmpty"
            );
        }
    }

    /// <summary>
    /// Get validation errors for heart rate
    /// </summary>
    public static List<string> ValidateHeartRateData(int min, int max, int avg)
    {
        var errors = new List<string>();

        if (!IsValidHeartRate(min)) errors.Add("Minimum heart rate is out of valid range");
        if (!IsValidHeartRate(max)) errors.Add("Maximum heart rate is out of valid range");
        if (!IsValidHeartRate(avg)) errors.Add("Average heart rate is out of valid range");
        if (min > max) errors.Add("Minimum heart rate cannot be greater than maximum");
        if (avg < min || avg > max) errors.Add("Average heart rate must be between minimum and maximum");

        return errors;
    }
}
