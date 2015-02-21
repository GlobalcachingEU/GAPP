using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GlobalcachingApplication.Framework.Interfaces;
using GlobalcachingApplication.Framework.Data;
using GlobalcachingApplication.Utils;
using GlobalcachingApplication.Utils.BasePlugin;

class Script
{
    public static bool Run(Plugin plugin, ICore core)
    {
        int[,] DTMatrix = new int[9, 9];
        int numMissing=9*9;
        int count=0;
        int prog=0;
        
        List<Geocache> gcList = null;
        gcList = DataAccess.GetFoundGeocaches(core.Geocaches,core.Logs,core.GeocachingComAccount);
        
        using (ProgressBlock progress = new ProgressBlock(plugin,
            "Creating S/T Matrix", "Seeking for Neo", gcList.Count, 0))
        {
          foreach (Geocache gc in gcList)
          {
              int ixD=(int)(gc.Difficulty*2+0.001)-2;
              int ixT=(int)(gc.Terrain*2+0.001)-2;
              //range checking is for cowards!
              if (DTMatrix[ixD,ixT]==0) {numMissing--;}            
              DTMatrix[ixD,ixT]++;
              
              prog++;
              if (prog % 10 == 0) {progress.UpdateProgress("Creating S/T Matrix", "Seeking for Neo", gcList.Count, prog);}
          }
        }
        
        prog=0;
        using (ProgressBlock progress = new ProgressBlock(plugin,
            "Selecting Caches", "", core.Geocaches.Count, 0))
        {
          //reset current selection
          foreach (Geocache gc in core.Geocaches)
          {
              int ixD=(int)(gc.Difficulty*2+0.001)-2;
              int ixT=(int)(gc.Terrain*2+0.001)-2;
             
              gc.Selected = (DTMatrix[ixD,ixT]==0);
              if (DTMatrix[ixD,ixT]==0) {count++;}
              prog++;
              if (prog % 50 == 0) {progress.UpdateProgress("Selecting Caches", "", core.Geocaches.Count, prog);}
          }
        }

        System.Windows.Forms.MessageBox.Show("Selected: "+count.ToString()+
                     "; Missing combinations in D/T Matrix: "+numMissing.ToString()+
                     " of "+gcList.Count.ToString()+" found Geocaches.");
        
        return true;
    }
}
