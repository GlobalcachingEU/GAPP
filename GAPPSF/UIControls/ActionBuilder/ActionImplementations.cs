using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Reflection;
using System.Threading;
using System.Windows.Media.Imaging;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.IO;

namespace GAPPSF.UIControls.ActionBuilder
{
    public class ActionStart : ActionImplementationCondition
    {
        public const string STR_NAME = "Start";
        public const string STR_SORTON = "SortGeocachesOn";
        public const string STR_ASCENDING = "Ascending";
        public const string STR_DESCENDING = "Descending";
        public const string STR_SORT_NONE = "";
        public const string STR_SORT_DISTANCE = "DistanceFromCenter";
        public const string STR_SORT_TERRAIN = "Terrain";
        public const string STR_SORT_DIFFICULTY = "Difficulty";
        public const string STR_SORT_FAVORITE = "Favorite";
        public const string STR_SORT_PUBLISHED = "Hidden date";
        public ActionStart()
            : base(STR_NAME)
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
            l.Content = Localization.TranslationManager.Instance.Translate(STR_SORTON);
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
        public List<Core.Data.Geocache> PrepareFlow()
        {
            List<Core.Data.Geocache> fresult = Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection.ToList();
            if (Values.Count > 1)
            {
                if (Values[0] != STR_SORT_NONE)
                {
                    fresult.Sort(delegate(Core.Data.Geocache x, Core.Data.Geocache y)
                    {
                        int result = 0;
                        Core.Data.Geocache gc1 = x as Core.Data.Geocache;
                        Core.Data.Geocache gc2 = y as Core.Data.Geocache;
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
                    });
                }
            }
            return fresult;
        }
        public override bool AllowEntryPoint
        {
            get { return false; }
        }
        public override Operator AllowOperators
        {
            get { return Operator.Equal; }
        }
        public override Operator Process(Core.Data.Geocache gc)
        {
            return Operator.Equal;
        }
    }

    public class ActionNameContains : ActionImplementationCondition
    {
        public const string STR_NAME = "NameContains";
        private string _value = "";
        public ActionNameContains()
            : base(STR_NAME)
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
        public override Operator Process(Core.Data.Geocache gc)
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
        public const string STR_NAME = "NotesContains";
        private string _value = "";
        public ActionNotesContains()
            : base(STR_NAME)
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
        public override Operator Process(Core.Data.Geocache gc)
        {
            int pos = -1;
            if (!string.IsNullOrEmpty(gc.Notes))
            {
                pos = gc.Notes.IndexOf(_value, StringComparison.InvariantCultureIgnoreCase);
            }
            if (pos < 0 && !string.IsNullOrEmpty(gc.PersonalNote))
            {
                pos = gc.PersonalNote.IndexOf(_value, StringComparison.InvariantCultureIgnoreCase);
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
        public ActionAttributes()
            : base(STR_NAME)
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
            cb.Content = Localization.TranslationManager.Instance.Translate(STR_ALL);
            cb.IsChecked = bool.Parse(Values[0]);
            sp.Children.Add(cb);
            ScrollViewer sv = new ScrollViewer();
            UniformGrid g = new UniformGrid();
            g.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            sv.Content = g;
            sv.CanContentScroll = true;
            sv.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            sv.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            sv.Width = 200;
            sv.Height = 120;
            foreach (Core.Data.GeocacheAttribute attr in Core.ApplicationData.Instance.GeocacheAttributes)
            {
                StackPanel ga = new StackPanel();
                ga.Width = 40;
                ga.Height = 60;
                Image img = new Image();
                img.ToolTip = Localization.TranslationManager.Instance.Translate(attr.Name);
                if (Math.Abs(attr.ID) < 100)
                {
                    img.Source = new BitmapImage(Utils.ResourceHelper.GetResourceUri(string.Format("/Resources/Attributes/{0}.gif", attr.ID.ToString().Replace('-','_'))));
                }
                else
                {
                    img.Source = new BitmapImage(Utils.ResourceHelper.GetResourceUri(string.Format("/Resources/Attributes/{0}.png", attr.ID.ToString().Replace('-', '_'))));
                }
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
            /*
            foreach (Core.Data.GeocacheAttribute attr in Core.ApplicationData.Instance.GeocacheAttributes)
            {
                if (attr.ID > 0)
                {
                    StackPanel ga = new StackPanel();
                    ga.Width = 40;
                    ga.Height = 60;
                    Image img = new Image();
                    img.ToolTip = Localization.TranslationManager.Instance.Translate(attr.Name);
                    if (Math.Abs(attr.ID) < 100)
                    {
                        img.Source = new BitmapImage(Utils.ResourceHelper.GetResourceUri(string.Format("/Resources/Attributes/{0}.gif", attr.ID.ToString().Replace('-', '_'))));
                    }
                    else
                    {
                        img.Source = new BitmapImage(Utils.ResourceHelper.GetResourceUri(string.Format("/Resources/Attributes/{0}.png", attr.ID.ToString().Replace('-', '_'))));
                    }
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
             * */
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
                if (cb.IsChecked == true)
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
        public override Operator Process(Core.Data.Geocache gc)
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
        public const string STR_NAME = "GeocacheCounter";
        private int _value = 0;
        private int _counter = 0;
        public ActionGeocacheCount()
            : base(STR_NAME)
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
        public override Operator Process(Core.Data.Geocache gc)
        {
            _counter++;
            return GetOperators(_counter.CompareTo(_value));
        }
    }

    public class ActionWaypointCounter : ActionImplementationCondition
    {
        public const string STR_NAME = "WaypointCounter";
        public const string STR_COORDSONLY = "OnlyWithCoords";
        public const string STR_ADDGEOCACHE = "CountGeocacheAsWaypoint";
        private int _value = 0;
        private int _counter = 0;
        private bool _withCoordsOnly = true;
        private bool _addGeocache = true;
        public ActionWaypointCounter()
            : base(STR_NAME)
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
            cb.Content = Localization.TranslationManager.Instance.Translate(STR_COORDSONLY);
            cb.IsChecked = bool.Parse(Values[1]);
            sp.Children.Add(cb);
            CheckBox cb2 = new CheckBox();
            cb2.Content = Localization.TranslationManager.Instance.Translate(STR_ADDGEOCACHE);
            cb2.IsChecked = bool.Parse(Values[2]);
            sp.Children.Add(cb2);
            return sp;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            TextBox tb = (uiElement as StackPanel).Children[0] as TextBox;
            Values[0] = tb.Text;
            CheckBox cb = (uiElement as StackPanel).Children[1] as CheckBox;
            Values[1] = cb.IsChecked == null ? false.ToString() : cb.IsChecked.ToString();
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
        public override Operator Process(Core.Data.Geocache gc)
        {
            List<Core.Data.Waypoint> wps = Utils.DataAccess.GetWaypointsFromGeocache(gc.Database, gc.Code);
            if (_withCoordsOnly)
            {
                _counter += (from w in wps where w.Lat != null && w.Lon != null select w).Count();
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
        public const string STR_NAME = "NumUserWaypoints";
        private int _value = 0;
        public ActionUserWaypointCount()
            : base(STR_NAME)
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
        public override Operator Process(Core.Data.Geocache gc)
        {
            return GetOperators(Utils.DataAccess.GetUserWaypointsFromGeocache(gc.Database,gc.Code).Count.CompareTo(_value));
        }
    }

    /*
    public class ActionGCVoteAverage : ActionImplementationCondition
    {
        public const string STR_NAME = "GCVote Average";
        private double _value = 0;
        public ActionGCVoteAverage()
            : base(STR_NAME)
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
                        sd = gcvote.Substring(pos1 + 1, pos2 - pos1 - 1);
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
    */

    public class ActionWaypointCount : ActionImplementationCondition
    {
        public const string STR_NAME = "NumWaypoints";
        private int _value = 0;
        public ActionWaypointCount()
            : base(STR_NAME)
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
        public override Operator Process(Core.Data.Geocache gc)
        {
            return GetOperators(Utils.DataAccess.GetWaypointsFromGeocache(gc.Database, gc.Code).Count.CompareTo(_value));
        }
    }

    public class ActionLogCount : ActionImplementationCondition
    {
        public const string STR_NAME = "NumLogs";
        private int _value = 0;
        public ActionLogCount()
            : base(STR_NAME)
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
        public override Operator Process(Core.Data.Geocache gc)
        {
            return GetOperators(Utils.DataAccess.GetLogs(gc.Database, gc.Code).Count.CompareTo(_value));
        }
    }

    public class ActionLogImagesCount : ActionImplementationCondition
    {
        public const string STR_NAME = "NumLogImages";
        private int _value = 0;
        public ActionLogImagesCount()
            : base(STR_NAME)
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
        public override Operator Process(Core.Data.Geocache gc)
        {
            int count = 0;
            List<Core.Data.Log> logs = Utils.DataAccess.GetLogs(gc.Database, gc.Code);
            foreach (Core.Data.Log l in logs)
            {
                count += Utils.DataAccess.GetLogImages(gc.Database, l.ID).Count;
            }
            return GetOperators(count.CompareTo(_value));
        }
    }

    public class ActionGcComNotes : ActionImplementationCondition
    {
        public const string STR_NAME = "gccomNotes";
        public ActionGcComNotes()
            : base(STR_NAME)
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
        public override Operator Process(Core.Data.Geocache gc)
        {
            Operator result = 0;
            if (Values.Count > 0)
            {
                bool value = false;
                if (bool.TryParse(Values[0], out value))
                {
                    if (string.IsNullOrEmpty(gc.PersonalNote) == value)
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
        public ActionNotes()
            : base(STR_NAME)
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
        public override Operator Process(Core.Data.Geocache gc)
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
        public const string STR_NAME = "DateOfFind";

        private DateTime _date = DateTime.MinValue;

        public ActionFoundDate()
            : base(STR_NAME)
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
        public override Operator Process(Core.Data.Geocache gc)
        {
            if (gc.Found && gc.FoundDate != null)
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
        public const string STR_NAME = "YourFavorite";
        private string[] _params = null;
        public ActionYourFavorite()
            : base(STR_NAME)
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
        public override Operator Process(Core.Data.Geocache gc)
        {
            Operator result = 0;
            if (Values.Count > 0)
            {
                bool value = false;
                if (bool.TryParse(Values[0], out value))
                {
                    if (Favorites.Manager.Instance.GeocacheFavorited(gc.Code ?? "") != value)
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

    public class ActionFound : ActionImplementationCondition
    {
        public const string STR_NAME = "Found";
        public ActionFound()
            : base(STR_NAME)
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
        public override Operator Process(Core.Data.Geocache gc)
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
        public const string STR_NAME = "IOwn";
        public ActionIsOwn()
            : base(STR_NAME)
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
        public override Operator Process(Core.Data.Geocache gc)
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
        public ActionLocked()
            : base(STR_NAME)
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
        public override Operator Process(Core.Data.Geocache gc)
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
        public ActionSelected()
            : base(STR_NAME)
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
        public override Operator Process(Core.Data.Geocache gc)
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
        public ActionFlagged()
            : base(STR_NAME)
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
        public override Operator Process(Core.Data.Geocache gc)
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

    public class ActionFavorites : ActionImplementationCondition
    {
        public const string STR_NAME = "Favorites";
        private int _value = 0;
        public ActionFavorites()
            : base(STR_NAME)
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
        public override Operator Process(Core.Data.Geocache gc)
        {
            return GetOperators(gc.Favorites.CompareTo(_value));
        }
    }

    public class ActionCustomCoords : ActionImplementationCondition
    {
        public const string STR_NAME = "CustomCoords";
        public ActionCustomCoords()
            : base(STR_NAME)
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
        public override Operator Process(Core.Data.Geocache gc)
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
        public const string STR_NAME = "PMO";
        public ActionMemberOnly()
            : base(STR_NAME)
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
        public override Operator Process(Core.Data.Geocache gc)
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
        public ActionDifficulty()
            : base(STR_NAME)
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
        public override Operator Process(Core.Data.Geocache gc)
        {
            return GetOperators(gc.Difficulty.ToString("0.#").Replace(',', '.'), Values.Count >= 0 ? Values[0] : "");
        }
    }

    public class ActionTerrain : ActionImplementationCondition
    {
        public const string STR_NAME = "Terrain";
        public ActionTerrain()
            : base(STR_NAME)
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
        public override Operator Process(Core.Data.Geocache gc)
        {
            return GetOperators(gc.Terrain.ToString("0.#").Replace(',', '.'), Values.Count >= 0 ? Values[0] : "");
        }
    }

    public class ActionContainer : ActionImplementationCondition
    {
        public const string STR_NAME = "Container";
        public ActionContainer()
            : base(STR_NAME)
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
            ComboBox cb = CreateComboBox((from g in Core.ApplicationData.Instance.GeocacheContainers select g.Name).OrderBy(x => x).ToArray(), Values[0]);
            return cb;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            ComboBox cb = uiElement as ComboBox;
            Values[0] = cb.Text;
        }
        public override Operator Process(Core.Data.Geocache gc)
        {
            return GetOperators(gc.Container.Name ?? "", Values.Count >= 0 ? Values[0] : "");
        }
    }

    public class ActionOwner : ActionImplementationCondition
    {
        public const string STR_NAME = "Owner";
        public ActionOwner()
            : base(STR_NAME)
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

            ComboBox cb = CreateComboBox(null, Values[0]);
            return cb;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            ComboBox cb = uiElement as ComboBox;
            Values[0] = cb.Text;
        }
        public override Operator Process(Core.Data.Geocache gc)
        {
            return GetOperators(gc.Owner ?? "", Values.Count >= 0 ? Values[0] : "");
        }
    }

    public class ActionPlacedBy : ActionImplementationCondition
    {
        public const string STR_NAME = "PlacedBy";
        public ActionPlacedBy()
            : base(STR_NAME)
        {
        }
        public override UIElement GetUIElement()
        {
            if (Values.Count == 0)
            {
                Values.Add("");
            }
            ComboBox cb = CreateComboBox(null, Values[0]);
            return cb;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            ComboBox cb = uiElement as ComboBox;
            Values[0] = cb.Text;
        }
        public override Operator Process(Core.Data.Geocache gc)
        {
            return GetOperators(gc.PlacedBy ?? "", Values.Count >= 0 ? Values[0] : "");
        }
    }

    /*
    public class ActionInCollection : ActionImplementationCondition
    {
        public const string STR_NAME = "In collection";
        public ActionInCollection()
            : base(STR_NAME)
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
    */

    public class ActionGeocacheType : ActionImplementationCondition
    {
        public const string STR_NAME = "GeocacheType";
        public ActionGeocacheType()
            : base(STR_NAME)
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
            ComboBox cb = CreateComboBox((from g in Core.ApplicationData.Instance.GeocacheTypes select g.Name).OrderBy(x => x).ToArray(), Values[0]);
            return cb;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            ComboBox cb = uiElement as ComboBox;
            Values[0] = cb.Text;
        }
        public override Operator Process(Core.Data.Geocache gc)
        {
            return GetOperators(gc.GeocacheType.Name ?? "", Values.Count >= 0 ? Values[0] : "");
        }
    }


    public class ActionArchived : ActionImplementationCondition
    {
        public const string STR_NAME = "Archived";
        public ActionArchived()
            : base(STR_NAME)
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
        public override Operator Process(Core.Data.Geocache gc)
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
        public ActionAvailable()
            : base(STR_NAME)
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
        public override Operator Process(Core.Data.Geocache gc)
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
        public const string STR_NAME = "DistanceToLocationKm";
        private double _value = 0.0;
        private Core.Data.Location _loc = null;
        public ActionDistanceToLocation()
            : base(STR_NAME)
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
                Values.Add(Utils.Conversion.GetCoordinatesPresentation(Core.ApplicationData.Instance.CenterLocation));
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
            GAPPSF.Dialogs.GetLocationWindow dlg = new GAPPSF.Dialogs.GetLocationWindow(Core.ApplicationData.Instance.CenterLocation);
            if (dlg.ShowDialog() == true)
            {
                (((sender as Button).Parent as Grid).Children[0] as TextBox).Text = Utils.Conversion.GetCoordinatesPresentation(dlg.Location);
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
        public override Operator Process(Core.Data.Geocache gc)
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
        public const string STR_NAME = "DistanceToCenterKm";
        private double _value = 0.0;
        public ActionDistanceToCenter()
            : base(STR_NAME)
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
        public override Operator Process(Core.Data.Geocache gc)
        {
            try
            {
                double dist = Utils.Calculus.CalculateDistance(gc, Core.ApplicationData.Instance.CenterLocation).EllipsoidalDistance / 1000.0;
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
        public ActionLon()
            : base(STR_NAME)
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
        public override Operator Process(Core.Data.Geocache gc)
        {
            return GetOperators(gc.Lon.CompareTo(_value));
        }
    }

    public class ActionLat : ActionImplementationCondition
    {
        public const string STR_NAME = "Latitude";
        private double _value = 0.0;
        public ActionLat()
            : base(STR_NAME)
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
        public override Operator Process(Core.Data.Geocache gc)
        {
            return GetOperators(gc.Lat.CompareTo(_value));
        }
    }

    public class ActionUpdatedDaysAgo : ActionImplementationCondition
    {
        public const string STR_NAME = "UpdatedDaysAgo";
        private int _value = 0;
        public ActionUpdatedDaysAgo()
            : base(STR_NAME)
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
        public override Operator Process(Core.Data.Geocache gc)
        {
            return GetOperators(((int)((DateTime.Now.Date - gc.DataFromDate.Date).TotalDays)).CompareTo(_value));
        }
    }


    public class ActionDataFromDate : ActionImplementationCondition
    {
        public const string STR_NAME = "DateOfData";

        private DateTime _date = DateTime.MinValue;

        public ActionDataFromDate()
            : base(STR_NAME)
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
        public override Operator Process(Core.Data.Geocache gc)
        {
            return GetOperators(gc.DataFromDate.Date.CompareTo(_date));
        }
    }

    public class ActionPublishedDaysAgo : ActionImplementationCondition
    {
        public const string STR_NAME = "PublishedDaysAgo";
        private int _value = 0;
        public ActionPublishedDaysAgo()
            : base(STR_NAME)
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
        public override Operator Process(Core.Data.Geocache gc)
        {
            return GetOperators(((int)((DateTime.Now.Date - gc.PublishedTime.Date).TotalDays)).CompareTo(_value));
        }
    }


    public class ActionPublished : ActionImplementationCondition
    {
        public const string STR_NAME = "Published";

        private DateTime _date = DateTime.MinValue;

        public ActionPublished()
            : base(STR_NAME)
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

        public override Operator Process(Core.Data.Geocache gc)
        {
            return GetOperators(gc.PublishedTime.Date.CompareTo(_date));
        }
    }

    public class ActionCode : ActionImplementationCondition
    {
        public const string STR_NAME = "Code";
        public ActionCode()
            : base(STR_NAME)
        {
        }
        public override UIElement GetUIElement()
        {
            if (Values.Count == 0)
            {
                Values.Add("");
            }
            ComboBox cb = CreateComboBox(new string[] { "GC" }, Values[0]);
            return cb;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            ComboBox cb = uiElement as ComboBox;
            Values[0] = cb.Text;
        }
        public override Operator Process(Core.Data.Geocache gc)
        {
            return GetOperators(gc.Code ?? "", Values.Count >= 0 ? Values[0] : "");
        }
    }

    public class ActionCountry : ActionImplementationCondition
    {
        public const string STR_NAME = "Country";
        public ActionCountry()
            : base(STR_NAME)
        {
        }
        public override UIElement GetUIElement()
        {
            if (Values.Count == 0)
            {
                Values.Add("");
            }
            ComboBox cb = CreateComboBox(null, Values[0]);
            return cb;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            ComboBox cb = uiElement as ComboBox;
            Values[0] = cb.Text;
        }
        public override Operator Process(Core.Data.Geocache gc)
        {
            return GetOperators(gc.Country ?? "", Values.Count >= 0 ? Values[0] : "");
        }
    }

    public class ActionState : ActionImplementationCondition
    {
        public const string STR_NAME = "State";
        public ActionState()
            : base(STR_NAME)
        {
        }
        public override UIElement GetUIElement()
        {
            if (Values.Count == 0)
            {
                Values.Add("");
            }
            ComboBox cb = CreateComboBox(null, Values[0]);
            return cb;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            ComboBox cb = uiElement as ComboBox;
            Values[0] = cb.Text;
        }
        public override Operator Process(Core.Data.Geocache gc)
        {
            return GetOperators(gc.State ?? "", Values.Count >= 0 ? Values[0] : "");
        }
    }

    public class ActionMunicipality : ActionImplementationCondition
    {
        public const string STR_NAME = "Municipality";
        public ActionMunicipality()
            : base(STR_NAME)
        {
        }
        public override UIElement GetUIElement()
        {
            if (Values.Count == 0)
            {
                Values.Add("");
            }
            ComboBox cb = CreateComboBox(null, Values[0]);
            return cb;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            ComboBox cb = uiElement as ComboBox;
            Values[0] = cb.Text;
        }
        public override Operator Process(Core.Data.Geocache gc)
        {
            return GetOperators(gc.Municipality ?? "", Values.Count >= 0 ? Values[0] : "");
        }
    }

    public class ActionCity : ActionImplementationCondition
    {
        public const string STR_NAME = "City";
        public ActionCity()
            : base(STR_NAME)
        {
        }
        public override UIElement GetUIElement()
        {
            if (Values.Count == 0)
            {
                Values.Add("");
            }
            ComboBox cb = CreateComboBox(null, Values[0]);
            return cb;
        }

        public override void CommitUIData(UIElement uiElement)
        {
            ComboBox cb = uiElement as ComboBox;
            Values[0] = cb.Text;
        }
        public override Operator Process(Core.Data.Geocache gc)
        {
            return GetOperators(gc.City ?? "", Values.Count >= 0 ? Values[0] : "");
        }
    }


    //-----------------------------------------------------------------------------------------------------
    //-----------------------------------------------------------------------------------------------------

    /*
    public class ActionUpdateStatus : ActionImplementationAction
    {
        public const string STR_NAME = "Update status";
        public const string STR_ERROR = "Error";
        public const string STR_UNABLELIVE = "Unable to access the Live API or process its data";
        public const string STR_UPDATINGGEOCACHES = "Updating geocaches...";
        public const string STR_UPDATINGGEOCACHE = "Updating geocache...";

        private List<Core.Data.Geocache> _gcList = null;
        private string _errormessage = null;
        private ManualResetEvent _actionDone = null;

        public ActionUpdateStatus()
            : base(STR_NAME)
        {
        }

        public override void FinalizeRun()
        {
            if (GeocachesAtInputConnector.Count > 0)
            {
                _errormessage = "";
                _gcList = (from Core.Data.Geocache g in GeocachesAtInputConnector.Values select g).ToList();
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
            catch (Exception e)
            {
                _errormessage = e.Message;
            }
            _actionDone.Set();
        }

    }
    */

    /*
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
    */

    public class ActionSetArchived : ActionImplementationAction
    {
        public const string STR_NAME = "SetArchived";
        public ActionSetArchived()
            : base(STR_NAME)
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
                    foreach (Core.Data.Geocache gc in GeocachesAtInputConnector.Values)
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
        public const string STR_NAME = "SetAvailable";
        public ActionSetAvailable()
            : base(STR_NAME)
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
                    foreach (Core.Data.Geocache gc in GeocachesAtInputConnector.Values)
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
        public const string STR_NAME = "SetSelected";
        public ActionSetSelected()
            : base(STR_NAME)
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
                    foreach (Core.Data.Geocache gc in GeocachesAtInputConnector.Values)
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
        public const string STR_NAME = "SetFlagged";
        public ActionSetFlagged()
            : base(STR_NAME)
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
                    foreach (Core.Data.Geocache gc in GeocachesAtInputConnector.Values)
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
        public const string STR_NAME = "DeleteGeocache";
        public ActionDeleteGeocache()
            : base(STR_NAME)
        {
        }
        public override void FinalizeRun()
        {
            foreach (Core.Data.Geocache gc in GeocachesAtInputConnector.Values)
            {
                Utils.DataAccess.DeleteGeocache(gc);
            }
            base.FinalizeRun();
        }
    }



    public class ActionClearSelection : ActionImplementationExecuteOnce
    {
        public const string STR_NAME = "ClearSelection";
        public ActionClearSelection()
            : base(STR_NAME)
        {
        }
        protected override bool Execute()
        {
            foreach (Core.Data.Geocache gc in Core.ApplicationData.Instance.ActiveDatabase.GeocacheCollection)
            {
                gc.Selected = false;
            }
            return true;
        }
    }


}
