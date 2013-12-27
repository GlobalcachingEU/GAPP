using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Drawing;
using System.Reflection;
using System.IO;
using System.Drawing.Drawing2D;

namespace GAPPSF.MapProviders.OSMBinMap
{
    public class ItemRenderer
    {
        private static ItemRenderer _uniqueInstance = null;
        private static object _lockObject = new object();
        private List<RenderInfo> _renderInfoList = new List<RenderInfo>();

        private ItemRenderer()
        {
            try
            {
                processTheme(Utils.ResourceHelper.GetEmbeddedTextFile("/MapProviders/OSMBinMap/DefaultTheme.xml"));
                //test
                //List<RenderInfo> result = new List<RenderInfo>();
                //getMatchedRenderInfo(_renderInfoList, result, "natural", "water", 15);
            }
            catch
            {
            }
        }

        private void processTheme(string s)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(s);
            XmlElement root = doc.DocumentElement;
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("x", root.NamespaceURI);
            XmlNodeList strngs = root.SelectNodes("x:rule", nsmgr);
            if (strngs != null)
            {
                foreach (XmlNode sn in strngs)
                {
                    RenderInfo ri = new RenderInfo();
                    ri.Parent = null;
                    _renderInfoList.Add(ri);
                    processRule(ri, sn, nsmgr);
                }
            }
        }

        private void processRule(RenderInfo ri, XmlNode n, XmlNamespaceManager nsmgr)
        {
            //get target
            ri.entity = n.Attributes["e"].Value;
            ri.key = n.Attributes["k"].Value;
            ri.value = n.Attributes["v"].Value;
            ri.zoom_min = getSafeAttribute(n, "zoom-min");
            ri.closed = getSafeAttribute(n, "closed");

            ri.keys = ri.key.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            ri.values = ri.value.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            if (!string.IsNullOrEmpty(ri.zoom_min))
            {
                ri.izoom_min = int.Parse(ri.zoom_min);
            }
            else
            {
                ri.izoom_min = 0;
            }
            if (!string.IsNullOrEmpty(ri.zoom_max))
            {
                ri.izoom_max = int.Parse(ri.zoom_max);
            }
            else
            {
                ri.izoom_max = 255;
            }

            //get theme properties (if any)
            XmlNode el = n.SelectSingleNode("x:line", nsmgr);
            if (el != null)
            {
                ri.tag = "line";
            }
            else
            {
                el = n.SelectSingleNode("x:area", nsmgr);
                if (el != null)
                {
                    ri.tag = "area";
                }
                else
                {
                    el = n.SelectSingleNode("x:caption", nsmgr);
                    if (el != null)
                    {
                        ri.tag = "caption";
                    }
                    else
                    {
                        el = n.SelectSingleNode("x:symbol", nsmgr);
                        if (el != null)
                        {
                            ri.tag = "symbol";
                        }
                        else
                        {
                            el = n.SelectSingleNode("x:circle", nsmgr);
                            if (el != null)
                            {
                                ri.tag = "circle";
                            }
                        }

                    }
                }
            }
            if (el != null)
            {
                ri.stroke = getSafeAttribute(el, "stroke");
                ri.stroke_width = getSafeAttribute(el, "stroke-width");
                ri.stroke_dasharray = getSafeAttribute(el, "stroke-dasharray");
                ri.stroke_linecap = getSafeAttribute(el, "stroke-linecap");
                ri.fill = getSafeAttribute(el, "fill");
                ri.src = getSafeAttribute(el, "src");
                ri.k = getSafeAttribute(el, "k");
                ri.font_size = getSafeAttribute(el, "font-size");
                ri.font_style = getSafeAttribute(el, "font-style");
                ri.r = getSafeAttribute(el, "r");

                if (!string.IsNullOrEmpty(ri.stroke))
                {
                    ri.Pen = new Pen(System.Drawing.ColorTranslator.FromHtml(ri.stroke));
                }
                if (ri.Pen != null && !string.IsNullOrEmpty(ri.stroke_width))
                {
                    ri.fstroke_width = (float)Utils.Conversion.StringToDouble(ri.stroke_width);
                    ri.Pen.Width = ri.fstroke_width;
                }
                if (ri.Pen != null && !string.IsNullOrEmpty(ri.stroke_dasharray))
                {
                    string[] sp = ri.stroke_dasharray.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    ri.Pen.DashPattern = (from s in sp select (float)int.Parse(s)).ToArray();
                }
                if (ri.Pen != null && !string.IsNullOrEmpty(ri.stroke_linecap))
                {

                }
                if (ri.tag == "line" && !string.IsNullOrEmpty(ri.fill))
                {
                    ri.FillPen = new Pen(System.Drawing.ColorTranslator.FromHtml(ri.fill));
                }

                if (!string.IsNullOrEmpty(ri.fill))
                {
                    ri.Brush = new SolidBrush(System.Drawing.ColorTranslator.FromHtml(ri.fill));
                }

                if (ri.tag == "area" && ri.Brush == null && !string.IsNullOrEmpty(ri.src))
                {
                    try
                    {
                        string fn = ri.src.Replace("jar:", "");
                        if (File.Exists(fn))
                        {
                            using (Bitmap img = (Bitmap)Image.FromFile(fn, true))
                            {
                                ri.Brush = new TextureBrush(img);
                                (ri.Brush as TextureBrush).WrapMode = System.Drawing.Drawing2D.WrapMode.Tile;
                            }
                        }
                        else
                        {
                            fn = Path.GetFileName(ri.src);
                            using (Bitmap img = (Bitmap)Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream(string.Format("GAPPSF.MapProviders.OSMBinMap.patterns.{0}", fn))))
                            {
                                ri.Brush = new TextureBrush(img);
                                (ri.Brush as TextureBrush).WrapMode = System.Drawing.Drawing2D.WrapMode.Tile;
                            }
                        }
                    }
                    catch
                    {
                    }
                }

                if (ri.tag == "symbol" && !string.IsNullOrEmpty(ri.src))
                {
                    try
                    {
                        string fn = ri.src.Replace("jar:", "");
                        if (File.Exists(fn))
                        {
                            ri.Symbol = Image.FromFile(fn, true);
                        }
                        else
                        {
                            fn = Path.GetFileName(ri.src);
                            ri.Symbol = Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream(string.Format("GAPPSF.MapProviders.OSMBinMap.symbols.{0}", fn)));
                        }
                    }
                    catch
                    {
                    }
                }
            }
            //check children
            XmlNodeList strngs = n.SelectNodes("x:rule", nsmgr);
            foreach (XmlNode sn in strngs)
            {
                RenderInfo r = new RenderInfo();
                ri.Children.Add(r);
                r.Parent = ri;
                processRule(r, sn, nsmgr);
            }
        }

        private string getSafeAttribute(XmlNode n, string attr)
        {
            XmlAttribute a = n.Attributes[attr];
            if (a == null)
            {
                return "";
            }
            else
            {
                return a.Value;
            }
        }

        public static ItemRenderer Instance
        {
            get
            {
                if (_uniqueInstance == null)
                {
                    lock (_lockObject)
                    {
                        if (_uniqueInstance == null)
                        {
                            _uniqueInstance = new ItemRenderer();
                        }
                    }
                }
                return _uniqueInstance;
            }
        }

        private void getMatchedRenderInfo(List<RenderInfo> root, List<RenderInfo> result, string entity, string k, string v, int zoom)
        {
            var ril = from r in root where
                           r.entity == entity && r.izoom_min <= zoom && r.izoom_max >= zoom &&
                           //exact match
                           (((r.key == "*" || r.keys.Contains(k)) && 
                           (r.value == "*" || r.values.Contains(v))) ||
                           //not
                           (r.values.Contains("~") &&
                           (!(r.keys.Contains(k)) || (r.key==k && r.values.Contains(v)))))
                       select r;
            foreach (var ri in ril)
            {
                if (ri.Children.Count == 0 && !string.IsNullOrEmpty(ri.tag))
                {
                    result.Add(ri);
                }
                else
                {
                    getMatchedRenderInfo(ri.Children, result, entity, k, v, zoom);
                    //if nothing added (no chidren match, we need to check the tag (properties might have been added
                    //exaxple:
                    /*
			            <rule e="way" k="amenity" v="fountain" closed="yes">
				            <area fill="#b5d6f1" stroke="#000080" stroke-width="0.15" />
				            <rule e="way" k="*" v="*" zoom-min="17">
					            <symbol src="jar:/org/mapsforge/android/maps/rendertheme/osmarender/symbols/fountain.png" />
				            </rule>
			            </rule>
                     * */
                    if (!string.IsNullOrEmpty(ri.tag))
                    {
                        result.Add(ri);
                    }
                }
            }
        }

        public List<RenderInfo> GetMatchedRenderInfo(RequestedTileInformation ti, Way way)
        {
            List<RenderInfo> result = new List<RenderInfo>();
            //now the easy sloppy way
            for (int i = 0; i < way.TagIDs.Count; i++ )
            {
                getMatchedRenderInfo(_renderInfoList, result, "way",  ti.mapFile.Header.WayTags[way.TagIDs[i]].Key, ti.mapFile.Header.WayTags[way.TagIDs[i]].Value, ti.Zoom);
            }
            return result;
        }

        private List<RenderInfo> getMatchedRenderInfo(RequestedTileInformation ti, POI poi)
        {
            List<RenderInfo> result = new List<RenderInfo>();
            //now the easy sloppy way
            for (int i = 0; i < poi.TagIDs.Count; i++)
            {
                getMatchedRenderInfo(_renderInfoList, result, "node", ti.mapFile.Header.POITags[poi.TagIDs[i]].Key, ti.mapFile.Header.POITags[poi.TagIDs[i]].Value, ti.Zoom);
            }
            return result;
        }

        private static double toRelTileX(double longitude, long tileX, int zoom)
        {
            return ((MapFile.GetTileX(longitude / 1000000.0, zoom) - tileX) * 256.0);
        }
        private static double toRelTileY(double latitude, long tileY, int zoom)
        {
            return ((MapFile.GetTileY(latitude / 1000000.0, zoom) - tileY) * 256.0);
        }

        private float getPaintZoomLevel(int zoomLevel)
        {
            float result;
            switch (zoomLevel)
            {
                case 22:
                    result = 54;
                    break;
                case 21:
                    result = 42;
                    break;
                case 20:
                    result = 30;
                    break;
                case 19:
                    result = 20;
                    break;
                case 18:
                    result = 12;
                    break;
                case 17:
                    result = 8;
                    break;
                case 16:
                    result = 6;
                    break;
                case 15:
                    result = 4;
                    break;
                case 14:
                    result = 2;
                    break;
                case 13:
                    result = 1.5f;
                    break;
                default:
                    result = 1;
                    break;
            }
            return result;
        }

        private Brush _fixedTextBrush = new SolidBrush(Color.Black);
        public void Render(RequestedTileInformation ti, Graphics g, Way way, RenderInfo ri)
        {
            lock (this) //gdi objects are not threadsafe
            {
                try
                {
                    if (ri.tag == "area")
                    {
                        RenderAreas(ti, g, way, ri);
                    }
                    else if (ri.tag == "line")
                    {
                        RenderLines(ti, g, way, ri);
                    }
                    else if (ri.tag == "caption")
                    {
                        RenderCaptions(ti, g, way, ri);
                    }
                    else if (ri.tag == "symbol")
                    {
                        RenderSymbols(ti, g, way, ri);
                    }
                }
                catch
                {
                }
            }
        }

        public void RenderSymbols(RequestedTileInformation ti, Graphics g, Way way, RenderInfo ri)
        {
            int lat;
            int lon;
            if (way.LabelLatitude != null && way.LabelLongitude != null)
            {
                lat = (int)way.LabelLatitude;
                lon = (int)way.LabelLongitude;
            }
            else
            {
                lat = (int)way.WayDataBlocks[0].DataBlock[0].CoordBlock.Average(x => x.Latitude);
                lon = (int)way.WayDataBlocks[0].DataBlock[0].CoordBlock.Average(x => x.Longitude);
            }
            g.DrawImageUnscaled(ri.Symbol, (int)toRelTileX(lon, ti.X, ti.Zoom), (int)toRelTileY(lat, ti.Y, ti.Zoom));
        }

        public void RenderCaptions(RequestedTileInformation ti, Graphics g, Way way, RenderInfo ri)
        {
            int lat;
            int lon;
            if (way.LabelLatitude != null && way.LabelLongitude != null)
            {
                lat = (int)way.LabelLatitude;
                lon = (int)way.LabelLongitude;
            }
            else
            {
                lat = (int)way.WayDataBlocks[0].DataBlock[0].CoordBlock.Average(x => x.Latitude);
                lon = (int)way.WayDataBlocks[0].DataBlock[0].CoordBlock.Average(x => x.Longitude);
            }

            using (Font fnt = new Font(FontFamily.GenericSerif, string.IsNullOrEmpty(ri.font_size) ? (float)10.0 : (float)Utils.Conversion.StringToDouble(ri.font_size), ri.font_style == "bold" ? FontStyle.Bold : FontStyle.Regular))
            using (StringFormat sf = new StringFormat())
            {
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;
                float x = (float)toRelTileX(lon, ti.X, ti.Zoom);
                float y = (float)toRelTileY(lat, ti.Y, ti.Zoom);
                if (x > 0 && x < 256 && y > 0 && y < 256)
                {
                    SizeF s = g.MeasureString(way.Name, fnt);
                    float w2 = (s.Width / 2.0f);
                    float h2 = (s.Height / 2.0f);
                    if (x + w2 > 256) x = 256 - w2;
                    if (y + h2 > 256) y = 256 - h2;
                    if (x < w2) x = w2;
                    if (y < h2) y = h2;

                    g.DrawString(way.Name, fnt, ri.Brush ?? new SolidBrush(Color.Black), x, y, sf);
                }
            }
        }

        public void RenderLines(RequestedTileInformation ti, Graphics g, Way way, RenderInfo ri)
        {
            if (way.WayDataBlocks != null && way.WayDataBlocks.Count > 0)
            {
                bool nDone = false;
                float f = getPaintZoomLevel(ti.Zoom);
                foreach (Way.WayData wd in way.WayDataBlocks)
                {
                    System.Drawing.PointF[] pa = (from p in wd.DataBlock[0].CoordBlock select new System.Drawing.PointF((float)toRelTileX(p.Longitude, ti.X, ti.Zoom), (float)toRelTileY(p.Latitude, ti.Y, ti.Zoom))).ToArray();
                    using (GraphicsPath gp = new GraphicsPath())
                    {
                        for (int i = 0; i < pa.Length - 1; i++)
                        {
                            gp.AddLine(pa[i], pa[i + 1]);
                        }
                        Pen p = ri.Pen ?? new Pen(Color.Black);
                        p.Width = ri.fstroke_width * f;
                        g.DrawPath(p, gp);
                        if (ri.FillPen != null)
                        {
                            ri.FillPen.Width = ri.fstroke_width * f * 0.8f;
                            g.DrawPath(ri.FillPen, gp);
                        }
                        if (!string.IsNullOrEmpty(way.Name) && p.Width > 6.0f && !nDone)
                        {
                            nDone = true; //only once per tile is enough I think
                            using (Font fnt = new Font(FontFamily.GenericSerif, p.Width))
                            {
                                //determine angle of polyline (use start and endpoint?)
                                //if wrong (text upside down), then reverse polyline
                                PointF p1 = pa[0];
                                PointF p2 = pa[pa.Length - 1];
                                if (p1.X > p2.X)
                                {
                                    gp.Reverse();
                                }
                                g.DrawString(way.Name, fnt, _fixedTextBrush, TextPathAlign.Center, TextPathPosition.CenterPath, 100, 0, gp);
                            }
                        }
                    }
                }
            }
        }

        public void RenderAreas(RequestedTileInformation ti, Graphics g, Way way, RenderInfo ri)
        {
            bool drawOutline = true;
            if (way.WayDataBlocks != null && way.WayDataBlocks.Count > 0)
            {
                foreach (Way.WayData wd in way.WayDataBlocks)
                {
                    if (wd.DataBlock[0].CoordBlock.Count > 2)
                    {
                        using (GraphicsPath gp = new GraphicsPath())
                        {
                            gp.AddPolygon((from p in wd.DataBlock[0].CoordBlock select new System.Drawing.PointF((float)toRelTileX(p.Longitude, ti.X, ti.Zoom), (float)toRelTileY(p.Latitude, ti.Y, ti.Zoom))).ToArray());
                            if (wd.DataBlock.Count == 1)
                            {
                                g.FillPath(ri.Brush ?? new SolidBrush(Color.Black), gp);
                            }
                            else
                            {
                                GraphicsPath[] gpExclude = new GraphicsPath[wd.DataBlock.Count - 1];
                                for (int i = 0; i < gpExclude.Length; i++)
                                {
                                    gpExclude[i] = new GraphicsPath();
                                    Way.WayCoordinateBlock cb = wd.DataBlock[i + 1];
                                    gpExclude[i].AddPolygon((from p in cb.CoordBlock select new System.Drawing.Point((int)toRelTileX(p.Longitude, ti.X, ti.Zoom), (int)toRelTileY(p.Latitude, ti.Y, ti.Zoom))).ToArray());
                                }
                                Region region = new Region(gp);
                                for (int i = 0; i < gpExclude.Length; i++)
                                {
                                    region.Exclude(gpExclude[i]);
                                }
                                g.FillRegion(ri.Brush ?? new SolidBrush(Color.White), region);
                                for (int i = 0; i < gpExclude.Length; i++)
                                {
                                    if (drawOutline && ri.Pen != null)
                                    {
                                        g.DrawPolygon(ri.Pen, gpExclude[i].PathPoints);
                                    }
                                    gpExclude[i].Dispose();
                                }
                            }
                            if (drawOutline && ri.Pen != null)
                            {
                                g.DrawPolygon(ri.Pen, gp.PathPoints);
                            }
                        }
                    }
                }
            }
        }

        public void Render(RequestedTileInformation ti, Graphics g, POI poi)
        {
            lock (this) //gdi objects are not threadsafe
            {
                try
                {
                    List<RenderInfo> ris = getMatchedRenderInfo(ti, poi);

                    bool cDone = false;
                    bool sDone = false;
                    foreach (RenderInfo ri in ris)
                    {
                        if (ri.tag == "circle")
                        {
                            break;
                        }
                    }
                    foreach (RenderInfo ri in ris)
                    {
                        if (ri.tag == "caption" && !cDone)
                        {
                            using (Font fnt = new Font(FontFamily.GenericSerif, string.IsNullOrEmpty(ri.font_size) ? (float)10.0 : (float)Utils.Conversion.StringToDouble(ri.font_size), ri.font_style == "bold" ? FontStyle.Bold : FontStyle.Regular))
                            using (StringFormat sf = new StringFormat())
                            {
                                sf.Alignment = StringAlignment.Center;
                                sf.LineAlignment = StringAlignment.Center;
                                float x = (float)toRelTileX(poi.Longitude, ti.X, ti.Zoom);
                                float y = (float)toRelTileY(poi.Latitude, ti.Y, ti.Zoom);
                                if (x > 0 && x < 256 && y > 0 && y < 256)
                                {
                                    SizeF s = g.MeasureString(poi.Name, fnt);
                                    float w2 = (s.Width / 2.0f);
                                    float h2 = (s.Height / 2.0f);
                                    if (x + w2 > 256) x = 256 - w2;
                                    if (y + h2 > 256) y = 256 - h2;
                                    if (x < w2) x = w2;
                                    if (y < h2) y = h2;
                                    g.DrawString(poi.Name, fnt, ri.Brush ?? _fixedTextBrush, x, y, sf);
                                    //g.DrawRectangle(new Pen(Color.Red, 1), x-w2, y-h2, s.Width, s.Height);
                                }
                            }
                            cDone = true;
                        }
                        else if (ri.tag == "symbol" && !sDone && ri.Symbol != null)
                        {
                            g.DrawImageUnscaled(ri.Symbol, (int)toRelTileX(poi.Longitude, ti.X, ti.Zoom), (int)toRelTileY(poi.Latitude, ti.Y, ti.Zoom));
                            sDone = true;
                        }
                    }
                }
                catch
                {
                }
            }
        }
    }
}
