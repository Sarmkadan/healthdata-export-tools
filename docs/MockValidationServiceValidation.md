# MockValidationServiceValidation

MockValidationServiceValidation is a static utility class that provides mock validation capabilities for testing and development scenarios. It exposes multiple overloads of the `Validate` method, a `IsValid` property to query the outcome of the most recent validation, and an `EnsureValid` method to enforce validity by throwing an exception when errors are present.

## API

### `Validate` (multiple overloads)

Each overload of the `Validate` method performs validation on a set of inputs and returns an `IReadOnlyList<string>` containing zero or more error messages. When validation succeeds, the returned list is empty. The exact parameters accepted by each overload are defined by the implementation; all overloads share the same return type and general contract.

- **Return value**: `IReadOnlyList<string>` – a list of validation error messages. An empty list indicates that all validation checks passed.

### `IsValid`

`public static bool IsValid`

Gets a value indicating whether the most recent call to any `Validate` overload succeeded (i.e., returned an empty list). The value is `true` if no validation errors were reported, and `false` otherwise.

- **Return value**: `bool` – `true` if the last validation passed; `false` if errors were found.

### `EnsureValid`

`public static void EnsureValid`

Throws an exception if the current validation state is invalid. The exception type and message are determined by the implementation. This method is typically called after a `Validate` invocation to halt execution when validation fails.

- **Throws**: An exception (e.g., `InvalidOperationException`) when `IsValid` is `false`.

## Usage

### Example 1: Validating input and checking validity

```csharp
// Assume Validate overload that accepts a string
string userInput = "some data";
MockValidationServiceValidation.Validate(userInput);

if (MockValidationServiceValidation.IsValid)
{
    Console.WriteLine("Input is valid.");
}
else
{
    Console.WriteLine("Validation failed.");
}
```

### Example 2: Using EnsureValid to enforce validation

```csharp
// Assume Validate overload that accepts multiple parameters
var record = new { Id = 42, Name = "Test" };
MockValidationServiceValidation.Validate(record.Id, record.Name);

try
{
    MockValidationServiceValidation.EnsureValid();
    Console.WriteLine("Record is valid, proceeding...");
}
catch (Exception ex)
{
    Console.WriteLine($"Validation error: {ex.Message}");
}
```

## Notes

- **Thread safety**: All members are static. If the underlying validation logic is not thread‑safe, concurrent calls to `Validate` from multiple threads may produce inconsistent results or corrupt internal state. The `IsValid` property reflects the outcome of the most recent `Validate` call, which can change unpredictably in a multi‑threaded context. Consider synchronizing access or using a dedicated instance per thread if thread safety is required.
- **Statefulness**: The `IsValid` property and `EnsureValid` method depend on the state set by the last `Validate` call. Calling `EnsureValid` without a prior `Validate` invocation may throw or behave unexpectedly, depending on the implementation.
- **Empty validation**: When all `Validate` overloads return an empty list, `IsValid` is `true` and `EnsureValid` does nothing. This is the expected behavior for valid inputs.
- **Error messages**: The strings returned by `Validate` are implementation‑defined. They may be localized, contain parameter values, or follow a specific format. Do not rely on the exact wording for programmatic decisions.
