using HealthDataExportTools.Services;
using System;
using Xunit;

namespace HealthDataExportTools.Tests
{
    /// <summary>
    /// Extension methods for validating <see cref="ValidationResult"/> instances in unit tests.
    /// Provides assertion helpers to verify validation outcomes against expected error states.
    /// </summary>
    public static class ValidationServiceTestsExtensions
    {
        /// <summary>
        /// Asserts that a validation result is valid (contains no errors).
        /// </summary>
        /// <param name="tests">The test fixture instance providing context.</param>
        /// <param name="result">The validation result to check for validity.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="result"/> is <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// Verifies the result is non-null and contains an empty collection of errors.
        /// </remarks>
        public static void AssertValidResult(this ValidationServiceTests tests, ValidationResult result)
        {
            ArgumentNullException.ThrowIfNull(result);
            Assert.Empty(result.Errors);
        }

        /// <summary>
        /// Asserts that a validation result is invalid with the expected number of errors.
        /// </summary>
        /// <param name="tests">The test fixture instance providing context.</param>
        /// <param name="result">The validation result to check for invalidity.</param>
        /// <param name="expectedErrorCount">The number of validation errors expected.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="result"/> is <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// Verifies the result is non-null, contains errors, and the error count matches the expected value.
        /// </remarks>
        public static void AssertInvalidResult(this ValidationServiceTests tests, ValidationResult result, int expectedErrorCount)
        {
            ArgumentNullException.ThrowIfNull(result);
            Assert.NotEmpty(result.Errors);
            Assert.Equal(expectedErrorCount, result.Errors.Count);
        }

        /// <summary>
        /// Verifies that a validation result contains no errors by delegating to <see cref="AssertValidResult"/>.
        /// </summary>
        /// <param name="tests">The test fixture instance providing context.</param>
        /// <param name="result">The validation result to check for validity.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="result"/> is <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// Provides an alternative assertion name for readability while reusing the same validation logic.
        /// </remarks>
        public static void VerifyNoErrors(this ValidationServiceTests tests, ValidationResult result)
        {
            ArgumentNullException.ThrowIfNull(result);
            tests.AssertValidResult(result);
        }
    }
}
