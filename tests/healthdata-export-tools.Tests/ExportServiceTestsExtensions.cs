using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthDataExportTools.Tests
{
    /// <summary>
    /// Provides extension methods for discovering and analyzing test methods in the <see cref="ExportServiceTests"/> class.
    /// </summary>
    public static class ExportServiceTestsExtensions
    {
        /// <summary>
        /// Retrieves the names of all test methods in the <see cref="ExportServiceTests"/> instance.
        /// A test method is defined as any public instance method whose name starts with "Test".
        /// </summary>
        /// <param name="tests">The <see cref="ExportServiceTests"/> instance.</param>
        /// <returns>An <see cref="IReadOnlyList{T}"/> of test method names.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <see langword="null"/>.</exception>
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
        /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="methodName"/> is <see langword="null"/> or empty.</exception>
        /// <exception cref="MissingMethodException">The specified method does not exist on the type.</exception>
        public static bool IsTestMethodAsync(this ExportServiceTests tests, string methodName)
        {
            ArgumentNullException.ThrowIfNull(tests);
            ArgumentException.ThrowIfNullOrEmpty(methodName);
            var method = tests.GetType().GetMethod(methodName);
            return method?.ReturnType == typeof(Task);
        }

        /// <summary>
        /// Retrieves the names of all asynchronous test methods in the <see cref="ExportServiceTests"/> instance.
        /// A test method is defined as any public instance method whose name starts with "Test".
        /// </summary>
        /// <param name="tests">The <see cref="ExportServiceTests"/> instance.</param>
        /// <returns>An <see cref="IReadOnlyList{T}"/> of asynchronous test method names.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tests"/> is <see langword="null"/>.</exception>
        public static IReadOnlyList<string> GetAsyncTestMethodNames(this ExportServiceTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);
            return tests.GetType().GetMethods().Where(m => m.ReturnType == typeof(Task)).Select(m => m.Name).ToList();
        }
    }
}