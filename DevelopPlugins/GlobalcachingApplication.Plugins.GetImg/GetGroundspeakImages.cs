using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.GetImg
{
    public class GetGroundspeakImages: Utils.BasePlugin.Plugin
    {
        public const string ACTION_IMPORT = "DBG: Import Images";

        private string _basePath = "";

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_IMPORT);

            _basePath = System.IO.Path.Combine(core.PluginDataPath, "Images");
            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
            }

            return await base.InitializeAsync(core);
        }

        public override Framework.PluginType PluginType
        {
            get
            {
                return Framework.PluginType.Debug;
            }
        }

        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result && action == ACTION_IMPORT)
            {
                try
                {
                    using (var client = new Utils.API.GeocachingLiveV6(Core))
                    using (var wc = new System.Net.WebClient())
                    {
                        var resp = client.Client.GetGeocacheDataTypes(client.Token, true, true, true);
                        if (resp.Status.StatusCode == 0)
                        {
                            string defaultPath = Path.Combine(new string[] { _basePath, "Default", "attributes" });
                            if (!Directory.Exists(defaultPath))
                            {
                                Directory.CreateDirectory(defaultPath);
                            }
                            foreach (var attr in resp.AttributeTypes)
                            {
                                wc.DownloadFile(attr.YesIconName, Path.Combine(defaultPath, string.Format("{0}.gif", attr.ID)));
                                wc.DownloadFile(attr.NoIconName, Path.Combine(defaultPath, string.Format("_{0}.gif", attr.ID)));
                            }

                            defaultPath = Path.Combine(new string[] { _basePath, "Default", "cachetypes" });
                            if (!Directory.Exists(defaultPath))
                            {
                                Directory.CreateDirectory(defaultPath);
                            }

                            string smallPath = Path.Combine(new string[] { _basePath, "Small", "cachetypes" });
                            if (!Directory.Exists(smallPath))
                            {
                                Directory.CreateDirectory(smallPath);
                            }
                            foreach (var gt in resp.GeocacheTypes)
                            {
                                string fn = string.Format("{0}.gif", gt.GeocacheTypeId);
                                wc.DownloadFile(gt.ImageURL, Path.Combine(defaultPath, fn));
                            }

                            defaultPath = Path.Combine(new string[] { _basePath, "Default", "logtypes" });
                            if (!Directory.Exists(defaultPath))
                            {
                                Directory.CreateDirectory(defaultPath);
                            }
                            foreach (var gt in resp.WptLogTypes)
                            {
                                wc.DownloadFile(gt.ImageURL, Path.Combine(defaultPath, string.Format("{0}.gif", gt.WptLogTypeId)));
                            }
                        }
                    }
                }
                catch
                {
                }
            }
            return result;
        }


        private static Image resizeImage(Image imgToResize, Size size)
        {
            int sourceWidth = imgToResize.Width;
            int sourceHeight = imgToResize.Height;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = ((float)size.Width / (float)sourceWidth);
            nPercentH = ((float)size.Height / (float)sourceHeight);

            if (nPercentH < nPercentW)
                nPercent = nPercentH;
            else
                nPercent = nPercentW;

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap b = new Bitmap(destWidth, destHeight);
            Graphics g = Graphics.FromImage((Image)b);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            g.DrawImage(imgToResize, 0, 0, destWidth, destHeight);
            g.Dispose();

            return (Image)b;
        }
        public void ResizeImage(string OriginalFile, string NewFile, int NewWidth, int MaxHeight, bool OnlyResizeIfWider)
        {
            System.Drawing.Image FullsizeImage = System.Drawing.Image.FromFile(OriginalFile);

            // Prevent using images internal thumbnail
            FullsizeImage.RotateFlip(System.Drawing.RotateFlipType.Rotate180FlipNone);
            FullsizeImage.RotateFlip(System.Drawing.RotateFlipType.Rotate180FlipNone);

            if (OnlyResizeIfWider)
            {
                if (FullsizeImage.Width <= NewWidth)
                {
                    NewWidth = FullsizeImage.Width;
                }
            }

            int NewHeight = FullsizeImage.Height * NewWidth / FullsizeImage.Width;
            if (NewHeight > MaxHeight)
            {
                // Resize with height instead
                NewWidth = FullsizeImage.Width * MaxHeight / FullsizeImage.Height;
                NewHeight = MaxHeight;
            }

            System.Drawing.Image NewImage = FullsizeImage.GetThumbnailImage(NewWidth, NewHeight, null, IntPtr.Zero);

            // Clear handle to original file so that we can overwrite it if necessary
            FullsizeImage.Dispose();

            // Save resized picture
            NewImage.Save(NewFile);
        }
    }


}
