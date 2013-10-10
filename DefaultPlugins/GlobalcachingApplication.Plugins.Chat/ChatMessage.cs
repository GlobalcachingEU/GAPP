using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

namespace GlobalcachingApplication.Plugins.Chat
{
    public class ChatMessage
    {
        public Dictionary<string, string> Parameters { get; private set; }
        public string Name { get; set; }
        public string Room { get; set; }
        public string ID { get; set; }

        public ChatMessage()
        {
            Parameters = new Dictionary<string, string>();
            Name = "";
            Room = "";
            ID = "";
        }

        public static ChatMessage Parse(byte[] data)
        {
            ChatMessage result = null;
            if (data != null && data.Length > 0)
            {
                try
                {
                    string xmlFileContents;
                    using (MemoryStream ms = new MemoryStream(data))
                    using (StreamReader textStreamReader = new StreamReader(ms))
                    {
                        xmlFileContents = textStreamReader.ReadToEnd();
                    }

                    result = new ChatMessage();
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(xmlFileContents);
                    XmlElement root = doc.DocumentElement;
                    result.ID = root.Attributes["id"].InnerText;
                    result.Room = root.Attributes["room"].InnerText;
                    result.Name = root.Attributes["name"].InnerText;

                    XmlNodeList strngs = root.SelectNodes("parameter");
                    if (strngs != null)
                    {
                        foreach (XmlNode sn in strngs)
                        {
                            result.Parameters.Add(sn.Attributes["name"].InnerText, sn.Attributes["value"].InnerText);
                        }
                    }
                }
                catch
                {
                    result = null;
                }
            }
            return result;
        }

        public byte[] ChatMessageData
        {
            get
            {
                byte[] result = null;
                try
                {
                    XmlDocument doc = new XmlDocument();
                    XmlElement root = doc.CreateElement("function");
                    root.SetAttribute("name", Name);
                    root.SetAttribute("room", Room);
                    root.SetAttribute("id", ID);
                    foreach (var di in Parameters)
                    {
                        if (di.Value!=null)
                        {
                            XmlElement lngElement = doc.CreateElement("parameter");
                            lngElement.SetAttribute("name", di.Key);
                            lngElement.SetAttribute("value", di.Value);
                            root.AppendChild(lngElement);
                        }
                    }
                    doc.AppendChild(root);
                    using (MemoryStream ms = new MemoryStream())
                    using (TextWriter sw = new StreamWriter(ms, Encoding.UTF8)) //Set encoding
                    {
                        doc.Save(sw);
                        result = ms.ToArray();
                    }

                }
                catch
                {
                }
                return result;
            }
        }
    }
}
