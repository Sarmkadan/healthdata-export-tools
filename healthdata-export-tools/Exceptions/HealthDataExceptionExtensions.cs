namespace HealthDataExportTools.Exceptions;

/// <summary>
/// Extensions for <see cref="HealthDataException"/>.
/// </summary>
public static class HealthDataExceptionExtensions
{
    /// <summary>
    /// Gets a human-readable representation of the exception, including error code and context data.
    /// </summary>
    /// <param name="exception">The <see cref="HealthDataException"/> instance.</param>
    /// <returns>A human-readable string representation of the exception.</returns>
    public static string ToDetailedString(this HealthDataException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var message = exception.Message;
        if (!string.IsNullOrEmpty(exception.ErrorCode))
        {
            message += $" (Error Code: {exception.ErrorCode})";
        }

        if (exception.ContextData != null)
        {
            message += $" Context Data: {{{string.Join(", ", exception.ContextData.Select(kvp => $"{kvp.Key}: {kvp.Value}"))}}}";
        }

        return message;
    }

    /// <summary>
    /// Tries to get a specific context data value by key.
    /// </summary>
    /// <param name="exception">The <see cref="HealthDataException"/> instance.</param>
    /// <param name="key">The key of the context data value.</param>
    /// <param name="value">The value associated with the key, or default if not found.</param>
    /// <returns>true if the key was found; otherwise, false.</returns>
    public static bool TryGetContextData(this HealthDataException exception, string key, out object? value)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentException.ThrowIfNullOrEmpty(key);

        if (exception.ContextData != null && exception.ContextData.TryGetValue(key, out value))
        {
            return true;
        }

        value = default;
        return false;
    }
}
