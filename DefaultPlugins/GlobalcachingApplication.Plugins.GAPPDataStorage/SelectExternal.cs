using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.GAPPDataStorage
{
    public class SelectExternal: Utils.BasePlugin.Plugin
    {
        public const string ACTION_SELECTPRESENT = "Other database|Select geocaches also present in other gpp database";
        public const string ACTION_SELECTNOTPRESENT = "Other database|Select geocaches not present in other gpp database";

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_SELECTPRESENT);
            AddAction(ACTION_SELECTNOTPRESENT);
            return await base.InitializeAsync(core);
        }

        public override Framework.PluginType PluginType
        {
            get
            {
                return Framework.PluginType.GeocacheSelectFilter;
            }
        }

        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result)
            {
                if (action == ACTION_SELECTPRESENT || action == ACTION_SELECTNOTPRESENT)
                {
                    List<string> gcList = getGeocacheCodes();
                    if (gcList != null)
                    {
                        Core.Geocaches.BeginUpdate();
                        if (action == ACTION_SELECTPRESENT)
                        {
                            foreach (Framework.Data.Geocache gc in Core.Geocaches)
                            {
                                gc.Selected = false;
                            }
                            var gl = from Framework.Data.Geocache g in Core.Geocaches
                                     join s in gcList on g.Code equals s
                                     select g;
                            foreach (Framework.Data.Geocache gc in gl)
                            {
                                gc.Selected = true;
                            }
                        }
                        else if (action == ACTION_SELECTNOTPRESENT)
                        {
                            foreach (Framework.Data.Geocache gc in Core.Geocaches)
                            {
                                gc.Selected = true;
                            }
                            var gl = from Framework.Data.Geocache g in Core.Geocaches
                                     join s in gcList on g.Code equals s
                                     select g;
                            foreach (Framework.Data.Geocache gc in gl)
                            {
                                gc.Selected = false;
                            }
                        }
                        Core.Geocaches.EndUpdate();
                    }
                }
            }
            return result;
        }

        List<string> getGeocacheCodes()
        {
            List<string> result = null;
            try
            {
                using (System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog())
                {
                    dlg.Filter = "*.gpp|*.gpp";
                    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        result = new List<string>();

                        string fn = Path.Combine(Path.GetDirectoryName(dlg.FileName), string.Concat(Path.GetFileNameWithoutExtension(dlg.FileName), ".cch"));
                        if (File.Exists(fn))
                        {
                            int lsize = sizeof(long);
                            byte[] memBuffer = new byte[1024];
                            using (MemoryStream ms = new MemoryStream(memBuffer))
                            using (BinaryReader br = new BinaryReader(ms))
                            using (FileStream fs = File.OpenRead(fn))
                            {
                                fs.Position = 0;
                                long eof = fs.Length;
                                long offset = 0;
                                long length = 0;
                                while (fs.Position < eof)
                                {
                                    offset = fs.Position;
                                    fs.Read(memBuffer, 0, lsize + 1);
                                    ms.Position = 0;
                                    length = br.ReadInt64();
                                    if (memBuffer[lsize] == 1)
                                    {
                                        int readCount = Math.Min(42, (int)(length - lsize - 1));
                                        fs.Read(memBuffer, 0, readCount);
                                        ms.Position = 0;
                                        result.Add(br.ReadString());
                                    }
                                    fs.Position = offset + length;
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
            }
            return result;
        }
    }
}
