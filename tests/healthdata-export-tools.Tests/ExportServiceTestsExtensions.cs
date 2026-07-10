using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthDataExportTools.Tests
{
    /// <summary>
    /// Provides extension methods for the <see cref="ExportServiceTests"/> class.
    /// </summary>
    public static class ExportServiceTestsExtensions
    {
        /// <summary>
        /// Retrieves the names of all test methods in the <see cref="ExportServiceTests"/> instance.
        /// </summary>
        /// <param name="tests">The <see cref="ExportServiceTests"/> instance.</param>
        /// <returns>An <see cref="IReadOnlyList{T}"/> of test method names.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is null.</exception>
        public static IReadOnlyList<string> GetTestMethodNames(this ExportServiceTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);
            return tests.GetType().GetMethods().Select(m => m.Name).ToList();
        }

        /// <summary>
        /// Determines whether the specified test method is asynchronous.
        /// </summary>
        /// <param name="tests">The <see cref="ExportServiceTests"/> instance.</param>
        /// <param name="methodName">The name of the test method.</param>
        /// <returns>True if the test method is asynchronous; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="methodName"/> is null or empty.</exception>
        public static bool IsTestMethodAsync(this ExportServiceTests tests, string methodName)
        {
            ArgumentNullException.ThrowIfNull(tests);
            ArgumentException.ThrowIfNullOrEmpty(methodName);
            var method = tests.GetType().GetMethod(methodName);
            return method?.ReturnType == typeof(Task);
        }

        /// <summary>
        /// Retrieves the names of all asynchronous test methods in the <see cref="ExportServiceTests"/> instance.
        /// </summary>
        /// <param name="tests">The <see cref="ExportServiceTests"/> instance.</param>
        /// <returns>An <see cref="IReadOnlyList{T}"/> of asynchronous test method names.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="tests"/> is null.</exception>
        public static IReadOnlyList<string> GetAsyncTestMethodNames(this ExportServiceTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);
            return tests.GetType().GetMethods().Where(m => m.ReturnType == typeof(Task)).Select(m => m.Name).ToList();
        }
    }
}
