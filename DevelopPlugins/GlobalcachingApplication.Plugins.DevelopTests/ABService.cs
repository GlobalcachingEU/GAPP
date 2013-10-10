using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Xml;
using System.Web;
using System.Collections.Specialized;
using System.IO;

namespace GlobalcachingApplication.Plugins.DevelopTests
{
    class ABService : Utils.BasePlugin.Plugin
    {
        public const string ACTION_SHOW = "Get Action Builder Flows";

        public override bool Initialize(Framework.Interfaces.ICore core)
        {
            //AddAction(ACTION_SHOW);

            return base.Initialize(core);
        }

        public override Framework.PluginType PluginType
        {
            get
            {
                return Framework.PluginType.Action;
            }
        }

        public override string DefaultAction
        {
            get
            {
                return ACTION_SHOW;
            }
        }


        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result && action == ACTION_SHOW)
            {
                try
                {
                    using (WebClient wc = new WebClient())
                    {
                        NameValueCollection formData = new NameValueCollection();
                        formData["token"] = Core.GeocachingComAccount.APIToken;
                        formData["func"] = "BLABLA";
                        byte[] data = wc.UploadValues("http://localhost:50017/ABService.aspx", "POST", formData);

                        using (MemoryStream ms = new MemoryStream(data))
                        {
                            XmlDocument doc = new XmlDocument();
                            doc.Load(ms);
                            XmlElement root = doc.DocumentElement;
                        }
                    }
                }
                catch
                {
                }
            }
            return result;
        }
    }
}
