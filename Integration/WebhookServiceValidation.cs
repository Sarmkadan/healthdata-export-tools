using System;
using System.Collections.Generic;
using System.Globalization;

namespace healthdata_export_tools.Integration
{
    /// <summary>
    /// Provides validation helpers for <see cref="WebhookService"/> instances.
    /// </summary>
    public static class WebhookServiceValidation
    {
        /// <summary>
        /// Validates a <see cref="WebhookService"/> instance and returns a list of human-readable problems.
        /// </summary>
        /// <param name="value">The webhook service to validate.</param>
        /// <returns>A read-only list of validation problems; empty if the service is valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this WebhookService value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            // Validate Id
            if (string.IsNullOrWhiteSpace(value.Id))
            {
                problems.Add($"WebhookService.Id must not be null or whitespace, but was '{(value.Id == null ? "null" : value.Id)}'.");
            }

            // Validate Url
            if (string.IsNullOrWhiteSpace(value.Url))
            {
                problems.Add("WebhookService.Url must not be null or whitespace.");
            }
            else if (!Uri.TryCreate(value.Url, UriKind.Absolute, out _) ||
                   !(value.Url.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                     value.Url.StartsWith("http://", StringComparison.OrdinalIgnoreCase)))
            {
                problems.Add($"WebhookService.Url must be a valid absolute URI with http:// or https:// scheme, but was '{value.Url}'.");
            }

            // Validate EventType
            if (string.IsNullOrWhiteSpace(value.EventType))
            {
                problems.Add("WebhookService.EventType must not be null or whitespace.");
            }

            // Validate CreatedAt
            if (value.CreatedAt == default)
            {
                problems.Add("WebhookService.CreatedAt must not be the default DateTime value.");
            }
            else if (value.CreatedAt > DateTime.UtcNow.AddMinutes(1))
            {
                problems.Add($"WebhookService.CreatedAt must not be in the future, but was {value.CreatedAt:O}.");
            }

            // Validate LastInvokedAt
            if (value.LastInvokedAt.HasValue && value.LastInvokedAt > DateTime.UtcNow.AddMinutes(1))
            {
                problems.Add($"WebhookService.LastInvokedAt must not be in the future, but was {value.LastInvokedAt.Value:O}.");
            }

            // Validate SuccessCount
            if (value.SuccessCount < 0)
            {
                problems.Add($"WebhookService.SuccessCount must be non-negative, but was {value.SuccessCount}.");
            }

            // Validate FailureCount
            if (value.FailureCount < 0)
            {
                problems.Add($"WebhookService.FailureCount must be non-negative, but was {value.FailureCount}.");
            }

            // Validate IsActive
            // No specific validation needed beyond being a valid boolean

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Determines whether a <see cref="WebhookService"/> instance is valid.
        /// </summary>
        /// <param name="value">The webhook service to check.</param>
        /// <returns><see langword="true"/> if the service is valid; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static bool IsValid(this WebhookService value)
        {
            return Validate(value).Count == 0;
        }

        /// <summary>
        /// Ensures that a <see cref="WebhookService"/> instance is valid, throwing an <see cref="ArgumentException"/>
        /// with a detailed message listing all validation problems if it is not.
        /// </summary>
        /// <param name="value">The webhook service to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> has validation problems.</exception>
        public static void EnsureValid(this WebhookService value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = Validate(value);
            if (problems.Count > 0)
            {
                throw new ArgumentException(
                    $"WebhookService validation failed with {problems.Count} problem(s):{Environment.NewLine}-
                    {string.Join($"{Environment.NewLine}- ", problems)}",
                    nameof(value));
            }
        }
    }
}