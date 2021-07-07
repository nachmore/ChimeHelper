using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace ChimeHelper
{
  public static class Chime
  {
    #region Native Functions & Helpers
    delegate bool EnumThreadDelegate(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool EnumThreadWindows(int dwThreadId, EnumThreadDelegate lpfn, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, int wParam, [Out] StringBuilder lParam);

    const int WM_GETTEXT = 0xD;

    private static IEnumerable<IntPtr> EnumerateProcessWindowHandles(int processId)
    {
      var handles = new List<IntPtr>();

      foreach (ProcessThread thread in Process.GetProcessById(processId).Threads)
        EnumThreadWindows(thread.Id,
            (hWnd, lParam) => { handles.Add(hWnd); return true; }, IntPtr.Zero);

      return handles;
    }
    #endregion

    public const string MEETING_URL_FORMAT = "chime://meeting/?pin={0}";

    public static void Join(string pin)
    {
      var chimeUrlProcess = new Process();

      // strip whitespace out of PINs since Chime sometimes refuses to parse the space
      chimeUrlProcess.StartInfo.FileName = String.Format(MEETING_URL_FORMAT, pin.Replace(" ", ""));
      chimeUrlProcess.StartInfo.UseShellExecute = true;

      chimeUrlProcess.Start();
    }

    /// <summary>
    /// Check if a meeting is already joined / opened on the current machine
    /// </summary>
    /// <param name="meetingSubject"></param>
    /// <returns></returns>
    public static bool IsMeetingAlreadyJoined(string meetingSubject)
    {
      var chimeProc = Process.GetProcessesByName("chime");

      // no running Chime
      if (chimeProc.Length == 0)
        return false;

      var procId = chimeProc[0].Id;

      foreach (var handle in EnumerateProcessWindowHandles(procId))
      {
        var title = new StringBuilder(65535);
        SendMessage(handle, WM_GETTEXT, title.Capacity, title);

        if (title.ToString() == meetingSubject)
          return true;
      }

      return false;
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
        // "Chime(room)"
        // etc
        rv = GetRegexMatches(@"Chime[^A-Za-z]*([A-Za-z]+)[^A-Za-z]", text);
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
        // ignore "chime.aws/dialinnumbers" as that redirects to an actual webpage
        rv = GetRegexMatches(@"chime.aws/(?!dialinnumbers)(.*)\b", text);

        if (rv.Count == 0)
        {
          // No URL based PINs found, go freeform...

          rv = GetFreeformPinsFromText(text);
        }
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
