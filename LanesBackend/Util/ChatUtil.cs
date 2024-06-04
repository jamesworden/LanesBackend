using System.Text;
using System.Text.RegularExpressions;
using LanesBackend.Models;

namespace LanesBackend.Util
{
  public static class ChatUtil
  {
    public static string ReplaceBadWordsWithAsterisks(string input)
    {
      StringBuilder result = new();

      foreach (char c in input)
      {
        if (IsValidUnicodeCharacter(c))
        {
          result.Append(NormalizeChar(c));
        }
      }

      string pattern = string.Join(
        "|",
        WordConstants
          .LowerCaseBadWords.Where(word => word.Length > 2)
          .Select(word =>
            string.Join(@"\s*", word.ToCharArray().Select(c => $"[{Regex.Escape(c.ToString())}]+"))
          )
      );

      string replaced = Regex.Replace(
        result.ToString(),
        pattern,
        match => new string('*', match.Value.Length),
        RegexOptions.IgnoreCase
      );

      return replaced;
    }

    private static bool IsValidUnicodeCharacter(char c)
    {
      return !char.IsSurrogate(c);
    }

    private static string NormalizeChar(char c)
    {
      foreach (KeyValuePair<string, string> entry in WordConstants.ForeignCharacters)
      {
        if (entry.Key.Contains(c))
        {
          return entry.Value[0].ToString();
        }
      }
      return c.ToString();
    }
  }
}
