// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.IO.Compression;

namespace HealthDataExportTools.Utilities;

/// <summary>
/// Utility class for file compression and decompression operations
/// Supports GZip and ZIP formats
/// </summary>
public static class CompressionUtility
{
    private const int BufferSize = 65536;

    /// <summary>
    /// Compress a file using GZip compression
    /// </summary>
    public static async Task<string> CompressFileGzipAsync(string inputPath, string? outputPath = null)
    {
        if (!File.Exists(inputPath))
            throw new FileNotFoundException($"Input file not found: {inputPath}");

        outputPath ??= $"{inputPath}.gz";

        try
        {
            using (var sourceStream = File.OpenRead(inputPath))
            using (var targetStream = File.Create(outputPath))
            using (var gzipStream = new GZipStream(targetStream, CompressionMode.Compress))
            {
                await sourceStream.CopyToAsync(gzipStream, BufferSize);
            }

            return outputPath;
        }
        catch (Exception ex)
        {
            throw new HealthDataException($"Failed to compress file: {inputPath}", ex);
        }
    }

    /// <summary>
    /// Decompress a GZip file
    /// </summary>
    public static async Task<string> DecompressFileGzipAsync(string inputPath, string? outputPath = null)
    {
        if (!File.Exists(inputPath))
            throw new FileNotFoundException($"Input file not found: {inputPath}");

        outputPath ??= inputPath.TrimEnd('z').TrimEnd('g').TrimEnd('.');

        try
        {
            using (var sourceStream = File.OpenRead(inputPath))
            using (var targetStream = File.Create(outputPath))
            using (var gzipStream = new GZipStream(sourceStream, CompressionMode.Decompress))
            {
                await gzipStream.CopyToAsync(targetStream, BufferSize);
            }

            return outputPath;
        }
        catch (Exception ex)
        {
            throw new HealthDataException($"Failed to decompress file: {inputPath}", ex);
        }
    }

    /// <summary>
    /// Create a ZIP archive from multiple files
    /// </summary>
    public static async Task<string> CreateZipArchiveAsync(List<string> filePaths, string outputPath)
    {
        try
        {
            using (var zipArchive = ZipFile.Open(outputPath, ZipArchiveMode.Create))
            {
                foreach (var filePath in filePaths)
                {
                    if (!File.Exists(filePath))
                    {
                        continue; // Skip missing files
                    }

                    var fileInfo = new FileInfo(filePath);
                    zipArchive.CreateEntryFromFile(filePath, fileInfo.Name, CompressionLevel.Optimal);
                }
            }

            return await Task.FromResult(outputPath);
        }
        catch (Exception ex)
        {
            throw new HealthDataException("Failed to create ZIP archive", ex);
        }
    }

    /// <summary>
    /// Extract a ZIP archive to a directory
    /// </summary>
    public static async Task<string> ExtractZipArchiveAsync(string zipPath, string? outputDirectory = null)
    {
        if (!File.Exists(zipPath))
            throw new FileNotFoundException($"ZIP file not found: {zipPath}");

        outputDirectory ??= Path.Combine(Path.GetDirectoryName(zipPath) ?? ".",
            Path.GetFileNameWithoutExtension(zipPath));

        try
        {
            Directory.CreateDirectory(outputDirectory);
            ZipFile.ExtractToDirectory(zipPath, outputDirectory, overwriteFiles: true);
            return await Task.FromResult(outputDirectory);
        }
        catch (Exception ex)
        {
            throw new HealthDataException($"Failed to extract ZIP archive: {zipPath}", ex);
        }
    }

    /// <summary>
    /// Get the compressed size compared to original
    /// </summary>
    public static double GetCompressionRatio(string originalPath, string compressedPath)
    {
        if (!File.Exists(originalPath) || !File.Exists(compressedPath))
            return 0;

        var originalSize = new FileInfo(originalPath).Length;
        var compressedSize = new FileInfo(compressedPath).Length;

        if (originalSize == 0)
            return 0;

        return Math.Round(((double)compressedSize / originalSize) * 100, 2);
    }

    /// <summary>
    /// Compress string content to byte array using GZip
    /// </summary>
    public static byte[] CompressString(string content)
    {
        try
        {
            var bytes = Encoding.UTF8.GetBytes(content);

            using (var input = new MemoryStream(bytes))
            using (var output = new MemoryStream())
            using (var gzip = new GZipStream(output, CompressionMode.Compress))
            {
                input.CopyTo(gzip);
                gzip.Flush();
                return output.ToArray();
            }
        }
        catch (Exception ex)
        {
            throw new HealthDataException("Failed to compress string", ex);
        }
    }

    /// <summary>
    /// Decompress byte array using GZip
    /// </summary>
    public static string DecompressString(byte[] compressedData)
    {
        try
        {
            using (var input = new MemoryStream(compressedData))
            using (var output = new MemoryStream())
            using (var gzip = new GZipStream(input, CompressionMode.Decompress))
            {
                gzip.CopyTo(output);
                return Encoding.UTF8.GetString(output.ToArray());
            }
        }
        catch (Exception ex)
        {
            throw new HealthDataException("Failed to decompress string", ex);
        }
    }

    /// <summary>
    /// Get size of a file in human-readable format
    /// </summary>
    public static string GetHumanReadableSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }

        return $"{len:F2} {sizes[order]}";
    }
}
