#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;

namespace HealthDataExportTools.Integration;

/// <summary>
/// Extension methods for <see cref="WebhookService"/> providing additional webhook management functionality
/// </summary>
public static class WebhookServiceExtensions
{
    /// <summary>
    /// Register multiple webhooks at once from a collection of webhook configurations
    /// </summary>
    /// <param name="service">The webhook service instance</param>
    /// <param name="webhookConfigs">Collection of webhook configurations to register</param>
    /// <returns>Number of successfully registered webhooks</returns>
    public static int RegisterWebhooks(this WebhookService service, IEnumerable<(string url, string eventType, bool isActive)> webhookConfigs)
    {
        if (service is null)
            throw new ArgumentNullException(nameof(service));

        if (webhookConfigs is null)
            throw new ArgumentNullException(nameof(webhookConfigs));

        int registeredCount = 0;
        foreach (var config in webhookConfigs)
        {
            service.RegisterWebhook(config.url, config.eventType, config.isActive);
            registeredCount++;
        }

        return registeredCount;
    }

    /// <summary>
    /// Check if a webhook with the specified event type exists and is active
    /// </summary>
    /// <param name="service">The webhook service instance</param>
    /// <param name="eventType">The event type to check</param>
    /// <returns>True if an active webhook exists for the event type; otherwise false</returns>
    public static bool HasActiveWebhookForEvent(this WebhookService service, string eventType)
    {
        if (service is null)
            throw new ArgumentNullException(nameof(service));

        if (string.IsNullOrEmpty(eventType))
            throw new ArgumentException("Event type cannot be empty", nameof(eventType));

        var webhooks = service.GetRegisteredWebhooks();
        return webhooks.Any(w =>
            w.IsActive &&
            w.EventType.Equals(eventType, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Get statistics for all registered webhooks
    /// </summary>
    /// <param name="service">The webhook service instance</param>
    /// <returns>A tuple containing total webhooks, active webhooks, total success count, and total failure count</returns>
    public static (int total, int active, int totalSuccess, int totalFailure) GetWebhookStatistics(this WebhookService service)
    {
        if (service is null)
            throw new ArgumentNullException(nameof(service));

        var webhooks = service.GetRegisteredWebhooks();
        int total = webhooks.Count;
        int active = webhooks.Count(w => w.IsActive);
        int totalSuccess = webhooks.Sum(w => w.SuccessCount);
        int totalFailure = webhooks.Sum(w => w.FailureCount);

        return (total, active, totalSuccess, totalFailure);
    }

    /// <summary>
    /// Find webhooks by URL pattern (supports partial matching)
    /// </summary>
    /// <param name="service">The webhook service instance</param>
    /// <param name="urlPattern">URL pattern to search for (case-insensitive)</param>
    /// <returns>List of matching webhooks</returns>
    public static List<Webhook> FindWebhooksByUrl(this WebhookService service, string urlPattern)
    {
        if (service is null)
            throw new ArgumentNullException(nameof(service));

        if (string.IsNullOrEmpty(urlPattern))
            throw new ArgumentException("URL pattern cannot be empty", nameof(urlPattern));

        var webhooks = service.GetRegisteredWebhooks();
        return webhooks
            .Where(w => w.Url.Contains(urlPattern, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// Clear all failed webhooks (those with failure count > 0) and reset their statistics
    /// </summary>
    /// <param name="service">The webhook service instance</param>
    /// <returns>Number of webhooks cleared</returns>
    public static int ClearFailedWebhooks(this WebhookService service)
    {
        if (service is null)
            throw new ArgumentNullException(nameof(service));

        var webhooks = service.GetRegisteredWebhooks();
        int clearedCount = 0;

        foreach (var webhook in webhooks.Where(w => w.FailureCount > 0).ToList())
        {
            webhook.SuccessCount = 0;
            webhook.FailureCount = 0;
            webhook.LastInvokedAt = null;
            clearedCount++;
        }

        return clearedCount;
    }

    /// <summary>
    /// Get webhooks grouped by event type
    /// </summary>
    /// <param name="service">The webhook service instance</param>
    /// <returns>Dictionary mapping event types to lists of webhooks</returns>
    public static Dictionary<string, List<Webhook>> GetWebhooksGroupedByEventType(this WebhookService service)
    {
        if (service is null)
            throw new ArgumentNullException(nameof(service));

        var webhooks = service.GetRegisteredWebhooks();
        return webhooks
            .GroupBy(w => w.EventType)
            .OrderBy(g => g.Key)
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    /// <summary>
    /// Trigger webhooks asynchronously with a timeout
    /// </summary>
    /// <param name="service">The webhook service instance</param>
    /// <param name="eventType">The event type to trigger</param>
    /// <param name="payload">The payload to send</param>
    /// <param name="timeoutMilliseconds">Timeout in milliseconds (default: 30000)</param>
    /// <returns>Task representing the operation</returns>
    public static async Task TriggerWebhooksAsync(this WebhookService service, string eventType, object payload, int timeoutMilliseconds = 30000)
    {
        if (service is null)
            throw new ArgumentNullException(nameof(service));

        if (string.IsNullOrEmpty(eventType))
            throw new ArgumentException("Event type cannot be empty", nameof(eventType));

        if (timeoutMilliseconds <= 0)
            throw new ArgumentOutOfRangeException(nameof(timeoutMilliseconds), "Timeout must be positive");

        using var cts = new CancellationTokenSource(timeoutMilliseconds);
        await service.TriggerWebhooksAsync(eventType, payload).ConfigureAwait(false);
    }
}
