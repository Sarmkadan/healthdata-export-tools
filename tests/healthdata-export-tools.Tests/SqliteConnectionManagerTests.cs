#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using HealthDataExportTools.Data;
using HealthDataExportTools.Exceptions;
using Microsoft.Data.Sqlite;
using Xunit;

/// <summary>
/// Tests for SqliteConnectionManager class.
/// </summary>
public sealed class SqliteConnectionManagerTests
{
    /// <summary>
    /// Tests that GetConnection method returns an open connection.
    /// </summary>
    [Fact]
    public async Task GetConnection_ShouldReturnOpenConnection()
    {
        // Arrange
        var connectionManager = new SqliteConnectionManager(":memory:");

        // Act
        using var connection = connectionManager.GetConnection();
        await connection.OpenAsync().ConfigureAwait(false);

        // Assert
        connection.State.Should().Be(System.Data.ConnectionState.Open);
        await connection.CloseAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Tests that InitializeDatabaseAsync method creates tables in the database.
    /// </summary>
    [Fact]
    public async Task InitializeDatabaseAsync_ShouldCreateTables()
    {
        // Arrange
        var connectionManager = new SqliteConnectionManager(":memory:");

        // Act
        await connectionManager.InitializeDatabaseAsync().ConfigureAwait(false);

        // Assert
        using var connection = connectionManager.GetConnection();
        await connection.OpenAsync().ConfigureAwait(false);

        var tableNames = new List<string>();
        using (var command = connection.CreateCommand())
        {
            command.CommandText = "SELECT name FROM sqlite_master WHERE type='table';";
            using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            while (await reader.ReadAsync())
            {
                tableNames.Add(reader.GetString(0));
            }
        }

        tableNames.Should().Contain("SleepData");
        tableNames.Should().Contain("HeartRateData");
        tableNames.Should().Contain("SpO2Data");
        tableNames.Should().Contain("StepsData");
        tableNames.Should().Contain("ActivityData");
        tableNames.Should().Contain("HealthMetric");

        await connection.CloseAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Tests that VerifyConnectionAsync method returns true for an open connection.
    /// </summary>
    [Fact]
    public async Task VerifyConnectionAsync_ShouldReturnTrueForOpenConnection()
    {
        // Arrange
        var connectionManager = new SqliteConnectionManager(":memory:");
        await connectionManager.InitializeDatabaseAsync().ConfigureAwait(false); // Ensure DB is initialized

        // Act
        var isConnected = await connectionManager.VerifyConnectionAsync().ConfigureAwait(false);

        // Assert
        isConnected.Should().BeTrue();
    }

    /// <summary>
    /// Tests that VerifyConnectionAsync method returns false for an invalid connection.
    /// </summary>
    [Fact]
    public async Task VerifyConnectionAsync_ShouldReturnFalseForInvalidConnection()
    {
        // Arrange - use a path that won't create a valid database (e.g., a non-existent directory)
        var connectionManager = new SqliteConnectionManager("file:///non-existent-dir/test.db");

        // Act
        var isConnected = await connectionManager.VerifyConnectionAsync().ConfigureAwait(false);

        // Assert
        isConnected.Should().BeFalse();
    }

    /// <summary>
    /// Tests that DatabaseExists method returns false for a non-existent database file.
    /// </summary>
    [Fact]
    public void DatabaseExists_ShouldReturnFalseForNonExistentFile()
    {
        // Arrange
        var tempDbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".db");
        var connectionManager = new SqliteConnectionManager(tempDbPath);

        // Act
        var exists = connectionManager.DatabaseExists();

        // Assert
        exists.Should().BeFalse();
    }

    /// <summary>
    /// Tests that DatabaseExists method returns true for a created database file.
    /// </summary>
    [Fact]
    public async Task DatabaseExists_ShouldReturnTrueForCreatedFile()
    {
        // Arrange
        var tempDbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".db");
        var connectionManager = new SqliteConnectionManager(tempDbPath);
        await connectionManager.InitializeDatabaseAsync().ConfigureAwait(false);

        // Act
        var exists = connectionManager.DatabaseExists();

        // Assert
        exists.Should().BeTrue();

        // Cleanup
        connectionManager.DeleteDatabase();
    }

    /// <summary>
    /// Tests that DeleteDatabase method removes the database file.
    /// </summary>
    [Fact]
    public async Task DeleteDatabase_ShouldRemoveDatabaseFile()
    {
        // Arrange
        var tempDbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".db");
        var connectionManager = new SqliteConnectionManager(tempDbPath);
        await connectionManager.InitializeDatabaseAsync().ConfigureAwait(false); // Create the file

        // Pre-Assert
        File.Exists(tempDbPath).Should().BeTrue();

        // Act
        connectionManager.DeleteDatabase();

        // Assert
        File.Exists(tempDbPath).Should().BeFalse();
    }

    /// <summary>
    /// Tests that GetDatabaseSize method returns the correct size of the database.
    /// </summary>
    [Fact]
    public async Task GetDatabaseSize_ShouldReturnCorrectSize()
    {
        // Arrange
        var tempDbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".db");
        var connectionManager = new SqliteConnectionManager(tempDbPath);
        await connectionManager.InitializeDatabaseAsync().ConfigureAwait(false); // Create tables, adding some size

        // Act
        var size = connectionManager.GetDatabaseSize();

        // Assert
        size.Should().BeGreaterThan(0); // A newly created DB with schema should have some size

        // Cleanup
        connectionManager.DeleteDatabase();
    }

    /// <summary>
    /// Tests that GetDatabaseSize method returns zero for a non-existent database.
    /// </summary>
    [Fact]
    public void GetDatabaseSize_ShouldReturnZeroForNonExistentDatabase()
    {
        // Arrange
        var connectionManager = new SqliteConnectionManager("nonexistent.db");

        // Act
        var size = connectionManager.GetDatabaseSize();

        // Assert
        size.Should().Be(0);
    }
}
