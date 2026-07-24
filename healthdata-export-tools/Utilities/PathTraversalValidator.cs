#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.IO;
using HealthDataExportTools.Exceptions;

namespace HealthDataExportTools.Utilities;

/// <summary>
/// Provides validation for file paths to prevent path traversal attacks.
/// Ensures that file paths stay within expected directories and do not contain
/// traversal sequences like '../' or './' that could escape intended boundaries.
/// </summary>
public static class PathTraversalValidator
{
    /// <summary>
    /// Validates that a path does not contain path traversal sequences.
    /// </summary>
    /// <param name="path">The path to validate. Cannot be null or empty.</param>
    /// <param name="baseDirectory">Optional base directory to check against. If provided,
    /// the resolved path must be within this directory.</param>
    /// <returns>The validated and normalized path.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="path"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="path"/> is empty or contains only whitespace.</exception>
    /// <exception cref="PathTraversalException">Path contains traversal sequences or is outside the allowed directory.</exception>
    public static string ValidatePath(string path, string? baseDirectory = null)
    {
        ArgumentNullException.ThrowIfNull(path);

        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path cannot be empty or whitespace.", nameof(path));
        }


        // Normalize path separators
        var normalizedPath = path.Replace('\\', Path.DirectorySeparatorChar);

        // Check for path traversal sequences
        if (normalizedPath.Contains("..") ||
            normalizedPath.Contains("./") ||
            normalizedPath.Contains("%2e") ||  // URL-encoded ..
            normalizedPath.Contains("%2f"))   // URL-encoded /
        {
            throw new PathTraversalException(
                "Path contains traversal sequences (../ or ./) which are not allowed.",
                path);
        }

        // Resolve to absolute path for validation
        string absolutePath;
        try
        {
            absolutePath = Path.GetFullPath(normalizedPath);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            throw new PathTraversalException(
                "Path is invalid or too long.",
                path,
                ex);
        }

        // Validate against base directory if provided
        if (baseDirectory != null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(baseDirectory);

            string absoluteBaseDirectory;
            try
            {
                absoluteBaseDirectory = Path.GetFullPath(baseDirectory);
            }
            catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
            {
                throw new PathTraversalException(
                    "Base directory is invalid or too long.",
                    baseDirectory,
                    ex);
            }

            // Ensure base directory ends with directory separator for proper comparison
            if (!absoluteBaseDirectory.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                absoluteBaseDirectory += Path.DirectorySeparatorChar;
            }

            // Ensure path ends with directory separator for proper comparison
            if (!absolutePath.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                // If path is a directory, add separator
                if (Directory.Exists(absolutePath) || normalizedPath.EndsWith("/") || normalizedPath.EndsWith("\\"))
                {
                    absolutePath += Path.DirectorySeparatorChar;
                }
            }

            // Check if path is within base directory
            if (!absolutePath.StartsWith(absoluteBaseDirectory, StringComparison.OrdinalIgnoreCase))
            {
                throw new PathTraversalException(
                    $"Path '{path}' resolves to '{absolutePath}' which is outside the allowed directory '{baseDirectory}'.",
                    path);
            }
        }

        return absolutePath;
    }

    /// <summary>
    /// Validates that a directory path does not contain path traversal sequences.
    /// </summary>
    /// <param name="directoryPath">The directory path to validate. Cannot be null or empty.</param>
    /// <param name="baseDirectory">Optional base directory to check against.</param>
    /// <returns>The validated and normalized directory path.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="directoryPath"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="directoryPath"/> is empty or contains only whitespace.</exception>
    /// <exception cref="PathTraversalException">Path contains traversal sequences or is outside the allowed directory.</exception>
    public static string ValidateDirectoryPath(string directoryPath, string? baseDirectory = null)
    {
        ArgumentNullException.ThrowIfNull(directoryPath);

        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            throw new ArgumentException("Directory path cannot be empty or whitespace.", nameof(directoryPath));
        }

        // Ensure directory path ends with separator for consistent validation
        var pathToValidate = directoryPath;
        if (!pathToValidate.EndsWith("/") && !pathToValidate.EndsWith("\\"))
        {
            pathToValidate += Path.DirectorySeparatorChar;
        }

        var validatedPath = ValidatePath(pathToValidate, baseDirectory);

        return validatedPath;
    }

    /// <summary>
    /// Validates that a file path does not contain path traversal sequences.
    /// </summary>
    /// <param name="filePath">The file path to validate. Cannot be null or empty.</param>
    /// <param name="baseDirectory">Optional base directory to check against.</param>
    /// <returns>The validated and normalized file path.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="filePath"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="filePath"/> is empty or contains only whitespace.</exception>
    /// <exception cref="PathTraversalException">Path contains traversal sequences or is outside the allowed directory.</exception>
    public static string ValidateFilePath(string filePath, string? baseDirectory = null)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be empty or whitespace.", nameof(filePath));
        }

        var validatedPath = ValidatePath(filePath, baseDirectory);

        return validatedPath;
    }
}

/// <summary>
/// Exception thrown when path traversal is detected.
/// </summary>
public sealed class PathTraversalException : HealthDataException
{
    /// <summary>
    /// The invalid path that triggered the exception.
    /// </summary>
    public string? InvalidPath { get; }

    /// <summary>
    /// Initialize a new instance of PathTraversalException
    /// </summary>
    /// <param name="message">Error message.</param>
    /// <param name="invalidPath">The invalid path.</param>
    public PathTraversalException(string message, string? invalidPath)
        : base(message, "PATH_TRAVERSAL_ERROR")
    {
        InvalidPath = invalidPath;
        ContextData = new() { { "InvalidPath", invalidPath ?? string.Empty } };
    }

    /// <summary>
    /// Initialize with inner exception
    /// </summary>
    /// <param name="message">Error message.</param>
    /// <param name="invalidPath">The invalid path.</param>
    /// <param name="innerException">Inner exception.</param>
    public PathTraversalException(string message, string? invalidPath, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = "PATH_TRAVERSAL_ERROR";
        InvalidPath = invalidPath;
        ContextData = new() { { "InvalidPath", invalidPath ?? string.Empty } };
    }
}