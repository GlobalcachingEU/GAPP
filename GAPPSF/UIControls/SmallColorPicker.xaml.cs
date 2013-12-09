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
using System.ComponentModel;
using System.Diagnostics;

namespace OpenSourceControls
{
    /// <summary>
    /// SmallColorPicker user control
    /// This code is open source published with the Code Project Open License (CPOL).
    ///
    /// Originally written by Øystein Bjørke, March 2009.
    /// 
    /// The code and accompanying article can be found at http://www.codeproject.com
    /// </summary>
    public partial class SmallColorPicker : UserControl
    {
        #region Dependency properties
        public Color SelectedColor
        {
            get { return (Color)GetValue(SelectedColorProperty); }
            set
            {
               // Debug.WriteLine("SelectedColor.SetValue");
                SetValue(SelectedColorProperty, value);
            }
        }

        public static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.Register("SelectedColor", typeof(Color), typeof(SmallColorPicker),
            new FrameworkPropertyMetadata(OnSelectedColorChanged));

        private static void OnSelectedColorChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            // Debug.WriteLine("OnSelectedColorChanged");

            SmallColorPicker cp = obj as SmallColorPicker;
            Debug.Assert(cp != null);

            Color newColor = (Color)args.NewValue;
            Color oldColor = (Color)args.OldValue;

            if (newColor == oldColor)
                return;

            // When the SelectedColor changes, set the selected value of the combo box
            if (cp.Picker.SelectedValue == null || (Color)cp.Picker.SelectedValue != newColor)
            {
                // Add the color if not found
                if (!cp.Picker.Items.Contains(newColor))
                    cp.Picker.Items.Add(newColor);
            }

            // Also update the brush
            cp.SelectedBrush = new SolidColorBrush(newColor);

            cp.OnColorChanged(oldColor, newColor);

        }

        public Brush SelectedBrush
        {
            get { return (Brush)GetValue(SelectedBrushProperty); }
            set { SetValue(SelectedBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedBrushProperty =
            DependencyProperty.Register("SelectedBrush", typeof(Brush), typeof(SmallColorPicker),
            new FrameworkPropertyMetadata(OnSelectedBrushChanged));

        private static void OnSelectedBrushChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            // Debug.WriteLine("OnSelectedBrushChanged");
            SmallColorPicker cp = (SmallColorPicker)obj;
            SolidColorBrush newBrush = (SolidColorBrush)args.NewValue;
            // SolidColorBrush oldBrush = (SolidColorBrush)args.OldValue;

            if (cp.SelectedColor != newBrush.Color)
                cp.SelectedColor = newBrush.Color;
        }
        #endregion

        #region Events
        public static readonly RoutedEvent ColorChangedEvent =
            EventManager.RegisterRoutedEvent("ColorChanged", RoutingStrategy.Bubble,
                typeof(RoutedPropertyChangedEventHandler<Color>), typeof(SmallColorPicker));

        public event RoutedPropertyChangedEventHandler<Color> ColorChanged
        {
            add { AddHandler(ColorChangedEvent, value); }
            remove { RemoveHandler(ColorChangedEvent, value); }
        }

        protected virtual void OnColorChanged(Color oldValue, Color newValue)
        {
            // Debug.WriteLine("OnColorChanged (old=" + oldValue + ", new=" + newValue + ")");
            RoutedPropertyChangedEventArgs<Color> args = new RoutedPropertyChangedEventArgs<Color>(oldValue, newValue);
            args.RoutedEvent = SmallColorPicker.ColorChangedEvent;
            RaiseEvent(args);
        }
        #endregion

        public SmallColorPicker()
        {
            InitializeComponent();
            InitializeColors();
        }

        public void InitializeColors()
        {
            Picker.Items.Clear();

            // Add some common colors
            AddColor(Colors.White);
            AddColor(Color.FromRgb(192, 192, 192));
            AddColor(Color.FromRgb(128, 128, 128));
            AddColor(Color.FromRgb(64, 64, 64));
            AddColor(Color.FromRgb(0, 0, 0));

            // Add a rainbow of colors
            int N = 32 - 5;
            for (int i = 0; i < N; i++)
            {
                double H = 0.8 * i / (N - 1);
                Color c = HsvToColor(H, 1.0, 1.0);
                AddColor(c);
            }

        }

        /// <summary>
        /// Add a color to the ColorPicker list
        /// </summary>
        /// <param name="c"></param>
        private void AddColor(Color c)
        {
            Picker.Items.Add(c);
        }

        /// <summary>
        /// Convert from HSV to Color
        /// http://en.wikipedia.org/wiki/HSL_color_space
        /// </summary>
        /// <param name="h">Hue</param>
        /// <param name="s">Saturation</param>
        /// <param name="v">Value</param>
        /// <returns></returns>
        public static Color HsvToColor(double hue, double sat, double val)
        {
            int i;
            double aa, bb, cc, f;
            double r, g, b;
            r = g = b = 0;

            if (sat == 0) // Gray scale
                r = g = b = val;
            else
            {
                if (hue == 1.0) hue = 0;
                hue *= 6.0;
                i = (int)Math.Floor(hue);
                f = hue - i;
                aa = val * (1 - sat);
                bb = val * (1 - (sat * f));
                cc = val * (1 - (sat * (1 - f)));
                switch (i)
                {
                    case 0: r = val; g = cc; b = aa; break;
                    case 1: r = bb; g = val; b = aa; break;
                    case 2: r = aa; g = val; b = cc; break;
                    case 3: r = aa; g = bb; b = val; break;
                    case 4: r = cc; g = aa; b = val; break;
                    case 5: r = val; g = aa; b = bb; break;
                }
            }
            return Color.FromRgb((byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
        }

    }

    [ValueConversion(typeof(Color), typeof(Brush))]
    public class ColorToBrushConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(Brush)) return null;
            if (!(value is Color)) return null;
            SolidColorBrush scb = new SolidColorBrush((Color)value);
            return scb;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

}
