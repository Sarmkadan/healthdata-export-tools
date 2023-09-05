# InMemoryHealthDataRepositoryTests

This class contains unit tests for the `InMemoryHealthDataRepository`, an in-memory implementation of a health data repository used in the `healthdata-export-tools` project. The tests verify the correctness of CRUD operations for sleep and heart rate data, as well as record counting and deletion by date. Each test method exercises a specific repository operation and asserts the expected behavior.

## API

### `InMemoryHealthDataRepositoryTests()`
Initializes a new instance of the test class. No parameters. No return value. Does not throw.

### `public async Task AddAndGetSleepData_ReturnsCorrectData()`
Verifies that adding a sleep record and then retrieving it by identifier returns the same data.  
**Parameters:** None.  
**Return value:** A `Task` representing the asynchronous test operation.  
**Throws:** `AssertFailedException` if the retrieved record does not match the added record.

### `public async Task UpdateSleepData_ReflectsChanges()`
Verifies that updating an existing sleep record correctly persists the modified fields.  
**Parameters:** None.  
**Return value:** A `Task` representing the asynchronous test operation.  
**Throws:** `AssertFailedException` if the updated record does not reflect the changes.

### `public async Task DeleteSleepData_RemovesData()`
Verifies that deleting a sleep record removes it from the repository and subsequent retrieval returns null or throws a key-not-found exception.  
**Parameters:** None.  
**Return value:** A `Task` representing the asynchronous test operation.  
**Throws:** `AssertFailedException` if the record is still present after deletion.

### `public async Task GetSleepRange_ReturnsCorrectData()`
Verifies that querying sleep records within a date range returns only those records whose timestamps fall within the specified interval.  
**Parameters:** None.  
**Return value:** A `Task` representing the asynchronous test operation.  
**Throws:** `AssertFailedException` if the returned set does not match the expected records.

### `public async Task AddAndGetHeartRateData_ReturnsCorrectData()`
Verifies that adding a heart rate record and then retrieving it by identifier returns the same data.  
**Parameters:** None.  
**Return value:** A `Task` representing the asynchronous test operation.  
**Throws:** `AssertFailedException` if the retrieved record does not match the added record.

### `public async Task GetTotalRecordCount_ReturnsCorrectCount()`
Verifies that the repository returns the correct total number of records (sleep and heart rate combined) after insertions and deletions.  
**Parameters:** None.  
**Return value:** A `Task` representing the asynchronous test operation.  
**Throws:** `AssertFailedException` if the reported count does not match the expected count.

### `public async Task DeleteOldRecords_RemovesRecordsBeforeDate()`
Verifies that deleting records older than a given date removes only those records and preserves newer ones.  
**Parameters:** None.  
**Return value:** A `Task` representing the asynchronous test operation.  
**Throws:** `AssertFailedException` if records before the cutoff are not removed or records after the cutoff are incorrectly removed.

## Usage

The following examples demonstrate how to instantiate the test class and execute its test methods programmatically, or how to use the repository in a test fixture.

**Example 1: Running a single test directly**

```csharp
var tests = new InMemoryHealthDataRepositoryTests();
await tests.AddAndGetSleepData_ReturnsCorrectData();
```

**Example 2: Using the repository in a custom test method**

```csharp
[Fact]
public async Task CustomSleepTest()
{
    var repo = new InMemoryHealthDataRepository();
    var sleepData = new SleepData { Id = 1, StartTime = DateTime.UtcNow, Duration = TimeSpan.FromHours(8) };
    await repo.AddSleepDataAsync(sleepData);
    var retrieved = await repo.GetSleepDataAsync(1);
    Assert.Equal(sleepData.Duration, retrieved.Duration);
}
```

## Notes

- **Edge cases:** The tests cover scenarios such as an empty repository, duplicate record insertion, boundary dates for range queries, and deletion of all records when the cutoff date is in the future. Ensure that the repository handles these cases without throwing unexpected exceptions.
- **Thread safety:** The `InMemoryHealthDataRepository` is not thread-safe. Each test method creates a fresh repository instance, so tests can be run sequentially without interference. Running tests in parallel on the same instance may cause race conditions and false failures.
- **Test isolation:** The test class does not share state between methods. Each test method is self-contained and does not depend on the execution order of other tests.
