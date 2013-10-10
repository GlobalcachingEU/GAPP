using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using onlyconnect;
using System.Xml;

namespace Globalcaching.HtmlEditor
{
    public partial class HtmlEditorControl : UserControl
    {
        private onlyconnect.HtmlEditor _he;
        public HtmlEditorControl()
        {
            InitializeComponent();

            _he = new onlyconnect.HtmlEditor();
            panelEditor.Controls.Add(_he);
            _he.Dock = DockStyle.Fill;

            _he.DefaultComposeSettings.BackColor = System.Drawing.Color.White;
            _he.DefaultComposeSettings.DefaultFont = new System.Drawing.Font("Arial", 15F);
            _he.DefaultComposeSettings.Enabled = false;
            _he.DefaultComposeSettings.ForeColor = System.Drawing.Color.Black;
            _he.DefaultPreamble = onlyconnect.EncodingType.Auto;
            _he.DocumentEncoding = onlyconnect.EncodingType.WindowsCurrent;
            _he.IsActiveContentEnabled = false;
            _he.Location = new System.Drawing.Point(7, 64);
            _he.Name = "htmlEditor1";
            _he.OptionKeyPath = "";
            _he.SelectionAlignment = System.Windows.Forms.HorizontalAlignment.Left;
            _he.SelectionBackColor = System.Drawing.Color.Empty;
            _he.SelectionBullets = false;
            _he.SelectionFont = null;
            _he.SelectionForeColor = System.Drawing.Color.Empty;
            _he.SelectionNumbering = false;
            _he.IsDesignMode = true;
        }

        public new string Text
        {
            get
            {
                string result = _he.GetDocumentSource() ?? "";
                if (result != "")
                {
                    int pos = result.IndexOf("<BODY>", StringComparison.OrdinalIgnoreCase);
                    if (pos > 0)
                    {
                        pos += 6;
                        int pos2 = result.IndexOf("</BODY>", pos, StringComparison.OrdinalIgnoreCase);
                        if (pos2 > 0)
                        {
                            string b = result.Substring(pos, pos2 - pos).Replace("\r\n","");
                            if (string.IsNullOrEmpty(b) || b.ToLower() == "<p>&nbsp;</p>")
                            {
                                result = "";
                            }
                            else
                            {
                                result = b;
                            }
                        }
                    }
                }
                return result;
            }
            set
            {
                _he.LoadDocument(string.Format("<html><head></head><body>{0}</body></html>",value??""));
            }
        }

        /*
        public string BodyHtml
        {
            get
            {
                string result = _he.BodyHtmlAfterClose;
                return result;
            }
            set
            {
                _he.LoadDocument(string.Format("<html><body>{0}</body></html>", value));
            }
        }
         * */

        private void toolStripButtonCut_Click(object sender, EventArgs e)
        {
            _he.Cut();
        }

        private void toolStripButtonCopy_Click(object sender, EventArgs e)
        {
            _he.Copy();
        }

        private void toolStripButtonPaste_Click(object sender, EventArgs e)
        {
            _he.Paste();
        }

        private void toolStripButtonUndo_Click(object sender, EventArgs e)
        {
            _he.Undo();
        }

        private void toolStripButtonBold_Click(object sender, EventArgs e)
        {
            _he.HtmlDocument2.ExecCommand("Bold", false, null);
        }

        private void toolStripButtonItalic_Click(object sender, EventArgs e)
        {
            _he.HtmlDocument2.ExecCommand("Italic", false, null);
        }

        private void toolStripButtonUnderline_Click(object sender, EventArgs e)
        {
            _he.HtmlDocument2.ExecCommand("Underline", false, null);
        }

        private void toolStripButtonAlignLeft_Click(object sender, EventArgs e)
        {
            _he.HtmlDocument2.ExecCommand("JustifyLeft", false, null);
        }

        private void toolStripButtonAlignCenter_Click(object sender, EventArgs e)
        {
            _he.HtmlDocument2.ExecCommand("JustifyCenter", false, null);
        }

        private void toolStripButtonAlignRight_Click(object sender, EventArgs e)
        {
            _he.HtmlDocument2.ExecCommand("JustifyRight", false, null);
        }

        private void toolStripButtonAlignJustify_Click(object sender, EventArgs e)
        {
            _he.HtmlDocument2.ExecCommand("JustifyFull", false, null);
        }

        private void toolStripMenuItemFontSize1_Click(object sender, EventArgs e)
        {
            _he.HtmlDocument2.ExecCommand("FontSize", false, "1");
        }

        private void toolStripMenuItemFontSize2_Click(object sender, EventArgs e)
        {
            _he.HtmlDocument2.ExecCommand("FontSize", false, "2");
        }

        private void toolStripMenuItemFontSize3_Click(object sender, EventArgs e)
        {
            _he.HtmlDocument2.ExecCommand("FontSize", false, "3");

        }

        private void toolStripMenuItemFontSize4_Click(object sender, EventArgs e)
        {
            _he.HtmlDocument2.ExecCommand("FontSize", false, "4");
        }

        private void toolStripMenuItemFontSize5_Click(object sender, EventArgs e)
        {
            _he.HtmlDocument2.ExecCommand("FontSize", false, "5");
        }

        private void toolStripMenuItemFontSize6_Click(object sender, EventArgs e)
        {
            _he.HtmlDocument2.ExecCommand("FontSize", false, "6");
        }

        private void toolStripMenuItemFontSize7_Click(object sender, EventArgs e)
        {
            _he.HtmlDocument2.ExecCommand("FontSize", false, "7");
        }

        private void toolStripButtonFontColor_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            if (colorDialog.ShowDialog() != DialogResult.Cancel)
            {
                _he.HtmlDocument2.ExecCommand("ForeColor", false, ColorTranslator.ToHtml(colorDialog.Color).ToString());
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            if (colorDialog.ShowDialog() != DialogResult.Cancel)
            {
                _he.HtmlDocument2.ExecCommand("BackColor", false, ColorTranslator.ToHtml(colorDialog.Color).ToString());
            }
        }

        private void toolStripButtonStrikeThrough_Click(object sender, EventArgs e)
        {
            _he.HtmlDocument2.ExecCommand("StrikeThrough", false, null);
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            FontDialog fontDialog = new FontDialog();
            fontDialog.ShowColor = true;
            fontDialog.ShowEffects = true;
            if (fontDialog.ShowDialog() != DialogResult.Cancel)
            {
                _he.HtmlDocument2.ExecCommand("ForeColor", false, ColorTranslator.ToHtml(fontDialog.Color).ToString());
                _he.HtmlDocument2.ExecCommand("FontName", false, fontDialog.Font.Name);
                _he.HtmlDocument2.ExecCommand("Bold", false, fontDialog.Font.Bold);
                _he.HtmlDocument2.ExecCommand("Underline", false, fontDialog.Font.Underline);
                _he.HtmlDocument2.ExecCommand("StrikeThrough", false, fontDialog.Font.Strikeout);
                _he.HtmlDocument2.ExecCommand("Italic", false, fontDialog.Font.Italic);
            }
        }

        private void toolStripButtonBulletedList_Click(object sender, EventArgs e)
        {
            _he.HtmlDocument2.ExecCommand("InsertUnorderedList", false, null);
        }

        private void toolStripButtonNumberedList_Click(object sender, EventArgs e)
        {
            _he.HtmlDocument2.ExecCommand("InsertOrderedList", false, null);
        }

        private void toolStripButtonDecreaseIndent_Click(object sender, EventArgs e)
        {
            _he.HtmlDocument2.ExecCommand("outdent", false, null);
        }

        private void toolStripButtonIncreaseIndent_Click(object sender, EventArgs e)
        {
            _he.HtmlDocument2.ExecCommand("Indent", false, null);
        }

        private void toolStripButtonInsertLink_Click(object sender, EventArgs e)
        {
            _he.HtmlDocument2.ExecCommand("createlink", false, null);
        }

        private void toolStripButtonInsertImage_Click(object sender, EventArgs e)
        {
            /*
            FormImageDlg dlg = new FormImageDlg();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    _he.HtmlDocument2.ExecCommand("insertimage", false, dlg.ImgSource);
                }
                catch
                {
                }
            }
            dlg.Dispose();
             * */
        }
    }
}
