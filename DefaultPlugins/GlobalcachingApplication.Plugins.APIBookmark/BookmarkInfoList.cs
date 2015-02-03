using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace GlobalcachingApplication.Plugins.APIBookmark
{
    public class BookmarkInfoList
    {
        private static BookmarkInfoList _uniqueInstance = null;
        private static object _lockObject = new object();

        public SelectGeocaches SelectGeocachesPlugin { get; set; }
        private List<BookmarkInfo> _bookmarkInfos = null;
        private Framework.Interfaces.ICore _core = null;

        private BookmarkInfoList(Framework.Interfaces.ICore core)
        {
            _bookmarkInfos = new List<BookmarkInfo>();
            _core = core;
            try
            {
                string xml = PluginSettings.Instance.Bookmarks;
                if (!string.IsNullOrEmpty(xml))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(xml);
                    XmlElement root = doc.DocumentElement;

                    XmlNodeList bmNodes = root.SelectNodes("bookmark");
                    if (bmNodes != null)
                    {
                        foreach (XmlNode n in bmNodes)
                        {
                            BookmarkInfo bm = new BookmarkInfo();
                            bm.Guid = n.SelectSingleNode("Guid").InnerText;
                            bm.Name = n.SelectSingleNode("Name").InnerText;
                            bm.GeocacheCodes = new List<string>();
                            XmlNodeList cNodes = n.SelectSingleNode("Codes").SelectNodes("Code");
                            if (cNodes != null)
                            {
                                foreach (XmlNode c in cNodes)
                                {
                                    bm.GeocacheCodes.Add(c.InnerText);
                                }
                            }
                            _bookmarkInfos.Add(bm);
                        }
                    }
                }

            }
            catch
            {
            }
        }

        private void saveBookmarks()
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                XmlElement root = doc.CreateElement("bookmarks");
                doc.AppendChild(root);
                foreach (BookmarkInfo bmi in _bookmarkInfos)
                {
                    XmlElement bm = doc.CreateElement("bookmark");
                    root.AppendChild(bm);

                    XmlElement el = doc.CreateElement("Name");
                    XmlText txt = doc.CreateTextNode(bmi.Name);
                    el.AppendChild(txt);
                    bm.AppendChild(el);

                    el = doc.CreateElement("Guid");
                    txt = doc.CreateTextNode(bmi.Guid.ToString());
                    el.AppendChild(txt);
                    bm.AppendChild(el);

                    el = doc.CreateElement("Codes");
                    bm.AppendChild(el);

                    if (bmi.GeocacheCodes != null)
                    {
                        foreach (string c in bmi.GeocacheCodes)
                        {
                            XmlElement cel = doc.CreateElement("Code");
                            txt = doc.CreateTextNode(c);
                            cel.AppendChild(txt);
                            el.AppendChild(cel);
                        }
                    }
                }
                PluginSettings.Instance.Bookmarks = doc.OuterXml;
            }
            catch
            {
            }
        }

        public static BookmarkInfoList Instance(Framework.Interfaces.ICore core)
        {
            if (_uniqueInstance == null)
            {
                lock (_lockObject)
                {
                    if (_uniqueInstance == null)
                    {
                        _uniqueInstance = new BookmarkInfoList(core);
                    }
                }
            }
            return _uniqueInstance;
        }

        public BookmarkInfo[] Bookmarks
        {
            get { return _bookmarkInfos.ToArray(); }
        }

        public void AddBookmark(BookmarkInfo bm)
        {
            _bookmarkInfos.Add(bm);
            saveBookmarks();
            if (SelectGeocachesPlugin != null)
            {
                SelectGeocachesPlugin.BookmarkAdded(bm);
            }
        }

        public void RemoveBookmark(BookmarkInfo bm)
        {
            _bookmarkInfos.Remove(bm);
            saveBookmarks();
            if (SelectGeocachesPlugin != null)
            {
                SelectGeocachesPlugin.BookmarkRemoved(bm);
            }
        }
    }
}
