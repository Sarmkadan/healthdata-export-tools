// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace HealthDataExportTools.Services;

/// <summary>
/// Service for sending notifications about export and import operations
/// Supports logging and extensible notification channels
/// </summary>
public class NotificationService
{
    private readonly ILogger<NotificationService> _logger;
    private readonly List<INotificationChannel> _channels;

    public NotificationService(ILogger<NotificationService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _channels = new List<INotificationChannel>();
    }

    /// <summary>
    /// Register a notification channel (email, webhook, etc.)
    /// </summary>
    public void RegisterChannel(INotificationChannel channel)
    {
        if (channel != null && !_channels.Contains(channel))
        {
            _channels.Add(channel);
            _logger.LogInformation("Registered notification channel: {Type}", channel.GetType().Name);
        }
    }

    /// <summary>
    /// Send export completion notification
    /// </summary>
    public async Task NotifyExportCompletedAsync(
        string exportId,
        int recordCount,
        string outputPath,
        TimeSpan duration)
    {
        var message = new NotificationMessage
        {
            Subject = "Health Data Export Completed",
            Body = $"""
                Export Completed Successfully

                Export ID: {exportId}
                Records Exported: {recordCount}
                Output Path: {outputPath}
                Duration: {duration.TotalSeconds:F2} seconds
                Timestamp: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}
                """,
            Type = NotificationType.Success,
            Timestamp = DateTime.UtcNow
        };

        await SendNotificationAsync(message);
    }

    /// <summary>
    /// Send export failure notification
    /// </summary>
    public async Task NotifyExportFailedAsync(
        string exportId,
        string errorMessage,
        Exception? exception = null)
    {
        var body = $"""
            Export Failed

            Export ID: {exportId}
            Error: {errorMessage}
            """;

        if (exception != null)
        {
            body += $"\nException Details: {exception.GetType().Name}\n{exception.Message}";
        }

        var message = new NotificationMessage
        {
            Subject = "Health Data Export Failed",
            Body = body,
            Type = NotificationType.Error,
            Timestamp = DateTime.UtcNow
        };

        await SendNotificationAsync(message);
    }

    /// <summary>
    /// Send import progress notification
    /// </summary>
    public async Task NotifyImportProgressAsync(
        string importId,
        int processedRecords,
        int totalRecords)
    {
        var percentage = totalRecords > 0 ? (processedRecords * 100 / totalRecords) : 0;

        var message = new NotificationMessage
        {
            Subject = "Health Data Import Progress",
            Body = $"Import Progress: {processedRecords}/{totalRecords} records ({percentage}%)",
            Type = NotificationType.Info,
            Timestamp = DateTime.UtcNow
        };

        await SendNotificationAsync(message);
    }

    /// <summary>
    /// Send warning notification for data quality issues
    /// </summary>
    public async Task NotifyDataQualityWarningsAsync(
        string operationId,
        List<string> warnings)
    {
        if (warnings == null || warnings.Count == 0)
            return;

        var warningsText = string.Join("\n  • ", warnings);

        var message = new NotificationMessage
        {
            Subject = "Data Quality Warnings",
            Body = $"""
                Data quality issues detected during operation {operationId}:

                  • {warningsText}
                """,
            Type = NotificationType.Warning,
            Timestamp = DateTime.UtcNow
        };

        await SendNotificationAsync(message);
    }

    /// <summary>
    /// Send custom notification
    /// </summary>
    public async Task SendNotificationAsync(NotificationMessage message)
    {
        if (message == null)
            return;

        _logger.LogInformation(
            "Sending {Type} notification: {Subject}",
            message.Type, message.Subject);

        var tasks = _channels.Select(channel => SendToChannelAsync(channel, message)).ToList();

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notifications");
        }
    }

    /// <summary>
    /// Send notification to specific channel
    /// </summary>
    private async Task SendToChannelAsync(INotificationChannel channel, NotificationMessage message)
    {
        try
        {
            await channel.SendAsync(message);
            _logger.LogDebug("Notification sent via {Channel}", channel.GetType().Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification via {Channel}", channel.GetType().Name);
        }
    }

    /// <summary>
    /// Get registered channels count
    /// </summary>
    public int GetChannelCount() => _channels.Count;
}

/// <summary>
/// Notification message structure
/// </summary>
public class NotificationMessage
{
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public DateTime Timestamp { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// Notification type enumeration
/// </summary>
public enum NotificationType
{
    Info,
    Success,
    Warning,
    Error
}

/// <summary>
/// Interface for notification channels
/// </summary>
public interface INotificationChannel
{
    Task SendAsync(NotificationMessage message);
}

/// <summary>
/// Log-based notification channel (always available)
/// </summary>
public class LogNotificationChannel : INotificationChannel
{
    private readonly ILogger<LogNotificationChannel> _logger;

    public LogNotificationChannel(ILogger<LogNotificationChannel> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(NotificationMessage message)
    {
        var logLevel = message.Type switch
        {
            NotificationType.Error => LogLevel.Error,
            NotificationType.Warning => LogLevel.Warning,
            NotificationType.Success => LogLevel.Information,
            _ => LogLevel.Information
        };

        _logger.Log(logLevel, "{Subject}: {Body}", message.Subject, message.Body);
        return Task.CompletedTask;
    }
}
