using System;
using System.Collections.Generic;
using Outlook = Microsoft.Office.Interop.Outlook;
using Microsoft.Office.Interop.Outlook;
using System.Text.RegularExpressions;

namespace ChimeOutlookHelper
{
  public class ChimeOutlookHelper
  {

    public interface IChimeMeeting
    {
      string Subject { get; set; }
      DateTime StartTime { get; set; }
      DateTime EndTime { get; set; }
      HashSet<string> Meetings { get; set; }
    }

    public struct ChimeMeeting : IChimeMeeting
    {
      public string Subject { get; set; }
      public DateTime StartTime { get; set; }
      public DateTime EndTime { get; set; }
      public HashSet<string> Meetings { get; set; }
    }

    public static List<ChimeMeeting> GetMeetings()
    {
      var calendar = OutlookHelper.GetCalendar();

      var appointments = OutlookHelper.GetAppointmentsAroundNow(calendar, 15);

      var meetings = new List<ChimeMeeting>();

      foreach (Outlook.AppointmentItem appointment in appointments)
      {
        var bridges = GetBridges(appointment);

        if (bridges.Count > 0)
        {
          meetings.Add(
            new ChimeMeeting()
            {
              Subject = appointment.Subject,
              StartTime = appointment.Start,
              EndTime = appointment.End,
              Meetings = bridges
            }
          );
        }
      }

      return meetings;
    }

    private static HashSet<string> GetBridges(AppointmentItem appointment)
    {
      var rv = new List<string>();

      rv.AddRange(GetBridgeFromLocation(appointment.Location));
      rv.AddRange(GetBridgeFromBody(appointment.Body));

      return new HashSet<string>(rv);
    }

    private static List<string> GetBridgeFromLocation(string location)
    {
      var rv = new List<string>();

      if (string.IsNullOrEmpty(location))
        return rv;

      // find chime bridge as whole number

      var re = new Regex(@"\d[\d ]{9,}", RegexOptions.Multiline);
      var matches = re.Matches(location);

      foreach (Match match in matches)
      {
        rv.Add(match.Value.Replace(" ", ""));
      }

      return rv;
    }

    // There are too many random numbers in invitation bodies (especially embedded links)
    // to be able to use the simple regex from GetBridgeFromLocation
    private static List<string> GetBridgeFromBody(string body)
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
        rv = GetBridgeFromLocation(body);
      }

      return rv;
    }
  }
}

