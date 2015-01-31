using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.ICal
{
    public partial class AddToCalendarForm : Form
    {
        public const string STR_TITLE = "Add events to calendar";
        public const string STR_ADDTOCAL = "Add all to calendar";
        public const string STR_ADDONETOCAL = "Add next to calendar";
        public const string STR_GEOCACHEEVENT = "Geocache event";
        public const string STR_BY = "by";
        public const string STR_DESCRIPTION = "Click the link to open up the cache page for the event";
        public const string STR_SAVE = "Save";
        public const string STR_GOOGLEXPL = "In google agenda select Other Agenda's, Import agenda and paste file name";
        public const string STR_ADDTO = "Add to";
        public const string STR_EVENT = "Event";
        public const string STR_START = "Start";
        public const string STR_END = "End";
        public const string STR_LOCATION = "Location";
        public const string STR_SUMMARY = "Summary";
        public const string STR_DESCRIPTIONB = "Description";

        public class CalendarEntry
        {
            public Framework.Data.Geocache gc { get; set; }
            public bool Enabled { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
            public string Summary { get; set; }
            public string Description { get; set; }
            public string Location { get; set; }

            public override string ToString()
            {
                return string.Format("{0}, {1}", gc.Code ?? "", gc.Name ?? "");
            }
        }
        private List<CalendarEntry> _calendarEntries = null;
        private Framework.Interfaces.ICore _core = null;

        public AddToCalendarForm()
        {
            InitializeComponent();
        }

        public AddToCalendarForm(Framework.Interfaces.ICore core, List<Framework.Data.Geocache> gcList)
            : this()
        {
            _core = core;

            checkBoxGoogleAgenda.Checked = PluginSettings.Instance.AddToGoogleCalendar;
            checkBoxOutlook.Checked = PluginSettings.Instance.AddToOutlook;
            checkBoxOpenGoogleCalendar.Checked = PluginSettings.Instance.OpenGoogleCalendar;

            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.buttonAddToCalendar.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ADDTOCAL);
            this.buttonAddNextToCalendar.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ADDONETOCAL);
            this.buttonSave.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SAVE);
            this.label13.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ADDTO);
            this.label12.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_EVENT);
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_START);
            this.label2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_END);
            this.label6.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_LOCATION);
            this.label8.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SUMMARY);
            this.label10.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DESCRIPTIONB);

            _calendarEntries = new List<CalendarEntry>();
            foreach (var gc in gcList)
            {
                CalendarEntry ce = new CalendarEntry();
                ce.gc = gc;
                ce.Enabled = true;
                ce.StartTime = new DateTime(gc.PublishedTime.Year, gc.PublishedTime.Month, gc.PublishedTime.Day, 9, 0, 0);
                ce.EndTime = new DateTime(gc.PublishedTime.Year, gc.PublishedTime.Month, gc.PublishedTime.Day, 17, 0, 0);
                ce.Summary = string.Format("{0}: {1} {2} {3}", Utils.LanguageSupport.Instance.GetTranslation(STR_GEOCACHEEVENT), gc.Name == null ? "" : gc.Name.Replace('"', ' '), Utils.LanguageSupport.Instance.GetTranslation(STR_BY), gc.PlacedBy == null ? "" : gc.PlacedBy.Replace('"', ' '));
                ce.Location = Utils.Conversion.GetCoordinatesPresentation(gc.Lat, gc.Lon);
                ce.Description = string.Format("{0} {1}", Utils.LanguageSupport.Instance.GetTranslation(STR_GEOCACHEEVENT), gc.Url ?? "");
                _calendarEntries.Add(ce);

                checkedListBox1.SetItemChecked(checkedListBox1.Items.Add(ce), true);                
            }
            buttonAddToCalendar.Enabled = _calendarEntries.Count > 0;
            buttonAddNextToCalendar.Enabled = _calendarEntries.Count > 0;
            checkedListBox1_SelectedIndexChanged(this, EventArgs.Empty);

            labelGoogleExpl.Visible = false;
            labelICSFilename.Visible = false;
            labelGoogleExpl.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_GOOGLEXPL);
        }

        private string createIcsContent(List<CalendarEntry> ces)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("BEGIN:VCALENDAR");
            sb.AppendLine("VERSION:2.0");
            sb.AppendLine("PRODID:-//GAPP// v2.0//EN");
            foreach (CalendarEntry ce in ces)
            {
                if (ce.Enabled)
                {
                    sb.AppendLine("BEGIN:VEVENT");
                    sb.AppendLine(string.Format("DTSTAMP:{0}Z", ce.StartTime.ToUniversalTime().ToString("s").Replace("-", "").Replace(":", "").Replace("/", "")));
                    sb.AppendLine(string.Format("DTSTART:{0}Z", ce.StartTime.ToUniversalTime().ToString("s").Replace("-", "").Replace(":", "").Replace("/", "")));
                    sb.AppendLine(string.Format("DTEND:{0}Z", ce.EndTime.ToUniversalTime().ToString("s").Replace("-", "").Replace(":", "").Replace("/", "")));
                    sb.AppendLine(string.Format("SUMMARY;ENCODING=QUOTED-PRINTABLE:{0}", ce.Summary));
                    sb.AppendLine(string.Format("LOCATION;ENCODING=QUOTED-PRINTABLE:{0}", ce.Location));
                    sb.AppendLine(string.Format("DESCRIPTION;ENCODING=QUOTED-PRINTABLE:{0}", ce.Description));
                    //sb.AppendLine(string.Format("SUMMARY:{0}", ce.Summary));
                    //sb.AppendLine(string.Format("LOCATION:{0}", ce.Location));
                    //sb.AppendLine(string.Format("DESCRIPTION:{0}", ce.Description));
                    sb.AppendLine("END:VEVENT");
                }
            }
            sb.AppendLine("END:VCALENDAR");
            return sb.ToString();
        }

        private void AddToCalendar(string ics)
        {
            try
            {
                string fn = System.IO.Path.Combine(_core.PluginDataPath, "AddToCalendar.ics" );
                System.IO.File.WriteAllText(fn, ics);

                if (PluginSettings.Instance.AddToOutlook)
                {
                    System.Diagnostics.Process.Start(fn);
                }
                if (PluginSettings.Instance.AddToGoogleCalendar)
                {
                    //System.Collections.Specialized.StringCollection FileCollection = new System.Collections.Specialized.StringCollection();
                    //FileCollection.Add(fn);
                    //Clipboard.SetFileDropList(FileCollection);
                    Clipboard.SetText(fn);
                    if (PluginSettings.Instance.OpenGoogleCalendar)
                    {
                        System.Diagnostics.Process.Start("https://www.google.com/calendar?tab=mc");
                    }
                    labelICSFilename.Text = fn;
                    labelICSFilename.Visible = true;
                    labelGoogleExpl.Visible = true;
                    labelGoogleExpl.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_GOOGLEXPL);
                }
            }
            catch
            {
            }
        }

        private void buttonAddToCalendar_Click(object sender, EventArgs e)
        {
            string s = createIcsContent(_calendarEntries);
            AddToCalendar(s);
            for (int i = 0; i < checkedListBox1.Items.Count; i++ )
            {
                checkedListBox1.SetItemChecked(i, false);
                (checkedListBox1.Items[i] as CalendarEntry).Enabled = false;
            }
            buttonAddToCalendar.Enabled = false;
            buttonAddNextToCalendar.Enabled = false;
        }

        private void checkedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            (checkedListBox1.Items[e.Index] as CalendarEntry).Enabled = e.NewValue == CheckState.Checked;
            buttonAddToCalendar.Enabled = (from c in _calendarEntries where c.Enabled select c).FirstOrDefault()!=null;
            buttonAddNextToCalendar.Enabled = buttonAddToCalendar.Enabled;
        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (checkedListBox1.SelectedIndex < 0)
            {
                labelEventName.Text = "-";
                dateTimePicker1.Enabled = false;
                dateTimePicker2.Enabled = false;
                textBox1.Enabled = false;
                textBox2.Enabled = false;
                textBox3.Enabled = false;
                buttonSave.Enabled = false;
            }
            else
            {
                CalendarEntry ce = checkedListBox1.Items[checkedListBox1.SelectedIndex] as CalendarEntry;
                dateTimePicker1.Enabled = true;
                dateTimePicker1.Value = ce.StartTime;
                dateTimePicker2.Enabled = true;
                dateTimePicker2.Value = ce.EndTime;
                textBox1.Enabled = true;
                textBox1.Text = ce.Location;
                textBox2.Enabled = true;
                textBox2.Text = ce.Summary;
                textBox3.Enabled = true;
                textBox3.Text = ce.Description;
                buttonSave.Enabled = true;
                labelEventName.Text = ce.ToString(); ;
            }
        }

        private void buttonAddNextToCalendar_Click(object sender, EventArgs e)
        {
            CalendarEntry ce = (from c in _calendarEntries where c.Enabled select c).FirstOrDefault();
            if (ce != null)
            {
                List<CalendarEntry> ces = new List<CalendarEntry>();
                ces.Add(ce);
                AddToCalendar(createIcsContent(ces));
                checkedListBox1.SetItemChecked(checkedListBox1.Items.IndexOf(ce), false);
                ce.Enabled = false;

                buttonAddToCalendar.Enabled = (from c in _calendarEntries where c.Enabled select c).FirstOrDefault() != null;
                buttonAddNextToCalendar.Enabled = buttonAddToCalendar.Enabled;
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            CalendarEntry ce = checkedListBox1.Items[checkedListBox1.SelectedIndex] as CalendarEntry;
            if (ce != null)
            {
                ce.StartTime = dateTimePicker1.Value;
                ce.EndTime = dateTimePicker2.Value;
                ce.Location = textBox1.Text.Replace('"',' ');
                ce.Summary = textBox2.Text.Replace('"', ' ');
                ce.Description = textBox3.Text.Replace('"', ' ');
            }
        }

        private void checkBoxOutlook_CheckedChanged(object sender, EventArgs e)
        {
            PluginSettings.Instance.AddToOutlook = checkBoxOutlook.Checked;
        }

        private void checkBoxGoogleAgenda_CheckedChanged(object sender, EventArgs e)
        {
            PluginSettings.Instance.AddToGoogleCalendar = checkBoxGoogleAgenda.Checked;
        }

        private void checkBoxOpenGoogleCalendar_CheckedChanged(object sender, EventArgs e)
        {
            PluginSettings.Instance.OpenGoogleCalendar = checkBoxOpenGoogleCalendar.Checked;
        }
    }
}
