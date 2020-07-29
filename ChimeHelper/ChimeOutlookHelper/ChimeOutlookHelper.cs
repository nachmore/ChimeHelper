using System;
using System.Collections.Generic;
using Outlook = Microsoft.Office.Interop.Outlook;
using Microsoft.Office.Interop.Outlook;
using System.Diagnostics;
using ChimeHelper;
using System.Linq;

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
      var meetings = new List<ChimeMeeting>();
      var calendars = OutlookHelper.GetCalendars();

      foreach (var calendar in calendars)
      {
        var appointments = OutlookHelper.GetAppointmentsAroundNow(calendar);

        foreach (Outlook.AppointmentItem appointment in appointments)
        {
          // TODO (if needed): GetPins() will collapse duplicate pins for a specific meeting
          //                   but we may need to collapse duplicate meetings across calendars
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
      }

      return meetings.OrderBy(m => m.StartTime).ToList();
    }

    private static HashSet<string> GetPins(AppointmentItem appointment)
    {
      var rv = new List<string>();

      Debug.WriteLine(appointment.Start + " -> " + appointment.End + ": " + appointment.Subject + "\n\tLocation: " + appointment.Location +
        "\n\tRequired Attendees: " + appointment.RequiredAttendees + "\n\tOptional Attendees: " + appointment.OptionalAttendees);

      rv.AddRange(Chime.GetPinsFromAttendees(appointment.OptionalAttendees));
      rv.AddRange(Chime.GetPinsFromAttendees(appointment.RequiredAttendees));

      rv.AddRange(Chime.GetPinsFromText(appointment.Location));
      rv.AddRange(Chime.GetPinsFromText(appointment.Body));

      Debug.WriteLine("\tPins: " + string.Join("\n\t", rv));

      return new HashSet<string>(rv);
    }

  }
}

