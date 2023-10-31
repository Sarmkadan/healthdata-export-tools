# InMemoryHealthDataRepositoryExtensions

Extension methods for `IHealthDataRepository` that provide in-memory implementations of common health data queries. These methods simplify access to the most recent records, aggregated values, and grouped data for health metrics like sleep, heart rate, SpO2, and steps.

## API

### `GetMostRecentSleepAsync`
Gets the most recent sleep data record from the repository.

- **Parameters**: None
- **Return value**: A `Task` resolving to the most recent `SleepData` record, or `null` if no records exist.
- **Exceptions**: Throws if the underlying repository fails to retrieve data.

### `GetMostRecentHeartRateAsync`
Gets the most recent heart rate data record from the repository.

- **Parameters**: None
- **Return value**: A `Task` resolving to the most recent `HeartRateData` record, or `null` if no records exist.
- **Exceptions**: Throws if the underlying repository fails to retrieve data.

### `GetMostRecentSpO2Async`
Gets the most recent SpO2 (blood oxygen saturation) data record from the repository.

- **Parameters**: None
- **Return value**: A `Task` resolving to the most recent `SpO2Data` record, or `null` if no records exist.
- **Exceptions**: Throws if the underlying repository fails to retrieve data.

### `GetMostRecentStepsAsync`
Gets the most recent steps data record from the repository.

- **Parameters**: None
- **Return value**: A `Task` resolving to the most recent `StepsData` record, or `null` if no records exist.
- **Exceptions**: Throws if the underlying repository fails to retrieve data.

### `GetAverageHeartRateAsync`
Calculates the average heart rate across all stored records.

- **Parameters**: None
- **Return value**: A `Task` resolving to the average heart rate as an `int`, or `null` if no records exist.
- **Exceptions**: Throws if the underlying repository fails to retrieve data.

### `GetTotalStepsAsync`
Calculates the total number of steps across all stored records.

- **Parameters**: None
- **Return value**: A `Task` resolving to the total steps as an `int`, or `null` if no records exist.
- **Exceptions**: Throws if the underlying repository fails to retrieve data.

### `GetAverageSpO2Async`
Calculates the average SpO2 (blood oxygen saturation) across all stored records.

- **Parameters**: None
- **Return value**: A `Task` resolving to the average SpO2 as an `int`, or `null` if no records exist.
- **Exceptions**: Throws if the underlying repository fails to retrieve data.

### `HasDataForDateAsync`
Checks whether any health data exists for a given date.

- **Parameters**:
  - `date` (`DateOnly`): The date to check.
- **Return value**: A `Task<bool>` resolving to `true` if data exists for the date, otherwise `false`.
- **Exceptions**: Throws if the underlying repository fails to check data.

### `GetLatestRecordAsync`
Gets the latest health data record of any type from the repository.

- **Parameters**: None
- **Return value**: A `Task` resolving to a tuple containing the `HealthDataType` and the record object, or `null` if no records exist.
- **Exceptions**: Throws if the underlying repository fails to retrieve data.

### `GetRecordsByDateGroupedAsync`
Groups all health data records by date and data type.

- **Parameters**: None
- **Return value**: A `Task<Dictionary<HealthDataType, List<object>>>` resolving to a dictionary mapping each `HealthDataType` to a list of its records, grouped by date.
- **Exceptions**: Throws if the underlying repository fails to retrieve data.

## Usage
