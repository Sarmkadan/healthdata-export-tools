# EventBus

A lightweight, in-memory event bus for decoupled communication between components in .NET applications. It supports typed events, asynchronous publishing, and dynamic subscription management, making it suitable for scenarios like domain event handling, service-to-service notifications, or cross-cutting concerns in modular architectures.

## API

### `EventBus()`
Initializes a new instance of the `EventBus` class with no subscribers.

### `void Subscribe<TEvent>(Action<TEvent> handler)`
Registers a handler for events of type `TEvent`.

- **Parameters**
  - `handler`: The delegate to invoke when an event of type `TEvent` is published.
- **Throws**
  - `ArgumentNullException`: If `handler` is `null`.

### `void Unsubscribe<TEvent>(Action<TEvent> handler)`
Removes a previously registered handler for events of type `TEvent`.

- **Parameters**
  - `handler`: The delegate to remove from the subscription list.
- **Throws**
  - `ArgumentNullException`: If `handler` is `null`.

### `async Task PublishAsync<TEvent>(TEvent @event)`
Asynchronously invokes all subscribed handlers for events of type `TEvent` in the order they were subscribed.

- **Parameters**
  - `@event`: The event instance to publish.
- **Returns**
  - A `Task` representing the asynchronous operation.
- **Throws**
  - `ArgumentNullException`: If `@event` is `null`.

### `async Task PublishBatchAsync<TEvent>(IEnumerable<TEvent> events)`
Asynchronously invokes all subscribed handlers for each event in the provided sequence, preserving order of both events and handlers.

- **Parameters**
  - `events`: A sequence of event instances to publish.
- **Returns**
  - A `Task` representing the asynchronous operation.
- **Throws**
  - `ArgumentNullException`: If `events` is `null`.

### `int GetSubscriberCount<TEvent>()`
Returns the number of handlers currently subscribed to events of type `TEvent`.

- **Returns**
  - The count of active subscribers for `TEvent`.

### `void ClearSubscribers<TEvent>()`
Removes all handlers subscribed to events of type `TEvent`.

### `void ClearAllSubscribers()`
Removes all handlers across all event types.

### `void Dispose()`
Releases all resources used by the `EventBus` and clears all subscribers. After disposal, the instance must not be used.

## Usage

### Basic Subscription and Publishing
