using HealthDataExportTools.Services;
using System;
using Xunit;

namespace HealthDataExportTools.Tests
{
    /// <summary>
    /// Extension methods for validating <see cref="ValidationResult"/> instances in unit tests.
    /// </summary>
    public static class ValidationServiceTestsExtensions
    {
        /// <summary>
        /// Asserts that a validation result is valid (contains no errors).
        /// </summary>
        /// <param name="tests">The test fixture instance.</param>
        /// <param name="result">The validation result to check.</param>
        /// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/>.</exception>
        public static void AssertValidResult(this ValidationServiceTests tests, ValidationResult result)
        {
            ArgumentNullException.ThrowIfNull(result);
            Assert.Empty(result.Errors);
        }

        /// <summary>
        /// Asserts that a validation result is invalid with the expected number of errors.
        /// </summary>
        /// <param name="tests">The test fixture instance.</param>
        /// <param name="result">The validation result to check.</param>
        /// <param name="expectedErrorCount">The expected number of validation errors.</param>
        /// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/>.</exception>
        public static void AssertInvalidResult(this ValidationServiceTests tests, ValidationResult result, int expectedErrorCount)
        {
            ArgumentNullException.ThrowIfNull(result);
            Assert.NotEmpty(result.Errors);
            Assert.Equal(expectedErrorCount, result.Errors.Count);
        }

        /// <summary>
        /// Verifies that a validation result contains no errors by delegating to <see cref="AssertValidResult"/>.
        /// </summary>
        /// <param name="tests">The test fixture instance.</param>
        /// <param name="result">The validation result to check.</param>
        /// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/>.</exception>
        public static void VerifyNoErrors(this ValidationServiceTests tests, ValidationResult result)
        {
            ArgumentNullException.ThrowIfNull(result);
            tests.AssertValidResult(result);
        }
    }
}