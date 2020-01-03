using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChimeHelper
{
  /// <summary>
  /// Manages state for the ChimeHelper application, including the Tray and timer for detecting meetings
  /// </summary>
  class ChimeHelperState
  {
    private static ChimeHelperState _instance;

    private ChimeHelperState() 
    {
        
    }

    public static ChimeHelperState Create()
    {
      if (_instance == null)
        _instance = new ChimeHelperState();

      return _instance;
    }



    }
  }
