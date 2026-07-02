// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

#nullable enable

using HealthDataExportTools.DTOs;
using HealthDataExportTools.Domain.Models;
using HealthDataExportTools.Services;

namespace HealthDataExportTools.Benchmarks;

public sealed class MockValidationService : IValidationService
{
    public ValidationResult ValidateSleepData(SleepData data) => new();
    public ValidationResult ValidateHeartRateData(HeartRateData data) => new();
    public ValidationResult ValidateSpO2Data(SpO2Data data) => new();
    public ValidationResult ValidateStepsData(StepsData data) => new();
    public ValidationResult ValidateActivityData(ActivityData data) => new();
    public ValidationResult ValidateHealthMetric(HealthMetric metric) => new();
}
