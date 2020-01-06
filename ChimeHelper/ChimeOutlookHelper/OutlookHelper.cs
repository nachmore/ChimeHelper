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
      var start = DateTime.Now.Subtract(new TimeSpan(hours, 0, 0));
      var end = start.Add(new TimeSpan(hours * 2, 0, 0));

      return GetAppointmentsInRange(calendar, start, end);
    }

    public static Outlook.Items GetAppointmentsInRange(Outlook.MAPIFolder folder, DateTime start, DateTime end, bool includeRecurrences = true)
    {
      var filter = $"[Start] >= '{start.ToString("g")}' AND [End] <= '{end.ToString("g")}'";

      var items = folder.Items;
      items.IncludeRecurrences = includeRecurrences;
      items.Sort("[Start]", Type.Missing);

      return items.Restrict(filter);
    }

  }
}
