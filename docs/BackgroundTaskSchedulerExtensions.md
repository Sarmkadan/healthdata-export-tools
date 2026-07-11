# BackgroundTaskSchedulerExtensions

Provides extension methods for scheduling, querying, and managing background tasks within the `healthdata-export-tools` project. These methods simplify interaction with the task scheduler by offering strongly-typed wrappers around common operations such as scheduling one-off or recurring tasks, canceling tasks by name, and retrieving task statuses.

## API

### `ScheduleOnce(IServiceProvider services, string taskName, Action<CancellationToken> taskAction, DateTimeOffset runAt, string? groupName = null)`
Schedules a one-time background task to execute at a specified time.

- **Parameters**
  - `services`: An `IServiceProvider` used to resolve required services.
  - `taskName`: A unique identifier for the task.
  - `taskAction`: The delegate to invoke when the task runs.
  - `runAt`: The time at which the task should execute.
  - `groupName`: Optional group name to categorize the task.
- **Return value**: A unique task identifier string.
- **Exceptions**: Throws `ArgumentNullException` if `services`, `taskName`, or `taskAction` is `null`. Throws `ArgumentException` if `taskName` is empty or whitespace.

---

### `ScheduleRecurring(IServiceProvider services, string taskName, Action<CancellationToken> taskAction, TimeSpan interval, string? groupName = null)`
Schedules a recurring background task that runs at fixed intervals.

- **Parameters**
  - `services`: An `IServiceProvider` used to resolve required services.
  - `taskName`: A unique identifier for the task.
  - `taskAction`: The delegate to invoke when the task runs.
  - `interval`: The time between task executions.
  - `groupName`: Optional group name to categorize the task.
- **Return value**: A unique task identifier string.
- **Exceptions**: Throws `ArgumentNullException` if `services`, `taskName`, or `taskAction` is `null`. Throws `ArgumentException` if `taskName` is empty or whitespace or if `interval` is zero or negative.

---
### `CancelTasksByName(IServiceProvider services, string taskName)`
Cancels all pending tasks matching the specified name.

- **Parameters**
  - `services`: An `IServiceProvider` used to resolve required services.
  - `taskName`: The name of tasks to cancel.
- **Return value**: The number of tasks canceled.
- **Exceptions**: Throws `ArgumentNullException` if `services` or `taskName` is `null`. Throws `ArgumentException` if `taskName` is empty or whitespace.

---
### `GetPendingTasks(IServiceProvider services)`
Retrieves all tasks that are scheduled but not yet started.

- **Parameters**
  - `services`: An `IServiceProvider` used to resolve required services.
- **Return value**: A list of `ScheduledTask` objects representing pending tasks. Never `null`.
- **Exceptions**: Throws `ArgumentNullException` if `services` is `null`.

---
### `GetRunningTasks(IServiceProvider services)`
Retrieves all tasks currently executing.

- **Parameters**
  - `services`: An `IServiceProvider` used to resolve required services.
- **Return value**: A list of `ScheduledTask` objects representing running tasks. Never `null`.
- **Exceptions**: Throws `ArgumentNullException` if `services` is `null`.

---
### `GetCompletedTasks(IServiceProvider services)`
Retrieves all tasks that have finished execution.

- **Parameters**
  - `services`: An `IServiceProvider` used to resolve required services.
- **Return value**: A list of `ScheduledTask` objects representing completed tasks. Never `null`.
- **Exceptions**: Throws `ArgumentNullException` if `services` is `null`.

---
### `GetTaskByName(IServiceProvider services, string taskName)`
Finds a task by its unique name.

- **Parameters**
  - `services`: An `IServiceProvider` used to resolve required services.
  - `taskName`: The name of the task to find.
- **Return value**: The `ScheduledTask` instance if found; otherwise `null`.
- **Exceptions**: Throws `ArgumentNullException` if `services` or `taskName` is `null`. Throws `ArgumentException` if `taskName` is empty or whitespace.

---
### `TaskExists(IServiceProvider services, string taskName)`
Determines whether a task with the given name exists.

- **Parameters**
  - `services`: An `IServiceProvider` used to resolve required services.
  - `taskName`: The name of the task to check.
- **Return value**: `true` if a task with the name exists; otherwise `false`.
- **Exceptions**: Throws `ArgumentNullException` if `services` or `taskName` is `null`. Throws `ArgumentException` if `taskName` is empty or whitespace.

---
### `GetTaskCount(IServiceProvider services)`
Gets the total number of tracked tasks.

- **Parameters**
  - `services`: An `IServiceProvider` used to resolve required services.
- **Return value**: The total number of tasks across all states.
- **Exceptions**: Throws `ArgumentNullException` if `services` is `null`.

---
### `GetPendingTaskCount(IServiceProvider services)`
Gets the number of tasks that are scheduled but not yet started.

- **Parameters**
  - `services`: An `IServiceProvider` used to resolve required services.
- **Return value**: The count of pending tasks.
- **Exceptions**: Throws `ArgumentNullException` if `services` is `null`.

---
### `GetRunningTaskCount(IServiceProvider services)`
Gets the number of tasks currently executing.

- **Parameters**
  - `services`: An `IServiceProvider` used to resolve required services.
- **Return value**: The count of running tasks.
- **Exceptions**: Throws `ArgumentNullException` if `services` is `null`.

---
### `GetCompletedTaskCount(IServiceProvider services)`
Gets the number of tasks that have finished execution.

- **Parameters**
  - `services`: An `IServiceProvider` used to resolve required services.
- **Return value**: The count of completed tasks.
- **Exceptions**: Throws `ArgumentNullException` if `services` is `null`.

## Usage
