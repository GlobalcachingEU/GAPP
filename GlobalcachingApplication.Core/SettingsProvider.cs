using GlobalcachingApplication.Framework.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace GlobalcachingApplication.Core
{
    class SettingsProvider : ISettings, IDisposable
    {
        [PetaPoco.TableName("SettingsScope")]
        [PetaPoco.PrimaryKey("ID")]
        private class SettingsScope
        {
            public int ID { get; set; }
            public string Name { get; set; }
        }

        private class Setting
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }

        private SettingsScope _scope = null;
        private Hashtable _currentSettings = null;
        private Hashtable _scopelessSettings = null;
        private PetaPoco.Database _database = null;

        public SettingsProvider(string scope)
        {
            _currentSettings = new Hashtable();
            _scopelessSettings = new Hashtable();

            string p = System.IO.Path.Combine(new string[] { System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "GAPP" });
            if (!Directory.Exists(p))
            {
                Directory.CreateDirectory(p);
            }
            string fn = System.IO.Path.Combine(p, "settings.db3");
            _database = new PetaPoco.Database(string.Format("data source=file:{0}", fn), Community.CsharpSqlite.SQLiteClient.SqliteClientFactory.Instance);

            if (!TableExists("Settings"))
            {
                _database.Execute("create table 'Settings' (Name text, Value text)");
                _database.Execute("insert into Settings (Name, Value) values ('Scope', 'default')");
            }

            if (!TableExists("SettingsScope"))
            {
                _database.Execute("create table 'SettingsScope' (ID integer primary key autoincrement, Name text)");
            }

            if (string.IsNullOrEmpty(scope))
            {
                scope = _database.ExecuteScalar<string>("select Value from Settings where Name=@0", "Scope");
                if (string.IsNullOrEmpty(scope))
                {
                    scope = "default";
                }
            }

            _scope = _database.FirstOrDefault<SettingsScope>("where Name=@0", scope);
            if (_scope == null)
            {
                _scope = new SettingsScope();
                _scope.Name = scope;
                _database.Save(_scope);
            }

            if (!TableExists(string.Format("Settings_{0}",_scope.ID)))
            {
                _database.Execute(string.Format("create table 'Settings_{0}' (Name text, Value text)", _scope.ID));
            }

            var settings = _database.Fetch<Setting>(string.Format("select * from Settings_{0}", _scope.ID));
            foreach (var s in settings)
            {
                _currentSettings.Add(s.Name, s.Value);
            }
            var scopelesssettings = _database.Fetch<Setting>("select * from Settings");
            foreach (var s in scopelesssettings)
            {
                _scopelessSettings.Add(s.Name, s.Value);
            }
        }

        public bool TableExists(string tableName)
        {
            bool result = false;
            try
            {
                string o = null;
                lock (this)
                {
                    o = _database.ExecuteScalar<string>(string.Format("SELECT name FROM sqlite_master WHERE type='table' AND name='{0}'", tableName));
                }
                if (o == null)
                {
                    result = false;
                }
                else
                {
                    result = true;
                }
            }
            catch
            {
            }
            return result;
        }
        public void Dispose()
        {
            if (_database != null)
            {
                _database.Dispose();
                _database = null;
            }
        }

        public PetaPoco.Database Database { get { return _database; } }

        public void SetSettingsScopeForNextStart(string name)
        {
            lock (this)
            {
                if (string.IsNullOrEmpty(name))
                {
                    name = "default";
                }
                _database.Execute("update Settings set Value=@0 where Name='Scope'", name);
            }
        }

        public void SetSettingsScope(string name, bool loadSettings)
        {
            lock (this)
            {
                int curID = _scope.ID;
                if (string.IsNullOrEmpty(name))
                {
                    name = "default";
                }
                _scope = _database.FirstOrDefault<SettingsScope>("where Name=@0", name);
                if (_scope == null)
                {
                    _scope = new SettingsScope();
                    _scope.Name = name;
                    _database.Save(_scope);
                }
                _database.Execute("update Settings set Value=@0 where Name='Scope'", name);
                if (!TableExists(string.Format("Settings_{0}", _scope.ID)))
                {
                    _database.Execute(string.Format("create table 'Settings_{0}' (Name text, Value text)", _scope.ID));
                }
                if (loadSettings && curID != _scope.ID)
                {
                    _currentSettings.Clear();
                    var settings = _database.Fetch<Setting>(string.Format("select * from Settings_{0}", _scope.ID));
                    foreach (var s in settings)
                    {
                        _currentSettings.Add(s.Name, s.Value);
                    }
                }
            }
        }

        public void DeleteSettingsScope(string name)
        {
            lock (this)
            {
                var scope = _database.FirstOrDefault<SettingsScope>("where Name=@0", name);
                if (scope != null && scope.ID != _scope.ID)
                {
                    if (TableExists(string.Format("Settings_{0}", scope.ID)))
                    {
                        _database.Execute(string.Format("drop table 'Settings_{0}'", scope.ID));
                    }
                    _database.Execute("delete from SettingsScope where ID=@0", scope.ID);
                }
            }
        }

        public void NewSettingsScope(string name, string copyFrom = null)
        {
            lock (this)
            {
                var scope = _database.FirstOrDefault<SettingsScope>("where Name=@0", name);
                if (scope == null)
                {
                    scope = new SettingsScope();
                    scope.Name = name;
                    _database.Save(scope);
                }
                if (!TableExists(string.Format("Settings_{0}", scope.ID)))
                {
                    _database.Execute(string.Format("create table 'Settings_{0}' (Name text, Value text)", scope.ID));
                }
                else
                {
                    _database.Execute(string.Format("delete from Settings_{0}", scope.ID));
                }
                if (copyFrom != null)
                {
                    var cscope = _database.FirstOrDefault<SettingsScope>("where Name=@0", copyFrom);
                    if (cscope != null)
                    {
                        var settings = _database.Fetch<Setting>(string.Format("select * from Settings_{0}", cscope.ID));
                        foreach (var set in settings)
                        {
                            _database.Execute(string.Format("insert into Settings_{0} (Name, Value) values (@0, @1)", scope.ID), set.Name, set.Value);
                        }
                    }
                }
            }
        }

        public string GetSettingsScope()
        {
            return _scope.Name;
        }

        public List<string> GetSettingsScopes()
        {
            List<string> result;
            lock (this)
            {
                result = _database.Fetch<string>("select Name from SettingsScope");
            }
            return result;
        }

        public void SetScopelessSettingsValue(string name, string value)
        {
            lock (this)
            {
                if (_scopelessSettings.Contains(name))
                {
                    _database.Execute("update Settings set Value=@0 where Name=@1", value ?? "", name);
                }
                else
                {
                    _database.Execute("insert into Settings (Name, Value) values (@0, @1)", name, value ?? "");
                }
                _scopelessSettings[name] = value;
            }
        }

        public string GetScopelessSettingsValue(string name, string defaultValue)
        {
            string result = null;
            lock (this)
            {
                if (_scopelessSettings.Contains(name))
                {
                    result = _scopelessSettings[name] as string;
                }
                else
                {
                    result = defaultValue;
                }
            }
            return result;
        }

        public void SetSettingsValue(string name, string value)
        {
            lock (this)
            {
                if (_currentSettings.Contains(name))
                {
                    _database.Execute(string.Format("update Settings_{0} set Value=@0 where Name=@1", _scope.ID), value ?? "", name);
                }
                else
                {
                    _database.Execute(string.Format("insert into Settings_{0} (Name, Value) values (@0, @1)", _scope.ID), name, value ?? "");
                }
                _currentSettings[name] = value;
            }
        }

        public string GetSettingsValue(string name, string defaultValue)
        {
            string result = null;
            lock (this)
            {
                if (_currentSettings.Contains(name))
                {
                    result = _currentSettings[name] as string;
                }
                else
                {
                    result = defaultValue;
                }
            }
            return result;
        }

        public void SetSettingsValueInt(string name, int value)
        {
            SetSettingsValue(name, value.ToString());
        }

        public int GetSettingsValueInt(string name, int defaultValue)
        {
            return int.Parse(GetSettingsValue(name, defaultValue.ToString()));
        }

        public void SetSettingsValueBool(string name, bool value)
        {
            SetSettingsValue(name, value.ToString());
        }

        public bool GetSettingsValueBool(string name, bool defaultValue)
        {
            return bool.Parse(GetSettingsValue(name, defaultValue.ToString()));
        }

        public void SetSettingsValueRectangle(string name, System.Drawing.Rectangle value)
        {
            SetSettingsValue(name, string.Format("{0},{1},{2},{3}",value.X, value.Y, value.Width, value.Height));
        }

        public void SetSettingsValueDouble(string name, double value)
        {
            SetSettingsValue(name, value.ToString(CultureInfo.InstalledUICulture));
        }

        public double GetSettingsValueDouble(string name, double defaultValue)
        {
            return double.Parse(GetSettingsValue(name, defaultValue.ToString()), CultureInfo.InstalledUICulture);
        }

        public System.Drawing.Rectangle GetSettingsValueRectangle(string name, System.Drawing.Rectangle defaultValue)
        {
            System.Drawing.Rectangle result = new System.Drawing.Rectangle();
            string r = GetSettingsValue(name, null);
            if (string.IsNullOrEmpty(r))
            {
                result.X = defaultValue.X;
                result.Y = defaultValue.Y;
                result.Width = defaultValue.Width;
                result.Height = defaultValue.Height;
            }
            else
            {
                string[] parts = r.Split(',');
                result.X = int.Parse(parts[0]);
                result.Y = int.Parse(parts[1]);
                result.Width = int.Parse(parts[2]);
                result.Height = int.Parse(parts[3]);
            }
            return result;
        }

        public void SetSettingsValueStringCollection(string name, System.Collections.Specialized.StringCollection value)
        {
            if (value == null)
            {
                SetSettingsValue(name, "");
            }
            else
            {
                XmlDocument doc = new XmlDocument();
                XmlElement root = doc.CreateElement("root");
                doc.AppendChild(root);
                foreach (var l in value)
                {
                    XmlElement el = doc.CreateElement("entry");
                    XmlText txt = doc.CreateTextNode(l);
                    el.AppendChild(txt);
                    root.AppendChild(el);
                }
                /*
                using (var stringWriter = new StringWriter())
                using (var xmlTextWriter = XmlWriter.Create(stringWriter))
                {
                    doc.WriteTo(xmlTextWriter);
                    xmlTextWriter.Flush();
                    SetSettingsValue(name, stringWriter.GetStringBuilder().ToString());
                } 
                 * */
                SetSettingsValue(name, doc.OuterXml);
            }
            
        }

        public System.Collections.Specialized.StringCollection GetSettingsValueStringCollection(string name, System.Collections.Specialized.StringCollection defaultValue)
        {
            System.Collections.Specialized.StringCollection result = null;
            string xmlDoc = GetSettingsValue(name, null);
            if (!string.IsNullOrEmpty(xmlDoc))
            {
                result = new System.Collections.Specialized.StringCollection();

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xmlDoc);
                var root = doc.DocumentElement;
                var lines = doc.SelectNodes("entry");
                foreach (XmlNode n in lines)
                {
                    result.Add(n.InnerText);
                }
            }
            return result;
        }

    }
}
