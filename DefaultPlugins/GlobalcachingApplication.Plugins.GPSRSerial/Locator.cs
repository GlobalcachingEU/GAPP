using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace GlobalcachingApplication.Plugins.GPSRSerial
{
    public class Locator : Utils.BasePlugin.Plugin
    {
        public const string ACTION_START = "GPS Serial|Start";
        public const string ACTION_STOP = "GPS Serial|Stop";

        public const string STR_GPSSERIAL = "GPS Serial";

        private System.IO.Ports.SerialPort _serialPort = null;
        private volatile bool _reading = false;
        private volatile bool _hasGPRMC = false;
        private ActiveNotification _activeNotification = null;
        private SynchronizationContext _context = null;

        public override bool Initialize(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_START);
            AddAction(ACTION_STOP);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(ActiveNotification.STR_CENTER));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ActiveNotification.STR_POSITION));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ActiveNotification.STR_SERVICEACTIVE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(ActiveNotification.STR_STOP));

            if (Properties.Settings.Default.UpgradeNeeded)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeNeeded = false;
                Properties.Settings.Default.Save();
            }

            _context = SynchronizationContext.Current;
            if (_context == null)
            {
                _context = new SynchronizationContext();
            }

            return base.Initialize(core);
        }

        public override Framework.PluginType PluginType
        {
            get
            {
                return Framework.PluginType.General;
            }
        }

        public override string FriendlyName
        {
            get
            {
                return STR_GPSSERIAL;
            }
        }

        public override void ApplicationClosing()
        {
            stopReading();
        }

        public override List<System.Windows.Forms.UserControl> CreateConfigurationPanels()
        {
            List<System.Windows.Forms.UserControl> pnls = base.CreateConfigurationPanels();
            if (pnls == null) pnls = new List<System.Windows.Forms.UserControl>();
            pnls.Add(new SettingsPanel());
            return pnls;
        }

        public override bool ApplySettings(List<System.Windows.Forms.UserControl> configPanels)
        {
            SettingsPanel panel = (from p in configPanels where p.GetType() == typeof(SettingsPanel) select p).FirstOrDefault() as SettingsPanel;
            panel.Apply();
            if (_serialPort != null)
            {
                stopReading();
                startReading();
            }
            return true;
        }

        private void startReading()
        {
            if (_serialPort == null)
            {
                try
                {
                    _hasGPRMC = false;
                    _serialPort = new System.IO.Ports.SerialPort(Properties.Settings.Default.ComPort, Properties.Settings.Default.BaudRate, (System.IO.Ports.Parity)Enum.Parse(typeof(System.IO.Ports.Parity), Properties.Settings.Default.Parity), Properties.Settings.Default.Databits, (System.IO.Ports.StopBits)Enum.Parse(typeof(System.IO.Ports.StopBits), Properties.Settings.Default.StopBits));
                    _serialPort.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(_serialPort_DataReceived);
                    _serialPort.Open();
                    _activeNotification = new ActiveNotification(Core);
                    _activeNotification.Stop += new EventHandler<EventArgs>(_activeNotification_Stop);
                    this.OnNotification(_activeNotification);
                }
                catch
                {
                    _serialPort = null;
                }
            }
        }

        void _activeNotification_Stop(object sender, EventArgs e)
        {
            stopReading();
        }

        void _serialPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            _reading = true;
            //not in UI thread!
            char[] splitter = new char[] { ',', '*' };
            try
            {
                string s = _serialPort.ReadLine();
                Core.DebugLog(Framework.Data.DebugLogLevel.Info, this, null, string.Format("{0} - {1}", DateTime.Now.ToString("HH:mm:ss.fff"), s));

                /*
                 * eg4. for NMEA 0183 version 3.00 active the Mode indicator field is added
                         $GPRMC,hhmmss.ss,A,llll.ll,a,yyyyy.yy,a,x.x,x.x,ddmmyy,x.x,a,m*hh
                    Field #
                    1    = UTC time of fix
                    2    = Data status (A=Valid position, V=navigation receiver warning)
                    3    = Latitude of fix
                    4    = N or S of longitude
                    5    = Longitude of fix
                    6    = E or W of longitude
                    7    = Speed over ground in knots
                    8    = Track made good in degrees True
                    9    = UTC date of fix
                    10   = Magnetic variation degrees (Easterly var. subtracts from true course)
                    11   = E or W of magnetic variation
                    12   = Mode indicator, (A=Autonomous, D=Differential, E=Estimated, N=Data not valid)
                    13   = Checksum
                 * 
                 * eg3. $GPGLL,5133.81,N,00042.25,W*75
                               1    2     3    4 5

                      1    5133.81   Current latitude
                      2    N         North/South
                      3    00042.25  Current longitude
                      4    W         East/West
                      5    *75       checksum

                 */
                s = s.Replace("\r", "").Replace("\n", "");
                if (s.StartsWith("$GPRMC"))
                {
                    _hasGPRMC = true;
                    if (checkCheckSum(s))
                    {
                        string[] parts = s.Split(splitter);
                        if (parts.Length > 7)
                        {
                            bool valid = parts[2] == "A";
                            Framework.Data.Location l = null;
                            if (valid)
                            {
                                string c = string.Format("{0} {1} {2} {3} {4} {5}",
                                    parts[4], parts[3].Substring(0, 2), parts[3].Substring(2, 6),
                                    parts[6], parts[5].Substring(0, 3), parts[5].Substring(3, 6)
                                    );
                                l = Utils.Conversion.StringToLocation(c);
                            }
                            _context.Post(new SendOrPostCallback(delegate(object state)
                            {
                                if (valid && l != null)
                                {
                                    Core.GPSLocation.UpdateGPSLocation(valid, l.Lat, l.Lon);
                                }
                                else
                                {
                                    Core.GPSLocation.UpdateGPSLocation(valid, Core.GPSLocation.Position.Lat, Core.GPSLocation.Position.Lon);
                                }
                                if (_activeNotification != null && !_activeNotification.IsDisposed && _activeNotification.Visible)
                                {
                                    _activeNotification.UpdateStatus(Core.GPSLocation);
                                }
                            }), null);
                        }
                    }
                }
                else if (!_hasGPRMC && s.StartsWith("$GPGLL"))
                {
                    if (checkCheckSum(s))
                    {
                        string[] parts = s.Split(splitter);
                        if (parts.Length > 6)
                        {
                            bool valid = parts[6] == "A";
                            Framework.Data.Location l = null;
                            if (valid)
                            {
                                string c = string.Format("{0} {1} {2} {3} {4} {5}", 
                                    parts[2], parts[1].Substring(0, 2), parts[1].Substring(2,6),
                                    parts[4], parts[3].Substring(0, 3), parts[3].Substring(3,6)
                                    );
                                l = Utils.Conversion.StringToLocation(c);
                            }
                            _context.Post(new SendOrPostCallback(delegate(object state)
                            {
                                if (valid && l!=null)
                                {
                                    Core.GPSLocation.UpdateGPSLocation(valid, l.Lat, l.Lon);
                                }
                                else
                                {
                                    Core.GPSLocation.UpdateGPSLocation(valid, Core.GPSLocation.Position.Lat, Core.GPSLocation.Position.Lon);
                                }
                                if (_activeNotification != null && !_activeNotification.IsDisposed && _activeNotification.Visible)
                                {
                                    _activeNotification.UpdateStatus(Core.GPSLocation);
                                }
                            }), null);
                        }
                    }
                }
            }
            catch
            {
            }
            _reading = false;
        }

        private bool checkCheckSum(string s)
        {
            bool result = true;
            int pos = s.IndexOf('*');
            if (pos > 0)
            {
                int cs = int.Parse(s.Substring(pos + 1), System.Globalization.NumberStyles.HexNumber);
                int x = 0;
                for (int i = 1; i < pos; i++)
                {
                    x ^= s[i]; 
                }
                result = x == cs;
            }
            return result;
        }

        private void stopReading()
        {
            if (_serialPort != null)
            {
                try
                {
                    _serialPort.DataReceived -= new System.IO.Ports.SerialDataReceivedEventHandler(_serialPort_DataReceived);
                    while (_reading)
                    {
                        System.Threading.Thread.Sleep(100);
                    }
                    _serialPort.Dispose();
                }
                catch
                {
                }
                _serialPort = null;

                if (_activeNotification != null)
                {
                    _activeNotification.Stop -= new EventHandler<EventArgs>(_activeNotification_Stop);
                    _activeNotification.Visible = false;
                    _activeNotification = null;
                }
            }
        }

        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result)
            {
                if (action == ACTION_START)
                {
                    startReading();
                }
                else if (action == ACTION_STOP)
                {
                    stopReading();
                }
            }
            return result;
        }

    }
}
