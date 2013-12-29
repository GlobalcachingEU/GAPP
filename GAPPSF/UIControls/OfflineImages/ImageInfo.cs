using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.UIControls.OfflineImages
{
    public class ImageInfo: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _url;
        public string Url 
        {
            get { return _url; }
            set { SetProperty(ref _url, value); } 
        }

        private string _filename;
        public string FileName
        {
            get { return _filename; }
            set { SetProperty(ref _filename, value); } 
        }

        protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string name = "")
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                var handler = PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs(name));
                }
            }
        }

    }
}
