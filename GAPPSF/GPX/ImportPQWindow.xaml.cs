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

namespace GAPPSF.GPX
{
    /// <summary>
    /// Interaction logic for ImportPQWindow.xaml
    /// </summary>
    public partial class ImportPQWindow : Window
    {
        public class PQData
        {
            public LiveAPI.LiveV6.PQData LiveAPIData { get; private set; }

            public PQData(LiveAPI.LiveV6.PQData _liveAPIData)
            {
                LiveAPIData = _liveAPIData;
            }

            public string Name { get { return LiveAPIData.Name; } }
            public string Generated { get { return LiveAPIData.DateLastGenerated.ToShortDateString(); } }
            public bool Processed { get { return false; } }
            public int PQCount { get { return LiveAPIData.PQCount; } }
            public bool Downloadable { get { return LiveAPIData.IsDownloadAvailable; } }
            public string PQType { get { return LiveAPIData.PQSearchType.ToString(); } }
        }

        public class DownloadedPQInfo
        {
            public Guid Guid { get; set; }
            public DateTime DownloadedAt { get; set; }
        }

        public ObservableCollection<PQData> PQDataCollection { get; private set; }
        private List<DownloadedPQInfo> _downloadedPqs;

        public ImportPQWindow()
        {
            PQDataCollection = new ObservableCollection<PQData>();
            _downloadedPqs = new List<DownloadedPQInfo>();

            DataContext = this;
            InitializeComponent();

            Dispatcher.BeginInvoke(new Action(async ()=>{
                Core.ApplicationData.Instance.BeginActiviy();
                try
                {
                    if (!string.IsNullOrEmpty(Core.Settings.Default.LiveAPIDownloadedPQs))
                    {
                        string[] lines = Core.Settings.Default.LiveAPIDownloadedPQs.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach(string l in lines)
                        {
                            string[] parts = l.Split('|');
                            DownloadedPQInfo dp = new DownloadedPQInfo();
                            dp.Guid = Guid.Parse(parts[0]);
                            dp.DownloadedAt = DateTime.Parse(parts[1]);
                            if (dp.DownloadedAt >= DateTime.Now.AddDays(-7))
                            {
                                _downloadedPqs.Add(dp);
                            }
                        }
                        setLiveAPIDownloadedPQs();
                    }
                    using (var api = new LiveAPI.GeocachingLiveV6())
                    {
                        var resp = await api.Client.GetPocketQueryListAsync(api.Token);
                        if (resp.Status.StatusCode==0)
                        {
                            foreach(var r in resp.PocketQueryList)
                            {
                                PQDataCollection.Add(new PQData(r));
                            }
                        }
                        else
                        {
                            Core.ApplicationData.Instance.Logger.AddLog(this, Core.Logger.Level.Error, resp.Status.StatusMessage);
                        }
                    }
                    foreach(var p in PQDataCollection)
                    {
                        if ((from a in _downloadedPqs where a.Guid == p.LiveAPIData.GUID select a).FirstOrDefault() == null)
                        {
                            listItems.SelectedItems.Add(p);
                        }
                    }
                }
                catch(Exception e)
                {
                    Core.ApplicationData.Instance.Logger.AddLog(this, e);
                }
                Core.ApplicationData.Instance.EndActiviy();
            }));
        }

        private void setLiveAPIDownloadedPQs()
        {
            StringBuilder sb = new StringBuilder();
            foreach(var p in _downloadedPqs)
            {
                sb.AppendLine(string.Format("{0}|{1}", p.Guid.ToString(), p.DownloadedAt.ToString("s")));
            }
            Core.Settings.Default.LiveAPIDownloadedPQs = sb.ToString();
        }

        private void updateProcessedPq(Guid pqguid)
        {
            DownloadedPQInfo dp = (from a in _downloadedPqs where a.Guid == pqguid select a).FirstOrDefault();
            if (dp == null)
            {
                dp = new DownloadedPQInfo();
                dp.Guid = pqguid;
                _downloadedPqs.Add(dp);
            }
            dp.DownloadedAt = DateTime.Now;
            setLiveAPIDownloadedPQs();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            listItems.SelectAll();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            listItems.UnselectAll();
        }

        private AsyncDelegateCommand _importSelectedPQ;
        public AsyncDelegateCommand ImportSelectedPQ
        {
            get
            {
                if (_importSelectedPQ==null)
                {
                    _importSelectedPQ = new AsyncDelegateCommand(param => DownloadSelectedPQ(),
                                param => listItems.SelectedItems.Count > 0);
                }
                return _importSelectedPQ;
            }
        }
        public async Task DownloadSelectedPQ()
        {
            List<LiveAPI.LiveV6.PQData> pqs = new List<LiveAPI.LiveV6.PQData>();
            foreach (PQData p in listItems.SelectedItems)
            {
                pqs.Add(p.LiveAPIData);
            }

            using (Utils.DataUpdater upd = new Utils.DataUpdater(Core.ApplicationData.Instance.ActiveDatabase))
            {
                await Task.Run(new Action(() =>
                {
                    try
                    {
                        using (Utils.ProgressBlock progress = new Utils.ProgressBlock("DownloadingPQ", "DownloadingPQ", pqs.Count, 0, true))
                        {
                            int index = 0;
                            try
                            {
                                using (var api = new LiveAPI.GeocachingLiveV6())
                                {
                                    Import imp = new Import();
                                    foreach (LiveAPI.LiveV6.PQData pq in pqs)
                                    {
                                        if (progress.Update(pq.Name, pqs.Count, index))
                                        {
                                            LiveAPI.LiveV6.GetPocketQueryZippedFileResponse resp = api.Client.GetPocketQueryZippedFile(api.Token, pq.GUID);
                                            if (resp.Status.StatusCode == 0)
                                            {
                                                using (System.IO.TemporaryFile tf = new System.IO.TemporaryFile(true))
                                                {
                                                    System.IO.File.WriteAllBytes(tf.Path, Convert.FromBase64String(resp.ZippedFile));
                                                    imp.ImportFile(tf.Path);
                                                    updateProcessedPq(pq.GUID);
                                                }
                                            }
                                            else
                                            {
                                                Core.ApplicationData.Instance.Logger.AddLog(this, Core.Logger.Level.Error, resp.Status.StatusMessage);
                                                break;
                                            }
                                            index++;
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Core.ApplicationData.Instance.Logger.AddLog(this, e);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Core.ApplicationData.Instance.Logger.AddLog(this, e);
                    }
                }));
            }
            Close();
        }
    }
}
