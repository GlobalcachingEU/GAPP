using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GAPPSF.Excel
{
    public class PropertyItem
    {
        public static List<PropertyItem> PropertyItems = new List<PropertyItem>();

        public string Name { get; private set; }
        public string Text { get { return ToString(); } }

        public PropertyItem(string name)
        {
            Name = name;
            PropertyItems.Add(this);
        }

        public virtual object GetValue(Core.Data.Geocache gc)
        {
            return null;
        }

        public override string ToString()
        {
            return Localization.TranslationManager.Instance.Translate(Name) as string;
        }
    }
}
