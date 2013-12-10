using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Resources;

namespace GAPPSF.Utils
{
    public class ResourceHelper
    {
        public static Uri GetResourceUri(string pathToResource)
        {
            //pathToResource e.g: /Resources/Compass/11.gif
            return new Uri(string.Format("pack://application:,,,{0}", pathToResource));
        }

        [DebuggerNonUserCode]
        public static string GetEmbeddedHtmlImageData(string pathToResource)
        {
            //data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAACAAAAA...
            string result = "";
            try
            {
                Uri u = GetResourceUri(pathToResource);
                if (u != null)
                {
                    StreamResourceInfo info = Application.GetResourceStream(u);
                    byte[] buffer = new byte[info.Stream.Length];
                    info.Stream.Read(buffer, 0, buffer.Length);
                    result = string.Concat("data:image/", pathToResource.Substring(pathToResource.LastIndexOf('.') + 1), ";base64,", Convert.ToBase64String(buffer));
                }
            }
            catch
            {
            }
            return result;
        }

        public static string GetEmbeddedTextFile(string pathToResource)
        {
            string result = "";
            try
            {
                StreamResourceInfo info = Application.GetResourceStream(GetResourceUri(pathToResource));
                using (StreamReader sr = new StreamReader(info.Stream))
                {
                    result = sr.ReadToEnd();
                }
            }
            catch
            {
            }
            return result;
        }


        public static void HideScriptErrors(WebBrowser wb, bool hide)
        {
            var fiComWebBrowser = typeof(WebBrowser).GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic);
            if (fiComWebBrowser == null) return;
            var objComWebBrowser = fiComWebBrowser.GetValue(wb);
            if (objComWebBrowser == null)
            {
                wb.Loaded += (o, s) => HideScriptErrors(wb, hide); //In case we are to early
                return;
            }
            objComWebBrowser.GetType().InvokeMember("Silent", BindingFlags.SetProperty, null, objComWebBrowser, new object[] { hide });
        }
    }
}
