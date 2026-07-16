// Health Data Export Tools

...

## NotificationServiceTests

The `NotificationServiceTests` class contains comprehensive unit tests for the `NotificationService` class. It tests various scenarios for sending notifications with different types (Info, Warning, Error) and registering notification channels. The tests ensure that the correct log level is used for each notification type and that registered channels receive notifications.

### Usage Example

```csharp
using HealthDataExportTools.Services;
using HealthDataExportTools.Tests;
using NSubstitute;
using Xunit;
using Microsoft.Extensions.Logging;

var mockLogger = Substitute.For<ILogger<NotificationService>>();
var notificationService = new NotificationService(mockLogger);

var notificationMessage = new NotificationMessage
{
    Subject = "Test Subject",
    Body = "Test Message",
    Type = NotificationType.Info,
    Timestamp = DateTime.UtcNow
};

await notificationService.SendNotificationAsync(notificationMessage);

// Register a channel and verify it receives the notification
var mockChannel = Substitute.For<INotificationChannel>();
notificationService.RegisterChannel(mockChannel);

await notificationService.SendNotificationAsync(notificationMessage);

await mockChannel.Received(1).SendAsync(Arg.Is<NotificationMessage>(nm => nm.Body.Contains(notificationMessage.Body)));
```
