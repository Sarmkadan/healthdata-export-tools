#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HealthDataExportTools.Events;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace HealthDataExportTools.Tests
{
    /// <summary>
    /// Simple test event used for the EventBus unit tests.
    /// Implements <see cref="IEvent"/> directly to avoid dependencies on internal event base classes.
    /// </summary>
    private sealed class TestEvent : IEvent
    {
        public string EventType => nameof(TestEvent);
        public Guid EventId { get; } = Guid.NewGuid();
        public string Message { get; }

        public TestEvent(string message) => Message = message;
    }

    public class EventBusTests
    {
        private readonly EventBus _eventBus;

        public EventBusTests()
        {
            // Use a null logger to keep test output clean.
            _eventBus = new EventBus(NullLogger<EventBus>.Instance);
        }

        [Fact]
        public void Publish_Should_Invoke_Subscriber()
        {
            // Arrange
            var called = false;
            _eventBus.Subscribe<TestEvent>(e => called = true);

            // Act
            _eventBus.Publish(new TestEvent("hello"));

            // Assert
            Assert.True(called, "Subscriber was not invoked by Publish.");
        }

        [Fact]
        public void Publish_Should_Invoke_Multiple_Subscribers()
        {
            // Arrange
            var callCount = 0;
            _eventBus.Subscribe<TestEvent>(e => callCount++);
            _eventBus.Subscribe<TestEvent>(e => callCount++);

            // Act
            _eventBus.Publish(new TestEvent("multi"));

            // Assert
            Assert.Equal(2, callCount);
        }

        [Fact]
        public void Unsubscribe_Should_Remove_Subscriber()
        {
            // Arrange
            var called = false;
            Action<TestEvent> handler = e => called = true;
            _eventBus.Subscribe(handler);
            _eventBus.Unsubscribe(handler);

            // Act
            _eventBus.Publish(new TestEvent("unsub"));

            // Assert
            Assert.False(called, "Handler was invoked after being unsubscribed.");
        }

        [Fact]
        public void Publish_Should_Not_Propagate_Handler_Exception()
        {
            // Arrange
            var called = false;
            _eventBus.Subscribe<TestEvent>(e => throw new InvalidOperationException("boom"));
            _eventBus.Subscribe<TestEvent>(e => called = true);

            // Act & Assert
            var exception = Record.Exception(() => _eventBus.Publish(new TestEvent("exception")));
            Assert.Null(exception);
            Assert.True(called, "Other handlers were not executed after an exception in a sibling handler.");
        }

        [Fact]
        public async Task PublishAsync_Should_Invoke_Sync_And_Async_Handlers()
        {
            // Arrange
            var syncCalled = false;
            var asyncCalled = false;

            _eventBus.Subscribe<TestEvent>(e => syncCalled = true);
            _eventBus.SubscribeAsync<TestEvent>(e =>
            {
                asyncCalled = true;
                return Task.CompletedTask;
            });

            // Act
            await _eventBus.PublishAsync(new TestEvent("async mix"));

            // Assert
            Assert.True(syncCalled, "Synchronous handler was not called.");
            Assert.True(asyncCalled, "Asynchronous handler was not called.");
        }

        [Fact]
        public async Task PublishAsync_Should_Not_Propagate_Handler_Exception()
        {
            // Arrange
            var asyncCalled = false;
            _eventBus.SubscribeAsync<TestEvent>(e => throw new InvalidOperationException("async boom"));
            _eventBus.SubscribeAsync<TestEvent>(e =>
            {
                asyncCalled = true;
                return Task.CompletedTask;
            });

            // Act & Assert
            var exception = await Record.ExceptionAsync(() => _eventBus.PublishAsync(new TestEvent("async exception")));
            Assert.Null(exception);
            Assert.True(asyncCalled, "Other async handlers were not executed after an exception in a sibling handler.");
        }
    }
}
