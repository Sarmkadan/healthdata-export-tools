# NotificationServiceTests
The `NotificationServiceTests` class is designed to test the functionality of the `NotificationService` class, which is responsible for sending notifications. This test class provides a set of test methods to verify that the `NotificationService` behaves as expected under different scenarios, including sending notifications of various types and registering channels to receive notifications.

## API
The `NotificationServiceTests` class has the following public members:
* `public NotificationServiceTests`: The constructor for the `NotificationServiceTests` class.
* `public async Task SendNotification_InfoType_LogsInformationMessage`: Tests that sending a notification of type "Info" results in an information message being logged.
* `public async Task SendNotification_WarningType_LogsWarningMessage`: Tests that sending a notification of type "Warning" results in a warning message being logged.
* `public async Task SendNotification_ErrorType_LogsErrorMessage`: Tests that sending a notification of type "Error" results in an error message being logged.
* `public async Task SendNotification_UnhandledType_DefaultsToInformation`: Tests that sending a notification of an unhandled type defaults to logging an information message.
* `public async Task RegisterChannel_ChannelReceivesNotification`: Tests that a registered channel receives notifications as expected.

## Usage
Here are two examples of using the `NotificationServiceTests` class:
```csharp
// Example 1: Testing notification sending
var tests = new NotificationServiceTests();
await tests.SendNotification_InfoType_LogsInformationMessage();
await tests.SendNotification_WarningType_LogsWarningMessage();
await tests.SendNotification_ErrorType_LogsErrorMessage();

// Example 2: Testing channel registration
var tests = new NotificationServiceTests();
await tests.RegisterChannel_ChannelReceivesNotification();
```

## Notes
When using the `NotificationServiceTests` class, note that the test methods are asynchronous and should be awaited to ensure that the tests complete as expected. Additionally, the test methods do not throw exceptions under normal circumstances, but may throw exceptions if the underlying `NotificationService` class is not functioning correctly. The `NotificationServiceTests` class is designed to be thread-safe, but it is still important to ensure that the test methods are not called concurrently to avoid any potential issues. Edge cases, such as sending notifications with null or empty messages, are not explicitly tested by the `NotificationServiceTests` class and may require additional testing.
