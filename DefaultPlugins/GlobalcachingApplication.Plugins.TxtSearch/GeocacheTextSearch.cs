using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.TxtSearch
{
    public class GeocacheTextSearch: Utils.BasePlugin.Plugin
    {
        public const string ACTION_SEARCH = "Search geocache by text";

        public const string STR_ACTIONTITLE = "Searching for text...";
        public const string STR_ACTIONTEXT = "Searching within description...";

        private ManualResetEvent _actionReady = null;
        private List<Framework.Data.Geocache> _gcSearchList;
        private StringComparison _scType;
        private string _txt;

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
                return ACTION_SEARCH;
            }
        }

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_SEARCH);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheTextSearchForm.STR_GEOCACHESEARCH));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheTextSearchForm.STR_SELECTION));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheTextSearchForm.STR_FIND));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheTextSearchForm.STR_FIELD));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheTextSearchForm.STR_ADDTOSELECTION));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheTextSearchForm.STR_SEARCHWITHINSELECTION));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheTextSearchForm.STR_NEWSEARCH));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheTextSearchForm.STR_NAME));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheTextSearchForm.STR_CODE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheTextSearchForm.STR_DESCRIPTION));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheTextSearchForm.STR_OWNER));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(GeocacheTextSearchForm.STR_CASESENSATIVE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ACTIONTITLE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ACTIONTEXT));

            return await base.InitializeAsync(core);
        }


        private void searchInDescriptionThreadMethod()
        {
            using (Utils.ProgressBlock progress = new Utils.ProgressBlock(this, STR_ACTIONTITLE, STR_ACTIONTEXT, _gcSearchList.Count, 0, true))
            {
                try
                {
                    int block = 0;
                    int index = 0;
                    foreach (Framework.Data.Geocache wp in _gcSearchList)
                    {
                        wp.Selected = (wp.ShortDescription.IndexOf(_txt, _scType) >= 0 || wp.LongDescription.IndexOf(_txt, _scType) >= 0);
                        block++;
                        index++;
                        if (block > 1000)
                        {
                            block = 0;
                            if (!progress.UpdateProgress(STR_ACTIONTITLE, STR_ACTIONTEXT, _gcSearchList.Count, index))
                            {
                                break;
                            }
                        }
                    }
                }
                catch
                {
                }
                //signal finished
            }
            _actionReady.Set();
        }

        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result)
            {
                using (GeocacheTextSearchForm dlg = new GeocacheTextSearchForm())
                {
                    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        Core.Geocaches.BeginUpdate();

                        if (dlg.SearchOption.Selection == GeocacheTextSearchForm.DialogContent.SelectionSelect.NewSearch)
                        {
                            //reset current
                            foreach (Framework.Data.Geocache gc in Core.Geocaches)
                            {
                                gc.Selected = false;
                            }
                        }

                        StringComparison scType;

                        if (dlg.SearchOption.CaseSensative) scType = StringComparison.CurrentCultureIgnoreCase;
                        else scType = StringComparison.OrdinalIgnoreCase;

                        if (dlg.SearchOption.Field == GeocacheTextSearchForm.DialogContent.FieldSelect.Description)
                        {
                            if (dlg.SearchOption.Selection == GeocacheTextSearchForm.DialogContent.SelectionSelect.WithinSelection)
                            {
                                _gcSearchList = Utils.DataAccess.GetSelectedGeocaches(Core.Geocaches);
                            }
                            else if (dlg.SearchOption.Selection == GeocacheTextSearchForm.DialogContent.SelectionSelect.AddToSelection)
                            {
                                _gcSearchList = (from Framework.Data.Geocache gc in Core.Geocaches where !gc.Selected select gc).ToList();
                            }
                            else
                            {
                                _gcSearchList = (from Framework.Data.Geocache gc in Core.Geocaches select gc).ToList();
                            }
                            _scType = scType;
                            _txt = dlg.SearchOption.Text;

                            _actionReady = new ManualResetEvent(false);
                            Thread thrd = new Thread(new ThreadStart(this.searchInDescriptionThreadMethod));
                            thrd.Start();
                            while (!_actionReady.WaitOne(100))
                            {
                                System.Windows.Forms.Application.DoEvents();
                            }
                            thrd.Join();

                            if (dlg.SearchOption.Selection == GeocacheTextSearchForm.DialogContent.SelectionSelect.WithinSelection)
                            {
                                //reset current
                                foreach (Framework.Data.Geocache gc in Core.Geocaches)
                                {
                                    gc.Selected = false;
                                }
                            }

                        }
                        else
                        {
                            var gcList = (from Framework.Data.Geocache wp in Core.Geocaches
                                         where ((dlg.SearchOption.Selection != GeocacheTextSearchForm.DialogContent.SelectionSelect.WithinSelection) || wp.Selected)
                                                && ((dlg.SearchOption.Field != GeocacheTextSearchForm.DialogContent.FieldSelect.Name || wp.Name.IndexOf(dlg.SearchOption.Text, scType) >= 0)
                                                && (dlg.SearchOption.Field != GeocacheTextSearchForm.DialogContent.FieldSelect.Code || wp.Code.IndexOf(dlg.SearchOption.Text, scType) >= 0)
                                                && (dlg.SearchOption.Field != GeocacheTextSearchForm.DialogContent.FieldSelect.Owner || wp.Owner.IndexOf(dlg.SearchOption.Text, scType) >= 0))
                                         select wp).ToList();

                            if (dlg.SearchOption.Selection == GeocacheTextSearchForm.DialogContent.SelectionSelect.WithinSelection)
                            {
                                //reset current
                                foreach (Framework.Data.Geocache gc in Core.Geocaches)
                                {
                                    gc.Selected = false;
                                }
                            }

                            foreach (Framework.Data.Geocache gc in gcList)
                            {
                                gc.Selected = true;
                            }
                        }


                        Core.Geocaches.EndUpdate();
                    }
                }
            }
            return result;
        }
    }
}
