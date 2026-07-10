// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

#nullable enable

using HealthDataExportTools.DTOs;
using HealthDataExportTools.Domain.Models;
using HealthDataExportTools.Services;

namespace HealthDataExportTools.Benchmarks;

/// <summary>
/// A mock implementation of the <see cref="IValidationService"/> interface.
/// </summary>
public sealed class MockValidationService : IValidationService
{
    /// <summary>
    /// Validates the provided <paramref name="data"/> and returns a <see cref="ValidationResult"/>.
    /// </summary>
    /// <param name="data">The <see cref="SleepData"/> to validate.</param>
    /// <returns>A <see cref="ValidationResult"/> representing the validation result.</returns>
    public ValidationResult ValidateSleepData(SleepData data) => new();

    /// <summary>
    /// Validates the provided <paramref name="data"/> and returns a <see cref="ValidationResult"/>.
    /// </summary>
    /// <param name="data">The <see cref="HeartRateData"/> to validate.</param>
    /// <returns>A <see cref="ValidationResult"/> representing the validation result.</returns>
    public ValidationResult ValidateHeartRateData(HeartRateData data) => new();

    /// <summary>
    /// Validates the provided <paramref name="data"/> and returns a <see cref="ValidationResult"/>.
    /// </summary>
    /// <param name="data">The <see cref="SpO2Data"/> to validate.</param>
    /// <returns>A <see cref="ValidationResult"/> representing the validation result.</returns>
    public ValidationResult ValidateSpO2Data(SpO2Data data) => new();

    /// <summary>
    /// Validates the provided <paramref name="data"/> and returns a <see cref="ValidationResult"/>.
    /// </summary>
    /// <param name="data">The <see cref="StepsData"/> to validate.</param>
    /// <returns>A <see cref="ValidationResult"/> representing the validation result.</returns>
    public ValidationResult ValidateStepsData(StepsData data) => new();

    /// <summary>
    /// Validates the provided <paramref name="data"/> and returns a <see cref="ValidationResult"/>.
    /// </summary>
    /// <param name="data">The <see cref="ActivityData"/> to validate.</param>
    /// <returns>A <see cref="ValidationResult"/> representing the validation result.</returns>
    public ValidationResult ValidateActivityData(ActivityData data) => new();

    /// <summary>
    /// Validates the provided <paramref name="metric"/> and returns a <see cref="ValidationResult"/>.
    /// </summary>
    /// <param name="metric">The <see cref="HealthMetric"/> to validate.</param>
    /// <returns>A <see cref="ValidationResult"/> representing the validation result.</returns>
    public ValidationResult ValidateHealthMetric(HealthMetric metric) => new();
}
