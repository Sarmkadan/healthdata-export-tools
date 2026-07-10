# NotificationService

Central service for dispatching notifications to registered channels. It provides typed messages for export completion, failures, import progress, data-quality issues, and generic alerts, while exposing metadata and timing information for downstream processing.

## API

### `public NotificationService()`

Constructs a new `NotificationService` instance with an empty channel registry and default empty message properties.

### `public void RegisterChannel(ILogNotificationChannel channel)`

Registers a new channel to receive notifications. Channels are invoked in registration order when a notification is sent.

- **channel** – The channel to register; must not be `null`.
- Throws `ArgumentNullException` when `channel` is `null`.

### `public async Task NotifyExportCompletedAsync()`

Sends an export-completed notification to all registered channels. The message body indicates success and includes metadata such as file path and record count.

- Returns a `Task` that completes when all channels have processed the message or thrown.

### `public async Task NotifyExportFailedAsync()`

Sends an export-failed notification to all registered channels. The message body describes the failure and includes metadata such as error details and stack trace.

- Returns a `Task` that completes when all channels have processed the message or thrown.

### `public async Task NotifyImportProgressAsync(int processed, int total)`

Sends an import-progress notification to all registered channels. The message body reports the current progress and includes metadata such as processed and total records.

- **processed** – Number of records processed so far.
- **total** – Total number of records expected.
- Throws `ArgumentOutOfRangeException` when `processed` or `total` is negative, or when `processed` exceeds `total`.

### `public async Task NotifyDataQualityWarningsAsync(IEnumerable<string> warnings)`

Sends a data-quality warning notification to all registered channels. The message body lists the warnings and includes metadata such as warning count and individual messages.

- **warnings** – Collection of warning messages; must not be `null` and must not contain `null` entries.
- Throws `ArgumentNullException` when `warnings` is `null`.
- Throws `ArgumentException` when any entry in `warnings` is `null`.

### `public async Task SendNotificationAsync()`

Sends the current notification (subject, body, type, timestamp, and metadata) to all registered channels.

- Returns a `Task` that completes when all channels have processed the message or thrown.

### `public async Task SendAsync(LogNotificationChannel channel)`

Sends the current notification to the specified channel only.

- **channel** – The channel to notify; must not be `null`.
- Returns a `Task` that completes when the channel has processed the message or thrown.
- Throws `ArgumentNullException` when `channel` is `null`.

### `public int GetChannelCount()`

Returns the number of channels currently registered.

- Returns the count as an `int`.

### `public string Subject`

Gets or sets the notification subject line. Defaults to an empty string.

### `public string Body`

Gets or sets the notification body text. Defaults to an empty string.

### `public NotificationType Type`

Gets or sets the notification type. Defaults to `NotificationType.Generic`.

### `public DateTime Timestamp`

Gets or sets the timestamp of the notification. Defaults to `DateTime.UtcNow` when first accessed.

### `public Dictionary<string, string> Metadata`

Gets the mutable dictionary of metadata key–value pairs attached to the notification. Defaults to an empty dictionary.

## Usage
