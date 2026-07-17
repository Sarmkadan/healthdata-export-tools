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
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <see langword="null"/>.</exception>
    public static string ToDetailedString(this HealthDataException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var message = exception.Message;
        if (!string.IsNullOrEmpty(exception.ErrorCode))
        {
            message += $" (Error Code: {exception.ErrorCode})";
        }

        if (exception.ContextData?.Count > 0)
        {
            var contextDataParts = new List<string>();
            foreach (var kvp in exception.ContextData)
            {
                contextDataParts.Add($"{kvp.Key}: {kvp.Value}");
            }
            message += $" Context Data: {{{string.Join(", ", contextDataParts)}}}";
        }

        return message;
    }

    /// <summary>
    /// Tries to get a specific context data value by key.
    /// </summary>
    /// <param name="exception">The <see cref="HealthDataException"/> instance.</param>
    /// <param name="key">The key of the context data value.</param>
    /// <param name="value">The value associated with the key, or <see langword="null"/> if not found.</param>
    /// <returns><see langword="true"/> if the key was found; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="key"/> is <see langword="null"/> or empty.</exception>
    public static bool TryGetContextData(this HealthDataException exception, string key, out object? value)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentException.ThrowIfNullOrEmpty(key);

        var contextData = exception.ContextData;
        if (contextData != null && contextData.TryGetValue(key, out value))
        {
            return true;
        }

        value = null;
        return false;
    }
}