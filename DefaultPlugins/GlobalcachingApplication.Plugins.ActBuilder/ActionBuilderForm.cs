using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.Collections;
using System.Xml;

namespace GlobalcachingApplication.Plugins.ActBuilder
{
    public partial class ActionBuilderForm : Utils.BasePlugin.BaseUIChildWindowForm
    {
        public static ActionBuilderForm ActionBuilderFormInstance = null;

        public const string STR_TITLE = "Action builder";
        public const string STR_FLOW = "Flow";
        public const string STR_EXECUTE = "Execute";
        public const string STR_DELETE = "Delete";
        public const string STR_NEW = "New";
        public const string STR_RENAME = "Rename";
        public const string STR_SAVE = "Save all";
        public const string STR_ACTIONS = "Actions";
        public const string STR_CONDITIONS = "Conditions";
        public const string STR_COPY = "Copy";
        public const string STR_EXECUTEONCE = "Execute once";
        public const string STR_FLOWEXECUTED = "Execution completed";
        public const string STR_DONOTSHOWAGAIN = "Do not show execution completed message";
        public const string STR_OK = "OK";
        public const string STR_OVERWRITE = "The flow already exists. Overwrite the existing flow?";
        public const string STR_WARNING = "Warning";
        public const string STR_DOWNLOADANDPUBLISH = "Download and publish";
        public const string STR_ERROR = "Error";
        public const string STR_ERRORIMPORTINGFLOW = "Error processing flow. The flow might have been created with a newer version of GAPP";
        public const string STR_START = "Start";

        public class ActionFlow
        {
            public string Name { get; set; }
            public List<ActionImplementation> Actions;

            public override string ToString()
            {
                return Name ?? "";
            }
        }
        private List<ActionFlow> _actionFlows = new List<ActionFlow>();
        private ActionFlow _activeFlow = null;
        private bool _sizeInitializing = true;
        private string _flowsFileName = null;
        private Label _executeOnceLabel = null;
        private object _clickedObject = null;

        public ActionBuilderForm()
        {
            InitializeComponent();
        }

        public ActionBuilderForm(Framework.Interfaces.IPlugin owner, Framework.Interfaces.ICore core)
            : base(owner, core)
        {
            InitializeComponent();

            ActionBuilderFormInstance = this;

            if (Properties.Settings.Default.UpgradeNeeded)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeNeeded = false;
                Properties.Settings.Default.Save();
            }

            if (Properties.Settings.Default.WindowPos != null && !Properties.Settings.Default.WindowPos.IsEmpty)
            {
                this.Bounds = Properties.Settings.Default.WindowPos;
                this.StartPosition = FormStartPosition.Manual;
            }

            actionBuilderEditor1.ShowConnectionLabels(Properties.Settings.Default.ShowConnectionLabel);

            _flowsFileName = System.IO.Path.Combine(new string[] { core.PluginDataPath, "FreeFlows.xml" });
            loadFlowsFile();

            splitContainer1.SplitterDistance = Properties.Settings.Default.LeftPanelWidth;
            if (splitContainer2.Width > Properties.Settings.Default.RightPanelWidth)
            {
                splitContainer2.SplitterDistance = splitContainer2.Width - Properties.Settings.Default.RightPanelWidth;
            }

            _sizeInitializing = false;

            try
            {
                Assembly asm = Assembly.GetExecutingAssembly();
                Type[] types = asm.GetTypes();
                foreach (Type t in types)
                {
                    if (t.IsClass && (t.BaseType == typeof(ActionImplementationCondition)))
                    {
                        ConstructorInfo constructor = t.GetConstructor(new Type[]{typeof(Framework.Interfaces.ICore)});
                        object[] parameters = new object[]{core};
                        ActionImplementationCondition obj = (ActionImplementationCondition)constructor.Invoke(parameters);

                        //exception for the start
                        if (obj is ActionStart)
                        {
                            //skip. auto
                        }
                        else
                        {
                            Button b = new Button();
                            b.Dock = DockStyle.Top;
                            b.Tag = obj;
                            b.Height = 25;
                            b.Click += new EventHandler(b_Click);
                            b.MouseDown += new MouseEventHandler(b_MouseDown);
                            b.MouseMove += new MouseEventHandler(b_MouseMove);
                            panel3.Controls.Add(b);

                            core.LanguageItems.Add(new Framework.Data.LanguageItem(obj.Name));
                        }
                    }
                }
                foreach (Type t in types)
                {
                    if (t.IsClass && (t.BaseType == typeof(ActionImplementationExecuteOnce)))
                    {
                        ConstructorInfo constructor = t.GetConstructor(new Type[] { typeof(Framework.Interfaces.ICore) });
                        object[] parameters = new object[] { core };
                        ActionImplementationExecuteOnce obj = (ActionImplementationExecuteOnce)constructor.Invoke(parameters);

                        Button b = new Button();
                        b.Dock = DockStyle.Top;
                        b.Tag = obj;
                        b.Height = 25;
                        b.Click += new EventHandler(b_Click);
                        b.MouseDown += new MouseEventHandler(b_MouseDown);
                        b.MouseMove += new MouseEventHandler(b_MouseMove);
                        panel5.Controls.Add(b);

                        core.LanguageItems.Add(new Framework.Data.LanguageItem(obj.Name));
                    }
                }
                Label la = new Label();
                la.AutoSize = false;
                la.TextAlign = ContentAlignment.MiddleCenter;
                la.Dock = DockStyle.Top;
                la.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_EXECUTEONCE);
                panel5.Controls.Add(la);
                _executeOnceLabel = la;
                foreach (Type t in types)
                {
                    if (t.IsClass && (t.BaseType == typeof(ActionImplementationAction)))
                    {
                        ConstructorInfo constructor = t.GetConstructor(new Type[] { typeof(Framework.Interfaces.ICore) });
                        object[] parameters = new object[] { core };
                        ActionImplementationAction obj = (ActionImplementationAction)constructor.Invoke(parameters);

                        //exception for the start
                        Button b = new Button();
                        b.Dock = DockStyle.Top;
                        b.Tag = obj;
                        b.Height = 25;
                        b.Click += new EventHandler(b_Click);
                        b.MouseDown += new MouseEventHandler(b_MouseDown);
                        b.MouseMove += new MouseEventHandler(b_MouseMove);
                        panel5.Controls.Add(b);

                        core.LanguageItems.Add(new Framework.Data.LanguageItem(obj.Name));
                    }
                }
            }
            catch
            {
            }

            SelectedLanguageChanged(this, EventArgs.Empty);
        }

        void b_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left && _clickedObject == sender)
            {
                Button b = sender as Button;
                if (b != null)
                {
                    ActionImplementation ai = b.Tag as ActionImplementation;
                    if (ai != null)
                    {
                        b.DoDragDrop(b.Tag.GetType().ToString(), DragDropEffects.Move);
                        _clickedObject = null;
                    }
                }
            }
        }

        void b_MouseDown(object sender, MouseEventArgs e)
        {
            _clickedObject = sender;
        }

        public List<ActionFlow> AvailableActionFlows
        {
            get { return _actionFlows; }
        }

        public void ApplicationInitialized()
        {
            Framework.Interfaces.IPluginUIMainWindow main = (from Framework.Interfaces.IPluginUIMainWindow a in Core.GetPlugin(Framework.PluginType.UIMainWindow) select a).FirstOrDefault();
            foreach (ActionFlow af in _actionFlows)
            {
                (this.OwnerPlugin as ActionBuilder).AddNewAction(af.Name);
                main.AddAction(OwnerPlugin, "Action builder", af.Name);
            }
        }

        public void SettingsChanged()
        {
            actionBuilderEditor1.ShowConnectionLabels(Properties.Settings.Default.ShowConnectionLabel);
        }

        public bool ImportXmlFlow(string xmlDoc)
        {
            bool result = false;
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xmlDoc);
                List<ActionFlow> flows = GetActionFlowsFromXml(doc);
                if (flows != null && flows.Count > 0)
                {
                    //for now assume 1
                    ActionFlow naf = flows[0];

                    //check for ID and name
                    ActionImplementation startAction = (from sa in naf.Actions where sa is ActionStart select sa).FirstOrDefault();
                    if (startAction != null)
                    {
                        ActionFlow found = null;
                        foreach (var af in _actionFlows)
                        {
                            ActionImplementation startAct = (from sa in af.Actions where sa is ActionStart select sa).FirstOrDefault();
                            if (startAct != null)
                            {
                                if (startAct.ID == startAction.ID)
                                {
                                    found = af;
                                    break;
                                }
                            }
                        }
                        if (found != null)
                        {
                            //ask overwrite
                            DialogResult dr = MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_OVERWRITE), Utils.LanguageSupport.Instance.GetTranslation(STR_WARNING), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                            if (dr == System.Windows.Forms.DialogResult.Cancel)
                            {
                                return result;
                            }
                            else if (dr == System.Windows.Forms.DialogResult.Yes)
                            {
                                //remove old
                                comboBox1.Items.Remove(found);
                                _actionFlows.Remove(found);

                                (this.OwnerPlugin as ActionBuilder).DeleteAction(found.Name);
                                Framework.Interfaces.IPluginUIMainWindow main = (from Framework.Interfaces.IPluginUIMainWindow a in Core.GetPlugin(Framework.PluginType.UIMainWindow) select a).FirstOrDefault();
                                main.RemoveAction(OwnerPlugin, "Action builder", found.Name);

                                //insert new
                                //but first check name
                                int index = 0;
                                while ((from a in _actionFlows where a.Name.ToLower() == naf.Name.ToLower() select a).Count() > 0)
                                {
                                    index++;
                                    naf.Name = string.Format("{0}{1}", naf.Name, index);
                                }
                                _actionFlows.Add(naf);

                                (this.OwnerPlugin as ActionBuilder).AddNewAction(naf.Name);
                                main.AddAction(OwnerPlugin, "Action builder", naf.Name);

                                comboBox1.Items.Add(naf);

                                comboBox1_SelectedIndexChanged(this, EventArgs.Empty);

                                result = true;
                            }
                            else
                            {
                                //copy and add
                                //but first check name
                                int index = 0;
                                while ((from a in _actionFlows where a.Name.ToLower() == naf.Name.ToLower() select a).Count() > 0)
                                {
                                    index++;
                                    naf.Name = string.Format("{0}{1}", naf.Name, index);
                                }

                                naf = CopyAs(naf, naf.Name);

                                _actionFlows.Add(naf);

                                (this.OwnerPlugin as ActionBuilder).AddNewAction(naf.Name);
                                Framework.Interfaces.IPluginUIMainWindow main = (from Framework.Interfaces.IPluginUIMainWindow a in Core.GetPlugin(Framework.PluginType.UIMainWindow) select a).FirstOrDefault();
                                main.AddAction(OwnerPlugin, "Action builder", naf.Name);

                                comboBox1.Items.Add(naf);

                                result = true;
                            }
                        }
                        else
                        {
                            //add
                            //but first check name
                            int index = 0;
                            while ((from a in _actionFlows where a.Name.ToLower() == naf.Name.ToLower() select a).Count() > 0)
                            {
                                index++;
                                naf.Name = string.Format("{0}{1}", naf.Name, index);
                            }
                            _actionFlows.Add(naf);

                            (this.OwnerPlugin as ActionBuilder).AddNewAction(naf.Name);
                            Framework.Interfaces.IPluginUIMainWindow main = (from Framework.Interfaces.IPluginUIMainWindow a in Core.GetPlugin(Framework.PluginType.UIMainWindow) select a).FirstOrDefault();
                            main.AddAction(OwnerPlugin, "Action builder", naf.Name);

                            comboBox1.Items.Add(naf);

                            result = true;
                        }
                    }
                    button14_Click(this, EventArgs.Empty);
                }
            }
            catch
            {
                MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_ERRORIMPORTINGFLOW), Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return result;
        }

        private List<ActionFlow> GetActionFlowsFromXml(XmlDocument doc)
        {
            List<ActionFlow> result = new List<ActionFlow>();
            List<ActionImplementation> allActionImpl = new List<ActionImplementation>();

            XmlNodeList nl = doc.SelectNodes("/flows/flow");
            foreach (XmlNode n in nl)
            {
                ActionFlow af = new ActionFlow();
                af.Name = n.Attributes["name"].Value;
                af.Actions = new List<ActionImplementation>();

                XmlNodeList al = n.SelectNodes("action");
                foreach (XmlNode a in al)
                {
                    Type t = Type.GetType(a.Attributes["type"].Value);
                    ConstructorInfo constructor = t.GetConstructor(new Type[] { typeof(Framework.Interfaces.ICore) });
                    object[] parameters = new object[] { Core };
                    ActionImplementation obj = (ActionImplementation)constructor.Invoke(parameters);
                    obj.ID = a.Attributes["id"].Value;
                    obj.Location = new System.Windows.Point((double)int.Parse(a.Attributes["x"].Value), (double)int.Parse(a.Attributes["y"].Value));

                    af.Actions.Add(obj);
                    allActionImpl.Add(obj);

                    XmlNodeList vl = a.SelectNodes("values/value");
                    foreach (XmlNode v in vl)
                    {
                        obj.Values.Add(v.InnerText);
                    }
                }

                result.Add(af);
            }

            //all actions are created, now we can match the ID's for the connections.
            nl = doc.SelectNodes("/flows/flow/action");
            foreach (XmlNode n in nl)
            {
                ActionImplementation ai = (from ac in allActionImpl where ac.ID == n.Attributes["id"].Value select ac).FirstOrDefault();
                XmlNodeList conl;
                ActionImplementation.Operator op = ai.AllowOperators;
                if ((op & ActionImplementation.Operator.Equal) != 0)
                {
                    conl = n.SelectNodes("Equal/ID");
                    foreach (XmlNode con in conl)
                    {
                        ai.ConnectToOutput((from ac in allActionImpl where ac.ID == con.InnerText select ac).FirstOrDefault(), ActionImplementation.Operator.Equal);
                    }
                }
                if ((op & ActionImplementation.Operator.Larger) != 0)
                {
                    conl = n.SelectNodes("Larger/ID");
                    foreach (XmlNode con in conl)
                    {
                        ai.ConnectToOutput((from ac in allActionImpl where ac.ID == con.InnerText select ac).FirstOrDefault(), ActionImplementation.Operator.Larger);
                    }
                }
                if ((op & ActionImplementation.Operator.LargerOrEqual) != 0)
                {
                    conl = n.SelectNodes("LargerOrEqual/ID");
                    foreach (XmlNode con in conl)
                    {
                        ai.ConnectToOutput((from ac in allActionImpl where ac.ID == con.InnerText select ac).FirstOrDefault(), ActionImplementation.Operator.LargerOrEqual);
                    }
                }
                if ((op & ActionImplementation.Operator.Less) != 0)
                {
                    conl = n.SelectNodes("Less/ID");
                    foreach (XmlNode con in conl)
                    {
                        ai.ConnectToOutput((from ac in allActionImpl where ac.ID == con.InnerText select ac).FirstOrDefault(), ActionImplementation.Operator.Less);
                    }
                }
                if ((op & ActionImplementation.Operator.LessOrEqual) != 0)
                {
                    conl = n.SelectNodes("LessOrEqual/ID");
                    foreach (XmlNode con in conl)
                    {
                        ai.ConnectToOutput((from ac in allActionImpl where ac.ID == con.InnerText select ac).FirstOrDefault(), ActionImplementation.Operator.LessOrEqual);
                    }
                }
                if ((op & ActionImplementation.Operator.NotEqual) != 0)
                {
                    conl = n.SelectNodes("NotEqual/ID");
                    foreach (XmlNode con in conl)
                    {
                        ai.ConnectToOutput((from ac in allActionImpl where ac.ID == con.InnerText select ac).FirstOrDefault(), ActionImplementation.Operator.NotEqual);
                    }
                }
            }
            return result;
        }

        private void loadFlowsFile()
        {
            //_flowsFileName
            try
            {
                if (System.IO.File.Exists(_flowsFileName))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(_flowsFileName);
                    _actionFlows = GetActionFlowsFromXml(doc);
                    foreach (var af in _actionFlows)
                    {
                        comboBox1.Items.Add(af);
                    }
                }
            }
            catch
            {
            }
        }

        void b_Click(object sender, EventArgs e)
        {
            Type t = (sender as Button).Tag.GetType();
            ConstructorInfo constructor = t.GetConstructor(new Type[] { typeof(Framework.Interfaces.ICore) });
            object[] parameters = new object[] { Core };
            ActionImplementation obj = (ActionImplementation)constructor.Invoke(parameters);
            obj.ID = Guid.NewGuid().ToString("N");
            _activeFlow.Actions.Add(obj);
            actionBuilderEditor1.AddActionControl(obj);
        }

        protected override void SelectedLanguageChanged(object sender, EventArgs e)
        {
            base.SelectedLanguageChanged(sender, e);

            try
            {
                this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
                this.label3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_FLOW);
                this.button1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_EXECUTE);
                this.button12.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DELETE);
                this.button2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NEW);
                this.button13.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_RENAME);
                this.button14.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SAVE);
                this.label2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ACTIONS);
                this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CONDITIONS);
                this.button5.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_COPY);
                if (_executeOnceLabel != null)
                {
                    _executeOnceLabel.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_EXECUTEONCE);
                }
                toolTip1.SetToolTip(button15,Utils.LanguageSupport.Instance.GetTranslation(STR_DOWNLOADANDPUBLISH));

                List<Button> pnlButtons = new List<Button>();
                panel3.SuspendLayout();
                foreach (Control cnt in panel3.Controls)
                {
                    if (cnt is Button)
                    {
                        Button b = cnt as Button;
                        if (b.Tag is ActionImplementationCondition)
                        {
                            b.Text = Utils.LanguageSupport.Instance.GetTranslation((b.Tag as ActionImplementationCondition).Name);
                            pnlButtons.Add(b);
                        }
                    }
                }
                foreach (Button b in pnlButtons)
                {
                    panel3.Controls.Remove(b);
                }
                panel3.Controls.AddRange(pnlButtons.OrderByDescending(x => x.Text).ToArray());
                panel3.ResumeLayout(true);


                foreach (Control cnt in panel5.Controls)
                {
                    if (cnt is Button)
                    {
                        Button b = cnt as Button;
                        if (b.Tag is ActionImplementation)
                        {
                            b.Text = Utils.LanguageSupport.Instance.GetTranslation((b.Tag as ActionImplementation).Name);
                        }
                    }
                }

                if (_activeFlow != null)
                {
                    foreach (var ai in _activeFlow.Actions)
                    {
                        ai.SelectedLanguageChanged();
                    }
                }
            }
            catch
            {
            }
        }

        private void ActionBuilderForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void checkButtonStates()
        {
            //new / rename
            if (textBox1.Text.Length == 0)
            {
                button2.Enabled = false;
                button13.Enabled = false;
            }
            else
            {
                ActionFlow fq = (from f in _actionFlows where f.Name == textBox1.Text select f).FirstOrDefault();
                button2.Enabled = (fq == null);
                button13.Enabled = (fq == null && _activeFlow != null);
            }
            button1.Enabled = (_activeFlow != null);
            button12.Enabled = (_activeFlow != null);
            panel3.Enabled = (_activeFlow != null);
            panel5.Enabled = (_activeFlow != null);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ActionFlow af = new ActionFlow();
            af.Name = textBox1.Text;
            af.Actions = new List<ActionImplementation>();
            var obj = new ActionStart(Core);
            obj.ID = Guid.NewGuid().ToString("N");
            af.Actions.Add(obj);
            _actionFlows.Add(af);

            (this.OwnerPlugin as ActionBuilder).AddNewAction(af.Name);
            Framework.Interfaces.IPluginUIMainWindow main = (from Framework.Interfaces.IPluginUIMainWindow a in Core.GetPlugin(Framework.PluginType.UIMainWindow) select a).FirstOrDefault();
            main.AddAction(OwnerPlugin, "Action builder", af.Name);

            comboBox1.Items.Add(af);
            comboBox1.SelectedIndex = comboBox1.Items.Count - 1;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            actionBuilderEditor1.ResetScale();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            actionBuilderEditor1.scaleDown();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            actionBuilderEditor1.scaleUp();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            actionBuilderEditor1.moveUp();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            actionBuilderEditor1.moveDown();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            actionBuilderEditor1.ResetPosition();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            actionBuilderEditor1.moveLeft();
        }

        private void button11_Click(object sender, EventArgs e)
        {
            actionBuilderEditor1.moveRight();
        }

        private void ActionBuilderForm_KeyDown(object sender, KeyEventArgs e)
        {
            actionBuilderEditor1.OnKeyDown(e.KeyCode);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            checkButtonStates();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            _activeFlow = comboBox1.SelectedItem as ActionFlow;
            if (_activeFlow==null)
            {
                actionBuilderEditor1.CommitData();
                actionBuilderEditor1.Clear(null);
            }
            else
            {
                actionBuilderEditor1.CommitData();
                actionBuilderEditor1.Clear(_activeFlow.Actions);
                ActionImplementation startAction = (from a in _activeFlow.Actions where a is ActionStart select a).FirstOrDefault();
                if (startAction != null)
                {
                    if (startAction.UIActionControl != null)
                    {
                        startAction.UIActionControl.Title.Content = string.Format("{0}\r\n{1}", Utils.LanguageSupport.Instance.GetTranslation(STR_START), _activeFlow.Name);
                    }
                }
            }
            checkButtonStates();
        }

        private void ActionBuilderForm_LocationChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                Properties.Settings.Default.WindowPos = this.Bounds;
                Properties.Settings.Default.Save();
            }
        }

        private void ActionBuilderForm_SizeChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                Properties.Settings.Default.WindowPos = this.Bounds;
                Properties.Settings.Default.Save();
            }
        }

        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            if (!_sizeInitializing)
            {
                Properties.Settings.Default.LeftPanelWidth = splitContainer1.SplitterDistance;
                Properties.Settings.Default.Save();
            }
        }

        private void splitContainer2_SplitterMoved(object sender, SplitterEventArgs e)
        {
            if (!_sizeInitializing)
            {
                Properties.Settings.Default.RightPanelWidth = splitContainer2.Width - splitContainer2.SplitterDistance;
                Properties.Settings.Default.Save();
            }
        }


        public XmlDocument CreateFlowXml(List<ActionFlow> flows)
        {
            XmlDocument doc;
            actionBuilderEditor1.CommitData();
            try
            {
                doc = new XmlDocument();
                XmlElement root = doc.CreateElement("flows");
                doc.AppendChild(root);
                foreach (ActionFlow af in flows)
                {
                    XmlElement q = doc.CreateElement("flow");
                    XmlAttribute attr = doc.CreateAttribute("name");
                    XmlText txt = doc.CreateTextNode(af.Name);
                    attr.AppendChild(txt);
                    q.Attributes.Append(attr);
                    root.AppendChild(q);
                    foreach (ActionImplementation ai in af.Actions)
                    {
                        XmlElement r = doc.CreateElement("action");
                        q.AppendChild(r);

                        attr = doc.CreateAttribute("type");
                        txt = doc.CreateTextNode(ai.GetType().ToString());
                        attr.AppendChild(txt);
                        r.Attributes.Append(attr);

                        attr = doc.CreateAttribute("id");
                        txt = doc.CreateTextNode(ai.ID);
                        attr.AppendChild(txt);
                        r.Attributes.Append(attr);

                        attr = doc.CreateAttribute("x");
                        txt = doc.CreateTextNode(((int)ai.Location.X).ToString());
                        attr.AppendChild(txt);
                        r.Attributes.Append(attr);

                        attr = doc.CreateAttribute("y");
                        txt = doc.CreateTextNode(((int)ai.Location.Y).ToString());
                        attr.AppendChild(txt);
                        r.Attributes.Append(attr);

                        XmlElement v = doc.CreateElement("values");
                        r.AppendChild(v);
                        foreach (string s in ai.Values)
                        {
                            XmlElement val = doc.CreateElement("value");
                            v.AppendChild(val);

                            txt = doc.CreateTextNode(s);
                            val.AppendChild(txt);
                        }

                        List<ActionImplementation> actImpl = ai.GetOutputConnections(ActionImplementation.Operator.Equal);
                        v = doc.CreateElement("Equal");
                        r.AppendChild(v);
                        foreach (ActionImplementation act in actImpl)
                        {
                            XmlElement val = doc.CreateElement("ID");
                            v.AppendChild(val);

                            txt = doc.CreateTextNode(act.ID);
                            val.AppendChild(txt);
                        }

                        actImpl = ai.GetOutputConnections(ActionImplementation.Operator.Larger);
                        v = doc.CreateElement("Larger");
                        r.AppendChild(v);
                        foreach (ActionImplementation act in actImpl)
                        {
                            XmlElement val = doc.CreateElement("ID");
                            v.AppendChild(val);

                            txt = doc.CreateTextNode(act.ID);
                            val.AppendChild(txt);
                        }

                        actImpl = ai.GetOutputConnections(ActionImplementation.Operator.LargerOrEqual);
                        v = doc.CreateElement("LargerOrEqual");
                        r.AppendChild(v);
                        foreach (ActionImplementation act in actImpl)
                        {
                            XmlElement val = doc.CreateElement("ID");
                            v.AppendChild(val);

                            txt = doc.CreateTextNode(act.ID);
                            val.AppendChild(txt);
                        }

                        actImpl = ai.GetOutputConnections(ActionImplementation.Operator.Less);
                        v = doc.CreateElement("Less");
                        r.AppendChild(v);
                        foreach (ActionImplementation act in actImpl)
                        {
                            XmlElement val = doc.CreateElement("ID");
                            v.AppendChild(val);

                            txt = doc.CreateTextNode(act.ID);
                            val.AppendChild(txt);
                        }

                        actImpl = ai.GetOutputConnections(ActionImplementation.Operator.LessOrEqual);
                        v = doc.CreateElement("LessOrEqual");
                        r.AppendChild(v);
                        foreach (ActionImplementation act in actImpl)
                        {
                            XmlElement val = doc.CreateElement("ID");
                            v.AppendChild(val);

                            txt = doc.CreateTextNode(act.ID);
                            val.AppendChild(txt);
                        }

                        actImpl = ai.GetOutputConnections(ActionImplementation.Operator.NotEqual);
                        v = doc.CreateElement("NotEqual");
                        r.AppendChild(v);
                        foreach (ActionImplementation act in actImpl)
                        {
                            XmlElement val = doc.CreateElement("ID");
                            v.AppendChild(val);

                            txt = doc.CreateTextNode(act.ID);
                            val.AppendChild(txt);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                doc = null;
                MessageBox.Show(ex.Message);
            }
            return doc;
        }

        private void button14_Click(object sender, EventArgs e)
        {
            XmlDocument doc = CreateFlowXml(_actionFlows);
            if (doc != null)
            {
                try
                {
                    doc.Save(_flowsFileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            if (_activeFlow != null)
            {
                comboBox1.Items.Remove(_activeFlow);
                _actionFlows.Remove(_activeFlow);

                (this.OwnerPlugin as ActionBuilder).DeleteAction(_activeFlow.Name);
                Framework.Interfaces.IPluginUIMainWindow main = (from Framework.Interfaces.IPluginUIMainWindow a in Core.GetPlugin(Framework.PluginType.UIMainWindow) select a).FirstOrDefault();
                main.RemoveAction(OwnerPlugin, "Action builder", _activeFlow.Name);

                comboBox1_SelectedIndexChanged(this, EventArgs.Empty);
            }
        }

        private void button13_Click(object sender, EventArgs e)
        {
            ActionFlow fq = _activeFlow;
            if (fq != null)
            {
                (this.OwnerPlugin as ActionBuilder).DeleteAction(fq.Name);
                Framework.Interfaces.IPluginUIMainWindow main = (from Framework.Interfaces.IPluginUIMainWindow a in Core.GetPlugin(Framework.PluginType.UIMainWindow) select a).FirstOrDefault();
                main.RemoveAction(OwnerPlugin, "Action builder", fq.Name);

                fq.Name = textBox1.Text;
                (this.OwnerPlugin as ActionBuilder).AddNewAction(fq.Name);
                main.AddAction(OwnerPlugin, "Action builder", fq.Name);

                typeof(ComboBox).InvokeMember("RefreshItems", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, comboBox1, new object[] { });

                checkButtonStates();
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            actionBuilderEditor1.CommitData();
            if (_activeFlow != null)
            {
                this.Cursor = Cursors.WaitCursor;
                RunActionFlow(_activeFlow, true);
                this.Cursor = Cursors.Default;
            }
            actionBuilderEditor1.UpdateLabels();
        }

        public void RunActionFlow(string name)
        {
            RunActionFlow(name, false);
        }

        public void RunActionFlow(string name, bool notifyDone)
        {
            actionBuilderEditor1.CommitData();
            ActionFlow af = (from c in _actionFlows where c.Name == name select c).FirstOrDefault();
            if (af != null)
            {
                RunActionFlow(af, notifyDone);
            }
            actionBuilderEditor1.UpdateLabels();
        }

        private void RunActionFlow(ActionFlow flow)
        {
            RunActionFlow(flow, false);
        }
        private void RunActionFlow(ActionFlow flow, bool notifyDone)
        {
            try
            {
                using (Utils.FrameworkDataUpdater upd = new Utils.FrameworkDataUpdater(Core))
                {
                    //flows can call eachother, initialize them all
                    foreach (ActionFlow af in _actionFlows)
                    {
                        foreach (ActionImplementation ai in af.Actions)
                        {
                            ai.PrepareRun();
                        }
                    }
                    //find start and run
                    ActionStart startAction = (from a in flow.Actions where a is ActionStart select a).FirstOrDefault() as ActionStart;
                    if (notifyDone)
                    {
                        //first start
                        startAction.PrepareFlow();
                    }
                    foreach (Framework.Data.Geocache gc in Core.Geocaches)
                    {
                        startAction.Run(gc);
                    }
                    //wrap up
                    foreach (ActionFlow af in _actionFlows)
                    {
                        foreach (ActionImplementation ai in af.Actions)
                        {
                            ai.FinalizeRun();
                        }
                    }
                }
                if (notifyDone && Properties.Settings.Default.ShowFlowCompletedMessage)
                {
                    using (ExecutionCompletedForm dlg = new ExecutionCompletedForm())
                    {
                        dlg.ShowDialog();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button13_EnabledChanged(object sender, EventArgs e)
        {
            button5.Enabled = button13.Enabled;
        }

        private ActionFlow CopyAs(ActionFlow fq, string newName)
        {
            ActionFlow target = new ActionFlow();
            target.Name = newName;
            target.Actions = new List<ActionImplementation>();

            Hashtable renamedIds = new Hashtable();
            foreach (ActionImplementation ai in fq.Actions)
            {
                Type t = ai.GetType();
                ConstructorInfo constructor = t.GetConstructor(new Type[] { typeof(Framework.Interfaces.ICore) });
                object[] parameters = new object[] { Core };
                ActionImplementation obj = (ActionImplementation)constructor.Invoke(parameters);
                obj.ID = Guid.NewGuid().ToString("N");
                obj.Location = new System.Windows.Point(ai.Location.X, ai.Location.Y);
                obj.Values.AddRange(ai.Values);

                renamedIds.Add(ai.ID, obj.ID);
                target.Actions.Add(obj);

            }
            foreach (ActionImplementation ai in fq.Actions)
            {
                string actId = (string)renamedIds[ai.ID];
                ActionImplementation act = (from a in target.Actions where a.ID == actId select a).FirstOrDefault();
                List<ActionImplementation.OutputConnectionInfo> srcCons = ai.GetOutputConnections();
                List<ActionImplementation.OutputConnectionInfo> dstCons = act.GetOutputConnections();
                foreach (ActionImplementation.OutputConnectionInfo con in srcCons)
                {
                    if (con.ConnectedAction != null)
                    {
                        ActionImplementation.OutputConnectionInfo nCon = new ActionImplementation.OutputConnectionInfo();
                        nCon.OutputOperator = con.OutputOperator;
                        string conToActId = renamedIds[con.ConnectedAction.ID] as string;
                        if (conToActId != null)
                        {
                            nCon.ConnectedAction = (from a in target.Actions where a.ID == conToActId select a).FirstOrDefault();
                        }
                        else
                        {
                            nCon.ConnectedAction = con.ConnectedAction;
                        }
                        dstCons.Add(nCon);
                    }
                }
            }
            return target;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            ActionFlow fq = _activeFlow;
            if (fq != null)
            {
                ActionFlow target = CopyAs(fq, textBox1.Text);

                _actionFlows.Add(target);

                (this.OwnerPlugin as ActionBuilder).AddNewAction(target.Name);
                Framework.Interfaces.IPluginUIMainWindow main = (from Framework.Interfaces.IPluginUIMainWindow a in Core.GetPlugin(Framework.PluginType.UIMainWindow) select a).FirstOrDefault();
                main.AddAction(OwnerPlugin, "Action builder", target.Name);

                comboBox1.Items.Add(target);
                comboBox1.SelectedIndex = comboBox1.Items.Count - 1;
            }
        }

        private void button15_Click(object sender, EventArgs e)
        {
            if (Utils.API.GeocachingLiveV6.CheckAPIAccessAvailable(Core, false))
            {
                using (ServerConnectionForm dlg = new ServerConnectionForm(this.OwnerPlugin as ActionBuilder, Core))
                {
                    dlg.ShowDialog();
                }
            }
        }

    }
}
