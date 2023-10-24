# XmlFormatter

The `XmlFormatter` class provides specialized XML serialization and validation capabilities for health data entities such as heart rate, SpO₂, steps, and sleep data. It supports both single-object and collection formatting, as well as schema validation for exported health data.

## API

### `public XmlFormatter()`

Initializes a new instance of the `XmlFormatter` class with default configuration.

### `public bool CanFormat`

Gets a value indicating whether the formatter is capable of formatting health data.

- **Return value**: `true` if the formatter can format health data; otherwise, `false`.

### `public async Task<string> FormatAsync(object data)`

Formats a single health data object into an XML string.

- **Parameters**:
  - `data`: The health data object to format. Must not be `null`.
- **Return value**: A `Task<string>` that resolves to the XML representation of the input data.
- **Exceptions**:
  - `ArgumentNullException`: Thrown if `data` is `null`.
  - `InvalidOperationException`: Thrown if the data type is not supported for formatting.

### `public async Task<string> FormatCollectionAsync(IEnumerable<object> data)`

Formats a collection of health data objects into a single XML string.

- **Parameters**:
  - `data`: The collection of health data objects to format. Must not be `null` and must not contain `null` elements.
- **Return value**: A `Task<string>` that resolves to the XML representation of the input collection.
- **Exceptions**:
  - `ArgumentNullException`: Thrown if `data` is `null`.
  - `ArgumentException`: Thrown if the collection contains `null` elements.
  - `InvalidOperationException`: Thrown if any data type in the collection is not supported for formatting.

### `public async Task<string> FormatSleepDataAsync(SleepData data)`

Formats a `SleepData` object into an XML string.

- **Parameters**:
  - `data`: The sleep data object to format. Must not be `null`.
- **Return value**: A `Task<string>` that resolves to the XML representation of the sleep data.
- **Exceptions**:
  - `ArgumentNullException`: Thrown if `data` is `null`.

### `public async Task<string> FormatHeartRateDataAsync(HeartRateData data)`

Formats a `HeartRateData` object into an XML string.

- **Parameters**:
  - `data`: The heart rate data object to format. Must not be `null`.
- **Return value**: A `Task<string>` that resolves to the XML representation of the heart rate data.
- **Exceptions**:
  - `ArgumentNullException`: Thrown if `data` is `null`.

### `public async Task<string> FormatSpO2DataAsync(SpO2Data data)`

Formats a `SpO2Data` object into an XML string.

- **Parameters**:
  - `data`: The SpO₂ data object to format. Must not be `null`.
- **Return value**: A `Task<string>` that resolves to the XML representation of the SpO₂ data.
- **Exceptions**:
  - `ArgumentNullException`: Thrown if `data` is `null`.

### `public async Task<string> FormatStepsDataAsync(StepsData data)`

Formats a `StepsData` object into an XML string.

- **Parameters**:
  - `data`: The steps data object to format. Must not be `null`.
- **Return value**: A `Task<string>` that resolves to the XML representation of the steps data.
- **Exceptions**:
  - `ArgumentNullException`: Thrown if `data` is `null`.

### `public async Task<List<string>> ValidateAsync(string xml)`

Validates an XML string against the health data schema.

- **Parameters**:
  - `xml`: The XML string to validate. Must not be `null` or empty.
- **Return value**: A `Task<List<string>>` that resolves to a list of validation errors. If the list is empty, the XML is valid.
- **Exceptions**:
  - `ArgumentException`: Thrown if `xml` is `null` or empty.
