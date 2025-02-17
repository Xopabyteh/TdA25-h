using h.Contracts.Users;

namespace h.ValidationTests.Auth;

public class PasswordValidatorTest
{
    [Theory]
    [InlineData("SuperSilneH3sloOhY3@h", true)]
    [InlineData("P@ssw0rd", true)]
    [InlineData("P@ssw0r", false)]
    [InlineData("p@ssword", false)]
    [InlineData("P@SSWORD", false)]
    [InlineData("p4ssword", false)]
    [InlineData("123", false)]
    [InlineData("only7ch", false)]
    public void PasswordValidator_ValidPassword(string passwod, bool expectedValidity)
    {
        // Arrange
        var validator = new SharedPasswordValidator();
        
        // Act
        var result = validator.Validate(passwod);

        // Assert
        Assert.Equal(expectedValidity, result.IsValid);
    }
}
