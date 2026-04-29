using FluentAssertions;
using Shop.Domain;

namespace Shop.Tests;

/// <summary>Тесты валидации текста в паттерне Arrange — Act — Assert.</summary>
public class TextValidationTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void EnsureNotBlank_NullEmptyOrWhitespace_Throws(string? value)
    {
        // Arrange
        const string paramName = "name";

        // Act
        var act = () => TextValidation.EnsureNotBlank(value, paramName);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void EnsureNotBlank_NormalText_ReturnsSameValue()
    {
        // Arrange
        const string value = "abc";
        const string paramName = "name";

        // Act
        var result = TextValidation.EnsureNotBlank(value, paramName);

        // Assert
        result.Should().Be(value);
    }
}

