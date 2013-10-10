using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Framework.Data
{
    public class LanguageItemCollection: ArrayList
    {
        public override bool Contains(object item)
        {
            bool result;
            if (item is Data.LanguageItem)
            {
                result = base.Contains(item);
                if (!result)
                {
                    foreach (Data.LanguageItem li in this)
                    {
                        if (li.Text.ToLower() == (item as Data.LanguageItem).Text.ToLower())
                        {
                            result = true;
                            break;
                        }
                    }
                }
            }
            else
            {
                result = false;
            }
            return result;
        }
        public override int Add(object value)
        {
            if (!(value is Data.LanguageItem) || Contains(value))
            {
                return -1;
            }
            else
            {
                return base.Add(value);
            }
        }

        public override void AddRange(ICollection c)
        {
            foreach (object li in c)
            {
                Add(li);
            }
        }

        public int AddText(string txt)
        {
            return Add(new Data.LanguageItem(txt));
        }

        public void AddTextRange(List<string> c)
        {
            foreach (string li in c)
            {
                AddText(li);
            }
        }
    }
}
