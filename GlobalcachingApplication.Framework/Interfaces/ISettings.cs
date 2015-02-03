using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Framework.Interfaces
{
    public interface ISettings
    {
        void SetSettingsScope(string name, bool loadSettings);
        void SetSettingsScopeForNextStart(string name);
        void NewSettingsScope(string name, string copyFrom = null);
        void DeleteSettingsScope(string name);
        string GetSettingsScope();
        List<string> GetSettingsScopes();

        string GetFullTableName(string tableName);

        void SetScopelessSettingsValue(string name, string value);
        string GetScopelessSettingsValue(string name, string defaultValue);

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
        void SetSettingsValueStringCollection(string name, InterceptedStringCollection value);
        InterceptedStringCollection GetSettingsValueStringCollection(string name, InterceptedStringCollection defaultValue);
        void SetSettingsValueColor(string name, System.Drawing.Color value);
        System.Drawing.Color GetSettingsValueColor(string name, System.Drawing.Color defaultValue);
        void SetSettingsValuePoint(string name, System.Drawing.Point value);
        System.Drawing.Point GetSettingsValuePoint(string name, System.Drawing.Point defaultValue);

        PetaPoco.Database Database { get; }
        bool TableExists(string tableName);
    }
}
