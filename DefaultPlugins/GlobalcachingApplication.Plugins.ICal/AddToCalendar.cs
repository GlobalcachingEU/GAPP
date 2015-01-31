using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.ICal
{
    public class AddToCalendar : Utils.BasePlugin.Plugin
    {
        public const string ACTION_ADD_ALL = "Add to calendar|All events";
        public const string ACTION_ADD_SELECTED = "Add to calendar|Selected events";
        public const string ACTION_ADD_ACTIVE = "Add to calendar|Active event";

        public const string STR_NOGEOCACHESELECTED = "No geocache selected for export";
        public const string STR_ERROR = "Error";

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            var p = new PluginSettings(core);

            AddAction(ACTION_ADD_ALL);
            AddAction(ACTION_ADD_SELECTED);
            AddAction(ACTION_ADD_ACTIVE);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_NOGEOCACHESELECTED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ERROR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(AddToCalendarForm.STR_ADDONETOCAL));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(AddToCalendarForm.STR_ADDTO));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(AddToCalendarForm.STR_ADDTOCAL));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(AddToCalendarForm.STR_BY));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(AddToCalendarForm.STR_DESCRIPTION));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(AddToCalendarForm.STR_DESCRIPTIONB));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(AddToCalendarForm.STR_END));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(AddToCalendarForm.STR_EVENT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(AddToCalendarForm.STR_GEOCACHEEVENT));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(AddToCalendarForm.STR_GOOGLEXPL));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(AddToCalendarForm.STR_LOCATION));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(AddToCalendarForm.STR_SAVE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(AddToCalendarForm.STR_START));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(AddToCalendarForm.STR_SUMMARY));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(AddToCalendarForm.STR_TITLE));

            return await base.InitializeAsync(core);
        }

        public override Framework.PluginType PluginType
        {
            get
            {
                return Framework.PluginType.Action;
            }
        }

        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result)
            {
                List<Framework.Data.Geocache> gcList = null;

                if (action == ACTION_ADD_ALL)
                {
                    gcList = (from Framework.Data.Geocache a in Core.Geocaches where a.PublishedTime>DateTime.Now select a).ToList();
                }
                else if (action == ACTION_ADD_SELECTED)
                {
                    gcList = (from Framework.Data.Geocache a in Core.Geocaches where a.Selected && a.PublishedTime > DateTime.Now select a).ToList();
                }
                else if (action == ACTION_ADD_ACTIVE)
                {
                    if (Core.ActiveGeocache != null && Core.ActiveGeocache.PublishedTime > DateTime.Now)
                    {
                        gcList = new List<Framework.Data.Geocache>();
                        gcList.Add(Core.ActiveGeocache);
                    }
                }
                if (gcList == null || gcList.Count == 0)
                {
                    System.Windows.Forms.MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_NOGEOCACHESELECTED), Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                }
                else
                {
                    using (AddToCalendarForm dlg = new AddToCalendarForm(Core, gcList))
                    {
                        dlg.ShowDialog();
                    }
                }
            }
            return result;
        }
    }
}
