# ChartExportOptions

`ChartExportOptions` is a configuration object that controls the content and appearance of a health-data HTML chart export. It specifies which chart components to include, whether a summary table should be rendered, and provides the title for the generated report. The class also exposes an asynchronous method that performs the actual export to an HTML file containing the requested charts.

## API

### `public string Title`

Gets or sets the title string displayed at the top of the exported HTML report. This value is written directly into the output document and is not automatically sanitized.

**Remarks:** If set to `null` or an empty string, the generated report will contain an empty or missing title element. No default is applied by this class.

---

### `public bool IncludeSummaryTable`

Gets or sets a value indicating whether a summary statistics table is rendered in the export. When `true`, the export includes aggregated metrics (e.g., averages, minimums, maximums) above or alongside the charts.

**Remarks:** The summary table draws from the same underlying data as the charts. Setting this to `false` suppresses the table entirely.

---

### `public bool IncludeSpO2Chart`

Gets or sets a value indicating whether the SpO₂ (blood oxygen saturation) chart is included in the export.

**Remarks:** If the data source contains no SpO₂ readings, the chart area may still render as an empty placeholder or with a "no data" indication, depending on the export implementation.

---

### `public bool IncludeActivityChart`

Gets or sets a value indicating whether the activity chart (steps, movement, or equivalent activity metric) is included in the export.

**Remarks:** When `false`, the activity section is omitted from the HTML output entirely.

---

### `public bool IncludeSleepCompositionChart`

Gets or sets a value indicating whether the sleep composition chart (e.g., deep, light, REM sleep breakdown) is included in the export.

**Remarks:** This flag only controls inclusion; it does not validate that sleep data exists in the source.

---

### `public Task ExportToHtmlChartsAsync`

Initiates the asynchronous export operation using the current property values and returns a `Task` representing the ongoing work. This overload does not accept parameters and relies entirely on the instance's configured state.

**Returns:** A `Task` that completes when the HTML file has been written.

**Exceptions:**
- `InvalidOperationException` – Thrown if the export cannot proceed because required underlying data is unavailable or the instance is in an invalid state (e.g., no output path has been configured externally).
- `IOException` – Thrown if file writing fails due to permissions, disk space, or path issues.

---

### `public async Task ExportToHtmlChartsAsync`

An `async`-annotated overload that performs the same operation as the parameterless `ExportToHtmlChartsAsync` but is declared with the `async` keyword, indicating it uses `await` internally. The behavior, return type, and exception profile are identical.

**Returns:** A `Task` that completes when the HTML file has been written.

**Exceptions:** Same as the non-`async` overload.

## Usage

### Example 1: Full export with all charts and summary

```csharp
var options = new ChartExportOptions
{
    Title = "Weekly Health Report",
    IncludeSummaryTable = true,
    IncludeSpO2Chart = true,
    IncludeActivityChart = true,
    IncludeSleepCompositionChart = true
};

try
{
    await options.ExportToHtmlChartsAsync();
    Console.WriteLine("Report generated successfully.");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Configuration error: {ex.Message}");
}
catch (IOException ex)
{
    Console.WriteLine($"File write error: {ex.Message}");
}
```

### Example 2: Minimal export with only activity data

```csharp
var options = new ChartExportOptions
{
    Title = "Activity Summary",
    IncludeSummaryTable = false,
    IncludeSpO2Chart = false,
    IncludeActivityChart = true,
    IncludeSleepCompositionChart = false
};

Task exportTask = options.ExportToHtmlChartsAsync();
await exportTask;
```

## Notes

- **Thread safety:** Instance properties (`Title`, `Include*` booleans) are not synchronized. Changing property values on one thread while `ExportToHtmlChartsAsync` is executing on another thread may result in an inconsistent export (e.g., a chart appearing despite its flag being toggled mid-operation). External synchronization is required if concurrent access is expected.
- **Edge cases:** If all chart flags are set to `false` and `IncludeSummaryTable` is also `false`, the export may produce an HTML document containing only the title and structural markup with no data sections. This is not treated as an error unless the implementation explicitly validates that at least one section is enabled.
- **Title handling:** The `Title` value is embedded verbatim. Callers must perform any necessary HTML escaping if the title originates from user input to avoid malformed output or injection risks.
- **Data dependency:** The export methods do not accept data parameters. They rely on data that is either injected via constructor (not shown here) or resolved from an ambient context. If that data is missing or incompatible with the requested charts, an `InvalidOperationException` is thrown.
- **File output:** The destination file path is not a property of this class. It is assumed to be provided through other means (e.g., constructor injection, a static configuration, or a method parameter in derived overloads). Ensure the path is set and accessible before calling the export methods.
