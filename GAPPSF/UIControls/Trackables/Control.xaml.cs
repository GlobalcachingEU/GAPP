using GAPPSF.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace GAPPSF.UIControls.Trackables
{
    /// <summary>
    /// Interaction logic for Control.xaml
    /// </summary>
    public partial class Control : UserControl, IDisposable, IUIControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<TrackableGroup> AvailableTrackableGroups { get; private set; }
        public ObservableCollection<TrackableItem> AvailableTrackableItems { get; private set; }

        private TrackableGroup _selectedTrackableGroup;
        public TrackableGroup SelectedTrackableGroup
        {
            get { return _selectedTrackableGroup; }
            set
            {
                if (_selectedTrackableGroup != value)
                {
                    SetProperty(ref _selectedTrackableGroup, value);
                    IsTrackableGroupSelected = _selectedTrackableGroup != null;

                    AvailableTrackableItems.Clear();
                    if (_selectedTrackableGroup!=null)
                    {
                        List<TrackableItem> trks = Core.Settings.Default.GetTrackables(_selectedTrackableGroup);
                        foreach (var g in trks)
                        {
                            AvailableTrackableItems.Add(g);
                        }
                    }
                }
            }
        }

        private bool _isTrackableGroupSelected;
        public bool IsTrackableGroupSelected
        {
            get { return _isTrackableGroupSelected; }
            set { SetProperty(ref _isTrackableGroupSelected, value); }
        }

        private TrackableItem _selectedTrackableItem;
        public TrackableItem SelectedTrackableItem
        {
            get { return _selectedTrackableItem; }
            set
            {
                SetProperty(ref _selectedTrackableItem, value);
                IsTrackableItemSelected = _selectedTrackableItem != null;
            }
        }

        private bool _isTrackableItemSelected;
        public bool IsTrackableItemSelected
        {
            get { return _isTrackableItemSelected; }
            set { SetProperty(ref _isTrackableItemSelected, value); }
        }


        public Control()
        {
            AvailableTrackableGroups = new ObservableCollection<TrackableGroup>();
            AvailableTrackableItems = new ObservableCollection<TrackableItem>();

            try
            {
                List<TrackableGroup> grps = Core.Settings.Default.GetTrackableGroups();
                foreach(var g in grps)
                {
                    AvailableTrackableGroups.Add(g);
                }
            }
            catch(Exception e)
            {
                Core.ApplicationData.Instance.Logger.AddLog(this, e);
            }

            InitializeComponent();

            DataContext = this;
        }

        public void Dispose()
        {
        }

        public override string ToString()
        {
            return Localization.TranslationManager.Instance.Translate("TrackableGroups") as string;
        }

        protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string name = "")
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                var handler = PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs(name));
                }
            }
        }


        public int WindowWidth
        {
            get
            {
                return Core.Settings.Default.TrackableGroupWindowWidth;
            }
            set
            {
                Core.Settings.Default.TrackableGroupWindowWidth = value;
            }
        }

        public int WindowHeight
        {
            get
            {
                return Core.Settings.Default.TrackableGroupWindowHeight;
            }
            set
            {
                Core.Settings.Default.TrackableGroupWindowHeight = value;
            }
        }

        public int WindowLeft
        {
            get
            {
                return Core.Settings.Default.TrackableGroupWindowLeft;
            }
            set
            {
                Core.Settings.Default.TrackableGroupWindowLeft = value;
            }
        }

        public int WindowTop
        {
            get
            {
                return Core.Settings.Default.TrackableGroupWindowTop;
            }
            set
            {
                Core.Settings.Default.TrackableGroupWindowTop = value;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            inputDialog.Show(Localization.TranslationManager.Instance.Translate("Name").ToString());
            inputDialog.DialogClosed += newDialog_DialogClosed;
        }

        private void newDialog_DialogClosed(object sender, EventArgs e)
        {
            inputDialog.DialogClosed -= newDialog_DialogClosed;
            if (inputDialog.DialogResult)
            {
                if (!string.IsNullOrEmpty(inputDialog.InputText))
                {
                    string s = inputDialog.InputText.Trim();
                    if (s.Length > 0)
                    {
                        //add groep
                        int maxId = 0;
                        if (AvailableTrackableGroups.Count > 0)
                        {
                            maxId = AvailableTrackableGroups.Max(x => x.ID);
                        }
                        maxId++;
                        TrackableGroup tg = new TrackableGroup();
                        tg.ID = maxId;
                        tg.Name = s;
                        try
                        {
                            Core.Settings.Default.AddTrackableGroup(tg);
                            AvailableTrackableGroups.Add(tg);
                            SelectedTrackableGroup = tg;
                        }
                        catch(Exception ex)
                        {
                            Core.ApplicationData.Instance.Logger.AddLog(this, ex);
                        }
                    }
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (SelectedTrackableGroup!=null)
            {
                try
                {
                    TrackableGroup tg = SelectedTrackableGroup;
                    Core.Settings.Default.DeleteTrackableGroup(tg);
                    AvailableTrackableGroups.Remove(tg);
                }
                catch (Exception ex)
                {
                    Core.ApplicationData.Instance.Logger.AddLog(this, ex);
                }
            }
        }

        private AsyncDelegateCommand _addOwnTrackablesCommand;
        public AsyncDelegateCommand AddOwnTrackablesCommand
        {
            get
            {
                if (_addOwnTrackablesCommand==null)
                {
                    _addOwnTrackablesCommand = new AsyncDelegateCommand(param => AddOwnTrackables(),
                        param => SelectedTrackableGroup != null);
                }
                return _addOwnTrackablesCommand;
            }
        }
        public async Task AddOwnTrackables()
        {
            if (SelectedTrackableGroup != null)
            {
                var tbg = SelectedTrackableGroup;
                Import imp = new Import();
                await imp.AddOwnTrackablesAsync(SelectedTrackableGroup);
                SelectedTrackableGroup = null;
                SelectedTrackableGroup = tbg;
            }
        }

        private AsyncDelegateCommand _addNewTrackablesCommand;
        public AsyncDelegateCommand AddNewTrackablesCommand
        {
            get
            {
                if (_addNewTrackablesCommand == null)
                {
                    _addNewTrackablesCommand = new AsyncDelegateCommand(param => AddNewTrackables(),
                        param => SelectedTrackableGroup != null);
                }
                return _addNewTrackablesCommand;
            }
        }
        public async Task AddNewTrackables()
        {
            if (SelectedTrackableGroup != null && !string.IsNullOrEmpty(addTrackables.Text))
            {
                var tbg = SelectedTrackableGroup;
                string[] parts = addTrackables.Text.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
                List<string> tbcodes = (from a in parts where a.ToUpper().StartsWith("TB") select a.ToUpper()).ToList();
                Import imp = new Import();
                await imp.AddUpdateTrackablesAsync(SelectedTrackableGroup, tbcodes);
                SelectedTrackableGroup = null;
                SelectedTrackableGroup = tbg;
            }
        }


        private void ShowTrackablesOnMap(List<TrackableItem> tbs)
        {
            try
            {
                string htmlcontent = Utils.ResourceHelper.GetEmbeddedTextFile("/UIControls/Trackables/trackablesmap.html");
                StringBuilder sb = new StringBuilder();

                foreach (var tb in tbs)
                {
                    StringBuilder bln = new StringBuilder();
                    bln.AppendFormat("<a href=\"http://www.geocaching.com/track/details.aspx?tracker={0}\" target=\"_blank\">{0}</a>", tb.Code);
                    bln.AppendFormat("<br />{0}: {1}", Localization.TranslationManager.Instance.Translate("Name"), tb.Name ?? "");
                    bln.AppendFormat("<br />{0}: {1}", Localization.TranslationManager.Instance.Translate("Owner"), tb.Owner ?? "");
                    bln.AppendFormat("<br />{0}: {1}", Localization.TranslationManager.Instance.Translate("CreatedOn"), tb.DateCreated.ToLongDateString());
                    if (!string.IsNullOrEmpty(tb.CurrentGeocacheCode))
                    {
                        bln.AppendFormat("<br />{0}: <a href=\"http://coord.info/{1}\" target=\"_blank\">{1}</a>", Localization.TranslationManager.Instance.Translate("InGeocache"), tb.CurrentGeocacheCode);
                    }
                    bln.AppendFormat("<br />{0}: {1} km", Localization.TranslationManager.Instance.Translate("TravelledDistance"), tb.DistanceKm.ToString("0.0"));

                    sb.AppendFormat("createMarker('{0}', new google.maps.LatLng({1}, {2}), {3}, '{4}');", tb.Code, tb.Lat.ToString().Replace(',', '.'), tb.Lon.ToString().Replace(',', '.'), string.IsNullOrEmpty(tb.CurrentGeocacheCode) ? "redIcon" : "blueIcon", bln.ToString().Replace("'", ""));
                }

                string html = htmlcontent.Replace("//patchwork", sb.ToString());
                string fn = System.IO.Path.Combine(Core.Settings.Default.SettingsFolder, "trackablesmap.html");
                System.IO.File.WriteAllText(fn, html);
                System.Diagnostics.Process.Start(fn);
            }
            catch (Exception e)
            {
                Core.ApplicationData.Instance.Logger.AddLog(this, e);
            }
        }

        private void ShowRouteOnMap(TrackableItem tb)
        {
            if (tb != null)
            {
                try
                {
                        string htmlcontent = Utils.ResourceHelper.GetEmbeddedTextFile("/UIControls/Trackables/trackablesmap.html");
                        StringBuilder sb = new StringBuilder();
                        List<TravelItem> til = Core.Settings.Default.GetTrackableTravels(tb);
                        for (int i = 0; i < til.Count; i++)
                        {
                            StringBuilder bln = new StringBuilder();
                            bln.AppendFormat("{0}: {1}", Localization.TranslationManager.Instance.Translate("Step"), i + 1);
                            bln.AppendFormat("<br />{0}: {1}", Localization.TranslationManager.Instance.Translate("Date"), til[i].DateLogged.ToLongDateString());
                            bln.AppendFormat("<br />{0}: <a href=\"http://coord.info/{1}\" target=\"_blank\">{1}</a>", Localization.TranslationManager.Instance.Translate("Geocache"), til[i].GeocacheCode);

                            string iconColor;
                            if (i == 0)
                            {
                                iconColor = "yellowIcon";
                            }
                            else if (i == til.Count - 1)
                            {
                                iconColor = "redIcon";
                            }
                            else
                            {
                                iconColor = "blueIcon";
                            }
                            sb.AppendFormat("createMarker('{5}-{0}', new google.maps.LatLng({1}, {2}), {3}, '{4}');", til[i].GeocacheCode, til[i].Lat.ToString().Replace(',', '.'), til[i].Lon.ToString().Replace(',', '.'), iconColor, bln.ToString().Replace("'", ""), i + 1);
                        }

                        if (til.Count > 1)
                        {
                            sb.AppendLine();
                            sb.Append("var polylineA = new google.maps.Polyline({map: map, path: [");
                            for (int i = 0; i < til.Count; i++)
                            {
                                if (i > 0)
                                {
                                    sb.Append(",");
                                }
                                sb.AppendFormat("new google.maps.LatLng({0}, {1})", til[i].Lat.ToString().Replace(',', '.'), til[i].Lon.ToString().Replace(',', '.'));
                            }
                            sb.Append("], strokeColor: '#8A2BE2', strokeWeight: 4, strokeOpacity: .9});");
                        }
                        string html = htmlcontent.Replace("//patchwork", sb.ToString());
                        string fn = System.IO.Path.Combine(Core.Settings.Default.SettingsFolder, "trackablesmap.html");
                        System.IO.File.WriteAllText(fn, html);
                        System.Diagnostics.Process.Start(fn);
                }
                catch (Exception e)
                {
                    Core.ApplicationData.Instance.Logger.AddLog(this, e);
                }
            }
        }


        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (AvailableTrackableItems.Count>0)
            {
                ShowTrackablesOnMap((from TrackableItem l in AvailableTrackableItems where l.Lat != null && l.Lon != null select l).ToList());
            }
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            if (AvailableTrackableItems.Count > 0)
            {
                ShowTrackablesOnMap((from TrackableItem l in logList.SelectedItems where l.Lat != null && l.Lon != null select l).ToList());
            }
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            if (SelectedTrackableItem!=null)
            {
                ShowRouteOnMap(SelectedTrackableItem);
            }
        }


        private AsyncDelegateCommand _updateAllTrackablesCommand;
        public AsyncDelegateCommand UpdateAllTrackablesCommand
        {
            get
            {
                if (_updateAllTrackablesCommand == null)
                {
                    _updateAllTrackablesCommand = new AsyncDelegateCommand(param => UpdateAllTrackables(),
                        param => SelectedTrackableGroup != null);
                }
                return _updateAllTrackablesCommand;
            }
        }
        public async Task UpdateAllTrackables()
        {
            if (SelectedTrackableGroup != null)
            {
                await UpdateTrackables((from a in AvailableTrackableItems select a.Code). ToList());
            }
        }


        private AsyncDelegateCommand _updateSelectedTrackablesCommand;
        public AsyncDelegateCommand UpdateSelectedTrackablesCommand
        {
            get
            {
                if (_updateSelectedTrackablesCommand == null)
                {
                    _updateSelectedTrackablesCommand = new AsyncDelegateCommand(param => UpdateSelectedTrackables(),
                        param => SelectedTrackableGroup != null);
                }
                return _updateSelectedTrackablesCommand;
            }
        }
        public async Task UpdateSelectedTrackables()
        {
            if (SelectedTrackableGroup != null)
            {
                await UpdateTrackables((from TrackableItem l in logList.SelectedItems select l.Code).ToList());
            }
        }

        public async Task UpdateTrackables(List<string> trkList)
        {
            if (SelectedTrackableGroup != null)
            {
                var tbg = SelectedTrackableGroup;
                Import imp = new Import();
                await imp.AddUpdateTrackablesAsync(SelectedTrackableGroup, trkList);
                SelectedTrackableGroup = null;
                SelectedTrackableGroup = tbg;
            }
        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            //delete selected
            if (SelectedTrackableGroup != null)
            {
                List<TrackableItem> tl = (from TrackableItem l in logList.SelectedItems select l).ToList();
                foreach (var t in tl)
                {
                    Core.Settings.Default.DeleteTrackable(SelectedTrackableGroup, t);
                    AvailableTrackableItems.Remove(t);
                }
            }
        }

        private void MenuItem_Click_4(object sender, RoutedEventArgs e)
        {
            if (SelectedTrackableItem != null)
            {
                try
                {
                    System.Diagnostics.Process.Start(SelectedTrackableItem.Url);
                }
                catch (Exception ex)
                {
                    Core.ApplicationData.Instance.Logger.AddLog(this, ex);
                }
            }
        }

        private void MenuItem_Click_5(object sender, RoutedEventArgs e)
        {
            if (SelectedTrackableItem != null)
            {
                if (!string.IsNullOrEmpty(SelectedTrackableItem.CurrentGeocacheCode))
                {
                    try
                    {
                        System.Diagnostics.Process.Start(string.Format("http://coord.info/{0}", SelectedTrackableItem.CurrentGeocacheCode));
                    }
                    catch (Exception ex)
                    {
                        Core.ApplicationData.Instance.Logger.AddLog(this, ex);
                    }
                }
            }
        }

        private void MenuItem_Click_6(object sender, RoutedEventArgs e)
        {
            if (SelectedTrackableItem != null)
            {
                ShowRouteOnMap(SelectedTrackableItem);
            }
        }

        private void MenuItem_Click_7(object sender, RoutedEventArgs e)
        {
            if (SelectedTrackableItem != null)
            {
                var t = SelectedTrackableItem;
                Core.Settings.Default.DeleteTrackable(SelectedTrackableGroup, t);
                AvailableTrackableItems.Remove(t);
            }
        }


    }
}
