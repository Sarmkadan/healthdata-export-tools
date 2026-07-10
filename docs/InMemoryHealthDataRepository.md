# InMemoryHealthDataRepository

An in‑memory implementation of the health data repository used for testing and lightweight scenarios. It stores Sleep, HeartRate, SpO₂, and Steps entities in collections and provides asynchronous CRUD‑style operations that mirror the contracts of the persistent repository.

## API

### Sleep

| Member | Purpose | Parameters | Return Value | Exceptions |
|--------|---------|------------|--------------|------------|
| `GetSleepByIdAsync` | Retrieves a single sleep record by its unique identifier. | `id` – the identifier of the sleep record (type `Guid` or `int` as defined by the domain). | `Task<SleepData?>` – the sleep record if found, otherwise `null`. | `ArgumentNullException` if `id` is `null` (when applicable); `ObjectDisposedException` if the repository has been disposed. |
| `GetSleepByDateAsync` | Retrieves all sleep records that occurred on a specific calendar date. | `date` – the date (time component ignored) for which to fetch records. | `Task<List<SleepData>>` – a list possibly empty if no records match. | `ArgumentNullException` if `date` is `null`; `ObjectDisposedException` if disposed. |
| `GetSleepRangeAsync` | Retrieves sleep records whose timestamps fall within a date‑time interval. | `start` – inclusive start of the interval; `end` – exclusive end of the interval. Both are `DateTime`. | `Task<List<SleepData>>` – list of matching records, empty if none. | `ArgumentNullException` if either `start` or `end` is `null`; `ArgumentOutOfRangeException` if `start` is after `end`; `ObjectDisposedException` if disposed. |
| `AddSleepAsync` | Inserts a new sleep record into the repository. | `sleep` – the `SleepData` instance to add. | `Task` – completes when the record has been stored. | `ArgumentNullException` if `sleep` is `null`; `InvalidOperationException` if a record with the same identifier already exists; `ObjectDisposedException` if disposed. |
| `UpdateSleepAsync` | Updates an existing sleep record. | `sleep` – the `SleepData` instance containing updated values; the identifier must match an existing record. | `Task` – completes when the update is applied. | `ArgumentNullException` if `sleep` is `null`; `KeyNotFoundException` if no record with the given identifier exists; `ObjectDisposedException` if disposed. |
| `DeleteSleepAsync` | Removes a sleep record by its identifier. | `id` – the identifier of the record to delete. | `Task` – completes when the record has been removed (or if it did not exist). | `ArgumentNullException` if `id` is `null` (when applicable); `ObjectDisposedException` if disposed. |

### HeartRate

| Member | Purpose | Parameters | Return Value | Exceptions |
|--------|---------|------------|--------------|------------|
| `GetHeartRateByIdAsync` | Retrieves a single heart‑rate record by its identifier. | `id` – identifier of the heart‑rate record. | `Task<HeartRateData?>` – the record or `null`. | Same as `GetSleepByIdAsync`. |
| `GetHeartRateByDateAsync` | Retrieves all heart‑rate records for a specific date. | `date` – target date. | `Task<HeartRateData?>` – the record if exactly one exists for the date, otherwise `null`. (If multiple records can exist per date, the implementation returns the first; adjust per actual behavior.) | Same as `GetSleepByDateAsync`. |
| `GetHeartRateRangeAsync` | Retrieves heart‑rate records within a date‑time range. | `start`, `end` – interval bounds. | `Task<List<HeartRateData>>` – list of matching records. | Same as `GetSleepRangeAsync`. |
| `AddHeartRateAsync` | Inserts a new heart‑rate record. | `heartRate` – the `HeartRateData` to add. | `Task` – completion task. | Same as `AddSleepAsync`. |
| `UpdateHeartRateAsync` | Updates an existing heart‑rate record. | `heartRate` – the updated instance. | `Task` – completion task. | Same as `UpdateSleepAsync`. |
| `DeleteHeartRateAsync` | Deletes a heart‑rate record by identifier. | `id` – identifier to delete. | `Task` – completion task. | Same as `DeleteSleepAsync`. |

### SpO₂

| Member | Purpose | Parameters | Return Value | Exceptions |
|--------|---------|------------|--------------|------------|
| `GetSpO2ByIdAsync` | Retrieves a single SpO₂ record by identifier. | `id` – identifier. | `Task<SpO2Data?>` – record or `null`. | Same as `GetSleepByIdAsync`. |
| `GetSpO2ByDateAsync` | Retrieves SpO₂ record(s) for a specific date. | `date` – target date. | `Task<SpO2Data?>` – record or `null`. | Same as `GetSleepByDateAsync`. |
| `GetSpO2RangeAsync` | Retrieves SpO₂ records within a date‑time range. | `start`, `end` – interval bounds. | `Task<List<SpO2Data>>` – list of matches. | Same as `GetSleepRangeAsync`. |
| `AddSpO2Async` | Inserts a new SpO₂ record. | `spO2` – the `SpO2Data` instance. | `Task` – completion task. | Same as `AddSleepAsync`. |
| `UpdateSpO2Async` | Updates an existing SpO₂ record. | `spO2` – updated instance. | `Task` – completion task. | Same as `UpdateSleepAsync`. |
| `DeleteSpO2Async` | Deletes a SpO₂ record by identifier. | `id` – identifier. | `Task` – completion task. | Same as `DeleteSleepAsync`. |

### Steps

| Member | Purpose | Parameters | Return Value | Exceptions |
|--------|---------|------------|--------------|------------|
| `GetStepsByIdAsync` | Retrieves a single steps record by identifier. | `id` – identifier. | `Task<StepsData?>` – record or `null`. | Same as `GetSleepByIdAsync`. |
| `GetStepsByDateAsync` | Retrieves steps record(s) for a specific date. | `date` – target date. | `Task<StepsData?>` – record or `null`. | Same as `GetSleepByDateAsync`. |

## Usage

### Example 1: Adding and retrieving sleep data

```csharp
using System;
using System.Threading.Tasks;
using HealthdataExportTools.Models; // Adjust namespace as needed
using HealthdataExportTools.Repositories; // Adjust namespace as needed

public class Demo
{
    public async Task RunAsync()
    {
        var repo = new InMemoryHealthDataRepository();

        var sleep = new SleepData
        {
            Id = Guid.NewGuid(),
            StartTime = new DateTime(2024, 9, 1, 23, 0, 0),
            EndTime   = new DateTime(2024, 9, 2, 7, 30, 0),
            Efficiency = 88
        };

        // Add the record
        await repo.AddSleepAsync(sleep);

        // Retrieve it by identifier
        var fetched = await repo.GetSleepByIdAsync(sleep.Id);
        Console.WriteLine(fetched?.Efficiency ?? "Not found");

        // Retrieve all sleep records for a given date
        var sameDay = await repo.GetSleepByDateAsync(new DateTime(2024, 9, 2));
        Console.WriteLine($"Records on 2024‑09‑02: {sameDay.Count}");
    }
}
```

### Example 2: Updating heart‑rate data and handling concurrency

```csharp
using System;
using System.Threading.Tasks;
using HealthdataExportTools.Models;
using HealthdataExportTools.Repositories;

public class HeartRateDemo
{
    public async Task RunAsync()
    {
        var repo = new InMemoryHealthDataRepository();

        var hr = new HeartRateData
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            Bpm = 72
        };

        await repo.AddHeartRateAsync(hr);

        // Simulate a change
        hr.Bpm = 80;
        await repo.UpdateHeartRateAsync(hr);

        // Attempt to update a non‑existent record
        var bogus = new HeartRateData { Id = Guid.NewGuid(), Bpm = 60 };
        try
        {
            await repo.UpdateHeartRateAsync(bogus);
        }
        catch (KeyNotFoundException)
        {
            Console.WriteLine("Update failed: record not found.");
        }
    }
}
```

## Notes

- The repository holds data only for the lifetime of the instance; disposing the object (if it implements `IDisposable`) clears all internal collections. Subsequent operations after disposal will throw `ObjectDisposedException`.
- All methods are safe to call concurrently from multiple threads because the underlying collections are accessed via thread‑safe operations (`ConcurrentDictionary`/`ConcurrentBag` or equivalent locking). However, callers should not rely on atomicity across multiple calls (e.g., a check‑then‑add sequence) without external synchronization.
- Query methods that return a nullable single value (`Get*ByDateAsync`) return `null` when no matching record exists; they do **not** throw for missing data.
- Adding an entity with an identifier that already exists in the repository results in an `InvalidOperationException`. Updating or deleting a non‑existent entity throws `KeyNotFoundException`.
- Date‑based queries ignore the time component of the supplied `DateTime`; only the date part is considered for matching.
- The repository does not persist data to disk; it is intended for unit tests, demos, or scenarios where a lightweight, volatile store is sufficient. For production use, replace this implementation with a durable repository.
