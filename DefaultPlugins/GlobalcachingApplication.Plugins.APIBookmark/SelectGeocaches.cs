using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.APIBookmark
{
    public class SelectGeocaches : Utils.BasePlugin.Plugin
    {
        public const string ACTION_BOOKMARK = "Bookmark";

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            BookmarkInfoList.Instance(core).SelectGeocachesPlugin = this;
            BookmarkInfo[] bis = BookmarkInfoList.Instance(core).Bookmarks;
            if (bis != null && bis.Length > 0)
            {
                foreach (BookmarkInfo bi in bis)
                {
                    AddAction(string.Format("{0}|{1}", ACTION_BOOKMARK, bi.Name.Replace('|',' ')));
                }
            }
            return await base.InitializeAsync(core);
        }

        public void BookmarkAdded(BookmarkInfo bi)
        {
            AddAction(string.Format("{0}|{1}", ACTION_BOOKMARK, bi.Name.Replace('|', ' ')));
            Framework.Interfaces.IPluginUIMainWindow main = (from Framework.Interfaces.IPluginUIMainWindow a in Core.GetPlugin(Framework.PluginType.UIMainWindow) select a).FirstOrDefault();
            main.AddAction(this, ACTION_BOOKMARK, bi.Name.Replace('|', ' '));
        }

        public void BookmarkRemoved(BookmarkInfo bi)
        {
            RemoveAction(string.Format("{0}|{1}", ACTION_BOOKMARK, bi.Name.Replace('|', ' ')));
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
                BookmarkInfo[] bis = BookmarkInfoList.Instance(Core).Bookmarks;
                if (bis != null && bis.Length > 0)
                {
                    foreach (BookmarkInfo bi in bis)
                    {
                        if (action == string.Format("{0}|{1}", ACTION_BOOKMARK, bi.Name.Replace('|', ' ')))
                        {
                            Core.Geocaches.BeginUpdate();
                            var gcl = from Framework.Data.Geocache g in Core.Geocaches
                                      join s in bi.GeocacheCodes on g.Code equals s
                                      select g;
                            foreach (Framework.Data.Geocache gc in gcl)
                            {
                                gc.Selected = true;
                            }
                            Core.Geocaches.EndUpdate();
                        }
                    }
                }                
            }
            return true;
        }
    }
}
