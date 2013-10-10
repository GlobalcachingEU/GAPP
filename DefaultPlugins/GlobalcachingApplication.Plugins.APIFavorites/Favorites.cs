using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Xml;

namespace GlobalcachingApplication.Plugins.APIFavorites
{
    public class Favorites
    {
        private static Favorites _uniqueInstance = null;
        private static object _lockObject = new object();

        private Hashtable _favorites = null;
        private string _databaseFilename = null;
        private Framework.Interfaces.ICore _core = null;

        private Favorites(Framework.Interfaces.ICore core)
        {
            _favorites = new Hashtable();
            _core = core;
            try
            {
                _databaseFilename = System.IO.Path.Combine(_core.PluginDataPath, "favorites.xml" );
                if (System.IO.File.Exists(_databaseFilename))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(_databaseFilename);
                    XmlElement root = doc.DocumentElement;

                    XmlNodeList bmNodes = root.SelectNodes("code");
                    if (bmNodes != null)
                    {
                        foreach (XmlNode n in bmNodes)
                        {
                            _favorites.Add(n.InnerText, true);
                        }
                    }
                }
            }
            catch
            {
            }
        }

        private void Save()
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                XmlElement root = doc.CreateElement("favorites");
                doc.AppendChild(root);
                foreach (string c in _favorites.Keys)
                {
                    XmlElement el = doc.CreateElement("code");
                    XmlText txt = doc.CreateTextNode(c);
                    el.AppendChild(txt);
                    root.AppendChild(el);
                }
                doc.Save(_databaseFilename);
            }
            catch
            {
            }
        }

        public static Favorites Instance(Framework.Interfaces.ICore core)
        {
            if (_uniqueInstance == null)
            {
                lock (_lockObject)
                {
                    if (_uniqueInstance == null)
                    {
                        _uniqueInstance = new Favorites(core);
                    }
                }
            }
            return _uniqueInstance;
        }

        public void AddGeocacheCode(string gcCode)
        {
            if (!GeocacheFavorited(gcCode))
            {
                _favorites.Add(gcCode, true);
                Save();
            }
        }

        public void RemoveGeocacheCode(string gcCode)
        {
            if (GeocacheFavorited(gcCode))
            {
                _favorites.Remove(gcCode);
                Save();
            }
        }

        public void Reset(string[] gcCodes)
        {
            _favorites.Clear();
            if (gcCodes != null)
            {
                foreach (string gc in gcCodes)
                {
                    _favorites.Add(gc, true);
                }
            }
            Save();
        }

        public bool GeocacheFavorited(string gcCode)
        {
            return (_favorites.ContainsKey(gcCode));
        }
    }
}
