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
using System.Windows.Shapes;

namespace GAPPSF.OKAPI
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach(var s in SiteManager.Instance.AvailableSites)
            {
                s.SaveSettings();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SiteManager.Instance.ActiveSite != null && !string.IsNullOrEmpty(SiteManager.Instance.ActiveSite.Username))
                {
                    string username = SiteManager.Instance.ActiveSite.Username;
                    string userid = OKAPIService.GetUserID(SiteManager.Instance.ActiveSite, ref username);
                    SiteManager.Instance.ActiveSite.Username = username;
                    SiteManager.Instance.ActiveSite.UserID = userid;
                }
            }
            catch(Exception ex)
            {
                Core.ApplicationData.Instance.Logger.AddLog(this, ex);
            }
        }
    }
}
