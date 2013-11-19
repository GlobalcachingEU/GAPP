using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace GAPPSF.UIControls
{
    /*usage:
          * public class Customer
            {
                public string Name { get; set; }
            }
    *       Customers = new ObservableCollection<CheckedListItem<Customer>>();
    *
            <ListBox ItemsSource="{Binding Customers}" >
                <ListBox.ItemTemplate>
                    <DataTemplate>
                        <CheckBox IsChecked="{Binding IsChecked}" Content="{Binding Path=Item.Name}" /> 
                    </DataTemplate>
                </ListBox.ItemTemplate>
            </ListBox>        
     */
    public class CheckedListItem<T> : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _isChecked;
        private T _item;

        public CheckedListItem()
        { }

        public CheckedListItem(T item, bool isChecked = false)
        {
            this._item = item;
            this._isChecked = isChecked;
        }

        public T Item
        {
            get { return _item; }
            set
            {
                if (!EqualityComparer<T>.Default.Equals(_item, value))
                {
                    _item = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("Item"));
                    }
                }
            }
        }


        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (value != _isChecked)
                {
                    _isChecked = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("IsChecked"));
                    }
                }
            }
        }
    }
}
