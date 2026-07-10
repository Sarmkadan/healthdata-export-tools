using System;
using System.Threading.Tasks;

namespace HealthDataExportTools.Tests
{
    /// <summary>
    /// Extension methods that compose the individual test cases of <see cref="InMemoryHealthDataRepositoryTests"/>
    /// into higher‑level scenarios. These helpers make it easier for test authors to run related
    /// assertions together without duplicating boilerplate.
    /// </summary>
    public static class InMemoryHealthDataRepositoryTestsExtensions
    {
        /// <summary>
        /// Executes the full suite of sleep‑data related tests in sequence.
        /// </summary>
        /// <param name="tests">The test class instance on which to invoke the individual test methods.</param>
        /// <returns>A task that completes when all sleep‑data tests have finished.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is <c>null</c>.</exception>
        public static async Task RunAllSleepDataTestsAsync(this InMemoryHealthDataRepositoryTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);

            await tests.AddAndGetSleepData_ReturnsCorrectData().ConfigureAwait(false);
            await tests.UpdateSleepData_ReflectsChanges().ConfigureAwait(false);
            await tests.DeleteSleepData_RemovesData().ConfigureAwait(false);
            await tests.GetSleepRange_ReturnsCorrectData().ConfigureAwait(false);
        }

        /// <summary>
        /// Executes the heart‑rate addition test followed by the total‑record‑count verification.
        /// </summary>
        /// <param name="tests">The test class instance.</param>
        /// <returns>A task that completes when both tests have finished.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is <c>null</c>.</exception>
        public static async Task RunHeartRateAndCountTestsAsync(this InMemoryHealthDataRepositoryTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);

            await tests.AddAndGetHeartRateData_ReturnsCorrectData().ConfigureAwait(false);
            await tests.GetTotalRecordCount_ReturnsCorrectCount().ConfigureAwait(false);
        }

        /// <summary>
        /// Executes the cleanup test that removes records older than a given date.
        /// </summary>
        /// <param name="tests">The test class instance.</param>
        /// <returns>A task that completes when the cleanup test has finished.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is <c>null</c>.</exception>
        public static async Task RunDeleteOldRecordsTestAsync(this InMemoryHealthDataRepositoryTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);

            await tests.DeleteOldRecords_RemovesRecordsBeforeDate().ConfigureAwait(false);
        }
    }
}
