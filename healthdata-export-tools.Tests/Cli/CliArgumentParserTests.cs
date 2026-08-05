#nullable enable
using HealthDataExportTools.Cli;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace HealthDataExportTools.Tests
{
    /// <summary>
    /// Contains unit tests for the <see cref="CliArgumentParser"/> class.
    /// </summary>
    public sealed partial class CliArgumentParserTests
    {
        private readonly CliArgumentParser _parser;
        private readonly Mock<ILogger<CliArgumentParserTests>> _loggerMock = new();
        private readonly ILogger<CliArgumentParserTests> _logger;

        /// <summary>
        /// Initializes a new instance of <see cref="CliArgumentParserTests"/>.
        /// Creates a <see cref="CliArgumentParser"/> and a mock logger for use in the tests.
        /// </summary>
        public CliArgumentParserTests()
        {
            _parser = new CliArgumentParser();
            _logger = _loggerMock.Object;
        }

        /// <summary>
        /// Verifies that parsing a valid full command (input and output paths) succeeds
        /// and that the resulting <see cref="ParseResult{T}.Options"/> contain the expected values.
        /// </summary>
        [Fact]
        public void Parse_ValidFullCommand_ReturnsSuccessResult()
        {
            _logger.LogInformation("Executing {Method}", nameof(Parse_ValidFullCommand_ReturnsSuccessResult));

            // Arrange
            var args = new[] { "--input", "input.csv", "--output", "output.json" };

            // Act
            var result = _parser.Parse(args);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Options);
            Assert.Equal("input.csv", result.Options.InputPath);
            Assert.Equal("output.json", result.Options.OutputPath);
            
            _logger.LogInformation("Completed {Method}", nameof(Parse_ValidFullCommand_ReturnsSuccessResult));
        }

        /// <summary>
        /// Ensures that an unknown flag is reported as a failure and that a suggestion
        /// containing the closest matching known flag is included in the error message.
        /// </summary>
        [Fact]
        public void Parse_UnknownFlag_ReturnsFailureResultWithSuggestion()
        {
            _logger.LogInformation("Executing {Method}", nameof(Parse_UnknownFlag_ReturnsFailureResultWithSuggestion));

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
            
            _logger.LogInformation("Completed {Method}", nameof(Parse_UnknownFlag_ReturnsFailureResultWithSuggestion));
        }

        /// <summary>
        /// Verifies that an unknown flag without a close match results in a failure
        /// with an error message that does not contain a suggestion.
        /// </summary>
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

        /// <summary>
        /// Checks that omitting a required value for an option (e.g., <c>--input</c>) results in a failure.
        /// </summary>
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

        /// <summary>
        /// Confirms that providing the <c>--help</c> flag returns a successful result with the Help option set.
        /// </summary>
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

        /// <summary>
        /// Validates that invalid date strings for <c>--start-date</c> and <c>--end-date</c>
        /// produce two errors indicating the respective format problems.
        /// </summary>
        [Fact]
        public void Parse_InvalidDateFormat_ReturnsFailureResult()
        {
            _logger.LogInformation("Executing {Method}", nameof(Parse_InvalidDateFormat_ReturnsFailureResult));
            
            // Arrange
            var args = new[] { "--start-date", "invalid-date", "--end-date", "2025-13-45" };

            // Act
            var result = _parser.Parse(args);

            // Assert
            Assert.False(result.Success);
            _logger.LogWarning("Parse failed for {Args} with {ErrorCount} errors", args, result.Errors.Count);
            Assert.Null(result.Options);
            Assert.Equal(2, result.Errors.Count);
            Assert.Contains("Invalid start date format", result.Errors[0]);
            Assert.Contains("Invalid end date format", result.Errors[1]);
        }

        /// <summary>
        /// Ensures that correctly formatted start and end dates are parsed successfully.
        /// </summary>
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

        /// <summary>
        /// Checks that a start date later than the end date results in a failure with an appropriate error.
        /// </summary>
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

        /// <summary>
        /// Verifies that an unsupported format value (e.g., <c>xml</c>) causes a failure with a descriptive error.
        /// </summary>
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

        /// <summary>
        /// Confirms that a supported format value (e.g., <c>csv</c>) parses successfully.
        /// </summary>
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

        /// <summary>
        /// Ensures that a max parallelism value less than 1 is rejected with an appropriate error message.
        /// </summary>
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

        /// <summary>
        /// Checks that a valid max parallelism value equal to the processor count parses successfully.
        /// </summary>
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

        /// <summary>
        /// Validates that a negative cache duration is rejected with a specific error.
        /// </summary>
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

        /// <summary>
        /// Tests that <see cref="CliArgumentParser.TryParse(string[], out var)"/> succeeds for valid arguments
        /// and populates the output <see cref="CliOptions"/> with the expected values.
        /// </summary>
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

        /// <summary>
        /// Ensures that <see cref="CliArgumentParser.TryParse(string[], out var)"/> returns <c>false</c>
        /// and a <c>null</c> options object when the arguments contain an unknown flag.
        /// </summary>
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

        /// <summary>
        /// Verifies that <see cref="CliArgumentParser.ParseWithValidation(string[])"/> returns a successful
        /// <see cref="ParseResult{T}"/> with populated options for a valid set of arguments.
        /// </summary>
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

        /// <summary>
        /// Tests that the Levenshtein‑distance based suggestion mechanism proposes the correct flag
        /// for a variety of common typographical errors.
        /// </summary>
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
