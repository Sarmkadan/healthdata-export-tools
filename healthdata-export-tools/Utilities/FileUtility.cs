// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace HealthDataExportTools.Utilities;

/// <summary>
/// Utility class for file operations including reading, writing, and management
/// </summary>
public static class FileUtility
{
    /// <summary>
    /// Safely read file content with error handling
    /// </summary>
    public static async Task<string> ReadFileAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");

            return await File.ReadAllTextAsync(filePath, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            throw new HealthDataException($"Failed to read file: {filePath}", ex);
        }
    }

    /// <summary>
    /// Safely write content to file with backup option
    /// </summary>
    public static async Task WriteFileAsync(string filePath, string content, bool createBackup = false)
    {
        try
        {
            // Create directory if it doesn't exist
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Create backup if file exists
            if (createBackup && File.Exists(filePath))
            {
                var backupPath = $"{filePath}.backup";
                File.Copy(filePath, backupPath, overwrite: true);
            }

            await File.WriteAllTextAsync(filePath, content, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            throw new HealthDataException($"Failed to write file: {filePath}", ex);
        }
    }

    /// <summary>
    /// Get all files in directory matching pattern
    /// </summary>
    public static List<string> GetFilesRecursive(string directoryPath, string searchPattern = "*")
    {
        try
        {
            if (!Directory.Exists(directoryPath))
                throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");

            return Directory.GetFiles(directoryPath, searchPattern, SearchOption.AllDirectories).ToList();
        }
        catch (Exception ex)
        {
            throw new HealthDataException($"Failed to enumerate files in: {directoryPath}", ex);
        }
    }

    /// <summary>
    /// Delete file safely with optional backup preservation
    /// </summary>
    public static bool DeleteFile(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return false;

            File.Delete(filePath);
            return true;
        }
        catch (Exception ex)
        {
            throw new HealthDataException($"Failed to delete file: {filePath}", ex);
        }
    }

    /// <summary>
    /// Get file size in bytes
    /// </summary>
    public static long GetFileSize(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");

            return new FileInfo(filePath).Length;
        }
        catch (Exception ex)
        {
            throw new HealthDataException($"Failed to get file size: {filePath}", ex);
        }
    }

    /// <summary>
    /// Check if file is readable
    /// </summary>
    public static bool IsFileReadable(string filePath)
    {
        try
        {
            using (var stream = File.OpenRead(filePath))
            {
                return stream.CanRead;
            }
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Generate unique filename if file already exists
    /// </summary>
    public static string GenerateUniqueFilePath(string filePath)
    {
        if (!File.Exists(filePath))
            return filePath;

        var directory = Path.GetDirectoryName(filePath) ?? ".";
        var filename = Path.GetFileNameWithoutExtension(filePath);
        var extension = Path.GetExtension(filePath);

        int counter = 1;
        string newPath;

        do
        {
            newPath = Path.Combine(directory, $"{filename}_{counter}{extension}");
            counter++;
        }
        while (File.Exists(newPath));

        return newPath;
    }

    /// <summary>
    /// Copy directory recursively
    /// </summary>
    public static async Task CopyDirectoryAsync(string sourceDir, string destDir)
    {
        try
        {
            Directory.CreateDirectory(destDir);

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                var destFile = Path.Combine(destDir, Path.GetFileName(file));
                File.Copy(file, destFile, overwrite: true);
            }

            foreach (var dir in Directory.GetDirectories(sourceDir))
            {
                var destSubDir = Path.Combine(destDir, Path.GetFileName(dir));
                await CopyDirectoryAsync(dir, destSubDir);
            }
        }
        catch (Exception ex)
        {
            throw new HealthDataException($"Failed to copy directory: {sourceDir}", ex);
        }
    }

    /// <summary>
    /// Get file hash (SHA256) for integrity verification
    /// </summary>
    public static async Task<string> GetFileSha256HashAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");

            using (var stream = File.OpenRead(filePath))
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashBytes = await Task.Run(() => sha256.ComputeHash(stream));
                return Convert.ToHexString(hashBytes);
            }
        }
        catch (Exception ex)
        {
            throw new HealthDataException($"Failed to compute file hash: {filePath}", ex);
        }
    }

    /// <summary>
    /// Read file lines asynchronously
    /// </summary>
    public static async Task<List<string>> ReadLinesAsync(string filePath)
    {
        try
        {
            var lines = new List<string>();
            using (var reader = new StreamReader(filePath, Encoding.UTF8))
            {
                string? line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    lines.Add(line);
                }
            }
            return lines;
        }
        catch (Exception ex)
        {
            throw new HealthDataException($"Failed to read lines from: {filePath}", ex);
        }
    }

    /// <summary>
    /// Write lines to file
    /// </summary>
    public static async Task WriteLinesAsync(string filePath, IEnumerable<string> lines)
    {
        try
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                foreach (var line in lines)
                {
                    await writer.WriteLineAsync(line);
                }
            }
        }
        catch (Exception ex)
        {
            throw new HealthDataException($"Failed to write lines to: {filePath}", ex);
        }
    }

    /// <summary>
    /// Get most recently modified files in directory
    /// </summary>
    public static List<string> GetRecentFiles(string directoryPath, int count = 10)
    {
        try
        {
            return Directory.GetFiles(directoryPath)
                .OrderByDescending(f => new FileInfo(f).LastWriteTimeUtc)
                .Take(count)
                .ToList();
        }
        catch (Exception ex)
        {
            throw new HealthDataException($"Failed to get recent files from: {directoryPath}", ex);
        }
    }
}
