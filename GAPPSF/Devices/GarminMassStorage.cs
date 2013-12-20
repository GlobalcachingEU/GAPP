using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace GAPPSF.Devices
{
    public class GarminMassStorage : IDisposable
    {
        private ManagementEventWatcher _managementEventWatcher = null;

        public class DeviceInfo
        {
            private string _driveName;
            private string _deviceName;
            private string _deviceXml;

            public DeviceInfo(string driveName)
            {
                _driveName = driveName;
            }

            public override string ToString()
            {
                return string.Format("{0}{1}", _driveName, _deviceName);
            }

            public static DeviceInfo From(DeviceInfo di)
            {
                DeviceInfo result = new DeviceInfo(di.DriveName);
                result.DeviceXml = di.DeviceXml;
                result.DeviceName = di.DeviceName;
                return result;
            }

            public string DriveName
            {
                get { return _driveName; }
            }

            public string DeviceName
            {
                get { return _deviceName; }
                set { _deviceName = value; }
            }

            public string DeviceXml
            {
                get { return _deviceXml; }
                set { _deviceXml = value; }
            }
        }
        private List<DeviceInfo> _connectedDevices = new List<DeviceInfo>();

        public class DeviceInfoEventArgs : EventArgs
        {
            private DeviceInfo _deviceInfo;

            public DeviceInfoEventArgs(DeviceInfo deviceInfo)
            {
                _deviceInfo = deviceInfo;
            }

            public DeviceInfo Device
            {
                get { return _deviceInfo; }
            }
        }

        public GarminMassStorage()
        {
            ScanForDevices();

            WqlEventQuery q;
            //ManagementOperationObserver observer = new ManagementOperationObserver();
            try
            {
                // Bind to local machine
                //ManagementScope scope = new ManagementScope("root\\CIMV2");
                //scope.Options.EnablePrivileges = true; //sets required privilege

                q = new WqlEventQuery("Select * from Win32_DeviceChangeEvent");

                _managementEventWatcher = new ManagementEventWatcher(q);
                _managementEventWatcher.EventArrived += new EventArrivedEventHandler(_managementEventWatcher_EventArrived);
                _managementEventWatcher.Start();
            }
            catch(Exception e)
            {
                Core.ApplicationData.Instance.Logger.AddLog(this, e);
            }
        }

        public event EventHandler<DeviceInfoEventArgs> DeviceAddedEvent;
        public event EventHandler<DeviceInfoEventArgs> DeviceRemovedEvent;

        public void ScanForDevices()
        {
            try
            {
                System.IO.DriveInfo[] dil = System.IO.DriveInfo.GetDrives();
                foreach (System.IO.DriveInfo di in dil)
                {
                    try
                    {
                        if (di.DriveType == System.IO.DriveType.Removable)
                        {
                            DeviceInfo dev = new DeviceInfo(di.Name.Replace("\\", ""));
                            lock (_connectedDevices)
                            {
                                bool insert = true;
                                for (int i = 0; i < _connectedDevices.Count; i++)
                                {
                                    if (_connectedDevices[i].DriveName == dev.DriveName)
                                    {
                                        insert = false;
                                        break;
                                    }
                                }
                                if (insert)
                                {
                                    if (ProcessDrive(dev))
                                    {
                                        _connectedDevices.Add(dev);
                                        if (DeviceAddedEvent != null) DeviceAddedEvent(this, new DeviceInfoEventArgs(dev));
                                    }
                                }
                            }
                        }
                    }
                    catch(Exception e)
                    {
                        Core.ApplicationData.Instance.Logger.AddLog(this, e);
                    }
                }
            }
            catch(Exception e)
            {
                Core.ApplicationData.Instance.Logger.AddLog(this, e);
            }
        }

        public List<DeviceInfo> ConnectedDevices
        {
            get
            {
                List<DeviceInfo> result = new List<DeviceInfo>();
                lock (_connectedDevices)
                {
                    foreach (DeviceInfo di in _connectedDevices)
                    {
                        result.Add(DeviceInfo.From(di));
                    }
                }
                return result;
            }
        }

        void _managementEventWatcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            ManagementBaseObject mbo = null;
            mbo = (ManagementBaseObject)e.NewEvent;
            try
            {
                PropertyData prop = mbo.Properties["DriveName"];
                if (prop != null)
                {
                    string driveName = (string)prop.Value;
                    prop = mbo.Properties["EventType"];
                    if (prop != null)
                    {
                        bool inserted = ((UInt16)prop.Value == 2);

                        lock (_connectedDevices)
                        {
                            if (inserted)
                            {
                                bool insert = true;
                                for (int i = 0; i < _connectedDevices.Count; i++)
                                {
                                    if (_connectedDevices[i].DriveName == driveName)
                                    {
                                        insert = false;
                                        break;
                                    }
                                }
                                if (insert)
                                {
                                    DeviceInfo di = new DeviceInfo(driveName);
                                    if (ProcessDrive(di))
                                    {
                                        _connectedDevices.Add(di);
                                        if (DeviceAddedEvent != null) DeviceAddedEvent(this, new DeviceInfoEventArgs(di));
                                    }
                                }
                            }
                            else
                            {
                                for (int i = 0; i < _connectedDevices.Count; i++)
                                {
                                    if (_connectedDevices[i].DriveName == driveName)
                                    {
                                        DeviceInfo di = _connectedDevices[i];
                                        _connectedDevices.RemoveAt(i);
                                        if (DeviceRemovedEvent != null) DeviceRemovedEvent(this, new DeviceInfoEventArgs(di));
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Core.ApplicationData.Instance.Logger.AddLog(this, ex);
            }
        }

        private bool ProcessDrive(DeviceInfo di)
        {
            bool result = false;
            try
            {
                if (System.IO.Directory.Exists(System.IO.Path.Combine(new string[] { di.DriveName, "Garmin", "GPX" })))
                {
                    di.DeviceName = "";
                    string fn = System.IO.Path.Combine(new string[] { di.DriveName, "Garmin", "GarminDevice.xml" });
                    if (System.IO.File.Exists(fn))
                    {
                        try
                        {
                            string s = System.IO.File.ReadAllText(fn);
                            di.DeviceXml = s;
                            XDocument xdoc = XDocument.Parse(s);
                            int pos = s.IndexOf("http://");
                            int pos2 = s.IndexOf("\"", pos);
                            string lns = s.Substring(pos, pos2 - pos);
                            XNamespace ns = XNamespace.Get(lns);

                            di.DeviceName = xdoc.Element(ns + "Device").Element(ns + "Model").Element(ns + "Description").Value;
                        }
                        catch(Exception e)
                        {
                            Core.ApplicationData.Instance.Logger.AddLog(this, e);
                        }
                    }
                    result = true;
                }
            }
            catch(Exception e)
            {
                Core.ApplicationData.Instance.Logger.AddLog(this, e);
            }
            return result;
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_managementEventWatcher != null)
            {
                _managementEventWatcher.Stop();
                _managementEventWatcher.Dispose();
                _managementEventWatcher = null;
            }
        }

        #endregion
    }

}
