using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Framework.Data
{
    public class LanguageItem
    {
        public string Text { get; private set; }

        private LanguageItem()
        {
        }

        public LanguageItem(string text)
        {
            Text = text;
        }
    }
}
