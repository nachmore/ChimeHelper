using System;

namespace ChimeHelper
{
  public static class Chime
  {
    public const string MEETING_URL_FORMAT = "chime://meeting/?pin={0}";

    public static void Join(string pin)
    {
      System.Diagnostics.Process.Start(String.Format(MEETING_URL_FORMAT, pin));
    }

  }
}
