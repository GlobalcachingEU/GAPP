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

namespace GAPPSF.UIControls.InternalWebBrowser
{
    /// <summary>
    /// Interaction logic for Control.xaml
    /// </summary>
    public partial class Control : UserControl, IUIControl, INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<string> Favorites { get; set; }
        private string _activeUrl;
        public string ActiveUrl
        {
            get { return _activeUrl; }
            set 
            { 
                SetProperty(ref _activeUrl, value);

                bool isFav = !string.IsNullOrEmpty(_activeUrl) && Favorites.Contains(_activeUrl);
                IsFavoriteVisibility = isFav ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                IsNotFavoriteVisibility = isFav ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
            }
        }

        private string _matchingGeocacheCount;
        public string MatchingGeocacheCount
        {
            get { return _matchingGeocacheCount; }
            set { SetProperty(ref _matchingGeocacheCount, value); }
        }

        private string _allGeocacheCount;
        public string AllGeocacheCount
        {
            get { return _allGeocacheCount; }
            set { SetProperty(ref _allGeocacheCount, value); }
        }

        private string _missingGeocacheCount;
        public string MissingGeocacheCount
        {
            get { return _missingGeocacheCount; }
            set { SetProperty(ref _missingGeocacheCount, value); }
        }

        private Visibility _isFavoriteVisibility;
        public Visibility IsFavoriteVisibility
        {
            get { return _isFavoriteVisibility; }
            set { SetProperty(ref _isFavoriteVisibility, value); }
        }

        private Visibility _isNotFavoriteVisibility;
        public Visibility IsNotFavoriteVisibility
        {
            get { return _isNotFavoriteVisibility; }
            set { SetProperty(ref _isNotFavoriteVisibility, value); }
        }

        public Control()
        {
            Favorites = new ObservableCollection<string>();
            if (!string.IsNullOrEmpty(Core.Settings.Default.WebBrowserBookmarks))
            {
                string[] lines = Core.Settings.Default.WebBrowserBookmarks.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string s in lines)
                {
                    Favorites.Add(s);
                }
            }
            IsFavoriteVisibility = System.Windows.Visibility.Collapsed;
            IsNotFavoriteVisibility = System.Windows.Visibility.Visible;

            InitializeComponent();

            DataContext = this;
            Favorites.CollectionChanged += Favorites_CollectionChanged;

            webBrowser1.LoadCompleted += webBrowser1_LoadCompleted;
            webBrowser1.Navigated += webBrowser1_Navigated;
            webBrowser1.Navigating += webBrowser1_Navigating;

            if (!string.IsNullOrEmpty(Core.Settings.Default.WebBrowserHomePage))
            {
                navigateToPage(Core.Settings.Default.WebBrowserHomePage);
            }
        }

        void webBrowser1_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            MissingGeocacheCount = "-";
            AllGeocacheCount = "-";
            MatchingGeocacheCount = "-";
        }

        public void Dispose()
        {
        }

        void Favorites_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            foreach(string s in Favorites)
            {
                sb.AppendLine(s);
            }
            Core.Settings.Default.WebBrowserBookmarks = sb.ToString();
        }

        private void navigateToPage(string url)
        {
            try
            {
                webBrowser1.Navigate(url);
            }
            catch(Exception e)
            {
                Core.ApplicationData.Instance.Logger.AddLog(this, e);
            }
        }

        void webBrowser1_Navigated(object sender, NavigationEventArgs e)
        {
            Utils.ResourceHelper.HideScriptErrors(webBrowser1, true);
        }

        private void removeNewTargets(mshtml.IHTMLElementCollection elcol)
        {
            if (elcol != null)
            {
                foreach (mshtml.IHTMLElement el in elcol)
                {
                    var o = el.getAttribute("target");
                    if (o is System.DBNull || o == null)
                    {

                    }
                    else
                    {
                        el.setAttribute("target", "_self");
                    }
                    //removeNewTargets(el.children);
                }
            }
        }

        private void removeNewTargets()
        {
            try
            {
                removeNewTargets(((mshtml.HTMLDocument)webBrowser1.Document).all);
            }
            catch (Exception e)
            {
                Core.ApplicationData.Instance.Logger.AddLog(this, e);
            }
        }

        void webBrowser1_LoadCompleted(object sender, NavigationEventArgs e)
        {
            ActiveUrl = e.Uri.ToString();
            processPage();
        }


        private void processPage()
        {
            removeNewTargets();

            //scan for GC codes
            List<string> gcList = GetAllGCCodes();
            AllGeocacheCount = gcList.Count.ToString();
            if (Core.ApplicationData.Instance.ActiveDatabase!=null)
            {
                int cnt = (from a in gcList
                           join b in Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection
                               on a equals b.Code
                           select a).Count();
                MatchingGeocacheCount = cnt.ToString();
                MissingGeocacheCount = (gcList.Count - cnt).ToString();
            }
            else
            {
                MissingGeocacheCount = "-";
                MatchingGeocacheCount = "-";
            }
        }

        private bool IsValidGCCode(string code)
        {
            const string ValidGCCodeCharacters = "0123456789ABCDEFGHJKMNPQRTVWXYZ";

            bool result = false;
            code = code.ToUpper();
            if (!string.IsNullOrEmpty(code) && code.StartsWith("GC") && code.Length > 3 && code.Length <= 8)
            {
                result = true;
                for (int i = 2; i < code.Length; i++)
                {
                    if (ValidGCCodeCharacters.IndexOf(code[i]) < 0)
                    {
                        result = false;
                        break;
                    }
                }
            }
            return result;
        }

        private List<string> GetAllGCCodes()
        {
            List<string> result = new List<string>();
            try
            {
                char[] SepChars = new char[] { ' ', '>', '<', '&', '"', '\'', '(', ')', '[', ']', '\t', ';', ',', '|', '/', '\\', '\r', '\n' };

                if (((mshtml.HTMLDocument)webBrowser1.Document).body != null)
                {
                    string doc = ((mshtml.HTMLDocument)webBrowser1.Document).body.innerHTML;
                    if (doc != null)
                    {
                        int pos = doc.IndexOf("GC", StringComparison.InvariantCultureIgnoreCase);
                        int pos2;
                        while (pos > 0)
                        {
                            if (SepChars.Contains(doc[pos - 1]))
                            {
                                pos2 = doc.IndexOfAny(SepChars, pos);
                                if (pos2 > 0)
                                {
                                    string code = doc.Substring(pos, pos2 - pos).ToUpper();
                                    if (IsValidGCCode(code))
                                    {
                                        if (!result.Contains(code))
                                        {
                                            result.Add(code);
                                        }
                                    }
                                }
                            }
                            pos = doc.IndexOf("GC", pos + 1, StringComparison.InvariantCultureIgnoreCase);
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Core.ApplicationData.Instance.Logger.AddLog(this, e);
            }
            return result;
        }


        public override string ToString()
        {
            return Localization.TranslationManager.Instance.Translate("WebBrowser") as string;
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
                return Core.Settings.Default.WebBrowserWindowWidth;
            }
            set
            {
                Core.Settings.Default.WebBrowserWindowWidth = value;
            }
        }

        public int WindowHeight
        {
            get
            {
                return Core.Settings.Default.WebBrowserWindowHeight;
            }
            set
            {
                Core.Settings.Default.WebBrowserWindowHeight = value;
            }
        }

        public int WindowLeft
        {
            get
            {
                return Core.Settings.Default.WebBrowserWindowLeft;
            }
            set
            {
                Core.Settings.Default.WebBrowserWindowLeft = value;
            }
        }

        public int WindowTop
        {
            get
            {
                return Core.Settings.Default.WebBrowserWindowTop;
            }
            set
            {
                Core.Settings.Default.WebBrowserWindowTop = value;
            }
        }

        private void ComboBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key== Key.Enter)
            {
                if (!string.IsNullOrEmpty(ActiveUrl))
                {
                    string s = ActiveUrl;
                    if (!ActiveUrl.ToLower().StartsWith("http://"))
                    {
                        ActiveUrl = string.Concat("http://", s);
                    }
                    navigateToPage(ActiveUrl);
                    e.Handled = true;
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            processPage();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                //todo, not that easy
            }
            catch (Exception ex)
            {
                Core.ApplicationData.Instance.Logger.AddLog(this, ex);
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                if (webBrowser1.CanGoBack)
                {
                    webBrowser1.GoBack();
                }
            }
            catch (Exception ex)
            {
                Core.ApplicationData.Instance.Logger.AddLog(this, ex);
            }    
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            try
            {
                if (webBrowser1.CanGoForward)
                {
                    webBrowser1.GoForward();
                }
            }
            catch (Exception ex)
            {
                Core.ApplicationData.Instance.Logger.AddLog(this, ex);
            }    
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            try
            {
                webBrowser1.Refresh(true);
            }
            catch (Exception ex)
            {
                Core.ApplicationData.Instance.Logger.AddLog(this, ex);
            }    
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(Core.Settings.Default.WebBrowserHomePage))
            {
                navigateToPage(Core.Settings.Default.WebBrowserHomePage);
            }
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(ActiveUrl) && !Favorites.Contains(ActiveUrl))
            {
                Favorites.Add(ActiveUrl);
                IsFavoriteVisibility = System.Windows.Visibility.Visible;
                IsNotFavoriteVisibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(ActiveUrl) && Favorites.Contains(ActiveUrl))
            {
                Favorites.Remove(ActiveUrl);
                IsFavoriteVisibility = System.Windows.Visibility.Collapsed;
                IsNotFavoriteVisibility = System.Windows.Visibility.Visible;
            }
        }

        private void Button_Click_8(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Core.ApplicationData.Instance.ActiveDatabase != null)
                {
                    List<string> gcList = GetAllGCCodes();
                    var gc = from a in gcList
                             join b in Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection
                                 on a equals b.Code
                             select b;
                    using (Utils.DataUpdater upd = new Utils.DataUpdater(Core.ApplicationData.Instance.ActiveDatabase))
                    {
                        foreach (var g in gc)
                        {
                            g.Selected = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Core.ApplicationData.Instance.Logger.AddLog(this, ex);
            }
        }

        private AsyncDelegateCommand _importAllCommand;
        private AsyncDelegateCommand ImportAllCommand
        {
            get
            {
                if (_importAllCommand==null)
                {
                    _importAllCommand = new AsyncDelegateCommand(param => ImportAll(),
                        param => Core.ApplicationData.Instance.ActiveDatabase != null && Core.Settings.Default.LiveAPIMemberTypeId > 0);
                }
                return _importAllCommand;
            }
        }
        public async Task ImportAll()
        {
            if (Core.ApplicationData.Instance.ActiveDatabase!=null)
            {
                List<string> gcList = GetAllGCCodes();
                if (gcList.Count>0)
                {
                    using (Utils.DataUpdater upd = new Utils.DataUpdater(Core.ApplicationData.Instance.ActiveDatabase))
                    {
                        await Task.Run(() =>
                        {
                            LiveAPI.Import.ImportGeocaches(Core.ApplicationData.Instance.ActiveDatabase, gcList);
                        });
                    }
                }
            }
        }

        private AsyncDelegateCommand _importMissingCommand;
        private AsyncDelegateCommand ImportMissingCommand
        {
            get
            {
                if (_importMissingCommand == null)
                {
                    _importMissingCommand = new AsyncDelegateCommand(param => ImportMissing(),
                        param => Core.ApplicationData.Instance.ActiveDatabase != null && Core.Settings.Default.LiveAPIMemberTypeId > 0);
                }
                return _importMissingCommand;
            }
        }
        public async Task ImportMissing()
        {
            if (Core.ApplicationData.Instance.ActiveDatabase != null)
            {
                List<string> allList = GetAllGCCodes();
                List<string> gcList = (from a in allList where Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection.GetGeocache(a) == null select a).ToList();
                if (gcList.Count > 0)
                {
                    using (Utils.DataUpdater upd = new Utils.DataUpdater(Core.ApplicationData.Instance.ActiveDatabase))
                    {
                        await Task.Run(() =>
                        {
                            LiveAPI.Import.ImportGeocaches(Core.ApplicationData.Instance.ActiveDatabase, gcList);
                        });
                    }
                }
            }
        }

    }
}
