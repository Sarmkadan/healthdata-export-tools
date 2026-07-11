using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace HealthDataExportTools.DTOs;

/// <summary>
/// Provides validation methods for <see cref="ExportResultDto"/> instances
/// </summary>
public static class ExportResultDtoValidation
{
    /// <summary>
    /// Validates an <see cref="ExportResultDto"/> instance and returns a list of validation errors
    /// </summary>
    /// <param name="value">The DTO to validate</param>
    /// <returns>A read-only list of validation error messages (empty if valid)</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    public static IReadOnlyList<string> Validate(this ExportResultDto value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate ExportId
        if (string.IsNullOrWhiteSpace(value.ExportId))
        {
            errors.Add("ExportId cannot be null or whitespace.");
        }
        else if (!Guid.TryParse(value.ExportId, out _))
        {
            errors.Add("ExportId must be a valid GUID.");
        }

        // Validate Status
        if (string.IsNullOrWhiteSpace(value.Status))
        {
            errors.Add("Status cannot be null or whitespace.");
        }
        else if (value.Status.Length > 50)
        {
            errors.Add("Status cannot exceed 50 characters.");
        }

        // Validate RecordsExported
        if (value.RecordsExported < 0)
        {
            errors.Add("RecordsExported cannot be negative.");
        }

        // Validate RecordsFiltered
        if (value.RecordsFiltered < 0)
        {
            errors.Add("RecordsFiltered cannot be negative.");
        }

        // Validate OutputPath
        if (string.IsNullOrWhiteSpace(value.OutputPath))
        {
            errors.Add("OutputPath cannot be null or whitespace.");
        }
        else if (value.OutputPath.Length > 1024)
        {
            errors.Add("OutputPath cannot exceed 1024 characters.");
        }

        // Validate OutputSizeBytes
        if (value.OutputSizeBytes < 0)
        {
            errors.Add("OutputSizeBytes cannot be negative.");
        }

        // Validate GeneratedFiles
        if (value.GeneratedFiles is null)
        {
            errors.Add("GeneratedFiles cannot be null.");
        }
        else
        {
            foreach (var (file, index) in value.GeneratedFiles.Select((f, i) => (f, i)))
            {
                if (file is null)
                {
                    errors.Add($"GeneratedFiles[{index}] cannot be null.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(file.FileName))
                {
                    errors.Add($"GeneratedFiles[{index}].FileName cannot be null or whitespace.");
                }
                else if (file.FileName.Length > 255)
                {
                    errors.Add($"GeneratedFiles[{index}].FileName cannot exceed 255 characters.");
                }

                if (string.IsNullOrWhiteSpace(file.FilePath))
                {
                    errors.Add($"GeneratedFiles[{index}].FilePath cannot be null or whitespace.");
                }
                else if (file.FilePath.Length > 1024)
                {
                    errors.Add($"GeneratedFiles[{index}].FilePath cannot exceed 1024 characters.");
                }

                if (string.IsNullOrWhiteSpace(file.Format))
                {
                    errors.Add($"GeneratedFiles[{index}].Format cannot be null or whitespace.");
                }
                else if (file.Format.Length > 50)
                {
                    errors.Add($"GeneratedFiles[{index}].Format cannot exceed 50 characters.");
                }

                if (file.RecordCount < 0)
                {
                    errors.Add($"GeneratedFiles[{index}].RecordCount cannot be negative.");
                }

                if (file.FileSizeBytes < 0)
                {
                    errors.Add($"GeneratedFiles[{index}].FileSizeBytes cannot be negative.");
                }

                if (file.CreatedAt == default)
                {
                    errors.Add($"GeneratedFiles[{index}].CreatedAt cannot be default DateTime.");
                }
            }
        }

        // Validate ExportedFormats
        if (value.ExportedFormats is null)
        {
            errors.Add("ExportedFormats cannot be null.");
        }
        else
        {
            foreach (var (format, index) in value.ExportedFormats.Select((f, i) => (f, i)))
            {
                if (string.IsNullOrWhiteSpace(format))
                {
                    errors.Add($"ExportedFormats[{index}] cannot be null or whitespace.");
                }
                else if (format.Length > 50)
                {
                    errors.Add($"ExportedFormats[{index}] cannot exceed 50 characters.");
                }
            }
        }

        // Validate StartTime
        if (value.StartTime == default)
        {
            errors.Add("StartTime cannot be default DateTime.");
        }

        // Validate EndTime
        if (value.EndTime == default)
        {
            errors.Add("EndTime cannot be default DateTime.");
        }
        else if (value.EndTime < value.StartTime)
        {
            errors.Add("EndTime cannot be earlier than StartTime.");
        }

        // Validate DeviceTypes
        if (value.DeviceTypes is not null)
        {
            foreach (var (deviceType, index) in value.DeviceTypes.Select((d, i) => (d, i)))
            {
                if (string.IsNullOrWhiteSpace(deviceType))
                {
                    errors.Add($"DeviceTypes[{index}] cannot be null or whitespace.");
                }
                else if (deviceType.Length > 100)
                {
                    errors.Add($"DeviceTypes[{index}] cannot exceed 100 characters.");
                }
            }
        }

        // Validate DataTypes
        if (value.DataTypes is not null)
        {
            foreach (var (dataType, index) in value.DataTypes.Select((d, i) => (d, i)))
            {
                if (string.IsNullOrWhiteSpace(dataType))
                {
                    errors.Add($"DataTypes[{index}] cannot be null or whitespace.");
                }
                else if (dataType.Length > 100)
                {
                    errors.Add($"DataTypes[{index}] cannot exceed 100 characters.");
                }
            }
        }

        // Validate Warnings
        if (value.Warnings is not null)
        {
            foreach (var (warning, index) in value.Warnings.Select((w, i) => (w, i)))
            {
                if (string.IsNullOrWhiteSpace(warning))
                {
                    errors.Add($"Warnings[{index}] cannot be null or whitespace.");
                }
                else if (warning.Length > 500)
                {
                    errors.Add($"Warnings[{index}] cannot exceed 500 characters.");
                }
            }
        }

        // Validate Errors
        if (value.Errors is not null)
        {
            foreach (var (error, index) in value.Errors.Select((e, i) => (e, i)))
            {
                if (string.IsNullOrWhiteSpace(error))
                {
                    errors.Add($"Errors[{index}] cannot be null or whitespace.");
                }
                else if (error.Length > 500)
                {
                    errors.Add($"Errors[{index}] cannot exceed 500 characters.");
                }
            }
        }

        // Validate IsCompressed
        // No validation needed - boolean can always be valid

        // Validate CompressionRatio
        if (value.CompressionRatio.HasValue)
        {
            if (value.CompressionRatio < 0 || value.CompressionRatio > 100)
            {
                errors.Add("CompressionRatio must be between 0 and 100.");
            }
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether an <see cref="ExportResultDto"/> instance is valid
    /// </summary>
    /// <param name="value">The DTO to check</param>
    /// <returns>True if the DTO is valid; otherwise, false</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    public static bool IsValid(this ExportResultDto value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that an <see cref="ExportResultDto"/> instance is valid, throwing an exception if not
    /// </summary>
    /// <param name="value">The DTO to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    /// <exception cref="ArgumentException">Thrown if the DTO is invalid, containing validation errors</exception>
    public static void EnsureValid(this ExportResultDto value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"ExportResultDto validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }
}
