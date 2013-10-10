using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace GlobalcachingApplication.Framework.Interfaces
{
    public interface ILanguageSupport
    {
        List<CultureInfo> GetSupportedCultures();
        string GetTranslation(CultureInfo targetCulture, string text);
        string GetTranslation(CultureInfo sourceCulture, CultureInfo targetCulture, string text);
    }
}
