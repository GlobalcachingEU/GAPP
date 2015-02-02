﻿using GlobalcachingApplication.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.AutoSelect
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

        public bool AutoSelectNewGeocaches
        {
            get { return _core.SettingsProvider.GetSettingsValueBool("AutoSelect.AutoSelectNewGeocaches", false); }
            set { _core.SettingsProvider.SetSettingsValueBool("AutoSelect.AutoSelectNewGeocaches", value); }
        }

    }
}