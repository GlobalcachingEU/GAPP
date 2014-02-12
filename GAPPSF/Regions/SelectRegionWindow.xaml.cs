using GAPPSF.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;

namespace GAPPSF.Regions
{
    /// <summary>
    /// Interaction logic for SelectRegionWindow.xaml
    /// </summary>
    public partial class SelectRegionWindow : Window
    {
        public ObservableCollection<Core.Data.AreaType> Arealevels { get; set; }
        public ObservableCollection<string> Areas { get; set; }
        private Core.Data.AreaType _selectedAreaLevel;
        public Core.Data.AreaType SelectedAreaLevel 
        {
            get { return _selectedAreaLevel; }
            set
            {
                _selectedAreaLevel = value;
                Areas.Clear();
                Areas.Add("");
                List<Core.Data.AreaInfo> ai = Shapefiles.ShapeFilesManager.Instance.GetAreasByLevel(_selectedAreaLevel);
                var uai = (from a in ai select a.Name).Distinct().OrderBy(x => x);
                foreach(var a in uai)
                {
                    Areas.Add(a);
                }
            }
        }
        public string Prefix { get; set; }
        public string SelectedArea { get; set; }
        public bool InEnvelope { get; set; }

        public SelectRegionWindow()
        {
            Areas = new ObservableCollection<string>();
            Arealevels = new ObservableCollection<Core.Data.AreaType>();
            foreach (Core.Data.AreaType s in Enum.GetValues(typeof(Core.Data.AreaType)))
            {
                Arealevels.Add(s);
            }
            SelectedAreaLevel = Core.Data.AreaType.Country;
            Prefix = "";

            DataContext = this;
            InitializeComponent();
        }

        private AsyncDelegateCommand _performActionCommand;
        public AsyncDelegateCommand PerformActionCommand
        {
            get
            {
                if (_performActionCommand == null)
                {
                    _performActionCommand = new AsyncDelegateCommand(param => AssignRegion(),
                        param => Core.ApplicationData.Instance.ActiveDatabase != null);
                }
                return _performActionCommand;
            }
        }
        public async Task AssignRegion()
        {
            if (Core.ApplicationData.Instance.ActiveDatabase != null)
            {
                List<Core.Data.Geocache> gcList = new List<Core.Data.Geocache>();
                if (selectionContext.GeocacheSelectionContext == UIControls.SelectionContext.Context.NewSelection)
                {
                    gcList = Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection;
                }
                else if (selectionContext.GeocacheSelectionContext == UIControls.SelectionContext.Context.WithinSelection)
                {
                    gcList = (from a in Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection where a.Selected select a).ToList();
                }
                else
                {
                    gcList = (from a in Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection where !a.Selected select a).ToList();
                }

                using (Utils.DataUpdater upd = new Utils.DataUpdater(Core.ApplicationData.Instance.ActiveDatabase))
                {
                    await Task.Run(() =>
                    {
                        try
                        {
                            //select the available areas
                            List<Core.Data.AreaInfo> areas;
                            if (string.IsNullOrEmpty(SelectedArea))
                            {
                                areas = Shapefiles.ShapeFilesManager.Instance.GetAreasByLevel(SelectedAreaLevel);
                            }
                            else
                            {
                                areas = Shapefiles.ShapeFilesManager.Instance.GetAreasByName(SelectedArea, SelectedAreaLevel);
                            }
                            if (areas != null && areas.Count > 0)
                            {
                                if (!string.IsNullOrEmpty(Prefix))
                                {
                                    areas = (from a in areas where a.Name.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase) select a).ToList();
                                }
                            }
                            if (areas != null && areas.Count > 0)
                            {

                                DateTime nextUpdate = DateTime.Now.AddSeconds(1);
                                using (Utils.ProgressBlock prog = new Utils.ProgressBlock("Searching", "Searching", gcList.Count, 0, true))
                                {
                                    int index = 0;
                                    foreach (var gc in gcList)
                                    {
                                        if (InEnvelope)
                                        {
                                            gc.Selected = Shapefiles.ShapeFilesManager.Instance.GetEnvelopAreasOfLocation(new Core.Data.Location(gc.Lat, gc.Lon), areas).Count > 0;
                                        }
                                        else
                                        {
                                            gc.Selected = Shapefiles.ShapeFilesManager.Instance.GetAreasOfLocation(new Core.Data.Location(gc.Lat, gc.Lon), areas).Count > 0;
                                        }

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
                            else
                            {
                                foreach (var gc in gcList)
                                {
                                    gc.Selected = false;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Core.ApplicationData.Instance.Logger.AddLog(this, e);
                        }
                    });
                }
            }
        }

    }
}
