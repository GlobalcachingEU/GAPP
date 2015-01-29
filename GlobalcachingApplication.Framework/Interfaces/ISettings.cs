using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Framework.Interfaces
{
    public interface ISettings
    {
        void SetSettingsScope(string name);
        void NewSettingsScope(string name, string copyFrom = null);
        string GetSettingsScope();
        List<string> GetSettingsScopes();

        void SetSettingsValue(string name, string value);
        string GetSettingsValue(string name, string defaultValue);

        void SetSettingsValueInt(string name, int value);
        int GetSettingsValueInt(string name, int defaultValue);
        void SetSettingsValueBool(string name, bool value);
        bool GetSettingsValueBool(string name, bool defaultValue);
        void SetSettingsValueRectangle(string name, System.Drawing.Rectangle value);
        System.Drawing.Rectangle GetSettingsValueRectangle(string name, System.Drawing.Rectangle defaultValue);
        void SetSettingsValueDouble(string name, double value);
        double GetSettingsValueDouble(string name, double defaultValue);
        void SetSettingsValueStringCollection(string name, System.Collections.Specialized.StringCollection value);
        System.Collections.Specialized.StringCollection GetSettingsValueStringCollection(string name, System.Collections.Specialized.StringCollection defaultValue);

        PetaPoco.Database Database { get; }
        bool TableExists(string tableName);
    }
}
