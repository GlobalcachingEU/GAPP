using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.APIBookmark
{
    public partial class ImportForm : Form
    {
        public const string STR_TITLE = "Import geocaches in bookmark";
        public const string STR_ERROR = "Error";
        public const string STR_BOOKMARKS = "Bookmarks";
        public const string STR_IMPORT = "Import";
        public const string STR_REMOVE = "Remove";
        public const string STR_NEWBOOKMARK = "New bookmark";
        public const string STR_NAME = "Name";
        public const string STR_URL = "Url";
        public const string STR_EG = "E.g.";
        public const string STR_IMPORTMISSING = "Import missing";

        private Framework.Interfaces.ICore _core = null;
        private List<string> _gcCodes = null;

        public ImportForm()
        {
            InitializeComponent();
        }

        public ImportForm(Framework.Interfaces.ICore core)
            : this()
        {
            _core = core;

            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.groupBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_BOOKMARKS);
            this.button2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_IMPORT);
            this.button3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_REMOVE);
            this.button1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_IMPORT);
            this.button5.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_IMPORTMISSING);
            this.button4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_IMPORTMISSING);
            this.groupBox2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NEWBOOKMARK);
            this.label7.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NAME);
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_URL);
            this.label4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_EG);

            loadBookmarks();
        }

        public ImportForm(Framework.Interfaces.ICore core, string name, string url)
            : this(core)
        {
            textBox2.Text = name;
            textBox1.Text = url;
        }

        private void loadBookmarks()
        {
            BookmarkInfo[] bis = BookmarkInfoList.Instance(_core).Bookmarks;
            if (bis!=null && bis.Length>0)
            {
                listBox1.Items.AddRange(bis);
            }
        }

        public List<string> SelectedGCCodes
        {
            get { return _gcCodes; }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Guid guid = getGuid();
            if (getBookmarkList(guid))
            {
                BookmarkInfo bi = new BookmarkInfo();
                bi.Name = textBox2.Text;
                bi.Guid = guid.ToString();
                bi.GeocacheCodes = _gcCodes.ToList();
                BookmarkInfoList.Instance(_core).AddBookmark(bi);
                DialogResult = System.Windows.Forms.DialogResult.OK;
                Close();
            }
        }

        private bool getBookmarkList(Guid guid)
        {
            bool result = false;

            Cursor = Cursors.WaitCursor;
            try
            {
                using (Utils.API.GeocachingLiveV6 api = new Utils.API.GeocachingLiveV6(_core))
                {
                    var req = new Utils.API.LiveV6.GetBookmarkListByGuidRequest();
                    req.AccessToken = api.Token;
                    req.BookmarkListGuid = guid;
                    var resp = api.Client.GetBookmarkListByGuid(req);
                    if (resp.Status.StatusCode == 0)
                    {
                        _gcCodes = (from c in resp.BookmarkList select c.CacheCode).ToList();
                        result = true;
                    }
                    else
                    {
                        MessageBox.Show(resp.Status.StatusMessage, Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR));
                    }
                }
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message, Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR));
            }
            Cursor = Cursors.Default;

            return result;
        }

        private Guid getGuid()
        {
            if (textBox1.Text.Length > 0)
            {
                int pos = textBox1.Text.IndexOf('=');
                if (pos > 0 && pos < textBox1.Text.Length-1)
                {
                    Guid guid = new Guid();
                    string s = textBox1.Text.Substring(pos + 1);
                    if (Guid.TryParse(s, out guid))
                    {
                        return guid;
                    }
                }
            }
            return Guid.Empty;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            button1.Enabled = getGuid() != Guid.Empty && textBox2.Text.Length > 0;
            button5.Enabled = button1.Enabled;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            button1.Enabled = getGuid() != Guid.Empty && textBox2.Text.Length > 0;
            button5.Enabled = button1.Enabled;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            button2.Enabled = listBox1.SelectedItem != null;
            button3.Enabled = listBox1.SelectedItem != null;
            button4.Enabled = listBox1.SelectedItem != null;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            BookmarkInfo bi = listBox1.SelectedItem as BookmarkInfo;
            if (bi != null)
            {
                BookmarkInfoList.Instance(_core).RemoveBookmark(bi);
                listBox1.Items.Remove(bi);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            BookmarkInfo bi = listBox1.SelectedItem as BookmarkInfo;
            if (bi != null)
            {
                if (getBookmarkList(Guid.Parse(bi.Guid)))
                {
                    bi.GeocacheCodes = _gcCodes.ToList();
                    DialogResult = System.Windows.Forms.DialogResult.OK;
                    Close();
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            BookmarkInfo bi = listBox1.SelectedItem as BookmarkInfo;
            if (bi != null)
            {
                if (getBookmarkList(Guid.Parse(bi.Guid)))
                {
                    bi.GeocacheCodes = _gcCodes.ToList();
                    List<string> present = (from a in _gcCodes
                                            join Framework.Data.Geocache g in _core.Geocaches on a equals g.Code
                                            select a).ToList();
                    foreach (string s in present)
                    {
                        _gcCodes.Remove(s);
                    }
                    DialogResult = System.Windows.Forms.DialogResult.OK;
                    Close();
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Guid guid = getGuid();
            if (getBookmarkList(guid))
            {
                BookmarkInfo bi = new BookmarkInfo();
                bi.Name = textBox2.Text;
                bi.Guid = guid.ToString();
                bi.GeocacheCodes = _gcCodes.ToList();
                BookmarkInfoList.Instance(_core).AddBookmark(bi);

                bi.GeocacheCodes = _gcCodes.ToList();
                List<string> present = (from a in _gcCodes
                                        join Framework.Data.Geocache g in _core.Geocaches on a equals g.Code
                                        select a).ToList();
                foreach (string s in present)
                {
                    _gcCodes.Remove(s);
                }

                DialogResult = System.Windows.Forms.DialogResult.OK;
                Close();
            }
        }
    }
}
