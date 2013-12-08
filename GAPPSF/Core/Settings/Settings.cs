using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core
{
    public partial class Settings : INotifyPropertyChanged
    {
        private static Settings _uniqueInstance = null;
        private static object _lockObject = new object();
        public static Settings Default
        {
            get
            {
                if (_uniqueInstance == null)
                {
                    lock (_lockObject)
                    {
                        if (_uniqueInstance == null)
                        {
                            _uniqueInstance = new Settings();
                        }
                    }
                }
                return _uniqueInstance;
            }
        }

        public Settings()
        {
#if DEBUG
            if (_uniqueInstance != null)
            {
                //you used the wrong binding
                //use: 
                //<properties:Settings x:Key="Settings" />
                //{Binding ArchivedRowColor, Source={x:Static p:Settings.Default}}
                System.Diagnostics.Debugger.Break();
            }
#endif
            _settingsStorage = new SqliteSettingsStorage();
            _settings = _settingsStorage.LoadSettings();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private ISettingsStorage _settingsStorage = null;
        private Hashtable _settings;

        protected void SetProperty(string value, [CallerMemberName] string name = "")
        {
            string field = getPropertyValue(name);
            if (!EqualityComparer<string>.Default.Equals(field, value))
            {
                lock (_lockObject)
                {
                    _settings[name] = value;
                    _settingsStorage.StoreSetting(name, value);
                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs(name));
                    }
                }
            }
        }

        private string getPropertyValue(string name)
        {
            string result;
            lock (_lockObject)
            {
                result = _settings[name] as string;
            }
            return result;
        }

        protected string GetProperty(string defaultValue, [CallerMemberName] string name = "")
        {
            string result;
            lock (_lockObject)
            {
                if (_settings.ContainsKey(name))
                {
                    result = _settings[name] as string;
                }
                else
                {
                    result = defaultValue;
                }
            }
            return result;
        }

    }
}
