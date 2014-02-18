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
            public string Caption { get; set; }
            public string Description { get; set; }
            public int RotationDeg { get; set; }

            public override string ToString()
            {
                return Caption ?? "";
            }

            public static ImageInfo FromDataString(string s)
            {
                ImageInfo result = new ImageInfo();
                try
                {
                    string[] parts = s.Split(new char[] { '|' });
                    result.Uri = parts[0];
                    result.RotationDeg = int.Parse(parts[1]);
                    result.Caption = parts[2].Replace("<!br!>", "\r\n").Replace("[!-!]", "|");
                    result.Description = parts[3].Replace("<!br!>", "\r\n").Replace("[!-!]", "|");
                }
                catch
                {
                    result = null;
                }
                return result;
            }

            public string ToDataString()
            {
                return string.Format("{0}|{1}|{2}|{3}", Uri, RotationDeg, Caption == null ? "" : Caption.Replace("|", "[!-!]").Replace("\n", "").Replace("\r", "<!br!>"), Description == null ? "" : Description.Replace("|", "[!-!]").Replace("\n", "").Replace("\r", "<!br!>"));
            }
        }

        public string GeocacheCode { get; set; }
        public string LogText { get; set; }
        public bool TrackableDrop { get; set; } //before log, show trackable dialog
        public string TrackableRetrieve { get; set; } //list of tracking numbers
        public bool AddToFavorites { get; set; }
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
                result.TrackableDrop = bool.Parse(parts[4]);
                result.TrackableRetrieve = parts[5];
                result.AddToFavorites = bool.Parse(parts[6]);
                for (int i = 7; i < parts.Length; i++)
                {
                    if (parts[i].Length>0)
                    {
                        ImageInfo ii = ImageInfo.FromDataString(parts[i].Replace("(!-!)", "|"));
                        if (ii != null)
                        {
                            result.Images.Add(ii);
                        }
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
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Images.Count; i++ )
            {
                if (i>0)
                {
                    sb.Append("|");
                }
                sb.Append(Images[i].ToDataString());
            }
            return sb.ToString();
        }

        public string ToDataString()
        {
            return string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}", GeocacheCode ?? "", LogType.ID, VisitDate.ToString("s"), LogText == null ? "" : LogText.Replace("|", "(!-!)").Replace("\n", "").Replace("\r", "<!br!>"), TrackableDrop.ToString(), TrackableRetrieve.Replace("|", "(!-!)"), AddToFavorites.ToString(), getImagesDataString().Replace("|", "(!-!)"));
        }
    }
}
