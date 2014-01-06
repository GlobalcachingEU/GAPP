using GAPPSF.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GAPPSF.UIControls.GeocacheFilter
{
    /// <summary>
    /// Interaction logic for Control.xaml
    /// </summary>
    public partial class Control : UserControl, IUIControl
    {
        public Control()
        {
            InitializeComponent();

            if (!string.IsNullOrEmpty(Core.Settings.Default.GeocacheFilterGeocacheTypes))
            {
                string[] parts = Core.Settings.Default.GeocacheFilterGeocacheTypes.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string s in parts)
                {
                    int id = int.Parse(s);

                    var c = (from a in geocacheTypes.AvailableTypes where a.Item.ID == id select a).FirstOrDefault();
                    if (c!=null)
                    {
                        c.IsChecked = true;
                    }
                }
            }
            foreach (var c in geocacheTypes.AvailableTypes)
            {
                c.PropertyChanged += c_PropertyChanged;
            }

            if (!string.IsNullOrEmpty(Core.Settings.Default.GeocacheFilterGeocacheContainers))
            {
                string[] parts = Core.Settings.Default.GeocacheFilterGeocacheContainers.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string s in parts)
                {
                    int id = int.Parse(s);

                    var c = (from a in geocacheContainers.AvailableTypes where a.Item.ID == id select a).FirstOrDefault();
                    if (c != null)
                    {
                        c.IsChecked = true;
                    }
                }
            }
            foreach (var c in geocacheContainers.AvailableTypes)
            {
                c.PropertyChanged += con_PropertyChanged;
            }

            if (!string.IsNullOrEmpty(Core.Settings.Default.GeocacheFilterGeocacheAttributes))
            {
                string[] parts = Core.Settings.Default.GeocacheFilterGeocacheAttributes.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string s in parts)
                {
                    int id = int.Parse(s);

                    var c = (from a in geocacheAttributes.AvailableTypes where a.Item.ID == id select a).FirstOrDefault();
                    if (c != null)
                    {
                        c.IsChecked = true;
                    }
                }
            }
            foreach (var c in geocacheAttributes.AvailableTypes)
            {
                c.PropertyChanged += attr_PropertyChanged;
            }


            DataContext = this;
        }

        private void attr_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            var sl = from a in geocacheAttributes.AvailableTypes where a.IsChecked select a;
            foreach (var c in sl)
            {
                sb.AppendFormat("|{0}", c.Item.ID);
            }
            Core.Settings.Default.GeocacheFilterGeocacheAttributes = sb.ToString();
        }

        void con_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            var sl = from a in geocacheContainers.AvailableTypes where a.IsChecked select a;
            foreach (var c in sl)
            {
                sb.AppendFormat("|{0}", c.Item.ID);
            }
            Core.Settings.Default.GeocacheFilterGeocacheContainers = sb.ToString();
        }

        void c_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            var sl = from a in geocacheTypes.AvailableTypes where a.IsChecked select a;
            foreach(var c in sl)
            {
                sb.AppendFormat("|{0}", c.Item.ID);
            }
            Core.Settings.Default.GeocacheFilterGeocacheTypes = sb.ToString();
        }

        public override string ToString()
        {
            return Localization.TranslationManager.Instance.Translate("GeocacheFilter") as string;
        }


        AsyncDelegateCommand _selectCommand;
        public ICommand SelectCommand
        {
            get
            {
                if (_selectCommand == null)
                {
                    _selectCommand = new AsyncDelegateCommand(param => this.SelectGeocaches(), param => Core.ApplicationData.Instance.ActiveDatabase!=null);
                }
                return _selectCommand;
            }
        }

        private bool foundByAny(Core.Data.Geocache gc, string[] users)
        {
            bool result = false;
            List<Core.Data.Log> lgs = (from a in Utils.DataAccess.GetLogs(gc.Database, gc.Code) where a.LogType.AsFound select a).ToList();
            //foreach (string usr in users)
            //{
            //    if ( (from a in lgs where string.Compare(a.Finder, usr, true)==0 select a).FirstOrDefault() != null)
            //    {
            //        result = true;
            //        break;
            //    }
            //}
            foreach(var l in lgs)
            {
                string f = l.Finder;
                if ((from a in users where string.Compare(a, f, true)==0 select a).FirstOrDefault()!=null)
                {
                    result = true;
                    break;
                }
            }
            return result;
        }
        private bool foundByNone(Core.Data.Geocache gc, string[] users)
        {
            bool result = true;
            List<Core.Data.Log> lgs = (from a in Utils.DataAccess.GetLogs(gc.Database, gc.Code) where a.LogType.AsFound select a).ToList();
            //foreach (string usr in users)
            //{
            //    if ((from a in lgs where string.Compare(a.Finder, usr, true) == 0 select a).FirstOrDefault() != null)
            //    {
            //        result = false;
            //        break;
            //    }
            //}
            foreach (var l in lgs)
            {
                string f = l.Finder;
                if ((from a in users where string.Compare(a, f, true) == 0 select a).FirstOrDefault() != null)
                {
                    result = false;
                    break;
                }
            }
            return result;
        }
        private bool foundByAll(Core.Data.Geocache gc, string[] users)
        {
            bool result = true;
            List<Core.Data.Log> lgs = (from a in Utils.DataAccess.GetLogs(gc.Database, gc.Code) where a.LogType.AsFound select a).ToList();
            //foreach (string usr in users)
            //{
            //    if ((from a in lgs where string.Compare(a.Finder, usr, true) == 0 select a).FirstOrDefault() == null)
            //    {
            //        result = false;
            //        break;
            //    }
            //}
            foreach (var l in lgs)
            {
                string f = l.Finder;
                if ((from a in users where string.Compare(a, f, true) == 0 select a).FirstOrDefault() != null)
                {
                    result = false;
                    break;
                }
            }
            return result;
        }

        private bool attrFilterPass(List<int> attrFilter, Core.Data.Geocache gc)
        {
            bool result = false;
            List<int> gcAttr = gc.AttributeIds;
            switch(Core.Settings.Default.GeocacheFilterAttributeFilter)
            {
                case AttributeFilter.ContainsAll:
                    result = (from a in gcAttr join b in attrFilter on a equals b select a).Count() == attrFilter.Count;
                    break;
                case AttributeFilter.ContainsAtLeastOne:
                    result = (from a in gcAttr join b in attrFilter on a equals b select a).Count() > 0;
                    break;
                case AttributeFilter.ContainsNone:
                    result = (from a in gcAttr join b in attrFilter on a equals b select a).Count() == 0;
                    break;
                default:
                    result = false;
                    break;
            }

            return result;
        }

        async public Task SelectGeocaches()
        {
            if (Core.ApplicationData.Instance.ActiveDatabase != null)
            {
                using (Utils.DataUpdater upd = new Utils.DataUpdater(Core.ApplicationData.Instance.ActiveDatabase))
                {
                    await Task.Run(() =>
                    {
                        List<Core.Data.Geocache> gcList;
                        if (selectionContext.GeocacheSelectionContext == SelectionContext.Context.NewSelection)
                        {
                            gcList = Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection;
                        }
                        else if (selectionContext.GeocacheSelectionContext == SelectionContext.Context.WithinSelection)
                        {
                            gcList = (from a in Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection where a.Selected select a).ToList();
                        }
                        else
                        {
                            gcList = (from a in Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection where !a.Selected select a).ToList();
                        }

                        DateTime nextUpdate = DateTime.Now.AddSeconds(1);
                        using (Utils.ProgressBlock prog = new Utils.ProgressBlock("Searching", "Searching", gcList.Count, 0, true))
                        {
                            string[] foundByUsers = Core.Settings.Default.GeocacheFilterFoundBy == null ? new string[] { } : Core.Settings.Default.GeocacheFilterFoundBy.Split(',');
                            for (int i = 0; i < foundByUsers.Length; i++)
                            {
                                foundByUsers[i] = foundByUsers[i].Trim();
                            }
                            string[] notFoundByUsers = Core.Settings.Default.GeocacheFilterNotFoundBy == null ? new string[] { } : Core.Settings.Default.GeocacheFilterNotFoundBy.Split(',');
                            for (int i = 0; i < notFoundByUsers.Length; i++)
                            {
                                notFoundByUsers[i] = notFoundByUsers[i].Trim();
                            }

                            Core.Data.Location loc = null;
                            double dist = 0;
                            if (Core.Settings.Default.GeocacheFilterLocationExpanded)
                            {
                                loc = getLocation();
                                dist = Core.Settings.Default.GeocacheFilterLocationRadius * 1000;
                                if (Core.Settings.Default.GeocacheFilterLocationKm== BooleanEnum.False)
                                {
                                    dist /= 0.621371192237;
                                }
                            }
                            List<int> cacheTypes = new List<int>();
                            if (Core.Settings.Default.GeocacheFilterGeocacheTypesExpanded)
                            {
                                if (!string.IsNullOrEmpty(Core.Settings.Default.GeocacheFilterGeocacheTypes))
                                {
                                    string[] parts = Core.Settings.Default.GeocacheFilterGeocacheTypes.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                                    foreach(string s in parts)
                                    {
                                        cacheTypes.Add(int.Parse(s));
                                    }
                                }
                            }
                            List<int> cacheContainers = new List<int>();
                            if (Core.Settings.Default.GeocacheFilterGeocacheContainersExpanded)
                            {
                                if (!string.IsNullOrEmpty(Core.Settings.Default.GeocacheFilterGeocacheContainers))
                                {
                                    string[] parts = Core.Settings.Default.GeocacheFilterGeocacheContainers.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                                    foreach (string s in parts)
                                    {
                                        cacheContainers.Add(int.Parse(s));
                                    }
                                }
                            }
                            List<int> cacheAttributes = new List<int>();
                            if (Core.Settings.Default.GeocacheFilterGeocacheAttributesExpanded)
                            {
                                if (!string.IsNullOrEmpty(Core.Settings.Default.GeocacheFilterGeocacheAttributes))
                                {
                                    string[] parts = Core.Settings.Default.GeocacheFilterGeocacheAttributes.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                                    foreach (string s in parts)
                                    {
                                        cacheAttributes.Add(int.Parse(s));
                                    }
                                }
                            }

                            if (loc != null || !Core.Settings.Default.GeocacheFilterLocationExpanded)
                            {
                                //set selected within gcList
                                int index = 0;
                                foreach (var gc in gcList)
                                {
                                    gc.Selected = (
                                        (!Core.Settings.Default.GeocacheFilterStatusExpanded || ((Core.Settings.Default.GeocacheFilterGeocacheStatus == GeocacheStatus.Enabled && gc.Available) ||
                                                                                                 (Core.Settings.Default.GeocacheFilterGeocacheStatus == GeocacheStatus.Disabled && !gc.Available && !gc.Archived) ||
                                                                                                 (Core.Settings.Default.GeocacheFilterGeocacheStatus == GeocacheStatus.Archived && gc.Archived))) &&
                                        (!Core.Settings.Default.GeocacheFilterOwnExpanded || ((Core.Settings.Default.GeocacheFilterOwn == BooleanEnum.True) == gc.IsOwn)) &&
                                        (!Core.Settings.Default.GeocacheFilterFoundExpanded || ((Core.Settings.Default.GeocacheFilterFound == BooleanEnum.True) == gc.Found)) &&
                                        (!Core.Settings.Default.GeocacheFilterFoundByExpanded || ((Core.Settings.Default.GeocacheFilterFoundByAll == BooleanEnum.True) && foundByAll(gc, foundByUsers)) ||
                                                                                                 ((Core.Settings.Default.GeocacheFilterFoundByAll == BooleanEnum.False) && foundByAny(gc, foundByUsers))) &&
                                        (!Core.Settings.Default.GeocacheFilterNotFoundByExpanded || ((Core.Settings.Default.GeocacheFilterNotFoundByAny == BooleanEnum.True) && !foundByAny(gc, foundByUsers)) ||
                                                                                                 ((Core.Settings.Default.GeocacheFilterNotFoundByAny == BooleanEnum.False) && !foundByAll(gc, foundByUsers))) &&
                                        (!Core.Settings.Default.GeocacheFilterLocationExpanded || (Utils.Calculus.CalculateDistance(gc, loc).EllipsoidalDistance<=dist)) &&
                                        (!Core.Settings.Default.GeocacheFilterCountryStateExpanded || (string.IsNullOrEmpty(Core.Settings.Default.GeocacheFilterCountry) || (!string.IsNullOrEmpty(Core.Settings.Default.GeocacheFilterCountry) && string.Compare(gc.Country,Core.Settings.Default.GeocacheFilterCountry, true)==0)) &&
                                                                                                      (string.IsNullOrEmpty(Core.Settings.Default.GeocacheFilterState) || (!string.IsNullOrEmpty(Core.Settings.Default.GeocacheFilterState) && string.Compare(gc.State, Core.Settings.Default.GeocacheFilterState, true) == 0))) &&
                                        (!Core.Settings.Default.GeocacheFilterMunicipalityCityExpanded || (string.IsNullOrEmpty(Core.Settings.Default.GeocacheFilterMunicipality) || (!string.IsNullOrEmpty(Core.Settings.Default.GeocacheFilterMunicipality) && string.Compare(gc.Municipality, Core.Settings.Default.GeocacheFilterMunicipality, true) == 0)) &&
                                                                                                      (string.IsNullOrEmpty(Core.Settings.Default.GeocacheFilterCity) || (!string.IsNullOrEmpty(Core.Settings.Default.GeocacheFilterCity) && string.Compare(gc.City, Core.Settings.Default.GeocacheFilterCity, true) == 0))) &&
                                        (!Core.Settings.Default.GeocacheFilterGeocacheTypesExpanded || (cacheTypes.Contains(gc.GeocacheType.ID))) &&
                                        (!Core.Settings.Default.GeocacheFilterGeocacheContainersExpanded || (cacheContainers.Contains(gc.Container.ID))) &&
                                        (!Core.Settings.Default.GeocacheFilterFavExpanded || (gc.Favorites >= Core.Settings.Default.GeocacheFilterMinFav && gc.Favorites <= Core.Settings.Default.GeocacheFilterMaxFav)) &&
                                        (!Core.Settings.Default.GeocacheFilterTerrainExpanded || ((gc.Terrain >= Core.Settings.Default.GeocacheFilterMinTerrain) && (gc.Terrain <= Core.Settings.Default.GeocacheFilterMaxTerrain))) &&
                                        (!Core.Settings.Default.GeocacheFilterDifficultyExpanded || ((gc.Difficulty >= Core.Settings.Default.GeocacheFilterMinDifficulty) && (gc.Difficulty <= Core.Settings.Default.GeocacheFilterMaxDifficulty))) &&
                                        (!Core.Settings.Default.GeocacheFilterHiddenDateExpanded || (gc.PublishedTime.Date >= Core.Settings.Default.GeocacheFilterMinHiddenDate.Date && gc.PublishedTime.Date <= Core.Settings.Default.GeocacheFilterMaxHiddenDate.Date)) &&
                                        (!Core.Settings.Default.GeocacheFilterGeocacheAttributesExpanded || attrFilterPass(cacheAttributes, gc))
                                        );

                                    index++;
                                    if (DateTime.Now >= nextUpdate)
                                    {
                                        if (!prog.Update("Searching", gcList.Count, index))
                                        {
                                            break;
                                        }
                                        nextUpdate = DateTime.Now.AddSeconds(1);
                                    }
                                }
                            }
                        }
                    });
                }
            }
        }


        public int WindowWidth
        {
            get
            {
                return Core.Settings.Default.GeocacheFilterWindowWidth;
            }
            set
            {
                Core.Settings.Default.GeocacheFilterWindowWidth = value;
            }
        }

        public int WindowHeight
        {
            get
            {
                return Core.Settings.Default.GeocacheFilterWindowHeight;
            }
            set
            {
                Core.Settings.Default.GeocacheFilterWindowHeight = value;
            }
        }

        public int WindowLeft
        {
            get
            {
                return Core.Settings.Default.GeocacheFilterWindowLeft;
            }
            set
            {
                Core.Settings.Default.GeocacheFilterWindowLeft = value;
            }
        }

        public int WindowTop
        {
            get
            {
                return Core.Settings.Default.GeocacheFilterWindowTop;
            }
            set
            {
                Core.Settings.Default.GeocacheFilterWindowTop = value;
            }
        }

        private  Core.Data.Location getLocation()
        {
            Core.Data.Location result = Utils.Conversion.StringToLocation(Core.Settings.Default.GeocacheFilterLocation);
            if (result==null)
            {
                //address?
                result = Utils.Geocoder.GetLocationOfAddress(Core.Settings.Default.GeocacheFilterLocation);
            }
            return result;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Core.Data.Location l = getLocation();
            if (l==null)
            {
                l = Core.ApplicationData.Instance.CenterLocation;
            }
            Dialogs.GetLocationWindow dlg = new Dialogs.GetLocationWindow(l);
            if (dlg.ShowDialog()==true)
            {
                Core.Settings.Default.GeocacheFilterLocation = dlg.Location.ToString();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Core.Data.Location l = getLocation();
            if (l == null)
            {
                Core.Settings.Default.GeocacheFilterLocation = "";
            }
            else
            {
                Core.Settings.Default.GeocacheFilterLocation = l.ToString();
            }
        }

    }
}
