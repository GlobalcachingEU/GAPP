using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using System.Net;
using System.Collections.Specialized;
using System.IO;
using System.Web;

namespace GlobalcachingApplication.Plugins.ActBuilder
{
    public partial class ServerConnectionForm : Form
    {
        //private const string _baseUrl = "http://localhost:50017/ABService.aspx";
        private string _baseUrl = "http://application.globalcaching.eu/ABService.aspx";

        public const string STR_TITLE1 = "Action builder";
        public const string STR_TITLE2 = "Download and publish";
        public const string STR_SERVERFLOWS = "Hosted flows";
        public const string STR_YOURFLOWS = "Upload your flows";
        public const string STR_YOURUPLOADED = "Your uploaded flows";
        public const string STR_ALLPUBLICFLOWS = "All public flows";
        public const string STR_DESCRIPTION = "Description";
        public const string STR_NAME = "Name";
        public const string STR_AUTHOR = "Author";
        public const string STR_CREATED = "Created";
        public const string STR_MODIFIED = "Modified";
        public const string STR_PUBLIC = "Public";
        public const string STR_DOWNLOAD = "Download";
        public const string STR_DELETE = "Delete";
        public const string STR_FLOW = "Flow";
        public const string STR_UPLOAD = "Upload";
        public const string STR_OK = "OK";
        public const string STR_SERVERERROR = "Server Error";
        public const string STR_SERVERERROR_1 = "Unable to validate user";
        public const string STR_SERVERERROR_2 = "Unknown function call";
        public const string STR_SERVERERROR_3 = "Parameters in function call invalid";
        public const string STR_SERVERERROR_4 = "Flow does not exists or is uploaded by someone else";
        public const string STR_SERVERERROR_5 = "Flow is uploaded by someone else";
        public const string STR_SERVERERROR_6 = "You have reached the upload limit";
        public const string STR_SERVERERROR_7 = "Flow is uploaded by someone else";
        public const string STR_ERROR = "ERROR";
        public const string STR_NOSCRIPTS = "Flows with scripts cannot be shared with other due to security reasons";
        public const string STR_SUCCESS = "Success";
        public const string STR_DOWNLOADSUCCESS = "The flow has been imported";

        private ActionBuilder _owner = null;
        private Framework.Interfaces.ICore _core = null;

        public class FlowInfo
        {
            public string ID { get; set; }
            public string Name { get; set; }
            public string Author { get; set; }
            public bool IsPublic { get; set; }
            public string Description { get; set; }
            public DateTime Created { get; set; }
            public DateTime Modified { get; set; }
            public ActionBuilderForm.ActionFlow ActionFlow { get; set; }

            public override string ToString()
            {
                return Name ?? "";
            }
        }
        private List<FlowInfo> _flows = new List<FlowInfo>();
        private List<FlowInfo> _filteredflows = new List<FlowInfo>();

        public ServerConnectionForm()
        {
            InitializeComponent();
        }

        public ServerConnectionForm(ActionBuilder owner, Framework.Interfaces.ICore core)
            : this()
        {
            _owner = owner;
            _core = core;

            var af = (owner.ChildForm as ActionBuilderForm).AvailableActionFlows;
            if (af != null)
            {
                foreach (var a in af)
                {
                    ActionImplementation startAction = (from sa in a.Actions where sa is ActionStart select sa).FirstOrDefault();
                    if (startAction != null)
                    {
                        FlowInfo fi = new FlowInfo();
                        fi.Author = "";
                        fi.Created = DateTime.MinValue;
                        fi.Modified = DateTime.MinValue;
                        fi.Description = "";
                        fi.Name = a.Name;
                        fi.ID = startAction.ID;
                        fi.IsPublic = false;
                        fi.ActionFlow = a;
                        comboBox1.Items.Add(fi);
                    }
                }
            }

            flowsList1.flowList.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(flowList_SelectionChanged);

            this.Text = string.Format("{0} - {1}", Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE1), Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE2));
            this.groupBox2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SERVERFLOWS);
            this.groupBox3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_YOURFLOWS);
            this.radioButton1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_YOURUPLOADED);
            this.radioButton2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ALLPUBLICFLOWS);
            this.label3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DESCRIPTION);
            this.label6.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DESCRIPTION);
            flowsList1.flowList.Columns[0].Header = Utils.LanguageSupport.Instance.GetTranslation(STR_NAME);
            flowsList1.flowList.Columns[1].Header = Utils.LanguageSupport.Instance.GetTranslation(STR_AUTHOR);
            flowsList1.flowList.Columns[2].Header = Utils.LanguageSupport.Instance.GetTranslation(STR_CREATED);
            flowsList1.flowList.Columns[3].Header = Utils.LanguageSupport.Instance.GetTranslation(STR_MODIFIED);
            flowsList1.flowList.Columns[4].Header = Utils.LanguageSupport.Instance.GetTranslation(STR_PUBLIC);
            this.button3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DOWNLOAD);
            this.button4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DELETE);
            this.label4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_FLOW);
            this.button5.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_UPLOAD);
            this.checkBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_PUBLIC);
            this.button6.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_OK);
        }

        void flowList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            FlowInfo fi = flowsList1.flowList.SelectedItem as FlowInfo;
            if (fi == null)
            {
                button3.Enabled = false;
                button4.Enabled = false;
                label2.Text = "";
            }
            else
            {
                button3.Enabled = true;
                button4.Enabled = radioButton1.Checked;
                label2.Text = fi.Description;
            }
        }

        private int ServerCall(string func, Dictionary<string, string> parameters, out XmlDocument doc)
        {
            int result = -1;
            doc = null;
            try
            {
                using (WebClient wc = new WebClient())
                {
                    NameValueCollection formData = new NameValueCollection();
                    formData["token"] = HttpUtility.UrlEncode(_core.GeocachingComAccount.APIToken);
                    formData["func"] = HttpUtility.UrlEncode(func);
                    if (parameters != null)
                    {
                        foreach (var t in parameters)
                        {
                            formData[t.Key] = HttpUtility.UrlEncode(t.Value);
                        }
                    }
                    byte[] data = wc.UploadValues(_baseUrl, "POST", formData);

                    using (MemoryStream ms = new MemoryStream(data))
                    {
                        doc = new XmlDocument();
                        doc.Load(ms);
                        XmlElement root = doc.DocumentElement;
                        result = int.Parse(root.SelectSingleNode("status").InnerText);
                    }
                }
                if (result != 0)
                {
                    MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(string.Format("{0}_{1}",STR_SERVERERROR,result)), Utils.LanguageSupport.Instance.GetTranslation(STR_SERVERERROR), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch
            {
                MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_SERVERERROR), Utils.LanguageSupport.Instance.GetTranslation(STR_SERVERERROR), MessageBoxButtons.OK, MessageBoxIcon.Error);
                doc = null;
            }
            return result;
        }

        private void downloadData()
        {
            Cursor = Cursors.WaitCursor;
            _flows.Clear();
            _filteredflows.Clear();
            flowsList1.flowList.DataContext = null;
            try
            {
                XmlDocument doc;
                string func;
                if (radioButton1.Checked)
                {
                    func = "GETOWN";
                }
                else
                {
                    func = "GETPUBLIC";
                }
                if (ServerCall(func, null, out doc) == 0)
                {
                    XmlNodeList nl = doc.SelectNodes("/Response/flows/flow");
                    if (nl != null)
                    {
                        foreach (XmlNode n in nl)
                        {
                            FlowInfo fi = new FlowInfo();
                            fi.Author = n.SelectSingleNode("Username").InnerText;
                            fi.Created = DateTime.Parse(n.SelectSingleNode("Created").InnerText);
                            fi.Modified = DateTime.Parse(n.SelectSingleNode("Modified").InnerText);
                            fi.Description = n.SelectSingleNode("FlowDescription").InnerText;
                            fi.IsPublic = bool.Parse(n.SelectSingleNode("IsPublic").InnerText);
                            fi.Name = n.SelectSingleNode("FlowName").InnerText;
                            fi.ID = n.SelectSingleNode("FlowID").InnerText;

                            _flows.Add(fi);
                            if (textBox2.Text.Length == 0 ||
                                fi.Name.IndexOf(textBox2.Text, StringComparison.InvariantCultureIgnoreCase) >= 0 ||
                                fi.Author.IndexOf(textBox2.Text, StringComparison.InvariantCultureIgnoreCase) >= 0 ||
                                fi.Description.IndexOf(textBox2.Text, StringComparison.InvariantCultureIgnoreCase) >= 0)
                            {
                                _filteredflows.Add(fi);
                            }
                        }
                        flowsList1.flowList.DataContext = _filteredflows;
                    }
                }
            }
            catch
            {
                MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_SERVERERROR), Utils.LanguageSupport.Instance.GetTranslation(STR_SERVERERROR), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            Cursor = Cursors.Default;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;

            downloadData();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            FlowInfo fi = comboBox1.SelectedItem as FlowInfo;
            if (fi == null)
            {
                textBox1.Text = "";
                textBox1.Enabled = false;
                checkBox1.Enabled = false;
                button5.Enabled = false;
            }
            else
            {
                textBox1.Text = fi.Description;
                checkBox1.Checked = fi.IsPublic;
                textBox1.Enabled = true;
                checkBox1.Enabled = true;
                button5.Enabled = true;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            FlowInfo fi = comboBox1.SelectedItem as FlowInfo;
            if (fi != null)
            {
                ActionImplementation scriptAction = (from sa in fi.ActionFlow.Actions where sa is ActionScriptAction select sa).FirstOrDefault();
                ActionImplementation scriptCondition = (from sa in fi.ActionFlow.Actions where sa is ActionScript select sa).FirstOrDefault();
                if (!checkBox1.Checked || (scriptAction == null && scriptCondition == null))
                {

                    List<ActionBuilderForm.ActionFlow> flowList = new List<ActionBuilderForm.ActionFlow>();
                    flowList.Add(fi.ActionFlow);
                    XmlDocument content = (_owner.ChildForm as ActionBuilderForm).CreateFlowXml(flowList);
                    if (content != null)
                    {
                        fi.Description = textBox1.Text;
                        fi.IsPublic = checkBox1.Checked;

                        Dictionary<string, string> pars = new Dictionary<string, string>();
                        pars.Add("IsPublic", fi.IsPublic.ToString());
                        pars.Add("FlowID", fi.ID);
                        pars.Add("FlowName", fi.Name);
                        pars.Add("FlowDescription", fi.Description);
                        pars.Add("FlowContent", content.OuterXml);

                        XmlDocument doc;
                        if (ServerCall("STOREFLOW", pars, out doc) == 0)
                        {
                            //MessageBox.Show("SUCCESS");
                            if (radioButton1.Checked || fi.IsPublic)
                            {
                                downloadData();
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR), Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_NOSCRIPTS), Utils.LanguageSupport.Instance.GetTranslation(STR_SERVERERROR), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                downloadData();
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
            {
                downloadData();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            FlowInfo fi = flowsList1.flowList.SelectedItem as FlowInfo;
            if (fi != null)
            {
                Dictionary<string, string> pars = new Dictionary<string, string>();
                pars.Add("id", fi.ID);

                XmlDocument doc;
                if (ServerCall("DELETEFLOWID", pars, out doc) == 0)
                {
                    downloadData();
                }
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            flowsList1.flowList.DataContext = null;
            _filteredflows.Clear();
            foreach (var fi in _flows)
            {
                if (textBox2.Text.Length == 0 ||
                    fi.Name.IndexOf(textBox2.Text, StringComparison.InvariantCultureIgnoreCase) >= 0 ||
                    fi.Author.IndexOf(textBox2.Text, StringComparison.InvariantCultureIgnoreCase) >= 0 ||
                    fi.Description.IndexOf(textBox2.Text, StringComparison.InvariantCultureIgnoreCase) >= 0)
                {
                    _filteredflows.Add(fi);
                }
            }
            flowsList1.flowList.DataContext = _filteredflows;
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                button1_Click(this, EventArgs.Empty);
                e.Handled = true;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            FlowInfo fi = flowsList1.flowList.SelectedItem as FlowInfo;
            if (fi != null)
            {
                Dictionary<string, string> pars = new Dictionary<string, string>();
                pars.Add("id", fi.ID);

                XmlDocument doc;
                if (ServerCall("GETFLOWID", pars, out doc) == 0)
                {
                    try
                    {
                        //extract flow xml
                        XmlNode nl = doc.SelectSingleNode("/Response/FlowContent");
                        if (nl != null)
                        {
                            string xmlDoc = nl.InnerText;

                            //add to current flows
                            if ((_owner.ChildForm as ActionBuilderForm).ImportXmlFlow(xmlDoc))
                            {
                                //add to combo
                                comboBox1.Items.Clear();
                                var af = (_owner.ChildForm as ActionBuilderForm).AvailableActionFlows;
                                if (af != null)
                                {
                                    foreach (var a in af)
                                    {
                                        ActionImplementation startAction = (from sa in a.Actions where sa is ActionStart select sa).FirstOrDefault();
                                        if (startAction != null)
                                        {
                                            FlowInfo afi = new FlowInfo();
                                            afi.Author = "";
                                            afi.Created = DateTime.MinValue;
                                            afi.Modified = DateTime.MinValue;
                                            afi.Description = "";
                                            afi.Name = a.Name;
                                            afi.ID = startAction.ID;
                                            afi.IsPublic = false;
                                            afi.ActionFlow = a;
                                            comboBox1.Items.Add(afi);
                                        }
                                    }
                                }
                                comboBox1_SelectedIndexChanged(this, EventArgs.Empty);

                                if (radioButton1.Checked)
                                {
                                    downloadData();
                                }

                                MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_DOWNLOADSUCCESS), Utils.LanguageSupport.Instance.GetTranslation(STR_SUCCESS), MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                    catch
                    {
                        MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR), Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

    }
}
