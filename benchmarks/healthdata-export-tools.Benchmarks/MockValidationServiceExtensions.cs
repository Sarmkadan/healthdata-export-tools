using System;
using System.Collections.Generic;
using HealthDataExportTools.Domain.Models;
using HealthDataExportTools.Services;

namespace HealthDataExportTools.Benchmarks
{
	/// <summary>
	/// Extension methods for <see cref="MockValidationService"/> that provide validation operations for benchmark scenarios.
	/// </summary>
	public static class MockValidationServiceExtensions
	{
		/// <summary>
		/// Determines whether all validation results are valid.
		/// </summary>
		/// <param name="service">The validation service instance.</param>
		/// <param name="results">The collection of validation results to check.</param>
		/// <returns><see langword="true"/> if all results are valid; otherwise, <see langword="false"/>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="results"/> is <see langword="null"/>.</exception>
		public static bool IsValid(this MockValidationService service, IEnumerable<ValidationResult> results)
		{
			ArgumentNullException.ThrowIfNull(service);
			ArgumentNullException.ThrowIfNull(results);

			foreach (var result in results)
			{
				if (!result.IsValid)
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Validates all provided health data records and returns a dictionary of validation results.
		/// </summary>
		/// <param name="service">The validation service instance.</param>
		/// <param name="sleepData">The sleep data to validate.</param>
		/// <param name="heartRateData">The heart rate data to validate.</param>
		/// <param name="spO2Data">The SpO2 data to validate.</param>
		/// <param name="stepsData">The steps data to validate.</param>
		/// <param name="activityData">The activity data to validate.</param>
		/// <returns>A dictionary mapping data type names to their validation results.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is <see langword="null"/>.</exception>
		public static Dictionary<string, ValidationResult> ValidateAll(
			this MockValidationService service,
			SleepData sleepData,
			HeartRateData heartRateData,
			SpO2Data spO2Data,
			StepsData stepsData,
			ActivityData activityData)
		{
			ArgumentNullException.ThrowIfNull(service);
			ArgumentNullException.ThrowIfNull(sleepData);
			ArgumentNullException.ThrowIfNull(heartRateData);
			ArgumentNullException.ThrowIfNull(spO2Data);
			ArgumentNullException.ThrowIfNull(stepsData);
			ArgumentNullException.ThrowIfNull(activityData);

			return new Dictionary<string, ValidationResult>(StringComparer.Ordinal)
			{
				["Sleep"] = service.ValidateSleepData(sleepData),
				["HeartRate"] = service.ValidateHeartRateData(heartRateData),
				["SpO2"] = service.ValidateSpO2Data(spO2Data),
				["Steps"] = service.ValidateStepsData(stepsData),
				["Activity"] = service.ValidateActivityData(activityData),
			};
		}

		/// <summary>
		/// Validates all provided health data records and returns a dictionary of validation results.
		/// </summary>
		/// <param name="service">The validation service instance.</param>
		/// <param name="sleepData">The collection of sleep data to validate.</param>
		/// <param name="heartRateData">The collection of heart rate data to validate.</param>
		/// <param name="spO2Data">The collection of SpO2 data to validate.</param>
		/// <param name="stepsData">The collection of steps data to validate.</param>
		/// <param name="activityData">The collection of activity data to validate.</param>
		/// <returns>A dictionary mapping data type names to their validation results.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is <see langword="null"/>.</exception>
		public static Dictionary<string, ValidationResult> ValidateAll(
			this MockValidationService service,
			IEnumerable<SleepData> sleepData,
			IEnumerable<HeartRateData> heartRateData,
			IEnumerable<SpO2Data> spO2Data,
			IEnumerable<StepsData> stepsData,
			IEnumerable<ActivityData> activityData)
		{
			ArgumentNullException.ThrowIfNull(service);
			ArgumentNullException.ThrowIfNull(sleepData);
			ArgumentNullException.ThrowIfNull(heartRateData);
			ArgumentNullException.ThrowIfNull(spO2Data);
			ArgumentNullException.ThrowIfNull(stepsData);
			ArgumentNullException.ThrowIfNull(activityData);

			var results = new Dictionary<string, ValidationResult>(StringComparer.Ordinal);

			foreach (var data in sleepData)
			{
				results.Add($"Sleep_{data.RecordDate:yyyyMMdd}", service.ValidateSleepData(data));
			}

			foreach (var data in heartRateData)
			{
				results.Add($"HeartRate_{data.RecordDate:yyyyMMdd}", service.ValidateHeartRateData(data));
			}

			foreach (var data in spO2Data)
			{
				results.Add($"SpO2_{data.RecordDate:yyyyMMdd}", service.ValidateSpO2Data(data));
			}

			foreach (var data in stepsData)
			{
				results.Add($"Steps_{data.RecordDate:yyyyMMdd}", service.ValidateStepsData(data));
			}

			foreach (var data in activityData)
			{
				results.Add($"Activity_{data.RecordDate:yyyyMMdd}_{data.ActivityType}", service.ValidateActivityData(data));
			}

			return results;
		}
	}
}
