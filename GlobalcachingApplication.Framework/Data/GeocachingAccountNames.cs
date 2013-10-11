using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace GlobalcachingApplication.Framework.Data
{
    public class GeocachingAccountNames: DataObject
    {
        public event EventArguments.GeocachingAccountNamesEventHandler Changed;

        private Hashtable _accountNames;

        public GeocachingAccountNames()
        {
            _accountNames = new Hashtable();
        }

        public string[] GeocachePrefixes
        {
            get
            {
                string[] result = new string[_accountNames.Keys.Count];
                if (result.Length > 0)
                {
                    _accountNames.Keys.CopyTo(result, 0);
                }
                return result;
            }
        }

        public void SetAccountName(string GeocachePrefix, string accountName)
        {
            string s = GeocachePrefix.ToUpper();
            if (accountName != _accountNames[s] as string)
            {
                _accountNames[s] = accountName;
                OnChanged(this);
            }
        }

        public string GetAccountName(string GeocacheCode)
        {
            string result = "";
            if (GeocacheCode != null && GeocacheCode.Length >= 2)
            {
                string pf = GeocacheCode.Substring(0, 2).ToUpper();
                result = _accountNames[pf] as string ?? "";

                //OK, to make it backwards compatible, we just map it on the geocaching.com account name
                if (result.Length == 0 && pf!="GC")
                {
                    result = _accountNames["GC"] as string ?? "";
                }
            }
            return result;
        }

        public void OnChanged(object sender)
        {
            if (Changed != null)
            {
                Changed(sender, new EventArguments.GeocachingAccountNamesEventArgs(this));
            }
        }

    }
}
