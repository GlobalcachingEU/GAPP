using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace GlobalcachingApplication.Utils.Controls
{
    public class ComboBoxCacheType : ComboBox
    {
        private List<Image> _images = null;
        private StringFormat _fs = null;

        public ComboBoxCacheType()
        {
            _fs = new StringFormat();
            _fs.Alignment = StringAlignment.Near;
            _fs.LineAlignment = StringAlignment.Center;

            this.DrawMode = DrawMode.OwnerDrawVariable;
            this.DropDownStyle = ComboBoxStyle.DropDownList;
            this.DrawItem += new DrawItemEventHandler(ComboBoxCacheType_DrawItem);
            ItemHeight = 25;
        }

        private void ClearImages()
        {
            if (_images != null)
            {
                foreach (Image img in _images)
                {
                    img.Dispose();
                }
                _images.Clear();
                _images = null;
            }
        }

        protected override void Dispose(bool disposing)
        {
            ClearImages();
            base.Dispose(disposing);
        }

        public void SetCacheTypes(Framework.Interfaces.ICore core, List<Framework.Data.GeocacheType> ct)
        {
            BeginUpdate();
            ClearImages();
            _images = new List<Image>();
            Items.Clear();
            foreach (var l in ct)
            {
                _images.Add(Image.FromFile(ImageSupport.Instance.GetImagePath(core, Framework.Data.ImageSize.Small, l)));
                Items.Add(l);
            }
            EndUpdate();
        }

        void ComboBoxCacheType_DrawItem(object sender, DrawItemEventArgs e)
        {
            Rectangle rText = e.Bounds;
            rText.X += 25;
            if (((e.State & DrawItemState.Selected) == DrawItemState.Selected) && DroppedDown)
            {
                using (Brush b = new SolidBrush(Color.Aqua))
                {
                    e.Graphics.FillRectangle(b, e.Bounds);
                }
            }
            else
            {
                using (Brush b = new SolidBrush(this.BackColor))
                {
                    e.Graphics.FillRectangle(b, e.Bounds);
                }
            }
            using (Brush b = new SolidBrush(this.ForeColor))
            {
                if (_images != null)
                {
                    if (e.Index >= 0)
                    {
                        e.Graphics.DrawImage(_images[e.Index], new Rectangle(5, e.Bounds.Top + 2, 20, ItemHeight-4));
                        e.Graphics.DrawString(Utils.LanguageSupport.Instance.GetTranslation(Items[e.Index].ToString()), this.Font, b, rText, _fs);
                    }
                }
            }
        }

    }
}
