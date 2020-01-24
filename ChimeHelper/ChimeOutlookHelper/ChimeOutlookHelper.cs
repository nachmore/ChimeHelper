using System;
using System.Collections.Generic;
using Outlook = Microsoft.Office.Interop.Outlook;
using Microsoft.Office.Interop.Outlook;
using System.Diagnostics;
using ChimeHelper;

namespace ChimeOutlookHelper
{
  public class ChimeOutlookHelper
  {
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

      rv.AddRange(Chime.GetPinsFromText(appointment.Location));
      rv.AddRange(Chime.GetPinsFromText(appointment.Body));

      Debug.WriteLine("\tPins: " + string.Join("\n\t", rv));

      return new HashSet<string>(rv);
    }

  }
}

