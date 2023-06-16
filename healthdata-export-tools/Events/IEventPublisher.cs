// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace HealthDataExportTools.Events;

/// <summary>
/// Defines the contract for event publishing in pub-sub pattern
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Publish an event to all registered subscribers
    /// </summary>
    Task PublishAsync<TEvent>(TEvent @event) where TEvent : class, IEvent;

    /// <summary>
    /// Publish multiple events in sequence
    /// </summary>
    Task PublishBatchAsync<TEvent>(List<TEvent> events) where TEvent : class, IEvent;
}

/// <summary>
/// Defines the contract for event subscribers
/// </summary>
public interface IEventSubscriber<TEvent> where TEvent : class, IEvent
{
    /// <summary>
    /// Handle the event asynchronously
    /// </summary>
    Task HandleAsync(TEvent @event);
}

/// <summary>
/// Base interface for all events
/// </summary>
public interface IEvent
{
    /// <summary>
    /// Unique event identifier
    /// </summary>
    string EventId { get; }

    /// <summary>
    /// Timestamp when event was created
    /// </summary>
    DateTime OccurredAt { get; }

    /// <summary>
    /// Type of event for routing
    /// </summary>
    string EventType { get; }

    /// <summary>
    /// Event source/aggregate that triggered the event
    /// </summary>
    string AggregateId { get; }
}

/// <summary>
/// Base class for events
/// </summary>
public abstract class EventBase : IEvent
{
    public string EventId { get; }
    public DateTime OccurredAt { get; }
    public string EventType { get; }
    public string AggregateId { get; }

    protected EventBase(string aggregateId)
    {
        EventId = Guid.NewGuid().ToString();
        OccurredAt = DateTime.UtcNow;
        EventType = GetType().Name;
        AggregateId = aggregateId;
    }
}
