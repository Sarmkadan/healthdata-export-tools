using System;
using System.Collections.Generic;
using System.Linq;

namespace HealthDataExportTools.DTOs
{
    /// <summary>
    /// Provides extension methods for <see cref="ExportResultDto"/>.
    /// </summary>
    public static class ExportResultDtoExtensions
    {
        /// <summary>
        /// Calculates the total duration of the export operation.
        /// </summary>
        /// <param name="result">The export result.</param>
        /// <returns>A <see cref="TimeSpan"/> representing the duration.</returns>
        /// <exception cref="ArgumentNullException">Thrown if result is null.</exception>
        public static TimeSpan GetDuration(this ExportResultDto result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            return result.EndTime - result.StartTime;
        }

        /// <summary>
        /// Determines if the export operation completed successfully.
        /// Success is defined as having a status of "Completed" and no errors.
        /// </summary>
        /// <param name="result">The export result.</param>
        /// <returns>True if successful; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if result is null.</exception>
        public static bool IsSuccessful(this ExportResultDto result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));

            bool isCompleted = string.Equals(result.Status, "Completed", StringComparison.OrdinalIgnoreCase);
            bool hasNoErrors = result.Errors == null || result.Errors.Count == 0;

            return isCompleted && hasNoErrors;
        }

        /// <summary>
        /// Determines if the export operation generated any warnings or errors.
        /// </summary>
        /// <param name="result">The export result.</param>
        /// <returns>True if there are warnings or errors; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if result is null.</exception>
        public static bool HasIssues(this ExportResultDto result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));

            bool hasWarnings = result.Warnings != null && result.Warnings.Any();
            bool hasErrors = result.Errors != null && result.Errors.Any();

            return hasWarnings || hasErrors;
        }

        /// <summary>
        /// Gets the total number of records processed, including both exported and filtered records.
        /// </summary>
        /// <param name="result">The export result.</param>
        /// <returns>The total count of processed records.</returns>
        /// <exception cref="ArgumentNullException">Thrown if result is null.</exception>
        public static int GetTotalProcessedRecords(this ExportResultDto result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            return result.RecordsExported + result.RecordsFiltered;
        }
    }
}
