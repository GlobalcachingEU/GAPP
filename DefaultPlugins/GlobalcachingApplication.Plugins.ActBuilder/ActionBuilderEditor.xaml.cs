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
using System.Reflection;
using System.Windows.Media.Effects;

namespace GlobalcachingApplication.Plugins.ActBuilder
{
    /// <summary>
    /// Interaction logic for ActionBuilderEditor.xaml
    /// </summary>
    public partial class ActionBuilderEditor : UserControl
    {
        private UIElement _elementForContextMenu;
        private UIElement _elementHover;
        private Line _currentConnectionLine = null;
        private TranslateTransform _translateTransform = new TranslateTransform(0, 0);
        private List<ActionImplementation> _activeActionListImplementations = null;
        private Point _mouseDown;
        private bool _doPan = false;
        private bool _showConnectionLabels = true;

        public class ConnectionInfo
        {
            public Line ConnectionLine { get; set; }
            public Grid StartConnector { get; set; }
            public Grid EndConnector { get; set; }
            public ActionControl StartActionControl { get; set; }
            public ActionControl EndActionControl { get; set; }
            public Label Label { get; set; }
        }
        private List<ConnectionInfo> _connectionInfo = new List<ConnectionInfo>();
        
        public ActionBuilderEditor()
        {
            InitializeComponent();

            dragCanvas.ContextMenu = null;
            dragCanvas.Focusable = true;
            dragCanvas.MouseMove += new MouseEventHandler(dragCanvas_MouseMove);
            dragCanvas.MouseDown += new MouseButtonEventHandler(dragCanvas_MouseDown);
        }

        void dragCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_elementHover == null && e.LeftButton == MouseButtonState.Pressed)
            {
                _doPan = true;
                _mouseDown = e.GetPosition(dragCanvas);
            }
            else
            {
                _doPan = false;
            }
        }

        public void CommitData()
        {
            if (_activeActionListImplementations != null)
            {
                foreach (ActionImplementation actImpl in _activeActionListImplementations)
                {
                    if (actImpl.UIActionControl.ActionContent.Children.Count > 0)
                    {
                        actImpl.CommitUIData(actImpl.UIActionControl.ActionContent.Children[0]);
                    }
                }
            }
        }

        public void ShowConnectionLabels(bool show)
        {
            if (_showConnectionLabels != show)
            {
                _showConnectionLabels = show;
                if (_connectionInfo != null)
                {
                    foreach (ConnectionInfo ci in _connectionInfo)
                    {
                        if (_showConnectionLabels)
                        {
                            ci.Label.Visibility = System.Windows.Visibility.Visible;
                        }
                        else
                        {
                            ci.Label.Visibility = System.Windows.Visibility.Hidden;
                        }
                    }
                }
            }
        }

        public void UpdateLabels()
        {
            if (_connectionInfo != null)
            {
                foreach (ConnectionInfo ci in _connectionInfo)
                {
                    if (ci.StartConnector == ci.StartActionControl.Equal)
                    {
                        ci.Label.Content = ci.StartActionControl.ActionImplementation.GetOutputConnectorPassCounter(ActionImplementation.Operator.Equal).ToString();
                    }
                    else if (ci.StartConnector == ci.StartActionControl.Larger)
                    {
                        ci.Label.Content = ci.StartActionControl.ActionImplementation.GetOutputConnectorPassCounter(ActionImplementation.Operator.Larger).ToString();
                    }
                    else if (ci.StartConnector == ci.StartActionControl.LargerEqual)
                    {
                        ci.Label.Content = ci.StartActionControl.ActionImplementation.GetOutputConnectorPassCounter(ActionImplementation.Operator.LargerOrEqual).ToString();
                    }
                    else if (ci.StartConnector == ci.StartActionControl.Smaller)
                    {
                        ci.Label.Content = ci.StartActionControl.ActionImplementation.GetOutputConnectorPassCounter(ActionImplementation.Operator.Less).ToString();
                    }
                    else if (ci.StartConnector == ci.StartActionControl.SmallerEqual)
                    {
                        ci.Label.Content = ci.StartActionControl.ActionImplementation.GetOutputConnectorPassCounter(ActionImplementation.Operator.LessOrEqual).ToString();
                    }
                    else if (ci.StartConnector == ci.StartActionControl.NotEqual)
                    {
                        ci.Label.Content = ci.StartActionControl.ActionImplementation.GetOutputConnectorPassCounter(ActionImplementation.Operator.NotEqual).ToString();
                    }
                }
            }
            if (_activeActionListImplementations != null)
            {
                foreach (ActionImplementation actImpl in _activeActionListImplementations)
                {
                    if (actImpl.UIActionControl.ActionContent!=null)
                    {
                        actImpl.UIActionControl.InputCounter.Content = actImpl.GeocachesAtInputConnector.Count.ToString();
                    }
                }
            }
        }

        public void Clear(List<ActionImplementation> newActiveActionList)
        {
            _currentConnectionLine = null;
            _connectionInfo.Clear();
            foreach (var cntrl in dragCanvas.Children)
            {
                if (cntrl is ActionControl)
                {
                    (cntrl as ActionControl).ActionImplementation =null;
                }
            }
            dragCanvas.Children.Clear();
            ResetScale();
            ResetPosition();

            _activeActionListImplementations = newActiveActionList;
            if (_activeActionListImplementations != null)
            {
                foreach (var a in _activeActionListImplementations)
                {
                    AddActionControl(a);
                }
                //restore connection
                foreach (var a in _activeActionListImplementations)
                {
                    List<ActionImplementation> conList = a.GetOutputConnections(ActionImplementation.Operator.Equal);
                    Grid g = a.UIActionControl.Equal;
                    foreach (var c in conList)
                    {
                        addConnector(a, c, g);
                    }
                    conList = a.GetOutputConnections(ActionImplementation.Operator.Larger);
                    g = a.UIActionControl.Larger;
                    foreach (var c in conList)
                    {
                        addConnector(a, c, g);
                    }
                    conList = a.GetOutputConnections(ActionImplementation.Operator.LargerOrEqual);
                    g = a.UIActionControl.LargerEqual;
                    foreach (var c in conList)
                    {
                        addConnector(a, c, g);
                    }
                    conList = a.GetOutputConnections(ActionImplementation.Operator.Less);
                    g = a.UIActionControl.Smaller;
                    foreach (var c in conList)
                    {
                        addConnector(a, c, g);
                    }
                    conList = a.GetOutputConnections(ActionImplementation.Operator.LessOrEqual);
                    g = a.UIActionControl.SmallerEqual;
                    foreach (var c in conList)
                    {
                        addConnector(a, c, g);
                    }
                    conList = a.GetOutputConnections(ActionImplementation.Operator.NotEqual);
                    g = a.UIActionControl.NotEqual;
                    foreach (var c in conList)
                    {
                        addConnector(a, c, g);
                    }
                }
            }
        }

        private void addConnector(ActionImplementation a, ActionImplementation c, Grid g)
        {
            ConnectionInfo ci = new ConnectionInfo();
            ci.StartActionControl = a.UIActionControl;
            ci.StartConnector = g;
            ci.EndActionControl = c.UIActionControl;
            ci.EndConnector = c.UIActionControl.EntryPoint;

            Line el = new Line();
            el.Cursor = Cursors.Pen;
            el.RenderTransform = _translateTransform;
            Point p = g.TranslatePoint(new Point(g.ActualWidth / 2, g.ActualHeight), dragCanvas);
            el.X1 = p.X;
            el.Y1 = p.Y;
            p = c.UIActionControl.EntryPoint.TranslatePoint(new Point(c.UIActionControl.EntryPoint.ActualWidth / 2, c.UIActionControl.EntryPoint.ActualHeight), dragCanvas);
            el.X2 = p.X;
            el.Y2 = p.Y;
            el.Stroke = (g.Children[0] as Label).Foreground;
            el.StrokeThickness = 2.0;
            dragCanvas.Children.Insert(0, el);
            WPF.JoshSmith.Controls.DragCanvas.SetCanBeDragged(el, false);
            ci.ConnectionLine = el;
            _connectionInfo.Add(ci);
            el.MouseEnter += new MouseEventHandler(el_MouseEnter);
            el.MouseLeave += new MouseEventHandler(el_MouseLeave);
            el.ContextMenu = itemContextMenu;

            Label lb = new Label();
            ci.Label = lb;
            lb.FontWeight = FontWeights.Bold;
            lb.Background = new SolidColorBrush(Colors.LightGray);
            if (_showConnectionLabels)
            {
                lb.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                lb.Visibility = System.Windows.Visibility.Hidden;
            }
            lb.RenderTransform = _translateTransform;
            dragCanvas.Children.Add(lb);
            WPF.JoshSmith.Controls.DragCanvas.SetCanBeDragged(lb, false);
            el.LayoutUpdated += new EventHandler(el_LayoutUpdated);
            PositionConnectionLineLabel(ci);
        }

        void el_LayoutUpdated(object sender, EventArgs e)
        {
            if (_connectionInfo != null)
            {
                foreach (var ci in _connectionInfo)
                {
                    PositionConnectionLineLabel(ci);
                }
            }
        }

        private void PositionConnectionLineLabel(ConnectionInfo ci)
        {
            Canvas.SetLeft(ci.Label, ci.ConnectionLine.X1 + (ci.ConnectionLine.X2 - ci.ConnectionLine.X1) / 2);
            Canvas.SetTop(ci.Label, ci.ConnectionLine.Y1 + (ci.ConnectionLine.Y2 - ci.ConnectionLine.Y1) / 2);
        }

        public void ResetScale()
        {
            scaler.ScaleX = 1;
            scaler.ScaleY = scaler.ScaleX;
        }

        public void ResetPosition()
        {
            _translateTransform.X = 0;
            _translateTransform.Y = 0;
        }

        public void scaleUp()
        {
            scaler.ScaleX = 1.25 * scaler.ScaleX;
            scaler.ScaleY = scaler.ScaleX;
            _translateTransform.X -= (0.125 * this.ActualWidth / scaler.ScaleX);
            _translateTransform.Y -= (0.125 * this.ActualHeight / scaler.ScaleY);
        }

        public void scaleDown()
        {
            _translateTransform.X += (0.125 * this.ActualWidth / scaler.ScaleX);
            _translateTransform.Y += (0.125 * this.ActualHeight / scaler.ScaleY);
            scaler.ScaleX = scaler.ScaleX / 1.25;
            scaler.ScaleY = scaler.ScaleX;
        }

        public void moveUp()
        {
            _translateTransform.Y += this.ActualHeight / 3.0 / scaler.ScaleY;
        }

        public void moveDown()
        {
            _translateTransform.Y -= this.ActualHeight / 3.0 / scaler.ScaleY;
        }

        public void moveLeft()
        {
            _translateTransform.X += this.ActualWidth / 3.0 / scaler.ScaleX;
        }

        public void moveRight()
        {
            _translateTransform.X -= this.ActualWidth / 3.0 / scaler.ScaleX;
        }

        void dragCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_currentConnectionLine != null)
            {
                Point p = e.GetPosition(dragCanvas);
                _currentConnectionLine.X2 = p.X - _translateTransform.X;
                _currentConnectionLine.Y2 = p.Y - _translateTransform.Y;
            }
            else if (_doPan && _elementHover == null && e.LeftButton == MouseButtonState.Pressed)
            {
                Point p = e.GetPosition(dragCanvas);
                _translateTransform.X += p.X - _mouseDown.X;
                _translateTransform.Y += p.Y - _mouseDown.Y;
                _mouseDown = p;
            }
        }

        public void AddActionControl(ActionImplementation actImpl)
        {
            if (actImpl.Location.X == 0 && actImpl.Location.Y == 0)
            {
                actImpl.Location= new Point(-_translateTransform.X, -_translateTransform.Y);
            }
            ActionControl ac = new ActionControl();
            Canvas.SetLeft(ac, actImpl.Location.X);
            Canvas.SetTop(ac, actImpl.Location.Y);
            ac.ActionImplementation = actImpl;
            actImpl.UIActionControl = ac;
            dragCanvas.Children.Add(ac);
            ac.PreviewMouseRightButtonDown += UserControl_PreviewMouseRightButtonDown;
            WPF.JoshSmith.Controls.DragCanvas.SetCanBeDragged(ac, false);

            ac.Equal.MouseDown += new MouseButtonEventHandler(Output_MouseDown);
            ac.Smaller.MouseDown += new MouseButtonEventHandler(Output_MouseDown);
            ac.SmallerEqual.MouseDown += new MouseButtonEventHandler(Output_MouseDown);
            ac.Larger.MouseDown += new MouseButtonEventHandler(Output_MouseDown);
            ac.LargerEqual.MouseDown += new MouseButtonEventHandler(Output_MouseDown);
            ac.NotEqual.MouseDown += new MouseButtonEventHandler(Output_MouseDown);
            ac.EntryPoint.MouseDown += new MouseButtonEventHandler(EntryPoint_MouseDown);
            ac.LayoutUpdated += new EventHandler(ac_LayoutUpdated);
            ac.MouseEnter += new MouseEventHandler(ac_MouseEnter);
            ac.MouseLeave += new MouseEventHandler(ac_MouseLeave);

            ac.RenderTransform = _translateTransform;

            if (!(actImpl is ActionStart))
            {
                ac.ContextMenu = itemContextMenu;
            }
        }

        void ac_MouseLeave(object sender, MouseEventArgs e)
        {
            _elementHover = sender as ActionControl;
            if (_elementHover != null)
            {
                (sender as ActionControl).outlineBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
            }
            _elementHover = null;
        }

        void ac_MouseEnter(object sender, MouseEventArgs e)
        {
            _elementHover = sender as ActionControl;
            if (_elementHover != null)
            {
                (sender as ActionControl).outlineBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255));
            }
        }

        void EntryPoint_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Grid g = sender as Grid;
            if (e.ClickCount == 1 && g != null)
            {
                if (_currentConnectionLine != null)
                {
                    ConnectionInfo ci = (from c in _connectionInfo where c.ConnectionLine == _currentConnectionLine select c).FirstOrDefault();
                    ActionControl ac = FindParent<ActionControl>(g);
                    if (ci.StartActionControl == ac)
                    {
                        //connect to itself? nah
                        dragCanvas.Children.Remove(_currentConnectionLine);
                        dragCanvas.Children.Remove(ci.Label);
                        _connectionInfo.Remove(ci);
                    }
                    else
                    {
                        ci.EndActionControl = ac;
                        ci.EndConnector = g;
                        Point p = g.TranslatePoint(new Point(g.ActualWidth / 2, g.ActualHeight), dragCanvas);
                        _currentConnectionLine.X2 = p.X - _translateTransform.X;
                        _currentConnectionLine.Y2 = p.Y - _translateTransform.Y;

                        //data
                        if (!ci.StartActionControl.ActionImplementation.ConnectToOutput(ci.EndActionControl.ActionImplementation, ci.StartActionControl.GetOperator(ci.StartConnector.Name)))
                        {
                            //oeps...
                            dragCanvas.Children.Remove(_currentConnectionLine);
                            dragCanvas.Children.Remove(ci.Label);
                            _connectionInfo.Remove(ci);
                        }
                    }
                    _currentConnectionLine = null;
                }
            }
        }

        void ac_LayoutUpdated(object sender, EventArgs e)
        {
            if (_activeActionListImplementations != null)
            {
                foreach (ConnectionInfo ci in _connectionInfo)
                {
                    Grid g = ci.StartConnector as Grid;
                    if (g != null)
                    {
                        Point p = g.TranslatePoint(new Point(g.ActualWidth / 2, g.ActualHeight), dragCanvas);
                        ci.ConnectionLine.X1 = p.X - _translateTransform.X;
                        ci.ConnectionLine.Y1 = p.Y - _translateTransform.Y;
                    }
                    g = ci.EndConnector as Grid;
                    if (g != null)
                    {
                        Point p = g.TranslatePoint(new Point(g.ActualWidth / 2, g.ActualHeight), dragCanvas);
                        ci.ConnectionLine.X2 = p.X - _translateTransform.X;
                        ci.ConnectionLine.Y2 = p.Y - _translateTransform.Y;
                    }
                }
                foreach (ActionImplementation actImpl in _activeActionListImplementations)
                {
                    Point p = actImpl.UIActionControl.TranslatePoint(new Point(0, 0), dragCanvas);
                    actImpl.Location = p;
                }
            }
        }

        public static T FindParent<T>(DependencyObject child)
          where T : DependencyObject
        {
            //get parent item
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            //we've reached the end of the tree
            if (parentObject == null) return null;

            //check if the parent matches the type we're looking for
            T parent = parentObject as T;
            if (parent != null)
            {
                return parent;
            }
            else
            {
                return FindParent<T>(parentObject);
            }
        }
        void Output_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Grid g = sender as Grid;
            if (e.ClickCount == 1 && g != null)
            {
                if (_currentConnectionLine == null && g.Children.Count>0)
                {
                    ConnectionInfo ci = new ConnectionInfo();
                    ActionControl ac = FindParent<ActionControl>(g);
                    Line el = new Line();
                    el.RenderTransform = _translateTransform;
                    Point p = g.TranslatePoint(new Point(g.ActualWidth / 2, g.ActualHeight), dragCanvas);
                    el.X1 = p.X - _translateTransform.X;
                    el.Y1 = p.Y - _translateTransform.Y;
                    p = e.GetPosition(dragCanvas);
                    el.X2 = p.X - _translateTransform.X;
                    el.Y2 = p.Y - _translateTransform.Y;
                    el.Stroke = (g.Children[0] as Label).Foreground;
                    el.Cursor = Cursors.Pen;
                    el.StrokeThickness = 2.0;
                    _currentConnectionLine = el;
                    dragCanvas.Children.Insert(0,el);
                    WPF.JoshSmith.Controls.DragCanvas.SetCanBeDragged(el, false);
                    ci.ConnectionLine = el;
                    ci.StartActionControl = ac;
                    ci.StartConnector = g;
                    _connectionInfo.Add(ci);
                    el.MouseEnter += new MouseEventHandler(el_MouseEnter);
                    el.MouseLeave += new MouseEventHandler(el_MouseLeave);
                    el.ContextMenu = itemContextMenu;

                    Label lb = new Label();
                    ci.Label = lb;
                    lb.FontWeight = FontWeights.Bold;
                    lb.Background = new SolidColorBrush(Colors.LightGray);
                    if (_showConnectionLabels)
                    {
                        lb.Visibility = System.Windows.Visibility.Visible;
                    }
                    else
                    {
                        lb.Visibility = System.Windows.Visibility.Hidden;
                    }
                    lb.RenderTransform = _translateTransform;
                    dragCanvas.Children.Insert(0, lb);
                    WPF.JoshSmith.Controls.DragCanvas.SetCanBeDragged(lb, false);
                    el.LayoutUpdated += new EventHandler(el_LayoutUpdated);
                    PositionConnectionLineLabel(ci);

                }
                else
                {
                    dragCanvas.Children.Remove(_currentConnectionLine);
                    ConnectionInfo ci = (from c in _connectionInfo where c.ConnectionLine == _currentConnectionLine select c).FirstOrDefault();
                    _connectionInfo.Remove(ci);
                    dragCanvas.Children.Remove(ci.Label);
                    _currentConnectionLine = null;
                }
            }
        }

        void el_MouseLeave(object sender, MouseEventArgs e)
        {
            this._elementHover = null;
            Line el = (sender as Line);
            if (el != null)
            {
                //(g.Children[0] as Label).Foreground
                ConnectionInfo ci = (from c in _connectionInfo where c.ConnectionLine == el select c).FirstOrDefault();
                if (ci != null)
                {
                    el.Stroke = (ci.StartConnector.Children[0] as Label).Foreground;
                }
            }
        }

        void el_MouseEnter(object sender, MouseEventArgs e)
        {
            Line el = (sender as Line);
            this._elementHover = el;
            if (el != null)
            {
                el.Stroke = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255));
            }
        }

        void OnMenuItemClick(object sender, RoutedEventArgs e)
        {
            if (this._elementForContextMenu == null)
                return;

            if (e.Source == this.menuItemDelete)
            {
                Line cl = this._elementForContextMenu as Line;
                if (cl != null)
                {
                    ConnectionInfo ci = (from c in _connectionInfo where c.ConnectionLine == cl select c).FirstOrDefault();
                    _connectionInfo.Remove(ci);
                    dragCanvas.Children.Remove(cl);
                    dragCanvas.Children.Remove(ci.Label);

                    if (cl == _currentConnectionLine)
                    {
                        _currentConnectionLine = null;
                    }
                    else
                    {
                        //connenction remove in data
                        ci.StartActionControl.ActionImplementation.RemoveOutputConnection(ci.EndActionControl.ActionImplementation, ci.StartActionControl.GetOperator(ci.StartConnector.Name));
                    }
                }
                else
                {
                    ActionControl ac = this._elementForContextMenu as ActionControl;
                    if (ac != null)
                    {
                        //delete all connections
                        int index = 0;
                        while (index<_connectionInfo.Count)
                        {
                            ConnectionInfo ci = _connectionInfo[index];
                            if (ci.StartActionControl==ac && ci.ConnectionLine == _currentConnectionLine)
                            {
                                dragCanvas.Children.Remove(ci.ConnectionLine);
                                dragCanvas.Children.Remove(ci.Label);
                                _currentConnectionLine = null;
                                _connectionInfo.RemoveAt(index);
                                //not in data yet
                            }
                            else if (ci.EndActionControl == ac)
                            {
                                dragCanvas.Children.Remove(ci.ConnectionLine);
                                dragCanvas.Children.Remove(ci.Label);
                                _connectionInfo.RemoveAt(index);

                                //remove in data
                                ci.StartActionControl.ActionImplementation.RemoveOutputConnection(ci.EndActionControl.ActionImplementation, ci.StartActionControl.GetOperator(ci.StartConnector.Name));
                            }
                            else if (ci.StartActionControl == ac)
                            {
                                dragCanvas.Children.Remove(ci.ConnectionLine);
                                dragCanvas.Children.Remove(ci.Label);
                                _connectionInfo.RemoveAt(index);

                                //remove in data
                                ci.StartActionControl.ActionImplementation.RemoveOutputConnection(ci.EndActionControl.ActionImplementation, ci.StartActionControl.GetOperator(ci.StartConnector.Name));
                            }
                            else
                            {
                                index++;
                            }
                        }

                        //all connections are gone
                        //delete control
                        dragCanvas.Children.Remove(ac);

                        //remove from data
                        _activeActionListImplementations.Remove(ac.ActionImplementation);
                    }
                }
            }
        }

        private void UserControl_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            this._elementForContextMenu = _elementHover;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Escape)
            {
                OnKeyDown(System.Windows.Forms.Keys.Escape);
            }
        }

        public void OnKeyDown(System.Windows.Forms.Keys key)
        {
            if (key == System.Windows.Forms.Keys.Escape)
            {
                if (_currentConnectionLine != null)
                {
                    ConnectionInfo ci = (from c in _connectionInfo where c.ConnectionLine == _currentConnectionLine select c).FirstOrDefault();
                    dragCanvas.Children.Remove(_currentConnectionLine);
                    dragCanvas.Children.Remove(ci.Label);
                    _connectionInfo.Remove(ci);
                    _currentConnectionLine = null;
                }
            }
        }

        private void dragCanvas_MouseEnter(object sender, MouseEventArgs e)
        {
            dragCanvas.Focus();
        }

        private void dragCanvas_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                scaleUp();
            }
            else
            {
                scaleDown();
            }
        }

        private void dragCanvas_DragEnter(object sender, DragEventArgs e)
        {
            string ai = e.Data.GetData(typeof(string)) as string;
            if (!string.IsNullOrEmpty(ai))
            {
                e.Effects = DragDropEffects.Move;
            }
        }

        private void dragCanvas_Drop(object sender, DragEventArgs e)
        {
            if (_activeActionListImplementations != null && _activeActionListImplementations.Count>0)
            {
                string ai = e.Data.GetData(typeof(string)) as string;
                if (!string.IsNullOrEmpty(ai))
                {
                    Type t = Assembly.GetExecutingAssembly().GetType(ai);
                    ConstructorInfo constructor = t.GetConstructor(new Type[] { typeof(Framework.Interfaces.ICore) });
                    object[] parameters = new object[] { _activeActionListImplementations[0].Core };
                    ActionImplementation obj = (ActionImplementation)constructor.Invoke(parameters);
                    obj.ID = Guid.NewGuid().ToString("N");
                    _activeActionListImplementations.Add(obj);
                    Point p = e.GetPosition(this);
                    obj.Location = new Point(-_translateTransform.X + p.X / scaler.ScaleX, -_translateTransform.Y + p.Y / scaler.ScaleY);
                    AddActionControl(obj);
                }
            }
        }
    }
}
