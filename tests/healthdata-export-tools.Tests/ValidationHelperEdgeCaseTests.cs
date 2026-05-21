#nullable enable
using FluentAssertions;
using HealthDataExportTools.Utilities;
using Xunit;

namespace HealthDataExportTools.Tests;

public sealed class ValidationHelperEdgeCaseTests
{
    [Theory]
    [InlineData(29, false)]  // Below MinHeartRate (30)
    [InlineData(30, true)]   // Exactly MinHeartRate
    [InlineData(120, true)]  // Normal range
    [InlineData(220, true)]  // Exactly MaxHeartRate
    [InlineData(221, false)] // Above MaxHeartRate
    [InlineData(0, false)]
    [InlineData(-1, false)]
    public void IsValidHeartRate_BoundaryValues(int bpm, bool expected) =>
        ValidationHelper.IsValidHeartRate(bpm).Should().Be(expected);

    [Theory]
    [InlineData(39, false)]
    [InlineData(40, true)]
    [InlineData(100, true)]
    [InlineData(101, false)]
    public void IsValidRestingHeartRate_BoundaryValues(int bpm, bool expected) =>
        ValidationHelper.IsValidRestingHeartRate(bpm).Should().Be(expected);

    [Theory]
    [InlineData(-1, false)]
    [InlineData(0, true)]
    [InlineData(100, true)]
    [InlineData(101, false)]
    public void IsValidSpO2_BoundaryValues(int pct, bool expected) =>
        ValidationHelper.IsValidSpO2(pct).Should().Be(expected);

    [Theory]
    [InlineData(179, false)]
    [InlineData(180, true)]
    [InlineData(720, true)]
    [InlineData(721, false)]
    public void IsValidSleepDuration_BoundaryValues(int min, bool expected) =>
        ValidationHelper.IsValidSleepDuration(min).Should().Be(expected);

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(1440, true)]
    [InlineData(1441, false)]
    public void IsValidActivityDuration_BoundaryValues(int min, bool expected) =>
        ValidationHelper.IsValidActivityDuration(min).Should().Be(expected);

    [Fact]
    public void IsValidRecordDate_FutureDate_ReturnsFalse() =>
        ValidationHelper.IsValidRecordDate(DateTime.UtcNow.AddDays(1)).Should().BeFalse();

    [Fact]
    public void IsValidRecordDate_Today_ReturnsTrue() =>
        ValidationHelper.IsValidRecordDate(DateTime.UtcNow).Should().BeTrue();

    [Fact]
    public void IsValidRecordDate_PastDate_ReturnsTrue() =>
        ValidationHelper.IsValidRecordDate(DateTime.UtcNow.AddYears(-1)).Should().BeTrue();

    [Theory]
    [InlineData("test@example.com", true)]
    [InlineData("", false)]
    [InlineData("not-an-email", false)]
    [InlineData("a@b.c", true)]
    public void IsValidEmail_VariousInputs(string email, bool expected) =>
        ValidationHelper.IsValidEmail(email).Should().Be(expected);

    [Theory]
    [InlineData(-90, -180, true)]
    [InlineData(90, 180, true)]
    [InlineData(-91, 0, false)]
    [InlineData(91, 0, false)]
    [InlineData(0, -181, false)]
    [InlineData(0, 181, false)]
    [InlineData(0, 0, true)]
    public void IsValidGpsCoordinates_BoundaryValues(double lat, double lon, bool expected) =>
        ValidationHelper.IsValidGpsCoordinates(lat, lon).Should().Be(expected);

    [Fact]
    public void IsValidDistance_NaN_ReturnsFalse() =>
        ValidationHelper.IsValidDistance(double.NaN).Should().BeFalse();

    [Fact]
    public void IsValidDistance_PositiveInfinity_ReturnsFalse() =>
        ValidationHelper.IsValidDistance(double.PositiveInfinity).Should().BeFalse();

    [Fact]
    public void IsValidDistance_Negative_ReturnsFalse() =>
        ValidationHelper.IsValidDistance(-0.1).Should().BeFalse();

    [Fact]
    public void IsValidDistance_Zero_ReturnsTrue() =>
        ValidationHelper.IsValidDistance(0).Should().BeTrue();

    [Fact]
    public void EnsureNotNull_NullValue_ThrowsValidationException()
    {
        var act = () => ValidationHelper.EnsureNotNull<string>(null, "TestField");
        act.Should().Throw<Exception>().WithMessage("*cannot be null*");
    }

    [Fact]
    public void EnsureNotEmpty_EmptyValue_ThrowsValidationException()
    {
        var act = () => ValidationHelper.EnsureNotEmpty("", "TestField");
        act.Should().Throw<Exception>().WithMessage("*cannot be empty*");
    }

    [Fact]
    public void EnsureNotEmpty_WhitespaceValue_ThrowsValidationException()
    {
        var act = () => ValidationHelper.EnsureNotEmpty("   ", "TestField");
        act.Should().Throw<Exception>().WithMessage("*cannot be empty*");
    }

    [Fact]
    public void EnsureInRange_OutOfRange_ThrowsValidationException()
    {
        var act = () => ValidationHelper.EnsureInRange(300, 0, 200, "HeartRate");
        act.Should().Throw<Exception>().WithMessage("*must be between*");
    }

    [Fact]
    public void EnsureInRange_InRange_DoesNotThrow()
    {
        var act = () => ValidationHelper.EnsureInRange(100, 0, 200, "HeartRate");
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateHeartRateData_MinGreaterThanMax_ReturnsError()
    {
        var errors = ValidationHelper.ValidateHeartRateData(100, 60, 80);
        errors.Should().Contain(e => e.Contains("greater than maximum"));
    }

    [Fact]
    public void ValidateHeartRateData_AvgOutsideMinMax_ReturnsError()
    {
        var errors = ValidationHelper.ValidateHeartRateData(60, 100, 50);
        errors.Should().Contain(e => e.Contains("between minimum and maximum"));
    }

    [Fact]
    public void ValidateHeartRateData_AllValid_ReturnsEmpty()
    {
        var errors = ValidationHelper.ValidateHeartRateData(60, 100, 80);
        errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData(-51, false)]
    [InlineData(-50, true)]
    [InlineData(60, true)]
    [InlineData(61, false)]
    public void IsValidTemperature_BoundaryValues(double celsius, bool expected) =>
        ValidationHelper.IsValidTemperature(celsius).Should().Be(expected);

    [Theory]
    [InlineData(-1, false)]
    [InlineData(0, true)]
    [InlineData(9000, true)]
    [InlineData(9001, false)]
    public void IsValidElevation_BoundaryValues(int meters, bool expected) =>
        ValidationHelper.IsValidElevation(meters).Should().Be(expected);
}
