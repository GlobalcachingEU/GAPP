using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Plugins.APIFavorites
{
    public class SelectFavorites : Utils.BasePlugin.Plugin
    {
        public const string ACTION_SELECT = "Select your Favorites";

        public override bool Initialize(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_SELECT);

            return base.Initialize(core);
        }

        public override string DefaultAction
        {
            get
            {
                return ACTION_SELECT;
            }
        }

        public override Framework.PluginType PluginType
        {
            get
            {
                return Framework.PluginType.GeocacheSelectFilter;
            }
        }

        public bool GetFavorite(string gcCode)
        {
            return Favorites.Instance(Core).GeocacheFavorited(gcCode);
        }

        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result && action == ACTION_SELECT)
            {
                Core.Geocaches.BeginUpdate();
                foreach (Framework.Data.Geocache gc in Core.Geocaches)
                {
                    gc.Selected = Favorites.Instance(Core).GeocacheFavorited(gc.Code);
                }
                Core.Geocaches.EndUpdate();
            }
            return result;
        }


    }
}
