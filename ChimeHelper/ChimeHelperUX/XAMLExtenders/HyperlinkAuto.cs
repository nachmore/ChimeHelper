using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Navigation;

namespace ChimeHelperUX.XAMLExtenders
{
  class HyperlinkAuto : Hyperlink
  {
    public HyperlinkAuto() : base()
    {
      this.RequestNavigate += HyperlinkAuto_RequestNavigate;
    }

    private void HyperlinkAuto_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
      if (NavigateUri != null)
      {
        Process.Start(new ProcessStartInfo(NavigateUri.AbsoluteUri));
      }
    }

  }
}
