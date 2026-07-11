# SqliteConnectionManager

A utility class that encapsulates the creation, initialization, and management of SQLite database connections for the health data export tools. It provides methods to check database existence, initialize schemas, verify connections, and perform basic database maintenance operations such as deletion and size retrieval.

## API

### `public SqliteConnectionManager`

Initializes a new instance of the `SqliteConnectionManager` with the default SQLite connection string configured for the health data export tools. The connection string is derived from application settings or environment variables specific to the project.

### `public SqliteConnection GetConnection()`

Creates and returns a new `SqliteConnection` instance using the configured connection string. The connection is not opened automatically; callers must open the connection as needed.

- **Parameters**: None.
- **Return value**: A new `SqliteConnection` instance.
- **Exceptions**: Throws `InvalidOperationException` if the connection string is null or empty.

### `public async Task InitializeDatabaseAsync()`

Ensures the SQLite database exists and applies any required schema migrations or initialization logic. This method is idempotent and safe to call multiple times.

- **Parameters**: None.
- **Return value**: A `Task` representing the asynchronous operation.
- **Exceptions**: Throws `InvalidOperationException` if the connection string is invalid or if database initialization fails.

### `public bool DatabaseExists()`

Checks whether the SQLite database file exists on disk.

- **Parameters**: None.
- **Return value**: `true` if the database file exists; otherwise, `false`.
- **Exceptions**: None.

### `public void DeleteDatabase()`

Deletes the SQLite database file from disk if it exists. Use with caution; this operation is irreversible.

- **Parameters**: None.
- **Return value**: None.
- **Exceptions**: Throws `IOException` if the file cannot be deleted due to access restrictions or if the file is locked.

### `public long GetDatabaseSize()`

Calculates the size of the SQLite database file in bytes.

- **Parameters**: None.
- **Return value**: The size of the database file in bytes, or `0` if the file does not exist.
- **Exceptions**: None.

### `public async Task<bool> VerifyConnectionAsync()`

Attempts to open and close a connection to the database to verify connectivity and file accessibility.

- **Parameters**: None.
- **Return value**: `true` if the connection was successfully opened and closed; otherwise, `false`.
- **Exceptions**: None.

### `public string GetConnectionString()`

Retrieves the current connection string used by the manager.

- **Parameters**: None.
- **Return value**: The connection string.
- **Exceptions**: None.

## Usage

### Example 1: Initialize and verify a database
