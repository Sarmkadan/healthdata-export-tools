#nullable enable
using System;

namespace HealthDataExportTools.Configuration;

/// <summary>
/// Configuration options for the <see cref="Integration.WebhookService"/>.
/// </summary>
public sealed class WebhookOptions
{
    /// <summary>
    /// Base URL used when a webhook URL is not explicitly supplied.
    /// </summary>
    public string? BaseUrl { get; set; }

    /// <summary>
    /// Timeout applied to the internal <see cref="HttpClient"/> used for webhook calls.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Maximum number of retry attempts when a webhook invocation fails.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;
}
