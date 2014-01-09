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

namespace GAPPSF.UIControls.LogViewer
{
    /// <summary>
    /// Interaction logic for Control.xaml
    /// </summary>
    public partial class Control : UserControl, IDisposable, IUIControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<Core.Data.Log> AvailableLogs { get; private set; }

        public Control()
        {
            InitializeComponent();

            AvailableLogs = new ObservableCollection<Core.Data.Log>();
            DataContext = this;

            Dispatcher.BeginInvoke(new Action(async () =>
            {
                await UpdateView();
            }));


            Core.ApplicationData.Instance.PropertyChanged += Instance_PropertyChanged;
            Core.Settings.Default.PropertyChanged += Default_PropertyChanged;
        }

        async void Default_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "LogViewerCurrentGeocacheOnly" || e.PropertyName == "LogViewerFilterOnFinder")
            {
                await UpdateView();
            }
        }

        async void Instance_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName=="ActiveDatabase" && !Core.Settings.Default.LogViewerCurrentGeocacheOnly)
            {
                await UpdateView();
            }
            else if (e.PropertyName == "ActiveGeocache" && Core.Settings.Default.LogViewerCurrentGeocacheOnly)
            {
                await UpdateView();
            }
        }

        public void Dispose()
        {
            Core.Settings.Default.PropertyChanged -= Default_PropertyChanged;
            Core.ApplicationData.Instance.PropertyChanged -= Instance_PropertyChanged;
        }

        private async Task UpdateView()
        {
            Core.ApplicationData.Instance.BeginActiviy();
            AvailableLogs.Clear();
            List<Core.Data.Log> lgs = null;
            if (Core.ApplicationData.Instance.ActiveGeocache!=null && Core.Settings.Default.LogViewerCurrentGeocacheOnly)
            {
                lgs = Utils.DataAccess.GetLogs(Core.ApplicationData.Instance.ActiveGeocache.Database, Core.ApplicationData.Instance.ActiveGeocache.Code);
                if (!string.IsNullOrEmpty(Core.Settings.Default.LogViewerFilterOnFinder))
                {
                    await Task.Run(() =>
                    {
                        int i = 0;
                        string s = Core.Settings.Default.LogViewerFilterOnFinder;
                        while (i<lgs.Count)
                        {
                            if (string.Compare(lgs[i].Finder, s, true)!=0)
                            {
                                lgs.RemoveAt(i);
                            }
                            else
                            {
                                i++;
                            }
                        }
                    });
                }
            }
            else if (Core.ApplicationData.Instance.ActiveDatabase != null && !Core.Settings.Default.LogViewerCurrentGeocacheOnly)
            {
                if (!string.IsNullOrEmpty(Core.Settings.Default.LogViewerFilterOnFinder))
                {
                    lgs = new List<Core.Data.Log>();
                    await Task.Run(() =>
                    {
                        DateTime nextUpdate = DateTime.Now.AddSeconds(1);
                        int index = 0;
                        using (Utils.ProgressBlock prog = new Utils.ProgressBlock("LoadingLogs","LoadingLogs",Core.ApplicationData.Instance.ActiveDatabase.LogCollection.Count,0,true))
                        {
                            string s = Core.Settings.Default.LogViewerFilterOnFinder;
                            foreach(var l in Core.ApplicationData.Instance.ActiveDatabase.LogCollection)
                            {
                                if (string.Compare(l.Finder, s, true) == 0)
                                {
                                    lgs.Add(l);
                                }
                                index++;
                                if (DateTime.Now>=nextUpdate)
                                {
                                    if (!prog.Update("LoadingLogs", Core.ApplicationData.Instance.ActiveDatabase.LogCollection.Count, index))
                                    {
                                        break;
                                    }
                                    nextUpdate = DateTime.Now.AddSeconds(1);
                                }
                            }
                        }
                    });
                }
            }
            if (lgs != null)
            {
                foreach (var l in lgs)
                {
                    AvailableLogs.Add(l);
                }
            }
            Core.ApplicationData.Instance.EndActiviy();
        }

        public override string ToString()
        {
            return Localization.TranslationManager.Instance.Translate("LogViewer") as string;
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
                return Core.Settings.Default.LogViewerWindowWidth;
            }
            set
            {
                Core.Settings.Default.LogViewerWindowWidth = value;
            }
        }

        public int WindowHeight
        {
            get
            {
                return Core.Settings.Default.LogViewerWindowHeight;
            }
            set
            {
                Core.Settings.Default.LogViewerWindowHeight = value;
            }
        }

        public int WindowLeft
        {
            get
            {
                return Core.Settings.Default.LogViewerWindowLeft;
            }
            set
            {
                Core.Settings.Default.LogViewerWindowLeft = value;
            }
        }

        public int WindowTop
        {
            get
            {
                return Core.Settings.Default.LogViewerWindowTop;
            }
            set
            {
                Core.Settings.Default.LogViewerWindowTop = value;
            }
        }

    }
}
