using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace ChimeHelper
{
  public static class Chime
  {
    public const string MEETING_URL_FORMAT = "chime://meeting/?pin={0}";

    public static void Join(string pin)
    {
      var chimeUrlProcess = new Process();

      chimeUrlProcess.StartInfo.UseShellExecute = true;
      chimeUrlProcess.StartInfo.FileName = String.Format(MEETING_URL_FORMAT, pin);

      chimeUrlProcess.Start();
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
      var rv = GetRegexMatches(@"(?<!https://(?!chime)+\S+)(?<![\+\d\s]+)\s*(\d[\d ]{9,})\b", text);

      if (rv.Count == 0)
      {
        // if we really haven't been able to find anything yet, then last ditch attempt to
        // find personal IDs by looking for a single word (with special characters) after
        // "Chime: " (ignores case)
        rv = GetRegexMatches(@"Chime:\s*(\S+)\b", text);
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
      var rv = GetRegexMatches(@"chime.aws/(\d{9,})\b", text);

      // if we couldn't find any URLs try more coarse matching
      if (rv.Count == 0)
      {
        rv = GetFreeformPinsFromText(text);
      }

      return rv;
    }

    /// <summary>
    /// Retrieves pins from the list of attendees.
    /// 
    /// i.e. expicitly looks for pin+XXXXXXXXX@chime.aws
    /// 
    /// Note: the reason we don't use freeform search is because that ignores numbers with +
    ///       to avoid capturing international phone numbers
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static List<string> GetPinsFromAttendees(string text)
    {
      return GetRegexMatches(@"pin\+(\d{9,})", text);
    }

    private static List<String> GetRegexMatches(string regex, string target)
    {
      var rv = new List<string>();

      if (string.IsNullOrEmpty(target))
        return rv;

      // try to find an ID in a URL
      var re = new Regex(regex, RegexOptions.Multiline | RegexOptions.IgnoreCase);
      var matches = re.Matches(target);

      foreach (Match match in matches)
      {
        var matchValue = match.Groups[1].Value;

        if (!string.IsNullOrWhiteSpace(matchValue))
        {
          // strip out all whitespace in order to help with dupe matching due to people
          // putting whitespaces in different places
          rv.Add(matchValue.Replace(" ", ""));
        }
      }

      return rv;
    }
  }
}
