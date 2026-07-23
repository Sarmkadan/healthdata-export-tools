#nullable enable
using HealthDataExportTools.Cli;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace HealthDataExportTools.Tests
{
    public sealed class CliArgumentParserTests
    {
        private readonly CliArgumentParser _parser;
        private readonly Mock<ILogger<CliArgumentParser>> _loggerMock;

        public CliArgumentParserTests()
        {
            _loggerMock = new Mock<ILogger<CliArgumentParser>>();
            _parser = new CliArgumentParser(_loggerMock.Object);
        }

        [Fact]
        public void Parse_ValidFullCommand_ReturnsSuccessResult()
        {
            // Arrange
            var args = new[] { "--input", "input.csv", "--output", "output.json" };

            // Act
            var result = _parser.Parse(args);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Options);
            Assert.Equal("input.csv", result.Options.InputPath);
            Assert.Equal("output.json", result.Options.OutputPath);
        }

        [Fact]
        public void Parse_UnknownFlag_ReturnsFailureResultWithSuggestion()
        {
            // Arrange
            var args = new[] { "--unknwon", "input.csv" };

            // Act
            var result = _parser.Parse(args);

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Options);
            Assert.Single(result.Errors);
            Assert.Contains("Did you mean", result.Errors[0]);
            Assert.Contains("--unknown", result.Errors[0]);
        }

        [Fact]
        public void Parse_UnknownFlagWithoutGoodMatch_ReturnsFailureResult()
        {
            // Arrange
            var args = new[] { "--xyzzy", "input.csv" };

            // Act
            var result = _parser.Parse(args);

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Options);
            Assert.Single(result.Errors);
            Assert.Contains("Unknown option '--xyzzy'", result.Errors[0]);
        }

        [Fact]
        public void Parse_MissingRequiredValue_ReturnsFailureResult()
        {
            // Arrange
            var args = new[] { "--input" };

            // Act
            var result = _parser.Parse(args);

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Options);
            Assert.Single(result.Errors);
            Assert.Contains("requires a value", result.Errors[0]);
        }

        [Fact]
        public void Parse_HelpFlag_ReturnsHelp()
        {
            // Arrange
            var args = new[] { "--help" };

            // Act
            var result = _parser.Parse(args);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Options);
            Assert.True(result.Options.Help);
        }

        [Fact]
        public void Parse_InvalidDateFormat_ReturnsFailureResult()
        {
            // Arrange
            var args = new[] { "--start-date", "invalid-date", "--end-date", "2025-13-45" };

            // Act
            var result = _parser.Parse(args);

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Options);
            Assert.Equal(2, result.Errors.Count);
            Assert.Contains("Invalid start date format", result.Errors[0]);
            Assert.Contains("Invalid end date format", result.Errors[1]);
        }

        [Fact]
        public void Parse_DateFormatWithCorrectFormat_ReturnsSuccess()
        {
            // Arrange
            var args = new[] { "--start-date", "2025-01-15", "--end-date", "2025-01-20" };

            // Act
            var result = _parser.Parse(args);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Options);
            Assert.Equal("2025-01-15", result.Options.StartDate);
            Assert.Equal("2025-01-20", result.Options.EndDate);
        }

        [Fact]
        public void Parse_StartDateAfterEndDate_ReturnsFailureResult()
        {
            // Arrange
            var args = new[] { "--start-date", "2025-01-20", "--end-date", "2025-01-15" };

            // Act
            var result = _parser.Parse(args);

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Options);
            Assert.Single(result.Errors);
            Assert.Contains("Start date cannot be after end date", result.Errors[0]);
        }

        [Fact]
        public void Parse_InvalidFormat_ReturnsFailureResult()
        {
            // Arrange
            var args = new[] { "--format", "xml" };

            // Act
            var result = _parser.Parse(args);

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Options);
            Assert.Single(result.Errors);
            Assert.Contains("Invalid format: xml. Valid options: json, csv, sqlite, xml, all", result.Errors[0]);
        }

        [Fact]
        public void Parse_ValidFormat_ReturnsSuccess()
        {
            // Arrange
            var args = new[] { "--format", "csv" };

            // Act
            var result = _parser.Parse(args);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Options);
            Assert.Equal("csv", result.Options.Format);
        }

        [Fact]
        public void Parse_InvalidMaxParallelism_ReturnsFailureResult()
        {
            // Arrange
            var args = new[] { "--max-parallelism", "0" };

            // Act
            var result = _parser.Parse(args);

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Options);
            Assert.Single(result.Errors);
            Assert.Contains("Max parallelism must be between 1", result.Errors[0]);
        }

        [Fact]
        public void Parse_ValidMaxParallelism_ReturnsSuccess()
        {
            // Arrange
            var processorCount = Environment.ProcessorCount;
            var args = new[] { "--max-parallelism", processorCount.ToString() };

            // Act
            var result = _parser.Parse(args);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Options);
            Assert.Equal(processorCount, result.Options.MaxParallelism);
        }

        [Fact]
        public void Parse_NegativeCacheDuration_ReturnsFailureResult()
        {
            // Arrange
            var args = new[] { "--cache-duration", "-5" };

            // Act
            var result = _parser.Parse(args);

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Options);
            Assert.Single(result.Errors);
            Assert.Contains("Cache duration cannot be negative", result.Errors[0]);
        }

        [Fact]
        public void ParseExtensions_TryParse_ReturnsCorrectResult()
        {
            // Arrange
            var args = new[] { "--input", "test.csv", "--format", "jsonl" };

            // Act
            bool success = _parser.TryParse(args, out var options);

            // Assert
            Assert.True(success);
            Assert.NotNull(options);
            Assert.Equal("test.csv", options.InputPath);
            Assert.Equal("jsonl", options.Format);
        }

        [Fact]
        public void ParseExtensions_TryParse_InvalidArgs_ReturnsFailure()
        {
            // Arrange
            var args = new[] { "--unknwon", "test" };

            // Act
            bool success = _parser.TryParse(args, out var options);

            // Assert
            Assert.False(success);
            Assert.Null(options);
        }

        [Fact]
        public void ParseExtensions_ParseWithValidation_ReturnsParseResult()
        {
            // Arrange
            var args = new[] { "--input", "test.csv", "--format", "csv" };

            // Act
            var result = _parser.ParseWithValidation(args);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Options);
        }

        [Fact]
        public void Parse_LevenshteinDistance_SuggestsCorrectFlag()
        {
            // Test various typos and expected suggestions
            var testCases = new[]
            {
                new { Input = new[] { "--outpu", "test" }, ExpectedSuggestion = "output" },
                new { Input = new[] { "--formt", "csv" }, ExpectedSuggestion = "format" },
                new { Input = new[] { "--dvc", "zepp" }, ExpectedSuggestion = "device" },
                new { Input = new[] { "--dat-typ", "steps" }, ExpectedSuggestion = "data-type" }
            };

            foreach (var testCase in testCases)
            {
                // Act
                var result = _parser.Parse(testCase.Input);

                // Assert
                Assert.False(result.Success);
                Assert.Contains("Did you mean", result.Errors[0]);
                Assert.Contains(testCase.ExpectedSuggestion, result.Errors[0]);
            }
        }
    }
}