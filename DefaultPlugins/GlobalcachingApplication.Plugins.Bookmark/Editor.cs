using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.Bookmark
{
    public class Editor : Utils.BasePlugin.BaseUIChildWindow, Framework.Interfaces.IPluginGeocacheCollection
    {
        public const string ACTION_SHOW = "Geocache collections";

        public override bool Initialize(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_SHOW);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(EditorForm.STR_TITLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(EditorForm.STR_ADDACTIVE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(EditorForm.STR_ADDSELECTED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(EditorForm.STR_COLLECTION));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(EditorForm.STR_DELETE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(EditorForm.STR_NAME));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(EditorForm.STR_NEW));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(EditorForm.STR_REMOVEFROMCOLLECTION));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(EditorForm.STR_SELECTALL));

            return base.Initialize(core);
        }

        public override Framework.PluginType PluginType
        {
            get
            {
                return Framework.PluginType.GeocacheCollection;
            }
        }

        public override string DefaultAction
        {
            get
            {
                return ACTION_SHOW;
            }
        }

        protected override Utils.BasePlugin.BaseUIChildWindowForm CreateUIChildWindowForm(Framework.Interfaces.ICore core)
        {
            return (new EditorForm(this, core));
        }

        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result)
            {
                if (UIChildWindowForm != null)
                {
                    if (action == ACTION_SHOW)
                    {
                        if (!UIChildWindowForm.Visible)
                        {
                            (UIChildWindowForm as EditorForm).UpdateView();
                            UIChildWindowForm.Show();
                        }
                        if (UIChildWindowForm.WindowState == FormWindowState.Minimized)
                        {
                            UIChildWindowForm.WindowState = FormWindowState.Normal;
                        }
                        UIChildWindowForm.BringToFront();
                    }
                }
                result = true;
            }
            return result;
        }


        public List<string> AvailableCollections(List<string> lst)
        {
            return Repository.Instance.AvailableCollections(lst);
        }

        public void AddCollection(string name)
        {
            Repository.Instance.AddCollection(name);
        }

        public void DeleteCollection(string name)
        {
            Repository.Instance.DeleteCollection(name);
        }

        public void AddToCollection(string collectionName, string geocacheCode)
        {
            Repository.Instance.AddToCollection(collectionName, geocacheCode);
        }

        public void RemoveFromCollection(string collectionName, string geocacheCode)
        {
            Repository.Instance.RemoveFromCollection(collectionName, geocacheCode);
        }

        public bool InCollection(string collectionName, string geocacheCode)
        {
            return Repository.Instance.InCollection(collectionName, geocacheCode);
        }
    }
}
