#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using HealthDataExportTools.Configuration;

namespace HealthDataExportTools.Correlation;

/// <summary>
/// <see cref="IServiceCollection"/> extension methods for registering the
/// health-data correlation engine and its supporting types.
/// </summary>
public static class CorrelationServiceExtensions
{
    /// <summary>
    /// Registers <see cref="ICorrelationEngine"/> as a singleton backed by
    /// <see cref="CorrelationEngine"/>.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configure">
    /// Optional delegate to customise <see cref="CorrelationEngineOptions"/>.
    /// When supplied, the configured instance is registered as a singleton so that
    /// application code can inject it directly if needed.
    /// </param>
    /// <returns>The same <paramref name="services"/> instance for chaining.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the supplied options fail validation.
    /// </exception>
    /// <example>
    /// <code>
    /// services.AddHealthDataExportServices()
    ///         .AddCorrelationEngine(opts =>
    ///         {
    ///             opts.AnalysisWindowDays    = 60;
    ///             opts.SignificanceThreshold = 0.4;
    ///             opts.InsightMode           = InsightGenerationMode.Comprehensive;
    ///         });
    /// </code>
    /// </example>
    public static IServiceCollection AddCorrelationEngine(
        this IServiceCollection services,
        Action<CorrelationEngineOptions>? configure = null)
    {
        if (configure is not null)
        {
            var options = new CorrelationEngineOptions();
            configure(options);

            var errors = options.Validate().ToList();
            if (errors.Count > 0)
                throw new InvalidOperationException(
                    $"CorrelationEngineOptions validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");

            services.TryAddSingleton(options);
        }

        services.TryAddSingleton<ICorrelationEngine, CorrelationEngine>();
        return services;
    }

    /// <summary>
    /// Registers <see cref="ICorrelationEngine"/> using a pre-built
    /// <paramref name="options"/> instance.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="options">
    /// A fully-initialised <see cref="CorrelationEngineOptions"/> instance.
    /// </param>
    /// <returns>The same <paramref name="services"/> instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="options"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the supplied options fail validation.
    /// </exception>
    public static IServiceCollection AddCorrelationEngine(
        this IServiceCollection services,
        CorrelationEngineOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var errors = options.Validate().ToList();
        if (errors.Count > 0)
            throw new InvalidOperationException(
                $"CorrelationEngineOptions validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");

        services.TryAddSingleton(options);
        services.TryAddSingleton<ICorrelationEngine, CorrelationEngine>();
        return services;
    }

    /// <summary>
    /// Resolves <see cref="ICorrelationEngine"/> from the service provider.
    /// Helper for one-shot usage where full DI wiring is not required.
    /// </summary>
    /// <param name="provider">A configured service provider.</param>
    /// <returns>The registered <see cref="ICorrelationEngine"/> instance.</returns>
    public static ICorrelationEngine GetCorrelationEngine(this IServiceProvider provider) =>
        provider.GetRequiredService<ICorrelationEngine>();
}
