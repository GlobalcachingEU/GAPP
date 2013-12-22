using GAPPSF.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
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

namespace GAPPSF.UIControls.ActionBuilder
{
    /// <summary>
    /// Interaction logic for Control.xaml
    /// </summary>
    public partial class Control : UserControl, IUIControl, IDisposable, INotifyPropertyChanged
    {
        private object _clickedObject = null;
        private Point _clickPosition;

        public event PropertyChangedEventHandler PropertyChanged;

        public Control()
        {
            InitializeComponent();

            DataContext = this;

            Localization.TranslationManager.Instance.LanguageChanged += Instance_LanguageChanged;

            try
            {
                Assembly asm = Assembly.GetExecutingAssembly();
                Type[] types = asm.GetTypes();
                foreach (Type t in types)
                {
                    if (t.IsClass && (t.BaseType == typeof(ActionImplementationCondition)))
                    {
                        ConstructorInfo constructor = t.GetConstructor(Type.EmptyTypes);
                        ActionImplementationCondition obj = (ActionImplementationCondition)constructor.Invoke(null);

                        //exception for the start
                        if (obj is ActionStart)
                        {
                            //skip. auto
                        }
                        else
                        {
                            Button b = new Button();
                            b.Tag = obj;
                            b.Height = 25;
                            b.PreviewMouseLeftButtonDown += b_PreviewMouseLeftButtonDown;
                            b.PreviewMouseMove += b_PreviewMouseMove;
                            b.PreviewMouseLeftButtonUp += b_PreviewMouseLeftButtonUp;
                            conditionPanel.Children.Add(b);
                        }
                    }
                }
                foreach (Type t in types)
                {
                    if (t.IsClass && (t.BaseType == typeof(ActionImplementationExecuteOnce)))
                    {
                        ConstructorInfo constructor = t.GetConstructor(Type.EmptyTypes);
                        ActionImplementationExecuteOnce obj = (ActionImplementationExecuteOnce)constructor.Invoke(null);

                        Button b = new Button();
                        b.Tag = obj;
                        b.Height = 25;
                        b.PreviewMouseLeftButtonUp += b_PreviewMouseLeftButtonUp;
                        b.PreviewMouseLeftButtonDown += b_PreviewMouseLeftButtonDown;
                        b.PreviewMouseMove += b_PreviewMouseMove;
                        oncePanel.Children.Add(b);
                    }
                }
                foreach (Type t in types)
                {
                    if (t.IsClass && (t.BaseType == typeof(ActionImplementationAction)))
                    {
                        ConstructorInfo constructor = t.GetConstructor(Type.EmptyTypes);
                        ActionImplementationAction obj = (ActionImplementationAction)constructor.Invoke(null);

                        //exception for the start
                        Button b = new Button();
                        b.Tag = obj;
                        b.Height = 25;
                        b.PreviewMouseLeftButtonUp += b_PreviewMouseLeftButtonUp;
                        b.PreviewMouseLeftButtonDown += b_PreviewMouseLeftButtonDown;
                        b.PreviewMouseMove += b_PreviewMouseMove;
                        actionPanel.Children.Add(b);
                    }
                }
            }
            catch(Exception e)
            {
                Core.ApplicationData.Instance.Logger.AddLog(this, e);
            }

            Instance_LanguageChanged(this, EventArgs.Empty);

            if (!string.IsNullOrEmpty(Core.Settings.Default.ActionBuilderActiveFlowName))
            {
                ActiveActionFlow = (from a in Manager.Instance.ActionFlows where a.Name == Core.Settings.Default.ActionBuilderActiveFlowName select a).FirstOrDefault();
            }
        }

        private void SaveData()
        {
            actionBuilderEditor1.CommitData();
            Manager.Instance.Save();
        }

        private AsyncDelegateCommand _executeCommand;
        public AsyncDelegateCommand ExecuteCommand
        {
            get
            {
                if (_executeCommand==null)
                {
                    _executeCommand = new AsyncDelegateCommand(param => ExecuteActiveFlowAsync(),
                        param => Core.ApplicationData.Instance.ActiveDatabase != null && IsFlowActive);
                }
                return _executeCommand;
            }
        }
        public async Task ExecuteActiveFlowAsync()
        {
            if (Core.ApplicationData.Instance.ActiveDatabase!=null && ActiveActionFlow!=null)
            {
                SaveData();
                await Manager.Instance.RunActionFow(ActiveActionFlow);
                actionBuilderEditor1.UpdateLabels();
            }
        }

        void b_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Type t = (sender as Button).Tag.GetType();
            ConstructorInfo constructor = t.GetConstructor(Type.EmptyTypes);
            ActionImplementation obj = (ActionImplementation)constructor.Invoke(null);
            obj.ID = Guid.NewGuid().ToString("N");
            ActiveActionFlow.Actions.Add(obj);
            actionBuilderEditor1.AddActionControl(obj);
            _clickedObject = null;
        }

        void b_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_clickedObject == sender)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    Button b = sender as Button;
                    if (b != null)
                    {
                        Point p = e.GetPosition(b);
                        if (p.X != _clickPosition.X || p.Y != _clickPosition.Y)
                        {
                            ActionImplementation ai = b.Tag as ActionImplementation;
                            if (ai != null)
                            {
                                _clickedObject = null;
                                e.Handled = true;
                                DragDrop.DoDragDrop(b, b.Tag.GetType().ToString(), DragDropEffects.Move);
                                Core.ApplicationData.Instance.Logger.AddLog(this, Core.Logger.Level.Info, "DragDrop.DoDragDrop");
                            }
                        }
                    }
                }
            }
        }

        void b_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _clickedObject = sender;
            _clickPosition = e.GetPosition(sender as Button);
            Core.ApplicationData.Instance.Logger.AddLog(this, Core.Logger.Level.Info, "start drag");
        }

        void Instance_LanguageChanged(object sender, EventArgs e)
        {
            List<Button> pnlButtons = new List<Button>();
            foreach (UIElement cnt in conditionPanel.Children)
            {
                if (cnt is Button)
                {
                    Button b = cnt as Button;
                    if (b.Tag is ActionImplementation)
                    {
                        b.Content = Localization.TranslationManager.Instance.Translate((b.Tag as ActionImplementation).Name);
                        pnlButtons.Add(b);
                    }
                }
            }
            conditionPanel.Children.Clear();
            var lst = pnlButtons.OrderBy(x => x.Content);
            foreach (var a in lst)
            {
                conditionPanel.Children.Add(a);
            }

            pnlButtons.Clear();
            foreach (UIElement cnt in actionPanel.Children)
            {
                if (cnt is Button)
                {
                    Button b = cnt as Button;
                    if (b.Tag is ActionImplementation)
                    {
                        b.Content = Localization.TranslationManager.Instance.Translate((b.Tag as ActionImplementation).Name);
                        pnlButtons.Add(b);
                    }
                }
            }
            actionPanel.Children.Clear();
            lst = pnlButtons.OrderBy(x => x.Content);
            foreach (var a in lst)
            {
                actionPanel.Children.Add(a);
            }

            pnlButtons.Clear();
            foreach (UIElement cnt in oncePanel.Children)
            {
                if (cnt is Button)
                {
                    Button b = cnt as Button;
                    if (b.Tag is ActionImplementation)
                    {
                        b.Content = Localization.TranslationManager.Instance.Translate((b.Tag as ActionImplementation).Name);
                        pnlButtons.Add(b);
                    }
                }
            }
            oncePanel.Children.Clear();
            lst = pnlButtons.OrderBy(x => x.Content);
            foreach (var a in lst)
            {
                oncePanel.Children.Add(a);
            }


        }

        public void Dispose()
        {
            actionBuilderEditor1.CommitData();
            actionBuilderEditor1.Clear(null);
            SaveData();
            Localization.TranslationManager.Instance.LanguageChanged -= Instance_LanguageChanged;
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string name = "")
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                var handler = PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs(name));
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public ActionFlow _activeActionFlow = null;
        public ActionFlow ActiveActionFlow
        {
            get { return _activeActionFlow; }
            set
            {
                if (SetProperty(ref _activeActionFlow, value))
                {
                    Core.Settings.Default.ActionBuilderActiveFlowName = _activeActionFlow == null ? "" : _activeActionFlow.Name;
                }
                IsFlowActive = _activeActionFlow != null;
            }
        }

        private bool _isFlowActive = false;
        public bool IsFlowActive
        {
            get { return _isFlowActive; }
            set { SetProperty(ref _isFlowActive, value); }
        }

        public int WindowWidth
        {
            get
            {
                return Core.Settings.Default.ActionBuilderWindowWidth;
            }
            set
            {
                Core.Settings.Default.ActionBuilderWindowWidth = value;
            }
        }

        public int WindowHeight
        {
            get
            {
                return Core.Settings.Default.ActionBuilderWindowHeight;
            }
            set
            {
                Core.Settings.Default.ActionBuilderWindowHeight = value;
            }
        }

        public int WindowLeft
        {
            get
            {
                return Core.Settings.Default.ActionBuilderWindowLeft;
            }
            set
            {
                Core.Settings.Default.ActionBuilderWindowLeft = value;
            }
        }

        public int WindowTop
        {
            get
            {
                return Core.Settings.Default.ActionBuilderWindowTop;
            }
            set
            {
                Core.Settings.Default.ActionBuilderWindowTop = value;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            inputDialog.Show(Localization.TranslationManager.Instance.Translate("Name").ToString());
            inputDialog.DialogClosed +=newDialog_DialogClosed;
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
                        if ((from a in Manager.Instance.ActionFlows where string.Compare(a.Name, s, true) == 0 select a).Count() == 0)
                        {
                            ActionFlow af = new ActionFlow();
                            af.Name = inputDialog.InputText;
                            Core.Settings.Default.ActionBuilderFlowID++;
                            af.ID = string.Format("actbuildf{0}", Core.Settings.Default.ActionBuilderFlowID);
                            af.Actions = new List<ActionImplementation>();
                            var obj = new ActionStart();
                            obj.ID = Guid.NewGuid().ToString("N");
                            af.Actions.Add(obj);
                            Manager.Instance.ActionFlows.Add(af);
                            ActiveActionFlow = af;

                            SaveData();
                            Manager.Instance.AddFlowToMenu(af);
                        }
                        else
                        {
                            Core.ApplicationData.Instance.Logger.AddLog(this, new Exception(string.Format("Flow with name {0} already exists.", s)));
                        }
                    }
                }
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ActiveActionFlow == null)
            {
                actionBuilderEditor1.CommitData();
                actionBuilderEditor1.Clear(null);
            }
            else
            {
                actionBuilderEditor1.CommitData();
                actionBuilderEditor1.Clear(ActiveActionFlow.Actions);
                ActionImplementation startAction = (from a in ActiveActionFlow.Actions where a is ActionStart select a).FirstOrDefault();
                if (startAction != null)
                {
                    if (startAction.UIActionControl != null)
                    {
                        startAction.UIActionControl.Title.Content = string.Format("{0}\r\n{1}", Localization.TranslationManager.Instance.Translate("Start"), ActiveActionFlow.Name);
                    }
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (ActiveActionFlow != null)
            {
                Manager.Instance.RemoveFlowFromMenu(ActiveActionFlow);
                ActionFlow af = ActiveActionFlow;
                ActiveActionFlow = null;
                Manager.Instance.ActionFlows.Remove(af);
                SaveData();
            }
        }

        private void UserControl_LostFocus(object sender, RoutedEventArgs e)
        {
            SaveData();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (ActiveActionFlow != null)
            {
                inputDialog.Show(Localization.TranslationManager.Instance.Translate("Name").ToString());
                inputDialog.DialogClosed += renameDialog_DialogClosed;
            }
        }

        private void renameDialog_DialogClosed(object sender, EventArgs e)
        {
            inputDialog.DialogClosed -= renameDialog_DialogClosed;
            if (ActiveActionFlow != null)
            {
                if (inputDialog.DialogResult)
                {
                    if (!string.IsNullOrEmpty(inputDialog.InputText))
                    {
                        string s = inputDialog.InputText.Trim();
                        if (s.Length > 0)
                        {
                            if ((from a in Manager.Instance.ActionFlows where string.Compare(a.Name, s, true) == 0 select a).Count() == 0)
                            {
                                Manager.Instance.RenameFlow(ActiveActionFlow, s);
                                SaveData();
                            }
                            else
                            {
                                Core.ApplicationData.Instance.Logger.AddLog(this, new Exception(string.Format("Flow with name {0} already exists.", s)));
                            }
                        }
                    }
                }
            }
        }


    }
}
