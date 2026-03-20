// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using HealthDataExportTools.DTOs;
using HealthDataExportTools.Domain.Models;

namespace HealthDataExportTools.Services;

public interface IValidationService
{
    ValidationResult ValidateSleepData(SleepData data);
    ValidationResult ValidateHeartRateData(HeartRateData data);
    ValidationResult ValidateSpO2Data(SpO2Data data);
    ValidationResult ValidateStepsData(StepsData data);
    ValidationResult ValidateActivityData(ActivityData data);
    ValidationResult ValidateHealthMetric(HealthMetric metric);
}
