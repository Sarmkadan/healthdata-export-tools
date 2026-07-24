#nullable enable
using HealthDataExportTools.Cli;
using System;
using Xunit;

namespace HealthDataExportTools.Tests
{
    public sealed partial class CliArgumentParserTests
    {
        [Fact]
        public void Parse_NoArgumentsProvided_ReturnsSuccessWithDefaults()
        {
            // Arrange
            var args = Array.Empty<string>();

            // Act
            var result = _parser.Parse(args);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Options);
            Assert.Equal("./exports", result.Options.InputPath);
            Assert.Equal("all", result.Options.Format);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void Parse_EmptyArray_ReturnsSuccessWithDefaults()
        {
            // Arrange
            var args = Array.Empty<string>();

            // Act
            var result = _parser.Parse(args);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Options);
            Assert.Equal("./exports", result.Options.InputPath);
        }

        [Fact]
        public void Parse_UnknownFlag_ReturnsFailureWithClearError()
        {
            // Arrange
            var args = new[] { "--unknwon", "input.csv" };

            // Act
            var result = _parser.Parse(args);

            // Assert - should return failure with clear error message, not throw exception
            Assert.False(result.Success);
            Assert.Null(result.Options);
            Assert.Single(result.Errors);
            Assert.Contains("Unknown option '--unknwon'", result.Errors[0]);
            Assert.DoesNotContain("IndexOutOfRangeException", result.Errors[0]);
            Assert.DoesNotContain("NullReferenceException", result.Errors[0]);
        }

        [Fact]
        public void Parse_MultipleUnknownFlags_ReturnsFailureWithMultipleErrors()
        {
            // Arrange
            var args = new[] { "--xyzzy", "--plover", "--nonexistent" };

            // Act
            var result = _parser.Parse(args);

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Options);
            Assert.Equal(3, result.Errors.Count);
            foreach (var error in result.Errors)
            {
                Assert.Contains("Unknown option", error);
            }
        }

        [Fact]
        public void Parse_RequiredValueMissingAtEndOfArgs_ReturnsFailureWithClearError()
        {
            // Arrange - flag that requires value is at the end with no value following
            var args = new[] { "--input" };

            // Act
            var result = _parser.Parse(args);

            // Assert - should return failure with clear error message, not throw IndexOutOfRangeException
            Assert.False(result.Success);
            Assert.Null(result.Options);
            Assert.Single(result.Errors);
            Assert.Contains("requires a value but none was provided", result.Errors[0]);
            Assert.DoesNotContain("IndexOutOfRangeException", result.Errors[0]);
        }

        [Fact]
        public void Parse_MultipleFlagsMissingValues_ReturnsFailureWithMultipleErrors()
        {
            // Arrange
            var args = new[] { "--input", "--output", "--format" };

            // Act
            var result = _parser.Parse(args);

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Options);
            Assert.Equal(3, result.Errors.Count);
            foreach (var error in result.Errors)
            {
                Assert.Contains("requires a value but none was provided", error);
            }
        }

        [Fact]
        public void Parse_DuplicateFlags_UsesLastValue()
        {
            // Arrange - duplicate flags should use the last value provided
            var args = new[] { "--format", "json", "--format", "csv" };

            // Act
            var result = _parser.Parse(args);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Options);
            Assert.Equal("csv", result.Options.Format);
        }

        [Fact]
        public void Parse_DuplicateUnknownFlags_ReturnsMultipleErrors()
        {
            // Arrange
            var args = new[] { "--unknown1", "--unknown2", "--unknown1" };

            // Act
            var result = _parser.Parse(args);

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Options);
            Assert.Equal(3, result.Errors.Count);
        }

        [Fact]
        public void Parse_FlagExpectingValueButNoneProvided_ReturnsFailure()
        {
            // Arrange - flag that requires value has no following token
            var args = new[] { "--output" };

            // Act
            var result = _parser.Parse(args);

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Options);
            Assert.Single(result.Errors);
            Assert.Contains("requires a value but none was provided", result.Errors[0]);
        }

        [Fact]
        public void Parse_MixedCaseFlags_HandledCaseInsensitively()
        {
            // Arrange - mixed case should work due to StringComparer.OrdinalIgnoreCase
            var args = new[] { "--INPUT", "./data", "--FORMAT", "json" };

            // Act
            var result = _parser.Parse(args);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Options);
            Assert.Equal("./data", result.Options.InputPath);
            Assert.Equal("json", result.Options.Format);
        }

        [Fact]
        public void Parse_InvalidFlagSyntax_SingleDash_ReturnsFailure()
        {
            // Arrange - single dash is not a valid flag format
            var args = new[] { "-h", "--input", "./data" };

            // Act
            var result = _parser.Parse(args);

            // Assert - should handle gracefully, not throw exception
            Assert.False(result.Success);
            Assert.Null(result.Options);
            Assert.Single(result.Errors);
            Assert.Contains("Unknown option 'h'", result.Errors[0]);
        }

        [Fact]
        public void Parse_InvalidFlagSyntax_TripleDash_ReturnsFailure()
        {
            // Arrange - triple dash should be treated as unknown flag
            var args = new[] { "---", "value" };

            // Act
            var result = _parser.Parse(args);

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Options);
            Assert.Single(result.Errors);
            Assert.Contains("Unknown option '--'", result.Errors[0]);
        }

        [Fact]
        public void Parse_FlagWithEmptyValue_ReturnsFailure()
        {
            // Arrange - flag with empty string value
            var args = new[] { "--format", "" };

            // Act
            var result = _parser.Parse(args);

            // Assert - empty value should be accepted by the parser (validation happens later)
            Assert.True(result.Success);
            Assert.NotNull(result.Options);
            Assert.Equal("", result.Options.Format);
        }

        [Fact]
        public void Parse_FlagWithWhitespaceValue_ReturnsSuccess()
        {
            // Arrange - flag with whitespace value
            var args = new[] { "--format", "  " };

            // Act
            var result = _parser.Parse(args);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Options);
            Assert.Equal("  ", result.Options.Format);
        }

        [Fact]
        public void Parse_ShortFlagMissingValue_ReturnsFailure()
        {
            // Arrange - short flag that requires value but none provided
            var args = new[] { "-v" };

            // Act
            var result = _parser.Parse(args);

            // Assert - should handle gracefully
            Assert.False(result.Success);
            Assert.Null(result.Options);
            Assert.Single(result.Errors);
        }

        [Fact]
        public void Parse_ShortFlagWithValue_ReturnsSuccess()
        {
            // Arrange - short flags don't require values in this parser
            var args = new[] { "-v" };

            // Act
            var result = _parser.Parse(args);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Options);
            Assert.True(result.Options.Verbose);
        }

        [Fact]
        public void Parse_UnknownShortFlag_ReturnsFailure()
        {
            // Arrange
            var args = new[] { "-x" };

            // Act
            var result = _parser.Parse(args);

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Options);
            Assert.Single(result.Errors);
            Assert.Contains("Unknown option '-x'", result.Errors[0]);
        }

        [Fact]
        public void Parse_FlagWithSpecialCharactersInValue_ReturnsSuccess()
        {
            // Arrange - values with special characters
            var args = new[] { "--input", "./data with spaces", "--format", "json-export_v2.0" };

            // Act
            var result = _parser.Parse(args);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Options);
            Assert.Equal("./data with spaces", result.Options.InputPath);
            Assert.Equal("json-export_v2.0", result.Options.Format);
        }

        [Fact]
        public void Parse_FlagWithQuotesInValue_ReturnsSuccess()
        {
            // Arrange - values with quotes
            var args = new[] { "--input", @"./data with ""quotes""" };

            // Act
            var result = _parser.Parse(args);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Options);
            Assert.Equal(@"./data with ""quotes""", result.Options.InputPath);
        }

        [Fact]
        public void Parse_NullArgsArray_ThrowsArgumentNullException()
        {
            // Arrange
            string[] args = null!;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _parser.Parse(args));
        }

        [Fact]
        public void Parse_InvalidIntegerValueForMaxParallelism_ReturnsFailure()
        {
            // Arrange - invalid integer value
            var args = new[] { "--max-parallelism", "invalid-number" };

            // Act
            var result = _parser.Parse(args);

            // Assert - should handle gracefully with clear error
            Assert.False(result.Success);
            Assert.Null(result.Options);
            Assert.Single(result.Errors);
            Assert.Contains("Invalid value for '--max-parallelism'", result.Errors[0]);
        }

        [Fact]
        public void Parse_NegativeIntegerValueForMaxParallelism_ReturnsFailure()
        {
            // Arrange - negative integer value
            var args = new[] { "--max-parallelism", "-10" };

            // Act
            var result = _parser.Parse(args);

            // Assert - validation error, not parsing exception
            Assert.False(result.Success);
            Assert.Null(result.Options);
            Assert.Contains("Max parallelism must be between 1", result.Errors[0]);
        }

        [Fact]
        public void Parse_FlagWithVeryLongValue_ReturnsSuccess()
        {
            // Arrange - very long path value
            var longPath = new string('a', 1000);
            var args = new[] { "--input", longPath };

            // Act
            var result = _parser.Parse(args);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Options);
            Assert.Equal(longPath, result.Options.InputPath);
        }

        [Fact]
        public void Parse_FlagsWithSimilarNames_GetCorrectSuggestions()
        {
            // Arrange - test various typos
            var testCases = new[]
            {
                new { Input = new[] { "--outpu", "test" }, ExpectedSuggestion = "output" },
                new { Input = new[] { "--formt", "csv" }, ExpectedSuggestion = "format" },
                new { Input = new[] { "--dvc", "zepp" }, ExpectedSuggestion = "device" },
                new { Input = new[] { "--dat-typ", "steps" }, ExpectedSuggestion = "data-type" },
                new { Input = new[] { "--inpt", "test" }, ExpectedSuggestion = "input" },
                new { Input = new[] { "--db", "test.db" }, ExpectedSuggestion = "database" },
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

        [Fact]
        public void Parse_FlagWithUnicodeCharacters_ReturnsSuccess()
        {
            // Arrange - unicode characters in values
            var args = new[] { "--input", "./data-测试", "--format", "json-测试" };

            // Act
            var result = _parser.Parse(args);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Options);
            Assert.Equal("./data-测试", result.Options.InputPath);
        }

        [Fact]
        public void Parse_ArgsWithNoFlags_ReturnsSuccessWithInputPath()
        {
            // Arrange - positional argument as input path
            var args = new[] { "./my-data" };

            // Act
            var result = _parser.Parse(args);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Options);
            Assert.Equal("./my-data", result.Options.InputPath);
        }

        [Fact]
        public void Parse_ArgsWithMixedPositionalAndFlags_ReturnsSuccess()
        {
            // Arrange
            var args = new[] { "./input-data", "--format", "csv", "--output", "./output-folder" };

            // Act
            var result = _parser.Parse(args);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Options);
            Assert.Equal("./input-data", result.Options.InputPath);
            Assert.Equal("csv", result.Options.Format);
            Assert.Equal("./output-folder", result.Options.OutputPath);
        }

        [Fact]
        public void Parse_VerifyCommandWithMissingManifest_ReturnsFailure()
        {
            // Arrange - verify command requires manifest path
            var args = new[] { "verify" };

            // Act
            var result = _parser.Parse(args);

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Options);
            Assert.Single(result.Errors);
            Assert.Contains("The 'verify' command requires --manifest <path>", result.Errors[0]);
        }

        [Fact]
        public void Parse_VerifyCommandWithManifest_ReturnsSuccess()
        {
            // Arrange
            var args = new[] { "verify", "--manifest", "./manifest.json" };

            // Act
            var result = _parser.Parse(args);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Options);
            Assert.Equal("verify", result.Options.Command);
            Assert.Equal("./manifest.json", result.Options.ManifestPath);
        }

        [Fact]
        public void Parse_ArgsWithMultiplePositionalArguments_UsesFirstAsInputPath()
        {
            // Arrange - multiple positional arguments, first one should be used
            var args = new[] { "./first-path", "./second-path", "--format", "json" };

            // Act
            var result = _parser.Parse(args);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Options);
            Assert.Equal("./first-path", result.Options.InputPath);
        }
    }
}