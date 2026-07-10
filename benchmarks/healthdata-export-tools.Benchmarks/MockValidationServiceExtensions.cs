using System;
using System.Collections.Generic;

namespace HealthDataExportTools.Benchmarks
{
    public static class MockValidationServiceExtensions
    {
        public static bool IsValid(this MockValidationService service, IEnumerable<ValidationResult> results)
        {
            foreach (var result in results)
            {
                if (result.HasErrors)
                {
                    return false;
                }
            }
            return true;
        }

        public static void ValidateAll(this MockValidationService service, 
            SleepData sleepData, 
            HeartRateData heartRateData, 
            SpO2Data spO2Data, 
            StepsData stepsData, 
            ActivityData activityData)
        {
            var sleepResult = service.ValidateSleepData(sleepData);
            var heartRateResult = service.ValidateHeartRateData(heartRateData);
            var spO2Result = service.ValidateSpO2Data(spO2Data);
            var stepsResult = service.ValidateStepsData(stepsData);
            var activityResult = service.ValidateActivityData(activityData);

            // You can add error handling or logging here
        }

        public static Dictionary<string, ValidationResult> ValidateAll(this MockValidationService service, 
            SleepData sleepData, 
            HeartRateData heartRateData, 
            SpO2Data spO2Data, 
            StepsData stepsData, 
            ActivityData activityData)
        {
            var results = new Dictionary<string, ValidationResult>
            {
                ["Sleep"] = service.ValidateSleepData(sleepData),
                ["HeartRate"] = service.ValidateHeartRateData(heartRateData),
                ["SpO2"] = service.ValidateSpO2Data(spO2Data),
                ["Steps"] = service.ValidateStepsData(stepsData),
                ["Activity"] = service.ValidateActivityData(activityData),
            };
            return results;
        }
    }
}
