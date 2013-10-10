using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Framework.Data
{
    public class GeocachingComAccountInfo
    {
        public event EventArguments.GeocacheComAccountEventHandler Changed;

        private string _accountName = "";
        private string _aPIToken = "";
        private string _aPITokenStaging = "";
        private int _memberTypeId = 0;
        private string _memberType = "";

        public GeocachingComAccountInfo()
        {
        }

        public string MemberType
        {
            get { return _memberType; }
            set
            {
                if (_memberType != value)
                {
                    _memberType = value;
                    OnChanged(this);
                }
            }
        }

        public int MemberTypeId
        {
            get { return _memberTypeId; }
            set
            {
                if (_memberTypeId != value)
                {
                    _memberTypeId = value;
                    OnChanged(this);
                }
            }
        }

        public string AccountName 
        {
            get { return _accountName; }
            set
            {
                if (_accountName != value)
                {
                    _accountName = value;
                    OnChanged(this);
                }
            }
        }
        public string APIToken
        {
            get { return _aPIToken; }
            set
            {
                if (_aPIToken != value)
                {
                    _aPIToken = value;
                    OnChanged(this);
                }
            }
        }

        public string APITokenStaging
        {
            get { return _aPITokenStaging; }
            set
            {
                if (_aPITokenStaging != value)
                {
                    _aPITokenStaging = value;
                    OnChanged(this);
                }
            }
        }

        public void OnChanged(object sender)
        {
            if (Changed != null)
            {
                Changed(sender, new EventArguments.GeocacheComAccountEventArgs(this));
            }
        }
    }
}
