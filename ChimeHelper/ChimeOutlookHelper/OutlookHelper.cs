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

    public static List<Outlook.Folder> GetCalendars()
    {
      var folders = new List<Outlook.Folder>();

      Outlook.Application outlook;
      Outlook.Stores stores = null;

      try
      {
        outlook = new Outlook.Application();
        stores = outlook.Session.Stores;
      }
      catch (Exception e)
      {
        Debug.WriteLine($"Exception initializing Outlook in GetCalendars.\n{e}");

        // this can generally be ignored (it's often a COM RETRYLATER when Outlook is stuck
        // booting up etc). Regardless, there is no remediation, so let's bail.
        //
        // Note: Yes, this is a double return, yes the for loop will just exit and it will return
        //       anyway, but who knows what will get added post the for at some point in the future
        //       and what chaose that will cause. This exception is exceedingly rare so would rather
        //       fail fast.
        return folders;
      }

      // foreach on COM objects can sometimes get into weird states when encountering
      // a corrupt pst,  where null objects repeat themselves, and a foreach goes into
      // an infinite loop, so prefer traditional for
      //
      // Note: These are one-based arrays
      //       See: https://docs.microsoft.com/en-us/dotnet/api/microsoft.office.interop.outlook._stores.item?view=outlook-pia#Microsoft_Office_Interop_Outlook__Stores_Item_System_Object_
      for (int i = 1; i <= stores?.Count; i++)
        {
          Outlook.Store store = null;

          try
          {
            // this is in the try since sometimes COM will freak out and throw
            // IndexOutOfRangeException even though we're < Count (corrupt pst situation)
            store = stores[i];

            // ignore public folders (causes slow Exchange calls, and we don't have a use case
            // for interactions with those)
            if (store.ExchangeStoreType == Outlook.OlExchangeStoreType.olExchangePublicFolder)
              continue;

            var folder = (Outlook.Folder)store.GetDefaultFolder(Outlook.OlDefaultFolders.olFolderCalendar);
            System.Diagnostics.Debug.WriteLine($"Found calendar: {folder.Name} in store {store.DisplayName}");

            folders.Add(folder);
          }
          catch (Exception e)
          {
            // Not every root folder has a calendar (for example, Public folders), so this exception can be ignored
            Debug.WriteLine($"Failed to get Calendar for {store?.DisplayName} type: {store?.ExchangeStoreType}:\n{e}");
          }
        }

      return folders;
    }

    public static List<Outlook.AppointmentItem> GetAppointmentsAroundNow(Outlook.Folder calendar, int hours = DEFAULT_SEARCH_HOURS)
    {
      try
      {
        var now = DateTime.Now;

        // align to the top of the hour since most meetings are aligned to the hour boundary
        var start = now.Subtract(new TimeSpan(hours, now.Minute, 0));

        // ignore the current hour (so the buffer applies back and forward equally)
        // don't worry about seconds, as :00 will still match
        var end = now.Add(new TimeSpan(hours, 60 - now.Minute, 0));

        return GetAppointmentsInRange(calendar, start, end);
      }
      catch (Exception e)
      {
        Debug.WriteLine($"Swallowed Exception in GetAppointmentsAroundNow: {e}");

        return null;
      }
    }

    public static List<Outlook.AppointmentItem> GetAppointmentsInRange(Outlook.Folder folder, DateTime start, DateTime end, bool includeRecurrences = true)
    {
      // we originally combined the filters with an "OR" but it would seemingly miss some random meetings for no apparent reason
      // explored whether or not there was API caching but couldn't find any. Splitting it out into two queries seems to solve the 
      // issue, even if the code is a little more weird.

      // find all meetings that start within the period (regardless of when they end specifically)
      var filter = $"[Start] >= '{start.ToString("g")}' AND [Start] <= '{end.ToString("g")}'";

      var rv = RestrictItems(folder, filter, includeRecurrences);

      // find meetings that are in-progress during the period (for example, all day events that start before the period but end
      // during or after it
      filter = $"[Start] < '{start.ToString("g")}' AND [End] >= '{start.ToString("g")}'";

      rv.AddRange(RestrictItems(folder, filter, includeRecurrences));

      return rv;
    }

    public static List<Outlook.AppointmentItem> RestrictItems(Outlook.Folder folder, string filter, bool includeRecurrences)
    {
      var rv = new List<Outlook.AppointmentItem>();

      var items = folder.Items;
      items.IncludeRecurrences = includeRecurrences;
      items.Sort("[Start]", Type.Missing);

      var restricted = items.Restrict(filter);

      foreach (Outlook.AppointmentItem item in restricted)
      {
        rv.Add(item);

        Debug.WriteLine("++: " + item.Start + " -> " + item.End + ": " + item.Subject);
      }

      return rv;
    }

  }
}
