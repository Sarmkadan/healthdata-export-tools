// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace HealthDataExportTools.Events;

/// <summary>
/// Implements event bus with pub-sub pattern for decoupled event handling
/// Thread-safe implementation with async support
/// </summary>
public class EventBus : IEventPublisher
{
    private readonly Dictionary<Type, List<Delegate>> _handlers;
    private readonly ReaderWriterLockSlim _handlersLock;
    private readonly ILogger<EventBus> _logger;

    public EventBus(ILogger<EventBus> logger)
    {
        _handlers = new Dictionary<Type, List<Delegate>>();
        _handlersLock = new ReaderWriterLockSlim();
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Subscribe to a specific event type
    /// </summary>
    public void Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : class, IEvent
    {
        if (handler == null)
            throw new ArgumentNullException(nameof(handler));

        _handlersLock.EnterWriteLock();
        try
        {
            var eventType = typeof(TEvent);

            if (!_handlers.ContainsKey(eventType))
            {
                _handlers[eventType] = new List<Delegate>();
            }

            _handlers[eventType].Add(handler);

            _logger.LogInformation("Subscribed to event: {EventType}", eventType.Name);
        }
        finally
        {
            _handlersLock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Unsubscribe from a specific event type
    /// </summary>
    public void Unsubscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : class, IEvent
    {
        if (handler == null)
            return;

        _handlersLock.EnterWriteLock();
        try
        {
            var eventType = typeof(TEvent);

            if (_handlers.TryGetValue(eventType, out var eventHandlers))
            {
                eventHandlers.Remove(handler);

                if (eventHandlers.Count == 0)
                {
                    _handlers.Remove(eventType);
                }

                _logger.LogInformation("Unsubscribed from event: {EventType}", eventType.Name);
            }
        }
        finally
        {
            _handlersLock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Publish an event to all registered subscribers
    /// </summary>
    public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : class, IEvent
    {
        if (@event == null)
            throw new ArgumentNullException(nameof(@event));

        _handlersLock.EnterReadLock();
        try
        {
            var eventType = typeof(TEvent);

            if (!_handlers.TryGetValue(eventType, out var eventHandlers) || eventHandlers.Count == 0)
            {
                _logger.LogWarning("No handlers found for event: {EventType}", eventType.Name);
                return;
            }

            _logger.LogInformation(
                "Publishing event: {EventType} (ID: {EventId}) to {HandlerCount} handlers",
                @event.EventType, @event.EventId, eventHandlers.Count);

            var tasks = new List<Task>();

            foreach (var handler in eventHandlers.Cast<Func<TEvent, Task>>())
            {
                try
                {
                    tasks.Add(handler(@event));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing event handler for: {EventType}", eventType.Name);
                }
            }

            await Task.WhenAll(tasks);
        }
        finally
        {
            _handlersLock.ExitReadLock();
        }
    }

    /// <summary>
    /// Publish multiple events in sequence
    /// </summary>
    public async Task PublishBatchAsync<TEvent>(List<TEvent> events) where TEvent : class, IEvent
    {
        if (events == null || events.Count == 0)
            return;

        _logger.LogInformation("Publishing batch of {Count} events", events.Count);

        foreach (var @event in events)
        {
            await PublishAsync(@event);
        }
    }

    /// <summary>
    /// Get list of subscribed handlers for an event type
    /// </summary>
    public int GetSubscriberCount<TEvent>() where TEvent : class, IEvent
    {
        _handlersLock.EnterReadLock();
        try
        {
            var eventType = typeof(TEvent);
            return _handlers.TryGetValue(eventType, out var handlers) ? handlers.Count : 0;
        }
        finally
        {
            _handlersLock.ExitReadLock();
        }
    }

    /// <summary>
    /// Clear all subscriptions for a specific event type
    /// </summary>
    public void ClearSubscribers<TEvent>() where TEvent : class, IEvent
    {
        _handlersLock.EnterWriteLock();
        try
        {
            var eventType = typeof(TEvent);
            if (_handlers.Remove(eventType))
            {
                _logger.LogInformation("Cleared all subscribers for event: {EventType}", eventType.Name);
            }
        }
        finally
        {
            _handlersLock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Clear all subscriptions
    /// </summary>
    public void ClearAllSubscribers()
    {
        _handlersLock.EnterWriteLock();
        try
        {
            _handlers.Clear();
            _logger.LogInformation("Cleared all event subscribers");
        }
        finally
        {
            _handlersLock.ExitWriteLock();
        }
    }

    public void Dispose()
    {
        _handlersLock?.Dispose();
    }
}
