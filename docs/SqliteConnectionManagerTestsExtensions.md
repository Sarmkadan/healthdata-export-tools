# SqliteConnectionManagerTestsExtensions

Utility class providing extension methods for `SqliteConnectionManager` to facilitate test setup, validation, and data verification in unit tests. These methods simplify common SQLite database operations required when testing data access components.

## API

### `VerifyDatabaseInitializedAsync`

Determines whether the SQLite database has been initialized and is ready for operations.

- **Parameters**
  - `connectionManager` (`SqliteConnectionManager`): The connection manager instance to verify.
- **Return value**
  - `Task<bool>`: `true` if the database is initialized; otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentNullException` if `connectionManager` is `null`.

### `CreateTestDatabaseWithSampleDataAsync`

Creates a new SQLite database in memory with a predefined schema and inserts sample data for testing purposes.

- **Parameters**
  - `connectionManager` (`SqliteConnectionManager`): The connection manager to use for database creation.
- **Return value**
  - `Task`: A task representing the asynchronous operation.
- **Exceptions**
  - Throws `ArgumentNullException` if `connectionManager` is `null`.
  - Throws `InvalidOperationException` if the database cannot be initialized or sample data cannot be inserted.

### `GetTableRecordCountAsync`

Retrieves the number of records in the specified table.

- **Parameters**
  - `connectionManager` (`SqliteConnectionManager`): The connection manager to use for querying.
  - `tableName` (`string`): The name of the table to count records in.
- **Return value**
  - `Task<long>`: The number of records in the table.
- **Exceptions**
  - Throws `ArgumentNullException` if `connectionManager` is `null` or if `tableName` is `null`.
  - Throws `SqliteException` if the table does not exist or if a database error occurs.

### `VerifyDatabaseSize`

Checks whether the SQLite database file size meets a minimum expected size threshold.

- **Parameters**
  - `connectionManager` (`SqliteConnectionManager`): The connection manager associated with the database.
  - `minSizeBytes` (`long`): The minimum expected size in bytes.
- **Return value**
  - `bool`: `true` if the database file size is greater than or equal to `minSizeBytes`; otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentNullException` if `connectionManager` is `null`.
  - Throws `InvalidOperationException` if the database file path cannot be resolved.

## Usage

### Example 1: Verifying database initialization and size
