using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Outlook = Microsoft.Office.Interop.Outlook;
using System.Diagnostics;

namespace ChimeOutlookHelper
{
  static class OutlookHelper
  {

    private const int DEFAULT_SEARCH_HOURS = 1;

    public static Outlook.MAPIFolder GetCalendar()
    {
      var outlook = new Outlook.Application();
      var stores = outlook.Session.Stores;

      foreach (Outlook.Store store in stores)
      {
        try
        {

          Outlook.MAPIFolder folder = store.GetDefaultFolder(Outlook.OlDefaultFolders.olFolderCalendar);
          System.Diagnostics.Debug.WriteLine(folder.Name);

          return folder;
        }
        catch (Exception e)
        {
          // Not every root folder has a calendar (for example, Public folders), so this exceptionc can be ignored
          Debug.WriteLine("Failed to get Calendar:\n" + e);
        }
      }

      throw new InvalidOperationException("Couldn't find a Calendar in the current Outlook installation");
    }

    public static Outlook.Items GetAppointmentsAroundNow(Outlook.MAPIFolder calendar, int hours = DEFAULT_SEARCH_HOURS)
    {
      var now = DateTime.Now;

      // align to the top of the hour since most meetings are aligned to the hour boundary
      var start = now.Subtract(new TimeSpan(hours, now.Minute, 0));

      // ignore the current hour (so the buffer applies back and forward equally)
      // don't worry about seconds, as :00 will still match
      var end = now.Add(new TimeSpan(hours, 60 - now.Minute, 0));

      return GetAppointmentsInRange(calendar, start, end);
    }

    public static Outlook.Items GetAppointmentsInRange(Outlook.MAPIFolder folder, DateTime start, DateTime end, bool includeRecurrences = true)
    {
      var filter = $"[Start] >= '{start.ToString("g")}' AND [Start] <= '{end.ToString("g")}'";

      var items = folder.Items;
      items.IncludeRecurrences = includeRecurrences;
      items.Sort("[Start]", Type.Missing);

      return items.Restrict(filter);
    }

  }
}
