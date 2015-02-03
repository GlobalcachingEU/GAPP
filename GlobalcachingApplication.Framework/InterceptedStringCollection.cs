using GlobalcachingApplication.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Framework
{
    public class InterceptedStringCollection : System.Collections.Specialized.StringCollection
    {
        private string _name;
        private ISettings _sp;

        public InterceptedStringCollection(ISettings sp, string name)
        {
            _sp = sp;
            _name = name;
        }

        public void AddWithoutSave(string value)
        {
            base.Add(value);
        }

        public new void Add(string value)
        {
            base.Add(value);
            _sp.SetSettingsValueStringCollection(_name, this);
        }

        public new void AddRange(string[] value)
        {
            base.AddRange(value);
            _sp.SetSettingsValueStringCollection(_name, this);
        }

        public new void Clear()
        {
            base.Clear();
            _sp.SetSettingsValueStringCollection(_name, this);
        }

        public new void Insert(int index, string value)
        {
            base.Insert(index, value);
            _sp.SetSettingsValueStringCollection(_name, this);
        }

        public new void Remove(string value)
        {
            base.Remove(value);
            _sp.SetSettingsValueStringCollection(_name, this);
        }

        public new void RemoveAt(int index)
        {
            base.RemoveAt(index);
            _sp.SetSettingsValueStringCollection(_name, this);
        }
    }

}
