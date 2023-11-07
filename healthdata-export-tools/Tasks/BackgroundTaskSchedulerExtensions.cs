#nullable enable

namespace HealthDataExportTools.Tasks;

/// <summary>
/// Extension methods for <see cref="BackgroundTaskScheduler"/> providing additional functionality
/// for scheduling, managing, and querying background tasks.
/// </summary>
public static class BackgroundTaskSchedulerExtensions
{
    /// <summary>
    /// Schedules a task to run once after a specified delay.
    /// </summary>
    /// <param name="scheduler">The task scheduler instance.</param>
    /// <param name="taskName">Name of the task to schedule.</param>
    /// <param name="action">The asynchronous action to execute.</param>
    /// <param name="delay">Delay before the task should execute.</param>
    /// <returns>The unique identifier for the scheduled task.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="scheduler"/> or <paramref name="action"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="taskName"/> is <see langword="null"/> or empty, or <paramref name="delay"/> is negative.</exception>
    public static string ScheduleOnce(this BackgroundTaskScheduler scheduler, string taskName, Func<Task> action, TimeSpan delay)
    {
        ArgumentNullException.ThrowIfNull(scheduler);
        ArgumentNullException.ThrowIfNull(action);
        ArgumentException.ThrowIfNullOrEmpty(taskName);

        if (delay.TotalMilliseconds < 0)
            throw new ArgumentException("Delay must be non-negative", nameof(delay));

        var executeAt = DateTime.UtcNow.Add(delay);
        return scheduler.ScheduleOnce(taskName, action, executeAt);
    }

    /// <summary>
    /// Schedules a recurring task with an initial delay before the first execution.
    /// </summary>
    /// <param name="scheduler">The task scheduler instance.</param>
    /// <param name="taskName">Name of the task to schedule.</param>
    /// <param name="action">The asynchronous action to execute.</param>
    /// <param name="interval">Interval between executions.</param>
    /// <param name="initialDelay">Initial delay before the first execution. If not specified, uses the interval value.</param>
    /// <returns>The unique identifier for the scheduled task.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="scheduler"/> or <paramref name="action"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="taskName"/> is <see langword="null"/> or empty, or <paramref name="interval"/> is not positive.</exception>
    public static string ScheduleRecurring(this BackgroundTaskScheduler scheduler, string taskName, Func<Task> action, TimeSpan interval, TimeSpan initialDelay)
    {
        ArgumentNullException.ThrowIfNull(scheduler);
        ArgumentNullException.ThrowIfNull(action);
        ArgumentException.ThrowIfNullOrEmpty(taskName);

        if (interval.TotalMilliseconds <= 0)
            throw new ArgumentException("Interval must be positive", nameof(interval));

        if (initialDelay.TotalMilliseconds <= 0)
            throw new ArgumentException("Initial delay must be positive", nameof(initialDelay));

        return scheduler.ScheduleRecurring(taskName, action, interval, initialDelay);
    }

    /// <summary>
    /// Cancels all tasks matching a specific name pattern.
    /// </summary>
    /// <param name="scheduler">The task scheduler instance.</param>
    /// <param name="namePattern">Pattern to match task names (supports * wildcard).</param>
    /// <returns>Number of tasks cancelled.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="scheduler"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="namePattern"/> is <see langword="null"/> or empty.</exception>
    public static int CancelTasksByName(this BackgroundTaskScheduler scheduler, string namePattern)
    {
        ArgumentNullException.ThrowIfNull(scheduler);
        ArgumentException.ThrowIfNullOrEmpty(namePattern);

        var wildcardPattern = namePattern.Replace("*", ".*");
        var regex = new System.Text.RegularExpressions.Regex(wildcardPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        var tasksToCancel = scheduler.GetScheduledTasks()
            .Where(t => regex.IsMatch(t.Name))
            .Select(t => t.Id)
            .ToList();

        int cancelledCount = 0;
        foreach (var taskId in tasksToCancel)
        {
            if (scheduler.CancelTask(taskId))
            {
                cancelledCount++;
            }
        }

        return cancelledCount;
    }

    /// <summary>
    /// Gets all pending tasks (tasks that haven't been executed yet).
    /// </summary>
    /// <param name="scheduler">The task scheduler instance.</param>
    /// <returns>List of pending tasks.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="scheduler"/> is <see langword="null"/>.</exception>
    public static List<ScheduledTask> GetPendingTasks(this BackgroundTaskScheduler scheduler)
    {
        ArgumentNullException.ThrowIfNull(scheduler);

        return scheduler.GetScheduledTasks()
            .Where(t => t.IsPending)
            .ToList();
    }

    /// <summary>
    /// Gets all running tasks (tasks currently being executed).
    /// </summary>
    /// <param name="scheduler">The task scheduler instance.</param>
    /// <returns>List of running tasks.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="scheduler"/> is <see langword="null"/>.</exception>
    public static List<ScheduledTask> GetRunningTasks(this BackgroundTaskScheduler scheduler)
    {
        ArgumentNullException.ThrowIfNull(scheduler);

        return scheduler.GetScheduledTasks()
            .Where(t => t.IsRunning)
            .ToList();
    }

    /// <summary>
    /// Gets all completed tasks.
    /// </summary>
    /// <param name="scheduler">The task scheduler instance.</param>
    /// <returns>List of completed tasks.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="scheduler"/> is <see langword="null"/>.</exception>
    public static List<ScheduledTask> GetCompletedTasks(this BackgroundTaskScheduler scheduler)
    {
        ArgumentNullException.ThrowIfNull(scheduler);

        return scheduler.GetScheduledTasks()
            .Where(t => t.IsCompleted)
            .ToList();
    }

    /// <summary>
    /// Finds a task by name.
    /// </summary>
    /// <param name="scheduler">The task scheduler instance.</param>
    /// <param name="taskName">Name of the task to find.</param>
    /// <returns>Found task or null if not found.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="scheduler"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="taskName"/> is <see langword="null"/> or empty.</exception>
    public static ScheduledTask? GetTaskByName(this BackgroundTaskScheduler scheduler, string taskName)
    {
        ArgumentNullException.ThrowIfNull(scheduler);
        ArgumentException.ThrowIfNullOrEmpty(taskName);

        return scheduler.GetScheduledTasks()
            .FirstOrDefault(t => string.Equals(t.Name, taskName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Checks if a task with the given name exists.
    /// </summary>
    /// <param name="scheduler">The task scheduler instance.</param>
    /// <param name="taskName">Name of the task to check.</param>
    /// <returns>True if task exists, false otherwise.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="scheduler"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="taskName"/> is <see langword="null"/> or empty.</exception>
    public static bool TaskExists(this BackgroundTaskScheduler scheduler, string taskName)
    {
        return scheduler.GetTaskByName(taskName) != null;
    }

    /// <summary>
    /// Gets the count of scheduled tasks.
    /// </summary>
    /// <param name="scheduler">The task scheduler instance.</param>
    /// <returns>Number of scheduled tasks.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="scheduler"/> is <see langword="null"/>.</exception>
    public static int GetTaskCount(this BackgroundTaskScheduler scheduler)
    {
        ArgumentNullException.ThrowIfNull(scheduler);
        return scheduler.GetScheduledTasks().Count;
    }

    /// <summary>
    /// Gets the count of pending tasks.
    /// </summary>
    /// <param name="scheduler">The task scheduler instance.</param>
    /// <returns>Number of pending tasks.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="scheduler"/> is <see langword="null"/>.</exception>
    public static int GetPendingTaskCount(this BackgroundTaskScheduler scheduler)
    {
        ArgumentNullException.ThrowIfNull(scheduler);
        return scheduler.GetPendingTasks().Count;
    }

    /// <summary>
    /// Gets the count of running tasks.
    /// </summary>
    /// <param name="scheduler">The task scheduler instance.</param>
    /// <returns>Number of running tasks.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="scheduler"/> is <see langword="null"/>.</exception>
    public static int GetRunningTaskCount(this BackgroundTaskScheduler scheduler)
    {
        ArgumentNullException.ThrowIfNull(scheduler);
        return scheduler.GetRunningTasks().Count;
    }

    /// <summary>
    /// Gets the count of completed tasks.
    /// </summary>
    /// <param name="scheduler">The task scheduler instance.</param>
    /// <returns>Number of completed tasks.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="scheduler"/> is <see langword="null"/>.</exception>
    public static int GetCompletedTaskCount(this BackgroundTaskScheduler scheduler)
    {
        ArgumentNullException.ThrowIfNull(scheduler);
        return scheduler.GetCompletedTasks().Count;
    }
}