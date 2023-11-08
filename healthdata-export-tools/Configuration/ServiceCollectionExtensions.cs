#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using HealthDataExportTools.Data;
using HealthDataExportTools.Services;
using System.Diagnostics.CodeAnalysis;

namespace HealthDataExportTools.Configuration;

/// <summary>
/// Extension methods for configuring health data services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add health data export services to the dependency injection container
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to</param>
    /// <param name="configure">Optional configuration action for <see cref="HealthDataExportOptions"/></param>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/></exception>
    public static IServiceCollection AddHealthDataExportServices(
        this IServiceCollection services,
        Action<HealthDataExportOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (configure is not null)
        {
            services.Configure(configure);
        }
        else
        {
            services.Configure<HealthDataExportOptions>(_ => { });
        }

        services.AddLogging();
        services.AddSingleton<ValidationService>();
        services.AddSingleton<HealthDataParserService>();
        services.AddSingleton<ExportService>();
        services.AddSingleton<ChartExportService>();
        services.AddSingleton<DataComparisonService>();
        services.AddSingleton<AnalyticsService>();
        services.AddSingleton<TrendAnomalyDetectionService>();

        return services;
    }

    /// <summary>
    /// Add in-memory repository for testing
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to</param>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/></exception>
    public static IServiceCollection AddInMemoryRepository(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IHealthDataRepository, InMemoryHealthDataRepository>();
        return services;
    }

    /// <summary>
    /// Add SQLite repository with connection management
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to</param>
    /// <param name="databasePath">Path to the SQLite database file</param>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> or <paramref name="databasePath"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException"><paramref name="databasePath"/> is empty or whitespace</exception>
    public static IServiceCollection AddSqliteRepository(
        this IServiceCollection services,
        [DisallowNull] string databasePath)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);

        services.AddSingleton<SqliteConnectionManager>(_ =>
        {
            var manager = new SqliteConnectionManager(databasePath);
            manager.InitializeDatabaseAsync().GetAwaiter().GetResult();
            return manager;
        });

        return services;
    }

    /// <summary>
    /// Create a minimal service provider for testing
    /// </summary>
    /// <param name="databasePath">Optional path to SQLite database file</param>
    /// <returns>A configured <see cref="IServiceProvider"/></returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/></exception>
    public static IServiceProvider CreateTestServiceProvider(string? databasePath = null)
    {
        var services = new ServiceCollection();

        services.AddHealthDataExportServices();

        if (!string.IsNullOrEmpty(databasePath))
        {
            services.AddSqliteRepository(databasePath);
        }
        else
        {
            services.AddInMemoryRepository();
        }

        return services.BuildServiceProvider();
    }
}

/// <summary>
/// Configuration builder for fluent API setup
/// </summary>
public sealed class HealthDataExportConfigurationBuilder
{
    private readonly IServiceCollection _services;
    private readonly HealthDataExportOptions _options;

    /// <summary>
    /// Initialize a new configuration builder
    /// </summary>
    /// <param name="services">The service collection to configure</param>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/></exception>
    public HealthDataExportConfigurationBuilder(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        _services = services;
        _options = new HealthDataExportOptions();
    }

    /// <summary>
    /// Set input directory path
    /// </summary>
    /// <param name="path">Input directory path</param>
    /// <returns>The builder instance for method chaining</returns>
    /// <exception cref="ArgumentException"><paramref name="path"/> is empty or whitespace</exception>
    public HealthDataExportConfigurationBuilder WithInputPath(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        _options.InputPath = path;
        return this;
    }

    /// <summary>
    /// Set output directory path
    /// </summary>
    /// <param name="path">Output directory path</param>
    /// <returns>The builder instance for method chaining</returns>
    /// <exception cref="ArgumentException"><paramref name="path"/> is empty or whitespace</exception>
    public HealthDataExportConfigurationBuilder WithOutputPath(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        _options.OutputPath = path;
        return this;
    }

    /// <summary>
    /// Set database path
    /// </summary>
    /// <param name="path">Database file path</param>
    /// <returns>The builder instance for method chaining</returns>
    /// <exception cref="ArgumentException"><paramref name="path"/> is empty or whitespace</exception>
    public HealthDataExportConfigurationBuilder WithDatabasePath(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        _options.DatabasePath = path;
        return this;
    }

    /// <summary>
    /// Set export format
    /// </summary>
    /// <param name="format">Export format to use</param>
    /// <returns>The builder instance for method chaining</returns>
    public HealthDataExportConfigurationBuilder WithExportFormat(Domain.Enums.ExportFormat format)
    {
        _options.ExportFormat = format;
        return this;
    }

    /// <summary>
    /// Enable data validation
    /// </summary>
    /// <param name="enable">Whether to enable validation</param>
    /// <returns>The builder instance for method chaining</returns>
    public HealthDataExportConfigurationBuilder WithDataValidation(bool enable = true)
    {
        _options.ValidateData = enable;
        return this;
    }

    /// <summary>
    /// Enable analysis
    /// </summary>
    /// <param name="enable">Whether to enable analysis</param>
    /// <returns>The builder instance for method chaining</returns>
    public HealthDataExportConfigurationBuilder WithAnalysis(bool enable = true)
    {
        _options.PerformAnalysis = enable;
        return this;
    }

    /// <summary>
    /// Set trend analysis days
    /// </summary>
    /// <param name="days">Number of days for trend analysis</param>
    /// <returns>The builder instance for method chaining</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="days"/> is not positive</exception>
    public HealthDataExportConfigurationBuilder WithTrendAnalysisDays(int days)
    {
        if (days <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(days), "TrendAnalysisDays must be positive");
        }

        _options.TrendAnalysisDays = days;
        return this;
    }

    /// <summary>
    /// Set date range
    /// </summary>
    /// <param name="startDate">Start date (inclusive)</param>
    /// <param name="endDate">End date (inclusive)</param>
    /// <returns>The builder instance for method chaining</returns>
    public HealthDataExportConfigurationBuilder WithDateRange(DateTime? startDate, DateTime? endDate)
    {
        _options.StartDate = startDate;
        _options.EndDate = endDate;
        return this;
    }

    /// <summary>
    /// Enable verbose logging
    /// </summary>
    /// <param name="enable">Whether to enable verbose logging</param>
    /// <returns>The builder instance for method chaining</returns>
    public HealthDataExportConfigurationBuilder WithVerboseLogging(bool enable = true)
    {
        _options.VerboseLogging = enable;
        return this;
    }

    /// <summary>
    /// Build the configuration
    /// </summary>
    /// <returns>A configured <see cref="IServiceProvider"/></returns>
    /// <exception cref="InvalidOperationException">Configuration validation failed</exception>
    public IServiceProvider Build()
    {
        var validationErrors = _options.Validate();
        if (validationErrors.Count > 0)
        {
            throw new InvalidOperationException(
                $"Configuration validation failed:{Environment.NewLine}" +
                string.Join(Environment.NewLine, validationErrors.Select(e => $" • {e}")));
        }

        _services.Configure<HealthDataExportOptions>(x =>
        {
            x.InputPath = _options.InputPath;
            x.OutputPath = _options.OutputPath;
            x.DatabasePath = _options.DatabasePath;
            x.ExportFormat = _options.ExportFormat;
            x.ValidateData = _options.ValidateData;
            x.PerformAnalysis = _options.PerformAnalysis;
            x.TrendAnalysisDays = _options.TrendAnalysisDays;
            x.StartDate = _options.StartDate;
            x.EndDate = _options.EndDate;
            x.VerboseLogging = _options.VerboseLogging;
        });

        _services.AddHealthDataExportServices();
        _services.AddInMemoryRepository();

        return _services.BuildServiceProvider();
    }
}