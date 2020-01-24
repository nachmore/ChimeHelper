using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ChimeHelper
{
  public static class Chime
  {
    public const string MEETING_URL_FORMAT = "chime://meeting/?pin={0}";

    public static void Join(string pin)
    {
      System.Diagnostics.Process.Start(String.Format(MEETING_URL_FORMAT, pin));
    }

    /// <summary>
    /// Retrieves any numbers from text that look like they may be a Chime pin.
    ///
    /// Ignores: numbers in URLs that don't also contain the word "chime"
    /// Ignores: long phone numbers that start with + (for country code), but will catch regular
    ///          phone numbers
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    private static List<string> GetFreeformPinsFromText(string text)
    {
      var rv = new List<string>();

      if (string.IsNullOrEmpty(text))
        return rv;

      // find chime pin as whole number

      var re = new Regex(@"(?<!https://(?!chime)+\S+)(?<![\+\d\s]+)\s?\d[\d ]{9,}", RegexOptions.Multiline);
      var matches = re.Matches(text);

      foreach (Match match in matches)
      {
        rv.Add(match.Value.Replace(" ", ""));
      }

      return rv;
    }

    /// <summary>
    /// Retrieve Pins from text
    ///
    /// This will initially try to grab pins from chime URLs (the most accurate), but will fall back
    /// to more fuzzy matching if none are found.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static List<string> GetPinsFromText(string text)
    {
      var rv = new List<string>();

      if (string.IsNullOrEmpty(text))
        return rv;

      // Chime is currently only English, but I have to assume that that will change in the future

      // try to find an ID in a URL
      var re = new Regex(@"chime.aws/(\d{9,})", RegexOptions.Multiline);
      var matches = re.Matches(text);

      foreach (Match match in matches)
      {
        rv.Add(match.Groups[1].Value);
      }

      // if we couldn't find any URLs try more coarse matching
      if (rv.Count == 0)
      {
        rv = GetFreeformPinsFromText(text);
      }

      return rv;
    }
  }
}
