using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Drawing.Drawing2D;
using System.Drawing;

namespace GAPPSF.MapProviders.OSMBinMap
{
    public class TileRenderer
    {
        private TileRenderer()
        {
        }

        public static System.Windows.Media.Imaging.BitmapImage GetTileBitmap(List<RequestedTileInformation> tis)
        {
            return GetBitmap(tis);
        }

        private static System.Windows.Media.Imaging.BitmapImage GetBitmap(List<RequestedTileInformation> tis)
        {
            System.Windows.Media.Imaging.BitmapImage result = null;
            bool valid = true;

            if (tis[0].Zoom == tis[0].mapFile.MapFileHandler.MapControlFactory.LastRequestedZoomLevel)
            {
                using (System.Drawing.Bitmap img = new System.Drawing.Bitmap(256, 256))
                {
                    img.SetResolution(96, 96);
                    using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(img))
                    {
                        g.SmoothingMode = SmoothingMode.AntiAlias;
                        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                        g.Clear(System.Drawing.ColorTranslator.FromHtml("#f1eee8"));
                        valid = valid && drawWays(tis, g);
                        valid = valid && drawPOIs(tis, g);
                    }
                    if (valid)
                    {
                        using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                        {
                            img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                            ms.Position = 0;

                            result = new System.Windows.Media.Imaging.BitmapImage();
                            result.BeginInit();
                            result.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                            result.StreamSource = ms;
                            result.EndInit();
                            result.Freeze();
                        }
                    }
                }
            }
            return result;
        }

        private static bool drawWays(List<RequestedTileInformation> tis, System.Drawing.Graphics g)
        {
            List<int> layers = new List<int>();
            foreach (var ti in tis)
            {
                if (ti.Ways != null)
                {
                    layers.AddRange((from w in ti.Ways select w.Layer).Distinct().ToArray());

                    foreach (Way w in ti.Ways)
                    {
                        if (w != null)
                        {
                            w.RenderInfos = ItemRenderer.Instance.GetMatchedRenderInfo(ti, w);
                        }
                    }
                }
            }
            if (layers.Count > 0)
            {
                int minLayer = layers.Min();
                int maxLayer = layers.Max();
                for (int i = minLayer; i <= maxLayer; i++)
                {
                    foreach (var ti in tis)
                    {
                        if (ti.Ways != null)
                        {
                            var ways = from w in ti.Ways where w.Layer==i select w;
                            string[] renderTypes = new string[] { "area", "line", "caption", "symbol" };
                            foreach (string s in renderTypes)
                            {
                                foreach (Way w in ways)
                                {
                                    if (ti.Zoom != ti.mapFile.MapFileHandler.MapControlFactory.LastRequestedZoomLevel)
                                    {
                                        return false;
                                    }
                                    RenderInfo ri = (from r in w.RenderInfos where r.tag == s select r).FirstOrDefault();
                                    if (ri != null)
                                    {
                                        ItemRenderer.Instance.Render(ti, g, w, ri);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }

        private static bool drawPOIs(List<RequestedTileInformation> tis, System.Drawing.Graphics g)
        {
            foreach (var ti in tis)
            {
                if (ti.POIs != null)
                {
                    foreach (POI p in ti.POIs)
                    {
                        if (ti.Zoom != ti.mapFile.MapFileHandler.MapControlFactory.LastRequestedZoomLevel)
                        {
                            return false;
                        }
                        ItemRenderer.Instance.Render(ti, g, p);
                    }
                }
            }
            return true;
        }

    }
}
