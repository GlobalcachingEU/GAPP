using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Reflection;
using CSScriptLibrary;
using System.Threading;
using System.Windows.Media.Imaging;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.IO;
using System.Text.RegularExpressions;

namespace GlobalcachingApplication.Plugins.ActBuilder
{
    public class ActionStart: ActionImplementationCondition, System.Collections.IComparer
    {
        public const string STR_NAME = "Start";
        public const string STR_SORTON = "Sort geocaches on:";
        public const string STR_ASCENDING = "Ascending";
        public const string STR_DESCENDING = "Descending";
        public const string STR_SORT_NONE = "";
        public const string STR_SORT_DISTANCE = "Distance from center";
        public const string STR_SORT_TERRAIN = "Terrain";
        public const string STR_SORT_DIFFICULTY = "Difficulty";
        public const string STR_SORT_FAVORITE = "Favorite";
        public const string STR_SORT_PUBLISHED = "Hidden date";
        public ActionStart(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
        }
        public override UIElement GetUIElement()
        {
            if (Values.Count == 0)
            {
                Values.Add(STR_SORT_NONE);
            }
            if (Values.Count < 2)
            {
                Values.Add(STR_ASCENDING);
            }
            StackPanel sp = new StackPanel();
            Label l = new Label();
            l.Content = Utils.LanguageSupport.Instance.GetTranslation(STR_SORTON);
            l.HorizontalAlignment = HorizontalAlignment.Center;
            sp.Children.Add(l);
            ComboBox cb = CreateComboBox(new string[] { STR_SORT_NONE, STR_SORT_DISTANCE, STR_SORT_PUBLISHED, STR_SORT_FAVORITE, STR_SORT_DIFFICULTY, STR_SORT_TERRAIN }, Values[0]);
            sp.Children.Add(cb);
            ComboBox cb2 = CreateComboBox(new string[] { STR_ASCENDING, STR_DESCENDING }, Values[1]);
            sp.Children.Add(cb2);
            return sp;
        }
        public override void CommitUIData(UIElement uiElement)
        {
            ComboBox cb = (uiElement as StackPanel).Children[1] as ComboBox;
            Values[0] = cb.Text;
            cb = (uiElement as StackPanel).Children[2] as ComboBox;
            Values[1] = cb.Text;
        }
        public void PrepareFlow()
        {
            if (Values.Count > 1)
            {
                if (Values[0] != STR_SORT_NONE)
                {
                    Core.Geocaches.Sort(this);
                }
            }
        }
        public override bool AllowEntryPoint
        {
            get { return false; }
        }
        public override Operator AllowOperators
        {
            get { return Operator.Equal; }
        }
        public override Operator Process(Framework.Data.Geocache gc)
        {
            return Operator.Equal;
        }

        public int Compare(object x, object y)
        {
            int result = 0;
            Framework.Data.Geocache gc1 = x as Framework.Data.Geocache;
            Framework.Data.Geocache gc2 = y as Framework.Data.Geocache;
            if (gc1 != null && gc2 != null)
            {
                if (Values[0] == STR_SORT_DISTANCE)
                {
                    result = gc1.DistanceToCenter.CompareTo(gc2.DistanceToCenter);
                }
                else if (Values[0] == STR_SORT_DIFFICULTY)
                {
                    result = gc1.Difficulty.CompareTo(gc2.Difficulty);
                }
                else if (Values[0] == STR_SORT_TERRAIN)
                {
                    result = gc1.Terrain.CompareTo(gc2.Terrain);
                }
                else if (Values[0] == STR_SORT_FAVORITE)
                {
                    result = gc1.Favorites.CompareTo(gc2.Favorites);
                }
                else if (Values[0] == STR_SORT_PUBLISHED)
                {
                    result = gc1.PublishedTime.CompareTo(gc2.PublishedTime);
                }
                if (Values[1] == STR_DESCENDING)
                {
                    result = -result;
                }
            }
            return result;
        }
    }


    public class ActionScript : ActionImplementationCondition
    {
        public const string STR_NAME = "Script";
        private Assembly _compiledAssembly = null;
        private System.IO.TemporaryFile _scriptFile = null;
        private ActionImplementationCondition _scriptedImplementation = null;
        private string _scriptContent = "";
        public ActionScript(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
            _scriptFile = new System.IO.TemporaryFile();
        }
        public override UIElement GetUIElement()
        {
            TextBox tb = new TextBox();
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            tb.AcceptsReturn = true;
            tb.Width = 150;
            tb.Height = 30;
            tb.FontFamily = new System.Windows.Media.FontFamily("Courier new");
            tb.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            tb.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            tb.MouseEnter += new System.Windows.Input.MouseEventHandler(tb_MouseEnter);
            tb.MouseLeave += new System.Windows.Input.MouseEventHandler(tb_MouseLeave);
            if (Values.Count == 0)
            {
                using (System.IO.StreamReader textStreamReader = new System.IO.StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("GlobalcachingApplication.Plugins.ActBuilder.ConditionScriptTemplate.txt")))
                {
                    Values.Add(textStreamReader.ReadToEnd());
                }
            }
            tb.Text = Values[0];
            return tb;
        }

        void tb_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is TextBox)
            {
                (sender as TextBox).VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                (sender as TextBox).HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
                (sender as TextBox).Width = 150;
                (sender as TextBox).Height = 30;
            }
        }

        void tb_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is TextBox)
            {
                (sender as TextBox).VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                (sender as TextBox).HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                (sender as TextBox).Width = 400;
                (sender as TextBox).Height = 400;
            }
        }

        public override bool PrepareRun()
        {
            base.PrepareRun();
            if (Values.Count > 0)
            {
                if (_scriptContent != Values[0])
                {
                    //cannot cleanup, waste of resources, I know...
                    _scriptedImplementation = null;
                }
                if (_scriptedImplementation == null)
                {
                    _scriptContent = Values[0];
                    //build script
                    System.IO.File.WriteAllText(_scriptFile.Path, _scriptContent);
                    _compiledAssembly = CSScript.Load(_scriptFile.Path);
                    //create object
                    Type[] types = _compiledAssembly.GetTypes();
                    foreach (Type t in types)
                    {
                        if (t.IsClass && (t.BaseType == typeof(ActionImplementationCondition)))
                        {
                            ConstructorInfo constructor = t.GetConstructor(new Type[] { typeof(Framework.Interfaces.ICore) });
                            object[] parameters = new object[] { Core };
                            _scriptedImplementation = (ActionImplementationCondition)constructor.Invoke(parameters);

                            break;
                        }
                    }
                    _scriptedImplementation.PrepareRun();
                }
            }
            return true;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            TextBox tb = uiElement as TextBox;
            Values[0] = tb.Text;
        }
        public override Operator Process(Framework.Data.Geocache gc)
        {
            if (_scriptedImplementation != null)
            {
                _scriptedImplementation.GeocachesAtInputConnector.Add(gc.Code, gc);
                return _scriptedImplementation.Process(gc);
            }
            else
            {
                return 0;
            }
        }

        public override void FinalizeRun()
        {
            if (_scriptedImplementation != null)
            {
                _scriptedImplementation.FinalizeRun();
            }
        }

    }

    public class ActionNameContains : ActionImplementationCondition
    {
        public const string STR_NAME = "Name contains";
        private string _value = "";
        public ActionNameContains(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
        }
        public override UIElement GetUIElement()
        {
            TextBox tb = new TextBox();
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            if (Values.Count == 0)
            {
                Values.Add("-");
            }
            tb.Text = Values[0];
            return tb;
        }
        public override ActionImplementation.Operator AllowOperators
        {
            get
            {
                return ActionImplementation.Operator.Equal | ActionImplementation.Operator.NotEqual;
            }
        }
        public override void CommitUIData(UIElement uiElement)
        {
            TextBox tb = uiElement as TextBox;
            Values[0] = tb.Text;
        }
        public override bool PrepareRun()
        {
            _value = "";
            if (Values.Count > 0)
            {
                _value = Values[0];
            }
            return base.PrepareRun();
        }
        public override Operator Process(Framework.Data.Geocache gc)
        {
            int pos = -1;
            if (!string.IsNullOrEmpty(gc.Name))
            {
                pos = gc.Name.IndexOf(_value, StringComparison.InvariantCultureIgnoreCase);
            }
            if (pos < 0)
            {
                return Operator.NotEqual;
            }
            else
            {
                return Operator.Equal;
            }
        }
    }

    public class ActionNotesContains : ActionImplementationCondition
    {
        public const string STR_NAME = "Notes contains";
        private string _value = "";
        public ActionNotesContains(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
        }
        public override UIElement GetUIElement()
        {
            TextBox tb = new TextBox();
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            if (Values.Count == 0)
            {
                Values.Add("-");
            }
            tb.Text = Values[0];
            return tb;
        }
        public override ActionImplementation.Operator AllowOperators
        {
            get
            {
                return ActionImplementation.Operator.Equal | ActionImplementation.Operator.NotEqual;
            }
        }
        public override void CommitUIData(UIElement uiElement)
        {
            TextBox tb = uiElement as TextBox;
            Values[0] = tb.Text;
        }
        public override bool PrepareRun()
        {
            _value = "";
            if (Values.Count > 0)
            {
                _value = Values[0];
            }
            return base.PrepareRun();
        }
        public override Operator Process(Framework.Data.Geocache gc)
        {
            int pos = -1;
            if (!string.IsNullOrEmpty(gc.Notes))
            {
                pos = gc.Notes.IndexOf(_value, StringComparison.InvariantCultureIgnoreCase);
            }
            if (pos < 0 && !string.IsNullOrEmpty(gc.PersonaleNote))
            {
                pos = gc.PersonaleNote.IndexOf(_value, StringComparison.InvariantCultureIgnoreCase);
            }
            if (pos < 0)
            {
                return Operator.NotEqual;
            }
            else
            {
                return Operator.Equal;
            }
        }
    }

    public class ActionAttributes : ActionImplementationCondition
    {
        public const string STR_NAME = "Attributes";
        public const string STR_ALL = "All";
        private bool _all = false;
        private List<int> _ids = new List<int>();
        public ActionAttributes(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
        }
        public override ActionImplementation.Operator AllowOperators
        {
            get
            {
                return ActionImplementation.Operator.Equal | ActionImplementation.Operator.NotEqual;
            }
        }
        public override UIElement GetUIElement()
        {
            if (Values.Count == 0)
            {
                Values.Add(false.ToString());
            }

            StackPanel sp = new StackPanel();
            CheckBox cb = new CheckBox();
            cb.Content = Utils.LanguageSupport.Instance.GetTranslation(STR_ALL);
            cb.IsChecked = bool.Parse(Values[0]);
            sp.Children.Add(cb);
            ScrollViewer sv = new ScrollViewer();
            UniformGrid g = new UniformGrid();
            g.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            sv.Content = g;
            sv.CanContentScroll = true;
            sv.HorizontalScrollBarVisibility =  ScrollBarVisibility.Hidden;
            sv.VerticalScrollBarVisibility =  ScrollBarVisibility.Visible;
            sv.Width = 200;
            sv.Height = 120;
            foreach (Framework.Data.GeocacheAttribute attr in Core.GeocacheAttributes)
            {
                StackPanel ga = new StackPanel();
                ga.Width = 40;
                ga.Height = 60;
                Image img = new Image();
                img.ToolTip = Utils.LanguageSupport.Instance.GetTranslation(attr.Name);
                img.Source = new BitmapImage(new Uri(Utils.ImageSupport.Instance.GetImagePath(Core, Framework.Data.ImageSize.Default, attr, Framework.Data.GeocacheAttribute.State.Yes)));
                img.Width = 30;
                img.Height = 30;
                img.HorizontalAlignment = HorizontalAlignment.Center;
                img.VerticalAlignment = VerticalAlignment.Top;
                CheckBox acb = new CheckBox();
                acb.HorizontalAlignment = HorizontalAlignment.Center;
                acb.VerticalAlignment = VerticalAlignment.Top;
                acb.Tag = attr.ID;
                acb.IsChecked = Values.Contains(acb.Tag.ToString());
                ga.Children.Add(img);
                ga.Children.Add(acb);
                g.Children.Add(ga);
            }
            foreach (Framework.Data.GeocacheAttribute attr in Core.GeocacheAttributes)
            {
                if (attr.ID > 0)
                {
                    StackPanel ga = new StackPanel();
                    ga.Width = 40;
                    ga.Height = 60;
                    Image img = new Image();
                    img.ToolTip = Utils.LanguageSupport.Instance.GetTranslation(attr.Name);
                    img.Source = new BitmapImage(new Uri(Utils.ImageSupport.Instance.GetImagePath(Core, Framework.Data.ImageSize.Default, attr, Framework.Data.GeocacheAttribute.State.No)));
                    img.Width = 30;
                    img.Height = 30;
                    img.HorizontalAlignment = HorizontalAlignment.Center;
                    img.VerticalAlignment = VerticalAlignment.Top;
                    CheckBox acb = new CheckBox();
                    acb.HorizontalAlignment = HorizontalAlignment.Center;
                    acb.VerticalAlignment = VerticalAlignment.Top;
                    acb.Tag = -attr.ID;
                    acb.IsChecked = Values.Contains(acb.Tag.ToString());
                    ga.Children.Add(img);
                    ga.Children.Add(acb);
                    g.Children.Add(ga);
                }
            }
            sp.Children.Add(sv);

            sv.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            sv.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            sv.MouseEnter += new System.Windows.Input.MouseEventHandler(sv_MouseEnter);
            sv.MouseLeave += new System.Windows.Input.MouseEventHandler(sv_MouseLeave);

            return sp;
        }

        void sv_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is ScrollViewer)
            {
                (sender as ScrollViewer).VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                (sender as ScrollViewer).HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
                (sender as ScrollViewer).Width = 200;
                (sender as ScrollViewer).Height = 120;
            }
        }

        void sv_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is ScrollViewer)
            {
                (sender as ScrollViewer).VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                (sender as ScrollViewer).HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                (sender as ScrollViewer).Width = 400;
                (sender as ScrollViewer).Height = 400;
            }
        }

        public override void CommitUIData(UIElement uiElement)
        {
            Values.Clear();
            Values.Add(((uiElement as StackPanel).Children[0] as CheckBox).IsChecked.ToString());
            UniformGrid g = ((uiElement as StackPanel).Children[1] as ScrollViewer).Content as UniformGrid;
            foreach (StackPanel sp in g.Children)
            {
                CheckBox cb = sp.Children[1] as CheckBox;
                if (cb.IsChecked==true)
                {
                    Values.Add(cb.Tag.ToString());
                }
            }
        }
        public override bool PrepareRun()
        {
            if (Values.Count > 0)
            {
                _all = bool.Parse(Values[0]);
                _ids.Clear();
                for (int i = 1; i < Values.Count; i++)
                {
                    _ids.Add(int.Parse(Values[i]));
                }
            }
            return base.PrepareRun();
        }
        public override Operator Process(Framework.Data.Geocache gc)
        {
            int cnt = (from a in gc.AttributeIds
                       join b in _ids on a equals b
                       select a).Count();
            if (_all)
            {
                if (cnt == _ids.Count)
                {
                    return Operator.Equal;
                }
                else
                {
                    return Operator.NotEqual;
                }
            }
            else
            {
                if (cnt > 0)
                {
                    return Operator.Equal;
                }
                else
                {
                    return Operator.NotEqual;
                }
            }
        }
    }


    public class ActionGeocacheCount : ActionImplementationCondition
    {
        public const string STR_NAME = "Geocache counter";
        private int _value = 0;
        private int _counter = 0;
        public ActionGeocacheCount(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
        }
        public override UIElement GetUIElement()
        {
            TextBox tb = new TextBox();
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            if (Values.Count == 0)
            {
                Values.Add("30");
            }
            tb.Text = Values[0];
            return tb;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            TextBox tb = uiElement as TextBox;
            Values[0] = tb.Text;
        }
        public override bool PrepareRun()
        {
            _value = 0;
            _counter = 0;
            if (Values.Count > 0)
            {
                int.TryParse(Values[0], out _value);
            }
            return base.PrepareRun();
        }
        public override Operator Process(Framework.Data.Geocache gc)
        {
            _counter++;
            return GetOperators(_counter.CompareTo(_value));
        }
    }

    public class ActionWaypointCounter : ActionImplementationCondition
    {
        public const string STR_NAME = "Waypoint counter";
        public const string STR_COORDSONLY = "Only with coords";
        public const string STR_ADDGEOCACHE = "Count geocache as waypoint";
        private int _value = 0;
        private int _counter = 0;
        private bool _withCoordsOnly = true;
        private bool _addGeocache = true;
        public ActionWaypointCounter(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
        }
        public override UIElement GetUIElement()
        {
            if (Values.Count == 0)
            {
                Values.Add("30");
            }
            if (Values.Count < 2)
            {
                Values.Add(true.ToString());
            }
            if (Values.Count < 3)
            {
                Values.Add(true.ToString());
            }
            StackPanel sp = new StackPanel();
            TextBox tb = new TextBox();
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            sp.Children.Add(tb);
            tb.Text = Values[0];
            CheckBox cb = new CheckBox();
            cb.Content = Utils.LanguageSupport.Instance.GetTranslation(STR_COORDSONLY);
            cb.IsChecked = bool.Parse(Values[1]);
            sp.Children.Add(cb);
            CheckBox cb2 = new CheckBox();
            cb2.Content = Utils.LanguageSupport.Instance.GetTranslation(STR_ADDGEOCACHE);
            cb2.IsChecked = bool.Parse(Values[2]);
            sp.Children.Add(cb2);
            return sp;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            TextBox tb = (uiElement as StackPanel).Children[0] as TextBox;
            Values[0] = tb.Text;
            CheckBox cb = (uiElement as StackPanel).Children[1] as CheckBox;
            Values[1] = cb.IsChecked==null? false.ToString() : cb.IsChecked.ToString();
            cb = (uiElement as StackPanel).Children[2] as CheckBox;
            Values[2] = cb.IsChecked == null ? false.ToString() : cb.IsChecked.ToString();
        }
        public override bool PrepareRun()
        {
            _value = 0;
            _counter = 0;
            if (Values.Count > 0)
            {
                int.TryParse(Values[0], out _value);
            }
            if (Values.Count > 1)
            {
                bool.TryParse(Values[1], out _withCoordsOnly);
            }
            if (Values.Count > 2)
            {
                bool.TryParse(Values[2], out _addGeocache);
            }
            return base.PrepareRun();
        }
        public override Operator Process(Framework.Data.Geocache gc)
        {
            List<Framework.Data.Waypoint> wps = Utils.DataAccess.GetWaypointsFromGeocache(Core.Waypoints, gc.Code);
            if (_withCoordsOnly)
            {
                _counter += (from w in wps where w.Lat!=null && w.Lon!=null select w).Count();
            }
            else
            {
                _counter += wps.Count;
            }
            if (_addGeocache)
            {
                _counter++;
            }
            return GetOperators(_counter.CompareTo(_value));
        }
    }

    public class ActionUserWaypointCount : ActionImplementationCondition
    {
        public const string STR_NAME = "# User Waypoints";
        private int _value = 0;
        public ActionUserWaypointCount(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
        }
        public override UIElement GetUIElement()
        {
            TextBox tb = new TextBox();
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            if (Values.Count == 0)
            {
                Values.Add("1");
            }
            tb.Text = Values[0];
            return tb;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            TextBox tb = uiElement as TextBox;
            Values[0] = tb.Text;
        }
        public override bool PrepareRun()
        {
            _value = 0;
            if (Values.Count > 0)
            {
                int.TryParse(Values[0], out _value);
            }
            return base.PrepareRun();
        }
        public override Operator Process(Framework.Data.Geocache gc)
        {
            return GetOperators((from Framework.Data.UserWaypoint wp in Core.UserWaypoints
                                 where wp.GeocacheCode == gc.Code
                                 select wp).Count().CompareTo(_value));
        }
    }


    public class ActionGCVoteAverage : ActionImplementationCondition
    {
        public const string STR_NAME = "GCVote Average";
        private double _value = 0;
        public ActionGCVoteAverage(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
        }
        public override UIElement GetUIElement()
        {
            TextBox tb = new TextBox();
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            if (Values.Count == 0)
            {
                Values.Add("1");
            }
            tb.Text = Values[0];
            return tb;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            TextBox tb = uiElement as TextBox;
            Values[0] = tb.Text;
        }
        public override bool PrepareRun()
        {
            Framework.Interfaces.IPlugin p = Utils.PluginSupport.PluginByName(Core, "GlobalcachingApplication.Plugins.GCVote.Import");
            if (p != null)
            {
                MethodInfo m = p.GetType().GetMethod("Activate");
                if (m != null)
                {
                    m.Invoke(p, null);
                }
            }

            _value = 0;
            if (Values.Count > 0)
            {
                try
                {
                    _value = Utils.Conversion.StringToDouble(Values[0]);
                }
                catch
                {
                }
            }
            return base.PrepareRun();
        }
        public override Operator Process(Framework.Data.Geocache gc)
        {
            Operator result = 0;
            try
            {
                string gcvote = gc.GetCustomAttribute("GCVote") as string;
                if (!string.IsNullOrEmpty(gcvote))
                {
                    //x/y (z)
                    string sd = gcvote.Substring(0, gcvote.IndexOf('/'));
                    double v = Utils.Conversion.StringToDouble(sd);
                    result = GetOperators(v.CompareTo(_value));
                }
            }
            catch
            {
            }
            return result;
        }
    }

    public class ActionGCVoteCount : ActionImplementationCondition
    {
        public const string STR_NAME = "GCVote Count";
        private int _value = 0;
        public ActionGCVoteCount(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
        }
        public override UIElement GetUIElement()
        {
            TextBox tb = new TextBox();
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            if (Values.Count == 0)
            {
                Values.Add("1");
            }
            tb.Text = Values[0];
            return tb;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            TextBox tb = uiElement as TextBox;
            Values[0] = tb.Text;
        }
        public override bool PrepareRun()
        {
            Framework.Interfaces.IPlugin p = Utils.PluginSupport.PluginByName(Core, "GlobalcachingApplication.Plugins.GCVote.Import");
            if (p != null)
            {
                MethodInfo m = p.GetType().GetMethod("Activate");
                if (m != null)
                {
                    m.Invoke(p, null);
                }
            }

            _value = 0;
            if (Values.Count > 0)
            {
                int.TryParse(Values[0], out _value);
            }
            return base.PrepareRun();
        }
        public override Operator Process(Framework.Data.Geocache gc)
        {
            Operator result = 0;
            try
            {
                string gcvote = gc.GetCustomAttribute("GCVote") as string;
                if (!string.IsNullOrEmpty(gcvote))
                {
                    //x/y (z)
                    int pos1 = gcvote.IndexOf('/');
                    int pos2 = gcvote.IndexOf(' ');
                    string sd;
                    if (pos2 < 0)
                    {
                        sd = gcvote.Substring(pos1 + 1);
                    }
                    else
                    {
                        sd = gcvote.Substring(pos1 + 1, pos2 - pos1-1);
                    }
                    double v = Utils.Conversion.StringToDouble(sd);
                    result = GetOperators(v.CompareTo(_value));
                }
            }
            catch
            {
            }
            return result;
        }
    }

    public class ActionWaypointCount : ActionImplementationCondition
    {
        public const string STR_NAME = "# Waypoints";
        private int _value = 0;
        public ActionWaypointCount(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
        }
        public override UIElement GetUIElement()
        {
            TextBox tb = new TextBox();
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            if (Values.Count == 0)
            {
                Values.Add("1");
            }
            tb.Text = Values[0];
            return tb;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            TextBox tb = uiElement as TextBox;
            Values[0] = tb.Text;
        }
        public override bool PrepareRun()
        {
            _value = 0;
            if (Values.Count > 0)
            {
                int.TryParse(Values[0], out _value);
            }
            return base.PrepareRun();
        }
        public override Operator Process(Framework.Data.Geocache gc)
        {
            return GetOperators(Core.Waypoints.GetWaypoints(gc.Code).Count().CompareTo(_value));
        }
    }

    public class ActionLogCount : ActionImplementationCondition
    {
        public const string STR_NAME = "# Logs";
        private int _value = 0;
        public ActionLogCount(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
        }
        public override UIElement GetUIElement()
        {
            TextBox tb = new TextBox();
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            if (Values.Count == 0)
            {
                Values.Add("1");
            }
            tb.Text = Values[0];
            return tb;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            TextBox tb = uiElement as TextBox;
            Values[0] = tb.Text;
        }
        public override bool PrepareRun()
        {
            _value = 0;
            if (Values.Count > 0)
            {
                int.TryParse(Values[0], out _value);
            }
            return base.PrepareRun();
        }
        public override Operator Process(Framework.Data.Geocache gc)
        {
            return GetOperators(Core.Logs.GetLogs(gc.Code).Count().CompareTo(_value));
        }
    }

    public class ActionLogImagesCount : ActionImplementationCondition
    {
        public const string STR_NAME = "# Log images";
        private int _value = 0;
        public ActionLogImagesCount(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
        }
        public override UIElement GetUIElement()
        {
            TextBox tb = new TextBox();
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            if (Values.Count == 0)
            {
                Values.Add("1");
            }
            tb.Text = Values[0];
            return tb;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            TextBox tb = uiElement as TextBox;
            Values[0] = tb.Text;
        }
        public override bool PrepareRun()
        {
            _value = 0;
            if (Values.Count > 0)
            {
                int.TryParse(Values[0], out _value);
            }
            return base.PrepareRun();
        }
        public override Operator Process(Framework.Data.Geocache gc)
        {
            int count = 0;
            List<Framework.Data.Log> logs = Core.Logs.GetLogs(gc.Code);
            foreach (Framework.Data.Log l in logs)
            {
                count += Core.LogImages.GetLogImages(l.ID).Count;
            }
            return GetOperators(count.CompareTo(_value));
        }
    }

    public class ActionGcComNotes : ActionImplementationCondition
    {
        public const string STR_NAME = "gc.com Notes";
        public ActionGcComNotes(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
        }
        public override UIElement GetUIElement()
        {
            if (Values.Count == 0)
            {
                Values.Add(true.ToString());
            }
            ComboBox cb = CreateComboBox(new string[] { true.ToString(), false.ToString() }, Values[0]);
            return cb;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            ComboBox cb = uiElement as ComboBox;
            Values[0] = cb.Text;
        }
        public override ActionImplementation.Operator AllowOperators
        {
            get
            {
                return Operator.Equal | Operator.NotEqual;
            }
        }
        public override Operator Process(Framework.Data.Geocache gc)
        {
            Operator result = 0;
            if (Values.Count > 0)
            {
                bool value = false;
                if (bool.TryParse(Values[0], out value))
                {
                    if (string.IsNullOrEmpty(gc.PersonaleNote) == value)
                    {
                        result = Operator.NotEqual;
                    }
                    else
                    {
                        result = Operator.Equal;
                    }
                }
            }
            return result;
        }
    }

    public class ActionNotes : ActionImplementationCondition
    {
        public const string STR_NAME = "Notes";
        public ActionNotes(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
        }
        public override UIElement GetUIElement()
        {
            if (Values.Count == 0)
            {
                Values.Add(true.ToString());
            }
            ComboBox cb = CreateComboBox(new string[] { true.ToString(), false.ToString() }, Values[0]);
            return cb;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            ComboBox cb = uiElement as ComboBox;
            Values[0] = cb.Text;
        }
        public override ActionImplementation.Operator AllowOperators
        {
            get
            {
                return Operator.Equal | Operator.NotEqual;
            }
        }
        public override Operator Process(Framework.Data.Geocache gc)
        {
            Operator result = 0;
            if (Values.Count > 0)
            {
                bool value = false;
                if (bool.TryParse(Values[0], out value))
                {
                    if (gc.ContainsNote != value)
                    {
                        result = Operator.NotEqual;
                    }
                    else
                    {
                        result = Operator.Equal;
                    }
                }
            }
            return result;
        }
    }


    public class ActionFoundDate : ActionImplementationCondition
    {
        public const string STR_NAME = "Date of find";

        private DateTime _date = DateTime.MinValue;

        public ActionFoundDate(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
        }
        public override UIElement GetUIElement()
        {
            DatePicker dp = new DatePicker();
            dp.HorizontalAlignment = HorizontalAlignment.Center;
            if (Values.Count == 0)
            {
                Values.Add(DateTime.Now.ToString("s"));
            }
            dp.SelectedDate = DateTime.Parse(Values[0]);
            return dp;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            DatePicker dp = uiElement as DatePicker;
            if (dp.SelectedDate != null)
            {
                Values[0] = ((DateTime)dp.SelectedDate).ToString("s");
            }
            else
            {
                Values[0] = "";
            }
        }
        public override bool PrepareRun()
        {
            _date = DateTime.MinValue;
            if (Values.Count > 0)
            {
                _date = DateTime.Parse(Values[0]).Date;
            }
            return base.PrepareRun();
        }
        public override Operator Process(Framework.Data.Geocache gc)
        {
            if (gc.Found && gc.FoundDate!=null)
            {

                return GetOperators(((DateTime)gc.FoundDate).Date.CompareTo(_date));
            }
            else
            {
                return 0;
            }
        }
    }

    public class ActionYourFavorite : ActionImplementationCondition
    {
        public const string STR_NAME = "Your Favorite";
        private Framework.Interfaces.IPlugin _favoritesPlugin = null;
        private MethodInfo _method = null;
        private string[] _params = null;
        public ActionYourFavorite(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
        }
        public override UIElement GetUIElement()
        {
            if (Values.Count == 0)
            {
                Values.Add(true.ToString());
            }
            ComboBox cb = CreateComboBox(new string[] { true.ToString(), false.ToString() }, Values[0]);
            return cb;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            ComboBox cb = uiElement as ComboBox;
            Values[0] = cb.Text;
        }
        public override ActionImplementation.Operator AllowOperators
        {
            get
            {
                return Operator.Equal | Operator.NotEqual;
            }
        }
        public override bool PrepareRun()
        {
            _favoritesPlugin = Utils.PluginSupport.PluginByName(Core, "GlobalcachingApplication.Plugins.APIFavorites.SelectFavorites");
            if (_favoritesPlugin != null)
            {
                _method = _favoritesPlugin.GetType().GetMethod("GetFavorite");
                _params = new string[1];
            }
            else
            {
                _method = null;
            }
            return base.PrepareRun();
        }
        public override Operator Process(Framework.Data.Geocache gc)
        {
            Operator result = 0;
            if (Values.Count > 0)
            {
                bool value = false;
                if (_method != null)
                {
                    if (bool.TryParse(Values[0], out value))
                    {
                        _params[0] = gc.Code??"";
                        if ((bool)_method.Invoke(_favoritesPlugin,_params) != value)
                        {
                            result = Operator.NotEqual;
                        }
                        else
                        {
                            result = Operator.Equal;
                        }
                    }
                }
            }
            return result;
        }
    }


    public class ActionFound : ActionImplementationCondition
    {
        public const string STR_NAME = "Found";
        public ActionFound(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
        }
        public override UIElement GetUIElement()
        {
            if (Values.Count == 0)
            {
                Values.Add(true.ToString());
            }
            ComboBox cb = CreateComboBox(new string[] { true.ToString(), false.ToString() }, Values[0]);
            return cb;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            ComboBox cb = uiElement as ComboBox;
            Values[0] = cb.Text;
        }
        public override ActionImplementation.Operator AllowOperators
        {
            get
            {
                return Operator.Equal | Operator.NotEqual;
            }
        }
        public override Operator Process(Framework.Data.Geocache gc)
        {
            Operator result = 0;
            if (Values.Count > 0)
            {
                bool value = false;
                if (bool.TryParse(Values[0], out value))
                {
                    if (gc.Found != value)
                    {
                        result = Operator.NotEqual;
                    }
                    else
                    {
                        result = Operator.Equal;
                    }
                }
            }
            return result;
        }
    }

    public class ActionIsOwn : ActionImplementationCondition
    {
        public const string STR_NAME = "Is own";
        public ActionIsOwn(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
        }
        public override UIElement GetUIElement()
        {
            if (Values.Count == 0)
            {
                Values.Add(true.ToString());
            }
            ComboBox cb = CreateComboBox(new string[] { true.ToString(), false.ToString() }, Values[0]);
            return cb;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            ComboBox cb = uiElement as ComboBox;
            Values[0] = cb.Text;
        }
        public override ActionImplementation.Operator AllowOperators
        {
            get
            {
                return Operator.Equal | Operator.NotEqual;
            }
        }
        public override Operator Process(Framework.Data.Geocache gc)
        {
            Operator result = 0;
            if (Values.Count > 0)
            {
                bool value = false;
                if (bool.TryParse(Values[0], out value))
                {
                    if (gc.IsOwn != value)
                    {
                        result = Operator.NotEqual;
                    }
                    else
                    {
                        result = Operator.Equal;
                    }
                }
            }
            return result;
        }
    }

    public class ActionLocked : ActionImplementationCondition
    {
        public const string STR_NAME = "Locked";
        public ActionLocked(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
        }
        public override UIElement GetUIElement()
        {
            if (Values.Count == 0)
            {
                Values.Add(true.ToString());
            }
            ComboBox cb = CreateComboBox(new string[] { true.ToString(), false.ToString() }, Values[0]);
            return cb;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            ComboBox cb = uiElement as ComboBox;
            Values[0] = cb.Text;
        }
        public override ActionImplementation.Operator AllowOperators
        {
            get
            {
                return Operator.Equal | Operator.NotEqual;
            }
        }
        public override Operator Process(Framework.Data.Geocache gc)
        {
            Operator result = 0;
            if (Values.Count > 0)
            {
                bool value = false;
                if (bool.TryParse(Values[0], out value))
                {
                    if (gc.Locked != value)
                    {
                        result = Operator.NotEqual;
                    }
                    else
                    {
                        result = Operator.Equal;
                    }
                }
            }
            return result;
        }
    }

    public class ActionSelected : ActionImplementationCondition
    {
        public const string STR_NAME = "Selected";
        public ActionSelected(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
        }
        public override UIElement GetUIElement()
        {
            if (Values.Count == 0)
            {
                Values.Add(true.ToString());
            }
            ComboBox cb = CreateComboBox(new string[] { true.ToString(), false.ToString() }, Values[0]);
            return cb;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            ComboBox cb = uiElement as ComboBox;
            Values[0] = cb.Text;
        }
        public override ActionImplementation.Operator AllowOperators
        {
            get
            {
                return Operator.Equal | Operator.NotEqual;
            }
        }
        public override Operator Process(Framework.Data.Geocache gc)
        {
            Operator result = 0;
            if (Values.Count > 0)
            {
                bool value = false;
                if (bool.TryParse(Values[0], out value))
                {
                    if (gc.Selected != value)
                    {
                        result = Operator.NotEqual;
                    }
                    else
                    {
                        result = Operator.Equal;
                    }
                }
            }
            return result;
        }
    }


    public class ActionFlagged : ActionImplementationCondition
    {
        public const string STR_NAME = "Flagged";
        public ActionFlagged(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
        }
        public override UIElement GetUIElement()
        {
            if (Values.Count == 0)
            {
                Values.Add(true.ToString());
            }
            ComboBox cb = CreateComboBox(new string[] { true.ToString(), false.ToString() }, Values[0]);
            return cb;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            ComboBox cb = uiElement as ComboBox;
            Values[0] = cb.Text;
        }
        public override ActionImplementation.Operator AllowOperators
        {
            get
            {
                return Operator.Equal | Operator.NotEqual;
            }
        }
        public override Operator Process(Framework.Data.Geocache gc)
        {
            Operator result = 0;
            if (Values.Count > 0)
            {
                bool value = false;
                if (bool.TryParse(Values[0], out value))
                {
                    if (gc.Flagged != value)
                    {
                        result = Operator.NotEqual;
                    }
                    else
                    {
                        result = Operator.Equal;
                    }
                }
            }
            return result;
        }
    }

    public class ActionNotSaved : ActionImplementationCondition
    {
        public const string STR_NAME = "Not saved";
        public ActionNotSaved(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
        }
        public override UIElement GetUIElement()
        {
            if (Values.Count == 0)
            {
                Values.Add(true.ToString());
            }
            ComboBox cb = CreateComboBox(new string[] { true.ToString(), false.ToString() }, Values[0]);
            return cb;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            ComboBox cb = uiElement as ComboBox;
            Values[0] = cb.Text;
        }
        public override ActionImplementation.Operator AllowOperators
        {
            get
            {
                return Operator.Equal | Operator.NotEqual;
            }
        }
        public override Operator Process(Framework.Data.Geocache gc)
        {
            Operator result = 0;
            if (Values.Count > 0)
            {
                bool value = false;
                if (bool.TryParse(Values[0], out value))
                {
                    if (gc.Saved == value)
                    {
                        result = Operator.NotEqual;
                    }
                    else
                    {
                        result = Operator.Equal;
                    }
                }
            }
            return result;
        }
    }


    public class ActionFavorites : ActionImplementationCondition
    {
        public const string STR_NAME = "Favorites";
        private int _value = 0;
        public ActionFavorites(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
        }
        public override UIElement GetUIElement()
        {
            TextBox tb = new TextBox();
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            if (Values.Count == 0)
            {
                Values.Add("1");
            }
            tb.Text = Values[0];
            return tb;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            TextBox tb = uiElement as TextBox;
            Values[0] = tb.Text;
        }
        public override bool PrepareRun()
        {
            _value = 0;
            if (Values.Count > 0)
            {
                int.TryParse(Values[0], out _value);
            }
            return base.PrepareRun();
        }
        public override Operator Process(Framework.Data.Geocache gc)
        {
            return GetOperators(gc.Favorites.CompareTo(_value));
        }
    }

    public class ActionCustomCoords : ActionImplementationCondition
    {
        public const string STR_NAME = "Custom coods";
        public ActionCustomCoords(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
        }
        public override UIElement GetUIElement()
        {
            if (Values.Count == 0)
            {
                Values.Add(true.ToString());
            }
            ComboBox cb = CreateComboBox(new string[] { true.ToString(), false.ToString() }, Values[0]);
            return cb;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            ComboBox cb = uiElement as ComboBox;
            Values[0] = cb.Text;
        }
        public override ActionImplementation.Operator AllowOperators
        {
            get
            {
                return Operator.Equal | Operator.NotEqual;
            }
        }
        public override Operator Process(Framework.Data.Geocache gc)
        {
            Operator result = 0;
            if (Values.Count > 0)
            {
                bool value = false;
                if (bool.TryParse(Values[0], out value))
                {
                    if (gc.ContainsCustomLatLon != value)
                    {
                        result = Operator.NotEqual;
                    }
                    else
                    {
                        result = Operator.Equal;
                    }
                }
            }
            return result;
        }
    }


    public class ActionMemberOnly : ActionImplementationCondition
    {
        public const string STR_NAME = "Member only";
        public ActionMemberOnly(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
        }
        public override UIElement GetUIElement()
        {
            if (Values.Count == 0)
            {
                Values.Add(true.ToString());
            }
            ComboBox cb = CreateComboBox(new string[] { true.ToString(), false.ToString() }, Values[0]);
            return cb;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            ComboBox cb = uiElement as ComboBox;
            Values[0] = cb.Text;
        }
        public override ActionImplementation.Operator AllowOperators
        {
            get
            {
                return Operator.Equal | Operator.NotEqual;
            }
        }
        public override Operator Process(Framework.Data.Geocache gc)
        {
            Operator result = 0;
            if (Values.Count > 0)
            {
                bool value = false;
                if (bool.TryParse(Values[0], out value))
                {
                    if (gc.MemberOnly != value)
                    {
                        result = Operator.NotEqual;
                    }
                    else
                    {
                        result = Operator.Equal;
                    }
                }
            }
            return result;
        }
    }

    public class ActionDifficulty : ActionImplementationCondition
    {
        public const string STR_NAME = "Difficulty";
        public ActionDifficulty(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
        }
        public override UIElement GetUIElement()
        {
            if (Values.Count == 0)
            {
                Values.Add("1");
            }
            ComboBox cb = CreateComboBox(new string[] { "1", "1.5", "2", "2.5", "3", "3.5", "4", "4.5", "5" }, Values[0]);
            return cb;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            ComboBox cb = uiElement as ComboBox;
            Values[0] = cb.Text;
        }
        public override Operator Process(Framework.Data.Geocache gc)
        {
            return GetOperators(gc.Difficulty.ToString("0.#").Replace(',', '.'), Values.Count >= 0 ? Values[0] : "");
        }
    }

    public class ActionTerrain : ActionImplementationCondition
    {
        public const string STR_NAME = "Terrain";
        public ActionTerrain(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
        }
        public override UIElement GetUIElement()
        {
            if (Values.Count == 0)
            {
                Values.Add("1");
            }
            ComboBox cb = CreateComboBox(new string[] { "1", "1.5", "2", "2.5", "3", "3.5", "4", "4.5", "5" }, Values[0]);
            return cb;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            ComboBox cb = uiElement as ComboBox;
            Values[0] = cb.Text;
        }
        public override Operator Process(Framework.Data.Geocache gc)
        {
            return GetOperators(gc.Terrain.ToString("0.#").Replace(',','.'), Values.Count >= 0 ? Values[0] : "");
        }
    }

    public class ActionContainer : ActionImplementationCondition
    {
        public const string STR_NAME = "Container";
        public ActionContainer(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
        }
        public override ActionImplementation.Operator AllowOperators
        {
            get
            {
                return Operator.Equal | Operator.NotEqual;
            }
        }
        public override UIElement GetUIElement()
        {
            if (Values.Count == 0)
            {
                Values.Add("");
            }
            ComboBox cb = CreateComboBox((from g in Core.GeocacheContainers select g.Name).OrderBy(x => x).ToArray(), Values[0]);
            return cb;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            ComboBox cb = uiElement as ComboBox;
            Values[0] = cb.Text;
        }
        public override Operator Process(Framework.Data.Geocache gc)
        {
            return GetOperators(gc.Container.Name ?? "", Values.Count >= 0 ? Values[0] : "");
        }
    }

    public class ActionOwner : ActionImplementationCondition
    {
        public const string STR_NAME = "Owner";
        public ActionOwner(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
        }
        public override UIElement GetUIElement()
        {
            if (Values.Count == 0)
            {
                Values.Add("");
            }
            //test
            //List<string> tst = new List<string>();
            //for (int i = 0; i < 100; i++)
            //{
            //    tst.AddRange((from Framework.Data.Geocache gc in Core.Geocaches where !string.IsNullOrEmpty(gc.Owner) select gc.Owner).Distinct().OrderBy(x => x).ToArray());
            //}
            //ComboBox cb = CreateComboBox(tst.Take(1000).ToArray(), Values[0]);
            //end test

            ComboBox cb = CreateComboBox((from Framework.Data.Geocache gc in Core.Geocaches where !string.IsNullOrEmpty(gc.Owner) select gc.Owner).Distinct().OrderBy(x => x).Take(1000).ToArray(), Values[0]);
            return cb;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            ComboBox cb = uiElement as ComboBox;
            Values[0] = cb.Text;
        }
        public override Operator Process(Framework.Data.Geocache gc)
        {
            return GetOperators(gc.Owner ?? "", Values.Count >= 0 ? Values[0] : "");
        }
    }

    public class ActionPlacedBy : ActionImplementationCondition
    {
        public const string STR_NAME = "Placed by";
        public ActionPlacedBy(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
        }
        public override UIElement GetUIElement()
        {
            if (Values.Count == 0)
            {
                Values.Add("");
            }
            ComboBox cb = CreateComboBox((from Framework.Data.Geocache gc in Core.Geocaches where !string.IsNullOrEmpty(gc.PlacedBy) select gc.PlacedBy).Distinct().OrderBy(x => x).Take(1000).ToArray(), Values[0]);
            return cb;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            ComboBox cb = uiElement as ComboBox;
            Values[0] = cb.Text;
        }
        public override Operator Process(Framework.Data.Geocache gc)
        {
            return GetOperators(gc.PlacedBy ?? "", Values.Count >= 0 ? Values[0] : "");
        }
    }

    public class ActionInCollection : ActionImplementationCondition
    {
        public const string STR_NAME = "In collection";
        public ActionInCollection(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
        }
        public override UIElement GetUIElement()
        {
            if (Values.Count == 0)
            {
                Values.Add("");
            }
            ComboBox cb = CreateComboBox(Utils.GeocacheCollectionSupport.Instance.AvailableCollections().ToArray(), Values[0]);
            return cb;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            ComboBox cb = uiElement as ComboBox;
            Values[0] = cb.Text;
        }
        public override ActionImplementation.Operator AllowOperators
        {
            get
            {
                return Operator.Equal | Operator.NotEqual;
            }
        }
        public override Operator Process(Framework.Data.Geocache gc)
        {
            if (Utils.GeocacheCollectionSupport.Instance.InCollection(Values[0], gc.Code))
            {
                return Operator.Equal;
            }
            else
            {
                return Operator.NotEqual;
            }
        }
    }

    public class ActionGeocacheType : ActionImplementationCondition
    {
        public const string STR_NAME = "Geocache type";
        public ActionGeocacheType(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
        }
        public override ActionImplementation.Operator AllowOperators
        {
            get
            {
                return Operator.Equal | Operator.NotEqual;
            }
        }
        public override UIElement GetUIElement()
        {
            if (Values.Count == 0)
            {
                Values.Add("");
            }
            ComboBox cb = CreateComboBox((from g in Core.GeocacheTypes select g.Name).OrderBy(x => x).ToArray(), Values[0]);
            return cb;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            ComboBox cb = uiElement as ComboBox;
            Values[0] = cb.Text;
        }
        public override Operator Process(Framework.Data.Geocache gc)
        {
            return GetOperators(gc.GeocacheType.Name ?? "", Values.Count >= 0 ? Values[0] : "");
        }
    }


    public class ActionArchived : ActionImplementationCondition
    {
        public const string STR_NAME = "Archived";
        public ActionArchived(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
        }
        public override UIElement GetUIElement()
        {
            if (Values.Count == 0)
            {
                Values.Add(true.ToString());
            }
            ComboBox cb = CreateComboBox(new string[] { true.ToString(), false.ToString() }, Values[0]);
            return cb;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            ComboBox cb = uiElement as ComboBox;
            Values[0] = cb.Text;
        }
        public override ActionImplementation.Operator AllowOperators
        {
            get
            {
                return Operator.Equal | Operator.NotEqual;
            }
        }
        public override Operator Process(Framework.Data.Geocache gc)
        {
            Operator result = 0;
            if (Values.Count > 0)
            {
                bool value = false;
                if (bool.TryParse(Values[0], out value))
                {
                    if (gc.Archived != value)
                    {
                        result = Operator.NotEqual;
                    }
                    else
                    {
                        result = Operator.Equal;
                    }
                }
            }
            return result;
        }
    }

    public class ActionAvailable : ActionImplementationCondition
    {
        public const string STR_NAME = "Available";
        public ActionAvailable(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
        }
        public override UIElement GetUIElement()
        {
            if (Values.Count == 0)
            {
                Values.Add(true.ToString());
            }
            ComboBox cb = CreateComboBox(new string[] { true.ToString(), false.ToString() }, Values[0]);
            return cb;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            ComboBox cb = uiElement as ComboBox;
            Values[0] = cb.Text;
        }
        public override ActionImplementation.Operator AllowOperators
        {
            get
            {
                return Operator.Equal | Operator.NotEqual;
            }
        }
        public override Operator Process(Framework.Data.Geocache gc)
        {
            Operator result = 0;
            if (Values.Count > 0)
            {
                bool value = false;
                if (bool.TryParse(Values[0], out value))
                {
                    if (gc.Available != value)
                    {
                        result = Operator.NotEqual;
                    }
                    else
                    {
                        result = Operator.Equal;
                    }
                }
            }
            return result;
        }
    }

    public class ActionDistanceToLocation : ActionImplementationCondition
    {
        public const string STR_NAME = "Distance to location (km)";
        private double _value = 0.0;
        private Framework.Data.Location _loc = null;
        public ActionDistanceToLocation(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
        }
        public override UIElement GetUIElement()
        {
            /*
             * ------------------------
             * |  N51.33.... | [...]  |
             * |         10           |
             * ------------------------
             */
            StackPanel sp = new StackPanel();

            Grid g = new Grid();
            TextBox tb = new TextBox();
            tb.HorizontalAlignment = HorizontalAlignment.Left;
            tb.Width = 180;
            if (Values.Count == 0)
            {
                Values.Add(Utils.Conversion.GetCoordinatesPresentation(Core.CenterLocation));
            }
            tb.Text = Values[0];
            g.Children.Add(tb);

            Button b = new Button();
            b.Content = "...";
            b.HorizontalAlignment = HorizontalAlignment.Right;
            b.Click += new RoutedEventHandler(bActionDistanceToLocation_Click);
            g.Children.Add(b);

            sp.Children.Add(g);

            tb = new TextBox();
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            if (Values.Count < 2)
            {
                Values.Add("10.0");
            }
            tb.Text = Values[1];
            sp.Children.Add(tb);
            return sp;
        }

        void bActionDistanceToLocation_Click(object sender, RoutedEventArgs e)
        {
            using (Utils.Dialogs.GetLocationForm dlg = new Utils.Dialogs.GetLocationForm(Core, Core.CenterLocation))
            {
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    (((sender as Button).Parent as Grid).Children[0] as TextBox).Text = Utils.Conversion.GetCoordinatesPresentation(dlg.Result);
                }
            }
        }

        public override void CommitUIData(UIElement uiElement)
        {
            Values[0] = (((uiElement as StackPanel).Children[0] as Grid).Children[0] as TextBox).Text;
            Values[1] = ((uiElement as StackPanel).Children[1] as TextBox).Text;
        }
        public override bool PrepareRun()
        {
            if (Values.Count > 1)
            {
                try
                {
                    _loc = Utils.Conversion.StringToLocation(Values[0]);
                    _value = Utils.Conversion.StringToDouble(Values[1]);
                }
                catch
                {
                }
            }
            return base.PrepareRun();
        }
        public override Operator Process(Framework.Data.Geocache gc)
        {
            if (_loc != null)
            {
                try
                {
                    double dist = Utils.Calculus.CalculateDistance(gc, _loc).EllipsoidalDistance / 1000.0;
                    return GetOperators(dist.CompareTo(_value));
                }
                catch
                {
                    return Operator.Larger;
                }
            }
            else
            {
                return 0;
            }
        }
    }

    public class ActionDistanceToCenter : ActionImplementationCondition
    {
        public const string STR_NAME = "Distance to center (km)";
        private double _value = 0.0;
        public ActionDistanceToCenter(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
        }
        public override UIElement GetUIElement()
        {
            TextBox tb = new TextBox();
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            if (Values.Count == 0)
            {
                Values.Add("1.0");
            }
            tb.Text = Values[0];
            return tb;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            TextBox tb = uiElement as TextBox;
            Values[0] = tb.Text;
        }
        public override bool PrepareRun()
        {
            if (Values.Count > 0)
            {
                try
                {
                    _value = Utils.Conversion.StringToDouble(Values[0]);
                }
                catch
                {
                }
            }
            return base.PrepareRun();
        }
        public override Operator Process(Framework.Data.Geocache gc)
        {
            try
            {
                double dist = Utils.Calculus.CalculateDistance(gc, Core.CenterLocation).EllipsoidalDistance / 1000.0;
                return GetOperators(dist.CompareTo(_value));
            }
            catch
            {
                return Operator.Larger;
            }
        }
    }


    public class ActionLon : ActionImplementationCondition
    {
        public const string STR_NAME = "Longitude";
        private double _value = 0.0;
        public ActionLon(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
        }
        public override UIElement GetUIElement()
        {
            TextBox tb = new TextBox();
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            if (Values.Count == 0)
            {
                Values.Add("1.0");
            }
            tb.Text = Values[0];
            return tb;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            TextBox tb = uiElement as TextBox;
            Values[0] = tb.Text;
        }
        public override bool PrepareRun()
        {
            if (Values.Count > 0)
            {
                try
                {
                    _value = Utils.Conversion.StringToDouble(Values[0]);
                }
                catch
                {
                }
            }
            return base.PrepareRun();
        }
        public override Operator Process(Framework.Data.Geocache gc)
        {
            return GetOperators(gc.Lon.CompareTo(_value));
        }
    }

    public class ActionLat : ActionImplementationCondition
    {
        public const string STR_NAME = "Latitude";
        private double _value = 0.0;
        public ActionLat(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
        }
        public override UIElement GetUIElement()
        {
            TextBox tb = new TextBox();
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            if (Values.Count == 0)
            {
                Values.Add("1.0");
            }
            tb.Text = Values[0];
            return tb;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            TextBox tb = uiElement as TextBox;
            Values[0] = tb.Text;
        }
        public override bool PrepareRun()
        {
            if (Values.Count > 0)
            {
                try
                {
                    _value = Utils.Conversion.StringToDouble(Values[0]);
                }
                catch
                {
                }
            }
            return base.PrepareRun();
        }
        public override Operator Process(Framework.Data.Geocache gc)
        {
            return GetOperators(gc.Lat.CompareTo(_value));
        }
    }

    public class ActionUpdatedDaysAgo : ActionImplementationCondition
    {
        public const string STR_NAME = "Updated days ago";
        private int _value = 0;
        public ActionUpdatedDaysAgo(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
        }
        public override UIElement GetUIElement()
        {
            TextBox tb = new TextBox();
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            if (Values.Count == 0)
            {
                Values.Add("7");
            }
            tb.Text = Values[0];
            return tb;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            TextBox tb = uiElement as TextBox;
            Values[0] = tb.Text;
        }
        public override bool PrepareRun()
        {
            double v = 0.0;
            _value = 0;
            if (Values.Count > 0)
            {
                double.TryParse(Values[0], out v);
                _value = (int)v;
            }
            return base.PrepareRun();
        }
        public override Operator Process(Framework.Data.Geocache gc)
        {
            return GetOperators(((int)((DateTime.Now.Date - gc.DataFromDate.Date).TotalDays)).CompareTo(_value));
        }
    }


    public class ActionDataFromDate : ActionImplementationCondition
    {
        public const string STR_NAME = "Date of data";

        private DateTime _date = DateTime.MinValue;
        
        public ActionDataFromDate(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
        }
        public override UIElement GetUIElement()
        {
            DatePicker dp = new DatePicker();
            dp.HorizontalAlignment = HorizontalAlignment.Center;
            if (Values.Count == 0)
            {
                Values.Add(DateTime.Now.ToString("s"));
            }
            dp.SelectedDate = DateTime.Parse(Values[0]);
            return dp;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            DatePicker dp = uiElement as DatePicker;
            if (dp.SelectedDate != null)
            {
                Values[0] = ((DateTime)dp.SelectedDate).ToString("s");
            }
            else
            {
                Values[0] = "";
            }
        }
        public override bool PrepareRun()
        {
            _date = DateTime.MinValue;
            if (Values.Count > 0)
            {
                _date = DateTime.Parse(Values[0]).Date;
            }
            return base.PrepareRun();
        }
        public override Operator Process(Framework.Data.Geocache gc)
        {
            return GetOperators(gc.DataFromDate.Date.CompareTo(_date));
        }
    }

    public class ActionPublishedDaysAgo : ActionImplementationCondition
    {
        public const string STR_NAME = "Published days ago";
        private int _value = 0;
        public ActionPublishedDaysAgo(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
        }
        public override UIElement GetUIElement()
        {
            TextBox tb = new TextBox();
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            if (Values.Count == 0)
            {
                Values.Add("7");
            }
            tb.Text = Values[0];
            return tb;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            TextBox tb = uiElement as TextBox;
            Values[0] = tb.Text;
        }
        public override bool PrepareRun()
        {
            double v = 0.0;
            _value = 0;
            if (Values.Count > 0)
            {
                double.TryParse(Values[0], out v);
                _value = (int)v;
            }
            return base.PrepareRun();
        }
        public override Operator Process(Framework.Data.Geocache gc)
        {
            return GetOperators(((int)((DateTime.Now.Date - gc.PublishedTime.Date).TotalDays)).CompareTo(_value));
        }
    }


    public class ActionPublished : ActionImplementationCondition
    {
        public const string STR_NAME = "Published";

        private DateTime _date = DateTime.MinValue;
        
        public ActionPublished(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
        }
        public override UIElement GetUIElement()
        {
            DatePicker dp = new DatePicker();
            dp.HorizontalAlignment = HorizontalAlignment.Center;
            if (Values.Count == 0)
            {
                Values.Add(DateTime.Now.ToString("s"));
            }
            dp.SelectedDate = DateTime.Parse(Values[0]);
            return dp;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            DatePicker dp = uiElement as DatePicker;
            if (dp.SelectedDate != null)
            {
                Values[0] = ((DateTime)dp.SelectedDate).ToString("s");
            }
            else
            {
                Values[0] = "";
            }
        }
        public override bool PrepareRun()
        {
            _date = DateTime.MinValue;
            if (Values.Count > 0)
            {
                _date = DateTime.Parse(Values[0]).Date;
            }
            return base.PrepareRun();
        }

        public override Operator Process(Framework.Data.Geocache gc)
        {
            return GetOperators(gc.PublishedTime.Date.CompareTo(_date));
        }
    }

    public class ActionCode : ActionImplementationCondition
    {
        public const string STR_NAME = "Code";
        public ActionCode(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
        }
        public override UIElement GetUIElement()
        {
            if (Values.Count == 0)
            {
                Values.Add("");
            }
            ComboBox cb = CreateComboBox(new string[]{"GC"}, Values[0]);
            return cb;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            ComboBox cb = uiElement as ComboBox;
            Values[0] = cb.Text;
        }
        public override Operator Process(Framework.Data.Geocache gc)
        {
            return GetOperators(gc.Code ?? "", Values.Count >= 0 ? Values[0] : "");
        }
    }

    public class ActionCountry : ActionImplementationCondition
    {
        public const string STR_NAME = "Country";
        public ActionCountry(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
        }
        public override UIElement GetUIElement()
        {
            if (Values.Count == 0)
            {
                Values.Add("");
            }
            ComboBox cb = CreateComboBox((from Framework.Data.Geocache gc in Core.Geocaches where !string.IsNullOrEmpty(gc.Country) select gc.Country).Distinct().OrderBy(x => x).ToArray(), Values[0]);
            return cb;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            ComboBox cb = uiElement as ComboBox;
            Values[0] = cb.Text;
        }
        public override Operator Process(Framework.Data.Geocache gc)
        {
            return GetOperators(gc.Country ?? "", Values.Count >= 0 ? Values[0] : "");
        }
    }

    public class ActionState : ActionImplementationCondition
    {
        public const string STR_NAME = "State";
        public ActionState(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
        }
        public override UIElement GetUIElement()
        {
            if (Values.Count == 0)
            {
                Values.Add("");
            }
            ComboBox cb = CreateComboBox((from Framework.Data.Geocache gc in Core.Geocaches where !string.IsNullOrEmpty(gc.State) select gc.State).Distinct().OrderBy(x => x).ToArray(), Values[0]);
            return cb;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            ComboBox cb = uiElement as ComboBox;
            Values[0] = cb.Text;
        }
        public override Operator Process(Framework.Data.Geocache gc)
        {
            return GetOperators(gc.State ?? "", Values.Count >= 0 ? Values[0] : "");
        }
    }

    public class ActionMunicipality : ActionImplementationCondition
    {
        public const string STR_NAME = "Municipality";
        public ActionMunicipality(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
        }
        public override UIElement GetUIElement()
        {
            if (Values.Count == 0)
            {
                Values.Add("");
            }
            ComboBox cb = CreateComboBox((from Framework.Data.Geocache gc in Core.Geocaches where !string.IsNullOrEmpty(gc.Municipality) select gc.Municipality).Distinct().OrderBy(x => x).ToArray(), Values[0]);
            return cb;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            ComboBox cb = uiElement as ComboBox;
            Values[0] = cb.Text;
        }
        public override Operator Process(Framework.Data.Geocache gc)
        {
            return GetOperators(gc.Municipality ?? "", Values.Count >= 0 ? Values[0] : "");
        }
    }

    public class ActionCity : ActionImplementationCondition
    {
        public const string STR_NAME = "City";
        public ActionCity(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
        }
        public override UIElement GetUIElement()
        {
            if (Values.Count == 0)
            {
                Values.Add("");
            }
            ComboBox cb = CreateComboBox((from Framework.Data.Geocache gc in Core.Geocaches where !string.IsNullOrEmpty(gc.City) select gc.City).Distinct().OrderBy(x => x).ToArray(), Values[0]);
            return cb;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            ComboBox cb = uiElement as ComboBox;
            Values[0] = cb.Text;
        }
        public override Operator Process(Framework.Data.Geocache gc)
        {
            return GetOperators(gc.City ?? "", Values.Count >= 0 ? Values[0] : "");
        }
    }


    //-----------------------------------------------------------------------------------------------------
    //-----------------------------------------------------------------------------------------------------

    public class ActionScriptAction : ActionImplementationAction
    {
        public const string STR_NAME = "Script";
        private Assembly _compiledAssembly = null;
        private System.IO.TemporaryFile _scriptFile = null;
        private ActionImplementationCondition _scriptedImplementation = null;
        private string _scriptContent = "";
        public ActionScriptAction(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
            _scriptFile = new System.IO.TemporaryFile();
        }
        public override UIElement GetUIElement()
        {
            TextBox tb = new TextBox();
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            tb.AcceptsReturn = true;
            tb.Width = 150;
            tb.Height = 30;
            tb.FontFamily = new System.Windows.Media.FontFamily("Courier new");
            tb.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            tb.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            tb.MouseEnter += new System.Windows.Input.MouseEventHandler(tb_MouseEnter);
            tb.MouseLeave += new System.Windows.Input.MouseEventHandler(tb_MouseLeave);
            if (Values.Count == 0)
            {
                using (System.IO.StreamReader textStreamReader = new System.IO.StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("GlobalcachingApplication.Plugins.ActBuilder.ActionScriptTemplate.txt")))
                {
                    Values.Add(textStreamReader.ReadToEnd());
                }
            }
            tb.Text = Values[0];
            return tb;
        }

        void tb_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is TextBox)
            {
                (sender as TextBox).VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                (sender as TextBox).HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
                (sender as TextBox).Width = 150;
                (sender as TextBox).Height = 30;
            }
        }

        void tb_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is TextBox)
            {
                (sender as TextBox).VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                (sender as TextBox).HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                (sender as TextBox).Width = 400;
                (sender as TextBox).Height = 400;
            }
        }

        public override bool PrepareRun()
        {
            base.PrepareRun();
            if (Values.Count > 0)
            {
                if (_scriptContent != Values[0])
                {
                    //cannot cleanup, waste of resources, I know...
                    _scriptedImplementation = null;
                }
                if (_scriptedImplementation == null)
                {
                    _scriptContent = Values[0];
                    //build script
                    System.IO.File.WriteAllText(_scriptFile.Path, _scriptContent);
                    _compiledAssembly = CSScript.Load(_scriptFile.Path);
                    //create object
                    Type[] types = _compiledAssembly.GetTypes();
                    foreach (Type t in types)
                    {
                        if (t.IsClass && (t.BaseType == typeof(ActionImplementationCondition)))
                        {
                            ConstructorInfo constructor = t.GetConstructor(new Type[] { typeof(Framework.Interfaces.ICore) });
                            object[] parameters = new object[] { Core };
                            _scriptedImplementation = (ActionImplementationCondition)constructor.Invoke(parameters);

                            break;
                        }
                    }
                    _scriptedImplementation.PrepareRun();
                }
            }
            return true;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            TextBox tb = uiElement as TextBox;
            Values[0] = tb.Text;
        }
        public override Operator Process(Framework.Data.Geocache gc)
        {
            if (_scriptedImplementation != null)
            {
                _scriptedImplementation.GeocachesAtInputConnector.Add(gc.Code, gc);
                return _scriptedImplementation.Process(gc);
            }
            else
            {
                return 0;
            }
        }

        public override void FinalizeRun()
        {
            if (_scriptedImplementation != null)
            {
                _scriptedImplementation.FinalizeRun();
            }
        }

    }

    public class ActionUpdateStatus : ActionImplementationAction
    {
        public const string STR_NAME = "Update status";
        public const string STR_ERROR = "Error";
        public const string STR_UNABLELIVE = "Unable to access the Live API or process its data";
        public const string STR_UPDATINGGEOCACHES = "Updating geocaches...";
        public const string STR_UPDATINGGEOCACHE = "Updating geocache...";

        private List<Framework.Data.Geocache> _gcList = null;
        private string _errormessage = null;
        private ManualResetEvent _actionDone = null;

        public ActionUpdateStatus(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
        }

        public override void FinalizeRun()
        {
            if (GeocachesAtInputConnector.Count > 0)
            {
                _errormessage = "";
                _gcList = (from Framework.Data.Geocache g in GeocachesAtInputConnector.Values select g).ToList();
                _actionDone = new ManualResetEvent(false);
                Thread thrd = new Thread(new ThreadStart(this.updateThreadMethod));
                thrd.Start();
                while (!_actionDone.WaitOne(100))
                {
                    System.Windows.Forms.Application.DoEvents();
                }
                thrd.Join();
                _actionDone.Dispose();
                if (!string.IsNullOrEmpty(_errormessage))
                {
                    System.Windows.Forms.MessageBox.Show(_errormessage, Utils.LanguageSupport.Instance.GetTranslation(Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR)), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                }
            }
            base.FinalizeRun();
        }

        private void updateThreadMethod()
        {
            try
            {
                using (Utils.ProgressBlock progress = new Utils.ProgressBlock(ActionBuilderForm.ActionBuilderFormInstance.OwnerPlugin as Utils.BasePlugin.Plugin, STR_UPDATINGGEOCACHES, STR_UPDATINGGEOCACHE, _gcList.Count, 0, true))
                {
                    int totalcount = _gcList.Count;
                    using (Utils.API.GeocachingLiveV6 client = new Utils.API.GeocachingLiveV6(Core, string.IsNullOrEmpty(Core.GeocachingComAccount.APIToken)))
                    {
                        int index = 0;
                        int gcupdatecount;
                        TimeSpan interval = new TimeSpan(0, 0, 0, 2, 100);
                        DateTime prevCall = DateTime.MinValue;
                        bool dodelay;
                        gcupdatecount = 100;
                        dodelay = (_gcList.Count / gcupdatecount > 30);
                        while (_gcList.Count > 0)
                        {
                            if (dodelay)
                            {
                                TimeSpan ts = DateTime.Now - prevCall;
                                if (ts < interval)
                                {
                                    Thread.Sleep(interval - ts);
                                }
                            }
                            var req = new Utils.API.LiveV6.GetGeocacheStatusRequest();
                            req.AccessToken = client.Token;
                            req.CacheCodes = (from a in _gcList select a.Code).Take(gcupdatecount).ToArray();
                            _gcList.RemoveRange(0, req.CacheCodes.Length);
                            index += req.CacheCodes.Length;
                            prevCall = DateTime.Now;
                            var resp = client.Client.GetGeocacheStatus(req);
                            if (resp.Status.StatusCode == 0 && resp.GeocacheStatuses != null)
                            {
                                foreach (var gs in resp.GeocacheStatuses)
                                {
                                    Framework.Data.Geocache gc = Utils.DataAccess.GetGeocache(Core.Geocaches, gs.CacheCode);
                                    if (gc != null)
                                    {
                                        gc.DataFromDate = DateTime.Now;
                                        gc.Archived = gs.Archived;
                                        gc.Available = gs.Available;
                                        gc.Name = gs.CacheName;
                                        gc.Title = gs.CacheName;
                                        gc.MemberOnly = gs.Premium;
                                    }
                                }
                            }
                            else if (resp.Status.StatusCode != 0)
                            {
                                _errormessage = resp.Status.StatusMessage;
                                break;
                            }
                            if (!progress.UpdateProgress(STR_UPDATINGGEOCACHES, STR_UPDATINGGEOCACHE, totalcount, index))
                            {
                                break;
                            }
                        }
                    }
                }
            }
            catch(Exception e)
            {
                _errormessage = e.Message;
            }
            _actionDone.Set();
        }

    }

    public class ActionUpdateData : ActionImplementationAction
    {
        public const string STR_NAME = "Refresh";
        public const string STR_ERROR = "Error";
        public const string STR_UNABLELIVE = "Unable to access the Live API or process its data";
        public const string STR_UPDATINGGEOCACHES = "Updating geocaches...";
        public const string STR_UPDATINGGEOCACHE = "Updating geocache...";

        private List<Framework.Data.Geocache> _gcList = null;
        private string _errormessage = null;
        private ManualResetEvent _actionDone = null;

        public ActionUpdateData(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
        }

        public override void FinalizeRun()
        {
            if (GeocachesAtInputConnector.Count > 0)
            {
                _errormessage = "";
                _gcList = (from Framework.Data.Geocache g in GeocachesAtInputConnector.Values select g).ToList();
                _actionDone = new ManualResetEvent(false);
                Thread thrd = new Thread(new ThreadStart(this.updateThreadMethod));
                thrd.Start();
                while (!_actionDone.WaitOne(100))
                {
                    System.Windows.Forms.Application.DoEvents();
                }
                thrd.Join();
                _actionDone.Dispose();
                if (!string.IsNullOrEmpty(_errormessage))
                {
                    System.Windows.Forms.MessageBox.Show(_errormessage, Utils.LanguageSupport.Instance.GetTranslation(Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR)), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                }                            
            }
            base.FinalizeRun();
        }

        private void updateThreadMethod()
        {
            try
            {
                using (Utils.ProgressBlock progress = new Utils.ProgressBlock(ActionBuilderForm.ActionBuilderFormInstance.OwnerPlugin as Utils.BasePlugin.Plugin, STR_UPDATINGGEOCACHES, STR_UPDATINGGEOCACHE, _gcList.Count, 0, true))
                {
                    int totalcount = _gcList.Count;
                    using (Utils.API.GeocachingLiveV6 client = new Utils.API.GeocachingLiveV6(Core, string.IsNullOrEmpty(Core.GeocachingComAccount.APIToken)))
                    {
                        int index = 0;
                        int gcupdatecount;
                        TimeSpan interval = new TimeSpan(0, 0, 0, 2, 100);
                        DateTime prevCall = DateTime.MinValue;
                        bool dodelay;
                        gcupdatecount = 50;
                        dodelay = (_gcList.Count > 30);
                        while (_gcList.Count > 0)
                        {
                            if (dodelay)
                            {
                                TimeSpan ts = DateTime.Now - prevCall;
                                if (ts < interval)
                                {
                                    Thread.Sleep(interval - ts);
                                }
                            }
                            Utils.API.LiveV6.SearchForGeocachesRequest req = new Utils.API.LiveV6.SearchForGeocachesRequest();
                            req.IsLite = Core.GeocachingComAccount.MemberTypeId == 1;
                            req.AccessToken = client.Token;
                            req.CacheCode = new Utils.API.LiveV6.CacheCodeFilter();
                            req.CacheCode.CacheCodes = (from a in _gcList select a.Code).Take(gcupdatecount).ToArray();
                            req.MaxPerPage = gcupdatecount;
                            req.GeocacheLogCount = 5;
                            index += req.CacheCode.CacheCodes.Length;
                            _gcList.RemoveRange(0, req.CacheCode.CacheCodes.Length);
                            prevCall = DateTime.Now;
                            var resp = client.Client.SearchForGeocaches(req);
                            if (resp.Status.StatusCode == 0 && resp.Geocaches != null)
                            {
                                Utils.API.Import.AddGeocaches(Core, resp.Geocaches);
                            }
                            else if (resp.Status.StatusCode != 0)
                            {
                                _errormessage = resp.Status.StatusMessage;
                                break;
                            }
                            if (!progress.UpdateProgress(STR_UPDATINGGEOCACHES, STR_UPDATINGGEOCACHE, totalcount, index))
                            {
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _errormessage = e.Message;
            }
            _actionDone.Set();
        }

    }


    public class ActionSetArchived : ActionImplementationAction
    {
        public const string STR_NAME = "Set Archived";
        public ActionSetArchived(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
        }
        public override UIElement GetUIElement()
        {
            if (Values.Count == 0)
            {
                Values.Add(true.ToString());
            }
            ComboBox cb = CreateComboBox(new string[] { true.ToString(), false.ToString() }, Values[0]);
            return cb;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            ComboBox cb = uiElement as ComboBox;
            Values[0] = cb.Text;
        }

        public override void FinalizeRun()
        {
            if (Values.Count > 0)
            {
                bool selected;
                if (bool.TryParse(Values[0], out selected))
                {
                    foreach (Framework.Data.Geocache gc in GeocachesAtInputConnector.Values)
                    {
                        gc.Archived = selected;
                    }
                }
            }
            base.FinalizeRun();
        }
    }

    public class ActionSetAvailable : ActionImplementationAction
    {
        public const string STR_NAME = "Set Available";
        public ActionSetAvailable(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
        }
        public override UIElement GetUIElement()
        {
            if (Values.Count == 0)
            {
                Values.Add(true.ToString());
            }
            ComboBox cb = CreateComboBox(new string[] { true.ToString(), false.ToString() }, Values[0]);
            return cb;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            ComboBox cb = uiElement as ComboBox;
            Values[0] = cb.Text;
        }

        public override void FinalizeRun()
        {
            if (Values.Count > 0)
            {
                bool selected;
                if (bool.TryParse(Values[0], out selected))
                {
                    foreach (Framework.Data.Geocache gc in GeocachesAtInputConnector.Values)
                    {
                        gc.Available = selected;
                    }
                }
            }
            base.FinalizeRun();
        }
    }

    public class ActionSetSelected : ActionImplementationAction
    {
        public const string STR_NAME = "Set Selected";
        public ActionSetSelected(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
        }
        public override UIElement GetUIElement()
        {
            if (Values.Count == 0)
            {
                Values.Add(true.ToString());
            }
            ComboBox cb = CreateComboBox(new string[]{true.ToString(), false.ToString()}, Values[0]);
            return cb;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            ComboBox cb = uiElement as ComboBox;
            Values[0] = cb.Text;
        }

        public override void FinalizeRun()
        {
            if (Values.Count > 0)
            {
                bool selected;
                if (bool.TryParse(Values[0], out selected))
                {
                    foreach (Framework.Data.Geocache gc in GeocachesAtInputConnector.Values)
                    {
                        gc.Selected = selected;
                    }
                }
            }
            base.FinalizeRun();
        }
    }

    public class ActionSetFlagged : ActionImplementationAction
    {
        public const string STR_NAME = "Set Flagged";
        public ActionSetFlagged(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
        }
        public override UIElement GetUIElement()
        {
            if (Values.Count == 0)
            {
                Values.Add(true.ToString());
            }
            ComboBox cb = CreateComboBox(new string[] { true.ToString(), false.ToString() }, Values[0]);
            return cb;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            ComboBox cb = uiElement as ComboBox;
            Values[0] = cb.Text;
        }

        public override void FinalizeRun()
        {
            if (Values.Count > 0)
            {
                bool selected;
                if (bool.TryParse(Values[0], out selected))
                {
                    foreach (Framework.Data.Geocache gc in GeocachesAtInputConnector.Values)
                    {
                        gc.Flagged = selected;
                    }
                }
            }
            base.FinalizeRun();
        }
    }

    public class ActionDeleteGeocache : ActionImplementationAction
    {
        public const string STR_NAME = "Delete geocache";
        public ActionDeleteGeocache(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
        }
        public override void FinalizeRun()
        {
            foreach (Framework.Data.Geocache gc in GeocachesAtInputConnector.Values)
            {
                Utils.DataAccess.DeleteGeocache(Core, gc);
            }
            base.FinalizeRun();
        }
    }


    public class ActionNextFlow : ActionImplementationAction
    {
        public const string STR_NAME = "Flow";
        private ActionImplementation _ai = null;
        public ActionNextFlow(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
        }
        public override UIElement GetUIElement()
        {
            if (Values.Count == 0)
            {
                Values.Add("");
            }
            ComboBox cb = CreateComboBox((from a in ActionBuilderForm.ActionBuilderFormInstance.AvailableActionFlows select a.Name).ToArray(), Values[0]);
            return cb;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            ComboBox cb = uiElement as ComboBox;
            Values[0] = cb.Text;
        }

        public override bool PrepareRun()
        {
            base.PrepareRun();
            _ai = null;
            if (Values.Count > 0)
            {
                ActionBuilderForm.ActionFlow af = (from a in ActionBuilderForm.ActionBuilderFormInstance.AvailableActionFlows where a.Name == Values[0] select a).FirstOrDefault();
                if (af != null)
                {
                    _ai = (from a in af.Actions where a is ActionStart select a).FirstOrDefault();
                }
            }
            return true;
        }

        public override Operator Process(Framework.Data.Geocache gc)
        {
            if (_ai!=null)
            {
                _ai.Run(gc);
            }
            return 0;
        }
    }

    public class ActionClearSelection : ActionImplementationExecuteOnce
    {
        public const string STR_NAME = "Clear selection";
        public ActionClearSelection(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
        }
        protected override bool Execute()
        {
            foreach (Framework.Data.Geocache gc in Core.Geocaches)
            {
                gc.Selected = false;
            }
            return true;
        }
    }

    public class ActionRunScript : ActionImplementationExecuteOnce
    {
        public const string STR_NAME = "Run script";
        public ActionRunScript(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
        }
        public override UIElement GetUIElement()
        {
            if (Values.Count == 0)
            {
                Values.Add("");
            }
            List<string> availableScripts = new List<string>();
            try
            {
                string[] files = Directory.GetFiles(Core.CSScriptsPath, "*.cs");
                if (files != null)
                {
                    foreach (string s in files)
                    {
                        if (!Path.GetFileName(s).StartsWith("_"))
                        {
                            availableScripts.Add(string.Format("{0}", Path.GetFileNameWithoutExtension(s)));
                        }
                    }
                }
            }
            catch
            {
            }
            ComboBox cb = CreateComboBox(availableScripts.ToArray(), Values[0]);
            return cb;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            ComboBox cb = uiElement as ComboBox;
            Values[0] = cb.Text;
        }

        protected override bool Execute()
        {
            if (Values[0] != "")
            {
                try
                {
                    AsmHelper scriptAsm = new AsmHelper(CSScript.Load(Path.Combine(Core.CSScriptsPath, string.Format("{0}.cs", Values[0]))));
                    scriptAsm.Invoke("Script.Run", new object[] { Utils.PluginSupport.PluginByName(Core, "GlobalcachingApplication.Plugins.ActBuilder.ActionBuilder"), Core });
                }
                catch
                {
                    
                }
            }
            return true;
        }

    }

    public class ActionExecuteAction : ActionImplementationExecuteOnce
    {
        public const string STR_NAME = "Execute action";
        public const string STR_PLUGIN = "Plugin";
        public const string STR_ACTION = "Action";
        public const string STR_SUBACTION = "Sub action";
        public ActionExecuteAction(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
        }

        public override UIElement GetUIElement()
        {
            /*
             * ------------------------
             * |  plugin  | combo
             * |  action | combo           
             * |  sub action | combo
             * ------------------------
             */
            StackPanel sp = new StackPanel();

            Grid g = new Grid();
            Label tb = new Label();
            tb.HorizontalAlignment = HorizontalAlignment.Left;
            tb.Width = 180;
            tb.Content = Utils.LanguageSupport.Instance.GetTranslation(STR_PLUGIN);
            g.Children.Add(tb);


            if (Values.Count == 0)
            {
                Values.Add("");
            }
            List<string> l = (from p in Core.GetPlugins() select p.GetType().FullName).OrderBy(x=>x).ToList();
            l.Insert(0, "");
            if (l.IndexOf(Values[0]) < 0)
            {
                Values[0] = "";
            }
            ComboBox b = CreateComboBox(l.ToArray(), Values[0]);
            b.IsEditable = false;
            b.Width = 200;
            b.HorizontalAlignment = HorizontalAlignment.Right;
            b.SelectionChanged += new SelectionChangedEventHandler(b_SelectionChanged);
            g.Children.Add(b);
            sp.Children.Add(g);

            Grid g2 = new Grid();
            Label tb2 = new Label();
            tb2.HorizontalAlignment = HorizontalAlignment.Left;
            tb2.Width = tb.Width;
            tb2.Content = Utils.LanguageSupport.Instance.GetTranslation(STR_ACTION);
            g2.Children.Add(tb2);


            List<string> actions = new List<string>();
            if (Values.Count == 1)
            {
                Values.Add("");
            }
            actions.Insert(0, "");
            actions.AddRange(getPluginActions(Values[0]));
            if (actions.IndexOf(Values[1]) < 0)
            {
                Values[1] = "";
            }
            ComboBox b2 = CreateComboBox(actions.ToArray(), Values[1]);
            b2.IsEditable = false;
            b2.Width = b.Width;
            b2.HorizontalAlignment = HorizontalAlignment.Right;
            b2.SelectionChanged += new SelectionChangedEventHandler(b2_SelectionChanged);
            g2.Children.Add(b2);
            sp.Children.Add(g2);


            Grid g3 = new Grid();
            Label tb3 = new Label();
            tb3.HorizontalAlignment = HorizontalAlignment.Left;
            tb3.Width = tb.Width;
            tb3.Content = Utils.LanguageSupport.Instance.GetTranslation(STR_SUBACTION);
            g3.Children.Add(tb3);

            List<string> subactions = new List<string>();
            if (Values.Count == 2)
            {
                Values.Add("");
            }
            subactions.Insert(0, "");
            subactions.AddRange(getPluginSubActions(Values[0], Values[1]));
            if (subactions.IndexOf(Values[2]) < 0)
            {
                Values[2] = "";
            }
            ComboBox b3 = CreateComboBox(subactions.ToArray(), Values[2]);
            b3.IsEditable = false;
            b3.Width = b.Width;
            b3.HorizontalAlignment = HorizontalAlignment.Right;
            g3.Children.Add(b3);
            sp.Children.Add(g3);

            return sp;
        }

        void b2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;

            List<string> al;
            ComboBoxItem cbi = cb.SelectedValue as ComboBoxItem;
            if (cbi != null)
            {
                al = getPluginSubActions(((((cb.Parent as Grid).Parent as StackPanel).Children[0] as Grid).Children[1] as ComboBox).Text, cbi.Content.ToString());
            }
            else
            {
                al = new List<string>();
            }
            al.Insert(0, "");

            ComboBox acb = (((cb.Parent as Grid).Parent as StackPanel).Children[2] as Grid).Children[1] as ComboBox;
            acb.Items.Clear();
            foreach (string s in al)
            {
                ComboBoxItem cboxitem = new ComboBoxItem();
                cboxitem.Content = s;
                acb.Items.Add(cboxitem);
            }
        }

        List<string> getPluginActions(string pluginName)
        {
            List<string> result = new List<string>();
            if (!string.IsNullOrEmpty(pluginName))
            {
                Framework.Interfaces.IPlugin p = Utils.PluginSupport.PluginByName(Core, pluginName);
                if (p != null)
                {
                    List<string> sl = p.GetActionSubactionList('|');
                    if (sl != null)
                    {
                        result = (from s in sl select s.Split(new char[] { '|' })[0]).Distinct().ToList();
                    }
                }
            }
            return result;
        }

        List<string> getPluginSubActions(string pluginName, string action)
        {
            List<string> result = new List<string>();
            if (!string.IsNullOrEmpty(pluginName))
            {
                Framework.Interfaces.IPlugin p = Utils.PluginSupport.PluginByName(Core, pluginName);
                if (p != null)
                {
                    List<string> sl = p.GetActionSubactionList('|');
                    if (sl != null)
                    {
                        List<string> al = (from s in sl where s.Split(new char[] { '|' })[0] == action select s).ToList();
                        foreach (string s in al)
                        {
                            if (s.IndexOf('|') > 0)
                            {
                                result.Add(s.Split(new char[] { '|' })[1]);
                            }
                        }
                    }
                }
            }
            return result;
        }

        void b_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;

            List<string> al;
            ComboBoxItem cbi = cb.SelectedValue as ComboBoxItem;
            if (cbi != null)
            {
                al = getPluginActions(cbi.Content.ToString());
            }
            else
            {
                al = new List<string>();
            }
            al.Insert(0, "");

            ComboBox acb = (((cb.Parent as Grid).Parent as StackPanel).Children[1] as Grid).Children[1] as ComboBox;
            acb.Items.Clear();
            foreach (string s in al)
            {
                ComboBoxItem cboxitem = new ComboBoxItem();
                cboxitem.Content = s;
                acb.Items.Add(cboxitem);
            }

        }

        public override void CommitUIData(UIElement uiElement)
        {
            //ComboBox cb = uiElement as ComboBox;
            Values[0] = (((uiElement as StackPanel).Children[0] as Grid).Children[1] as ComboBox).Text;
            Values[1] = (((uiElement as StackPanel).Children[1] as Grid).Children[1] as ComboBox).Text;
            Values[2] = (((uiElement as StackPanel).Children[2] as Grid).Children[1] as ComboBox).Text;
        }

        protected override bool Execute()
        {
            if (!string.IsNullOrEmpty(Values[0]) && !string.IsNullOrEmpty(Values[1]))
            {
                Framework.Interfaces.IPlugin p = Utils.PluginSupport.PluginByName(Core, Values[0]);
                if (p != null)
                {
                    if (string.IsNullOrEmpty(Values[2]))
                    {
                        p.Action(Values[1]);
                    }
                    else
                    {
                        p.Action(string.Format("{0}|{1}",Values[1],Values[2]));
                    }
                }               
            }
            return true;
        }
    }

    public class ActionRunShortcut : ActionImplementationExecuteOnce
    {
        public const string STR_NAME = "Activate shortcut";
        public ActionRunShortcut(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
        }
        public override UIElement GetUIElement()
        {
            if (Values.Count == 0)
            {
                Values.Add("");
            }
            List<string> availableScripts = new List<string>();
            availableScripts.AddRange((from a in Core.ShortcutInfo where !string.IsNullOrEmpty(a.ShortcutKeyString) orderby a.ShortcutKeyString select a.ShortcutKeyString).Distinct().ToList());

            ComboBox cb = CreateComboBox(availableScripts.ToArray(), Values[0]);
            return cb;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            ComboBox cb = uiElement as ComboBox;
            Values[0] = cb.Text;
        }

        protected override bool Execute()
        {
            if (Values[0] != "")
            {
                try
                {
                    Framework.Data.ShortcutInfo sci = (from a in Core.ShortcutInfo where a.ShortcutKeyString == Values[0] select a).FirstOrDefault();
                    if (sci != null)
                    {
                        Framework.Interfaces.IPlugin p = Utils.PluginSupport.PluginByName(Core, sci.PluginType);
                        if (p != null)
                        {
                            if (string.IsNullOrEmpty(sci.PluginSubAction))
                            {
                                p.Action(sci.PluginAction);
                            }
                            else
                            {
                                p.Action(string.Format("{0}|{1}", sci.PluginAction, sci.PluginSubAction));
                            }
                        }
                    }
                }
                catch
                {

                }
            }
            return true;
        }

    }


    public class ActionDescriptionContains : ActionImplementationCondition
    {
        public const string STR_NAME = "Description contains";
        public const string STR_REGULAREXPRESSION = "Regular expression";
        private string _value = "";
        private Regex _regEx = null;
        private bool _isRegularExpression = false;
        public ActionDescriptionContains(Framework.Interfaces.ICore core)
            : base(STR_NAME, core)
        {
        }
        public override ActionImplementation.Operator AllowOperators
        {
            get
            {
                return ActionImplementation.Operator.Equal | ActionImplementation.Operator.NotEqual;
            }
        }
        public override UIElement GetUIElement()
        {
            if (Values.Count == 0)
            {
                Values.Add("");
            }
            if (Values.Count < 2)
            {
                Values.Add(false.ToString());
            }
            StackPanel sp = new StackPanel();
            TextBox tb = new TextBox();
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            sp.Children.Add(tb);
            tb.Text = Values[0];
            CheckBox cb = new CheckBox();
            cb.Content = Utils.LanguageSupport.Instance.GetTranslation(STR_REGULAREXPRESSION);
            cb.IsChecked = bool.Parse(Values[1]);
            sp.Children.Add(cb);
            return sp;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            TextBox tb = (uiElement as StackPanel).Children[0] as TextBox;
            Values[0] = tb.Text;
            CheckBox cb = (uiElement as StackPanel).Children[1] as CheckBox;
            Values[1] = cb.IsChecked == null ? false.ToString() : cb.IsChecked.ToString();
        }
        public override bool PrepareRun()
        {
            _value = "";
            _isRegularExpression = false;
            if (Values.Count > 0)
            {
                _value = Values[0];
            }
            if (Values.Count > 1)
            {
                bool.TryParse(Values[1], out _isRegularExpression);
            }
            if (_isRegularExpression)
            {
                _regEx = new Regex(_value, RegexOptions.Multiline);
            }
            else
            {
                _regEx = null;
            }
            return base.PrepareRun();
        }
        public override Operator Process(Framework.Data.Geocache gc)
        {
            if (!_isRegularExpression)
            {
                int pos = -1;
                string sd = gc.ShortDescription;
                if (!string.IsNullOrEmpty(sd))
                {
                    pos = sd.IndexOf(_value, StringComparison.InvariantCultureIgnoreCase);
                }
                if (pos<0)
                {
                    sd = gc.LongDescription;
                    if (!string.IsNullOrEmpty(sd))
                    {
                        pos = sd.IndexOf(_value, StringComparison.InvariantCultureIgnoreCase);
                    }
                }
                if (pos < 0)
                {
                    return Operator.NotEqual;
                }
                else
                {
                    return Operator.Equal;
                }
            }
            else if (_regEx!=null)
            {
                string sd = gc.ShortDescription;
                int cnt = 0;
                if (!string.IsNullOrEmpty(sd))
                {
                    cnt = _regEx.Matches(sd).Count;
                }
                if (cnt==0)
                {
                    sd = gc.LongDescription;
                    if (!string.IsNullOrEmpty(sd))
                    {
                        cnt = _regEx.Matches(sd).Count;
                    }
                }
                if (cnt == 0)
                {
                    return Operator.NotEqual;
                }
                else
                {
                    return Operator.Equal;
                }
            }
            else
            {
                return Operator.NotEqual;
            }
        }
    }

}
