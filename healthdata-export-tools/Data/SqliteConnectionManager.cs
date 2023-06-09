// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Data.SQLite;
using HealthDataExportTools.Exceptions;
using HealthDataExportTools.Utilities;

namespace HealthDataExportTools.Data;

/// <summary>
/// Manages SQLite database connections and initialization
/// </summary>
public class SqliteConnectionManager
{
    private readonly string _databasePath;
    private readonly string _connectionString;

    public SqliteConnectionManager(string databasePath)
    {
        _databasePath = databasePath;
        _connectionString = string.Format(Constants.Database.SqliteConnectionTemplate, databasePath);
    }

    /// <summary>
    /// Get a new SQLite connection
    /// </summary>
    public SQLiteConnection GetConnection()
    {
        try
        {
            var connection = new SQLiteConnection(_connectionString);
            return connection;
        }
        catch (Exception ex)
        {
            throw new DataAccessException("Failed to create database connection", "GetConnection", ex);
        }
    }

    /// <summary>
    /// Initialize database schema
    /// </summary>
    public async Task InitializeDatabaseAsync()
    {
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = GetSchemaScript();
            await command.ExecuteNonQueryAsync();

            await connection.CloseAsync();
        }
        catch (Exception ex)
        {
            throw new DataAccessException("Failed to initialize database", "InitializeDatabase", ex);
        }
    }

    /// <summary>
    /// Check if database file exists
    /// </summary>
    public bool DatabaseExists()
    {
        return File.Exists(_databasePath);
    }

    /// <summary>
    /// Delete database file
    /// </summary>
    public void DeleteDatabase()
    {
        try
        {
            if (File.Exists(_databasePath))
                File.Delete(_databasePath);
        }
        catch (IOException ex)
        {
            throw new DataAccessException("Failed to delete database", "DeleteDatabase", ex);
        }
    }

    /// <summary>
    /// Get database file size in bytes
    /// </summary>
    public long GetDatabaseSize()
    {
        try
        {
            if (!DatabaseExists()) return 0;
            var info = new FileInfo(_databasePath);
            return info.Length;
        }
        catch (Exception ex)
        {
            throw new DataAccessException("Failed to get database size", "GetDatabaseSize", ex);
        }
    }

    /// <summary>
    /// Get the SQL schema creation script
    /// </summary>
    private static string GetSchemaScript()
    {
        return @"
            CREATE TABLE IF NOT EXISTS SleepData (
                Id TEXT PRIMARY KEY,
                RecordDate TEXT NOT NULL,
                DeviceId TEXT NOT NULL,
                SleepStart TEXT NOT NULL,
                SleepEnd TEXT NOT NULL,
                DurationMinutes INTEGER NOT NULL,
                DeepSleepMinutes INTEGER NOT NULL,
                LightSleepMinutes INTEGER NOT NULL,
                RemSleepMinutes INTEGER NOT NULL,
                AwakeMinutes INTEGER NOT NULL,
                Quality INTEGER,
                Score INTEGER,
                AverageHeartRate INTEGER,
                CreatedUtc TEXT NOT NULL,
                ModifiedUtc TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS HeartRateData (
                Id TEXT PRIMARY KEY,
                RecordDate TEXT NOT NULL,
                DeviceId TEXT NOT NULL,
                MinimumBpm INTEGER NOT NULL,
                MaximumBpm INTEGER NOT NULL,
                AverageBpm INTEGER NOT NULL,
                RestingBpm INTEGER,
                MeasurementCount INTEGER NOT NULL,
                HeartRateVariability REAL,
                StressLevel INTEGER,
                CardioZoneMinutes INTEGER,
                FatBurnZoneMinutes INTEGER,
                CreatedUtc TEXT NOT NULL,
                ModifiedUtc TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS SpO2Data (
                Id TEXT PRIMARY KEY,
                RecordDate TEXT NOT NULL,
                DeviceId TEXT NOT NULL,
                MinimumPercentage INTEGER NOT NULL,
                MaximumPercentage INTEGER NOT NULL,
                AveragePercentage INTEGER NOT NULL,
                RestingPercentage INTEGER,
                MeasurementCount INTEGER NOT NULL,
                LowSpO2Events INTEGER,
                LowestAlertValue INTEGER,
                ReliabilityScore INTEGER,
                CreatedUtc TEXT NOT NULL,
                ModifiedUtc TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS StepsData (
                Id TEXT PRIMARY KEY,
                RecordDate TEXT NOT NULL,
                DeviceId TEXT NOT NULL,
                TotalSteps INTEGER NOT NULL,
                DistanceKm REAL NOT NULL,
                CaloriesBurned INTEGER NOT NULL,
                DailyGoal INTEGER NOT NULL,
                GoalAchievementPercentage INTEGER,
                AverageCadence INTEGER,
                PeakStepsPerHour INTEGER,
                ActiveMinutes INTEGER,
                WalkingMinutes INTEGER,
                RunningMinutes INTEGER,
                CreatedUtc TEXT NOT NULL,
                ModifiedUtc TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS ActivityData (
                Id TEXT PRIMARY KEY,
                RecordDate TEXT NOT NULL,
                DeviceId TEXT NOT NULL,
                ActivityType TEXT NOT NULL,
                StartTime TEXT NOT NULL,
                EndTime TEXT NOT NULL,
                DurationMinutes INTEGER NOT NULL,
                DistanceKm REAL NOT NULL,
                AveragePaceMinPerKm REAL,
                AverageSpeedKmh REAL,
                MaximumSpeedKmh REAL,
                CaloriesBurned INTEGER NOT NULL,
                AverageHeartRate INTEGER,
                MaximumHeartRate INTEGER,
                ElevationGainMeters INTEGER,
                ElevationLossMeters INTEGER,
                IntensityLevel INTEGER,
                Rating REAL,
                CreatedUtc TEXT NOT NULL,
                ModifiedUtc TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS HealthMetric (
                Id TEXT PRIMARY KEY,
                RecordDate TEXT NOT NULL,
                MetricName TEXT NOT NULL,
                Value REAL NOT NULL,
                Unit TEXT NOT NULL,
                NormalRangeLow REAL,
                NormalRangeHigh REAL,
                PreviousValue REAL,
                Trend INTEGER,
                PercentageChange REAL,
                HealthStatus TEXT,
                ConfidenceScore INTEGER,
                SampleDays INTEGER,
                CreatedUtc TEXT NOT NULL,
                ModifiedUtc TEXT NOT NULL
            );

            CREATE INDEX IF NOT EXISTS idx_sleep_date ON SleepData(RecordDate);
            CREATE INDEX IF NOT EXISTS idx_heartrate_date ON HeartRateData(RecordDate);
            CREATE INDEX IF NOT EXISTS idx_spo2_date ON SpO2Data(RecordDate);
            CREATE INDEX IF NOT EXISTS idx_steps_date ON StepsData(RecordDate);
            CREATE INDEX IF NOT EXISTS idx_activity_date ON ActivityData(RecordDate);
            CREATE INDEX IF NOT EXISTS idx_metric_name ON HealthMetric(MetricName);
        ";
    }

    /// <summary>
    /// Verify database connectivity
    /// </summary>
    public async Task<bool> VerifyConnectionAsync()
    {
        try
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1;";
            var result = await command.ExecuteScalarAsync();
            await connection.CloseAsync();
            return result != null;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Get connection string
    /// </summary>
    public string GetConnectionString() => _connectionString;
}
