# CliArgumentParser

A utility class for parsing and validating command-line arguments in .NET console applications. It provides structured access to command-line options while enforcing type safety and usage constraints.

## API

### `public CliArgumentParser`

Initializes a new instance of the `CliArgumentParser` class. This constructor prepares the parser to process command-line arguments according to the expected schema defined by `CliOptions`.

### `public CliOptions Parse(string[] args)`

Parses the provided command-line arguments into a strongly-typed `CliOptions` object.

- **Parameters**:
  - `args`: An array of strings representing the command-line arguments, typically obtained from `Environment.GetCommandLineArgs()` or similar.

- **Return Value**:
  - A populated `CliOptions` instance containing the parsed values. If no arguments are provided, default values from `CliOptions` are used.

- **Exceptions**:
  - Throws `ArgumentException` if the arguments are malformed or do not conform to the expected format.

### `public List<string> Validate()`

Validates the parsed `CliOptions` instance and returns a list of error messages if validation fails.

- **Return Value**:
  - An empty list if validation succeeds. Otherwise, a list of human-readable error messages describing validation failures.

- **Exceptions**:
  - Does not throw exceptions directly; errors are returned as a list of strings.

### `public static string GetHelpText()`

Generates and returns a formatted help text string that describes the available command-line options, their usage, and examples.

- **Return Value**:
  - A string containing the help documentation suitable for display to users.

## Usage

### Example 1: Basic Parsing and Validation
