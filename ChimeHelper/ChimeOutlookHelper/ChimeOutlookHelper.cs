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

        if (appointments == null)
          continue;

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
      /*
       * ---------------------------
Chime Helper: AppDomain UnhandledException
---------------------------
Apologies, an unexpected error has occurred! Hit Ctrl+C and send us the output so that we can fix the issue!

System.NullReferenceException: Object reference not set to an instance of an object.

   at ChimeOutlookHelper.ChimeOutlookHelper.GetPins(AppointmentItem appointment)

   at ChimeOutlookHelper.ChimeOutlookHelper.GetMeetings()

   at ChimeHelperUX.ChimeHelperState.CheckForChimeMeetings(Object stateInfo)

   at System.Threading.ExecutionContext.RunInternal(ExecutionContext executionContext, ContextCallback callback, Object state)

--- End of stack trace from previous location where exception was thrown ---

   at System.Threading.TimerQueueTimer.CallCallback(Boolean isThreadPool)

   at System.Threading.TimerQueueTimer.Fire(Boolean isThreadPool)

   at System.Threading.TimerQueue.FireNextTimers()
---------------------------
OK   
---------------------------
*/
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

