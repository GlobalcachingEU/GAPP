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
        DateTime dt = DateTime.Now.AddDays(-7);
        var gcList = from Geocache gc in core.Geocaches 
                     where gc.DataFromDate < dt 
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

        if (MessageBox.Show(LanguageSupport.Instance.GetTranslation("Automatically archive selected geocaches?"),LanguageSupport.Instance.GetTranslation("Archive"), MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2)== DialogResult.Yes)
        {
            foreach (Geocache gc in gcList)
            {
                gc.Archived = true;
                gc.Available = false;
            }
        }

        core.Geocaches.EndUpdate();
        return true;
    }
}
