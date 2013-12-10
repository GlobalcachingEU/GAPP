using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Resources;
using System.Reflection;
using System.Globalization;

namespace GAPPSF.Localization
{
    /// <summary>
    /// 
    /// </summary>
    public class ResxTranslationProvider : ITranslationProvider
    {
        #region Private Members

        private readonly ResourceManager _resourceManager;

        #endregion

        #region Construction

        /// <summary>
        /// Initializes a new instance of the <see cref="ResxTranslationProvider"/> class.
        /// </summary>
        /// <param name="baseName">Name of the base.</param>
        /// <param name="assembly">The assembly.</param>
        public ResxTranslationProvider(string baseName, Assembly assembly)
        {
            _resourceManager = new ResourceManager(baseName, assembly);
        }

        #endregion

        #region ITranslationProvider Members

        /// <summary>
        /// See <see cref="ITranslationProvider.Translate" />
        /// </summary>
        public object Translate(string key)
        {
            return _resourceManager.GetString(key);
        }

        #endregion

        #region ITranslationProvider Members

        /// <summary>
        /// See <see cref="ITranslationProvider.AvailableLanguages" />
        /// </summary>
        public IEnumerable<CultureInfo> Languages
        {
            get
            {
                yield return new CultureInfo("");
                yield return new CultureInfo("en");
                yield return new CultureInfo("nl");
                yield return new CultureInfo("de");
                yield return new CultureInfo("fr");
            }
        }

        #endregion
    }
}
