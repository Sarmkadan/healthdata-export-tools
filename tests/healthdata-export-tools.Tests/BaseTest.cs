#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HealthDataExportTools.Tests
{
    /// <summary>
    /// Base class for tests that requires a test fixture.
    /// </summary>
    public abstract class BaseTest : IClassFixture<TestFixture>
    {
        /// <summary>
        /// Gets the service provider for the test.
        /// </summary>
        protected readonly ServiceProvider ServiceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseTest"/> class.
        /// </summary>
        /// <param name="fixture">The test fixture.</param>
        protected BaseTest(TestFixture fixture)
        {
            ServiceProvider = fixture.ServiceProvider;
        }
    }

    /// <summary>
    /// Test fixture class that provides a service provider.
    /// </summary>
    public sealed class TestFixture
    {
        /// <summary>
        /// Gets the service provider.
        /// </summary>
        public ServiceProvider ServiceProvider { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestFixture"/> class.
        /// </summary>
        public TestFixture()
        {
            var services = new ServiceCollection();
            // Register services needed for tests
            // Example: services.AddTransient<IMyService, MyService>();
            ServiceProvider = services.BuildServiceProvider();
        }
    }
}
