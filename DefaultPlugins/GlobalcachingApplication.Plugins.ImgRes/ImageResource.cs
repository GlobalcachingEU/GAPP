using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GlobalcachingApplication.Plugins.ImgRes
{
    public class ImageResource: Utils.BasePlugin.Plugin, Framework.Interfaces.IImageResource
    {
        private string _baseImagePath;

        public override bool Initialize(Framework.Interfaces.ICore core)
        {
            _baseImagePath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "Images");
            return base.Initialize(core);
        }
        public override Framework.PluginType PluginType
        {
            get
            {
                return Framework.PluginType.ImageResource;
            }
        }

        public override bool Prerequisite
        {
            get
            {
                return true;
            }
        }

        public string GetImagePath(Framework.Data.ImageSize imageSize, Framework.Data.GeocacheType geocacheType)
        {
            string result;
            string fn;
            if (imageSize == Framework.Data.ImageSize.Map)
            {
                fn = string.Format("{0}.png", geocacheType.ID);
            }
            else
            {
                fn = string.Format("{0}.gif", geocacheType.ID);
            }
            result = Path.Combine(new string[] { _baseImagePath, imageSize.ToString(),"cachetypes", fn });
            if (!File.Exists(result))
            {
                if (imageSize != Framework.Data.ImageSize.Default)
                {
                    result = Path.Combine(new string[] { _baseImagePath, imageSize.ToString(), "cachetypes", fn });
                    if (!File.Exists(result))
                    {
                        result = null;
                    }
                }
                else
                {
                    result = null;
                }
            }
            return result;
        }

        public string GetImagePath(Framework.Data.ImageSize imageSize, Framework.Data.GeocacheType geocacheType, bool corrected)
        {
            string result;
            string fn;
            if (imageSize == Framework.Data.ImageSize.Map)
            {
                fn = string.Format("{1}{0}.png", geocacheType.ID, corrected ? "c" : "");
            }
            else
            {
                fn = string.Format("{1}{0}.gif", geocacheType.ID, corrected ? "c" : "");
            }
            result = Path.Combine(new string[] { _baseImagePath, imageSize.ToString(), "cachetypes", fn });
            if (!File.Exists(result))
            {
                result = GetImagePath(imageSize, geocacheType);
            }
            return result;
        }

        public string GetImagePath(Framework.Data.ImageSize imageSize, Framework.Data.GeocacheAttribute attr, Framework.Data.GeocacheAttribute.State state)
        {
            string result;
            string fn;
            string ext;
            if (attr.ID < 100)
            {
                ext = "gif";
            }
            else
            {
                //above 100 = opencaching.de
                ext = "png";
            }
            if (state== Framework.Data.GeocacheAttribute.State.Yes)
            {
                fn = string.Format("{0}.{1}", attr.ID, ext);
            }
            else if (state == Framework.Data.GeocacheAttribute.State.No)
            {
                fn = string.Format("_{0}.{1}", attr.ID, ext);
            }
            else
            {
                return null;
            }
            result = Path.Combine(new string[] { _baseImagePath, imageSize.ToString(), "attributes", fn });
            if (!File.Exists(result))
            {
                if (imageSize != Framework.Data.ImageSize.Default)
                {
                    result = Path.Combine(new string[] { _baseImagePath, imageSize.ToString(), "attributes", fn });
                    if (!File.Exists(result))
                    {
                        result = null;
                    }
                }
                else
                {
                    result = null;
                }
            }
            return result;
        }

        public string GetImagePath(Framework.Data.ImageSize imageSize, Framework.Data.LogType logType)
        {
            string result;
            string fn;
            fn = string.Format("{0}.gif", logType.ID);
            result = Path.Combine(new string[] { _baseImagePath, imageSize.ToString(), "logtypes", fn });
            if (!File.Exists(result))
            {
                if (imageSize != Framework.Data.ImageSize.Default)
                {
                    result = Path.Combine(new string[] { _baseImagePath, imageSize.ToString(), "logtypes", fn });
                    if (!File.Exists(result))
                    {
                        result = null;
                    }
                }
                else
                {
                    result = null;
                }
            }
            return result;
        }

        public string GetImagePath(Framework.Data.ImageSize imageSize, Framework.Data.WaypointType waypointType)
        {
            string result;
            string fn;
            fn = string.Format("{0}.gif", waypointType.ID);
            result = Path.Combine(new string[] { _baseImagePath, imageSize.ToString(), "wptypes", fn });
            if (!File.Exists(result))
            {
                if (imageSize != Framework.Data.ImageSize.Default)
                {
                    result = Path.Combine(new string[] { _baseImagePath, imageSize.ToString(), "wptypes", fn });
                    if (!File.Exists(result))
                    {
                        result = null;
                    }
                }
                else
                {
                    result = null;
                }
            }
            return result;
        }

        public string GetImagePath(Framework.Data.ImageSize imageSize, Framework.Data.GeocacheContainer container)
        {
            string result;
            string fn;
            fn = string.Format("{0}.gif", container.Name.Replace(' ','_'));
            result = Path.Combine(new string[] { _baseImagePath, imageSize.ToString(), "container", fn });
            if (!File.Exists(result))
            {
                if (imageSize != Framework.Data.ImageSize.Default)
                {
                    result = Path.Combine(new string[] { _baseImagePath, imageSize.ToString(), "container", fn });
                    if (!File.Exists(result))
                    {
                        result = null;
                    }
                }
                else
                {
                    result = null;
                }
            }
            return result;
        }

        public string GetImagePath(Framework.Data.ImageSize imageSize, Framework.Data.CompassDirection compassDir)
        {
            string result;
            string fn = string.Format("{0}.gif", compassDir.ToString());
            result = Path.Combine(new string[] { _baseImagePath, imageSize.ToString(), "compass", fn });
            if (!File.Exists(result))
            {
                if (imageSize != Framework.Data.ImageSize.Default)
                {
                    result = Path.Combine(new string[] { _baseImagePath, imageSize.ToString(), "compass", fn });
                    if (!File.Exists(result))
                    {
                        result = null;
                    }
                }
                else
                {
                    result = null;
                }
            }
            return result;
        }

        public bool GrabImages(Framework.Data.Geocache gc)
        {
            return false;
        }

        public string GetImagePath(string orgUrl)
        {
            return null;
        }

        public string GetImagePath(Framework.Data.Geocache gc, string orgUrl)
        {
            return null;
        }

    }
}
