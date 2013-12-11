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

            DataContext = this;
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
            foreach (string usr in users)
            {
                if ( (from a in lgs where string.Compare(a.Finder, usr, true)==0 select a).FirstOrDefault() != null)
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
            foreach (string usr in users)
            {
                if ((from a in lgs where string.Compare(a.Finder, usr, true) == 0 select a).FirstOrDefault() != null)
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
            foreach (string usr in users)
            {
                if ((from a in lgs where string.Compare(a.Finder, usr, true) == 0 select a).FirstOrDefault() == null)
                {
                    result = false;
                    break;
                }
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
                        if (Core.Settings.Default.GeocacheSelectionContext == SelectionContext.Context.NewSelection)
                        {
                            gcList = Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection;
                        }
                        else if (Core.Settings.Default.GeocacheSelectionContext == SelectionContext.Context.WithinSelection)
                        {
                            gcList = (from a in Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection where a.Selected select a).ToList();
                        }
                        else
                        {
                            gcList = (from a in Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection where !a.Selected select a).ToList();
                        }

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

                        //set selected within gcList
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
                                                                                         ((Core.Settings.Default.GeocacheFilterNotFoundByAny == BooleanEnum.False) && !foundByAll(gc, foundByUsers)))
                                );
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

    }
}
