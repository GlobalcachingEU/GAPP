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

        public override string ToString()
        {
            return _name ?? "";
        }

        public CacheListColumnInfo(int colIndex, int visIndex, bool vis, string name)
        {
            _columnIndex = colIndex;
            _visualIndex = visIndex;
            _visible = vis;
            _name = name;
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

        private string _name = "";
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
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
