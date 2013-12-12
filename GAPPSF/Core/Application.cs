using GAPPSF.Core.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml.Serialization;

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
        public WaypointTypeCollection WaypointTypes { get; private set; }
        public Data.Location HomeLocation { get; private set; }
        public Data.Location CenterLocation { get; private set; }
        public AccountInfoCollection AccountInfos { get; private set; }
        public GeocacheAttributeCollection GeocacheAttributes { get; private set; }

        private bool _switchingDatabase = false;
        private Storage.Database _activeDatabase = null;
        public Storage.Database ActiveDatabase
        {
            get { return _activeDatabase; }
            set 
            {
                if (_activeDatabase != value)
                {
                    _switchingDatabase = true;
                    Core.Settings.Default.ActiveDatabase = value == null ? null : value.FileName;
                    ActiveGeocache = null;
                    SetProperty(ref _activeDatabase, value);
                    if (_activeDatabase!=null)
                    {
                        string gcCode = _activeDatabase.LastActiveGeocacheCode;
                        if (!string.IsNullOrEmpty(gcCode))
                        {
                            ActiveGeocache = _activeDatabase.GeocacheCollection.GetGeocache(gcCode);
                        }
                    }
                    _switchingDatabase = false;
                }
            }
        }

        private Data.Geocache _activeGeocache = null;
        public Data.Geocache ActiveGeocache
        {
            get { return _activeGeocache; }
            set 
            {
                Core.Settings.Default.ActiveGeocache = value == null ? null : value.Code;
                if (!_switchingDatabase && ActiveDatabase != null)
                {
                    ActiveDatabase.LastActiveGeocacheCode = value == null ? "" : value.Code;
                }
                SetProperty(ref _activeGeocache, value); 
            }
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
        
        public ApplicationData()
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
            string s = Settings.Default.AccountInfos;
            //s = "";
            if (string.IsNullOrEmpty(s))
            {
                AccountInfos = new AccountInfoCollection();
            }
            else
            {
                Type[] itemTypes = { typeof(AccountInfo) };
                XmlSerializer serializer = new XmlSerializer(typeof(AccountInfoCollection), itemTypes);
                using (StringReader r = new StringReader(s))
                {
                    AccountInfos = serializer.Deserialize(r) as AccountInfoCollection;
                }            
            }
            GeocacheAttributes = new GeocacheAttributeCollection();
            Databases = new Storage.DatabaseCollection();
            GeocacheTypes = new GeocacheTypeCollection();
            GeocacheContainers = new GeocacheContainerCollection();
            WaypointTypes = new WaypointTypeCollection();
            LogTypes = new LogTypeCollection();
            HomeLocation = new Location(Settings.Default.HomeLocationLat, Settings.Default.HomeLocationLon);
            CenterLocation = new Location(Settings.Default.CenterLocationLat, Settings.Default.CenterLocationLon);

            HomeLocation.PropertyChanged += HomeLocation_PropertyChanged;
            CenterLocation.PropertyChanged += CenterLocation_PropertyChanged;
            AccountInfos.CollectionChanged += AccountInfos_CollectionChanged;

            string[] accountPrefixes = new string[] { "GC", "OX", "OB", "OC", "MZ", "OU", "OP"};
            foreach (string acc in accountPrefixes)
            {
                if (AccountInfos.GetAccountInfo(acc) == null)
                {
                    AccountInfos.Add(new AccountInfo(acc, ""));
                }
            }

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

            addWaypointType(0, "Unknown");
            addWaypointType(217, "Parking Area");
            addWaypointType(220, "Final Location");
            addWaypointType(218, "Question to Answer");
            addWaypointType(452, "Reference Point");
            addWaypointType(219, "Stages of a Multicache");
            addWaypointType(221, "Trailhead");


            addCacheAttribute(0, "Unknown");
            addCacheAttribute(1, "Dogs");
            addCacheAttribute(2, "Access or parking fee");
            addCacheAttribute(3, "Climbing gear");
            addCacheAttribute(4, "Boat");
            addCacheAttribute(5, "Scuba gear");
            addCacheAttribute(6, "Recommended for kids");
            addCacheAttribute(7, "Takes less than an hour");
            addCacheAttribute(8, "Scenic view");
            addCacheAttribute(9, "Significant Hike");
            addCacheAttribute(10, "Difficult climbing");
            addCacheAttribute(11, "May require wading");
            addCacheAttribute(12, "May require swimming");
            addCacheAttribute(13, "Available at all times");
            addCacheAttribute(14, "Recommended at night");
            addCacheAttribute(15, "Available during winter");
            addCacheAttribute(16, "Cactus");
            addCacheAttribute(17, "Poison plants");
            addCacheAttribute(18, "Dangerous Animals");
            addCacheAttribute(19, "Ticks");
            addCacheAttribute(20, "Abandoned mines");
            addCacheAttribute(21, "Cliff / falling rocks");
            addCacheAttribute(22, "Hunting");
            addCacheAttribute(23, "Dangerous area");
            addCacheAttribute(24, "Wheelchair accessible");
            addCacheAttribute(25, "Parking available");
            addCacheAttribute(26, "Public transportation");
            addCacheAttribute(27, "Drinking water nearby");
            addCacheAttribute(28, "Public restrooms nearby");
            addCacheAttribute(29, "Telephone nearby");
            addCacheAttribute(30, "Picnic tables nearby");
            addCacheAttribute(31, "Camping available");
            addCacheAttribute(32, "Bicycles");
            addCacheAttribute(33, "Motorcycles");
            addCacheAttribute(34, "Quads");
            addCacheAttribute(35, "Off-road vehicles");
            addCacheAttribute(36, "Snowmobiles");
            addCacheAttribute(37, "Horses");
            addCacheAttribute(38, "Campfires");
            addCacheAttribute(39, "Thorns");
            addCacheAttribute(40, "Stealth required");
            addCacheAttribute(41, "Stroller accessible");
            addCacheAttribute(42, "Needs maintenance");
            addCacheAttribute(43, "Watch for livestock");
            addCacheAttribute(44, "Flashlight required");
            addCacheAttribute(45, "Lost And Found Tour");
            addCacheAttribute(46, "Truck Driver/RV");
            addCacheAttribute(47, "Field Puzzle");
            addCacheAttribute(48, "UV Light Required");
            addCacheAttribute(49, "Snowshoes");
            addCacheAttribute(50, "Cross Country Skis");
            addCacheAttribute(51, "Special Tool Required");
            addCacheAttribute(52, "Night Cache");
            addCacheAttribute(53, "Park and Grab");
            addCacheAttribute(54, "Abandoned Structure");
            addCacheAttribute(55, "Short hike (less than 1km)");
            addCacheAttribute(56, "Medium hike (1km-10km)");
            addCacheAttribute(57, "Long Hike (+10km)");
            addCacheAttribute(58, "Fuel Nearby");
            addCacheAttribute(59, "Food Nearby");
            addCacheAttribute(60, "Wireless Beacon");
            addCacheAttribute(61, "Partnership Cache");
            addCacheAttribute(62, "Seasonal Access");
            addCacheAttribute(63, "Tourist Friendly");
            addCacheAttribute(64, "Tree Climbing");
            addCacheAttribute(65, "Front Yard (Private Residence)");
            addCacheAttribute(66, "Teamwork Required");
            addCacheAttribute(106, "Only loggable at Opencaching");
            addCacheAttribute(108, "Letterbox (needs stamp)");
            addCacheAttribute(123, "First aid available");
            addCacheAttribute(125, "Long walk");
            addCacheAttribute(127, "Hilly area");
            addCacheAttribute(130, "Point of interest");
            addCacheAttribute(132, "Webcam");
            addCacheAttribute(133, "Within enclosed rooms (caves, buildings etc.");
            addCacheAttribute(134, "In the water");
            addCacheAttribute(135, "Without GPS (letterboxes, cistes, compass juggling ...)");
            addCacheAttribute(137, "Overnight stay necessary");
            addCacheAttribute(139, "Only available at specified times");
            addCacheAttribute(140, "By day only");
            addCacheAttribute(141, "Tide");
            addCacheAttribute(142, "All seasons");
            addCacheAttribute(143, "Breeding season / protected nature");
            addCacheAttribute(147, "Compass");
            addCacheAttribute(150, "Cave equipment");
            addCacheAttribute(153, "Aircraft");
            addCacheAttribute(154, "Investigation");
            addCacheAttribute(156, "Arithmetical problem");
            addCacheAttribute(157, "Other cache type");
            addCacheAttribute(158, "Ask owner for start conditions");

        }

        void AccountInfos_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Type[] itemTypes = { typeof(AccountInfo) };
            XmlSerializer serializer = new XmlSerializer(typeof(AccountInfoCollection), itemTypes);
            using (StringWriter w = new StringWriter())
            {
                serializer.Serialize(w, AccountInfos);
                Settings.Default.AccountInfos = w.ToString();
            }
            if (PropertyChanged!=null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("AccountInfos"));
            }
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
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("CenterLocation"));
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
                if (PropertyChanged!=null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("HomeLocation"));
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

        protected void addWaypointType(int id, string name)
        {
            Data.WaypointType attr = new Data.WaypointType();
            attr.ID = id;
            attr.Name = name;
            WaypointTypes.Add(attr);
        }

        protected void addCacheAttribute(int id, string name)
        {
            Data.GeocacheAttribute attr = new Data.GeocacheAttribute();
            attr.ID = id;
            attr.Name = name;
            GeocacheAttributes.Add(attr);
        }

    }
}
