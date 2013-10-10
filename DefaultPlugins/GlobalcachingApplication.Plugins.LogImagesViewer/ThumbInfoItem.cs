using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.LogImagesViewer
{
    public partial class ThumbInfoItem : UserControl
    {
        public const string STR_BY = "By";

        private ThumbInfo _imgInfo = null;
        private Framework.Interfaces.ICore _core = null;

        public ThumbInfoItem()
        {
            InitializeComponent();
        }

        public ThumbInfoItem(ThumbInfo imgInfo, Framework.Interfaces.ICore core)
        {
            InitializeComponent();
            _imgInfo = imgInfo;
            _core = core;
        }

        public void SetImage(string img)
        {
            pictureBox1.ImageLocation = img;
        }

        public ThumbInfo ImgInfo
        {
            get { return _imgInfo; }
            set
            {
                _imgInfo = value;
                if (_imgInfo == null)
                {
                    SetImage(null);
                    linkLabelGC.Visible = false;
                    linkLabelLG.Visible = false;
                    labelDate.Visible = false;
                    labelName.Visible = false;
                    label1.Visible = false;
                }
                else
                {
                    SetImage(_imgInfo.ImageFileLocation);
                    linkLabelGC.Links.Clear();
                    linkLabelLG.Links.Clear();

                    linkLabelGC.Text = _imgInfo.Geocache.Code;
                    if (_imgInfo.Geocache.Url != null)
                    {
                        linkLabelGC.Links.Add(0, _imgInfo.Geocache.Code.Length, _imgInfo.Geocache.Code);
                    }
                    linkLabelLG.Text = _imgInfo.Log.ID;
                    linkLabelLG.Links.Add(0, _imgInfo.Log.ID.Length, _imgInfo.Log.ID);

                    labelDate.Text = _imgInfo.Log.Date.ToString("d");
                    labelName.Text = _imgInfo.LogImage.Name ?? "";
                    label1.Text = string.Format("{0} {1}", Utils.LanguageSupport.Instance.GetTranslation(STR_BY), _imgInfo.Log.Finder ?? "");

                    linkLabelGC.Visible = true;
                    linkLabelLG.Visible = true;
                    labelDate.Visible = true;
                    labelName.Visible = true;
                    label1.Visible = true;
                }
            }
        }

        private void linkLabelGC_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_imgInfo != null)
            {
                _core.ActiveGeocache = _imgInfo.Geocache;
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (_imgInfo != null)
            {
                try
                {
                    System.Diagnostics.Process.Start(_imgInfo.LogImage.Url);
                }
                catch
                {
                }
            }
        }

        private void linkLabelLG_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (_imgInfo != null)
            {
                try
                {
                    if (_imgInfo.Log.ID.StartsWith("G"))
                    {
                        System.Diagnostics.Process.Start(string.Format("http://coord.info/{0}",_imgInfo.Log.ID));
                    }
                    else
                    {
                        System.Diagnostics.Process.Start(string.Format("http://coord.info/{0}", Utils.Conversion.GetCacheCodeFromCacheID(int.Parse(_imgInfo.Log.ID)).Replace("GC","GL")));
                    }
                }
                catch
                {
                }
            }
        }

    }
}
