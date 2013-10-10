using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.Gallery
{
    public partial class ImageInfoItem : UserControl
    {
        private ImageInfo _imgInfo = null;

        public ImageInfoItem()
        {
            InitializeComponent();
        }

        public ImageInfoItem(ImageInfo imgInfo)
        {
            InitializeComponent();
            _imgInfo = imgInfo;
        }

        public ImageInfo ImgInfo
        {
            get { return _imgInfo; }
            set
            {
                if (_imgInfo != value)
                {
                    _imgInfo = value;
                    //Image img = pictureBox1.Image;
                    //pictureBox1.Image = null;
                    //if (img != null)
                    //{
                    //    img.Dispose();
                    //    img = null;
                    //}
                    if (_imgInfo == null)
                    {
                        pictureBox1.ImageLocation = null;
                        linkLabelGC.Visible = false;
                        linkLabelLG.Visible = false;
                        labelDate.Visible = false;
                        labelName.Visible = false;
                        labelDescription.Visible = false;
                        //todo
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(_imgInfo.ThumbFile) && System.IO.File.Exists(_imgInfo.ThumbFile))
                        {
                            pictureBox1.ImageLocation = _imgInfo.ThumbFile;
                        }
                        else
                        {
                            pictureBox1.ImageLocation = _imgInfo.ThumbUrl;
                        }
                        linkLabelGC.Links.Clear();
                        linkLabelLG.Links.Clear();

                        linkLabelGC.Text = _imgInfo.LogCacheCode;
                        linkLabelGC.Links.Add(0, _imgInfo.LogCacheCode.Length, string.Format("http://coord.info/{0}",_imgInfo.LogCacheCode));
                        linkLabelLG.Text = _imgInfo.LogCode;
                        linkLabelLG.Links.Add(0, _imgInfo.LogCode.Length, _imgInfo.LogUrl);

                        labelDate.Text = _imgInfo.LogVisitDate.ToString("d");
                        labelName.Text = _imgInfo.Name;
                        labelDescription.Text = _imgInfo.Description;

                        linkLabelGC.Visible = true;
                        linkLabelLG.Visible = true;
                        labelDate.Visible = true;
                        labelName.Visible = true;
                        labelDescription.Visible = true;
                        //todo
                    }
                }
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(_imgInfo.ImgFile) && System.IO.File.Exists(_imgInfo.ImgFile))
                {
                    System.Diagnostics.Process.Start(_imgInfo.ImgFile);
                }
                else
                {
                    System.Diagnostics.Process.Start(_imgInfo.Url);
                }
            }
            catch
            {
            }
        }

        private void linkLabelGC_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Link.LinkData.ToString());
        }

        private void linkLabelLG_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Link.LinkData.ToString());
        }
    }
}
