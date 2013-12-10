using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Windows.Markup;
using System.Windows.Controls.Primitives;

namespace GAPPSF.UIControls
{
    /// <summary>
    /// Interaction logic for TimeCtrl.xaml
    /// </summary>
    [ContentProperty("Children")]
    public partial class TimeCtrl : UserControl, IFrameTxtBoxCtrl
    {
        private static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(DateTime), typeof(TimeCtrl), new PropertyMetadata(DateTime.Now, ValueChangedCallback));

        private static readonly DependencyProperty TimePatternProperty =
            DependencyProperty.Register("TimePattern", typeof(string), typeof(TimeCtrl), new PropertyMetadata(SystemDateInfo.LongTimePattern));

        private static readonly DependencyProperty TextAlignmentProperty =
            DependencyProperty.Register("TextAlignment", typeof(TextAlignment), typeof(TimeCtrl), new PropertyMetadata(TextAlignment.Left, TextAlignmentChangedCallback));

        public static readonly DependencyProperty UseValidTimesProperty =
            DependencyProperty.Register("UseValidTimes", typeof(bool), typeof(TimeCtrl), new PropertyMetadata(false, UseValidTimesChangedCallback));

        public static readonly DependencyProperty ValidTimesNameProperty =
            DependencyProperty.Register("ValidTimesName", typeof(string), typeof(TimeCtrl), new PropertyMetadata(LanguageStrings.ValidTimes));

        public static readonly DependencyProperty NoValidTimesStringProperty =
            DependencyProperty.Register("NoValidTimesString", typeof(string), typeof(TimeCtrl), new PropertyMetadata(LanguageStrings.None));

        private static readonly DependencyProperty InvalidTimeTextBrushProperty =
            DependencyProperty.Register("InvalidTimeTextBrush", typeof(Brush), typeof(TimeCtrl), new PropertyMetadata(Brushes.Red));

        public static readonly DependencyProperty IsValidTimeProperty =
            DependencyProperty.Register("IsValidTime", typeof(bool), typeof(TimeCtrl), new PropertyMetadata(true, IsValidTimeChangedCallback));

        private static void ValueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TimeCtrl tc = d as TimeCtrl;

            if (tc != null && e.NewValue is DateTime)
            {
                foreach (UIElement ele in tc.TimeCtrls.Children)
                {
                    var ctrl = ele as FrameworkElement;

                    HMSType hmsType = ctrl.get_HMSType();

                    if (hmsType != HMSType.unknown)
                    {
                        var tb = ctrl as TextBox;
                        if (tb != null)
                            tb.set_HMSText((DateTime)e.NewValue);
                    }
                }
                tc.SetIsValidTime();
            }
        }
        private static void TextAlignmentChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TimeCtrl tc = d as TimeCtrl;

            if (tc != null && e.NewValue is TextAlignment)
            {
                tc.TextBoxCtrl.TextAlignment = (TextAlignment)e.NewValue;
                tc.ReloadTimeCtrlsGrid();
            }
        }
        private static void IsValidTimeChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TimeCtrl tc = d as TimeCtrl;

            if (tc != null && e.NewValue is bool)
            {
                foreach (FrameworkElement fe in tc.TimeCtrls.Children)
                {
                    if (fe is TextBox)
                        ((TextBox)fe).Foreground = tc.TextBrush;
                    else if (fe is TextBlock)
                        ((TextBlock)fe).Foreground = tc.TextBrush;
                }
            }
        }
        private static void UseValidTimesChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TimeCtrl tc = d as TimeCtrl;

            if (tc != null && e.NewValue is bool)
                tc.SetIsValidTime();
        }
        static TimeCtrl()
        {
            Coercer.Initialize<TimeCtrl>();
        }
        public TimeCtrl()
        {
            Children = new ObservableCollection<ValidTimeItem>();
            InitializeComponent();
            this.Children.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Children_CollectionChanged);
        }
        public DateTime Value
        {
            get { return (DateTime)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        public string TimePattern
        {
            get { return (string)GetValue(TimePatternProperty); }
            set { SetValue(TimePatternProperty, value); }
        }
        public TextAlignment TextAlignment
        {
            get { return (TextAlignment)GetValue(TextAlignmentProperty); }
            set { SetValue(TextAlignmentProperty, value); }
        }
        public bool UseValidTimes
        {
            get { return (bool)GetValue(UseValidTimesProperty); }
            set { SetValue(UseValidTimesProperty, value); }
        }
        public string ValidTimesName
        {
            get { return (string)GetValue(ValidTimesNameProperty); }
            set { SetValue(ValidTimesNameProperty, value); }
        }
        public string NoValidTimesString
        {
            get { return (string)GetValue(NoValidTimesStringProperty); }
            set { SetValue(NoValidTimesStringProperty, value); }
        }
        public Brush InvalidTimeTextBrush
        {
            get { return (Brush)GetValue(InvalidTimeTextBrushProperty); }
            set { SetValue(InvalidTimeTextBrushProperty, value); }
        }
        public bool IsValidTime
        {
            get { return (bool)GetValue(IsValidTimeProperty); }
            private set { SetValue(IsValidTimeProperty, value); }
        }
        public ObservableCollection<ValidTimeItem> Children
        {
            get; private set;
        }
        private Brush TextBrush
        {
            get
            {
                return IsEnabled ? (IsValidTime ? Foreground : InvalidTimeTextBrush) : SystemColors.GrayTextBrush;
            }
        }
        private void SetIsValidTime()
        {
            if (!UseValidTimes)
                IsValidTime = true;
            else
            {
                IsValidTime = false;

                foreach (ValidTimeItem vti in Children)
                {
                    if (Value >= vti.BeginTime && Value <= vti.EndTime)
                    {
                        IsValidTime = true;
                        break;
                    }
                }
            }
        }
        private void Children_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SetIsValidTime();
        }
        TextBox IFrameTxtBoxCtrl.TextBox
        {
            get { return TextBoxCtrl; }
        }
        private bool MouseClicked = false;
        private void AddGridCtrl(FrameworkElement ctrl)
        {
            var cd = new ColumnDefinition();
            cd.Width = new GridLength(1, GridUnitType.Auto);
            TimeCtrls.ColumnDefinitions.Add(cd);
            TimeCtrls.Children.Add(ctrl);
            Grid.SetColumn(ctrl, TimeCtrls.ColumnDefinitions.Count - 1);
        }
        private void AddHMSCtrlContextMenuItems(TextBox tb, params ICommand[] Commands)
        {
            foreach (ICommand Command in Commands)
            {
                MenuItem mi = new MenuItem();
                mi.Command = Command;
                tb.ContextMenu.Items.Add(mi);
            }
        }
        private bool CmdBindCutPasteExecuteHandler(object sender, bool Execute, bool Cut)
        {
            var tb = sender as TextBox;
            bool CanExecute = false;
            string ClipTxt = "";

            if (tb != null)
            {
                int CurPos = tb.SelectionStart, CurrentValue = 0, SelLength = tb.SelectionLength;

                if (Cut)
                    ClipTxt = tb.Text.Substring(CurPos, tb.SelectionLength);

                string Txt = tb.Text.Remove(CurPos, tb.SelectionLength);

                if (!Cut) // If paste
                    ClipTxt = (string)System.Windows.Clipboard.GetData("Text");

                CanExecute = (!Cut && string.IsNullOrEmpty(ClipTxt)) ? false : (tb.IsAM_PM() ? 
                    (ClipTxt == SystemDateInfo.AMDesignator || ClipTxt == SystemDateInfo.PMDesignator)
                    : ValidateInput(tb, (Cut)? Txt : Txt.Insert(CurPos, ClipTxt), out CurrentValue));
                
                if (Execute)
                {
                    Value = tb.IsAM_PM() ? Value.Reset_AM_PM_Time(ClipTxt == SystemDateInfo.AMDesignator)
                                        : Value.ResetTime(CurrentValue, tb.get_HMSType());
                    tb.SelectionStart = CurPos + SelLength;

                    if (Cut)
                        System.Windows.Clipboard.SetData("Text", ClipTxt);
                }
            }
            return CanExecute;
        }
        private void CmdBindCutExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            CmdBindCutPasteExecuteHandler(sender, true, true);
            e.Handled = true;
        }
        void CmdBindCutCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var tb = sender as TextBox;
            e.CanExecute = (tb != null) && tb.SelectionLength > 0 && !tb.IsAM_PM();
        }
        private void CmdBindPasteExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            CmdBindCutPasteExecuteHandler(sender, true, false);
            e.Handled = true;
        }
        void CmdBindPasteCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CmdBindCutPasteExecuteHandler(sender, false, false);
            e.Handled = true;
        }
        private void CmdBindCopyTimeExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            string strClipTime = "";
            foreach (FrameworkElement fe in TimeCtrls.Children)
            {
                if (fe is TextBox)
                    strClipTime += ((TextBox)fe).Text;
                else if (fe is TextBlock)
                    strClipTime += ((TextBlock)fe).Text;
            }
            System.Windows.Clipboard.SetData("Text", strClipTime);
            e.Handled = true;
        }
        private void CmdBindPasteTimeExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            int Hour, Minute, Second;
            if (TimeCtrlExtensions.IsValidTime((string)System.Windows.Clipboard.GetData("Text"), TimePattern, out Hour, out Minute, out Second))
                Value = new DateTime(Value.Year, Value.Month, Value.Day, Hour, Minute, Second, Value.Millisecond, Value.Kind);
        }
        void CmdBindPasteTimeCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            int Hour, Minute, Second;
            e.CanExecute = TimeCtrlExtensions.IsValidTime((string)System.Windows.Clipboard.GetData("Text"), TimePattern, out Hour, out Minute, out Second);
        }
        private void AddValidTimeString(string str, bool Highlighted = false)
        {
            var rd = new RowDefinition();
            ValidTimesGrid.RowDefinitions.Add(rd);
            TextBlock tb = new TextBlock();
            tb.Text = str;
            
            if (Highlighted)
                tb.FontWeight = FontWeights.Bold;

            tb.HorizontalAlignment = HorizontalAlignment.Center;
            tb.Background = Background;
            tb.Foreground = Foreground;
            tb.Margin = new Thickness(0.0);
            ValidTimesGrid.Children.Add(tb);
            Grid.SetRow(tb, ValidTimesGrid.RowDefinitions.Count - 1); ;
            ValidTimesGrid.Height += tb.Height;
        }
        private string GetFormatedStr(TimeEntry te)
        {
            string strFormat = "";
            string strAMPM = (te.Hour >= 12) ? SystemDateInfo.PMDesignator : SystemDateInfo.AMDesignator;
            char AMPMShort = (te.Hour >= 12) ? SystemDateInfo.PMDesignator[TimeCtrlExtensions.Get_t_Idx()] :
                                                SystemDateInfo.AMDesignator[TimeCtrlExtensions.Get_t_Idx()];

            foreach (FrameworkElement fe in TimeCtrls.Children)
                strFormat += fe.get_TextFormat();

            return string.Format(strFormat, te.Hour, te.Hour.Get12Hour(), te.Minute, te.Second, strAMPM, AMPMShort);
        }
        private void CmdBindShowValidTimesExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (ValidTimesGrid.Background == null)
                ValidTimesGrid.Background = Brushes.White;

            ValidTimesGrid.Children.Clear();
            ValidTimesGrid.ColumnDefinitions.Clear();
            ValidTimesGrid.RowDefinitions.Clear();
            var cd = new ColumnDefinition();
            ValidTimesGrid.ColumnDefinitions.Add(cd);
            ValidTimesGrid.Height = 0;

            AddValidTimeString(" " + ValidTimesName + " ", true);

            string str;
            foreach (ValidTimeItem vti in Children)
            {
                str = " " + LanguageStrings.From + " " + GetFormatedStr(vti.BeginTime) + " " + LanguageStrings.To
                                                                                 + " " + GetFormatedStr(vti.EndTime) + " ";
                AddValidTimeString(str);
            }
            if (Children.Count == 0)
                AddValidTimeString(" " + NoValidTimesString + " ");

            ValidTimesPopup.IsOpen = true;
        }
        private void AddHMSCtrlContextMenu(TextBox tb)
        {
            ContextMenu cmSource = new ContextMenu();
            tb.ContextMenu = cmSource;
            cmSource.PlacementTarget = tb; // Very important line: without this, context menu won't work if cursor not over a TextBox (but will otherwise!)

            AddHMSCtrlContextMenuItems(tb, ApplicationCommands.Cut, ApplicationCommands.Copy, ApplicationCommands.Paste, 
                                                TimeCtrlCustomCommands.CopyTime, TimeCtrlCustomCommands.PasteTime);

            if (UseValidTimes)
            {
                TimeCtrlCustomCommands.ShowValidTimes.Text = ValidTimesName;
                AddHMSCtrlContextMenuItems(tb, TimeCtrlCustomCommands.ShowValidTimes);
            }
            CommandBinding CmdBindCut = new CommandBinding(ApplicationCommands.Cut, CmdBindCutExecuted, CmdBindCutCanExecute);
            tb.CommandBindings.Add(CmdBindCut);

            CommandBinding CmdBindPaste = new CommandBinding(ApplicationCommands.Paste, CmdBindPasteExecuted, CmdBindPasteCanExecute);
            tb.CommandBindings.Add(CmdBindPaste);

            CommandBinding CmdBindCopyTime = new CommandBinding(TimeCtrlCustomCommands.CopyTime, CmdBindCopyTimeExecuted);
            tb.CommandBindings.Add(CmdBindCopyTime);

            CommandBinding CmdBindPasteTime = new CommandBinding(TimeCtrlCustomCommands.PasteTime, CmdBindPasteTimeExecuted, CmdBindPasteTimeCanExecute);
            tb.CommandBindings.Add(CmdBindPasteTime);

            CommandBinding CmdBindShowValidTimes = new CommandBinding(TimeCtrlCustomCommands.ShowValidTimes, CmdBindShowValidTimesExecuted);
            tb.CommandBindings.Add(CmdBindShowValidTimes);
        }
        private void AddHMSCtrl(HMSType hmsType)
        {
            TextBox tb = new TextBox();
            tb.set_HMSType(hmsType);
            tb.set_HMSText(Value);
            tb.Height = ActualHeight;
            tb.Margin = new Thickness(0.0);
            tb.BorderThickness = new Thickness();
            tb.Background = Background;
            tb.Foreground = TextBrush;
            tb.IsEnabled = IsEnabled;
           
            if (hmsType == HMSType.t || hmsType == HMSType.tt)
            {
                tb.Focusable = true;
                tb.AllowDrop = false;
                tb.IsReadOnly = true;
                tb.IsUndoEnabled = false;
                tb.GotKeyboardFocus += new KeyboardFocusChangedEventHandler(tb_tt_GotKeyboardFocus);
                tb.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(tb_tt_PreviewMouseLeftButtonDown); 
                tb.PreviewKeyDown += new KeyEventHandler(tb_tt_PreviewKeyDown);
                tb.PreviewTextInput += new TextCompositionEventHandler(tb_tt_PreviewTextInput);
            }
            else
            {
                tb.PreviewTextInput += new TextCompositionEventHandler(tb_PreviewTextInput);
                tb.PreviewLostKeyboardFocus += new KeyboardFocusChangedEventHandler(tb_PreviewLostKeyboardFocus);
                tb.GotKeyboardFocus += new KeyboardFocusChangedEventHandler(tb_GotKeyboardFocus);
                tb.LostKeyboardFocus += new KeyboardFocusChangedEventHandler(tb_LostKeyboardFocus);
                tb.PreviewKeyDown += new KeyEventHandler(tb_PreviewKeyDown);
                tb.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(tb_PreviewMouseLeftButtonDown);
                tb.PreviewMouseRightButtonDown += new MouseButtonEventHandler(tb_PreviewMouseRightButtonDown);
                tb.PreviewQueryContinueDrag += new QueryContinueDragEventHandler(tb_PreviewQueryContinueDrag);
                tb.PreviewDragEnter += new DragEventHandler(tb_PreviewDrag);
                tb.PreviewDragOver += new DragEventHandler(tb_PreviewDrag);                
            }
            AddHMSCtrlContextMenu(tb);
            tb.PreviewMouseWheel += new MouseWheelEventHandler(tb_PreviewMouseWheel);
            AddGridCtrl(tb);
        }
        private void AddString(string str)
        {
            TextBlock tb = new TextBlock();
            tb.Text = str;
            tb.Background = Brushes.Transparent;
            tb.Foreground = TextBrush;
            tb.Margin = new Thickness(0.0);
            tb.Height = Height;
            AddGridCtrl(tb);
        }
        void tb_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // MouseWheel does not work very well. The mouse cursor has to be over the item for it to work.
            // CodePlex sample does same, which is why I am leaving implementation as such.
            var tb = sender as TextBox;

            if (tb != null)
            {
                if (e.Delta > 0)
                    ManipulateValue(tb, IncrementValue);
                else
                    ManipulateValue(tb, DecrementValue);
                
                // Following is illegal: !!!!! (would replace 4 lines above)
                // ManipulateValue(tb, (e.Delta > 0) ? IncrementValue : DecrementValue);
                tb.Focus();
                e.Handled = true;
            }
        }
        void tb_tt_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var tb = sender as TextBox;

            if (tb != null)
                tb.SelectAll();
        }
        void tb_tt_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var tb = sender as TextBox;

            if (tb != null)
            {
                tb.Focus();
                e.Handled = true;
            }             
        }
        void AM_PM_Handle(TextBox tb)
        {
            bool IsAm;
            if (tb.get_HMSType() == HMSType.tt)
                IsAm = (tb.Text == SystemDateInfo.AMDesignator);
            else // tb.get_HMSType() == HMSType.t
                IsAm = (tb.Text == SystemDateInfo.AMDesignator[TimeCtrlExtensions.Get_t_Idx()].ToString());

            Value = Value.Reset_AM_PM_Time(IsAm);

            tb.SelectAll();
        }
        void AM_PM_Change(TextBox tb)
        {
            if (tb.get_HMSType() == HMSType.tt)
                tb.Text = (tb.Text == SystemDateInfo.AMDesignator) ? SystemDateInfo.PMDesignator : SystemDateInfo.AMDesignator;
            else // tb.get_HMSType() == HMSType.t
            {
                int Idx = TimeCtrlExtensions.Get_t_Idx();
                tb.Text = (tb.Text == SystemDateInfo.AMDesignator[Idx].ToString()) ?
                    SystemDateInfo.PMDesignator[Idx].ToString() : SystemDateInfo.AMDesignator[Idx].ToString();
            }
            AM_PM_Handle(tb);
        }
        void tb_tt_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var tb = sender as TextBox;

            if (tb == null)
                return;

            if (e.Key == Key.Up || e.Key == Key.Down)
            {
                AM_PM_Change(tb);
                e.Handled = true;
            }
        }
        bool AM_PM_HandleInput(TextBox tb, string InputTxt, string AM_PM_Designator, int Idx)
        {
            if (string.Compare(InputTxt, AM_PM_Designator[Idx].ToString(), true) == 0)
            {
                if (tb.get_HMSType() == HMSType.tt)
                    tb.Text = AM_PM_Designator;  
                else // tb.get_HMSType() == HMSType.t
                    tb.Text = AM_PM_Designator[Idx].ToString(); 
         
                AM_PM_Handle(tb);
                return true;
            }
            return false;
        }
        void tb_tt_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = sender as TextBox;

            if (tb != null)
            {
                int Idx = TimeCtrlExtensions.Get_t_Idx();
                   
                if (!AM_PM_HandleInput(tb, e.Text, SystemDateInfo.AMDesignator, Idx))
                    e.Handled = AM_PM_HandleInput(tb, e.Text, SystemDateInfo.PMDesignator, Idx);
                else
                    e.Handled = true;
            }
        }
        void tb_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var tb = sender as TextBox;

            if (tb != null && tb.Text == "")
                tb.set_HMSText(Value);     
        }
        void tb_PreviewDrag(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
            e.Handled = true;
        }
        void tb_PreviewQueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            e.Action = DragAction.Cancel;
            e.Handled = true;          
        }
        void tb_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            MouseClicked = true;
        }
        void tb_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MouseClicked = true; 
        }
        private delegate void ValueManipulator(TextBox tb, int NewValue);

        private int AdjustHalfDayHour(TextBox tb, int CurrentValue)
        {
            if (tb.IsHalfDayHour())
            {
                if (CurrentValue == 12)
                {
                    if (Value.Hour < 12)
                        CurrentValue = 0;
                }
                else if (Value.Hour >= 12)
                    CurrentValue += 12; 
            }
            return CurrentValue;
        }
        private void IncrementValue(TextBox tb, int NewValue)
        {
            NewValue = (NewValue < tb.get_Max() - 1) ? NewValue + 1 : tb.get_Min();
            NewValue = AdjustHalfDayHour(tb, NewValue);
            Value = Value.ResetTime(NewValue, tb.get_HMSType());
        }
        private void DecrementValue(TextBox tb, int NewValue)
        {
            NewValue = (NewValue > tb.get_Min()) ? NewValue - 1 : tb.get_Max() - 1;
            NewValue = AdjustHalfDayHour(tb, NewValue);
            Value = Value.ResetTime(NewValue, tb.get_HMSType());
        }
        private void ManipulateValue(TextBox tb, ValueManipulator ValMan)
        {
            if (tb.get_HMSType() == HMSType.t || tb.get_HMSType() == HMSType.tt)
            {
                AM_PM_Change(tb);
                return;
            }
            int NewValue;

            if (int.TryParse(tb.Text, out NewValue))
                ValMan(tb, NewValue);

            tb.Focus();
            tb.SelectAll();
        }
        void tb_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var tb = sender as TextBox;

            if (tb == null)
                return;

            if (e.Key == Key.Up)
            {       
                ManipulateValue(tb, IncrementValue);
                e.Handled = true;
            }
            else if (e.Key == Key.Down)
            {
                ManipulateValue(tb, DecrementValue);
                e.Handled = true;
            }
            else if (e.Key == Key.Delete || e.Key == Key.Back)
            {
                int CurPos = tb.SelectionStart;
                string Txt = tb.Text;
                e.Handled = true;
               
                if (tb.SelectionLength > 0)
                    Txt = Txt.Remove(CurPos, tb.SelectionLength);
                else if (e.Key == Key.Delete && CurPos < Txt.Length)
                {
                    Txt = Txt.Remove(CurPos, 1);

                    if (tb.IsAlways2CharInt())
                        CurPos++;
                }
                else if (e.Key == Key.Back && CurPos > 0)
                {
                    Txt = Txt.Remove(CurPos - 1, 1);
                    if (!tb.IsAlways2CharInt()) --CurPos;
                }
                else
                    e.Handled = false;

                if (e.Handled)
                {
                    int CurrentValue;
                    if (ValidateInput(tb, Txt, out CurrentValue))
                    {
                        Value = Value.ResetTime(CurrentValue, tb.get_HMSType());
                        tb.SelectionStart = CurPos;
                    }
                    else if (Txt == "" || (Txt == "0" && tb.get_HMSType() == HMSType.hhour))
                    {
                        tb.SelectionStart = 0;
                        tb.Text = "";
                    }
                    else
                        e.Handled = false;
                }
            }
            else if (e.Key == Key.Space) // Want to prevent entering spaces. Amazingly, tb_PreviewTextInput not called, even though
                // tb_PreviewTextInput IS called when enter return key (and e.Text = '\r') ??!!!  
                e.Handled = true;
        }
        void tb_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var tb = sender as TextBox;

            if (tb != null && MouseClicked == false) // We do not want to select all when user has clicked on TextBox. 
                // Without the MouseClicked parameter, everything is selected/deselected immediately, which looks ugly.
                // I agree having this parameter is not the most pretty programming style, but know no better way. 
                tb.SelectAll();

            MouseClicked = false;
        }
        void tb_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var tb = sender as TextBox;

            if (tb != null && e.NewFocus == tb.ContextMenu)
            {
                tb.Focus();
                tb.ContextMenu.DataContext = tb;
                e.Handled = true;
            }
        }
        /// <param name="shortHMS">An HMS tag corresponding to 'H', 'h', 'm', 's', 't' </param>
        /// <param name="longHMS">An HMS tag corresponding to 'HH', 'hh', 'mm', 'ss', 'tt'</param>
        private int LoadTimeTag(string TimePattern, int Idx, HMSType shortHMS, HMSType longHMS)
        {
            if (Idx < TimePattern.Length - 1 && TimePattern[Idx + 1] == TimePattern[Idx])
            {
                AddHMSCtrl(longHMS);
                Idx++;
            }
            else
                AddHMSCtrl(shortHMS);

            return Idx;
        }
        private int LoadTimeSeparator(string TimePattern, int Idx)
        {
            // Assume the separator is something like ':' or '.'. Not considering case when want to use a character normally used
            // to indicate a time tag.
            int StartIdx = Idx;

            while (Idx < TimePattern.Length - 1 && TimePattern[Idx + 1] != 'H'
                                                && TimePattern[Idx + 1] != 'h'
                                                && TimePattern[Idx + 1] != 'm'
                                                && TimePattern[Idx + 1] != 's'
                                                && TimePattern[Idx + 1] != 't')
                Idx++;

            AddString(TimePattern.Substring(StartIdx, Idx - StartIdx + 1));
            return Idx;
        }
        private void LoadTimePattern(string TimePattern)
        {
            for (int Idx = 0; Idx < TimePattern.Length; Idx++)
            {
                switch (TimePattern[Idx])
                {
                    case 'H':
                        Idx = LoadTimeTag(TimePattern, Idx, HMSType.Hour, HMSType.HHour);
                    break;

                    case 'h':
                        Idx = LoadTimeTag(TimePattern, Idx, HMSType.hour, HMSType.hhour);
                    break;

                    case 'm':
                        Idx = LoadTimeTag(TimePattern, Idx, HMSType.minute, HMSType.mminute);
                    break;

                    case 's':
                        Idx = LoadTimeTag(TimePattern, Idx, HMSType.second, HMSType.ssecond);
                    break;

                    case 't':
                        Idx = LoadTimeTag(TimePattern, Idx, HMSType.t, HMSType.tt);
                    break;

                    default:
                        Idx = LoadTimeSeparator(TimePattern, Idx);
                    break;
                }
            }
        }
        private void ReloadTimeCtrlsGrid()
        {
            TimeCtrls.Children.Clear();
            TimeCtrls.ColumnDefinitions.Clear();
            TimeCtrls.RowDefinitions.Clear();
            TimeCtrls.RowDefinitions.Add(new RowDefinition());

            if (TextAlignment == TextAlignment.Right || TextAlignment == TextAlignment.Center)
                TimeCtrls.ColumnDefinitions.Add(new ColumnDefinition());

            LoadTimePattern(TimePattern);

            if (TextAlignment == TextAlignment.Left || TextAlignment == TextAlignment.Center)
                TimeCtrls.ColumnDefinitions.Add(new ColumnDefinition());
        }
        private void Root_Loaded(object sender, RoutedEventArgs e)
        {
            ReloadTimeCtrlsGrid();
        }
        private TextBox GetNextTextBox(int Idx)
        {
            while (++Idx < TimeCtrls.Children.Count && !(TimeCtrls.Children[Idx] is TextBox)) ;
            return (Idx < TimeCtrls.Children.Count) ? (TextBox)TimeCtrls.Children[Idx] : null;
        }
        void tb_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox tb = sender as TextBox;

            if (tb != null)
            {
                int CurrentValue;

                int CurPos = tb.SelectionStart, SelLen = tb.SelectionLength;
                string Txt = tb.Text.Remove(CurPos, SelLen);

                if (Keyboard.GetKeyStates(Key.Insert) == KeyStates.Toggled && CurPos < Txt.Length)
                    Txt = Txt.Remove(CurPos, 1);

                Txt = Txt.Insert(CurPos, e.Text);
            
                // Don't do anything for entries like "003" "012" or "05" (not '0' prefixed) when entry is a '0' only
                if (!(e.Text == "0" && ((Txt.Length == 3 && Txt[0] == '0' && CurPos == 0) || (!tb.IsAlways2CharInt() && Txt.Length == 2 && Txt[0] == '0')))
                    && ValidateInput(tb, Txt, out CurrentValue))
                {
                    CurrentValue = AdjustHalfDayHour(tb, CurrentValue);
                    Value = Value.ResetTime(CurrentValue, tb.get_HMSType());

                    tb.SelectionStart = (tb.Text.Length == 1 ||
                                    (CurPos == 0 && SelLen < 2 && Txt.Length >= 2) ||
                                    (Txt.Length == 3 && Txt[0] == '0' && CurPos == 1 && SelLen == 0)) ? 1 : 2;            
                }
                else if (tb.IsHalfDayHour() && (Txt == "0" || Txt == "00"))
                    tb.Text = "";
            }
            e.Handled = true;
        }
        private bool ValidateInput(TextBox tb, string Txt, out int CurrentValue)
        {
            if (int.TryParse(Txt, out CurrentValue))
                return (CurrentValue < tb.get_Max() && CurrentValue >= tb.get_Min());

            return false;
        }
        private TextBox SetFocusToClosestTextBox()
        {
            Point pt;
            TextBox tbClosest = null;
            double MinDist = 0;

            foreach (UIElement ele in TimeCtrls.Children)
            {
                var tb = ele as TextBox;
                if (tb != null)
                {
                    pt = Mouse.GetPosition(tb);
                    if (tbClosest == null || Math.Abs(pt.X) < MinDist)
                    {
                        tbClosest = tb;
                        MinDist = Math.Abs(pt.X);
                    }
                }
            }
            if (tbClosest != null)
            {
                tbClosest.Focus();
                tbClosest.CaptureMouse();
            }
            return tbClosest;
        }
        private void Root_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MouseClicked = true; 
            SetFocusToClosestTextBox();
        }
        private void Root_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            MouseClicked = true; 
            TextBox tb = SetFocusToClosestTextBox();

            if (tb != null)
            {
                tb.ContextMenu.IsOpen = true;
                e.Handled = true;
            }
        }
        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
            e.Handled = true;
        }
        private TextBox GetTextBoxToFocusOn()
        {
            var tb = FocusManager.GetFocusedElement(this) as TextBox;

            if (tb == null)
                tb = GetNextTextBox(-1);

            return tb;
        }
        private void UpDown_UpClick(object sender, RoutedEventArgs e)
        {
            var tb = GetTextBoxToFocusOn();
            if (tb != null) ManipulateValue(tb, IncrementValue);
        }
        private void UpDown_DownClick(object sender, RoutedEventArgs e)
        {
            var tb = GetTextBoxToFocusOn();
            if (tb != null) ManipulateValue(tb, DecrementValue);
        }
        private void Root_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            foreach (FrameworkElement fe in TimeCtrls.Children)
            {
                if (fe is TextBox)
                {
                    ((TextBox)fe).IsEnabled = (bool)e.NewValue;
                    ((TextBox)fe).Foreground = TextBrush;
                }
                else if (fe is TextBlock)
                    ((TextBlock)fe).Foreground = TextBrush;
            }
        } 
    }
}