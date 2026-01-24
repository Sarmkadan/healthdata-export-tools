#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HealthDataExportTools.Tests
{
    public abstract class BaseTest : IClassFixture<TestFixture>
    {
        protected readonly ServiceProvider ServiceProvider;

        protected BaseTest(TestFixture fixture)
        {
            ServiceProvider = fixture.ServiceProvider;
        }
    }

    public sealed class TestFixture
    {
        public ServiceProvider ServiceProvider { get; private set; }

        public TestFixture()
        {
            var services = new ServiceCollection();
            // Register services needed for tests
            // Example: services.AddTransient<IMyService, MyService>();
            ServiceProvider = services.BuildServiceProvider();
        }
    }
}
