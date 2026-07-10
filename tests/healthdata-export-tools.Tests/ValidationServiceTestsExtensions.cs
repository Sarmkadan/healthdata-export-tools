using HealthDataExportTools.Services;
using Xunit;

namespace HealthDataExportTools.Tests
{
    public static class ValidationServiceTestsExtensions
    {
        public static void AssertValidResult(this ValidationServiceTests tests, ValidationResult result)
        {
            Assert.NotNull(result);
            Assert.Empty(result.Errors);
        }

        public static void AssertInvalidResult(this ValidationServiceTests tests, ValidationResult result, int expectedErrorCount)
        {
            Assert.NotNull(result);
            Assert.NotEmpty(result.Errors);
            Assert.Equal(expectedErrorCount, result.Errors.Count);
        }

        public static void VerifyNoErrors(this ValidationServiceTests tests, ValidationResult result)
        {
            tests.AssertValidResult(result);
        }
    }
}
