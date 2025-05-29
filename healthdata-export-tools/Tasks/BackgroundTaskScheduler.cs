// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace HealthDataExportTools.Tasks;

/// <summary>
/// Background task scheduler for deferred and recurring operations
/// </summary>
public class BackgroundTaskScheduler
{
    private readonly Dictionary<string, ScheduledTask> _scheduledTasks;
    private readonly ReaderWriterLockSlim _tasksLock;
    private readonly ILogger<BackgroundTaskScheduler> _logger;
    private readonly List<Task> _runningTasks;

    public BackgroundTaskScheduler(ILogger<BackgroundTaskScheduler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _scheduledTasks = new Dictionary<string, ScheduledTask>();
        _runningTasks = new List<Task>();
        _tasksLock = new ReaderWriterLockSlim();
    }

    /// <summary>
    /// Schedule a task to run once at a specific time
    /// </summary>
    public string ScheduleOnce(string taskName, Func<Task> action, DateTime executeAt)
    {
        if (string.IsNullOrEmpty(taskName))
            throw new ArgumentException("Task name cannot be empty", nameof(taskName));

        var taskId = Guid.NewGuid().ToString();

        _tasksLock.EnterWriteLock();
        try
        {
            var scheduledTask = new ScheduledTask
            {
                Id = taskId,
                Name = taskName,
                ExecuteAt = executeAt,
                IsRecurring = false,
                CreatedAt = DateTime.UtcNow
            };

            _scheduledTasks[taskId] = scheduledTask;

            // Calculate delay and schedule execution
            var delay = executeAt - DateTime.UtcNow;
            if (delay.TotalMilliseconds > 0)
            {
                _ = Task.Delay(delay).ContinueWith(async _ =>
                {
                    await ExecuteTaskAsync(taskId, action);
                });
            }
            else
            {
                _ = ExecuteTaskAsync(taskId, action);
            }

            _logger.LogInformation("Task scheduled once: {TaskName} at {ExecuteAt}", taskName, executeAt);
            return taskId;
        }
        finally
        {
            _tasksLock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Schedule a recurring task
    /// </summary>
    public string ScheduleRecurring(
        string taskName,
        Func<Task> action,
        TimeSpan interval,
        TimeSpan? initialDelay = null)
    {
        if (string.IsNullOrEmpty(taskName))
            throw new ArgumentException("Task name cannot be empty", nameof(taskName));

        if (interval.TotalMilliseconds <= 0)
            throw new ArgumentException("Interval must be positive", nameof(interval));

        var taskId = Guid.NewGuid().ToString();
        initialDelay ??= interval;

        _tasksLock.EnterWriteLock();
        try
        {
            var scheduledTask = new ScheduledTask
            {
                Id = taskId,
                Name = taskName,
                Interval = interval,
                IsRecurring = true,
                CreatedAt = DateTime.UtcNow
            };

            _scheduledTasks[taskId] = scheduledTask;

            // Start recurring execution
            _ = Task.Delay(initialDelay.Value).ContinueWith(async _ =>
            {
                while (true)
                {
                    _tasksLock.EnterReadLock();
                    var taskStillExists = _scheduledTasks.ContainsKey(taskId);
                    _tasksLock.ExitReadLock();

                    if (!taskStillExists)
                        break;

                    await ExecuteTaskAsync(taskId, action);
                    await Task.Delay(interval);
                }
            });

            _logger.LogInformation("Recurring task scheduled: {TaskName} every {Interval}",
                taskName, interval);

            return taskId;
        }
        finally
        {
            _tasksLock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Cancel a scheduled task
    /// </summary>
    public bool CancelTask(string taskId)
    {
        _tasksLock.EnterWriteLock();
        try
        {
            if (_scheduledTasks.Remove(taskId))
            {
                _logger.LogInformation("Task cancelled: {TaskId}", taskId);
                return true;
            }
            return false;
        }
        finally
        {
            _tasksLock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Get all scheduled tasks
    /// </summary>
    public List<ScheduledTask> GetScheduledTasks()
    {
        _tasksLock.EnterReadLock();
        try
        {
            return _scheduledTasks.Values.ToList();
        }
        finally
        {
            _tasksLock.ExitReadLock();
        }
    }

    /// <summary>
    /// Get a specific scheduled task
    /// </summary>
    public ScheduledTask? GetTask(string taskId)
    {
        _tasksLock.EnterReadLock();
        try
        {
            return _scheduledTasks.TryGetValue(taskId, out var task) ? task : null;
        }
        finally
        {
            _tasksLock.ExitReadLock();
        }
    }

    /// <summary>
    /// Execute a task
    /// </summary>
    private async Task ExecuteTaskAsync(string taskId, Func<Task> action)
    {
        try
        {
            _tasksLock.EnterUpgradeableReadLock();
            try
            {
                if (!_scheduledTasks.TryGetValue(taskId, out var task))
                    return;

                task.LastExecutedAt = DateTime.UtcNow;
                task.ExecutionCount++;
            }
            finally
            {
                _tasksLock.ExitUpgradeableReadLock();
            }

            _logger.LogDebug("Executing task: {TaskId}", taskId);
            await action();

            _tasksLock.EnterUpgradeableReadLock();
            try
            {
                if (_scheduledTasks.TryGetValue(taskId, out var task))
                {
                    task.LastCompletedAt = DateTime.UtcNow;
                }
            }
            finally
            {
                _tasksLock.ExitUpgradeableReadLock();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing task: {TaskId}", taskId);
        }
    }
}

/// <summary>
/// Information about a scheduled task
/// </summary>
public class ScheduledTask
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime? ExecuteAt { get; set; }
    public TimeSpan? Interval { get; set; }
    public bool IsRecurring { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastExecutedAt { get; set; }
    public DateTime? LastCompletedAt { get; set; }
    public int ExecutionCount { get; set; }

    public bool IsPending => !LastExecutedAt.HasValue;
    public bool IsRunning => LastExecutedAt.HasValue && !LastCompletedAt.HasValue;
    public bool IsCompleted => LastCompletedAt.HasValue;
}
