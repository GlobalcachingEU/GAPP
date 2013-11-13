using GAPPSF.Core.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GAPPSF.Core
{
    public class ApplicationData: INotifyPropertyChanged
    {
        private static ApplicationData _uniqueInstance = null;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow MainWindow { get; set; }

        public Storage.DatabaseCollection Databases { get; private set; }
        public GeocacheTypeCollection GeocacheTypes { get; private set; }
        public GeocacheContainerCollection GeocacheContainers { get; private set; }
        public LogTypeCollection LogTypes { get; private set; }
        public Data.Location HomeLocation { get; private set; }
        public Data.Location CenterLocation { get; private set; }

        private Storage.Database _activeDatabase = null;
        public Storage.Database ActiveDatabase
        {
            get { return _activeDatabase; }
            set 
            {
                if (_activeDatabase != value)
                {
                    ActiveGeocache = null;
                    SetProperty(ref _activeDatabase, value);
                }
            }
        }

        private Data.Geocache _activeGeocache = null;
        public Data.Geocache ActiveGeocache
        {
            get { return _activeGeocache; }
            set { SetProperty(ref _activeGeocache, value); }
        }

        private int _activityCounter = 0;
        public void BeginActiviy()
        {
            _activityCounter++;
            if (_activityCounter==1)
            {
                UIIsIdle = false;
            }
        }
        public void EndActiviy()
        {
            _activityCounter--;
            if (_activityCounter == 0)
            {
                UIIsIdle = true;
            }
        }

        private bool _uiIsIdle = true;
        public bool UIIsIdle
        {
            get { return _uiIsIdle; }
            set
            {
                if (_uiIsIdle != value)
                {
                    SetProperty(ref _uiIsIdle, value);
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }
        
        private ApplicationData()
        {
#if DEBUG
            if (_uniqueInstance != null)
            {
                //you used the wrong binding
                //use: 
                //<properties:ApplicationData x:Key="ApplicationData" />
                //{Binding Databases, Source={x:Static p:ApplicationData.Instance}}
                //{Binding Source={x:Static p:ApplicationData.Instance}, Path=ActiveDatabase.GeocacheCollection}
                System.Diagnostics.Debugger.Break();
            }
#endif

            Databases = new Storage.DatabaseCollection();
            GeocacheTypes = new GeocacheTypeCollection();
            GeocacheContainers = new GeocacheContainerCollection();
            LogTypes = new LogTypeCollection();
            HomeLocation = new Location(Settings.Default.HomeLocationLat, Settings.Default.HomeLocationLon);
            CenterLocation = new Location(Settings.Default.CenterLocationLat, Settings.Default.CenterLocationLon);

            HomeLocation.PropertyChanged += HomeLocation_PropertyChanged;
            CenterLocation.PropertyChanged += CenterLocation_PropertyChanged;

            addCacheType(0, "Not present");
            addCacheType(2, "Traditional Cache");
            addCacheType(3, "Multi-cache");
            addCacheType(4, "Virtual Cache");
            addCacheType(5, "Letterbox Hybrid");
            addCacheType(6, "Event Cache");
            addCacheType(8, "Unknown (Mystery) Cache", "Unknown Cache");
            addCacheType(9, "Project APE Cache");
            addCacheType(11, "Webcam Cache");
            addCacheType(12, "Locationless (Reverse) Cache");
            addCacheType(13, "Cache In Trash Out Event");
            addCacheType(137, "Earthcache");
            addCacheType(453, "Mega-Event Cache");
            addCacheType(605, "Geocache Course");
            addCacheType(1304, "GPS Adventures Exhibit");
            addCacheType(1858, "Wherigo Cache");
            addCacheType(3653, "Lost and Found Event Cache");
            addCacheType(3773, "Groundspeak HQ");
            addCacheType(3774, "Groundspeak Lost and Found Celebration");
            addCacheType(4738, "Groundspeak Block Party");

            addCacheContainer(0, "Unknown");
            addCacheContainer(1, "Not chosen");
            addCacheContainer(2, "Micro");
            addCacheContainer(3, "Regular");
            addCacheContainer(4, "Large");
            addCacheContainer(5, "Virtual");
            addCacheContainer(6, "Other");
            addCacheContainer(8, "Small");

            addLogType(0, "Unknown", false);
            addLogType(1, "Unarchive", false);
            addLogType(2, "Found it", true);
            addLogType(3, "Didn't find it", false);
            addLogType(4, "Write note", false);
            addLogType(5, "Archive", false);
            addLogType(6, "Archive", false);
            addLogType(7, "Needs Archived", false);
            addLogType(8, "Mark Destroyed", false);
            addLogType(9, "Will Attend", false);
            addLogType(10, "Attended", true);
            addLogType(11, "Webcam Photo Taken", true);
            addLogType(12, "Unarchive", false);
            addLogType(13, "Retrieve It from a Cache", false);
            addLogType(14, "Dropped Off", false);
            addLogType(15, "Transfer", false);
            addLogType(16, "Mark Missing", false);
            addLogType(17, "Recovered", false);
            addLogType(18, "Post Reviewer Note", false);
            addLogType(19, "Grab It (Not from a Cache)", false);
            addLogType(20, "Write Jeep 4x4 Contest Essay", false);
            addLogType(21, "Upload Jeep 4x4 Contest Photo", false);
            addLogType(22, "Temporarily Disable Listing", false);
            addLogType(23, "Enable Listing", false);
            addLogType(24, "Publish Listing", false);
            addLogType(25, "Retract Listing", false);
            addLogType(30, "Uploaded Goal Photo for \"A True Original\"", false);
            addLogType(31, "Uploaded Goal Photo for \"Yellow Jeep Wrangler\"", false);
            addLogType(32, "Uploaded Goal Photo for \"Construction Site\"", false);
            addLogType(33, "Uploaded Goal Photo for \"State Symbol\"", false);
            addLogType(34, "Uploaded Goal Photo for \"American Flag\"", false);
            addLogType(35, "Uploaded Goal Photo for \"Landmark/Memorial\"", false);
            addLogType(36, "Uploaded Goal Photo for \"Camping\"", false);
            addLogType(37, "Uploaded Goal Photo for \"Peaks and Valleys\"", false);
            addLogType(38, "Uploaded Goal Photo for \"Hiking\"", false);
            addLogType(39, "Uploaded Goal Photo for \"Ground Clearance\"", false);
            addLogType(40, "Uploaded Goal Photo for \"Water Fording\"", false);
            addLogType(41, "Uploaded Goal Photo for \"Traction\"", false);
            addLogType(42, "Uploaded Goal Photo for \"Tow Package\"", false);
            addLogType(43, "Uploaded Goal Photo for \"Ultimate Makeover\"", false);
            addLogType(44, "Uploaded Goal Photo for \"Paint Job\"", false);
            addLogType(45, "Needs Maintenance", false);
            addLogType(46, "Owner Maintenance", false);
            addLogType(47, "Update Coordinates", false);
            addLogType(48, "Discovered It", false);
            addLogType(49, "Uploaded Goal Photo for \"Discovery\"", false);
            addLogType(50, "Uploaded Goal Photo for \"Freedom\"", false);
            addLogType(51, "Uploaded Goal Photo for \"Adventure\"", false);
            addLogType(52, "Uploaded Goal Photo for \"Camaraderie\"", false);
            addLogType(53, "Uploaded Goal Photo for \"Heritage\"", false);
            addLogType(54, "Reviewer Note", false);
            addLogType(55, "Lock User (Ban)", false);
            addLogType(56, "Unlock User (Unban)", false);
            addLogType(57, "Groundspeak Note", false);
            addLogType(58, "Uploaded Goal Photo for \"Fun\"", false);
            addLogType(59, "Uploaded Goal Photo for \"Fitness\"", false);
            addLogType(60, "Uploaded Goal Photo for \"Fighting Diabetes\"", false);
            addLogType(61, "Uploaded Goal Photo for \"American Heritage\"", false);
            addLogType(62, "Uploaded Goal Photo for \"No Boundaries\"", false);
            addLogType(63, "Uploaded Goal Photo for \"Only in a Jeep\"", false);
            addLogType(64, "Uploaded Goal Photo for \"Discover New Places\"", false);
            addLogType(65, "Uploaded Goal Photo for \"Definition of Freedom\"", false);
            addLogType(66, "Uploaded Goal Photo for \"Adventure Starts Here\"", false);
            addLogType(67, "Needs Attention", false);
            addLogType(68, "Post Reviewer Note", false);
            addLogType(69, "Move To Collection", false);
            addLogType(70, "Move To Inventory", false);
            addLogType(71, "Throttle User", false);
            addLogType(72, "Enter CAPTCHA", false);
            addLogType(73, "Change Username", false);
            addLogType(74, "Announcement", false);
            addLogType(75, "Visited", false);
        }

        void CenterLocation_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Data.Location ll = sender as Data.Location;
            if (ll != null)
            {
                if (e.PropertyName == "Lat")
                {
                    Settings.Default.CenterLocationLat = ll.Lat;
                }
                else if (e.PropertyName == "Lon")
                {
                    Settings.Default.CenterLocationLon = ll.Lon;
                }
            }
        }

        void HomeLocation_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Data.Location ll = sender as Data.Location;
            if (ll != null)
            {
                if (e.PropertyName == "Lat")
                {
                    Settings.Default.HomeLocationLat = ll.Lat;
                }
                else if (e.PropertyName == "Lon")
                {
                    Settings.Default.HomeLocationLon = ll.Lon;
                }
            }
        }

        public static ApplicationData Instance
        {
            get
            {
                if (_uniqueInstance==null)
                {
                    _uniqueInstance = new ApplicationData();
                }
                return _uniqueInstance;
            }
        }

        protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string name = "")
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                var handler = PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs(name));
                }
            }
        }

        private void addCacheType(int id, string name)
        {
            GeocacheType ct = new GeocacheType();
            ct.ID = id;
            ct.Name = name;
            GeocacheTypes.Add(ct);
        }
        private void addCacheType(int id, string name, string gpxTag)
        {
            GeocacheType ct = new GeocacheType(gpxTag);
            ct.ID = id;
            ct.Name = name;
            GeocacheTypes.Add(ct);
        }

        private void addCacheContainer(int id, string name)
        {
            Data.GeocacheContainer attr = new Data.GeocacheContainer();
            attr.ID = id;
            attr.Name = name;
            GeocacheContainers.Add(attr);
        }

        protected void addLogType(int id, string name, bool asFound)
        {
            Data.LogType lt = new Data.LogType();
            lt.ID = id;
            lt.Name = name;
            lt.AsFound = asFound;
            LogTypes.Add(lt);
        }


    }
}
