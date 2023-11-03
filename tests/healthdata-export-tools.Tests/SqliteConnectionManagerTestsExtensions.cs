#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Data;
using FluentAssertions;
using HealthDataExportTools.Data;
using Microsoft.Data.Sqlite;
using Xunit;

namespace HealthDataExportTools.Tests;

public static class SqliteConnectionManagerTestsExtensions
{
    /// <summary>
    /// Extension method to verify that a database connection is properly initialized and contains expected tables.
    /// </summary>
    /// <param name="connectionManager">The connection manager to verify. Cannot be null.</param>
    /// <param name="expectedTableNames">List of table names that should exist in the database. Cannot be null.</param>
    /// <returns>True if verification succeeds, false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="connectionManager"/> or <paramref name="expectedTableNames"/> is null.</exception>
    public static async Task<bool> VerifyDatabaseInitializedAsync(
        this SqliteConnectionManager connectionManager,
        params string[] expectedTableNames)
    {
        ArgumentNullException.ThrowIfNull(connectionManager);
        ArgumentNullException.ThrowIfNull(expectedTableNames);

        // Verify connection is valid
        var isConnected = await connectionManager.VerifyConnectionAsync().ConfigureAwait(false);
        if (!isConnected)
        {
            return false;
        }

        // Get list of actual tables
        using var connection = connectionManager.GetConnection();
        await connection.OpenAsync().ConfigureAwait(false);

        var actualTableNames = new List<string>();
        using (var command = connection.CreateCommand())
        {
            command.CommandText = "SELECT name FROM sqlite_master WHERE type='table';\n";
            using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                actualTableNames.Add(reader.GetString(0));
            }
        }

        await connection.CloseAsync().ConfigureAwait(false);

        // Verify all expected tables exist
        foreach (var expectedTable in expectedTableNames)
        {
            if (!actualTableNames.Contains(expectedTable, StringComparer.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Extension method to create a test database with sample data for testing purposes.
    /// </summary>
    /// <param name="connectionManager">The connection manager. Cannot be null.</param>
    /// <param name="sampleDataCount">Number of sample records to insert. Must be non-negative.</param>
    /// <returns>Task representing the async operation</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="connectionManager"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="sampleDataCount"/> is negative.</exception>
    public static async Task CreateTestDatabaseWithSampleDataAsync(
        this SqliteConnectionManager connectionManager,
        int sampleDataCount = 10)
    {
        ArgumentNullException.ThrowIfNull(connectionManager);
        ArgumentOutOfRangeException.ThrowIfNegative(sampleDataCount);

        await connectionManager.InitializeDatabaseAsync().ConfigureAwait(false);

        using var connection = connectionManager.GetConnection();
        await connection.OpenAsync().ConfigureAwait(false);

        // Insert sample SleepData
        for (int i = 0; i < sampleDataCount; i++)
        {
            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO SleepData (Date, StartTime, EndTime, QualityScore, MinutesAsleep, MinutesAwake)
                VALUES (date('now', ?i), time('now', ?i), time('now', ?i + 1), ?quality, ?asleep, ?awake)
            ";
            command.Parameters.Add(new SqliteParameter("?i", i));
            command.Parameters.Add(new SqliteParameter("?quality", 75 + i % 25));
            command.Parameters.Add(new SqliteParameter("?asleep", 420 + i * 10));
            command.Parameters.Add(new SqliteParameter("?awake", 30 + i * 2));
            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        // Insert sample HeartRateData
        for (int i = 0; i < sampleDataCount; i++)
        {
            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO HeartRateData (Date, Timestamp, AverageRate, MinRate, MaxRate, RestingRate)
                VALUES (date('now', ?i), datetime('now', ?i), ?avg, ?min, ?max, ?resting)
            ";
            command.Parameters.Add(new SqliteParameter("?i", i));
            command.Parameters.Add(new SqliteParameter("?avg", 70 + i % 20));
            command.Parameters.Add(new SqliteParameter("?min", 50 + i % 15));
            command.Parameters.Add(new SqliteParameter("?max", 120 + i % 30));
            command.Parameters.Add(new SqliteParameter("?resting", 60 + i % 10));
            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        await connection.CloseAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Extension method to get the current record count for a specific table.
    /// </summary>
    /// <param name="connectionManager">The connection manager. Cannot be null.</param>
    /// <param name="tableName">Name of the table to count records in. Cannot be null or empty.</param>
    /// <returns>Number of records in the table</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="connectionManager"/> or <paramref name="tableName"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="tableName"/> is empty or whitespace.</exception>
    public static async Task<long> GetTableRecordCountAsync(
        this SqliteConnectionManager connectionManager,
        string tableName)
    {
        ArgumentNullException.ThrowIfNull(connectionManager);
        ArgumentException.ThrowIfNullOrEmpty(tableName, nameof(tableName));

        using var connection = connectionManager.GetConnection();
        await connection.OpenAsync().ConfigureAwait(false);

        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM {tableName};\n";
        var result = await command.ExecuteScalarAsync().ConfigureAwait(false);

        await connection.CloseAsync().ConfigureAwait(false);

        return Convert.ToInt64(result);
    }

    /// <summary>
    /// Extension method to verify that a database file has the expected size range.
    /// </summary>
    /// <param name="connectionManager">The connection manager. Cannot be null.</param>
    /// <param name="minSizeBytes">Minimum expected size in bytes. Must be non-negative.</param>
    /// <param name="maxSizeBytes">Maximum expected size in bytes. Must be non-negative and greater than or equal to <paramref name="minSizeBytes"/>.</param>
    /// <returns>True if size is within expected range, false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="connectionManager"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="minSizeBytes"/> or <paramref name="maxSizeBytes"/> is negative, or when <paramref name="maxSizeBytes"/> is less than <paramref name="minSizeBytes"/>.</exception>
    public static bool VerifyDatabaseSize(
        this SqliteConnectionManager connectionManager,
        long minSizeBytes,
        long maxSizeBytes)
    {
        ArgumentNullException.ThrowIfNull(connectionManager);
        ArgumentOutOfRangeException.ThrowIfNegative(minSizeBytes);
        ArgumentOutOfRangeException.ThrowIfNegative(maxSizeBytes);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxSizeBytes, minSizeBytes);

        var size = connectionManager.GetDatabaseSize();
        return size >= minSizeBytes && size <= maxSizeBytes;
    }
}