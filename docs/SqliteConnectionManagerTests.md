# SqliteConnectionManagerTests

Unit test suite that validates the behavior of the `SqliteConnectionManager` class, covering connection lifecycle, database initialization, file existence checks, deletion, and size reporting. Each test method exercises a specific public contract of the manager under both nominal and edge-case conditions.

## API

### public async Task GetConnection_ShouldReturnOpenConnection
Verifies that a connection obtained from the manager is in the open state and ready for use.  
**Parameters:** none (test method).  
**Returns:** a completed task once the assertion passes.  
**Throws:** assertion failure if the returned connection is null or its `State` is not `Open`.

### public async Task InitializeDatabaseAsync_ShouldCreateTables
Ensures that calling the initialization method results in the expected schema objects being present in the database.  
**Parameters:** none (test method).  
**Returns:** a completed task after confirming table existence.  
**Throws:** assertion failure if required tables are missing after initialization.

### public async Task VerifyConnectionAsync_ShouldReturnTrueForOpenConnection
Confirms that the verification method returns `true` when the underlying connection is healthy and open.  
**Parameters:** none (test method).  
**Returns:** a completed task; the assertion expects a `true` result.  
**Throws:** assertion failure if the method returns `false` for a valid open connection.

### public async Task VerifyConnectionAsync_ShouldReturnFalseForInvalidConnection
Confirms that the verification method returns `false` when the connection is invalid (e.g., disposed or pointing to a non-existent file).  
**Parameters:** none (test method).  
**Returns:** a completed task; the assertion expects a `false` result.  
**Throws:** assertion failure if the method returns `true` for an invalid connection.

### public void DatabaseExists_ShouldReturnFalseForNonExistentFile
Validates that the existence check returns `false` when no database file has been created.  
**Parameters:** none (test method).  
**Returns:** void; assertion expects `false`.  
**Throws:** assertion failure if the check returns `true` for a path that does not exist on disk.

### public async Task DatabaseExists_ShouldReturnTrueForCreatedFile
Validates that the existence check returns `true` after the database file has been physically created.  
**Parameters:** none (test method).  
**Returns:** a completed task; assertion expects `true`.  
**Throws:** assertion failure if the check returns `false` for a file known to exist.

### public async Task DeleteDatabase_ShouldRemoveDatabaseFile
Verifies that invoking the delete operation removes the database file from the file system.  
**Parameters:** none (test method).  
**Returns:** a completed task after confirming the file no longer exists.  
**Throws:** assertion failure if the file remains on disk after the delete call.

### public async Task GetDatabaseSize_ShouldReturnCorrectSize
Checks that the reported size matches the actual file size of a created database.  
**Parameters:** none (test method).  
**Returns:** a completed task; assertion compares the returned size against the expected file length.  
**Throws:** assertion failure if the returned size does not equal the real file size.

### public void GetDatabaseSize_ShouldReturnZeroForNonExistentDatabase
Checks that the size method returns zero when no database file exists.  
**Parameters:** none (test method).  
**Returns:** void; assertion expects `0`.  
**Throws:** assertion failure if a non-zero value is returned for a missing file.

## Usage

```csharp
// Example 1: Running the full test suite with an in-memory or temporary file-based fixture
[TestFixture]
public class DatabaseIntegrationTests
{
    private SqliteConnectionManagerTests _tests;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _tests = new SqliteConnectionManagerTests();
    }

    [Test]
    public async Task FullConnectionLifecycle_ShouldSucceed()
    {
        await _tests.GetConnection_ShouldReturnOpenConnection();
        await _tests.InitializeDatabaseAsync_ShouldCreateTables();
        await _tests.VerifyConnectionAsync_ShouldReturnTrueForOpenConnection();
    }

    [Test]
    public async Task FileManagement_ShouldReflectReality()
    {
        _tests.DatabaseExists_ShouldReturnFalseForNonExistentFile();
        await _tests.DatabaseExists_ShouldReturnTrueForCreatedFile();
        await _tests.DeleteDatabase_ShouldRemoveDatabaseFile();
    }
}
```

```csharp
// Example 2: Isolating size-related assertions in a CI pipeline
[TestFixture]
public class StorageValidationTests
{
    private SqliteConnectionManagerTests _tests;

    [SetUp]
    public void SetUp()
    {
        _tests = new SqliteConnectionManagerTests();
    }

    [Test]
    public void MissingDatabase_ShouldReportZeroSize()
    {
        _tests.GetDatabaseSize_ShouldReturnZeroForNonExistentDatabase();
    }

    [Test]
    public async Task PopulatedDatabase_ShouldReportAccurateSize()
    {
        await _tests.GetDatabaseSize_ShouldReturnCorrectSize();
    }
}
```

## Notes

- **Edge cases:** `GetDatabaseSize_ShouldReturnZeroForNonExistentDatabase` explicitly covers the scenario where the database path does not exist, ensuring callers receive a safe default rather than an exception. `VerifyConnectionAsync_ShouldReturnFalseForInvalidConnection` tests a deliberately broken or disposed connection, confirming the manager fails gracefully.
- **File system dependency:** Tests that create or delete files (`DatabaseExists_ShouldReturnTrueForCreatedFile`, `DeleteDatabase_ShouldRemoveDatabaseFile`, `GetDatabaseSize_ShouldReturnCorrectSize`) assume the test runner has write permissions in the target directory. Temporary paths or in-memory fallbacks are typically used to avoid collisions.
- **Thread safety:** All async test methods are designed to run independently. No shared mutable state is exposed between tests; each test arranges its own instance of the manager and cleans up afterward. Concurrent execution of these tests is safe under standard test runners that isolate fixtures.
- **Schema assumptions:** `InitializeDatabaseAsync_ShouldCreateTables` relies on a known set of expected table names. Any change to the manager’s initialization logic must be reflected in this test’s assertions to avoid false negatives.
