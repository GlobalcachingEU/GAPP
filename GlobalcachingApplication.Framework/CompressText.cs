using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;

namespace GlobalcachingApplication.Framework
{
    public class CompressText
    {
        public static byte[] ZipText(string txt)
        {
            if (string.IsNullOrEmpty(txt)) return null;

            byte[] result = null;
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                using (ZipOutputStream s = new ZipOutputStream(ms))
                {
                    s.SetLevel(9); // 0-9, 9 being the highest compression
                    ZipEntry entry = new ZipEntry("t");
                    s.PutNextEntry(entry);
                    byte[] buffer = System.Text.UTF8Encoding.UTF8.GetBytes(txt);
                    s.Write(buffer, 0, buffer.Length);
                    s.Finish();
                }
                result = ms.ToArray();
            }
            return result;
        }

        public static string UnzipText(byte[] zippedText)
        {
            if (zippedText == null || zippedText.Length == 0) return "";

            StringBuilder sb = new StringBuilder();
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream(zippedText))
            using (ZipInputStream s = new ZipInputStream(ms))
            {
                ZipEntry theEntry = s.GetNextEntry();
                byte[] data = new byte[1024];
                if (theEntry != null)
                {
                    while (true)
                    {
                        int size = s.Read(data, 0, data.Length);
                        if (size > 0)
                        {
                            sb.Append(System.Text.UTF8Encoding.UTF8.GetString(data, 0, size));
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            return sb.ToString();
        }
    }
}
