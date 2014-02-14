using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.LiveAPILogGeocaches
{
    public class LogInfo
    {
        public class ImageInfo
        {
            public string Uri { get; set; }
            public string Name { get; set; }
            public int RotationDeg { get; set; }

            public override string ToString()
            {
                return Name ?? "";
            }
        }

        public string GeocacheCode { get; set; }
        public string LogText { get; set; }
        public Core.Data.LogType LogType { get; set; }
        public DateTime VisitDate { get; set; }
        public ObservableCollection<ImageInfo> Images { get; private set; }

        public LogInfo()
        {
            Images = new ObservableCollection<ImageInfo>();
        }

        public override string ToString()
        {
            return GeocacheCode ?? "";
        }

        public static LogInfo FromDataString(string s)
        {
            LogInfo result = new LogInfo();
            try
            {
                string[] parts = s.Split(new char[] { '|' });
                result.GeocacheCode = parts[0];
                result.LogText = parts[3].Replace("<!br!>", "\r\n").Replace("(!-!)", "|");
                result.VisitDate = DateTime.Parse(parts[2]);
                result.LogType = Utils.DataAccess.GetLogType(int.Parse(parts[1]));
                for (int i=4; i<parts.Length; i++)
                {
                    if (parts[i].Length>0)
                    {
                        //todo
                    }
                }
            }
            catch
            {
                result = null;
            }
            return result;
        }

        private string getImagesDataString()
        {
            //todo
            return "";
        }

        public string ToDataString()
        {
            return string.Format("{0}|{1}|{2}|{3}|{4}", GeocacheCode ?? "", LogType.ID, VisitDate.ToString("s"), LogText == null ? "" : LogText.Replace("|", "(!-!)").Replace("\n", "").Replace("\r", "<!br!>"), getImagesDataString());
        }
    }
}
