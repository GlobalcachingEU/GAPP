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
using System.Windows.Media.Imaging;
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

        [DebuggerNonUserCode]
        public static void SaveToFile(string pathToResource, string filename, bool overwrite)
        {
            try
            {
                if (overwrite || !File.Exists(filename))
                {
                    Uri u = GetResourceUri(pathToResource);
                    if (u != null)
                    {
                        StreamResourceInfo info = Application.GetResourceStream(u);
                        byte[] buffer = new byte[info.Stream.Length];
                        info.Stream.Read(buffer, 0, buffer.Length);
                        File.WriteAllBytes(filename, buffer);
                    }
                }
            }
            catch
            {
            }
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

        public static bool ScaleImage(string sourcePath, string destPath, int maxWidth, int maxHeight, double maxMB, int quality, int rotationDeg)
        {
            bool result = false;
            try
            {
                //todo: check result if size in MB is not above max.
                //for now, just apply the max size in pixels
                System.Drawing.Image image = System.Drawing.Image.FromFile(sourcePath);
                if (rotationDeg==180) image.RotateFlip(System.Drawing.RotateFlipType.Rotate180FlipNone);
                else if (rotationDeg == 90) image.RotateFlip(System.Drawing.RotateFlipType.Rotate90FlipNone);
                else if (rotationDeg == 270) image.RotateFlip(System.Drawing.RotateFlipType.Rotate270FlipNone);
                //max size
                System.Drawing.Size sz1 = ImageUtilities.GetNewSize(new System.Drawing.Size(image.Size.Width, image.Size.Height), new System.Drawing.Size(maxWidth, maxHeight));
                System.Drawing.Bitmap bmp = ImageUtilities.ResizeImage(image, sz1.Width, sz1.Height);
                ImageUtilities.SaveJpeg(destPath, bmp, quality);

                result = true;
            }
            catch(Exception e)
            {
                Core.ApplicationData.Instance.Logger.AddLog(new ResourceHelper(), e);
            }
            return result;
        }

    }
}
