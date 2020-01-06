using System;
using System.Collections.Generic;
using Outlook = Microsoft.Office.Interop.Outlook;
using Microsoft.Office.Interop.Outlook;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace ChimeOutlookHelper
{
  public class ChimeOutlookHelper
  {
    public const string MEETING_URL_FORMAT = "chime://meeting/?pin={0}";

    public struct ChimeMeeting
    {
      public string Subject { get; set; }
      public DateTime StartTime { get; set; }
      public DateTime EndTime { get; set; }
      public List<string> Pins { get; set; }
    }

    public static List<ChimeMeeting> GetMeetings()
    {
      var calendar = OutlookHelper.GetCalendar();

      var appointments = OutlookHelper.GetAppointmentsAroundNow(calendar);

      var meetings = new List<ChimeMeeting>();

      foreach (Outlook.AppointmentItem appointment in appointments)
      {
        var pins = GetPins(appointment);

        if (pins.Count > 0)
        {
          meetings.Add(
            new ChimeMeeting()
            {
              Subject = appointment.Subject,
              StartTime = appointment.Start,
              EndTime = appointment.End,
              Pins = new List<string>(pins)
            }
          );
        }
      }

      return meetings;
    }

    private static HashSet<string> GetPins(AppointmentItem appointment)
    {
      var rv = new List<string>();

      Debug.WriteLine(appointment.Start + " -> " + appointment.End + ": " + appointment.Subject + "\n\tLocation: " + appointment.Location);

      rv.AddRange(GetPinsFromText(appointment.Location));
      rv.AddRange(GetPinsFromBody(appointment.Body));

      Debug.WriteLine("\tPins: " + string.Join("\n\t", rv));

      return new HashSet<string>(rv);
    }

    private static List<string> GetPinsFromText(string location)
    {
      var rv = new List<string>();

      if (string.IsNullOrEmpty(location))
        return rv;

      // find chime pin as whole number

      var re = new Regex(@"(?<!https://(?!chime)+\S+)(?<![\+\d\s]+)\s?\d[\d ]{9,}", RegexOptions.Multiline);
      var matches = re.Matches(location);

      foreach (Match match in matches)
      {
        rv.Add(match.Value.Replace(" ", ""));
      }

      return rv;
    }

    // There are too many random numbers in invitation bodies (especially embedded links)
    // to be able to use the simple regex from GetPinsFromLocation
    private static List<string> GetPinsFromBody(string body)
    {
      var rv = new List<string>();

      if (string.IsNullOrEmpty(body))
        return rv;

      // Chime is currently only English, but I have to assume that that will change in the future

      // try to find an ID in a URL
      var re = new Regex(@"chime.aws/(\d{9,})", RegexOptions.Multiline);
      var matches = re.Matches(body);

      foreach (Match match in matches)
      {
        rv.Add(match.Groups[1].Value);
      }

      // if we couldn't find any URLs try more coarse matching
      if (rv.Count == 0)
      {
        rv = GetPinsFromText(body);
      }

      return rv;
    }
  }
}

