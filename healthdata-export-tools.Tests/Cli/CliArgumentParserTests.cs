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
        public void ParseArguments_ValidFullCommand_ReturnsValidOptions()
        {
            // Arrange
            var args = new[] { "--input", "input.csv", "--output", "output.json" };

            // Act
            var result = _parser.ParseArguments(args);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Options);
            Assert.Equal("input.csv", result.Options.Input);
            Assert.Equal("output.json", result.Options.Output);
        }

        [Fact]
        public void ParseArguments_UnknownFlag_ReturnsFailure()
        {
            // Arrange
            var args = new[] { "--unknown", "input.csv", "--output", "output.json" };

            // Act
            var result = _parser.ParseArguments(args);

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Options);
        }

        [Fact]
        public void ParseArguments_MissingRequiredValue_ReturnsFailure()
        {
            // Arrange
            var args = new[] { "--input" };

            // Act
            var result = _parser.ParseArguments(args);

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Options);
        }

        [Fact]
        public void ParseArguments_DuplicateFlags_ReturnsFailure()
        {
            // Arrange
            var args = new[] { "--input", "input.csv", "--input", "input2.csv" };

            // Act
            var result = _parser.ParseArguments(args);

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Options);
        }

        [Fact]
        public void ParseArguments_HelpFlag_ReturnsHelp()
        {
            // Arrange
            var args = new[] { "--help" };

            // Act
            var result = _parser.ParseArguments(args);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Options);
            Assert.True(result.Options.ShowHelp);
        }
    }
}
