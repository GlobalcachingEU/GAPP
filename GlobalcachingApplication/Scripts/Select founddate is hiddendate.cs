using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GlobalcachingApplication.Framework.Data;
using GlobalcachingApplication.Framework.Interfaces;
using GlobalcachingApplication.Utils;
using GlobalcachingApplication.Utils.BasePlugin;
using System.Windows.Forms;

class Script
{
    public static bool Run(Plugin plugin, ICore core)
    {
        var gcList = from Geocache gc in core.Geocaches 
                     where gc.FoundDate!=null
                        && ((DateTime)gc.FoundDate).Month == gc.PublishedTime.Month 
                        && ((DateTime)gc.FoundDate).Day == gc.PublishedTime.Day
                     select gc;

        //mass update, so within begin and end
        core.Geocaches.BeginUpdate();

        //reset current selection
        foreach (Geocache gc in core.Geocaches)
        {
            gc.Selected = false;
        }
        //set the intended selection
        foreach (Geocache gc in gcList)
        {
            gc.Selected = true;
        }

        core.Geocaches.EndUpdate();
        return true;
    }
}
