using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.QryBuilder
{
    public class QueryBuilder : Utils.BasePlugin.BaseUIChildWindow
    {
        public const string ACTION_SHOW = "Query builder|Editor";
        public const string ACTION_SEP = "Query builder|-";

        public override bool Initialize(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_SHOW);
            AddAction(ACTION_SEP);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(QueryBuilderForm.STR_TITLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(QueryBuilderForm.STR_DELETE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(QueryBuilderForm.STR_EXECUTE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(QueryBuilderForm.STR_EXPLAIN));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(QueryBuilderForm.STR_NEW));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(QueryBuilderForm.STR_QUERY));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(QueryBuilderForm.STR_RENAME));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(QueryBuilderForm.STR_SAVE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(QueryBuilderForm.STR_ERROR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(QueryBuilderForm.STR_CIRCULARREFRENCE));

            core.LanguageItems.Add(new Framework.Data.LanguageItem(QueryOperator.STR_AND));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(QueryOperator.STR_END));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(QueryOperator.STR_OR));

            QueryExpressionImplementation qei;
            qei = new QECountry(core);
            qei = new QEState(core);
            qei = new QEMunicipality(core);
            qei = new QECity(core);
            qei = new QECode(core);
            qei = new QEPublished(core);
            qei = new QEDataFromDate(core);
            qei = new QELat(core);
            qei = new QELon(core);
            qei = new QEDistanceToCenter(core);
            qei = new QEAvailable(core);
            qei = new QEArchived(core);
            qei = new QEGeocacheType(core);
            qei = new QEPlacedBy(core);
            qei = new QEOwner(core);
            qei = new QEContainer(core);
            qei = new QETerrain(core);
            qei = new QEDifficulty(core);
            qei = new QEMemberOnly(core);
            qei = new QECustomCoords(core);
            qei = new QEFavorites(core);
            qei = new QEFlagged(core);
            qei = new QEFound(core);
            qei = new QELocked(core);
            qei = new QEQuery(core);

            return base.Initialize(core);
        }

        protected override Utils.BasePlugin.BaseUIChildWindowForm CreateUIChildWindowForm(Framework.Interfaces.ICore core)
        {
            return (new QueryBuilderForm(this, core));
        }

        public override Framework.PluginType PluginType
        {
            get
            {
                return Framework.PluginType.GeocacheSelectFilter;
            }
        }

        public override string DefaultAction
        {
            get
            {
                return ACTION_SHOW;
            }
        }

        public override void ApplicationInitialized()
        {
            base.ApplicationInitialized();
            if (UIChildWindowForm != null)
            {
                (UIChildWindowForm as QueryBuilderForm).ApplicationInitialized();
            }
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
                            UIChildWindowForm.Show();
                        }
                        if (UIChildWindowForm.WindowState == FormWindowState.Minimized)
                        {
                            UIChildWindowForm.WindowState = FormWindowState.Normal;
                        }
                        UIChildWindowForm.BringToFront();
                    }
                    else
                    {
                        (UIChildWindowForm as QueryBuilderForm).ExecuteQuery(action.Substring(action.IndexOf('|') + 1));
                    }
                }
                result = true;
            }
            return result;
        }

    }
}
