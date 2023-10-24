# JsonFormatter

A formatter class that converts health data objects into JSON strings, supporting individual records, collections, and domain-specific health metrics such as sleep, heart rate, blood oxygen saturation (SpO2), and step counts.

## API

### `public JsonFormatter()`

Initializes a new instance of the `JsonFormatter` class.

### `public bool CanFormat`

Gets a value indicating whether the formatter is capable of formatting the current data context.

- **Return value**: `true` if the formatter can process the data; otherwise, `false`.

### `public async Task<string> FormatAsync()`

Formats the current health data object into a JSON string.

- **Return value**: A `Task<string>` representing the JSON representation of the data.
- **Exceptions**: Throws `InvalidOperationException` if the data is null or incompatible with the formatter.

### `public async Task<string> FormatCollectionAsync()`

Formats a collection of health data objects into a JSON string.

- **Return value**: A `Task<string>` containing the JSON array of formatted objects.
- **Exceptions**: Throws `InvalidOperationException` if the collection is null or empty.

### `public async Task<string> FormatSleepDataAsync()`

Formats sleep data into a JSON string.

- **Return value**: A `Task<string>` containing the JSON representation of the sleep data.
- **Exceptions**: Throws `InvalidOperationException` if the sleep data is null or invalid.

### `public async Task<string> FormatHeartRateDataAsync()`

Formats heart rate data into a JSON string.

- **Return value**: A `Task<string>` containing the JSON representation of the heart rate data.
- **Exceptions**: Throws `InvalidOperationException` if the heart rate data is null or invalid.

### `public async Task<string> FormatSpO2DataAsync()`

Formats blood oxygen saturation (SpO2) data into a JSON string.

- **Return value**: A `Task<string>` containing the JSON representation of the SpO2 data.
- **Exceptions**: Throws `InvalidOperationException` if the SpO2 data is null or invalid.

### `public async Task<string> FormatStepsDataAsync()`

Formats step count data into a JSON string.

- **Return value**: A `Task<string>` containing the JSON representation of the step data.
- **Exceptions**: Throws `InvalidOperationException` if the step data is null or invalid.

### `public async Task<List<string>> ValidateAsync()`

Validates the current health data object and returns a list of validation messages.

- **Return value**: A `Task<List<string>>` containing validation error messages. An empty list indicates successful validation.
- **Exceptions**: Throws `InvalidOperationException` if the data context is invalid for validation.

## Usage

### Example 1: Formatting a single health record
