using System;
using System.Threading.Tasks;

namespace HealthDataExportTools.Tests
{
    /// <summary>
    /// Extension methods that group related test executions for <see cref="DomainModelTests"/>.
    /// These helpers make it easier to run a suite of related tests from a single call.
    /// </summary>
    public static class DomainModelTestsExtensions
    {
        /// <summary>
        /// Executes all sleep‑data related tests on the supplied <see cref="DomainModelTests"/> instance.
        /// </summary>
        /// <param name="tests">The test class instance. Must not be <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is <see langword="null"/>.</exception>
        public static void RunSleepDataTests(this DomainModelTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);

            tests.SleepData_CalculateQuality_DurationUnder6Hours_ReturnsPoor();
            tests.SleepData_GetDeepSleepPercentage_ReturnsCorrectRatio();
            tests.SleepData_GetDeepSleepPercentage_ZeroDuration_ReturnsZero();
        }

        /// <summary>
        /// Executes all steps‑data related tests on the supplied <see cref="DomainModelTests"/> instance.
        /// </summary>
        /// <param name="tests">The test class instance. Must not be <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is <see langword="null"/>.</exception>
        public static void RunStepsDataTests(this DomainModelTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);

            tests.StepsData_UpdateGoalAchievement_StepsExceedGoal_SetsGoalAchievedTrueAndCorrectPercentage();
            tests.StepsData_UpdateGoalAchievement_ZeroDailyGoal_SetsAchievementToZero();
            tests.StepsData_SetHourlySteps_HourAbove23_ThrowsArgumentOutOfRangeException();
        }

        /// <summary>
        /// Executes all heart‑rate and SpO₂ related tests on the supplied <see cref="DomainModelTests"/> instance.
        /// </summary>
        /// <param name="tests">The test class instance. Must not be <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is <see langword="null"/>.</exception>
        public static void RunHeartRateAndSpO2Tests(this DomainModelTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);

            tests.HeartRateData_AddMeasurement_UpdatesMeasurementCount();
            tests.HeartRateData_CalculateHeartRateReserve_WithRestingBpm_ReturnsMaxMinusResting();
            tests.SpO2Data_AddMeasurement_ReadingBelow95_IncrementsLowSpO2Events();
            tests.SpO2Data_HasConcerningLevels_MinimumBelow90_ReturnsTrue();
        }

        /// <summary>
        /// Executes all in‑memory cache provider related asynchronous tests on the supplied <see cref="DomainModelTests"/> instance.
        /// </summary>
        /// <param name="tests">The test class instance. Must not be <see langword="null"/>.</param>
        /// <returns>A task that completes when all cache tests have finished.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is <see langword="null"/>.</exception>
        public static async Task RunCacheProviderTestsAsync(this DomainModelTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);

            await tests.InMemoryCacheProvider_SetAndGet_WithMockedLogger_ReturnsStoredValue();
            await tests.InMemoryCacheProvider_GetNonExistentKey_ReturnsNull();
            await tests.InMemoryCacheProvider_RemoveKey_SubsequentGetReturnsNull();
        }
    }
}