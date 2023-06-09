// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace HealthDataExportTools.Utilities;

/// <summary>
/// Application-wide constants and configuration defaults
/// </summary>
public static class Constants
{
    /// <summary>
    /// Application version
    /// </summary>
    public const string AppVersion = "1.0.0";

    /// <summary>
    /// Application name
    /// </summary>
    public const string AppName = "Health Data Export Tools";

    // Health Data Validation Constants
    public static class HealthData
    {
        /// <summary>
        /// Minimum reasonable heart rate in BPM
        /// </summary>
        public const int MinHeartRate = 30;

        /// <summary>
        /// Maximum reasonable heart rate in BPM
        /// </summary>
        public const int MaxHeartRate = 220;

        /// <summary>
        /// Resting heart rate minimum threshold in BPM
        /// </summary>
        public const int RestingHeartRateMin = 40;

        /// <summary>
        /// Resting heart rate maximum threshold in BPM
        /// </summary>
        public const int RestingHeartRateMax = 100;

        /// <summary>
        /// Normal SpO2 minimum percentage
        /// </summary>
        public const int NormalSpO2Min = 95;

        /// <summary>
        /// Normal SpO2 maximum percentage
        /// </summary>
        public const int NormalSpO2Max = 100;

        /// <summary>
        /// SpO2 alert threshold (concerning level)
        /// </summary>
        public const int SpO2AlertThreshold = 90;

        /// <summary>
        /// Minimum sleep duration for a valid night in minutes
        /// </summary>
        public const int MinSleepDuration = 180;

        /// <summary>
        /// Maximum sleep duration in minutes
        /// </summary>
        public const int MaxSleepDuration = 720;

        /// <summary>
        /// Recommended daily steps goal
        /// </summary>
        public const int RecommendedDailySteps = 10000;

        /// <summary>
        /// Minimum activity duration in minutes
        /// </summary>
        public const int MinActivityDuration = 1;

        /// <summary>
        /// Maximum activity duration in minutes
        /// </summary>
        public const int MaxActivityDuration = 1440;
    }

    // File Format Constants
    public static class FileFormats
    {
        /// <summary>
        /// CSV file extension
        /// </summary>
        public const string CsvExtension = ".csv";

        /// <summary>
        /// JSON file extension
        /// </summary>
        public const string JsonExtension = ".json";

        /// <summary>
        /// SQLite database extension
        /// </summary>
        public const string SqliteExtension = ".db";

        /// <summary>
        /// ZIP archive extension
        /// </summary>
        public const string ZipExtension = ".zip";

        /// <summary>
        /// XML file extension
        /// </summary>
        public const string XmlExtension = ".xml";
    }

    // Date/Time Constants
    public static class DateTime
    {
        /// <summary>
        /// Standard date format for display
        /// </summary>
        public const string DateFormat = "yyyy-MM-dd";

        /// <summary>
        /// Standard date-time format for display
        /// </summary>
        public const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

        /// <summary>
        /// ISO 8601 format
        /// </summary>
        public const string Iso8601Format = "O";
    }

    // Database Constants
    public static class Database
    {
        /// <summary>
        /// SQLite connection string template
        /// </summary>
        public const string SqliteConnectionTemplate = "Data Source={0};Version=3;";

        /// <summary>
        /// Default database file name
        /// </summary>
        public const string DefaultDatabaseName = "healthdata.db";

        /// <summary>
        /// Table name for sleep records
        /// </summary>
        public const string SleepTableName = "SleepData";

        /// <summary>
        /// Table name for heart rate records
        /// </summary>
        public const string HeartRateTableName = "HeartRateData";

        /// <summary>
        /// Table name for SpO2 records
        /// </summary>
        public const string SpO2TableName = "SpO2Data";

        /// <summary>
        /// Table name for steps records
        /// </summary>
        public const string StepsTableName = "StepsData";

        /// <summary>
        /// Table name for activity records
        /// </summary>
        public const string ActivityTableName = "ActivityData";
    }

    // Export Constants
    public static class Export
    {
        /// <summary>
        /// Default CSV delimiter
        /// </summary>
        public const char CsvDelimiter = ',';

        /// <summary>
        /// Default encoding for text files
        /// </summary>
        public const string DefaultEncoding = "UTF-8";

        /// <summary>
        /// JSON indentation spaces
        /// </summary>
        public const int JsonIndentation = 2;
    }

    // Error Messages
    public static class ErrorMessages
    {
        public const string InvalidFilePath = "The specified file path is invalid or does not exist.";
        public const string ParsingFailed = "Failed to parse the health data file.";
        public const string ValidationFailed = "Health data validation failed.";
        public const string ExportFailed = "Failed to export health data.";
        public const string DatabaseError = "A database error occurred.";
        public const string InvalidDataFormat = "The data format is not recognized.";
        public const string MissingRequiredData = "Required data fields are missing.";
    }
}
