using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;

namespace GlobalcachingApplication.Plugins.APILOGC
{
    public partial class ImageEditorForm : Form
    {
        public const string STR_TILTE = "Add image to log";
        public const string STR_ERROR = "Error";
        public const string STR_NOTSUPPORTEDIMAGETYPE = "Failed to load image.";
        public const string STR_ORIGINALIMAGE = "Original image";
        public const string STR_FILE = "File";
        public const string STR_BYTES = "Bytes";
        public const string STR_SIZEWH = "Size W x H";
        public const string STR_LIMITS = "Limits";
        public const string STR_IMGTOADDTOLOG = "Image to add to log";
        public const string STR_SCALE = "Scale";
        public const string STR_CAPTION = "Caption";
        public const string STR_DESCRIPTION = "Description";
        public const string STR_OK = "OK";
        public const string STR_QUALITY = "Quality";

        private Image _originalImage = null;
        private TemporaryFile tmpFile = null;
        private int _rotation = 0;

        public string Caption { get; private set; }
        public string Description { get; private set; }
        public TemporaryFile ImageFilePath { get; private set; }
        public string Filename { get; private set; }

        public ImageEditorForm()
        {
            InitializeComponent();

            numericUpDown1.Value = (decimal)Properties.Settings.Default.MaxImageSizeMB;
            numericUpDown2.Value = Properties.Settings.Default.MaxImageWidth;
            numericUpDown3.Value = Properties.Settings.Default.MaxImageHeight;
            numericUpDown4.Value = Properties.Settings.Default.ImageQuality;
            numericUpDown5.Value = 100;
            button2.Enabled = false;

            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TILTE);
            this.groupBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ORIGINALIMAGE);
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_FILE);
            this.label4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_BYTES);
            this.label6.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SIZEWH);
            this.groupBox2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_LIMITS);
            this.groupBox3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_IMGTOADDTOLOG);
            this.label17.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SCALE);
            this.label24.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_BYTES);
            this.label21.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SIZEWH);
            this.label26.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CAPTION);
            this.label30.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DESCRIPTION);
            this.button2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_OK);
            this.label15.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_QUALITY);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox1.Text = openFileDialog1.FileName;
                loadImage(0);
            }
        }

        protected void loadImage(int rotation)
        {
            button2.Enabled = false;
            if (_originalImage != null)
            {
                _originalImage.Dispose();
                _originalImage = null;
                textBox1.Text = "";
                label7.Text = "-";
                label8.Text = "-";
                textBox2.Text = "";
                pictureBox1.Image = null;
                button3.Enabled = false;
                button4.Enabled = false;
            }
            try
            {
                _originalImage = Image.FromFile(openFileDialog1.FileName);
                _rotation = rotation;
                switch (rotation)
                {
                    case 0:
                        break;
                    case 1:
                        _originalImage.RotateFlip(RotateFlipType.Rotate90FlipNone);
                        break;
                    case 2:
                        _originalImage.RotateFlip(RotateFlipType.Rotate180FlipNone);
                        break;
                    case 3:
                        _originalImage.RotateFlip(RotateFlipType.Rotate270FlipNone);
                        break;
                }
                label8.Text = string.Format("{0} x {1}", _originalImage.Width, _originalImage.Height);
                FileInfo fi = new FileInfo(openFileDialog1.FileName);
                label7.Text = string.Format("{0:0.000} MB", (double)fi.Length / (1024.0 * 1024.0));
                textBox2.Text = Path.GetFileName(openFileDialog1.FileName);
                textBox3.Text = Path.GetFileName(openFileDialog1.FileName);
                Size sz = Utils.ImageUtilities.GetNewSize(_originalImage.Size, pictureBox1.Size);
                pictureBox1.Image = Utils.ImageUtilities.ResizeImage(_originalImage, sz.Width, sz.Height);
                getImageResult();
                button3.Enabled = true;
                button4.Enabled = true;
            }
            catch
            {
                _originalImage = null;
                button3.Enabled = false;
                button4.Enabled = false;
                MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_NOTSUPPORTEDIMAGETYPE), Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR));
            }
        }

        private bool _busy = false;
        private void getImageResult()
        {
            if (_busy || _originalImage==null) return;
            _busy = true;
            try
            {
                if (tmpFile == null)
                {
                    tmpFile = new TemporaryFile(true);
                }
                //max size
                Size sz1 = Utils.ImageUtilities.GetNewSize(_originalImage.Size, new Size(Properties.Settings.Default.MaxImageWidth, Properties.Settings.Default.MaxImageHeight));
                //scale size
                double f = (double)numericUpDown5.Value / 100.0;
                Size sz2 = Utils.ImageUtilities.GetNewSize(_originalImage.Size, new Size((int)((double)_originalImage.Width * f), (int)((double)_originalImage.Height * f)));
                //take smaller picture as result
                if (sz1.Width < sz2.Width || sz1.Height < sz2.Height)
                {
                    //the limit is smaller than scaled image
                    //adjust scale
                    Utils.ImageUtilities.SaveJpeg(tmpFile.Path, Utils.ImageUtilities.ResizeImage(_originalImage, sz1.Width, sz1.Height), Properties.Settings.Default.ImageQuality);
                    f = ((double)sz1.Width / (double)_originalImage.Size.Width);
                    numericUpDown5.Value = (int)(f * 100.0);
                    label19.Text = string.Format("{0} x {1}", sz1.Width, sz1.Height);
                }
                else
                {
                    Utils.ImageUtilities.SaveJpeg(tmpFile.Path, Utils.ImageUtilities.ResizeImage(_originalImage, sz2.Width, sz2.Height), Properties.Settings.Default.ImageQuality);
                    label19.Text = string.Format("{0} x {1}", sz2.Width, sz2.Height);
                }
                FileInfo fi = new FileInfo(tmpFile.Path);
                double MBcnt = (double)fi.Length / (1024.0 * 1024.0);
                label22.Text = string.Format("{0:0.000} MB", MBcnt);
                if (MBcnt <= Properties.Settings.Default.MaxImageSizeMB)
                {
                    label22.ForeColor = Color.Black;
                }
                else
                {
                    label22.ForeColor = Color.Red;
                }
                button2.Enabled = true;
            }
            catch
            {
            }
            _busy = false;
            return;
        }

        private void numericUpDown5_ValueChanged(object sender, EventArgs e)
        {
            getImageResult();
        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.ImageQuality = (int)numericUpDown4.Value;
            Properties.Settings.Default.Save();
            getImageResult();
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.MaxImageWidth = (int)numericUpDown2.Value;
            Properties.Settings.Default.Save();
            getImageResult();
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.MaxImageHeight = (int)numericUpDown3.Value;
            Properties.Settings.Default.Save();
            getImageResult();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.MaxImageSizeMB = (double)numericUpDown1.Value;
            Properties.Settings.Default.Save();
            getImageResult();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Caption = textBox2.Text;
            this.Description = textBox3.Text;
            this.ImageFilePath = tmpFile;
            this.Filename = Path.GetFileName(textBox1.Text);
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }

        public void Clear()
        {
            if (tmpFile != null)
            {
                tmpFile.Dispose();
                tmpFile = null;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (_busy || _originalImage == null) return;
            _rotation += 1;
            if (_rotation >= 4) _rotation = 0;
            loadImage(_rotation);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (_busy || _originalImage == null) return;
            _rotation -= 1;
            if (_rotation < 0) _rotation = 3;
            loadImage(_rotation);
        }

    }
}
