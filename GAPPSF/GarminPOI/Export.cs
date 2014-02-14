using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.GarminPOI
{
    public class Export
    {
        private int intMaxNameLen = 22; //just for not always having to access the properties
        private int intMaxDescLen = 84;
        private bool nameIsGCCode = false;
        private bool boolExportCaches = true;
        private bool boolExportWpts = true;
        private string strProgTitle = null;
        private string strProgSubtitle = null;
        private Dictionary<string, System.IO.StreamWriter> sd = null;

        public async Task ExportToGarminPOIAsync(Core.Storage.Database db, List<Core.Data.Geocache> gcList)
        {
            await Task.Run(() =>
                {
                    try
                    {
                        ExportToGarminPOI(db, gcList);
                    }
                    catch (Exception e)
                    {
                        Core.ApplicationData.Instance.Logger.AddLog(this, e);
                    }

                });
        }

        public void ExportToGarminPOI(Core.Storage.Database db, List<Core.Data.Geocache> gcList)
        {
            intMaxNameLen = Core.Settings.Default.GarminPOINameLengthLimit;
            intMaxDescLen = Core.Settings.Default.GarminPOIDescriptionLengthLimit;
            boolExportCaches = Core.Settings.Default.GarminPOIExportGeocachePOIs;
            boolExportWpts = Core.Settings.Default.GarminPOIExportWaypointPOIs;
            strProgTitle = "ExportingPOI";
            strProgSubtitle = "CreatingFile";
            nameIsGCCode = (Core.Settings.Default.GarminPOIPOINameTypeName == UIControls.GeocacheFilter.BooleanEnum.False);

            List<Core.Data.Waypoint> wpList = null;

            if (Core.Settings.Default.GarminPOIClearExportDirectory)
            {
                //Try to assure that the path is not the root of a drive or empty
                if ((Core.Settings.Default.GarminPOIExportPath != "") &&
                    (Core.Settings.Default.GarminPOIExportPath != Path.GetPathRoot(Core.Settings.Default.GarminPOIExportPath)))
                {
                    Array.ForEach(Directory.GetFiles(Core.Settings.Default.GarminPOIExportPath),
                        delegate(string path)
                        {
                            if ((Path.GetExtension(path).ToUpper() == ".CSV") ||
                                (Path.GetExtension(path).ToUpper() == ".BMP")) { File.Delete(path); }
                        });
                }
            }

            using (Utils.ProgressBlock progress = new Utils.ProgressBlock(
                strProgTitle, strProgSubtitle, gcList.Count, 0))
            {
                sd = new Dictionary<string, System.IO.StreamWriter>();
                {
                    int index = 0;
                    //todo: clear all csv-files in path?

                    //Iterate selected caches
                    foreach (var gc in gcList)
                    {
                        //cache name 
                        String name = nameIsGCCode ? gc.Code : ProcessName(gc.Name, intMaxNameLen);

                        //1st part of description: GC-Code + basic cache info
                        String desc = (!nameIsGCCode ? (gc.Code + " ") : "") +
                                TolerantSubstring(gc.GeocacheType.Name, 0, 2) +
                                "D" + gc.Difficulty.ToString("0.#").Replace(",", ".").Replace(".5", "5") +
                                "T" + gc.Terrain.ToString("0.#").Replace(",", ".").Replace(".5", "5") +
                                TolerantSubstring(gc.Container.Name, 0, 2) + " ";

                        //2nd part of description, put hints, notes, personalnote as limits allow
                        desc = desc + ProcessDescription((nameIsGCCode ? (gc.Name + ":") : "") +
                                                            gc.EncodedHints + " " +
                                                            gc.Notes + " " +
                                                            gc.PersonalNote, intMaxDescLen - desc.Length);
                        //"PersonaleNote" is from the docs
                        String lon = FmtCsvLatLon(gc.Lon);
                        String lat = FmtCsvLatLon(gc.Lat);
                        //use custom lat/lon if defined
                        if (gc.CustomLat != null && gc.CustomLon != null)
                        {
                            lon = FmtCsvLatLon((double)gc.CustomLon);//double? -> double
                            lat = FmtCsvLatLon((double)gc.CustomLat);
                        }
                        //write to CSV (file selected by gc type)
                        if (boolExportCaches)
                        {
                            CacheCsv(gc).WriteLine(lon + "," + lat + ",\"" + name + "\",\"" + desc + "\"");
                        }

                        /*
                         UserWaypoint
                              int ID (458421)
                            string GeocacheCode;
                            string Description; ("Coordinate Override")
                            double Lat;
                            double Lon;
                            DateTime Date;
                    public static List<Framework.Data.UserWaypoint> GetUserWaypointsFromGeocache(Framework.Data.UserWaypointCollection wpCollection, string geocacheCode)
                         */


                        //Iterate all waypoints of cache
                        wpList = Utils.DataAccess.GetWaypointsFromGeocache(db, gc.Code);
                        foreach (var wp in wpList)
                        { //only export waypoints with coords
                            if (wp.Lat != null && wp.Lon != null)
                            {
                                //Make Wpt name of Cachename + # + "wpt-ID 2-letter prefix"
                                //others may prefer simply wp.Code
                                String wpname = (nameIsGCCode ? gc.Code : ProcessName(gc.Name, intMaxNameLen - 3))
                                                    + "#" + TolerantSubstring(wp.Code, 0, 2);

                                String wpdesc = wp.Code + " ";
                                //depending on source the "real" name of the wpt sometimes is either in "Name" or "Description"
                                String wpd = wp.Name;
                                if (wpd == wp.Code)
                                {//use description if name only holds the wpcode
                                    wpd = wp.Description;
                                }
                                //POI description = name + comment
                                wpdesc = wpdesc + ProcessDescription(wpd + ": " +
                                                                    wp.Comment, intMaxDescLen - wpdesc.Length);
                                String wplon = FmtCsvLatLon((double)wp.Lon);
                                String wplat = FmtCsvLatLon((double)wp.Lat);
                                //wpts have no custom lat/lon

                                if (boolExportWpts)
                                {
                                    WptCsv(gc, wp).WriteLine(wplon + "," + wplat + ",\"" + wpname + "\",\"" + wpdesc + "\"");
                                }
                            }
                        }
                        index++;
                        if (index % 50 == 0)
                        {
                            progress.Update(strProgSubtitle, gcList.Count, index);
                        }
                    }
                }
                foreach (var kv in sd) //close csv streams
                {
                    kv.Value.Flush();
                    kv.Value.Close();
                }
                sd = null;
                /*
                 * POI Loader
                 */
                if (Core.Settings.Default.GarminPOIRunPOILoader && (Core.Settings.Default.GarminPOIPOILoaderFilename != ""))
                {
                    if (Core.Settings.Default.GarminPOIPassDirectoryToPOILoader)
                    {
                        try
                        {
                            RegistryKey rkCU = Registry.CurrentUser;
                            RegistryKey rkSettings = rkCU.OpenSubKey(@"Software\Garmin\POI Loader\Settings", RegistryKeyPermissionCheck.ReadWriteSubTree);
                            rkSettings.SetValue("Directory", Core.Settings.Default.GarminPOIExportPath);
                            rkCU.Close();
                        }
                        catch { }
                    }


                    Process poiLoader = new Process();
                    poiLoader.StartInfo.FileName = Core.Settings.Default.GarminPOIPOILoaderFilename;
                    if (Core.Settings.Default.GarminPOIRunPOILoaderSilently)
                    {
                        poiLoader.StartInfo.Arguments = "/Silent";
                    }
                    else
                    {
                        poiLoader.StartInfo.Arguments = "";
                    }
                    //poiLoader.StartInfo.Arguments = "/Directory \"" + Properties.Settings.Default.POIExportPath + "\""; // /silent is possible
                    // /Silent|/s /UsbUnitId|/u <UNITID> /Silent|/s /Directory|/d "<DIRECTORY>"
                    // HKEY_CURRENT_USER\Software\Garmin\POI Loader\Settings  Key "Directory" REG_SZ
                    poiLoader.StartInfo.UseShellExecute = true;
                    poiLoader.StartInfo.ErrorDialog = true;
                    poiLoader.Start();
                }
                //Properties.Settings.Default.RunPOILoader
                //Properties.Settings.Default.POILoaderFilename

            }

        }

        //////////////////////////////////////////////////////////////
        //Tolerate wrong Indices & lengths
        //////////////////////////////////////////////////////////////
        private string TolerantSubstring(String str, int startIndex, int length)
        {
            if (str.Length >= (startIndex + length))
            {
                return str.Substring(startIndex, length);
            }
            else
            {
                if (str.Length > startIndex)
                {
                    return str.Substring(startIndex);
                }
                else
                {
                    return String.Empty;
                }
            }
        }

        //////////////////////////////////////////////////////////////
        //remove Spaces and capitalize Chars after Space (if any)
        //////////////////////////////////////////////////////////////
        private string RemoveSpaces(String name)
        {
            string res = "";
            for (var i = 0; i < name.Length; i++)
            {
                if (name[i] != ' ')
                {
                    if ((i > 0) && (name[i - 1] == ' '))
                    {
                        //alternating Big&Small Letters HELLO THERE -> HELLOtHERE, Hello There -> HelloThere
                        if ((res.Length > 0) && char.IsLetter(res[res.Length - 1]) &&
                            (res[res.Length - 1]) == char.ToUpper(res[res.Length - 1]))
                        {
                            res = res + name.Substring(i, 1).ToLower();
                        }
                        else
                        {
                            res = res + name.Substring(i, 1).ToUpper();
                        }
                    }
                    else
                    {
                        res = res + name.Substring(i, 1);
                    }
                }
            }
            return res;
        }

        //////////////////////////////////////////////////////////////
        //remove Non Alphanumeric Chars
        //////////////////////////////////////////////////////////////
        private string RemoveNonAlnum(String name)
        {
            string res = "";
            for (var i = 0; i < name.Length; i++)
            {
                if (char.IsLetter(name[i]) || char.IsDigit(name[i]))
                {
                    //alternating Big&Small Letters HELLO THERE -> HELLOtHERE, Hello There -> HelloThere
                    if ((i > 0) && !char.IsLetter(name[i - 1]) && !char.IsDigit(name[i - 1]))
                    {
                        if ((res.Length > 0) && char.IsLetter(res[res.Length - 1]) &&
                            (res[res.Length - 1]) == char.ToUpper(res[res.Length - 1]))
                        {
                            res = res + name.Substring(i, 1).ToLower();
                        }
                        else
                        {
                            res = res + name.Substring(i, 1).ToUpper();
                        }
                    }
                    else
                    {
                        res = res + name.Substring(i, 1);
                    }
                }
            }
            return res;
        }

        //////////////////////////////////////////////////////////////
        //trim string but retain digits, try also to retain some roman
        //digits at the end i.e. Cache IV -> CaIV
        //////////////////////////////////////////////////////////////
        private string TrimButRetainDigits(String name, int maxlen, int mindigits)
        {
            string res = name;
            int cdig = 0;
            bool atEnd = true;
            //strip from end on
            for (var i = res.Length - 1; (i >= 0) && (res.Length > maxlen); i--)
            {
                if (cdig < mindigits && (char.IsDigit(res[i]) ||
                     (atEnd && (res[i] == 'I' || res[i] == 'V' || res[i] == 'X'))))
                {
                    cdig++;
                }
                else
                {
                    res = res.Remove(i, 1);
                    atEnd = false;
                }
            }
            return res;
        }

        //////////////////////////////////////////////////////////////
        //Process cache name to fit into limit
        //////////////////////////////////////////////////////////////
        private string ProcessName(String name, int maxlen)
        {
            if (string.IsNullOrEmpty(name))
            {
                return "-";
            }

            name = name.Replace("\"", "'"); //remove double quotes for csv
            if (name.Length > maxlen)
            {
                name = RemoveSpaces(name);
            }
            if (name.Length > maxlen)
            {
                name = RemoveNonAlnum(name);
            }
            if (name.Length > maxlen)
            {
                name = TrimButRetainDigits(name, maxlen, 6);
            }

            return name;
        }

        //////////////////////////////////////////////////////////////
        //Process cache description to fit into limit
        //////////////////////////////////////////////////////////////
        private string ProcessDescription(String desc, int maxlen)
        {
            if (string.IsNullOrEmpty(desc))
            {
                return "";
            }

            desc = desc.Replace("\"", "'"); //remove double quotes for csv
            desc = desc.Replace("\n", " "); //remove crlf
            desc = desc.Replace("\r", " ");
            desc = desc.Replace("\t", " ");
            desc = desc.Replace("  ", " ").Trim(); //remove double spaces
            if (desc.Length > maxlen)
            {
                desc = RemoveSpaces(desc);
            }

            //truncating
            maxlen = 2 * maxlen; //max length is usually just a rough estimate, let the description be a bit longer (but less then poi loader limit)
            if (maxlen > 1023) { maxlen = 1023; }
            if (desc.Length > maxlen)
            {
                desc = desc.Remove(maxlen, desc.Length - maxlen);
            }

            return desc;
        }

        //////////////////////////////////////////////////////////////
        //defines the filename based on gc attribute & opens the file if new
        //////////////////////////////////////////////////////////////
        private System.IO.StreamWriter CacheCsv(Core.Data.Geocache gc)
        {
            string csvName = null;
            string postFix = "";

            switch (gc.GeocacheType.ID)
            {
                case 2:
                    csvName = "Traditional";
                    if (gc.Container.ID == 2)
                    {
                        csvName = "Micro"; //ShowMicroTradis
                    }
                    break;
                case 3:
                    csvName = "Multi";
                    if (gc.CustomLat != null && gc.CustomLon != null)
                    {
                        postFix = "_Solved";
                    }
                    break;
                case 4:
                    csvName = "Virtual"; break;
                case 5:
                    csvName = "Letterbox"; break;
                case 6:
                    csvName = "Event"; break;
                case 8:
                    csvName = "Mystery";
                    if (gc.CustomLat != null && gc.CustomLon != null)
                    {
                        postFix = "_Solved";
                    }
                    break;//
                case 9:
                    csvName = "Project_APE"; break;
                case 11:
                    csvName = "Webcam"; break;
                case 12:
                    csvName = "Locationless"; break;
                case 13:
                    csvName = "CITO"; break;
                case 27:
                    csvName = "Benchmark"; break; //hopefully
                case 137:
                    csvName = "Earth"; break;
                case 453:
                    csvName = "Mega_Event"; break;
                case 605:
                    csvName = "Course"; break; //Geocache Course
                case 1304:
                    csvName = "Maze"; break; //GPS Adventures Exhibit
                case 1858:
                    csvName = "Whereigo";
                    if (gc.CustomLat != null && gc.CustomLon != null)
                    {
                        postFix = "_Solved";
                    }
                    break;
                case 3653:
                    csvName = "Lost_and_Found_Event"; break;
                case 3773:
                    csvName = "Groundspeak_HQ"; break;
                case 3774:
                    csvName = "Groundspeak_Lost_and_Found"; break;
                case 4738:
                    csvName = "Groundspeak_Block_Party"; break;
                case 95342:
                    csvName = "Munzee"; break;
                case 95343:
                    csvName = "Munzee_Virtual"; break;
                case 95344:
                    csvName = "Munzee_Maintenance"; break;
                case 95345:
                    csvName = "Munzee_Business"; break;
                case 95346:
                    csvName = "Munzee_Mystery"; break;
                case 95347:
                    csvName = "Munzee_NFC"; break;
                case 95348:
                    csvName = "Munzee_Premium"; break;
                default:
                    csvName = gc.GeocacheType.Name.Replace(" ", "_"); break;
            }
            if (gc.Found)
            {
                csvName = "Found";
                if (gc.CustomLat != null && gc.CustomLon != null)
                {
                    postFix = "_Solved";
                }
            }
            if (gc.IsOwn)
            {
                csvName = "Own";
                postFix = ""; //only allowed: disabled
            }
            if (!gc.Available || gc.Archived)
            {
                postFix = "_Disabled";//overwrites "_Solved" if any
            }
            csvName = csvName + postFix;

            if (!sd.ContainsKey(csvName))
            { //new -> open File
                System.IO.StreamWriter cs = null;
                //
                string csvPath = Core.Settings.Default.GarminPOIExportPath + Path.DirectorySeparatorChar + csvName + ".csv";
                cs = new System.IO.StreamWriter(csvPath, false,
                           Encoding.GetEncoding(1250), 32768);
                //1250 = Write as ANSI
                sd.Add(csvName, cs);
                //create gc icon
                try
                {
                    //using (Stream input = Assembly.GetExecutingAssembly().GetManifestResourceStream("GlobalcachingApplication.Plugins.ExportGarminPOI.Resources." + csvName + ".bmp"))
                    //using (Stream output = File.Create(Core.Settings.Default.GarminPOIExportPath + Path.DirectorySeparatorChar + csvName + ".bmp"))
                    //{
                    //    input.CopyTo(output);
                    //}
                    Utils.ResourceHelper.SaveToFile("/GarminPOI/Resources/" + csvName + ".bmp", Core.Settings.Default.GarminPOIExportPath + Path.DirectorySeparatorChar + csvName + ".bmp", true);
                }
                catch { } //ignore invalid resource or write errors here

            }
            return sd[csvName];
        }

        //////////////////////////////////////////////////////////////
        //defines the filename based on wpt attribute & opens the file
        // if new
        //////////////////////////////////////////////////////////////
        private System.IO.StreamWriter WptCsv(Core.Data.Geocache gc,
                                                       Core.Data.Waypoint wpt)
        {
            string csvName = null;
            string postFix = "";

            switch (wpt.WPType.ID)
            {
                case 217:
                    csvName = "Parking_Area"; break;
                case 218:
                    csvName = "Question_to_Answer"; break;
                case 219:
                    csvName = "Stages_of_a_Multicache"; break;
                case 220:
                    csvName = "Final_Location"; break;
                case 221:
                    csvName = "Trailhead"; break;
                case 452:
                    csvName = "Reference_Point"; break;
                default:
                    csvName = wpt.WPType.Name.Replace(" ", "_"); break;
            }
            if (gc.Found) { postFix = "_Found"; }
            if (!gc.Available || gc.Archived) { postFix = "_Disabled"; }
            csvName = csvName + postFix;

            if (!sd.ContainsKey(csvName))
            { //new -> open File
                System.IO.StreamWriter cs = null;

                string csvPath = Core.Settings.Default.GarminPOIExportPath + Path.DirectorySeparatorChar + csvName + ".csv";
                cs = new System.IO.StreamWriter(csvPath, false,
                           Encoding.GetEncoding(1250), 32768);
                sd.Add(csvName, cs);
                //create wpt icon
                try
                {
                    //using (Stream input = Assembly.GetExecutingAssembly().GetManifestResourceStream("GlobalcachingApplication.Plugins.ExportGarminPOI.Resources." + csvName + ".bmp"))
                    //using (Stream output = File.Create(Properties.Settings.Default.POIExportPath + Path.DirectorySeparatorChar + csvName + ".bmp"))
                    //{
                    //    input.CopyTo(output);
                    //}
                    Utils.ResourceHelper.SaveToFile("/GarminPOI/Resources/" + csvName + ".bmp", Core.Settings.Default.GarminPOIExportPath + Path.DirectorySeparatorChar + csvName + ".bmp", true);
                }
                catch { } //ignore invalid resource or write errors here
            }
            return sd[csvName];
        }

        //////////////////////////////////////////////////////////////
        // Converts double to decimal degrees used in csv
        //////////////////////////////////////////////////////////////
        private string FmtCsvLatLon(double latlon)
        {
            return latlon.ToString("0.000000").Replace(",", ".");//replace for eventual german number format (n,nn)
        }

    }
}
