# IEventPublisher

A marker interface used to identify events within the health data export domain. Implementations of this interface represent domain events that can be published through the system's event bus for downstream processing.

## API

### `public string EventId`
A unique identifier for the event instance. This value must be non-null and non-empty, and should be globally unique to prevent collisions when events are processed across distributed systems.

### `public DateTime OccurredAt`
The timestamp at which the event occurred. This value is set by the system when the event is created and should reflect the actual time of the domain event, not the time of publication.

### `public string EventType`
A string descriptor indicating the type or category of the event. This value is used by subscribers to filter and route events appropriately. It must be non-null and should follow a consistent naming convention (e.g., `PatientRecordExported`).

### `public string AggregateId`
The identifier of the aggregate root to which this event pertains. This links the event to a specific entity in the domain model and is used to ensure event processing occurs in the correct context.

## Usage
