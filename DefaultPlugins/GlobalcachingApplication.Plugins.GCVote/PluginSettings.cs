using GlobalcachingApplication.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.GCVote
{
    public class PluginSettings
    {
        public static PluginSettings _uniqueInstance = null;
        private ICore _core = null;

        public PluginSettings(ICore core)
        {
            _uniqueInstance = this;
            _core = core;
        }

        public static PluginSettings Instance
        {
            get { return _uniqueInstance; }
        }


        public bool ActivateAtAtartup
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("GCVote.ActivateAtAtartup", false); }
            set { _core.SettingsProvider.SetSettingsValueBool("GCVote.ActivateAtAtartup", value); }
        }

        public string GCVoteUsername
        {
            get { return _core.SettingsProvider.GetSettingsValue("GCVote.GCVoteUsername", null); }
            set { _core.SettingsProvider.SetSettingsValue("GCVote.GCVoteUsername", value); }
        }

        public string GCVotePassword
        {
            get { return _core.SettingsProvider.GetSettingsValue("GCVote.GCVotePassword", null); }
            set { _core.SettingsProvider.SetSettingsValue("GCVote.GCVotePassword", value); }
        }

    }
}
