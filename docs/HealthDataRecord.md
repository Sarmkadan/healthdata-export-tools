# HealthDataRecord

Represents a single health data record captured from a device. This abstract class provides common metadata (identifiers, timestamps, device information) and defines a contract for validation and summary generation. Derived types implement device‑specific logic for `IsValid` and `GetSummary`, while shared behaviors like marking as validated and updating modification timestamps are provided as virtual members.

## API

- **`public string Id`**  
  Gets or sets the unique identifier for this record. Typically assigned by the system when the record is created.

- **`public DateTime CreatedUtc`**  
  Gets or sets the UTC timestamp when the record was first created.

- **`public DateTime ModifiedUtc`**  
  Gets or sets the UTC timestamp of the last modification to the record. Updated automatically by `Touch()` and `MarkAsValidated()`.

- **`public DateTime RecordDate`**  
  Gets or sets the date and time (in UTC) when the health data was actually measured or recorded by the device.

- **`public string DeviceId`**  
  Gets or sets the identifier of the device that generated the data.

- **`public string? FirmwareVersion`**  
  Gets or sets the firmware version of the device at the time of recording. Can be `null` if the version is unknown.

- **`public bool IsValidated`**  
  Gets or sets a value indicating whether the record has been manually or programmatically validated. The default is `false`.

- **`public string? Notes`**  
  Gets or sets optional free‑text notes associated with the record. Can be `null`.

- **`public virtual void MarkAsValidated()`**  
  Marks the record as validated. The default implementation sets `IsValidated` to `true` and updates `ModifiedUtc` to the current UTC time.  
  *Throws*: None.

- **`public abstract bool IsValid`**  
  When implemented in a derived class, returns `true` if the record’s data passes all device‑specific validation rules; otherwise `false`.  
  *Throws*: None (validation logic should not throw; return `false` on failure).

- **`public abstract Dictionary<string, object> GetSummary()`**  
  When implemented in a derived class, returns a dictionary of key‑value pairs summarizing the record’s data (e.g., measurement values, device status). The keys are strings; values can be any object suitable for serialization.  
  *Throws*: None.

- **`public virtual void Touch()`**  
  Updates the `ModifiedUtc` property to the current UTC time. Typically called when any non‑trivial change is made to the record.  
  *Throws*: None.

## Usage

### Example 1: Creating and validating a heart‑rate record

```csharp
public class HeartRateRecord : HealthDataRecord
{
    public int Bpm { get; set; }

    public override bool IsValid => Bpm > 30 && Bpm < 250;

    public override Dictionary<string, object> GetSummary()
    {
        return new Dictionary<string, object>
        {
            ["type"] = "heart_rate",
            ["bpm"] = Bpm,
            ["device"] = DeviceId
        };
    }
}

// Usage
var record = new HeartRateRecord
{
    Id = Guid.NewGuid().ToString(),
    CreatedUtc = DateTime.UtcNow,
    RecordDate = DateTime.UtcNow.AddMinutes(-5),
    DeviceId = "HR-123",
    FirmwareVersion = "2.1.0",
    Bpm = 72
};

if (record.IsValid)
{
    record.MarkAsValidated();
    var summary = record.GetSummary();
    Console.WriteLine($"Validated record {record.Id} with BPM {summary["bpm"]}");
}
```

### Example 2: Updating notes and using Touch

```csharp
var record = new HeartRateRecord
{
    Id = "rec-001",
    CreatedUtc = DateTime.UtcNow,
    RecordDate = DateTime.UtcNow,
    DeviceId = "HR-123"
};

// Later, add a note
record.Notes = "Patient was resting";
record.Touch();  // Updates ModifiedUtc

// Export summary
var exportData = record.GetSummary();
exportData["notes"] = record.Notes;
```

## Notes

- **Thread safety**: Instances of `HealthDataRecord` are not thread‑safe. Concurrent reads and writes from multiple threads must be synchronized externally (e.g., with a lock).
- **Nullability**: `FirmwareVersion` and `Notes` are nullable. Code that consumes these properties should check for `null` before use, especially when serializing or formatting output.
- **`Id` uniqueness**: The `Id` property is a plain string; no uniqueness constraint is enforced by the base class. Callers are responsible for assigning distinct identifiers (e.g., GUIDs).
- **`MarkAsValidated` and `Touch`**: Both virtual methods update `ModifiedUtc`. Overrides in derived classes should call the base implementation or otherwise ensure the timestamp is refreshed.
- **Abstract members**: `IsValid` and `GetSummary` must be implemented in non‑abstract derived classes. Failure to do so will cause a compile‑time error.
- **`RecordDate` vs. timestamps**: `RecordDate` represents the measurement time, while `CreatedUtc` and `ModifiedUtc` track system‑level lifecycle. They may differ significantly if data is imported or backfilled.
