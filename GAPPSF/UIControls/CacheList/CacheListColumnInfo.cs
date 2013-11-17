using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.UIControls
{
    public class CacheListColumnInfo: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public CacheListColumnInfo()
        {
        }

        public CacheListColumnInfo(int colIndex, int visIndex, bool vis)
        {
            _columnIndex = colIndex;
            _visualIndex = visIndex;
            _visible = vis;
        }

        private int _columnIndex = 0;
        public int ColumnIndex
        {
            get { return _columnIndex; }
            set { SetProperty(ref _columnIndex, value); }
        }

        private int _visualIndex = 0;
        public int VisualIndex
        {
            get { return _visualIndex; }
            set { SetProperty(ref _visualIndex, value); }
        }

        private bool _visible = true;
        public bool Visible
        {
            get { return _visible; }
            set { SetProperty(ref _visible, value); }
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
