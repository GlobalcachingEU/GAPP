using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.UIControls.IgnoreGeocaches
{
    public class IgnoreCategory
    {
        public string Category { get; private set; }
        protected string _rcText { get; private set; }
        public IgnoreCategory(string category, string rcText)
        {
            Category = category;
            _rcText = rcText;
            Text = Localization.TranslationManager.Instance.Translate(_rcText) as string;
        }
        public string Text { get; set; }
        public ObservableCollection<string> Items { get; set; }
        public override string ToString()
        {
            return Text;
        }
        public virtual void UpdateItems() { ;}
        public virtual void UpdateText() 
        {
            Text = Localization.TranslationManager.Instance.Translate(_rcText) as string;
        }
        public virtual void AddItem(string item) { ;}
        public virtual void RemoveItems(List<string> items) { ;}
    }

    public class IgnoredGeocacheCodes : IgnoreCategory
    {
        public IgnoredGeocacheCodes()
            : base("code","Code")
        {
            Items = new ObservableCollection<string>(Core.Settings.Default.IgnoredGeocacheCodes);
        }
        public override void UpdateItems()
        {
            Items.Clear();
            List<string> items = Core.Settings.Default.IgnoredGeocacheCodes;
            foreach(string s in items)
            {
                Items.Add(s);
            }
        }
        public override void AddItem(string item)
        {
            Core.Settings.Default.AddIgnoreGeocacheCodes(new string[] { item }.ToList());
        }
        public override void RemoveItems(List<string> items)
        {
            Core.Settings.Default.DeleteIgnoreGeocacheCodes(items);
        }
    }

    public class IgnoredGeocacheNames : IgnoreCategory
    {
        public IgnoredGeocacheNames()
            : base("name", "Name")
        {
            Items = new ObservableCollection<string>(Core.Settings.Default.IgnoredGeocacheNames);
        }
        public override void UpdateItems()
        {
            Items.Clear();
            List<string> items = Core.Settings.Default.IgnoredGeocacheNames;
            foreach (string s in items)
            {
                Items.Add(s);
            }
        }
        public override void AddItem(string item)
        {
            Core.Settings.Default.AddIgnoreGeocacheNames(new string[] { item }.ToList());
        }
        public override void RemoveItems(List<string> items)
        {
            Core.Settings.Default.DeleteIgnoreGeocacheNames(items);
        }
    }

    public class IgnoredGeocacheOwners : IgnoreCategory
    {
        public IgnoredGeocacheOwners()
            : base("owner", "Owner")
        {
            Items = new ObservableCollection<string>(Core.Settings.Default.IgnoredGeocacheOwners);
        }
        public override void UpdateItems()
        {
            Items.Clear();
            List<string> items = Core.Settings.Default.IgnoredGeocacheOwners;
            foreach (string s in items)
            {
                Items.Add(s);
            }
        }
        public override void AddItem(string item)
        {
            Core.Settings.Default.AddIgnoreGeocacheOwners(new string[] { item }.ToList());
        }
        public override void RemoveItems(List<string> items)
        {
            Core.Settings.Default.DeleteIgnoreGeocacheOwners(items);
        }

    }

}
