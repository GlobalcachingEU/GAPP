using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.UIControls.ActionBuilder
{
    public class ActionFlow: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _name = "";
        public string Name 
        {
            get { return _name; }
            set { SetProperty(ref _name, value); } 
        }
        public string ID { get; set; }
        public List<ActionImplementation> Actions;

        public override string ToString()
        {
            return Name ?? "";
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
