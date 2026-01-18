// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using NSubstitute;
using Xunit;
using HealthDataExportTools.Services;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace HealthDataExportTools.Tests;

public class NotificationServiceTests
{
    private readonly ILogger<NotificationService> _mockLogger;
    private readonly NotificationService _sut;

    public NotificationServiceTests()
    {
        _mockLogger = Substitute.For<ILogger<NotificationService>>();
        _sut = new NotificationService(_mockLogger);
    }

    [Fact]
    public async Task SendNotification_InfoType_LogsInformationMessage()
    {
        // Arrange
        var messageBody = "Test Info Message";
        var notificationMessage = new NotificationMessage
        {
            Subject = "Test Subject",
            Body = messageBody,
            Type = NotificationType.Info,
            Timestamp = DateTime.UtcNow
        };

        // Act
        await _sut.SendNotificationAsync(notificationMessage);

        // Assert
        _mockLogger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString().Contains(messageBody)),
            null,
            Arg.Any<Func<object, Exception, string>>());
    }

    [Fact]
    public async Task SendNotification_WarningType_LogsWarningMessage()
    {
        // Arrange
        var messageBody = "Test Warning Message";
        var notificationMessage = new NotificationMessage
        {
            Subject = "Test Subject",
            Body = messageBody,
            Type = NotificationType.Warning,
            Timestamp = DateTime.UtcNow
        };

        // Act
        await _sut.SendNotificationAsync(notificationMessage);

        // Assert
        _mockLogger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString().Contains(messageBody)),
            null,
            Arg.Any<Func<object, Exception, string>>());
    }

    [Fact]
    public async Task SendNotification_ErrorType_LogsErrorMessage()
    {
        // Arrange
        var messageBody = "Test Error Message";
        var exception = new Exception("Test Exception");
        var notificationMessage = new NotificationMessage
        {
            Subject = "Test Subject",
            Body = messageBody,
            Type = NotificationType.Error,
            Timestamp = DateTime.UtcNow
        };

        // Act
        await _sut.SendNotificationAsync(notificationMessage);

        // Assert
        _mockLogger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString().Contains(messageBody)),
            exception, // The actual service logs the exception, not passes it to SendNotificationAsync directly
            Arg.Any<Func<object, Exception, string>>());
    }

    [Fact]
    public async Task SendNotification_UnhandledType_DefaultsToInformation()
    {
        // Arrange
        var messageBody = "Test Unhandled Type";
        var notificationMessage = new NotificationMessage
        {
            Subject = "Test Subject",
            Body = messageBody,
            Type = (NotificationType)999, // Unhandled enum value
            Timestamp = DateTime.UtcNow
        };

        // Act
        await _sut.SendNotificationAsync(notificationMessage);

        // Assert
        _mockLogger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString().Contains(messageBody)),
            null,
            Arg.Any<Func<object, Exception, string>>());
    }

    [Fact]
    public async Task RegisterChannel_ChannelReceivesNotification()
    {
        // Arrange
        var mockChannel = Substitute.For<INotificationChannel>();
        _sut.RegisterChannel(mockChannel);
        var messageBody = "Channel Test Message";
        var notificationMessage = new NotificationMessage
        {
            Subject = "Channel Test",
            Body = messageBody,
            Type = NotificationType.Info,
            Timestamp = DateTime.UtcNow
        };

        // Act
        await _sut.SendNotificationAsync(notificationMessage);

        // Assert
        await mockChannel.Received(1).SendAsync(Arg.Is<NotificationMessage>(nm => nm.Body.Contains(messageBody)));
    }


}
