# CompressionUtility
The `CompressionUtility` class provides a set of static methods for compressing and decompressing files and strings using various algorithms. It offers methods for creating and extracting zip archives, compressing and decompressing files using gzip, and calculating compression ratios. Additionally, it includes utility methods for compressing and decompressing strings and converting sizes to human-readable formats.

## API
* `public static async Task<string> CompressFileGzipAsync`: Compresses a file using gzip and returns the path to the compressed file. This method is asynchronous and may throw exceptions if the file cannot be read or written.
* `public static async Task<string> DecompressFileGzipAsync`: Decompresses a gzip-compressed file and returns the path to the decompressed file. This method is asynchronous and may throw exceptions if the file cannot be read or written.
* `public static async Task<string> CreateZipArchiveAsync`: Creates a zip archive from a set of files and returns the path to the archive. This method is asynchronous and may throw exceptions if the files cannot be read or the archive cannot be written.
* `public static async Task<string> ExtractZipArchiveAsync`: Extracts the contents of a zip archive to a directory and returns the path to the extraction directory. This method is asynchronous and may throw exceptions if the archive cannot be read or the files cannot be written.
* `public static double GetCompressionRatio`: Calculates the compression ratio of a compressed file compared to its original size.
* `public static byte[] CompressString`: Compresses a string using a compression algorithm and returns the compressed bytes.
* `public static string DecompressString`: Decompresses a compressed string and returns the original string.
* `public static string GetHumanReadableSize`: Converts a size in bytes to a human-readable format (e.g., KB, MB, GB).

## Usage
```csharp
// Example 1: Compressing and decompressing a file
string originalFilePath = "path/to/original/file.txt";
string compressedFilePath = await CompressionUtility.CompressFileGzipAsync(originalFilePath);
string decompressedFilePath = await CompressionUtility.DecompressFileGzipAsync(compressedFilePath);

// Example 2: Creating and extracting a zip archive
string[] filesToArchive = new string[] { "file1.txt", "file2.txt" };
string archivePath = await CompressionUtility.CreateZipArchiveAsync(filesToArchive);
string extractionDirectory = await CompressionUtility.ExtractZipArchiveAsync(archivePath);
```

## Notes
The `CompressionUtility` class is designed to be thread-safe, as all methods are static and do not rely on instance state. However, the asynchronous methods may throw exceptions if the underlying file system operations fail. Additionally, the compression and decompression methods may not always result in a reduction in size, especially for already-compressed data. The `GetCompressionRatio` method can be used to determine the effectiveness of the compression. The `GetHumanReadableSize` method is useful for displaying file sizes in a user-friendly format.
