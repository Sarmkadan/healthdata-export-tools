// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace HealthDataExportTools.Integration;

/// <summary>
/// Service for managing webhooks that notify external systems of events
/// </summary>
public class WebhookService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WebhookService> _logger;
    private readonly List<Webhook> _registeredWebhooks;
    private readonly ReaderWriterLockSlim _webhooksLock;

    public WebhookService(HttpClient httpClient, ILogger<WebhookService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _registeredWebhooks = new List<Webhook>();
        _webhooksLock = new ReaderWriterLockSlim();
    }

    /// <summary>
    /// Register a new webhook
    /// </summary>
    public void RegisterWebhook(string url, string eventType, bool isActive = true)
    {
        if (string.IsNullOrEmpty(url))
            throw new ArgumentException("Webhook URL cannot be empty", nameof(url));

        _webhooksLock.EnterWriteLock();
        try
        {
            var webhook = new Webhook
            {
                Id = Guid.NewGuid().ToString(),
                Url = url,
                EventType = eventType,
                IsActive = isActive,
                CreatedAt = DateTime.UtcNow
            };

            _registeredWebhooks.Add(webhook);
            _logger.LogInformation("Webhook registered: {EventType} -> {Url}", eventType, url);
        }
        finally
        {
            _webhooksLock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Unregister a webhook
    /// </summary>
    public bool UnregisterWebhook(string webhookId)
    {
        _webhooksLock.EnterWriteLock();
        try
        {
            var webhook = _registeredWebhooks.FirstOrDefault(w => w.Id == webhookId);
            if (webhook != null)
            {
                _registeredWebhooks.Remove(webhook);
                _logger.LogInformation("Webhook unregistered: {Id}", webhookId);
                return true;
            }
            return false;
        }
        finally
        {
            _webhooksLock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Trigger webhooks for a specific event
    /// </summary>
    public async Task TriggerWebhooksAsync(string eventType, object payload)
    {
        _webhooksLock.EnterReadLock();
        try
        {
            var applicableWebhooks = _registeredWebhooks
                .Where(w => w.IsActive && w.EventType.Equals(eventType, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (applicableWebhooks.Count == 0)
            {
                _logger.LogDebug("No webhooks registered for event: {EventType}", eventType);
                return;
            }

            _logger.LogInformation("Triggering {Count} webhooks for event: {EventType}",
                applicableWebhooks.Count, eventType);

            var tasks = applicableWebhooks.Select(wh => InvokeWebhookAsync(wh, eventType, payload)).ToList();

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering webhooks");
            }
        }
        finally
        {
            _webhooksLock.ExitReadLock();
        }
    }

    /// <summary>
    /// Invoke a single webhook
    /// </summary>
    private async Task InvokeWebhookAsync(Webhook webhook, string eventType, object payload)
    {
        try
        {
            var request = new WebhookRequest
            {
                EventType = eventType,
                Timestamp = DateTime.UtcNow,
                Payload = payload,
                RetryCount = 0
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogDebug("Invoking webhook: {Url} for event: {EventType}", webhook.Url, eventType);

            var response = await _httpClient.PostAsync(webhook.Url, content);

            if (response.IsSuccessStatusCode)
            {
                webhook.LastInvokedAt = DateTime.UtcNow;
                webhook.SuccessCount++;
                _logger.LogDebug("Webhook invoked successfully: {Url}", webhook.Url);
            }
            else
            {
                webhook.FailureCount++;
                _logger.LogWarning("Webhook invocation failed with status {Status}: {Url}",
                    response.StatusCode, webhook.Url);
            }
        }
        catch (Exception ex)
        {
            webhook.FailureCount++;
            _logger.LogError(ex, "Error invoking webhook: {Url}", webhook.Url);
        }
    }

    /// <summary>
    /// Get all registered webhooks
    /// </summary>
    public List<Webhook> GetRegisteredWebhooks()
    {
        _webhooksLock.EnterReadLock();
        try
        {
            return _registeredWebhooks.ToList();
        }
        finally
        {
            _webhooksLock.ExitReadLock();
        }
    }

    /// <summary>
    /// Enable or disable a webhook
    /// </summary>
    public bool SetWebhookActive(string webhookId, bool isActive)
    {
        _webhooksLock.EnterWriteLock();
        try
        {
            var webhook = _registeredWebhooks.FirstOrDefault(w => w.Id == webhookId);
            if (webhook != null)
            {
                webhook.IsActive = isActive;
                _logger.LogInformation("Webhook {Id} is now {State}",
                    webhookId, isActive ? "active" : "inactive");
                return true;
            }
            return false;
        }
        finally
        {
            _webhooksLock.ExitWriteLock();
        }
    }
}

/// <summary>
/// Webhook registration
/// </summary>
public class Webhook
{
    public string Id { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastInvokedAt { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
}

/// <summary>
/// Webhook request body
/// </summary>
public class WebhookRequest
{
    [JsonPropertyName("eventType")]
    public string EventType { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("payload")]
    public object? Payload { get; set; }

    [JsonPropertyName("retryCount")]
    public int RetryCount { get; set; }
}
