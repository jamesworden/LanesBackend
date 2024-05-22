using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using LanesBackend.Models;

namespace LanesBackend.Util
{
  public class ChatUtil
  {
    public static string ReplaceBadWordsWithAsterisks(string rawMessage)
    {
      // Normalize the input string to decompose combined characters (e.g., é -> e + ́)
      string normalizedMessage = rawMessage.Normalize(NormalizationForm.FormD);
      string messageWithoutAccents = StripAccentMarks(normalizedMessage);
      foreach (string badWord in WordConstants.LowerCaseBadWords)
      {
        string normalizedBadWord = badWord.Normalize(NormalizationForm.FormD);
        string strippedBadWord = StripAccentMarks(normalizedBadWord);
        string pattern = @"\b" + string.Join(@"[^\w]*", strippedBadWord.ToCharArray()) + @"\b";
        string asterisks = new string('*', badWord.Length);
        messageWithoutAccents = Regex.Replace(
          messageWithoutAccents,
          pattern,
          asterisks,
          RegexOptions.IgnoreCase
        );
      }

      return messageWithoutAccents;
    }

    public static string StripAccentMarks(string text)
    {
      var sb = new StringBuilder();
      foreach (char c in text)
      {
        if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
        {
          sb.Append(c);
        }
      }
      return sb.ToString().Normalize(NormalizationForm.FormC);
    }
  }
}
