using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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

        public static string GetEmbeddedHtmlImageData(string pathToResource)
        {
            //data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAACAAAAA...
            string result = "";
            try
            {
                StreamResourceInfo info = Application.GetResourceStream(GetResourceUri(pathToResource));
                byte[] buffer = new byte[info.Stream.Length];
                info.Stream.Read(buffer, 0, buffer.Length);
                result = string.Concat("data:image/", pathToResource.Substring(pathToResource.LastIndexOf('.')+1), ";base64,", Convert.ToBase64String(buffer));
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

    }
}
