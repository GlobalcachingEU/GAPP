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
using System.Globalization;

namespace GAPPSF.UIControls
{
    /// <summary>
    /// Interaction logic for NumericUpDown.xaml
    /// </summary>
    public partial class NumericUpDown : UserControl,
                                        IFrameTxtBoxCtrl
    {
        static NumericUpDown()
        {
            Coercer.Initialize<NumericUpDown>();
        }
        private static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(decimal), typeof(NumericUpDown), new PropertyMetadata(100M));

        private static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(decimal), typeof(NumericUpDown), new PropertyMetadata(0M));

        private static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(decimal), typeof(NumericUpDown), new PropertyMetadata(0M, ValueChangedCallback));
        
        private static readonly DependencyProperty StepProperty =
            DependencyProperty.Register("Step", typeof(decimal), typeof(NumericUpDown), new PropertyMetadata(1M));

        private static readonly DependencyProperty DecimalPlacesProperty =
            DependencyProperty.Register("DecimalPlaces", typeof(int), typeof(NumericUpDown), new PropertyMetadata(0));

        private static readonly DependencyProperty DecimalSeparatorTypeProperty =
            DependencyProperty.Register("DecimalSeparatorType", typeof(DecimalSeparatorType), typeof(NumericUpDown), new PropertyMetadata(DecimalSeparatorType.System_Defined));

        private static readonly DependencyProperty NegativeSignTypeProperty =
            DependencyProperty.Register("NegativeSignType", typeof(NegativeSignType), typeof(NumericUpDown), new PropertyMetadata(NegativeSignType.System_Defined));

        private static readonly DependencyProperty NegativeSignSideProperty =
            DependencyProperty.Register("NegativeSignSide", typeof(NegativeSignSide), typeof(NumericUpDown), new PropertyMetadata(NegativeSignSide.System_Defined));

        private static readonly DependencyProperty NegativeTextBrushProperty =
            DependencyProperty.Register("NegativeTextBrush", typeof(Brush), typeof(NumericUpDown));

        private static readonly DependencyProperty TextAlignmentProperty =
            DependencyProperty.Register("TextAlignment", typeof(TextAlignment), typeof(NumericUpDown), new PropertyMetadata(TextAlignment.Right, TextAlignmentChangedCallback));

        private static readonly DependencyProperty OutOfRangeTextBrushProperty =
            DependencyProperty.Register("OutOfRangeTextBrush", typeof(Brush), typeof(NumericUpDown));
  
        private static void ValueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NumericUpDown NumUpDown = d as NumericUpDown;

            if (NumUpDown != null && e.NewValue is decimal)
                NumUpDown.FormatTextBox((decimal)e.NewValue);
        }
        private static void TextAlignmentChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NumericUpDown NumUpDown = d as NumericUpDown;

            if (NumUpDown != null && e.NewValue is TextAlignment)
                NumUpDown.TextBoxCtrl.TextAlignment = (TextAlignment)e.NewValue;
        }
        struct TempCutCopyInfo
        {
            public int Pos;
            public decimal Value;
            public string CutStr;
        }
        private TempCutCopyInfo TempCCInf;

        public NumericUpDown()
        {
            InitializeComponent();
        }
        private void Root_Loaded(object sender, RoutedEventArgs e)
        {
            if (Value < Minimum)
                Value = Minimum;
            else if (Value > Maximum)
                Value = Maximum;
            
            FormatTextBox(Value);
        }
        public decimal Maximum
        {
            get { return (decimal)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }
        public decimal Minimum
        {
            get { return (decimal)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }
        public decimal Value
        {
            get { return (decimal)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        public decimal Step
        {
            get { return (decimal)GetValue(StepProperty); }
            set { SetValue(StepProperty, value); }
        }
        public int DecimalPlaces
        {
            get { return (int)GetValue(DecimalPlacesProperty); }
            set { SetValue(DecimalPlacesProperty, value); }
        }
        public DecimalSeparatorType DecimalSeparatorType
        {
            get { return (DecimalSeparatorType)GetValue(DecimalSeparatorTypeProperty); }
            set { SetValue(DecimalSeparatorTypeProperty, value); }
        }
        public NegativeSignType NegativeSignType
        {
            get { return (NegativeSignType)GetValue(NegativeSignTypeProperty); }
            set { SetValue(NegativeSignTypeProperty, value); }
        }
        public NegativeSignSide NegativeSignSide
        {
            get { return (NegativeSignSide)GetValue(NegativeSignSideProperty); }
            set { SetValue(NegativeSignSideProperty, value); }
        }
        public Brush NegativeTextBrush
        {
            get { return (Brush)GetValue(NegativeTextBrushProperty); }
            set { SetValue(NegativeTextBrushProperty, value); }
        }
        public TextAlignment TextAlignment
        {
            get { return (TextAlignment)GetValue(TextAlignmentProperty); }
            set { SetValue(TextAlignmentProperty, value); }
        }
        public Brush OutOfRangeTextBrush
        {
            get { return (Brush)GetValue(OutOfRangeTextBrushProperty); }
            set { SetValue(OutOfRangeTextBrushProperty, value); }
        }
        TextBox IFrameTxtBoxCtrl.TextBox 
        {
            get { return TextBoxCtrl; } 
        }
        private Brush GetTextBrush(decimal dec)
        {
            return IsEnabled ? (dec < 0.0M && NegativeTextBrush != null) ? NegativeTextBrush : Foreground : SystemColors.GrayTextBrush;
        }
        private void FormatTextBox(decimal dec)
        {
            TextBoxCtrl.Foreground = GetTextBrush(dec);
            TextBoxCtrl.Text = dec.ToString("N", GetNumberFormat());
        }
        private void UpdateInput()
        {
            decimal CurrentValue;
            string Txt = TextBoxCtrl.Text;
            if (ValidateInput(ref Txt, out CurrentValue))
                Value = CurrentValue;

            FormatTextBox(Value);
        }

        private delegate void OperateAction();

        private void UpIncr()
        {
            if (Value + Step <= Maximum)
                Value += Step;
        }
        private void DownIncr()
        {
            if (Value - Step >= Minimum)
                Value -= Step;
        }
        private void OnOperateAction(OperateAction OpAct)
        {
            UpdateInput();
            OpAct();                      
            TextBoxCtrl.Focus();
            TextBoxCtrl.SelectAll();
        }
        private void OnUpIncr()
        {
            OnOperateAction(UpIncr);
        }
        private void OnDownIncr()
        {
            OnOperateAction(DownIncr);
        }
        private void UpDown_UpClick(object sender, RoutedEventArgs e)
        {
            OnUpIncr();
        }
        private void UpDown_DownClick(object sender, RoutedEventArgs e)
        {
            OnDownIncr();
        }
        private void UpdateTextBoxTxt(string Txt, int CurPos, ref Decimal CurrentValue)
        {
            if (OutOfRangeTextBrush != null && (CurrentValue < Minimum || CurrentValue > Maximum))
                TextBoxCtrl.Foreground = OutOfRangeTextBrush; 
            else
                TextBoxCtrl.Foreground = (Txt.Count(chr => chr == GetNegativeSign()[0]) == 1 && NegativeTextBrush != null) ?
                                                                                                NegativeTextBrush : Foreground;
            TextBoxCtrl.Text = Txt;
            TextBoxCtrl.SelectionStart = CurPos;
        }
        private void TextBoxCtrl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = false;

            if (e.Key == Key.Space) // Want to prevent entering spaces. Amazingly, TextBoxCtrl_PreviewTextInput not called, even though
                                    // TextBoxCtrl_PreviewTextInput IS called when enter return key (and e.Text = '\r') ??!!!  
                e.Handled = true;
            else if (e.Key == Key.Left)
            {
                // Prevent overlapping over an existing negative sign.
                if (IsNegativePrefix() && TextBoxCtrl.Text != null && TextBoxCtrl.Text.Length > 0
                                       && TextBoxCtrl.Text[0] == GetNegativeSign()[0])
                    e.Handled = (TextBoxCtrl.SelectionStart <= 1);
            }
            else if (e.Key == Key.Right)
            {
                if (!IsNegativePrefix() && TextBoxCtrl.Text != null && TextBoxCtrl.Text.Length > 0
                       && TextBoxCtrl.Text[TextBoxCtrl.Text.Length - 1] == GetNegativeSign()[0])
                    e.Handled = (TextBoxCtrl.SelectionStart == TextBoxCtrl.Text.Length - 1);
            }
            else if (e.Key == Key.Up)
            {
                OnUpIncr();
                e.Handled = true;
            }
            else if (e.Key == Key.Down)
            {
                OnDownIncr();
                e.Handled = true;
            }
            else if (e.Key == Key.Home || e.Key == Key.PageUp)
            {
                if (IsNegativePrefix() && TextBoxCtrl.Text != null && TextBoxCtrl.Text.Length > 0
                                                        && TextBoxCtrl.Text[0] == GetNegativeSign()[0])
                {
                    TextBoxCtrl.SelectionStart = 1;
                    TextBoxCtrl.SelectionLength = 0;
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.End || e.Key == Key.PageDown)
            {
                if (!IsNegativePrefix() && TextBoxCtrl.Text != null && TextBoxCtrl.Text.Length > 0
                                        && TextBoxCtrl.Text[TextBoxCtrl.Text.Length - 1] == GetNegativeSign()[0])
                {
                    TextBoxCtrl.SelectionStart = TextBoxCtrl.Text.Length - 1;
                    TextBoxCtrl.SelectionLength = 0;
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Delete || e.Key == Key.Back)
            {
                int CurPos = TextBoxCtrl.SelectionStart;
                string Txt = TextBoxCtrl.Text;
                bool UpdateTxt = true;

                if (TextBoxCtrl.SelectionLength > 0)
                    Txt = Txt.Remove(CurPos, TextBoxCtrl.SelectionLength);
                else if (e.Key == Key.Delete)
                {
                    if (CurPos < Txt.Length)
                        Txt = Txt.Remove(CurPos, 1);
                    else
                        UpdateTxt = false;
                }
                else if (CurPos > 0)
                    Txt = Txt.Remove(--CurPos, 1);
                else
                    UpdateTxt = false;

                decimal CurrentValue;
                if (UpdateTxt && ValidateInput(ref Txt, out CurrentValue, false))
                    UpdateTextBoxTxt(Txt, CurPos, ref CurrentValue);

                e.Handled = true;
            }
            else if (e.Key==Key.Enter)
            {
                e.Handled = true;

                UpdateInput(); ;
            }
        }
        public string GetDecimalSeparator()
        {
            switch (DecimalSeparatorType)
            {
                case DecimalSeparatorType.Point:
                    return ".";
                case DecimalSeparatorType.Comma:
                    return ",";
                case DecimalSeparatorType.System_Defined:
                default:
                    return SystemNumberInfo.DecimalSeparator;
            }
        }
        public string GetNegativeSign()
        {
            switch (NegativeSignType)
            {
                case NegativeSignType.Minus:
                    return "-";
                case NegativeSignType.System_Defined:
                default:
                    return SystemNumberInfo.NegativeSign;
            }
        }
        public bool IsNegativePrefix()
        {
            switch (NegativeSignSide)
            {
                case NegativeSignSide.Prefix:
                    return true;
                case NegativeSignSide.Suffix:
                    return false;
                case NegativeSignSide.System_Defined:
                default:
                    return SystemNumberInfo.IsNegativePrefix;
            }
        }
        IFormatProvider GetNumberFormat()
        {
            CultureInfo info = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            NumberFormatInfo nfi = info.NumberFormat;
            nfi.NumberDecimalSeparator = GetDecimalSeparator();
            nfi.NumberDecimalDigits = (DecimalPlaces >= 0) ? DecimalPlaces : 0; // Only works for output, not input as we require here...
            nfi.NegativeSign = GetNegativeSign();
            nfi.CurrencyDecimalDigits = (DecimalPlaces >= 0) ? DecimalPlaces : 0;
            nfi.NumberGroupSeparator = "";

            if (IsNegativePrefix() && nfi.NumberNegativePattern >= 3)
                nfi.NumberNegativePattern = 1;

            if (!IsNegativePrefix() && nfi.NumberNegativePattern < 3)
                nfi.NumberNegativePattern = 3;

            return info;
        }
        private bool GetCurrentValue(string Txt, out decimal CurrentValue)
        {
            // We need a bit of an extension. We require value such as "", "-", "." or "12." to be valid 
            // (with negative sign set to '-', and decimal separator set to '.' of course)
            if (Txt != null)
            {
                // if decimal places > 0 and only one decimal separator typed and it is after all digits... 
                // this makes an entry such as 12. valid.
                if (DecimalPlaces > 0 && Txt.Count(chr => chr == GetDecimalSeparator()[0]) == 1
                  && ((Txt.Length > 0 && Txt[Txt.Length - 1] == GetDecimalSeparator()[0])
                 || (Txt.Length > 1 && Txt[Txt.Length - 1] == GetNegativeSign()[0] && Txt[Txt.Length - 2] == GetDecimalSeparator()[0])))
                    Txt = Txt.Replace(GetDecimalSeparator(), "");

                if (Txt == "" || Txt == GetNegativeSign())
                {
                    CurrentValue = 0.0M;
                    return true;
                }
                if (Txt.Length > 0)
                    return decimal.TryParse(Txt, ((DecimalPlaces > 0) ? NumberStyles.AllowDecimalPoint : 0)
                        | (IsNegativePrefix() ? NumberStyles.AllowLeadingSign : NumberStyles.AllowTrailingSign), GetNumberFormat(), out CurrentValue);
            }
            CurrentValue = 0.0M;
            return false;
        }
        private bool ValidateInput(ref string Txt, out decimal CurrentValue, bool RangeCheck = true)
        {
            CurrentValue = 0.0m;
            bool IsValid = false;

            if (DecimalPlaces > 0) // If Txt has more digits than decimal places, trim out excess digits, otherwise get funny results.
            {
                int Idx, Length, NegSymbIdx;
                Idx = Txt.IndexOf(GetDecimalSeparator());

                if (Idx != -1)
                {
                    NegSymbIdx = Txt.IndexOf(GetNegativeSign(), ++Idx);
                    Length = (NegSymbIdx != -1)? NegSymbIdx - Idx : Txt.Length - Idx;

                    if (Length > DecimalPlaces)
                        Txt = Txt.Remove(Idx + DecimalPlaces, Length - DecimalPlaces);
                }
            }
            IsValid = GetCurrentValue(Txt, out CurrentValue);

            if (IsValid)
            {
                if (RangeCheck && (CurrentValue > Maximum || CurrentValue < Minimum))
                    IsValid = false;
            }
            return IsValid;
        }
        private void TextBoxCtrl_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length == 0)
                return;

            int CurPos = TextBoxCtrl.SelectionStart;
            string Txt;

            if (e.Text == GetNegativeSign() && Minimum < 0)
            {
                Txt = TextBoxCtrl.Text;
                if (string.IsNullOrEmpty(Txt))
                {
                    Txt = GetNegativeSign();

                    if (IsNegativePrefix())
                        CurPos++;
                }
                else
                {
                    if (IsNegativePrefix())
                    {
                        if ((Txt[0] == GetNegativeSign()[0]))
                        {
                            Txt = Txt.Remove(0, 1);
                            if (CurPos > 0) CurPos--;
                        }
                        else
                        {
                            Txt = GetNegativeSign() + Txt;
                            CurPos++;
                        }
                    }
                    else
                        Txt = (Txt[Txt.Length - 1] == GetNegativeSign()[0]) ? Txt.Remove(Txt.Length - 1, 1) : Txt + GetNegativeSign();
                }
            }
            else if (e.Text != GetNegativeSign())
            {
                Txt = TextBoxCtrl.Text.Remove(CurPos, TextBoxCtrl.SelectionLength);

                if (Keyboard.GetKeyStates(Key.Insert) == KeyStates.Toggled && CurPos < Txt.Length)
                    Txt = Txt.Remove(CurPos, 1);

                Txt = Txt.Insert(CurPos, e.Text);
                CurPos++;
            }
            else
                Txt = TextBoxCtrl.Text;

            decimal CurrentValue;
            if (ValidateInput(ref Txt, out CurrentValue, false))
            {
                // Remove any leading '0'. 
                int LeadZeroIdx = 0, StartPos = 0;

                if (Txt.Length > 0 && Txt[StartPos] == GetNegativeSign()[0])
                    LeadZeroIdx = StartPos = 1;

                while (LeadZeroIdx < Txt.Length && Txt[LeadZeroIdx] == '0')
                    LeadZeroIdx++;
 
                // Keep just 1 leading '0' unless 1st non zero is a digit.
                if (LeadZeroIdx != StartPos && !(LeadZeroIdx < Txt.Length && char.IsDigit(Txt[LeadZeroIdx])))
                    --LeadZeroIdx;

                Txt = Txt.Remove(StartPos, LeadZeroIdx - StartPos);

                if (e.Text == "0" && CurPos == StartPos + 1)
                    CurPos = StartPos;

                // This is for case of a suffixed negative sign and decimal places > 0. If type a number at end of string, this
                // is removed because it has moved beyond decimal places allowed. However, in this case do not want to move caret, 
                // otherwise it moves over negative sign.   
                if (e.Text != GetNegativeSign() && CurPos - 1 < Txt.Length && !IsNegativePrefix() &&
                                                            Txt[CurPos - 1] == GetNegativeSign()[0] && CurPos > 0)
                    CurPos--;

                UpdateTextBoxTxt(Txt, CurPos, ref CurrentValue);
            }
            e.Handled = true;
        }
        private void TextBoxCtrl_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
                OnUpIncr();
            else
                OnDownIncr();
        }
        private void TextBoxCtrl_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            UpdateInput();
        }
        private void Root_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            TextBoxCtrl.Foreground = GetTextBrush(Value);
        }
        private void InitTempInf(bool CanExecute, int SelStart, decimal CurrentValue, string CurrentSel)
        {
            if (CanExecute)
            {
                TempCCInf.Pos = SelStart;
                TempCCInf.Value = CurrentValue;
                TempCCInf.CutStr = CurrentSel;
            }
        }     
        private void Command_Cut_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            decimal CurrentValue = 0.0M;
            string NewTxt = TextBoxCtrl.Text.Remove(TextBoxCtrl.SelectionStart, TextBoxCtrl.SelectionLength);
            e.CanExecute = TextBoxCtrl.SelectionLength > 0 &&
               ValidateInput(ref NewTxt, out CurrentValue);

            InitTempInf(e.CanExecute, TextBoxCtrl.SelectionStart, CurrentValue, 
                                            TextBoxCtrl.Text.Substring(TextBoxCtrl.SelectionStart, TextBoxCtrl.SelectionLength));
            e.Handled = true;
        }
        private void Command_Paste_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            decimal CurrentValue = 0.0M;
            string ClipTxt = (string)System.Windows.Clipboard.GetData("Text");

            if (string.IsNullOrEmpty(ClipTxt))
                e.CanExecute = false;
            else
            {
                int CurPos = TextBoxCtrl.SelectionStart;
                string Txt = TextBoxCtrl.Text.Remove(CurPos, TextBoxCtrl.SelectionLength);
                Txt = Txt.Insert(CurPos, ClipTxt);
                e.CanExecute = ValidateInput(ref Txt, out CurrentValue);
                InitTempInf(e.CanExecute, TextBoxCtrl.SelectionStart, CurrentValue, "");
            }
            e.Handled = true;
        }
        private void CommandBinding_CutExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Value = TempCCInf.Value;
            TextBoxCtrl.SelectionStart = TempCCInf.Pos;
            System.Windows.Clipboard.SetData("Text", TempCCInf.CutStr);
            e.Handled = true;
        }
        private void CommandBinding_PasteExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Value = TempCCInf.Value;
            TextBoxCtrl.SelectionStart = TempCCInf.Pos;
        }
        private void TextBoxCtrl_PreviewDragOver(object sender, DragEventArgs e)
        {
           Control ctrl = sender as Control;

            if (ctrl != null)
            {
                string DragTxt = (string)e.Data.GetData("Text");
                Point pt = e.GetPosition(TextBoxCtrl);
                Object obj = e.Source;

                TextBoxCtrl.Focus();
                                
                int CharPosClosest = TextBoxCtrl.GetCharacterIndexFromPoint(pt, true); // With SnapToText = false, always get 1 returned 

                if (CharPosClosest == TextBoxCtrl.Text.Length - 1)
                {
                    // According to MSDN documentation, GetRectFromCharacterIndex always returns the zero-width rectangle preceeding 
                    // the character.
                    // Documentation says nothing about one past the last character. This I just guessed. Logically it should work,
                    // and you should be able to get this information, but documentation says nothing about this.
                    Rect rc = TextBoxCtrl.GetRectFromCharacterIndex(CharPosClosest+1);

                    if (rc.Right < pt.X)
                        CharPosClosest++;
                }
                TextBoxCtrl.SelectionStart = CharPosClosest;

                TextBoxCtrl.SelectionLength = 0;
                decimal CurrentValue;

                int CurPos = TextBoxCtrl.SelectionStart;          
                string Txt = TextBoxCtrl.Text.Insert(CurPos, DragTxt);
                e.Effects = ValidateInput(ref Txt, out CurrentValue) ? DragDropEffects.Move : DragDropEffects.None;
            }
            e.Handled = true;
        }
        private void TextBoxCtrl_PreviewDrop(object sender, DragEventArgs e)
        {
            string DragTxt = (string)e.Data.GetData("Text");
            int CurPos = TextBoxCtrl.SelectionStart;
            string Txt = TextBoxCtrl.Text.Insert(CurPos, DragTxt);
            decimal CurrentValue;

            if (ValidateInput(ref Txt, out CurrentValue))
            {
                e.Effects = DragDropEffects.Move;
                Value = CurrentValue;
                TextBoxCtrl.SelectionStart = CurPos;
                TextBoxCtrl.SelectionLength = DragTxt.Length;
            }
            e.Handled = true;
        }
        private void TextBoxCtrl_PreviewQueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            // Very reluctantly, preventing this control from initiating any drag drop operations.
            // There are cases when this should be prevented anyway or only copy allowed (not move): when text entry is no longer a valid – see
            // commented code below.
            // Otherwise, the reason I am preventing drag and drop are because: 
            //      (i)	It does not seem possible to receive a notification that a drop has taken place. e.Action is never set to DragAction.Drop,
            //      and I have not found a workaround. GiveFeedback does not seem to notify a drop has actually taken place either. We need to
            //      receive this notification, so underlying decimal Value can be correctly changed.
            //      (ii) It does not seem possible to indicate if the drop target is the same as the source. This is very pertinent. For instance,
            //      user enters a number “12.34”, then selects “.3” and drags that just after the “1”. User would expect to get “1.324”. To do this,
            //      We would need to know that the source is same as destination, as “.3” needs to be removed, before being reinserted. That is, 
            //      you need to insert it in “124” and not “12.34”, otherwise get “1.32.34”. Works fine with normal textboxes, but here doing number
            //      validation, and “1.32.34” is obviously an invalid number.
            // Otherwise, cut/copy/paste should work fine. Only drag/drop not functional.
            e.Action = DragAction.Cancel;
            e.Handled = true;
            /*string Txt = TextBoxCtrl.Text.Remove(TextBoxCtrl.SelectionStart, TextBoxCtrl.SelectionLength);
            decimal CurrentValue;

            if (!ValidateInput(Txt, out CurrentValue))
            {
                // Would be nice if we could have effects set to copy only (not move), but this does not seem possible.
                // This is because e.Effects: not available in QueryContinueDrag,and read-only in GiveFeedBack.
                e.Action = DragAction.Cancel;
                e.Handled = true;
            }*/
        }
    }
}
