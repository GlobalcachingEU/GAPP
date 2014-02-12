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
    /// Interaction logic for AssignRegionWindow.xaml
    /// </summary>
    public partial class AssignRegionWindow : Window
    {
        public ObservableCollection<Core.Data.AreaType> Arealevels { get; set; }
        public Core.Data.AreaType SelectedAreaLevel { get; set; }
        public string Prefix { get; set; }
        public bool UnassignedOnly { get; set; }
        public bool SelectedOnly { get; set; }

        public AssignRegionWindow()
        {
            Arealevels = new ObservableCollection<Core.Data.AreaType>();
            foreach (Core.Data.AreaType s in Enum.GetValues(typeof(Core.Data.AreaType)))
            {
                if (s != Core.Data.AreaType.Other)
                {
                    Arealevels.Add(s);
                }
            }
            SelectedAreaLevel = Core.Data.AreaType.Country;
            UnassignedOnly = true;
            Prefix = "";
            SelectedOnly = Core.ApplicationData.Instance.MainWindow.GeocacheSelectionCount > 0;

            DataContext = this;

            InitializeComponent();
        }

        private AsyncDelegateCommand _performActionCommand;
        public AsyncDelegateCommand PerformActionCommand
        {
            get
            {
                if (_performActionCommand==null)
                {
                    _performActionCommand = new AsyncDelegateCommand(param => AssignRegion(),
                        param => Core.ApplicationData.Instance.ActiveDatabase!=null);
                }
                return _performActionCommand;
            }
        }
        public async Task AssignRegion()
        {
            if (Core.ApplicationData.Instance.ActiveDatabase!=null)
            {
                List<Core.Data.Geocache> gcList = new List<Core.Data.Geocache>();

                using (Utils.DataUpdater upd = new Utils.DataUpdater(Core.ApplicationData.Instance.ActiveDatabase))
                {
                    await Task.Run(() => {
                        try
                        {
                            switch (SelectedAreaLevel)
                            {
                                case Core.Data.AreaType.Country:
                                    gcList = (from g in Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection where (!UnassignedOnly || string.IsNullOrEmpty(g.Country)) && (g.Selected || !SelectedOnly) select g).ToList();
                                    break;
                                case Core.Data.AreaType.State:
                                    gcList = (from g in Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection where (!UnassignedOnly || string.IsNullOrEmpty(g.State)) && (g.Selected || !SelectedOnly) select g).ToList();
                                    break;
                                case Core.Data.AreaType.Municipality:
                                    gcList = (from g in Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection where (!UnassignedOnly || string.IsNullOrEmpty(g.Municipality)) && (g.Selected || !SelectedOnly) select g).ToList();
                                    break;
                                case Core.Data.AreaType.City:
                                    gcList = (from g in Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection where (!UnassignedOnly || string.IsNullOrEmpty(g.City)) && (g.Selected || !SelectedOnly) select g).ToList();
                                    break;
                            }

                            DateTime nextUpdate = DateTime.Now.AddSeconds(1);
                            using (Utils.ProgressBlock prog = new Utils.ProgressBlock("AssignRegionsToGeocaches", "AssignRegionsToGeocaches", gcList.Count, 0, true))
                            {
                                List<Core.Data.AreaInfo> areasFilter = Shapefiles.ShapeFilesManager.Instance.GetAreasByLevel(SelectedAreaLevel);
                                if (areasFilter != null && areasFilter.Count > 0)
                                {
                                    int index = 0;
                                    foreach (var gc in gcList)
                                    {
                                        List<Core.Data.AreaInfo> areas = Shapefiles.ShapeFilesManager.Instance.GetAreasOfLocation(new Core.Data.Location(gc.Lat, gc.Lon), areasFilter);
                                        if (areas != null && areas.Count > 0)
                                        {
                                            Core.Data.AreaInfo ai = areas[0];
                                            if (Prefix.Length > 0)
                                            {
                                                ai = (from g in areas where g.Name.StartsWith(Prefix) select g).FirstOrDefault();
                                            }
                                            if (ai != null)
                                            {
                                                switch (SelectedAreaLevel)
                                                {
                                                    case Core.Data.AreaType.Country:
                                                        gc.Country = ai.Name;
                                                        break;
                                                    case Core.Data.AreaType.State:
                                                        gc.State = ai.Name;
                                                        break;
                                                    case Core.Data.AreaType.Municipality:
                                                        gc.Municipality = ai.Name;
                                                        break;
                                                    case Core.Data.AreaType.City:
                                                        gc.City = ai.Name;
                                                        break;
                                                }
                                            }
                                        }
                                        index++;
                                        if (DateTime.Now>=nextUpdate)
                                        {
                                            if (!prog.Update("AssignRegionsToGeocaches", gcList.Count, index))
                                            {
                                                break;
                                            }
                                            nextUpdate = DateTime.Now.AddSeconds(1);
                                        }
                                    }
                                }
                            }
                        }
                        catch(Exception e)
                        {
                            Core.ApplicationData.Instance.Logger.AddLog(this, e);
                        }
                    });
                }
            }
        }
    }
}
