#nullable enable

namespace HealthDataExportTools.Tasks;

/// <summary>
/// Extension methods for BackgroundTaskScheduler providing additional functionality
/// </summary>
public static class BackgroundTaskSchedulerExtensions
{
    /// <summary>
    /// Schedule a task to run once after a delay
    /// </summary>
    /// <param name="scheduler">The task scheduler instance</param>
    /// <param name="taskName">Name of the task</param>
    /// <param name="action">Action to execute</param>
    /// <param name="delay">Delay before execution</param>
    /// <returns>Task ID</returns>
    public static string ScheduleOnce(this BackgroundTaskScheduler scheduler, string taskName, Func<Task> action, TimeSpan delay)
    {
        if (scheduler == null)
            throw new ArgumentNullException(nameof(scheduler));

        if (string.IsNullOrEmpty(taskName))
            throw new ArgumentException("Task name cannot be empty", nameof(taskName));

        if (action == null)
            throw new ArgumentNullException(nameof(action));

        if (delay.TotalMilliseconds < 0)
            throw new ArgumentException("Delay must be non-negative", nameof(delay));

        var executeAt = DateTime.UtcNow.Add(delay);
        return scheduler.ScheduleOnce(taskName, action, executeAt);
    }

    /// <summary>
    /// Schedule a recurring task with a delay before the first execution
    /// </summary>
    /// <param name="scheduler">The task scheduler instance</param>
    /// <param name="taskName">Name of the task</param>
    /// <param name="action">Action to execute</param>
    /// <param name="interval">Interval between executions</param>
    /// <param name="initialDelay">Optional initial delay before first execution</param>
    /// <returns>Task ID</returns>
    public static string ScheduleRecurring(this BackgroundTaskScheduler scheduler, string taskName, Func<Task> action, TimeSpan interval, TimeSpan initialDelay)
    {
        if (scheduler == null)
            throw new ArgumentNullException(nameof(scheduler));

        if (string.IsNullOrEmpty(taskName))
            throw new ArgumentException("Task name cannot be empty", nameof(taskName));

        if (action == null)
            throw new ArgumentNullException(nameof(action));

        if (interval.TotalMilliseconds <= 0)
            throw new ArgumentException("Interval must be positive", nameof(interval));

        if (initialDelay.TotalMilliseconds <= 0)
            throw new ArgumentException("Initial delay must be positive", nameof(initialDelay));

        return scheduler.ScheduleRecurring(taskName, action, interval, initialDelay);
    }

    /// <summary>
    /// Cancel all tasks matching a specific name pattern
    /// </summary>
    /// <param name="scheduler">The task scheduler instance</param>
    /// <param name="namePattern">Pattern to match task names (supports * wildcard)</param>
    /// <returns>Number of tasks cancelled</returns>
    public static int CancelTasksByName(this BackgroundTaskScheduler scheduler, string namePattern)
    {
        if (scheduler == null)
            throw new ArgumentNullException(nameof(scheduler));

        if (string.IsNullOrEmpty(namePattern))
            throw new ArgumentException("Name pattern cannot be empty", nameof(namePattern));

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
    /// Get all pending tasks (tasks that haven't been executed yet)
    /// </summary>
    /// <param name="scheduler">The task scheduler instance</param>
    /// <returns>List of pending tasks</returns>
    public static List<ScheduledTask> GetPendingTasks(this BackgroundTaskScheduler scheduler)
    {
        if (scheduler == null)
            throw new ArgumentNullException(nameof(scheduler));

        return scheduler.GetScheduledTasks()
            .Where(t => t.IsPending)
            .ToList();
    }

    /// <summary>
    /// Get all running tasks (tasks currently being executed)
    /// </summary>
    /// <param name="scheduler">The task scheduler instance</param>
    /// <returns>List of running tasks</returns>
    public static List<ScheduledTask> GetRunningTasks(this BackgroundTaskScheduler scheduler)
    {
        if (scheduler == null)
            throw new ArgumentNullException(nameof(scheduler));

        return scheduler.GetScheduledTasks()
            .Where(t => t.IsRunning)
            .ToList();
    }

    /// <summary>
    /// Get all completed tasks
    /// </summary>
    /// <param name="scheduler">The task scheduler instance</param>
    /// <returns>List of completed tasks</returns>
    public static List<ScheduledTask> GetCompletedTasks(this BackgroundTaskScheduler scheduler)
    {
        if (scheduler == null)
            throw new ArgumentNullException(nameof(scheduler));

        return scheduler.GetScheduledTasks()
            .Where(t => t.IsCompleted)
            .ToList();
    }

    /// <summary>
    /// Find a task by name
    /// </summary>
    /// <param name="scheduler">The task scheduler instance</param>
    /// <param name="taskName">Name of the task to find</param>
    /// <returns>Found task or null</returns>
    public static ScheduledTask? GetTaskByName(this BackgroundTaskScheduler scheduler, string taskName)
    {
        if (scheduler == null)
            throw new ArgumentNullException(nameof(scheduler));

        if (string.IsNullOrEmpty(taskName))
            throw new ArgumentException("Task name cannot be empty", nameof(taskName));

        return scheduler.GetScheduledTasks()
            .FirstOrDefault(t => string.Equals(t.Name, taskName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Check if a task with the given name exists
    /// </summary>
    /// <param name="scheduler">The task scheduler instance</param>
    /// <param name="taskName">Name of the task to check</param>
    /// <returns>True if task exists, false otherwise</returns>
    public static bool TaskExists(this BackgroundTaskScheduler scheduler, string taskName)
    {
        if (scheduler == null)
            throw new ArgumentNullException(nameof(scheduler));

        return scheduler.GetTaskByName(taskName) != null;
    }

    /// <summary>
    /// Get the count of scheduled tasks
    /// </summary>
    /// <param name="scheduler">The task scheduler instance</param>
    /// <returns>Number of scheduled tasks</returns>
    public static int GetTaskCount(this BackgroundTaskScheduler scheduler)
    {
        if (scheduler == null)
            throw new ArgumentNullException(nameof(scheduler));

        return scheduler.GetScheduledTasks().Count;
    }

    /// <summary>
    /// Get the count of pending tasks
    /// </summary>
    /// <param name="scheduler">The task scheduler instance</param>
    /// <returns>Number of pending tasks</returns>
    public static int GetPendingTaskCount(this BackgroundTaskScheduler scheduler)
    {
        if (scheduler == null)
            throw new ArgumentNullException(nameof(scheduler));

        return scheduler.GetPendingTasks().Count;
    }

    /// <summary>
    /// Get the count of running tasks
    /// </summary>
    /// <param name="scheduler">The task scheduler instance</param>
    /// <returns>Number of running tasks</returns>
    public static int GetRunningTaskCount(this BackgroundTaskScheduler scheduler)
    {
        if (scheduler == null)
            throw new ArgumentNullException(nameof(scheduler));

        return scheduler.GetRunningTasks().Count;
    }

    /// <summary>
    /// Get the count of completed tasks
    /// </summary>
    /// <param name="scheduler">The task scheduler instance</param>
    /// <returns>Number of completed tasks</returns>
    public static int GetCompletedTaskCount(this BackgroundTaskScheduler scheduler)
    {
        if (scheduler == null)
            throw new ArgumentNullException(nameof(scheduler));

        return scheduler.GetCompletedTasks().Count;
    }
}