using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.GPSRSerial
{
    public partial class ActiveNotification : Utils.Controls.NotificationMessageBox
    {
        public const string STR_SERVICEACTIVE = "GPS location service is active";
        public const string STR_POSITION = "Position";
        public const string STR_STOP = "Stop";
        public const string STR_CENTER = "Center";

        public event EventHandler<EventArgs> Stop;

        private Framework.Data.Location _activePosition = null;
        private Framework.Interfaces.ICore _core = null;

        public ActiveNotification()
        {
            InitializeComponent();
        }
        public ActiveNotification(Framework.Interfaces.ICore core)
            : this()
        {
            _core = core;

            label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SERVICEACTIVE);
            label2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_POSITION);
            button1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_STOP);
            button2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CENTER);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (Stop != null)
            {
                Stop(this, EventArgs.Empty);
            }
        }

        public void UpdateStatus(Framework.Data.GPSLocation loc)
        {
            if (loc.Valid != button2.Enabled)
            {
                button2.Enabled = loc.Valid;
            }
            if (loc.Valid)
            {
                _activePosition = loc.Position;
                label4.Text = Utils.Conversion.GetCoordinatesPresentation(_activePosition);
            }
            else
            {
                label4.Text = "?";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (_activePosition != null && _core != null)
            {
                _core.CenterLocation.SetLocation(_activePosition.Lat, _activePosition.Lon);
                _core.Geocaches.BeginUpdate();
                foreach (Framework.Data.Geocache gc in _core.Geocaches)
                {
                    Utils.Calculus.SetDistanceAndAngleGeocacheFromLocation(gc, _core.CenterLocation);
                }
                _core.Geocaches.EndUpdate();
            }
        }
    }
}
