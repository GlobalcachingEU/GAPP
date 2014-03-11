using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using GAPPSF.ActionSequence;
using GAPPSF.Commands;

namespace GAPPSF.Dialogs
{
    /// <summary>
    /// Interaction logic for ActionSequenceWindow.xaml
    /// </summary>
    public partial class ActionSequenceWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ActionSequenceWindow()
        {
            InitializeComponent();

            moveDown.IsEnabled = false;
            moveUp.IsEnabled = false;
            removeAction.IsEnabled = false;

            DataContext = this;
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

        private Sequence _activeSequence;
        public Sequence ActiveSequence
        {
            get { return _activeSequence; }
            set 
            { 
                SetProperty(ref _activeSequence, value);
                IsSequenceActive = value != null;
            }
        }

        private bool _isSequenceActive = false;
        public bool IsSequenceActive
        {
            get { return _isSequenceActive; }
            set { SetProperty(ref _isSequenceActive, value); }
        }


        private AsyncDelegateCommand _executeCommand;
        public AsyncDelegateCommand ExecuteCommand
        {
            get
            {
                if (_executeCommand == null)
                {
                    _executeCommand = new AsyncDelegateCommand(param => ExecuteSequence(),
                        param => Core.ApplicationData.Instance.ActiveDatabase != null && IsSequenceActive);
                }
                return _executeCommand;
            }
        }
        public async Task ExecuteSequence()
        {
            if (Core.ApplicationData.Instance.ActiveDatabase != null && ActiveSequence != null)
            {
                await Manager.Instance.ExecuteSequence(ActiveSequence);
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
                        if ((from a in Manager.Instance.ActionSequences where string.Compare(a.Name, s, true) == 0 select a).Count() == 0)
                        {
                            Sequence af = new Sequence();
                            af.Name = inputDialog.InputText;
                            Core.Settings.Default.ActionSequenceID++;
                            af.ID = string.Format("actseqf{0}", Core.Settings.Default.ActionSequenceID);
                            Manager.Instance.ActionSequences.Add(af);
                            ActiveSequence = af;

                            Manager.Instance.Save();
                            Manager.Instance.AddSequenceToMenu(af);
                        }
                        else
                        {
                            Core.ApplicationData.Instance.Logger.AddLog(this, new Exception(string.Format("Sequence with name {0} already exists.", s)));
                        }
                    }
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (ActiveSequence != null)
            {
                Manager.Instance.RemoveSequenceFromMenu(ActiveSequence);
                Sequence af = ActiveSequence;
                ActiveSequence = null;
                Manager.Instance.ActionSequences.Remove(af);
                Manager.Instance.Save();
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (ActiveSequence != null)
            {
                inputDialog.Show(Localization.TranslationManager.Instance.Translate("Name").ToString());
                inputDialog.DialogClosed += renameDialog_DialogClosed;
            }
        }

        private void renameDialog_DialogClosed(object sender, EventArgs e)
        {
            inputDialog.DialogClosed -= renameDialog_DialogClosed;
            if (ActiveSequence != null)
            {
                if (inputDialog.DialogResult)
                {
                    if (!string.IsNullOrEmpty(inputDialog.InputText))
                    {
                        string s = inputDialog.InputText.Trim();
                        if (s.Length > 0)
                        {
                            if ((from a in Manager.Instance.ActionSequences where string.Compare(a.Name, s, true) == 0 select a).Count() == 0)
                            {
                                Manager.Instance.RenameSequence(ActiveSequence, s);
                                Manager.Instance.Save();
                            }
                            else
                            {
                                Core.ApplicationData.Instance.Logger.AddLog(this, new Exception(string.Format("Sequence with name {0} already exists.", s)));
                            }
                        }
                    }
                }
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (ActiveSequence != null)
            {
                SelectActionWindow dlg = new SelectActionWindow();
                if (dlg.ShowDialog() == true)
                {
                    ActiveSequence.Actions.Add(new Tuple<string, string>(dlg.SelectedMenuItem.Name, dlg.SelectedPathFormat));
                    Manager.Instance.Save();
                }
            }
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            moveDown.IsEnabled = actionlist.SelectedIndex >= 0 && actionlist.SelectedIndex < actionlist.Items.Count-1;
            moveUp.IsEnabled = actionlist.SelectedIndex >= 0 && actionlist.SelectedIndex > 0;
            removeAction.IsEnabled = actionlist.SelectedIndex >= 0;            
        }

        private void removeAction_Click(object sender, RoutedEventArgs e)
        {
            Tuple<string, string> act = actionlist.SelectedItem as Tuple<string, string>;
            if (ActiveSequence != null && act != null)
            {
                ActiveSequence.Actions.Remove(act);
                Manager.Instance.Save();
            }
        }

        private void moveUp_Click(object sender, RoutedEventArgs e)
        {
            Tuple<string, string> act = actionlist.SelectedItem as Tuple<string, string>;
            if (ActiveSequence != null && act != null)
            {
                int index = actionlist.SelectedIndex;
                ActiveSequence.Actions.Remove(act);
                ActiveSequence.Actions.Insert(index-1, act);
                actionlist.SelectedItem = act;
                Manager.Instance.Save();
            }
        }

        private void moveDown_Click(object sender, RoutedEventArgs e)
        {
            Tuple<string, string> act = actionlist.SelectedItem as Tuple<string, string>;
            if (ActiveSequence != null && act != null)
            {
                int index = actionlist.SelectedIndex;
                ActiveSequence.Actions.Remove(act);
                ActiveSequence.Actions.Insert(index + 1, act);
                actionlist.SelectedItem = act;
                Manager.Instance.Save();
            }
        }

    }
}
