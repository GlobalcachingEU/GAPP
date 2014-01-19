using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace GlobalcachingApplication.Utils
{
    public class ImageSupport
    {
        private const string DEFAULT_IMAGE = "image-missing.png";

        private static ImageSupport _uniqueInstance = null;
        private static object _lockObject = new object();
        private List<Framework.Interfaces.IImageResource> _imageResourcePlugins = new List<Framework.Interfaces.IImageResource>();
        private string _baseImagePath;
        private Framework.Interfaces.ICore _core = null;

        public ImageSupport()
        {
            _baseImagePath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "Images");
        }

        public static ImageSupport Instance
        {
            get
            {
                if (_uniqueInstance == null)
                {
                    lock (_lockObject)
                    {
                        if (_uniqueInstance == null)
                        {
                            _uniqueInstance = new ImageSupport();
                        }
                    }
                }
                return _uniqueInstance;
            }
        }

        public static List<string> GetImageUrlsFromGeocache(string html)
        {
            List<string> result = new List<string>();
            try
            {
                Regex r = new Regex(@"</?\w+\s+[^>]*>", RegexOptions.Multiline);
                MatchCollection mc = r.Matches(html);
                foreach (Match m in mc)
                {
                    string s = m.Value.Substring(1).Replace('\r', ' ').Replace('\n', ' ').Trim();
                    if (s.StartsWith("img ", StringComparison.OrdinalIgnoreCase))
                    {
                        int pos = s.IndexOf(" src", StringComparison.OrdinalIgnoreCase);
                        if (pos >= 0)
                        {
                            pos = s.IndexOfAny(new char[] { '\'', '"' }, pos);
                            if (pos > 0)
                            {
                                int pos2 = s.IndexOfAny(new char[] { '\'', '"' }, pos + 1);
                                if (pos2 > 0)
                                {
                                    string link = s.Substring(pos + 1, pos2 - pos - 1);
                                    result.Add(link);
                                }
                            }
                        }
                    }
                }
            }
            catch
            {

            }
            return result;
        }
        public static List<string> GetImageUrlsFromGeocache(Framework.Data.Geocache gc)
        {
            return GetImageUrlsFromGeocache(string.Format("{0}{1}", gc.ShortDescriptionInHtml ? gc.ShortDescription ?? "" : "", gc.LongDescriptionInHtml ? gc.LongDescription ?? "" : ""));
        }

        public virtual void UpdateImageSupportPlugins(Framework.Interfaces.ICore core)
        {
            _core = core;
            List<Framework.Interfaces.IPlugin> p = core.GetPlugin(Framework.PluginType.ImageResource);
            if (p != null)
            {
                foreach (Framework.Interfaces.IImageResource mwp in p)
                {
                    if (!_imageResourcePlugins.Contains(mwp))
                    {
                        _imageResourcePlugins.Add(mwp);
                    }
                }
                foreach (Framework.Interfaces.IPlugin mwp in _imageResourcePlugins)
                {
                    if (!p.Contains(mwp))
                    {
                        _imageResourcePlugins.Remove(mwp as Framework.Interfaces.IImageResource);
                    }
                }
            }
        }


        public string GetDefaultImagePath(Framework.Data.ImageSize imageSize)
        {
            string result;
            result = Path.Combine(Path.Combine(_baseImagePath, imageSize.ToString()), DEFAULT_IMAGE);
            return result;
        }

        public string GetImagePath(Framework.Interfaces.ICore core, Framework.Data.ImageSize imageSize, Framework.Data.GeocacheType geocacheType)
        {
            string result = null;
            foreach (Framework.Interfaces.IImageResource ir in _imageResourcePlugins)
            {
                result = ir.GetImagePath(imageSize, geocacheType);
                if (!string.IsNullOrEmpty(result))
                {
                    break;
                }
            }
            if (result == null && imageSize != Framework.Data.ImageSize.Default)
            {
                result = GetImagePath(core, Framework.Data.ImageSize.Default, geocacheType);
            }
            else if (string.IsNullOrEmpty(result))
            {
                //select default
                result = GetDefaultImagePath(imageSize);
            }
            return result;
        }
        public string GetImagePath(Framework.Interfaces.ICore core, Framework.Data.ImageSize imageSize, Framework.Data.GeocacheType geocacheType, bool corrected)
        {
            string result = null;
            foreach (Framework.Interfaces.IImageResource ir in _imageResourcePlugins)
            {
                result = ir.GetImagePath(imageSize, geocacheType, corrected);
                if (!string.IsNullOrEmpty(result))
                {
                    break;
                }
            }
            if (result == null && imageSize != Framework.Data.ImageSize.Default)
            {
                result = GetImagePath(core, Framework.Data.ImageSize.Default, geocacheType, corrected);
            }
            else if (string.IsNullOrEmpty(result))
            {
                //select default
                result = GetDefaultImagePath(imageSize);
            }
            return result;
        }
        public string GetImagePath(Framework.Interfaces.ICore core, Framework.Data.ImageSize imageSize, Framework.Data.GeocacheAttribute attr, Framework.Data.GeocacheAttribute.State state)
        {
            string result = null;
            foreach (Framework.Interfaces.IImageResource ir in _imageResourcePlugins)
            {
                result = ir.GetImagePath(imageSize, attr, state);
                if (!string.IsNullOrEmpty(result))
                {
                    break;
                }
            }
            if (result == null && imageSize != Framework.Data.ImageSize.Default)
            {
                result = GetImagePath(core, Framework.Data.ImageSize.Default, attr, state);
            }
            else if (string.IsNullOrEmpty(result))
            {
                //select default
                result = GetDefaultImagePath(imageSize);
            }
            return result;
        }
        public string GetImagePath(Framework.Interfaces.ICore core, Framework.Data.ImageSize imageSize, Framework.Data.LogType logType)
        {
            string result = null;
            foreach (Framework.Interfaces.IImageResource ir in _imageResourcePlugins)
            {
                result = ir.GetImagePath(imageSize, logType);
                if (!string.IsNullOrEmpty(result))
                {
                    break;
                }
            }
            if (result == null && imageSize != Framework.Data.ImageSize.Default)
            {
                result = GetImagePath(core, Framework.Data.ImageSize.Default, logType);
            }
            else if (string.IsNullOrEmpty(result))
            {
                //select default
                result = GetDefaultImagePath(imageSize);
            }
            return result;
        }
        public string GetImagePath(Framework.Interfaces.ICore core, Framework.Data.ImageSize imageSize, Framework.Data.WaypointType waypointType)
        {
            string result = null;
            foreach (Framework.Interfaces.IImageResource ir in _imageResourcePlugins)
            {
                result = ir.GetImagePath(imageSize, waypointType);
                if (!string.IsNullOrEmpty(result))
                {
                    break;
                }
            }
            if (result == null && imageSize != Framework.Data.ImageSize.Default)
            {
                result = GetImagePath(core, Framework.Data.ImageSize.Default, waypointType);
            }
            else if (string.IsNullOrEmpty(result))
            {
                //select default
                result = GetDefaultImagePath(imageSize);
            }
            return result;
        }
        public string GetImagePath(Framework.Interfaces.ICore core, Framework.Data.ImageSize imageSize, Framework.Data.GeocacheContainer container)
        {
            string result = null;
            foreach (Framework.Interfaces.IImageResource ir in _imageResourcePlugins)
            {
                result = ir.GetImagePath(imageSize, container);
                if (!string.IsNullOrEmpty(result))
                {
                    break;
                }
            }
            if (result == null && imageSize != Framework.Data.ImageSize.Default)
            {
                result = GetImagePath(core, Framework.Data.ImageSize.Default, container);
            }
            else if (string.IsNullOrEmpty(result))
            {
                //select default
                result = GetDefaultImagePath(imageSize);
            }
            return result;
        }

        public string GetImagePath(Framework.Interfaces.ICore core, Framework.Data.ImageSize imageSize, Framework.Data.CompassDirection compassDir)
        {
            string result = null;
            foreach (Framework.Interfaces.IImageResource ir in _imageResourcePlugins)
            {
                result = ir.GetImagePath(imageSize, compassDir);
                if (!string.IsNullOrEmpty(result))
                {
                    break;
                }
            }
            if (result == null && imageSize != Framework.Data.ImageSize.Default)
            {
                result = GetImagePath(core, Framework.Data.ImageSize.Default, compassDir);
            }
            else if (string.IsNullOrEmpty(result))
            {
                //select default
                result = GetDefaultImagePath(imageSize);
            }
            return result;
        }

        public bool GrabImages(Framework.Data.Geocache gc)
        {
            bool result = false;
            foreach (Framework.Interfaces.IImageResource ir in _imageResourcePlugins)
            {
                if (ir.GrabImages(gc))
                {
                    result = true;
                    break;
                }
            }
            return result;
        }

        public string GetImagePath(string orgUrl)
        {
            string result = null;
            foreach (Framework.Interfaces.IImageResource ir in _imageResourcePlugins)
            {
                result = ir.GetImagePath(orgUrl);
                if (!string.IsNullOrEmpty(result))
                {
                    break;
                }
            }
            if (string.IsNullOrEmpty(result))
            {
                result = orgUrl;
            }
            return result;
        }

        public string GetImagePath(Framework.Data.Geocache gc, string orgUrl)
        {
            string result = null;
            foreach (Framework.Interfaces.IImageResource ir in _imageResourcePlugins)
            {
                result = ir.GetImagePath(gc, orgUrl);
                if (!string.IsNullOrEmpty(result))
                {
                    break;
                }
            }
            if (string.IsNullOrEmpty(result))
            {
                result = orgUrl;
            }
            return null;
        }

    }
}
