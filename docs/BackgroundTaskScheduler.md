# BackgroundTaskScheduler

The `BackgroundTaskScheduler` type provides a simple API for scheduling and monitoring background work items. It allows one‑time and recurring tasks to be queued, queried, and cancelled while exposing execution metadata such as timestamps and counts.

## API

### BackgroundTaskScheduler()
Creates a new instance of the scheduler. The instance starts in a ready state and can be used immediately to schedule tasks.

### string ScheduleOnce()
Schedules a one‑time background task to be executed as soon as possible.  
- **Parameters:** none.  
- **Return value:** a string identifier that uniquely represents the scheduled task.  
- **Exceptions:**  
  - `ObjectDisposedException` if the scheduler has been disposed.  
  - `InvalidOperationException` if the scheduler is in an invalid state (e.g., already shutdown).

### string ScheduleRecurring()
Schedules a recurring background task that will repeat according to its interval.  
- **Parameters:** none.  
- **Return value:** a string identifier for the recurring task.  
- **Exceptions:** same as `ScheduleOnce`.

### bool CancelTask()
Attempts to cancel the most recently scheduled task.  
- **Parameters:** none.  
- **Return value:** `true` if a task was found and cancellation was requested; `false` if no cancellable task exists.  
- **Exceptions:**  
  - `ObjectDisposedException` if the scheduler has been disposed.  
  - `InvalidOperationException` if the scheduler cannot process cancellations at this time.

### List<ScheduledTask> GetScheduledTasks()
Retrieves a snapshot of all tasks currently known to the scheduler.  
- **Parameters:** none.  
- **Return value:** a list of `ScheduledTask` objects representing each scheduled task. The list is a copy; modifications to the list do not affect the scheduler’s internal state.  
- **Exceptions:**  
  - `ObjectDisposedException` if the scheduler has been disposed.

### ScheduledTask? GetTask()
Returns the task associated with the scheduler’s current context (if any).  
- **Parameters:** none.  
- **Return value:** a `ScheduledTask` instance, or `null` when no task is associated.  
- **Exceptions:**  
  - `ObjectDisposedException` if the scheduler has been disposed.

### string Id
Gets the unique identifier of the scheduler instance.  
- **Get-only.** No exceptions are thrown under normal operation.

### string Name
Gets or sets a descriptive name for the scheduler.  
- **Get/Set.** Setting to `null` throws `null` results in an `ArgumentNullException`.  
- **Exceptions:** `ObjectDisposedException` if the scheduler has been disposed.

### DateTime? ExecuteAt
Gets the scheduled point in time for the next execution of the task.  
- **Get-only.** Returns `null` when no execution time is defined (e.g., for a cancelled task).  
- **Exceptions:** `ObjectDisposedException` if the scheduler has been disposed.

### TimeSpan? Interval
Gets the recurrence interval for a repeating task.  
- **Get-only.** Returns `null` for non‑recurring (one‑time) tasks.  
- **Exceptions:** `ObjectDisposedException` if the scheduler has been disposed.

### bool IsRecurring
Indicates whether the scheduled task is configured to repeat.  
- **Get-only.** `true` for recurring tasks, `false` for one‑time tasks.  
- **Exceptions:** `ObjectDisposedException` if the scheduler has been disposed.

### DateTime CreatedAt
Gets the timestamp when the scheduler instance was instantiated.  
- **Get-only.** No exceptions are thrown under normal operation.

### DateTime? LastExecutedAt
Gets the timestamp of the most recent execution attempt of the task.  
- **Get-only.** Returns `null` if the task has never been executed.  
- **Exceptions:** `ObjectDisposedException` if the scheduler has been disposed.

### DateTime? LastCompletedAt
Gets the timestamp of the most recent successful completion of the task.  
- **Get-only.** Returns `null` if the task has never completed successfully.  
- **Exceptions:** `ObjectDisposedException` if the scheduler has been disposed.

### int ExecutionCount
Gets the total number of times the task has been executed (including failed attempts).  
- **Get-only.** No exceptions are thrown under normal operation.

## Usage

### Example 1: Scheduling a one‑time task and monitoring its execution
```csharp
using var scheduler = new BackgroundTaskScheduler();

// Schedule a task that will run as soon as possible.
string taskId = scheduler.ScheduleOnce();

// Retrieve the list of scheduled tasks to verify.
var tasks = scheduler.GetScheduledTasks();
Console.WriteLine($"Scheduled {tasks.Count} task(s).");

// Later, check execution metadata.
ScheduledTask? task = scheduler.GetTask();
if (task != null)
{
    Console.WriteLine($"Task {task.Id} executed {task.ExecutionCount} time(s).");
    Console.WriteLine($"Last run: {task.LastExecutedAt}");
}
```

### Example 2: Scheduling a recurring task and cancelling it
```csharp
var scheduler = new BackgroundTaskScheduler { Name = "ExportWorker" };

// Schedule a recurring task (interval and logic are configured elsewhere).
string recurringId = scheduler.ScheduleRecurring();
Console.WriteLine($"Recurring task scheduled with ID {recurringId}.");

// Simulate some work...
Thread.Sleep(TimeSpan.FromSeconds(5));

// Attempt to cancel the recurring task.
bool cancelled = scheduler.CancelTask();
Console.WriteLine($"Cancellation requested: {cancelled}");

// Verify that the task is no longer considered recurring.
ScheduledTask? task = scheduler.GetTask();
if (task != null)
{
    Console.WriteLine($"IsRecurring after cancel: {task.IsRecurring}");
}
```

## Notes
- All members that access internal state throw `ObjectDisposedException` if the scheduler has been disposed via a `Dispose()` call (not shown in the public signature but assumed to exist).  
- The scheduler does not guarantee ordered execution of multiple tasks; tasks are processed by the underlying thread pool as resources become available.  
- `GetScheduledTasks()` returns a copy; modifications to the returned list do not affect the scheduler’s internal collection.  
- Changing the `Name` property while a task is executing is safe, but the new name will only be reflected in subsequent queries.  
- The `ExecutionCount` property is incremented each time the task’s delegate is invoked, regardless of whether the invocation completes successfully or throws an exception.  
- Thread‑safety: read‑only properties and methods that return snapshots (`GetScheduledTasks`, `GetTask`) are safe to call concurrently with scheduling or cancellation calls. However, calling `ScheduleOnce` or `ScheduleRecurring` concurrently from multiple threads may result in non‑deterministic ordering of task IDs; external synchronization is required if a deterministic order is needed.  
- Passing `null` to the `Name` setter throws `ArgumentNullException`; all other setters (if any) follow similar validation for invalid arguments.  
- The scheduler does not persist scheduled tasks across application restarts; all state is lost when the instance is disposed or the process ends.
