# WebhookService

The `WebhookService` manages the registration, triggering, and lifecycle of webhooks for asynchronous event notifications. It provides methods to register/unregister webhooks, trigger events to registered endpoints, and track invocation statistics and status.

## API

### `WebhookService`
Constructor for the webhook service. Initializes the service with default or injected dependencies for webhook management.

### `void RegisterWebhook(string url, string eventType)`
Registers a new webhook with the specified URL and event type.
- **Parameters**:
  - `url`: The endpoint URL to invoke when the event occurs.
  - `eventType`: The type of event this webhook subscribes to.
- **Throws**: `ArgumentNullException` if `url` or `eventType` is null.
- **Throws**: `ArgumentException` if `url` is not a valid URI or `eventType` is empty.

### `bool UnregisterWebhook(string id)`
Removes a registered webhook by its unique identifier.
- **Parameters**:
  - `id`: The unique identifier of the webhook to remove.
- **Returns**: `true` if the webhook was found and removed; otherwise, `false`.
- **Throws**: `ArgumentNullException` if `id` is null.

### `async Task TriggerWebhooksAsync(string eventType, object? payload, int retryCount = 0)`
Triggers all registered webhooks matching the specified event type, invoking their endpoints with the provided payload.
- **Parameters**:
  - `eventType`: The event type to match against registered webhooks.
  - `payload`: The data payload to send to the webhook endpoints.
  - `retryCount`: The current retry attempt number (used internally for retries).
- **Returns**: A task representing the asynchronous operation.
- **Throws**: `ArgumentNullException` if `eventType` is null.
- **Throws**: `InvalidOperationException` if the HTTP client fails to send requests.

### `List<Webhook> GetRegisteredWebhooks()`
Retrieves a list of all currently registered webhooks.
- **Returns**: A list of `Webhook` objects representing the registered webhooks.

### `bool SetWebhookActive(string id, bool isActive)`
Updates the active status of a registered webhook.
- **Parameters**:
  - `id`: The unique identifier of the webhook.
  - `isActive`: The desired active status.
- **Returns**: `true` if the webhook was found and updated; otherwise, `false`.
- **Throws**: `ArgumentNullException` if `id` is null.

### `string Id` (Property)
Gets the unique identifier of the webhook. Read-only.

### `string Url` (Property)
Gets the endpoint URL of the webhook. Read-only.

### `string EventType` (Property)
Gets the event type this webhook subscribes to. Read-only.

### `bool IsActive` (Property)
Gets or sets whether the webhook is active and should receive events. Defaults to `true`.

### `DateTime CreatedAt` (Property)
Gets the timestamp when the webhook was registered. Read-only.

### `DateTime? LastInvokedAt` (Property)
Gets the timestamp of the last successful invocation, or `null` if never invoked. Read-only.

### `int SuccessCount` (Property)
Gets the number of successful invocations. Read-only.

### `int FailureCount` (Property)
Gets the number of failed invocations. Read-only.

### `string EventType` (Property in `WebhookTrigger` nested type)
Gets the event type that triggered the webhook invocation. Read-only.

### `DateTime Timestamp` (Property in `WebhookTrigger` nested type)
Gets the timestamp when the webhook was triggered. Read-only.

### `object? Payload` (Property in `WebhookTrigger` nested type)
Gets the payload data sent during the webhook trigger. Read-only.

### `int RetryCount` (Property in `WebhookTrigger` nested type)
Gets the number of retry attempts made for this trigger. Read-only.

## Usage

### Registering and Triggering a Webhook
