using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Drawing;

namespace GAPPSF.MapProviders.OSMBinMap
{
    public class RenderInfo
    {
        public RenderInfo Parent { get; set; }
        public List<RenderInfo> Children { get; set; }

        //target
        public string entity { get; set; } //way|node
        public string key { get; set; }
        public string value { get; set; }
        public string zoom_min { get; set; }
        public string zoom_max { get; set; }
        public string closed { get; set; }

        public string[] keys { get; set; }
        public string[] values { get; set; }
        public int izoom_min { get; set; }
        public int izoom_max { get; set; }
        public float fstroke_width { get; set; }

        //theme properties
        public string tag { get; set; } //line|area|pathText|lineSymbol|caption|symbol| todo nodes: circle_caption|circle
        //caption
        public string stroke { get; set; }
        public string stroke_width { get; set; }
        public string font_size { get; set; }
        public string font_style { get; set; }
        public string fill { get; set; }
        public string k { get; set; }
        public string stroke_linecap { get; set; }
        public string stroke_dasharray { get; set; }
        public string src { get; set; }
        public string r { get; set; }

        //translated properties
        public Brush Brush { get; set; }
        public Pen Pen { get; set; }
        public Pen FillPen { get; set; }
        public Image Symbol { get; set; }

        public RenderInfo()
        {
            Children = new List<RenderInfo>();
        }
    }
}
