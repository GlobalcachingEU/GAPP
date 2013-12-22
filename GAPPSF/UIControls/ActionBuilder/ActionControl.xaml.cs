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

namespace GAPPSF.UIControls.ActionBuilder
{
    /// <summary>
    /// Interaction logic for ActionControl.xaml
    /// </summary>
    public partial class ActionControl : UserControl
    {
        private ActionImplementation _actionImplementation = null;

        public ActionControl()
        {
            InitializeComponent();
        }

        public ActionImplementation ActionImplementation
        {
            get { return _actionImplementation; }
            set
            {
                if (_actionImplementation != null)
                {
                    _actionImplementation.UIActionControl = null;
                }
                _actionImplementation = value;
                if (_actionImplementation != null)
                {
                    UIElement uiEl = _actionImplementation.GetUIElement();
                    if (uiEl != null)
                    {
                        ActionContent.Children.Add(uiEl);
                    }
                    Title.Content = Localization.TranslationManager.Instance.Translate(_actionImplementation.Name ?? "");
                    if (!_actionImplementation.AllowEntryPoint)
                    {
                        EntryPoint.Children.Clear();
                    }
                    ActionImplementation.Operator allowedOperators = _actionImplementation.AllowOperators;
                    if ((allowedOperators & ActionImplementation.Operator.Less) == 0) Smaller.Children.Clear();
                    if ((allowedOperators & ActionImplementation.Operator.LessOrEqual) == 0) SmallerEqual.Children.Clear();
                    if ((allowedOperators & ActionImplementation.Operator.Equal) == 0) Equal.Children.Clear();
                    if ((allowedOperators & ActionImplementation.Operator.NotEqual) == 0) NotEqual.Children.Clear();
                    if ((allowedOperators & ActionImplementation.Operator.LargerOrEqual) == 0) LargerEqual.Children.Clear();
                    if ((allowedOperators & ActionImplementation.Operator.Larger) == 0) Larger.Children.Clear();
                }
            }
        }

        public ActionImplementation.Operator GetOperator(string name)
        {
            if (name == Smaller.Name) return ActionImplementation.Operator.Less;
            else if (name == SmallerEqual.Name) return ActionImplementation.Operator.LessOrEqual;
            else if (name == Equal.Name) return ActionImplementation.Operator.Equal;
            else if (name == NotEqual.Name) return ActionImplementation.Operator.NotEqual;
            else if (name == LargerEqual.Name) return ActionImplementation.Operator.LargerOrEqual;
            else if (name == Larger.Name) return ActionImplementation.Operator.Larger;
            else return ActionImplementation.Operator.Equal;
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            Grid g = sender as Grid;
            if (g != null)
            {
                if (g.Children.Count > 0)
                {
                    g.Background = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255));
                }
                else
                {
                    g.Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                }
            }
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            Grid g = sender as Grid;
            if (g != null)
            {
                g.Background = null;
            }
        }

        private void Grid_MouseEnter_1(object sender, MouseEventArgs e)
        {
            DragCanvas.SetCanBeDragged(this, true);
        }

        private void Grid_MouseLeave_1(object sender, MouseEventArgs e)
        {
            DragCanvas.SetCanBeDragged(this, false);
        }
    }
}
