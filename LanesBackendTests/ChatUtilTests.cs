using LanesBackend.Util;

namespace UnitTest
{
  public class ChatUtilTests
  {
    [Theory]
    [InlineData("This is a damn message", "This is a **** message")]
    [InlineData("This is a dámn message", "This is a **** message")]
    [InlineData("This is a d a m n message", "This is a ******* message")]
    [InlineData("This is a worddamnword message", "This is a wor*****word message")]
    [InlineData("This is a damn with multiple damn", "This is a **** with multiple ****")]
    [InlineData("bit ch", "******")]
    [InlineData("biitch", "******")]
    public void ReplaceBadWords_WithAsterisks(string input, string expected)
    {
      // Act
      var result = ChatUtil.ReplaceBadWordsWithAsterisks(input);
      // Assert
      Assert.Equal(expected, result);
    }
  }
}
