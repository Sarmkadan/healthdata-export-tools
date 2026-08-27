#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using HealthDataExportTools.Correlation;
using HealthDataExportTools.DTOs;
using Xunit;

namespace HealthDataExportTools.Tests
{
    /// <summary>
    /// Contains unit tests for <see cref="CorrelationAnalysisResultDtoExtensions"/>.
    /// </summary>
    public class CorrelationAnalysisResultDtoTestsExtensions
    {
        private static CorrelationAnalysisResultDto CreateTestResult(
            IEnumerable<MetricCorrelationDto>? correlations = null)
        {
            return new CorrelationAnalysisResultDto
            {
                AnalysisId = "test123",
                GeneratedAt = DateTimeOffset.UtcNow,
                WindowDays = 30,
                Correlations = correlations?.ToList() ?? new List<MetricCorrelationDto>(),
                Insights = new List<CrossMetricInsightDto>(),
                TotalMetricPairsAnalyzed = 0,
                SignificantCorrelationsFound = 0
            };
        }

        private static MetricCorrelationDto CreateCorrelation(
            string metricA, string metricB, double coefficient,
            CorrelationStrength strength, CorrelationDirection direction,
            int sampleCount = 30)
        {
            return new MetricCorrelationDto
            {
                Pair = new CorrelationPair(metricA, metricB),
                Coefficient = coefficient,
                Strength = strength,
                Direction = direction,
                SampleCount = sampleCount,
                Interpretation = $"Test interpretation for {metricA} and {metricB}",
                AnalysisPeriodStart = new DateOnly(2026, 1, 1),
                AnalysisPeriodEnd = new DateOnly(2026, 1, 30)
            };
        }

        [Fact]
        public void GetStrongestCorrelation_ReturnsNull_WhenNoCorrelations()
        {
            // Arrange
            var result = CreateTestResult();

            // Act
            var strongest = result.GetStrongestCorrelation();

            // Assert
            strongest.Should().BeNull();
        }

        [Fact]
        public void GetStrongestCorrelation_ReturnsStrongest_WhenCorrelationsExist()
        {
            // Arrange
            var correlations = new List<MetricCorrelationDto>
            {
                CreateCorrelation("Steps", "Sleep", 0.5, CorrelationStrength.Strong, CorrelationDirection.Positive),
                CreateCorrelation("HeartRate", "Weight", 0.8, CorrelationStrength.VeryStrong, CorrelationDirection.Negative),
                CreateCorrelation("Calories", "Steps", 0.3, CorrelationStrength.Moderate, CorrelationDirection.Positive)
            };
            var result = CreateTestResult(correlations);

            // Act
            var strongest = result.GetStrongestCorrelation();

            // Assert
            strongest.Should().NotBeNull();
            strongest!.Pair.MetricA.Should().Be("HeartRate");
            strongest.Pair.MetricB.Should().Be("Weight");
            strongest.Coefficient.Should().Be(0.8);
        }

        [Fact]
        public void GetStrongestCorrelation_ThrowsArgumentNullException_WhenResultIsNull()
        {
            // Arrange
            CorrelationAnalysisResultDto result = null!;

            // Act
            Action act = () => result.GetStrongestCorrelation();

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void GetCorrelationsAboveThreshold_ReturnsEmptyList_WhenNoCorrelationsMeetThreshold()
        {
            // Arrange
            var correlations = new List<MetricCorrelationDto>
            {
                CreateCorrelation("Steps", "Sleep", 0.2, CorrelationStrength.Weak, CorrelationDirection.Positive),
                CreateCorrelation("HeartRate", "Weight", 0.1, CorrelationStrength.Negligible, CorrelationDirection.Negative)
            };
            var result = CreateTestResult(correlations);

            // Act
            var filtered = result.GetCorrelationsAboveThreshold(0.3);

            // Assert
            filtered.Should().BeEmpty();
        }

        [Fact]
        public void GetCorrelationsAboveThreshold_ReturnsFilteredList_WhenSomeMeetThreshold()
        {
            // Arrange
            var correlations = new List<MetricCorrelationDto>
            {
                CreateCorrelation("Steps", "Sleep", 0.5, CorrelationStrength.Strong, CorrelationDirection.Positive),
                CreateCorrelation("HeartRate", "Weight", 0.2, CorrelationStrength.Weak, CorrelationDirection.Negative),
                CreateCorrelation("Calories", "Steps", 0.7, CorrelationStrength.VeryStrong, CorrelationDirection.Positive)
            };
            var result = CreateTestResult(correlations);

            // Act
            var filtered = result.GetCorrelationsAboveThreshold(0.4);

            // Assert
            filtered.Should().HaveCount(2);
            filtered[0].Pair.MetricA.Should().Be("Calories");
            filtered[0].Pair.MetricB.Should().Be("Steps");
            filtered[0].Coefficient.Should().Be(0.7);
            filtered[1].Pair.MetricA.Should().Be("Steps");
            filtered[1].Pair.MetricB.Should().Be("Sleep");
            filtered[1].Coefficient.Should().Be(0.5);
        }

        [Fact]
        public void GetCorrelationsAboveThreshold_ReturnsAll_WhenThresholdIsZero()
        {
            // Arrange
            var correlations = new List<MetricCorrelationDto>
            {
                CreateCorrelation("Steps", "Sleep", 0.5, CorrelationStrength.Strong, CorrelationDirection.Positive),
                CreateCorrelation("HeartRate", "Weight", -0.3, CorrelationStrength.Moderate, CorrelationDirection.Negative)
            };
            var result = CreateTestResult(correlations);

            // Act
            var filtered = result.GetCorrelationsAboveThreshold(0.0);

            // Assert
            filtered.Should().HaveCount(2);
            // Should be ordered by absolute coefficient descending
            filtered[0].AbsoluteCoefficient.Should().BeGreaterOrEqualTo(filtered[1].AbsoluteCoefficient);
        }

        [Fact]
        public void GetCorrelationsAboveThreshold_ThrowsArgumentOutOfRangeException_WhenThresholdIsNegative()
        {
            // Arrange
            var result = CreateTestResult();

            // Act
            Action act = () => result.GetCorrelationsAboveThreshold(-0.1);

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void GetCorrelationsAboveThreshold_ThrowsArgumentOutOfRangeException_WhenThresholdIsGreaterThanOne()
        {
            // Arrange
            var result = CreateTestResult();

            // Act
            Action act = () => result.GetCorrelationsAboveThreshold(1.5);

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void GetCorrelationsAboveThreshold_ThrowsArgumentNullException_WhenResultIsNull()
        {
            // Arrange
            CorrelationAnalysisResultDto result = null!;

            // Act
            Action act = () => result.GetCorrelationsAboveThreshold(0.5);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void GetPositiveCorrelationsCount_ReturnsZero_WhenNoPositiveCorrelations()
        {
            // Arrange
            var correlations = new List<MetricCorrelationDto>
            {
                CreateCorrelation("Steps", "Sleep", -0.5, CorrelationStrength.Strong, CorrelationDirection.Negative),
                CreateCorrelation("HeartRate", "Weight", -0.2, CorrelationStrength.Weak, CorrelationDirection.Negative)
            };
            var result = CreateTestResult(correlations);

            // Act
            var count = result.GetPositiveCorrelationsCount();

            // Assert
            count.Should().Be(0);
        }

        [Fact]
        public void GetPositiveCorrelationsCount_ReturnsCorrectCount_WhenPositiveCorrelationsExist()
        {
            // Arrange
            var correlations = new List<MetricCorrelationDto>
            {
                CreateCorrelation("Steps", "Sleep", 0.5, CorrelationStrength.Strong, CorrelationDirection.Positive),
                CreateCorrelation("HeartRate", "Weight", -0.3, CorrelationStrength.Moderate, CorrelationDirection.Negative),
                CreateCorrelation("Calories", "Steps", 0.7, CorrelationStrength.VeryStrong, CorrelationDirection.Positive)
            };
            var result = CreateTestResult(correlations);

            // Act
            var count = result.GetPositiveCorrelationsCount();

            // Assert
            count.Should().Be(2);
        }

        [Fact]
        public void GetPositiveCorrelationsCount_ThrowsArgumentNullException_WhenResultIsNull()
        {
            // Arrange
            CorrelationAnalysisResultDto result = null!;

            // Act
            Action act = () => result.GetPositiveCorrelationsCount();

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void GetNegativeCorrelationsCount_ReturnsZero_WhenNoNegativeCorrelations()
        {
            // Arrange
            var correlations = new List<MetricCorrelationDto>
            {
                CreateCorrelation("Steps", "Sleep", 0.5, CorrelationStrength.Strong, CorrelationDirection.Positive),
                CreateCorrelation("HeartRate", "Weight", 0.2, CorrelationStrength.Weak, CorrelationDirection.Positive)
            };
            var result = CreateTestResult(correlations);

            // Act
            var count = result.GetNegativeCorrelationsCount();

            // Assert
            count.Should().Be(0);
        }

        [Fact]
        public void GetNegativeCorrelationsCount_ReturnsCorrectCount_WhenNegativeCorrelationsExist()
        {
            // Arrange
            var correlations = new List<MetricCorrelationDto>
            {
                CreateCorrelation("Steps", "Sleep", 0.5, CorrelationStrength.Strong, CorrelationDirection.Positive),
                CreateCorrelation("HeartRate", "Weight", -0.3, CorrelationStrength.Moderate, CorrelationDirection.Negative),
                CreateCorrelation("Calories", "Steps", -0.7, CorrelationStrength.VeryStrong, CorrelationDirection.Negative)
            };
            var result = CreateTestResult(correlations);

            // Act
            var count = result.GetNegativeCorrelationsCount();

            // Assert
            count.Should().Be(2);
        }

        [Fact]
        public void GetNegativeCorrelationsCount_ThrowsArgumentNullException_WhenResultIsNull()
        {
            // Arrange
            CorrelationAnalysisResultDto result = null!;

            // Act
            Action act = () => result.GetNegativeCorrelationsCount();

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void GetAverageCoefficient_ReturnsZero_WhenNoCorrelations()
        {
            // Arrange
            var result = CreateTestResult();

            // Act
            var average = result.GetAverageCoefficient();

            // Assert
            average.Should().Be(0.0);
        }

        [Fact]
        public void GetAverageCoefficient_ReturnsCorrectAverage_WhenCorrelationsExist()
        {
            // Arrange
            var correlations = new List<MetricCorrelationDto>
            {
                CreateCorrelation("Steps", "Sleep", 0.5, CorrelationStrength.Strong, CorrelationDirection.Positive),
                CreateCorrelation("HeartRate", "Weight", -0.3, CorrelationStrength.Moderate, CorrelationDirection.Negative),
                CreateCorrelation("Calories", "Steps", 0.7, CorrelationStrength.VeryStrong, CorrelationDirection.Positive)
            };
            var result = CreateTestResult(correlations);

            // Act
            var average = result.GetAverageCoefficient();

            // Assert
            average.Should().Be(0.3); // (0.5 + (-0.3) + 0.7) / 3 = 0.9 / 3 = 0.3
        }

        [Fact]
        public void GetAverageCoefficient_ThrowsArgumentNullException_WhenResultIsNull()
        {
            // Arrange
            CorrelationAnalysisResultDto result = null!;

            // Act
            Action act = () => result.GetAverageCoefficient();

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }
    }
}