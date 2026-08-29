#nullable enable
// =============================================================================
// Author: Automated Test Generation
// =============================================================================

using System;
using System.IO;
using System.Threading.Tasks;
using HealthDataExportTools.Utilities;
using Xunit;

namespace HealthDataExportTools.Tests;

/// <summary>
/// Tests for <see cref="CompressionUtility"/> covering GZip round trips,
/// default output paths and input validation.
/// </summary>
public sealed class CompressionUtilityTests : IDisposable
{
    private readonly string _tempDir;

    public CompressionUtilityTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
    }

    [Fact]
    public async Task CompressAndDecompressGzip_RestoresOriginalContent()
    {
        // Arrange
        const string originalContent = "Health data: 12,345 steps\nHeart rate: 68 bpm\nUnicode: ❤";
        var inputPath = Path.Combine(_tempDir, "health-data.txt");
        var compressedPath = Path.Combine(_tempDir, "health-data.txt.gz");
        var decompressedPath = Path.Combine(_tempDir, "restored-health-data.txt");
        await File.WriteAllTextAsync(inputPath, originalContent);

        // Act
        await CompressionUtility.CompressFileGzipAsync(inputPath, compressedPath);
        await CompressionUtility.DecompressFileGzipAsync(compressedPath, decompressedPath);

        // Assert
        Assert.True(File.Exists(compressedPath));
        Assert.Equal(originalContent, await File.ReadAllTextAsync(decompressedPath));
    }

    [Fact]
    public async Task GzipOperations_WithoutOutputPaths_DeriveExpectedPathsForFilenameEndingInG()
    {
        // Arrange
        const string originalContent = "content for a filename ending in g";
        var inputPath = Path.Combine(_tempDir, "log");
        var expectedCompressedPath = Path.Combine(_tempDir, "log.gz");
        await File.WriteAllTextAsync(inputPath, originalContent);

        // Act
        var compressedPath = await CompressionUtility.CompressFileGzipAsync(inputPath);
        File.Delete(inputPath);
        var decompressedPath = await CompressionUtility.DecompressFileGzipAsync(compressedPath);

        // Assert
        Assert.Equal(expectedCompressedPath, compressedPath);
        Assert.Equal(inputPath, decompressedPath);
        Assert.Equal(originalContent, await File.ReadAllTextAsync(decompressedPath));
    }

    [Fact]
    public async Task GzipOperations_WithMissingInputFile_ThrowFileNotFoundException()
    {
        // Arrange
        var missingPath = Path.Combine(_tempDir, "missing.gz");

        // Act and Assert
        await Assert.ThrowsAsync<FileNotFoundException>(
            () => CompressionUtility.CompressFileGzipAsync(missingPath));
        await Assert.ThrowsAsync<FileNotFoundException>(
            () => CompressionUtility.DecompressFileGzipAsync(missingPath));
    }

    [Fact]
    public async Task GzipOperations_WithNullOrEmptyInputPath_ThrowArgumentException()
    {
        // Act and Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => CompressionUtility.CompressFileGzipAsync(null!));
        await Assert.ThrowsAsync<ArgumentException>(
            () => CompressionUtility.CompressFileGzipAsync(string.Empty));
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => CompressionUtility.DecompressFileGzipAsync(null!));
        await Assert.ThrowsAsync<ArgumentException>(
            () => CompressionUtility.DecompressFileGzipAsync(string.Empty));
    }
}
