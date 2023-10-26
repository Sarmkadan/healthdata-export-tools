# WebhookServiceExtensions
The `WebhookServiceExtensions` class provides a set of extension methods for the `WebhookService` class, allowing for the registration, management, and triggering of webhooks. These methods enable the retrieval of webhook statistics, clearing of failed webhooks, and grouping of webhooks by event type, among other functionalities.

## API
* `public static int RegisterWebhooks(this WebhookService service, IEnumerable<Webhook> webhooks)`: Registers a collection of webhooks with the `WebhookService`. The method returns the number of successfully registered webhooks. It throws an exception if the input `webhooks` collection is null or if any of the webhooks in the collection are invalid.
* `public static bool HasActiveWebhookForEvent(this WebhookService service, string eventType)`: Checks if there is an active webhook registered for a specific event type. The method returns `true` if an active webhook is found, `false` otherwise. It throws an exception if the input `eventType` is null or empty.
* `public static (int total, int active, int totalSuccess, int totalFailure) GetWebhookStatistics(this WebhookService service)`: Retrieves statistics about the webhooks registered with the `WebhookService`. The method returns a tuple containing the total number of webhooks, the number of active webhooks, the total number of successful webhook triggers, and the total number of failed webhook triggers.
* `public static List<Webhook> FindWebhooksByUrl(this WebhookService service, string url)`: Finds all webhooks registered with the `WebhookService` that have a specific URL. The method returns a list of matching webhooks. It throws an exception if the input `url` is null or empty.
* `public static int ClearFailedWebhooks(this WebhookService service)`: Clears all failed webhooks registered with the `WebhookService`. The method returns the number of cleared webhooks.
* `public static Dictionary<string, List<Webhook>> GetWebhooksGroupedByEventType(this WebhookService service)`: Groups all webhooks registered with the `WebhookService` by their event types. The method returns a dictionary where the keys are event types and the values are lists of webhooks associated with each event type.
* `public static async Task TriggerWebhooksAsync(this WebhookService service, string eventType)`: Triggers all webhooks registered with the `WebhookService` that are associated with a specific event type. The method is asynchronous and returns a task that completes when the triggering is finished. It throws an exception if the input `eventType` is null or empty.

## Usage
```csharp
// Registering webhooks
var webhooks = new List<Webhook>
{
    new Webhook { Url = "https://example.com/webhook1", EventType = "Event1" },
    new Webhook { Url = "https://example.com/webhook2", EventType = "Event2" }
};
var service = new WebhookService();
var registeredCount = service.RegisterWebhooks(webhooks);
Console.WriteLine($"Registered {registeredCount} webhooks");

// Triggering webhooks
var triggerTask = service.TriggerWebhooksAsync("Event1");
await triggerTask;
Console.WriteLine("Webhooks triggered");
```

## Notes
The `WebhookServiceExtensions` class is designed to be thread-safe, allowing for concurrent access to the `WebhookService` instance. However, the `TriggerWebhooksAsync` method may throw exceptions if the underlying webhook triggering mechanism encounters errors. It is recommended to handle such exceptions accordingly. Additionally, the `ClearFailedWebhooks` method permanently removes failed webhooks, so use it with caution to avoid accidental data loss. The `GetWebhooksGroupedByEventType` method returns a snapshot of the current webhook grouping, which may change over time as webhooks are added or removed.
