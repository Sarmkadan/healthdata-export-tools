# FileUtility

Utility class providing static asynchronous and synchronous helpers for common file‑system operations in the healthdata‑export‑tools project.

## API

### `public static async Task<string> ReadFileAsync(string path)`
- **Purpose:** Reads the entire contents of a text file asynchronously.
- **Parameters:** `path` – Full or relative path to the file to read.
- **Return Value:** The file contents as a string.
- **Exceptions:** 
  - `ArgumentNullException` if `path` is null.
  - `ArgumentException` if `path` is empty or whitespace.
  - `FileNotFoundException` if the file does not exist.
  - `UnauthorizedAccessException` if the caller lacks permission.
  - `IOException` for other I/O errors.

### `public static async Task WriteFileAsync(string path, string content)`
- **Purpose:** Writes a string to a file asynchronously, creating the file if it does not exist or overwriting it otherwise.
- **Parameters:** 
  - `path` – Destination file path.
  - `content` – Text to write.
- **Return Value:** None.
- **Exceptions:** 
  - `ArgumentNullException` if `path` or `content` is null.
  - `ArgumentException` if `path` is empty or whitespace.
  - `UnauthorizedAccessException` if the directory is not writable.
  - `IOException` for I/O failures.

### `public static List<string> GetFilesRecursive(string directoryPath, string searchPattern = "*", SearchOption searchOption = SearchOption.AllDirectories)`
- **Purpose:** Returns a list of file paths matching a search pattern, optionally recursing into sub‑directories.
- **Parameters:** 
  - `directoryPath` – Root directory to search.
  - `searchPattern` – Search string (default `"*"`).
  - `searchOption` – Whether to search sub‑directories (default `AllDirectories`).
- **Return Value:** List of full file paths.
- **Exceptions:** 
  - `ArgumentNullException` if `directoryPath` is null.
  - `DirectoryNotFoundException` if the directory does not exist.
  - `UnauthorizedAccessException` if access to a directory is denied.

### `public static bool DeleteFile(string path)`
- **Purpose:** Deletes the specified file.
- **Parameters:** `path` – File to delete.
- **Return Value:** `true` if the file was deleted; `false` if the file did not exist.
- **Exceptions:** 
  - `ArgumentNullException` if `path` is null.
  - `UnauthorizedAccessException` if the file is read‑only or the caller lacks delete permission.
  - `IOException` for other deletion errors.

### `public static long GetFileSize(string path)`
- **Purpose:** Retrieves the size of a file in bytes.
- **Parameters:** `path` – File to measure.
- **Return Value:** File size as a signed 64‑bit integer.
- **Exceptions:** 
  - `ArgumentNullException` if `path` is null.
  - `FileNotFoundException` if the file does not exist.
  - `UnauthorizedAccessException` if the file cannot be accessed.

### `public static bool IsFileReadable(string path)`
- **Purpose:** Checks whether the file exists and can be opened for reading.
- **Parameters:** `path` – File to test.
- **Return Value:** `true` if the file is readable; otherwise `false`.
- **Exceptions:** 
  - `ArgumentNullException` if `path` is null.
  - `UnauthorizedAccessException` is caught internally and results in a `false` return rather than being propagated.

### `public static string GenerateUniqueFilePath(string directoryPath, string fileName)`
- **Purpose:** Creates a unique file path by appending a numeric suffix if a file with the same name already exists.
- **Parameters:** 
  - `directoryPath` – Directory where the file will reside.
  - `fileName` – Desired file name (including extension).
- **Return Value:** A full path that does not correspond to an existing file.
- **Exceptions:** 
  - `ArgumentNullException` if either argument is null.
  - `ArgumentException` if `directoryPath` or `fileName` is empty/whitespace.
  - `DirectoryNotFoundException` if the directory does not exist.

### `public static async Task CopyDirectoryAsync(string sourceDir, string destDir)`
- **Purpose:** Asynchronously copies an entire directory tree to a new location.
- **Parameters:** 
  - `sourceDir` – Directory to copy.
  - `destDir` – Target directory (will be created if it does not exist).
- **Return Value:** None.
- **Exceptions:** 
  - `ArgumentNullException` if either path is null.
  - `DirectoryNotFoundException` if `sourceDir` does not exist.
  - `UnauthorizedAccessException` if access to source or destination is denied.
  - `IOException` for copy failures (e.g., file in use).

### `public static async Task<string> GetFileSha256HashAsync(string path)`
- **Purpose:** Computes the SHA‑256 hash of a file asynchronously.
- **Parameters:** `path` – File to hash.
- **Return Value:** Hexadecimal string representation of the hash.
- **Exceptions:** 
  - `ArgumentNullException` if `path` is null.
  - `FileNotFoundException` if the file does not exist.
  - `UnauthorizedAccessException` if the file cannot be opened for reading.
  - `IOException` for other I/O errors.

### `public static async Task<List<string>> ReadLinesAsync(string path)`
- **Purpose:** Reads all lines of a text file asynchronously into a list.
- **Parameters:** `path` – File to read.
- **Return Value:** List containing each line of the file (without line terminators).
- **Exceptions:** Same as `ReadFileAsync`.

### `public static async Task WriteLinesAsync(string path, IEnumerable<string> lines)`
- **Purpose:** Writes a collection of strings to a file, each string on its own line, asynchronously.
- **Parameters:** 
  - `path` – Destination file.
  - `lines` – Enumeration of lines to write.
- **Return Value:** None.
- **Exceptions:** Same as `WriteFileAsync`; additionally throws `ArgumentNullException` if `lines` is null.

### `public static List<string> GetRecentFiles(int count = 10)`
- **Purpose:** Returns the most recently accessed files tracked by the application (implementation‑specific).
- **Parameters:** `count` – Maximum number of entries to return (default 10).
- **Return Value:** List of file paths ordered from most to least recent.
- **Exceptions:** 
  - `ArgumentOutOfRangeException` if `count` is less than 1.
  - `InvalidOperationException` if the recent‑file tracking mechanism is not initialized.

## Usage

```csharp
// Example 1: Asynchronously read a configuration file and compute its hash.
string configPath = @"C:\HealthExport\config.json";
string contents = await FileUtility.ReadFileAsync(configPath);
string hash = await FileUtility.GetFileSha256HashAsync(configPath);
Console.WriteLine($"Config hash: {hash}");
```

```csharp
// Example 2: Copy a directory of export files to a backup location, ensuring unique names.
string source = @"D:\Exports\Today";
string backupRoot = @"D:\Backups";
string backupDir = Path.Combine(backupRoot, $"Backup_{DateTime.UtcNow:yyyyMMdd_HHmmss}");
await FileUtility.CopyDirectoryAsync(source, backupDir);

// Generate a unique log file name inside the backup directory.
string logPath = FileUtility.GenerateUniqueFilePath(backupDir, "export.log");
await FileUtility.WriteLinesAsync(logPath, new[] { "Export started", "Export completed" });
```

## Notes

- All methods are static and rely only on their parameters; they do not access mutable static state, making them thread‑safe for concurrent calls assuming the underlying file system operations themselves are safe (e.g., two threads writing to the same file may still cause conflicts).
- Path validation is performed before any I/O operation; however, the methods do not guard against race conditions where the file system state changes between the validation check and the actual operation (TOCTOU). Callers should handle possible `IOException` or `UnauthorizedAccessException` accordingly.
- `GetRecentFiles` depends on an internal recent‑file list that is populated elsewhere in the codebase; if that list has not been initialized, the method will throw an `InvalidOperationException`.
- The asynchronous methods do not accept a `CancellationToken`; cancellation must be handled at a higher level if required.
- When dealing with very large files, prefer streaming APIs; `ReadFileAsync` and `GetFileSha256HashAsync` load the entire file into memory.
