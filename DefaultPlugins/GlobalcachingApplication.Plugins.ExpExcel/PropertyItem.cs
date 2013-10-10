using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Plugins.ExpExcel
{
    public class PropertyItem
    {
        public static List<PropertyItem> PropertyItems = new List<PropertyItem>();

        public Framework.Interfaces.ICore Core { get; private set; }
        public string Name { get; private set; }

        public PropertyItem(Framework.Interfaces.ICore core, string name)
        {
            Core = core;
            Name = name;
            core.LanguageItems.Add(new Framework.Data.LanguageItem(name));
            PropertyItems.Add(this);
        }

        public virtual object GetValue(Framework.Data.Geocache gc)
        {
            return null;
        }

        public override string ToString()
        {
            return Utils.LanguageSupport.Instance.GetTranslation(Name);
        }
    }
}
