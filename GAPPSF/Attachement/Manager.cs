using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Attachement
{
    public class Manager
    {
        private static Manager _uniqueInstance = null;
        private static object _lockObject = new object();

        private Manager() { }

        public static Manager Instance
        {
            get
            {
                if (_uniqueInstance==null)
                {
                    lock(_lockObject)
                    {
                        if (_uniqueInstance==null)
                        {
                            _uniqueInstance = new Manager();
                        }
                    }
                }
                return _uniqueInstance;
            }
        }

        public List<Item> GetAttachements(string gcCode)
        {
            return Core.Settings.Default.GetAttachements(gcCode);
        }
        public void AddAttachement(Item item)
        {
            Core.Settings.Default.AddAttachement(item);
        }
        public void DeleteAttachement(Item item)
        {
            Core.Settings.Default.DeleteAttachement(item);
        }

        public List<string> GeocacheCodesWithAttachement()
        {
            List<string> result;
            List<Item> items = GetAttachements(null);
            result = (from a in items select a.GeocacheCode).Distinct().ToList();
            return result;
        }

    }
}
