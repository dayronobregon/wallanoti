using Wallanoti.Src.Users.Domain.ValueObjects;

namespace Wallanoti.Tests.Users._1_Domain;

public class VerificationCodeTest
{
    [Fact]
    public void Verify_ShouldReturnTrueForMatchingCode()
    {
        var code = new VerificationCode("123456");

        Assert.True(code.Verify("123456"));
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("abcdef")]
    [InlineData("654321")]
    public void Verify_ShouldReturnFalseForInvalidCodes(string input)
    {
        var code = new VerificationCode("123456");

        Assert.False(code.Verify(input));
    }
}
