// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using HealthDataExportTools.Data;
using HealthDataExportTools.Services;

namespace HealthDataExportTools.Configuration;

/// <summary>
/// Extension methods for configuring health data services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add health data export services to the dependency injection container
    /// </summary>
    public static IServiceCollection AddHealthDataExportServices(
        this IServiceCollection services,
        Action<HealthDataExportOptions>? configure = null)
    {
        if (configure != null)
            services.Configure(configure);
        else
            services.Configure<HealthDataExportOptions>(x => { });

        services.AddSingleton<ValidationService>();
        services.AddSingleton<HealthDataParserService>();
        services.AddSingleton<ExportService>();
        services.AddSingleton<AnalyticsService>();

        return services;
    }

    /// <summary>
    /// Add in-memory repository for testing
    /// </summary>
    public static IServiceCollection AddInMemoryRepository(this IServiceCollection services)
    {
        services.AddSingleton<IHealthDataRepository, InMemoryHealthDataRepository>();
        return services;
    }

    /// <summary>
    /// Add SQLite repository with connection management
    /// </summary>
    public static IServiceCollection AddSqliteRepository(
        this IServiceCollection services,
        string databasePath)
    {
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
    public static IServiceProvider CreateTestServiceProvider(string? databasePath = null)
    {
        var services = new ServiceCollection();

        services.AddHealthDataExportServices();

        if (!string.IsNullOrEmpty(databasePath))
            services.AddSqliteRepository(databasePath);
        else
            services.AddInMemoryRepository();

        return services.BuildServiceProvider();
    }
}

/// <summary>
/// Configuration builder for fluent API setup
/// </summary>
public class HealthDataExportConfigurationBuilder
{
    private readonly IServiceCollection _services;
    private readonly HealthDataExportOptions _options;

    public HealthDataExportConfigurationBuilder(IServiceCollection services)
    {
        _services = services;
        _options = new HealthDataExportOptions();
    }

    /// <summary>
    /// Set input directory path
    /// </summary>
    public HealthDataExportConfigurationBuilder WithInputPath(string path)
    {
        _options.InputPath = path;
        return this;
    }

    /// <summary>
    /// Set output directory path
    /// </summary>
    public HealthDataExportConfigurationBuilder WithOutputPath(string path)
    {
        _options.OutputPath = path;
        return this;
    }

    /// <summary>
    /// Set database path
    /// </summary>
    public HealthDataExportConfigurationBuilder WithDatabasePath(string path)
    {
        _options.DatabasePath = path;
        return this;
    }

    /// <summary>
    /// Set export format
    /// </summary>
    public HealthDataExportConfigurationBuilder WithExportFormat(Domain.Enums.ExportFormat format)
    {
        _options.ExportFormat = format;
        return this;
    }

    /// <summary>
    /// Enable data validation
    /// </summary>
    public HealthDataExportConfigurationBuilder WithDataValidation(bool enable = true)
    {
        _options.ValidateData = enable;
        return this;
    }

    /// <summary>
    /// Enable analysis
    /// </summary>
    public HealthDataExportConfigurationBuilder WithAnalysis(bool enable = true)
    {
        _options.PerformAnalysis = enable;
        return this;
    }

    /// <summary>
    /// Set trend analysis days
    /// </summary>
    public HealthDataExportConfigurationBuilder WithTrendAnalysisDays(int days)
    {
        _options.TrendAnalysisDays = days;
        return this;
    }

    /// <summary>
    /// Set date range
    /// </summary>
    public HealthDataExportConfigurationBuilder WithDateRange(DateTime? startDate, DateTime? endDate)
    {
        _options.StartDate = startDate;
        _options.EndDate = endDate;
        return this;
    }

    /// <summary>
    /// Enable verbose logging
    /// </summary>
    public HealthDataExportConfigurationBuilder WithVerboseLogging(bool enable = true)
    {
        _options.VerboseLogging = enable;
        return this;
    }

    /// <summary>
    /// Build the configuration
    /// </summary>
    public IServiceProvider Build()
    {
        var validationErrors = _options.Validate();
        if (validationErrors.Count > 0)
            throw new InvalidOperationException(
                "Configuration validation failed:\n" +
                string.Join("\n", validationErrors.Select(e => $"  • {e}")));

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
