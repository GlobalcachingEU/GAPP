/*
  Export Caches in CSV-Format for Garmin POI Loader
  
  Version 2
  Author: Contact FamilienCacher at Globalcaching.eu
  
  This script exports selected caches in a format suited for the Garmin POI Loader.
  In that way you can display a big number of geocaches with individual! icons even on older
  garmin devices. (i.e. 15000 Caches+Wpts is usable on a Garmin Etrex Vista Hcx)
  
  How To Use:
  #1: Create a folder where the CSV-Files+Icons should be placed
  #2: Select the desired Caches in GAPP
  #3: Execute this Script
  #4: Start Garmin POI Loader, locate the Folder and transfer the POIs on your Garmin.
  
  Notes:
  The CSV-Folder is not emptied before export. Only selected cachetypes are overwritten, so
  cachetypes from previous exports may remain (if not deleted manually).

  The Description contains cache details in short form:
    TrD15T2Sm is: "Traditional Difficulty:1,5 Terrain:2 Small"


  Limits:
  Depending on your device you can edit the limits (INT_...) defined below.
  The script tries to put as much info in the fields as possible (i.e.) if the limits are reached.
  Some "intelligent Text compression" is used for that.
  
  Naturally you may customize this script to your desires to use different fields in name & description  
  
  Icon Types:
  Found (Smiley), Cachetypes + Events, Micros (Traditional with size "Micro")
  All of above for Disabled/Archived caches
  Solved Mysteries,Multis & WhereIGOs (custom coordinates)
  Waypoints

*/

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Framework=GlobalcachingApplication.Framework;
using GlobalcachingApplication.Framework.Data;
using GlobalcachingApplication.Framework.Interfaces;
using Utils=GlobalcachingApplication.Utils;
using GlobalcachingApplication.Utils.BasePlugin;

class Script
{
    public const int INT_MAXNAMELEN = 22;  //Max. Length of POI Name (Limit on Etrex Vista Hcx: 40)
    public const int INT_MINNAMEDIGITS = 6;//Retain at least this amount of digits in name (<Maxnamelen)
    public const int INT_MAXDESCLEN = 84;  //Max. Length of Description (Limit on Etrex Vista Hcx 84)
 
 
    public const string STR_EXPORTINGCSV = "Exporting CSV";
    public const string STR_CREATINGFILE = "Creating File..";
 

    private static string csvFile = null;
    private static Dictionary<string, System.IO.StreamWriter> sd = null;
 
    //////////////////////////////////////////////////////////////
    //remove Spaces and capitalize Chars after Space (if any)
    //////////////////////////////////////////////////////////////
    private static string RemoveSpaces(String name)
    {
     string res="";
     for (var i=0; i<name.Length; i++) {       
       if (name[i] != ' ') {
          if ((i>0) && (name[i-1] == ' ')) {
           //alternating Big&Small Letters HELLO THERE -> HELLOtHERE, Hello There -> HelloThere
            if ((res.Length>0) && char.IsLetter(res[res.Length-1]) && 
                (res[res.Length-1])==char.ToUpper(res[res.Length-1])) {
              res=res+name.Substring(i,1).ToLower(); 
            }
            else {
              res=res+name.Substring(i,1).ToUpper(); 
            }                      
          } else {
            res=res+name.Substring(i,1);
          }
       }
     }
     return res;
    }
    
    //////////////////////////////////////////////////////////////
    //remove Non Alphanumeric Chars
    //////////////////////////////////////////////////////////////
    private static string RemoveNonAlnum(String name)
    {
     string res="";
     for (var i=0; i<name.Length; i++) {       
       if (char.IsLetter(name[i]) || char.IsDigit(name[i])) {
           //alternating Big&Small Letters HELLO THERE -> HELLOtHERE, Hello There -> HelloThere
          if ((i>0) && !char.IsLetter(name[i-1]) && !char.IsDigit(name[i-1])) {
            if ((res.Length>0) && char.IsLetter(res[res.Length-1]) && 
                (res[res.Length-1])==char.ToUpper(res[res.Length-1])) {
              res=res+name.Substring(i,1).ToLower(); 
            }
            else {
              res=res+name.Substring(i,1).ToUpper(); 
            }            
          } else {
            res=res+name.Substring(i,1);
          }
       }
     }
     return res;
    }

    //////////////////////////////////////////////////////////////
    //trim string but retain digits, try also to retain some roman
    //digits at the end i.e. Cache IV -> CaIV
    //////////////////////////////////////////////////////////////
    private static string TrimButRetainDigits(String name, int maxlen, int mindigits)
    {
     string res=name;
     int cdig=0;
     bool atEnd=true;
     //strip from end on
     for (var i=res.Length-1; (i>=0) && (res.Length>maxlen); i--) {
       if (cdig<mindigits && (char.IsDigit(res[i]) || 
            (atEnd && (res[i]=='I' || res[i]=='V' || res[i]=='X'))) ) {
           cdig++;
         }
       else {
         res=res.Remove(i,1);
         atEnd=false;
       }
     }
     return res;
    }
    
    //////////////////////////////////////////////////////////////
    //Process cache name to fit into limit
    //////////////////////////////////////////////////////////////
    private static string ProcessName(String name, int maxlen)
    {
     if (string.IsNullOrEmpty(name)) {
      return "-";
     }
     
     name=name.Replace("\"","'"); //remove double quotes for csv
     if (name.Length>maxlen) {
       name=RemoveSpaces(name);     
     }
     if (name.Length>maxlen) {
       name=RemoveNonAlnum(name);
     }     
     if (name.Length>maxlen) {
       name=TrimButRetainDigits(name,maxlen,INT_MINNAMEDIGITS);
     }
     
     return name;
    }
    
    //////////////////////////////////////////////////////////////
    //Process cache description to fit into limit
    //////////////////////////////////////////////////////////////
    private static string ProcessDescription(String desc, int maxlen)
    {
     if (string.IsNullOrEmpty(desc)) {
      return "";
     }    
     
     desc=desc.Replace("\"","'"); //remove double quotes for csv
     desc=desc.Replace("\n"," "); //remove crlf
     desc=desc.Replace("\r"," ");
     desc=desc.Replace("\t"," ");
     desc=desc.Replace("  "," ").Trim(); //remove double spaces
     if (desc.Length>maxlen) {
       desc=RemoveSpaces(desc);     
     }
     if (desc.Length>maxlen) {
      desc=desc.Remove(maxlen,desc.Length-maxlen);
     }
     
     return desc;
    }

    //////////////////////////////////////////////////////////////
    //defines the filename based on gc attribute & opens the file if new
    //////////////////////////////////////////////////////////////
    private static System.IO.StreamWriter CacheCsv(Framework.Data.Geocache gc) {
      string csvName=null;
      string postFix="";
      
      switch (gc.GeocacheType.ID)
      {
       case 2: 
        csvName="Traditional";
        if (gc.Container.ID==2) {
         csvName="Micro"; //ShowMicroTradis
        }
        break;
       case 3: 
        csvName="Multi";
        if (gc.CustomCoords || (gc.CustomLat != null && gc.CustomLon != null)) {
         postFix="_Solved";
        }
        break;
       case 4: 
        csvName="Virtual";break;
       case 5: 
        csvName="Letterbox";break;
       case 6: 
        csvName="Event";break;
       case 8: 
        csvName="Mystery";
        if (gc.CustomCoords || (gc.CustomLat != null && gc.CustomLon != null)) {
         postFix="_Solved";
        }
        break;//
       case 9: 
        csvName="Project_APE";break;
       case 11: 
        csvName="Webcam";break;
       case 12: 
        csvName="Locationless";break;
       case 13: 
        csvName="CITO";break;
       case 27: 
        csvName="Benchmark";break; //hopefully
       case 137: 
        csvName="Earth";break;
       case 453: 
        csvName="Mega_Event";break;
       case 605: 
        csvName="Course";break; //Geocache Course
       case 1304: 
        csvName="Maze";break; //GPS Adventures Exhibit
       case 1858: 
        csvName="Whereigo";
        if (gc.CustomCoords || (gc.CustomLat != null && gc.CustomLon != null)) {
         postFix="_Solved";
        }
        break;
       case 3653: 
        csvName="Lost_and_Found_Event";break;
       case 3773: 
        csvName="Groundspeak_HQ";break;
       case 3774: 
        csvName="Groundspeak_Lost_and_Found";break;
       case 4738: 
        csvName="Groundspeak_Block_Party";break;
       default:
        csvName=gc.GeocacheType.Name.Replace(" ","_");break;
      }
      if (gc.Found)
      {
       csvName="Found";
       if (gc.CustomCoords || (gc.CustomLat != null && gc.CustomLon != null)) {
        postFix="_Solved";
       }
      }
      if (!gc.Available || gc.Archived) {
       postFix="_Disabled";//overwrites "_Solved" if any
      }
      csvName=csvName+postFix;
     
      if (!sd.ContainsKey(csvName)) { //new -> open File
        System.IO.StreamWriter cs=null;
        //
        string csvPath=Path.GetDirectoryName(csvFile)+Path.DirectorySeparatorChar+csvName+".csv";
        cs=new System.IO.StreamWriter(csvPath, false, 
                   Encoding.GetEncoding(1250));
                             //1250 = Write as ANSI
        sd.Add(csvName,cs);
        //create gc icon
        byte[] binaryData;
        binaryData = System.Convert.FromBase64String(GetBmpData(csvName));
        System.IO.FileStream bmpFile = new System.IO.FileStream(
                 Path.GetDirectoryName(csvFile)+Path.DirectorySeparatorChar+csvName+".bmp",
                 System.IO.FileMode.Create,System.IO.FileAccess.Write);
        bmpFile.Write(binaryData, 0, binaryData.Length);
        bmpFile.Close();        
       
       
      }
      return sd[csvName];
    }

    //////////////////////////////////////////////////////////////
    //defines the filename based on wpt attribute & opens the file
    // if new
    //////////////////////////////////////////////////////////////
    private static System.IO.StreamWriter WptCsv(Framework.Data.Geocache gc,
                                                   Framework.Data.Waypoint wpt) {
      string csvName=null;
      string postFix="";
      
      switch (wpt.WPType.ID)
      {
       case 217: 
        csvName="Parking_Area";break;
       case 218: 
        csvName="Question_to_Answer";break;
       case 219: 
        csvName="Stages_of_a_Multicache";break;
       case 220: 
        csvName="Final_Location";break;
       case 221: 
        csvName="Trailhead";break;
       case 452: 
        csvName="Reference_Point";break;
       default:
        csvName=wpt.WPType.Name.Replace(" ","_");break;
      }
      if (gc.Found) {postFix="_Found";}
      if (!gc.Available || gc.Archived) {postFix="_Disabled";}
      csvName=csvName+postFix;
      
      if (!sd.ContainsKey(csvName)) { //new -> open File
        System.IO.StreamWriter cs=null;

        string csvPath=Path.GetDirectoryName(csvFile)+Path.DirectorySeparatorChar+csvName+".csv";
        cs=new System.IO.StreamWriter(csvPath, false, 
                   Encoding.GetEncoding(1250));
        sd.Add(csvName,cs);
        //create wpt icon
        byte[] binaryData;
        binaryData = System.Convert.FromBase64String(GetBmpData(csvName));
        System.IO.FileStream bmpFile = new System.IO.FileStream(
                 Path.GetDirectoryName(csvFile)+Path.DirectorySeparatorChar+csvName+".bmp",
                 System.IO.FileMode.Create,System.IO.FileAccess.Write);
        bmpFile.Write(binaryData, 0, binaryData.Length);
        bmpFile.Close();        
      }
      return sd[csvName];
    }
    
    //////////////////////////////////////////////////////////////
    // Converts double to decimal degrees used in csv
    //////////////////////////////////////////////////////////////
    private static string FmtCsvLatLon(double latlon)
    {
      return latlon.ToString("0.000000").Replace(",",".");//replace for eventual german number format (n,nn)
    }
    
    //********************************************************************************************
    //
    // Main 
    //
    //********************************************************************************************
    public static bool Run(Plugin plugin, ICore core)
    {
     List<Framework.Data.Geocache> gcList = null;
     List<Framework.Data.Waypoint> wpList = null;

     gcList = Utils.DataAccess.GetSelectedGeocaches(core.Geocaches);
     if (gcList.Count==0) {
       System.Windows.Forms.MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation("no geocache selected for export"));
       return true;
     }
     
     System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog();
        
     dlg.FileName = "Traditional.csv";
     //dlg.OverwritePrompt=false;
     dlg.Filter = "*.csv|*.csv";
    
     //Select File
     if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK) 
     {
       
        csvFile = dlg.FileName; 

        using (Utils.ProgressBlock progress = new Utils.ProgressBlock(plugin,
            STR_EXPORTINGCSV, STR_CREATINGFILE, gcList.Count, 0))
        {
            sd = new Dictionary<string, System.IO.StreamWriter>();
            { 
             int block = 0;
             int index = 0;
             //todo: clear all csv-files in path?
             
             //Iterate selected caches
             foreach (Framework.Data.Geocache gc in gcList)
             {
              //cache name made up of Name (others might prefer gc.Code)
              String name = ProcessName(gc.Name,INT_MAXNAMELEN);
              
              //1st part of description: GC-Code + basic cache info
              String desc = gc.Code + " "+
                     gc.GeocacheType.Name.Substring(0,2)+
                     "D" + gc.Difficulty.ToString("0.#").Replace(",",".").Replace(".5","5") + 
                     "T"+ gc.Terrain.ToString("0.#").Replace(",",".").Replace(".5","5") + 
                     gc.Container.Name.Substring(0,2)+" ";
              
              //2nd part of description, put hints, notes, personalnote as limits allow
              desc = desc + ProcessDescription(gc.EncodedHints+" "+
                                               gc.Notes+" "+
                                               gc.PersonaleNote,INT_MAXDESCLEN-desc.Length);
                                         //"PersonaleNote" is from the docs
              String lon=FmtCsvLatLon(gc.Lon);
              String lat=FmtCsvLatLon(gc.Lat);
              //use custom lat/lon if defined
              if (gc.CustomLat != null && gc.CustomLon != null) {
                lon=FmtCsvLatLon((double)gc.CustomLon);//double? -> double
                lat=FmtCsvLatLon((double)gc.CustomLat);
              }
              //write to CSV (file selected by gc type)
              CacheCsv(gc).WriteLine(lon + "," + lat + ",\"" + name + "\",\"" + desc + "\"");
              
              //Iterate all waypoints of cache
              wpList = Utils.DataAccess.GetWaypointsFromGeocache(core.Waypoints,gc.Code);
              foreach (Framework.Data.Waypoint wp in wpList)
              { //only export waypoints with coords
                if (wp.Lat != null && wp.Lon != null) {
                  //Make Wpt name of Cachename + # + "wpt-ID 2-letter prefix"
                  //others may prefer simply wp.Code
                  String wpname = ProcessName(gc.Name,INT_MAXNAMELEN-3)+"#"+wp.Code.Substring(0,2);
              
                  String wpdesc = wp.Code + " ";
                  //depending on source the "real" name of the wpt sometimes is either in "Name" or "Description"
                  String wpd= wp.Name;
                  if (wpd == wp.Code) {//use description if name only holds the wpcode
                    wpd=wp.Description;
                  }
                  //POI description = name + comment
                  wpdesc = wpdesc + ProcessDescription(wpd+": "+
                                                   wp.Comment,INT_MAXDESCLEN-wpdesc.Length);
                  String wplon=FmtCsvLatLon((double)wp.Lon);
                  String wplat=FmtCsvLatLon((double)wp.Lat);
                  //wpts have no custom lat/lon
              
                  WptCsv(gc,wp).WriteLine(wplon + "," + wplat + ",\"" + wpname + "\",\"" + wpdesc + "\"");
                }
              }              
              block++;
              index++;
              if (block > 5)
              {
               block = 0;               
               progress.UpdateProgress(STR_EXPORTINGCSV, STR_CREATINGFILE, gcList.Count, index);
              }
             }
            }
            foreach (var kv in sd) //close csv streams
            {
              kv.Value.Close();
            }
            sd = null;
        }
     }

     return true;
    }

//=======================================================================================================
// Icons in uuencoded form
//=======================================================================================================
    private static string GetBmpData(String BmpName)
    { string bmpdata=null;
     
      switch (BmpName)
      {
  case "CITO":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAAAAAA
  ACAAAABBAAAAagAAAIsAAAC0AAAA1QAAAP8AAAAAMAAAIDAAAEEwAABqMAAAizAAALQwAADVMAAA
  /zAAAABlAAAgZQAAQWUAAGplAACLZQAAtGUAANVlAAD/ZQAAAJUAACCVAABBlQAAapUAAIuVAAC0
  lQAA1ZUAAP+VAAAAygAAIMoAAEHKAABqygAAi8oAALTKAADVygAA/8oAAAD/AAAg/wAAQf8AAGr/
  AACL/wAAtP8AANX/AAD//wAAAAA5ACAAOQBBADkAagA5AIsAOQC0ADkA1QA5AP8AOQAAMDkAIDA5
  AEEwOQBqMDkAizA5ALQwOQDVMDkA/zA5AABlOQAgZTkAQWU5AGplOQCLZTkAtGU5ANVlOQD/ZTkA
  AJU5ACCVOQBBlTkAapU5AIuVOQC0lTkA1ZU5AP+VOQAAyjkAIMo5AEHKOQBqyjkAi8o5ALTKOQDV
  yjkA/8o5AAD/OQAg/zkAQf85AGr/OQCL/zkAtP85ANX/OQD//zkAAAB7ACAAewBBAHsAagB7AIsA
  ewC0AHsA1QB7AP8AewAAMHsAIDB7AEEwewBqMHsAizB7ALQwewDVMHsA/zB7AABlewAgZXsAQWV7
  AGplewCLZXsAtGV7ANVlewD/ZXsAAJV7ACCVewBBlXsAapV7AIuVewC0lXsA1ZV7AP+VewAAynsA
  IMp7AEHKewBqynsAi8p7ALTKewDVynsA/8p7AAD/ewAg/3sAQf97AGr/ewCL/3sAtP97ANX/ewD/
  /3sAAAC9ACAAvQBBAL0AagC9AIsAvQC0AL0A1QC9AP8AvQAAML0AIDC9AEEwvQBqML0AizC9ALQw
  vQDVML0A/zC9AABlvQAgZb0AQWW9AGplvQCLZb0AtGW9ANVlvQD/Zb0AAJW9ACCVvQBBlb0AapW9
  AIuVvQC0lb0A1ZW9AP+VvQAAyr0AIMq9AEHKvQBqyr0Ai8q9ALTKvQDVyr0A/8q9AAD/vQAg/70A
  Qf+9AGr/vQCL/70AtP+9ANX/vQD//70AAAD/ACAA/wBBAP8AagD/AIsA/wC0AP8A1QD/AP8A/wAA
  MP8AIDD/AEEw/wBqMP8AizD/ALQw/wDVMP8A/zD/AABl/wAgZf8AQWX/AGpl/wCLZf8AtGX/ANVl
  /wD/Zf8AAJX/ACCV/wBBlf8AapX/AIuV/wC0lf8A1ZX/AP+V/wAAyv8AIMr/AEHK/wBqyv8Ai8r/
  ALTK/wDVyv8A/8r/AAD//wAg//8AQf//AGr//wCL//8AtP//ANX//wD///8A////AObm5gDV1dUA
  xcXFALS0tACkpKQAlJSUAIODgwBzc3MAYmJiAFJSUgBBQUEAMTExACAgIAAQEBAAAAAAAMfHx8fy
  tra2tra28sfHx8fHx8e28e/v7+/v7/G2x8fHx8d98u+2TUREfbbv733Hx8e2tn1MGxsTDAwMfbZ9
  9cfyfX1EUxoaGhMMDAxNfX3y9PRNGoVUGlMbDAwMDH199H31TBpThYWEGwwMDAxEfbZ9fUsahPGE
  GhMMFAwMRH19fX1LGoSEhBt9TX19fUR9fX19RBoaGhoTRH0MFEREfX318n0aGhoaEwxFFPQLfXW2
  8n19TBoaGhsMFH0URH198sf0fX1MGhoaGxMTTfR9fcfHx33x77ZMS0t8tu/vfcfHx8fHtvHv7+/v
  7+/xtvHxx8fHx8fytra2tra28u/x8cc=
     ";break;
  case "CITO_Disabled":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAA/wD/
  AAAA/wDVyr0A////AObm5gCLlTkAapU5AIuVewC0lXsAtLS0AEGVAABqlQAAamUAALSVOQCLMAAA
  i2UAAItlOQDV1dUAtGU5AGowAACLynsApKSkAGrKOQC0ynsAi8o5AAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABAQEB
  AQEBAQEBAQEBAQABAQABBAEDAQMBAwECAQEBAQAIEQMCDRAQCAIDAQEBAQEBAggFCwsMDg4OAQEB
  FQEBCAgQFgoKCgwOAQEBCAEBAQENChcYChYLAQEBDggIAQEVBQoWFxcUAQEBDg4QAQEBAQYKFAQU
  AQEBDw4OEAgBAQgGChQUAQEBDQgICBABAQEBEAoKAQEBEAgODxAQCAEBEQgKAQEBDA4SDwkTCAEB
  AQEIAQEBCgsODwgPEAgIAQEJAQEBCgoKCwwMDQkIAQEBAQEBAwIFBgYHAgMDCAABAQEBAgEDAQMB
  AwEEAQQBAQABAQEBAQEBAQEBAQEBAQA=
     ";break;
  case "Earth":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAAAAAA
  ACAAAABBAAAAagAAAIsAAAC0AAAA1QAAAP8AAAAAMAAAIDAAAEEwAABqMAAAizAAALQwAADVMAAA
  /zAAAABlAAAgZQAAQWUAAGplAACLZQAAtGUAANVlAAD/ZQAAAJUAACCVAABBlQAAapUAAIuVAAC0
  lQAA1ZUAAP+VAAAAygAAIMoAAEHKAABqygAAi8oAALTKAADVygAA/8oAAAD/AAAg/wAAQf8AAGr/
  AACL/wAAtP8AANX/AAD//wAAAAA5ACAAOQBBADkAagA5AIsAOQC0ADkA1QA5AP8AOQAAMDkAIDA5
  AEEwOQBqMDkAizA5ALQwOQDVMDkA/zA5AABlOQAgZTkAQWU5AGplOQCLZTkAtGU5ANVlOQD/ZTkA
  AJU5ACCVOQBBlTkAapU5AIuVOQC0lTkA1ZU5AP+VOQAAyjkAIMo5AEHKOQBqyjkAi8o5ALTKOQDV
  yjkA/8o5AAD/OQAg/zkAQf85AGr/OQCL/zkAtP85ANX/OQD//zkAAAB7ACAAewBBAHsAagB7AIsA
  ewC0AHsA1QB7AP8AewAAMHsAIDB7AEEwewBqMHsAizB7ALQwewDVMHsA/zB7AABlewAgZXsAQWV7
  AGplewCLZXsAtGV7ANVlewD/ZXsAAJV7ACCVewBBlXsAapV7AIuVewC0lXsA1ZV7AP+VewAAynsA
  IMp7AEHKewBqynsAi8p7ALTKewDVynsA/8p7AAD/ewAg/3sAQf97AGr/ewCL/3sAtP97ANX/ewD/
  /3sAAAC9ACAAvQBBAL0AagC9AIsAvQC0AL0A1QC9AP8AvQAAML0AIDC9AEEwvQBqML0AizC9ALQw
  vQDVML0A/zC9AABlvQAgZb0AQWW9AGplvQCLZb0AtGW9ANVlvQD/Zb0AAJW9ACCVvQBBlb0AapW9
  AIuVvQC0lb0A1ZW9AP+VvQAAyr0AIMq9AEHKvQBqyr0Ai8q9ALTKvQDVyr0A/8q9AAD/vQAg/70A
  Qf+9AGr/vQCL/70AtP+9ANX/vQD//70AAAD/ACAA/wBBAP8AagD/AIsA/wC0AP8A1QD/AP8A/wAA
  MP8AIDD/AEEw/wBqMP8AizD/ALQw/wDVMP8A/zD/AABl/wAgZf8AQWX/AGpl/wCLZf8AtGX/ANVl
  /wD/Zf8AAJX/ACCV/wBBlf8AapX/AIuV/wC0lf8A1ZX/AP+V/wAAyv8AIMr/AEHK/wBqyv8Ai8r/
  ALTK/wDVyv8A/8r/AAD//wAg//8AQf//AGr//wCL//8AtP//ANX//wD///8A////AObm5gDV1dUA
  xcXFALS0tACkpKQAlJSUAIODgwBzc3MAYmJiAFJSUgBBQUEAMTExACAgIAAQEBAAAAAAAMfHx8fH
  x8fHx8fHx8fHx8fHx8fH8/n5+EM5+PLHx8fHx8fv+Pi2tra2QUD99sfHx8fH+X22tra2tkFAQAj1
  x8fH9Hy2tra2trZCQEBB/fHHx/m2tra2tra2hXuFtkL3x8f5tra2tvtycXFy+fhL+8fHfLa2trb7
  ceLi4eKyOfzHx/yGtra2cqr49/hxcar6x8cIS7a2tnLk9+/v4eHjcsfH+kFCQob54/fv5ODi4vfH
  x/QIQEB8Q+Jz5ODi43Hxx8fH+AhAfH2qcOHi43r2x8fHx8f4CIW2cnHj4nL2x8fHx8fHx/L4+fw5
  cvjxx8fHx8fHx8fHx8fHx8fHx8fHx8c=
     ";break;
  case "Earth_Disabled":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAA/wD/
  AAAA/wBzc3MAMTExAEFlewDm5uYAADAAALTKewDVyr0AIGV7AGrK/wBByv8AlJSUAIuVewC0lXsA
  QZW9AABlewAgyv8AQZV7AGplOQBqZXsAi8r/AADK/wBSUlIAIGU5AEFlOQCDg4MA////AGqVOQDV
  ynsAQUFBAEHKvQAgMDkAYmJiALS0tAAgICAAAGU5AMXFxQAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABAQEB
  AQEBAQEBAQEBAQABAQABJQEhARMBAgEAAQEBAQAbAgIICAgIGCQjAQEBAQEBIQ4ICAgICBgkAQEB
  AAEBIg0ICAgICAgZAQEBIwEBAQEICAgICAgIAQEBCBkaAQEhCAgICB4EAQEBIQIcAQEBAQgICAge
  AQEBEQsfIAMBAQMdCAgIAQEBGgIJCQ8BAQEBHAgIAQEBGhsbEREKBAEBFxgZAQEBChobFRYLCwEB
  AQEGAQEBEwsUFRYLCgkFAQEAAQEBDQ4PEBELChIMAQEBAQEBBgcIBAkKCwQMAAABAQEBAAECAQMB
  BAEFAQABAQABAQEBAQEBAQEBAQEBAQA=
     ";break;
  case "Event":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAAAAAA
  ACAAAABBAAAAagAAAIsAAAC0AAAA1QAAAP8AAAAAMAAAIDAAAEEwAABqMAAAizAAALQwAADVMAAA
  /zAAAABlAAAgZQAAQWUAAGplAACLZQAAtGUAANVlAAD/ZQAAAJUAACCVAABBlQAAapUAAIuVAAC0
  lQAA1ZUAAP+VAAAAygAAIMoAAEHKAABqygAAi8oAALTKAADVygAA/8oAAAD/AAAg/wAAQf8AAGr/
  AACL/wAAtP8AANX/AAD//wAAAAA5ACAAOQBBADkAagA5AIsAOQC0ADkA1QA5AP8AOQAAMDkAIDA5
  AEEwOQBqMDkAizA5ALQwOQDVMDkA/zA5AABlOQAgZTkAQWU5AGplOQCLZTkAtGU5ANVlOQD/ZTkA
  AJU5ACCVOQBBlTkAapU5AIuVOQC0lTkA1ZU5AP+VOQAAyjkAIMo5AEHKOQBqyjkAi8o5ALTKOQDV
  yjkA/8o5AAD/OQAg/zkAQf85AGr/OQCL/zkAtP85ANX/OQD//zkAAAB7ACAAewBBAHsAagB7AIsA
  ewC0AHsA1QB7AP8AewAAMHsAIDB7AEEwewBqMHsAizB7ALQwewDVMHsA/zB7AABlewAgZXsAQWV7
  AGplewCLZXsAtGV7ANVlewD/ZXsAAJV7ACCVewBBlXsAapV7AIuVewC0lXsA1ZV7AP+VewAAynsA
  IMp7AEHKewBqynsAi8p7ALTKewDVynsA/8p7AAD/ewAg/3sAQf97AGr/ewCL/3sAtP97ANX/ewD/
  /3sAAAC9ACAAvQBBAL0AagC9AIsAvQC0AL0A1QC9AP8AvQAAML0AIDC9AEEwvQBqML0AizC9ALQw
  vQDVML0A/zC9AABlvQAgZb0AQWW9AGplvQCLZb0AtGW9ANVlvQD/Zb0AAJW9ACCVvQBBlb0AapW9
  AIuVvQC0lb0A1ZW9AP+VvQAAyr0AIMq9AEHKvQBqyr0Ai8q9ALTKvQDVyr0A/8q9AAD/vQAg/70A
  Qf+9AGr/vQCL/70AtP+9ANX/vQD//70AAAD/ACAA/wBBAP8AagD/AIsA/wC0AP8A1QD/AP8A/wAA
  MP8AIDD/AEEw/wBqMP8AizD/ALQw/wDVMP8A/zD/AABl/wAgZf8AQWX/AGpl/wCLZf8AtGX/ANVl
  /wD/Zf8AAJX/ACCV/wBBlf8AapX/AIuV/wC0lf8A1ZX/AP+V/wAAyv8AIMr/AEHK/wBqyv8Ai8r/
  ALTK/wDVyv8A/8r/AAD//wAg//8AQf//AGr//wCL//8AtP//ANX//wD///8A////AObm5gDV1dUA
  xcXFALS0tACkpKQAlJSUAIODgwBzc3MAYmJiAFJSUgBBQUEAMTExACAgIAAQEBAAAAAAAMfHx8fH
  x8fHx8fHx8fHx8fHx8fHx8fHx8fH9PHHx8fHx8fHx8fHx8fHx/r478fHx8fHx8fH7/L09PP5+PLH
  x8fHx+/0+Pj29fX29PL578fHx/H59vLx8fHx8fHx9Pnyx+/58/Hx7+/v7+/x8fHy+fH3s/Lx7+/v
  7+/v7/Hx8fL4+ezt7+/v8aSjo6Oj8fHx+XLs7u/v7+/x5ubm8e/x8ff57O3v86ysrKysrKzx8fH5
  9avs7vHz8/Pz8/Pz8fH09+/4q+zt7+/v7+/v7ey0+e/H7/Zys+zs7e3s7LNy+O/Hx8fv8fb5cnp6
  cvn38u/Hx8fHx8fHx8fHx8fHx8fHx8c=
     ";break;
  case "Event_Disabled":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAA/wD/
  AAAA/wDm5uYAYmJiAEGVewBBZXsAg4ODAP///wBqyr0Ai///ALT//wBzc3MAi8q9AMXFxQC0tLQA
  i5W9ANX//wDVyv8AamW9ANXV1QCUlJQApKSkAFJSUgAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABAQEB
  AQEBAQEBAQEBAQABAQABAAEAAQABDgEAAQEBAQAAAAAAAAAAABYLAQEBAQEBAAAABxMODg0DAQEB
  AAEBAAcOCwsUFRUUAQEBBwEBAQEDFBMCAgICAQEBDgMTAQEDDQICBwcHAQEBAgITAQEBARMCBwcH
  AQEBBwICAhMBAQkKBwcHAQEBEhISAgIBAQEBEAcHAQEBERERAgcCAgEBCQoHAQEBDw8PDw8CAgEB
  AQEJAQEBDQ0NDQ0NAgIOAQELAQEBBwcHBwcHCgkMAQEBAQEBCAkJCgoJCQgFCwcBAQEBAgEDAQQB
  BQEGAQcBAQABAQEBAQEBAQEBAQEBAQA=
     ";break;
  case "Final_Location":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAAAAAA
  ACAAAABBAAAAagAAAIsAAAC0AAAA1QAAAP8AAAAAMAAAIDAAAEEwAABqMAAAizAAALQwAADVMAAA
  /zAAAABlAAAgZQAAQWUAAGplAACLZQAAtGUAANVlAAD/ZQAAAJUAACCVAABBlQAAapUAAIuVAAC0
  lQAA1ZUAAP+VAAAAygAAIMoAAEHKAABqygAAi8oAALTKAADVygAA/8oAAAD/AAAg/wAAQf8AAGr/
  AACL/wAAtP8AANX/AAD//wAAAAA5ACAAOQBBADkAagA5AIsAOQC0ADkA1QA5AP8AOQAAMDkAIDA5
  AEEwOQBqMDkAizA5ALQwOQDVMDkA/zA5AABlOQAgZTkAQWU5AGplOQCLZTkAtGU5ANVlOQD/ZTkA
  AJU5ACCVOQBBlTkAapU5AIuVOQC0lTkA1ZU5AP+VOQAAyjkAIMo5AEHKOQBqyjkAi8o5ALTKOQDV
  yjkA/8o5AAD/OQAg/zkAQf85AGr/OQCL/zkAtP85ANX/OQD//zkAAAB7ACAAewBBAHsAagB7AIsA
  ewC0AHsA1QB7AP8AewAAMHsAIDB7AEEwewBqMHsAizB7ALQwewDVMHsA/zB7AABlewAgZXsAQWV7
  AGplewCLZXsAtGV7ANVlewD/ZXsAAJV7ACCVewBBlXsAapV7AIuVewC0lXsA1ZV7AP+VewAAynsA
  IMp7AEHKewBqynsAi8p7ALTKewDVynsA/8p7AAD/ewAg/3sAQf97AGr/ewCL/3sAtP97ANX/ewD/
  /3sAAAC9ACAAvQBBAL0AagC9AIsAvQC0AL0A1QC9AP8AvQAAML0AIDC9AEEwvQBqML0AizC9ALQw
  vQDVML0A/zC9AABlvQAgZb0AQWW9AGplvQCLZb0AtGW9ANVlvQD/Zb0AAJW9ACCVvQBBlb0AapW9
  AIuVvQC0lb0A1ZW9AP+VvQAAyr0AIMq9AEHKvQBqyr0Ai8q9ALTKvQDVyr0A/8q9AAD/vQAg/70A
  Qf+9AGr/vQCL/70AtP+9ANX/vQD//70AAAD/ACAA/wBBAP8AagD/AIsA/wC0AP8A1QD/AP8A/wAA
  MP8AIDD/AEEw/wBqMP8AizD/ALQw/wDVMP8A/zD/AABl/wAgZf8AQWX/AGpl/wCLZf8AtGX/ANVl
  /wD/Zf8AAJX/ACCV/wBBlf8AapX/AIuV/wC0lf8A1ZX/AP+V/wAAyv8AIMr/AEHK/wBqyv8Ai8r/
  ALTK/wDVyv8A/8r/AAD//wAg//8AQf//AGr//wCL//8AtP//ANX//wD///8A////AObm5gDV1dUA
  xcXFALS0tACkpKQAlJSUAIODgwBzc3MAYmJiAFJSUgBBQUEAMTExACAgIAAQEBAAAAAAAPPz8/Pz
  8/Pz8/Pz8/Pz87Xzx8fxQ/jHx8fHx8fHx8fz88fx80R88cfHx8fHx8fH8/PH8X1E9PHHx8fHx8fH
  x/Pzx/F0RPTxx8fHx8fHx8fz88fzREX1rPHxx8fHx8fH8/PH9ER8qqGr8/HHx8fHx/Pz8X1E96jQ
  mJrz8cfHx8fz8/FERKvZoJiYq/S28cfH8/PzRESqqKCYoNmhoqzyx/Pz9ET3qqCgmKCgoJiZrMfz
  831E9qmgmJjYqaqs8vHH8/NERKvYoJir8vLxx8fHx/PzRHy0qqvl8cfHx8fHx8fz8/S28fHHx8fH
  x8fHx8fH8/Pz8/Pz8/Pz8/O18/Pz8/M=
     ";break;
  case "Final_Location_Disabled":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAAxcXF
  AAAA/wDm5uYA/wD/AEGVvQBqlb0AtMr/AItlOQAAZb0AADC9ANXV1QAAlf8AIJW9AIuVvQC0tLQA
  g4ODACAwvQAglf8AIGW9AEFlvQDVyr0AtJV7AACVvQAAZf8Ai5V7ALRlOQCkpKQAi2V7AGplOQC0
  yr0AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABAQEB
  AQEBAQEBAQEBAR0BAQMBHAEDAQMBAwEDAQEBAQMCAAcYAgMDAwMDAQEBAQEBAhUHDgIDAwMDAQEB
  AwEBAwIbBw4CAwMDAQEBAwEBAQEABxkaDQICAQEBAwMDAQEDDgcYBBIFAQEBAwMDAQEBARUHDxYX
  AQEBAgMDAwMBAQIHBwURAQEBBQ4UAgMBAQEBBwcEAQEBCBESEw0KAwEBDgcPAQEBCQgICAkQDQEB
  AQEHAQEBCQkLDAQNCgIDAQEHAQEBCAkFCgoCAwMDAQEBAQEBBAUGAgMDAwMDAwMBAQEBAgEDAQMB
  AwEDAQMBAQABAQEBAQEBAQEBAQEBAQA=
     ";break;
  case "Final_Location_Found":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAAxcXF
  ALTKvQC0tLQA1cq9AObm5gD/AP8Ai2U5AIuVewCLyr0AQZW9AGqVvQC0yv8AAJX/AABlvQAAML0A
  1dXVALSVewCUlJQAIJW9AIuVvQCDg4MAIDC9AACVvQAglf8AIGW9AEFlvQAgygAAAGX/AEEwvQC0
  ZTkAi2V7AGplOQBzc3MAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAGgAAAAAAAAEABQUEHyAFGhoaBQUFBQUAAAUEAAYHGhoaGhoFBQUFAAAFBBAGAhoaGhoaGgUF
  BQAABQQeBhoaGhoaGhoaBQUAAAUABh0aGhoEBRoaGhoFAAAFAgYHCRoaAAQFGhoaGgAABBAGFBYb
  DhwABAUaGhoaAAQGBgoXDQ4OCgIDBBoaGgAABgYJFg0ODRcYGRMPGgAAAgYUCQ0NDg0NDQ4VEwUA
  ABAGERINDg4MEgkTDwQFAAAGBgoMDQ4KDw8EBQUFBQAABgcICQoLBAUFBQUFBQUAAAIDBAQFBQUF
  BQUFBQUFAAAAAAAAAAAAAAABAAAAAAA=
     ";break;
  case "Found":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAAAAAA
  ACAAAABBAAAAagAAAIsAAAC0AAAA1QAAAP8AAAAAMAAAIDAAAEEwAABqMAAAizAAALQwAADVMAAA
  /zAAAABlAAAgZQAAQWUAAGplAACLZQAAtGUAANVlAAD/ZQAAAJUAACCVAABBlQAAapUAAIuVAAC0
  lQAA1ZUAAP+VAAAAygAAIMoAAEHKAABqygAAi8oAALTKAADVygAA/8oAAAD/AAAg/wAAQf8AAGr/
  AACL/wAAtP8AANX/AAD//wAAAAA5ACAAOQBBADkAagA5AIsAOQC0ADkA1QA5AP8AOQAAMDkAIDA5
  AEEwOQBqMDkAizA5ALQwOQDVMDkA/zA5AABlOQAgZTkAQWU5AGplOQCLZTkAtGU5ANVlOQD/ZTkA
  AJU5ACCVOQBBlTkAapU5AIuVOQC0lTkA1ZU5AP+VOQAAyjkAIMo5AEHKOQBqyjkAi8o5ALTKOQDV
  yjkA/8o5AAD/OQAg/zkAQf85AGr/OQCL/zkAtP85ANX/OQD//zkAAAB7ACAAewBBAHsAagB7AIsA
  ewC0AHsA1QB7AP8AewAAMHsAIDB7AEEwewBqMHsAizB7ALQwewDVMHsA/zB7AABlewAgZXsAQWV7
  AGplewCLZXsAtGV7ANVlewD/ZXsAAJV7ACCVewBBlXsAapV7AIuVewC0lXsA1ZV7AP+VewAAynsA
  IMp7AEHKewBqynsAi8p7ALTKewDVynsA/8p7AAD/ewAg/3sAQf97AGr/ewCL/3sAtP97ANX/ewD/
  /3sAAAC9ACAAvQBBAL0AagC9AIsAvQC0AL0A1QC9AP8AvQAAML0AIDC9AEEwvQBqML0AizC9ALQw
  vQDVML0A/zC9AABlvQAgZb0AQWW9AGplvQCLZb0AtGW9ANVlvQD/Zb0AAJW9ACCVvQBBlb0AapW9
  AIuVvQC0lb0A1ZW9AP+VvQAAyr0AIMq9AEHKvQBqyr0Ai8q9ALTKvQDVyr0A/8q9AAD/vQAg/70A
  Qf+9AGr/vQCL/70AtP+9ANX/vQD//70AAAD/ACAA/wBBAP8AagD/AIsA/wC0AP8A1QD/AP8A/wAA
  MP8AIDD/AEEw/wBqMP8AizD/ALQw/wDVMP8A/zD/AABl/wAgZf8AQWX/AGpl/wCLZf8AtGX/ANVl
  /wD/Zf8AAJX/ACCV/wBBlf8AapX/AIuV/wC0lf8A1ZX/AP+V/wAAyv8AIMr/AEHK/wBqyv8Ai8r/
  ALTK/wDVyv8A/8r/AAD//wAg//8AQf//AGr//wCL//8AtP//ANX//wD///8A////AObm5gDV1dUA
  xcXFALS0tACkpKQAlJSUAIODgwBzc3MAYmJiAFJSUgBBQUEAMTExACAgIAAQEBAAAAAAAMfHx8fH
  x8fHx8fHx8fHx8fHx8fHrKKhoaGirMfHx8fHx8fHoanh4eHh4amix8fHx8fHoeHhqaGhoani4aHH
  x8fHoeHioaHi4uKhoeLhosfHrKnhqqGhoqqioaGq4qmsx6Lh4qGhoaGhoaGhoerhoseh4err6+vr
  6+vr6+vq4aHHoeHq6+vr6+vr6+vr6uGhx6Hh6uvioeLr4qHi6+rhocei4eLrqqHj66qh4+vq4aLH
  rKnh6qqh4+uqoeLq4amsx8eh4eHioeLr4qHi4uGhx8fHx6Hh4eLq6uri4eGhx8fHx8fHoanh4eHh
  4amhx8fHx8fHx8esoqGhoaKsx8fHx8c=
     ";break;
  case "Found_Disabled":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAA/wD/
  AAAA/wAgZb0AIMr/AEHK/wBB//8Aav//AGrK/wBBlb0AIJW9AIuVvQBBZb0AAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABAQEB
  AQEBAQEBAQEBAQABAQABCgECAQIBCgEAAQEBAQAAAgkDAwMDAwkLAQEBAQEBAgMDCQICAgkEAQEB
  AAEBAgMEAgIEBAQCAQEBCwEBAQEDCAICCwgLAQEBBAkKAQEDBAICAgICAQEBAgUDAQEBAQUGBgYG
  AQEBBgYFAwIBAQMFBgYGAQEBBgYGBQMBAQEBBQYEAQEBBAIEBgUDAgEBAwQGAQEBBggCBwYFAwEB
  AQEDAQEBBwYIAgQFAwkKAQECAQEBAgQGBAIEBAMCAQEBAQEBAwQFBQUEAwMCAAABAQEBAgEDAQMB
  AwECAQABAQABAQEBAQEBAQEBAQEBAQA=
     ";break;
  case "Found_Solved":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAA/wD/
  AIuVvQBBZb0AIGW9ACCVvQAgyv8AQcr/AEH//wBq//8AQZW9AGrK/wAgygAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAACwAAAAAAAAAAAAAAAQIDCwsLAQAAAAAAAAAAAwQFCwsLCwsCAAAAAAAAAwUFBAsLCwsLCwMA
  AAAAAwUGAwsLCwsLCwsLAgAAAQQFCQMLCwsCAwsLCwsBAAIFBgMDAwsLAwMDCwsLCwADBQcICAgI
  CAgICAgLCwsLAwUHCAgICAgICAgIBwsLCwMFBwgGAwYIBgMGCAcFCwACBQYICQMKCAkDCggHBQIA
  AQQFBwkDCggJAwYHBQQBAAADBQUGAwYIBgMGBgUDAAAAAAMFBQYHBwcGBQUDAAAAAAAAAwQFBQUF
  BQQDAAAAAAAAAAABAgMDAwIBAAAAAAA=
     ";break;
  case "Groundspeak_Block_Party":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAAAAAA
  ACAAAABBAAAAagAAAIsAAAC0AAAA1QAAAP8AAAAAMAAAIDAAAEEwAABqMAAAizAAALQwAADVMAAA
  /zAAAABlAAAgZQAAQWUAAGplAACLZQAAtGUAANVlAAD/ZQAAAJUAACCVAABBlQAAapUAAIuVAAC0
  lQAA1ZUAAP+VAAAAygAAIMoAAEHKAABqygAAi8oAALTKAADVygAA/8oAAAD/AAAg/wAAQf8AAGr/
  AACL/wAAtP8AANX/AAD//wAAAAA5ACAAOQBBADkAagA5AIsAOQC0ADkA1QA5AP8AOQAAMDkAIDA5
  AEEwOQBqMDkAizA5ALQwOQDVMDkA/zA5AABlOQAgZTkAQWU5AGplOQCLZTkAtGU5ANVlOQD/ZTkA
  AJU5ACCVOQBBlTkAapU5AIuVOQC0lTkA1ZU5AP+VOQAAyjkAIMo5AEHKOQBqyjkAi8o5ALTKOQDV
  yjkA/8o5AAD/OQAg/zkAQf85AGr/OQCL/zkAtP85ANX/OQD//zkAAAB7ACAAewBBAHsAagB7AIsA
  ewC0AHsA1QB7AP8AewAAMHsAIDB7AEEwewBqMHsAizB7ALQwewDVMHsA/zB7AABlewAgZXsAQWV7
  AGplewCLZXsAtGV7ANVlewD/ZXsAAJV7ACCVewBBlXsAapV7AIuVewC0lXsA1ZV7AP+VewAAynsA
  IMp7AEHKewBqynsAi8p7ALTKewDVynsA/8p7AAD/ewAg/3sAQf97AGr/ewCL/3sAtP97ANX/ewD/
  /3sAAAC9ACAAvQBBAL0AagC9AIsAvQC0AL0A1QC9AP8AvQAAML0AIDC9AEEwvQBqML0AizC9ALQw
  vQDVML0A/zC9AABlvQAgZb0AQWW9AGplvQCLZb0AtGW9ANVlvQD/Zb0AAJW9ACCVvQBBlb0AapW9
  AIuVvQC0lb0A1ZW9AP+VvQAAyr0AIMq9AEHKvQBqyr0Ai8q9ALTKvQDVyr0A/8q9AAD/vQAg/70A
  Qf+9AGr/vQCL/70AtP+9ANX/vQD//70AAAD/ACAA/wBBAP8AagD/AIsA/wC0AP8A1QD/AP8A/wAA
  MP8AIDD/AEEw/wBqMP8AizD/ALQw/wDVMP8A/zD/AABl/wAgZf8AQWX/AGpl/wCLZf8AtGX/ANVl
  /wD/Zf8AAJX/ACCV/wBBlf8AapX/AIuV/wC0lf8A1ZX/AP+V/wAAyv8AIMr/AEHK/wBqyv8Ai8r/
  ALTK/wDVyv8A/8r/AAD//wAg//8AQf//AGr//wCL//8AtP//ANX//wD///8A////AObm5gDV1dUA
  xcXFALS0tACkpKQAlJSUAIODgwBzc3MAYmJiAFJSUgBBQUEAMTExACAgIAAQEBAAAAAAAMfH/IKC
  goKCgoKCgoKCgoLHCUKCQkJCQkKCQkJCQkKCQkJCgkJCQkp6gkJCQkJCgkJCQoJCQkJCQoJCSkJC
  QoJCQkKCQkpCQkKCQnqCSkKCQkJCgkJ6QkJCgkJCQkJCgkJCQoKCgoKCgoKCgoKCgoJCQkKCQoJC
  QkKCQkJCgkKCQkJCgkJ6QkJCgkJCQnpCgkJCQoJCSkJKQoJCQkJCQoJCQkKCQkp6QkKCenpCQkKC
  QkJCgkJCSkJCgkJCQkJCekJCQoJ6enp6eoJ6enp6enpCQkJ6enp6enp6enp6enp6QkJKSkpKSkpK
  SkpKSkpCx0JKSkpKSkpKSkpKSkpCx8c=
     ";break;
  case "Groundspeak_Block_Party_Disabled":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAAQWU5
  AAAA/wD/AP8AQZU5AEGVewBBynsAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAIBAQEB
  AQEBAQEBAQEBAQUBAQABAAEAAQABAAEAAQEBAQAABQAAAAMEBQAAAQEBAQEBAAUAAAAAAAUAAQEB
  AAEBAAAFAAMAAAAFAQEBAwEBAQEABQAEAAAAAQEBAAAAAQEAAAUFBQUFAQEBBQUFAQEBAQAFAAUA
  AQEBAAAABQABAQAABQAEAQEBBQAAAAQBAQEBAAUAAQEBAAUAAAAAAAEBAAAFAQEBAAAFBAQAAAEB
  AQEAAQEBAwAABQAAAAAAAQEAAQEBBAQEBAUEBAQEAQEBAQEBBAQEBAQEBAQEBAQBAQEBAwEDAQMB
  AwEDAQMBAQABAQEBAQEBAQEBAQEBAQI=
     ";break;
  case "Groundspeak_HQ":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAAAAAA
  ACAAAABBAAAAagAAAIsAAAC0AAAA1QAAAP8AAAAAMAAAIDAAAEEwAABqMAAAizAAALQwAADVMAAA
  /zAAAABlAAAgZQAAQWUAAGplAACLZQAAtGUAANVlAAD/ZQAAAJUAACCVAABBlQAAapUAAIuVAAC0
  lQAA1ZUAAP+VAAAAygAAIMoAAEHKAABqygAAi8oAALTKAADVygAA/8oAAAD/AAAg/wAAQf8AAGr/
  AACL/wAAtP8AANX/AAD//wAAAAA5ACAAOQBBADkAagA5AIsAOQC0ADkA1QA5AP8AOQAAMDkAIDA5
  AEEwOQBqMDkAizA5ALQwOQDVMDkA/zA5AABlOQAgZTkAQWU5AGplOQCLZTkAtGU5ANVlOQD/ZTkA
  AJU5ACCVOQBBlTkAapU5AIuVOQC0lTkA1ZU5AP+VOQAAyjkAIMo5AEHKOQBqyjkAi8o5ALTKOQDV
  yjkA/8o5AAD/OQAg/zkAQf85AGr/OQCL/zkAtP85ANX/OQD//zkAAAB7ACAAewBBAHsAagB7AIsA
  ewC0AHsA1QB7AP8AewAAMHsAIDB7AEEwewBqMHsAizB7ALQwewDVMHsA/zB7AABlewAgZXsAQWV7
  AGplewCLZXsAtGV7ANVlewD/ZXsAAJV7ACCVewBBlXsAapV7AIuVewC0lXsA1ZV7AP+VewAAynsA
  IMp7AEHKewBqynsAi8p7ALTKewDVynsA/8p7AAD/ewAg/3sAQf97AGr/ewCL/3sAtP97ANX/ewD/
  /3sAAAC9ACAAvQBBAL0AagC9AIsAvQC0AL0A1QC9AP8AvQAAML0AIDC9AEEwvQBqML0AizC9ALQw
  vQDVML0A/zC9AABlvQAgZb0AQWW9AGplvQCLZb0AtGW9ANVlvQD/Zb0AAJW9ACCVvQBBlb0AapW9
  AIuVvQC0lb0A1ZW9AP+VvQAAyr0AIMq9AEHKvQBqyr0Ai8q9ALTKvQDVyr0A/8q9AAD/vQAg/70A
  Qf+9AGr/vQCL/70AtP+9ANX/vQD//70AAAD/ACAA/wBBAP8AagD/AIsA/wC0AP8A1QD/AP8A/wAA
  MP8AIDD/AEEw/wBqMP8AizD/ALQw/wDVMP8A/zD/AABl/wAgZf8AQWX/AGpl/wCLZf8AtGX/ANVl
  /wD/Zf8AAJX/ACCV/wBBlf8AapX/AIuV/wC0lf8A1ZX/AP+V/wAAyv8AIMr/AEHK/wBqyv8Ai8r/
  ALTK/wDVyv8A/8r/AAD//wAg//8AQf//AGr//wCL//8AtP//ANX//wD///8A////AObm5gDV1dUA
  xcXFALS0tACkpKQAlJSUAIODgwBzc3MAYmJiAFJSUgBBQUEAMTExACAgIAAQEBAAAAAAAMfHx8fz
  e0JCQkJ788fHx8fHx/F7QkJCQkJCEhJ78cfHx/F7SkpCQkJCQkJCEkLxx8eDSkpKSkpCQkJCQkIR
  e8fyenpBSkpBQUJBcUFxeUG1s3riekp64kF64uLi4nFCe3p64nlKeuJx4npK4uJxQUJ6euKqeari
  qqpCeqqqqkFCgnrisrKy4qqqQUpKqqpCQoKC4qp6euJx4nFBeeJ6QkqzguKqerLierLi4uKqSkp7
  8oKygoKCsoJ6enp6ekpK8sezgoKCgrR6enp6enp6g8fHx7OCgvGzgnp6enp6e/HHx8fHs73HgoKC
  enp6g/HHx8fHx8fH8bKCgoK08sfHx8c=
     ";break;
  case "Groundspeak_HQ_Disabled":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAA/wD/
  AAAA/wBqyr0AQcp7AEGVewDm5uYAapV7AIvKvQBByr0AQZU5AEHK/wBBlb0AIGV7ACBlOQAglXsA
  QWU5AEFlAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABAQEB
  AQEBAQEBAQEBAQABAQUBDwEPAQ8BEAEGAQEBAQUGCQkPDw8PDw8PAQEBAQEBCQkJCQkPDw8PAQEB
  BgEBBAQNCQkNDQ8NAQEBDgEBAQEKBAkECg0EAQEBCgwPAQEECg4JBAoMAQEBCgoMAQEBAQoLDgsK
  AQEBBAsLCw0BAQQKCAgIAQEBDQkJCwsBAQEBCgsEAQEBCgwNDgoEDwEBAwoLAQEBBAgKCgoLCQEB
  AQEIAQEBCAMEBAQEBAkJAQECAQEBAwcEBAQEBAQEAQEBAQEBAwUCAwQEBAQEBgUBAQEBAgEAAQMB
  BAEEAQUBAQABAQEBAQEBAQEBAQEBAQA=
     ";break;
  case "Groundspeak_Lost_and_Found":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAAAAAA
  ACAAAABBAAAAagAAAIsAAAC0AAAA1QAAAP8AAAAAMAAAIDAAAEEwAABqMAAAizAAALQwAADVMAAA
  /zAAAABlAAAgZQAAQWUAAGplAACLZQAAtGUAANVlAAD/ZQAAAJUAACCVAABBlQAAapUAAIuVAAC0
  lQAA1ZUAAP+VAAAAygAAIMoAAEHKAABqygAAi8oAALTKAADVygAA/8oAAAD/AAAg/wAAQf8AAGr/
  AACL/wAAtP8AANX/AAD//wAAAAA5ACAAOQBBADkAagA5AIsAOQC0ADkA1QA5AP8AOQAAMDkAIDA5
  AEEwOQBqMDkAizA5ALQwOQDVMDkA/zA5AABlOQAgZTkAQWU5AGplOQCLZTkAtGU5ANVlOQD/ZTkA
  AJU5ACCVOQBBlTkAapU5AIuVOQC0lTkA1ZU5AP+VOQAAyjkAIMo5AEHKOQBqyjkAi8o5ALTKOQDV
  yjkA/8o5AAD/OQAg/zkAQf85AGr/OQCL/zkAtP85ANX/OQD//zkAAAB7ACAAewBBAHsAagB7AIsA
  ewC0AHsA1QB7AP8AewAAMHsAIDB7AEEwewBqMHsAizB7ALQwewDVMHsA/zB7AABlewAgZXsAQWV7
  AGplewCLZXsAtGV7ANVlewD/ZXsAAJV7ACCVewBBlXsAapV7AIuVewC0lXsA1ZV7AP+VewAAynsA
  IMp7AEHKewBqynsAi8p7ALTKewDVynsA/8p7AAD/ewAg/3sAQf97AGr/ewCL/3sAtP97ANX/ewD/
  /3sAAAC9ACAAvQBBAL0AagC9AIsAvQC0AL0A1QC9AP8AvQAAML0AIDC9AEEwvQBqML0AizC9ALQw
  vQDVML0A/zC9AABlvQAgZb0AQWW9AGplvQCLZb0AtGW9ANVlvQD/Zb0AAJW9ACCVvQBBlb0AapW9
  AIuVvQC0lb0A1ZW9AP+VvQAAyr0AIMq9AEHKvQBqyr0Ai8q9ALTKvQDVyr0A/8q9AAD/vQAg/70A
  Qf+9AGr/vQCL/70AtP+9ANX/vQD//70AAAD/ACAA/wBBAP8AagD/AIsA/wC0AP8A1QD/AP8A/wAA
  MP8AIDD/AEEw/wBqMP8AizD/ALQw/wDVMP8A/zD/AABl/wAgZf8AQWX/AGpl/wCLZf8AtGX/ANVl
  /wD/Zf8AAJX/ACCV/wBBlf8AapX/AIuV/wC0lf8A1ZX/AP+V/wAAyv8AIMr/AEHK/wBqyv8Ai8r/
  ALTK/wDVyv8A/8r/AAD//wAg//8AQf//AGr//wCL//8AtP//ANX//wD///8A////AObm5gDV1dUA
  xcXFALS0tACkpKQAlJSUAIODgwBzc3MAYmJiAFJSUgBBQUEAMTExACAgIAAQEBAAAAAAAMfHx8fz
  e0JCQkJ788fHx8fHx/F7QkJCQkJCEhJ78cfHx/F7SkpCQkJCQkJCEkLHx8d7SkpKSkpCQkJCQkIR
  e8fyenpKQkpyQUJBcUF7fBG1tHq070F64kGq4uLj73tCe3p6tO9BeuJ54npK8e1xQUJ6erTv9vXi
  qqpChLWqqkFCgnq07/Ly4qqqQUpKqqpCQoKCtO95euJ64nFBeuJ6QkqzgrTveuLierLi4uKqSkp7
  8oKCgoKCsnp6enp6ekpK8sezgoKCgrR6enp6enp6e8fHx7OCgvKzgnp6enp6e/HHx8fHs7XHgoKC
  enp6g/HHx8fHx8fH8bKCgoKz8sfHx8c=
     ";break;
  case "Groundspeak_Lost_and_Found_Disabled":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAA/wD/
  AAAA/wBqyr0AQcp7AEGVewDm5uYA1dXVAGqVewCLyr0AQcq9AEGVOQD///8AQcr/AEGVvQAglXsA
  IGV7ACBlOQBBZTkAlJSUAKSkpACLynsAtMq9ALT//wBBZXsAi5V7AEFlAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABAQEB
  AQEBAQEBAQEBAQABAQUBEQERAREBGQEHAQEBAQUHCgoRERERERERAQEBAQEBCgoKCgoRERERAQEB
  BwEBBAQKEQoXEBEQAQEBGAEBAQEICxAEDBANAQEBCwcRAQEECAsQBAwOAQEBBRYPAQEBAQgLEhMM
  AQEBFBUNDRABAQQICwYGAQEBEAoKDQ0BAQEBCAsOAQEBDA8QBAwEEQEBAwgLAQEBBAkMDAwNCgEB
  AQEDAQEBCQQEBAQEBAoKAQECAQEBAwgEBAQEBAQEAQEBAQEBAwYCAwQEBAQEBwUBAQEBAgEAAQMB
  BAEEAQUBAQABAQEBAQEBAQEBAQEBAQA=
     ";break;
  case "Letterbox":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAAAAAA
  ACAAAABBAAAAagAAAIsAAAC0AAAA1QAAAP8AAAAAMAAAIDAAAEEwAABqMAAAizAAALQwAADVMAAA
  /zAAAABlAAAgZQAAQWUAAGplAACLZQAAtGUAANVlAAD/ZQAAAJUAACCVAABBlQAAapUAAIuVAAC0
  lQAA1ZUAAP+VAAAAygAAIMoAAEHKAABqygAAi8oAALTKAADVygAA/8oAAAD/AAAg/wAAQf8AAGr/
  AACL/wAAtP8AANX/AAD//wAAAAA5ACAAOQBBADkAagA5AIsAOQC0ADkA1QA5AP8AOQAAMDkAIDA5
  AEEwOQBqMDkAizA5ALQwOQDVMDkA/zA5AABlOQAgZTkAQWU5AGplOQCLZTkAtGU5ANVlOQD/ZTkA
  AJU5ACCVOQBBlTkAapU5AIuVOQC0lTkA1ZU5AP+VOQAAyjkAIMo5AEHKOQBqyjkAi8o5ALTKOQDV
  yjkA/8o5AAD/OQAg/zkAQf85AGr/OQCL/zkAtP85ANX/OQD//zkAAAB7ACAAewBBAHsAagB7AIsA
  ewC0AHsA1QB7AP8AewAAMHsAIDB7AEEwewBqMHsAizB7ALQwewDVMHsA/zB7AABlewAgZXsAQWV7
  AGplewCLZXsAtGV7ANVlewD/ZXsAAJV7ACCVewBBlXsAapV7AIuVewC0lXsA1ZV7AP+VewAAynsA
  IMp7AEHKewBqynsAi8p7ALTKewDVynsA/8p7AAD/ewAg/3sAQf97AGr/ewCL/3sAtP97ANX/ewD/
  /3sAAAC9ACAAvQBBAL0AagC9AIsAvQC0AL0A1QC9AP8AvQAAML0AIDC9AEEwvQBqML0AizC9ALQw
  vQDVML0A/zC9AABlvQAgZb0AQWW9AGplvQCLZb0AtGW9ANVlvQD/Zb0AAJW9ACCVvQBBlb0AapW9
  AIuVvQC0lb0A1ZW9AP+VvQAAyr0AIMq9AEHKvQBqyr0Ai8q9ALTKvQDVyr0A/8q9AAD/vQAg/70A
  Qf+9AGr/vQCL/70AtP+9ANX/vQD//70AAAD/ACAA/wBBAP8AagD/AIsA/wC0AP8A1QD/AP8A/wAA
  MP8AIDD/AEEw/wBqMP8AizD/ALQw/wDVMP8A/zD/AABl/wAgZf8AQWX/AGpl/wCLZf8AtGX/ANVl
  /wD/Zf8AAJX/ACCV/wBBlf8AapX/AIuV/wC0lf8A1ZX/AP+V/wAAyv8AIMr/AEHK/wBqyv8Ai8r/
  ALTK/wDVyv8A/8r/AAD//wAg//8AQf//AGr//wCL//8AtP//ANX//wD///8A////AObm5gDV1dUA
  xcXFALS0tACkpKQAlJSUAIODgwBzc3MAYmJiAFJSUgBBQUEAMTExACAgIAAQEBAAAAAAAPHy8e/H
  x8fHx8fHx8fHx8f7+jr9+fby78fHx8fHx8fH+vPx9Pv3+Pr59/Pxx8fHx/rz8fH0+vLx8vT3+fr4
  9PH68/Hx8fX58vHx8fHx8/f8+vPx8fHx9/jx8bby8fH4/vrz8fHx8fH59/j6+vT59/n68/Hx8fHz
  9/z18fP99vH5+vPx8fP3+vbx8fHx+PXx+frztvb79/Hx8fHx8fL68fn79vr48fHx8fHx8fHx9/b5
  /fv29PPy8fHx8fHx8fH6+vH1+Pn6+ff19PPy8fHx9v3Hx8fH7/L09/n6+vj19PX+x8fHx8fHx8fH
  x/H09/f38sfHx8fHx8fHx8fHx8fHx8c=
     ";break;
  case "Letterbox_Disabled":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAA/wD/
  AAAA/wC0tLQAg4ODAP///wDV1dUAYmJiAFJSUgBzc3MApKSkAMXFxQDm5uYAlJSUANXKvQBBQUEA
  ICAgAEEwOQAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAsBAQEB
  AQEBAQEBAQEBAQABARABBgEFAQABAAEAAQEBAQoLAg4DCAcGAwoLAQEBAQEBCwsCBwULBQIDAQEB
  AgEBCgsLCwkGBQsLAQEBCgEBAQELCwsLAwgLAQEBCwsIAQEKCwsLCwsGAQEBBwIGAQEBAQsLCwsK
  AQEBCwoPDAsBAQoLCwoDAQEBCwsLCAkBAQEBDQwOAQEBCwsLCwUHCwEBDAcIAQEBCwsLCwsLAwEB
  AQEMAQEBCwsLCwsLCwsHAQEJAQEBBgMJAgoFCwsLAQEBAQEBBAUCAwYHBwgJAgkBAQEBAAEAAQAB
  AAECAQMBAQABAQEBAQEBAQEBAQEBAQA=
     ";break;
  case "Locationless":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAAAAAA
  ACAAAABBAAAAagAAAIsAAAC0AAAA1QAAAP8AAAAAMAAAIDAAAEEwAABqMAAAizAAALQwAADVMAAA
  /zAAAABlAAAgZQAAQWUAAGplAACLZQAAtGUAANVlAAD/ZQAAAJUAACCVAABBlQAAapUAAIuVAAC0
  lQAA1ZUAAP+VAAAAygAAIMoAAEHKAABqygAAi8oAALTKAADVygAA/8oAAAD/AAAg/wAAQf8AAGr/
  AACL/wAAtP8AANX/AAD//wAAAAA5ACAAOQBBADkAagA5AIsAOQC0ADkA1QA5AP8AOQAAMDkAIDA5
  AEEwOQBqMDkAizA5ALQwOQDVMDkA/zA5AABlOQAgZTkAQWU5AGplOQCLZTkAtGU5ANVlOQD/ZTkA
  AJU5ACCVOQBBlTkAapU5AIuVOQC0lTkA1ZU5AP+VOQAAyjkAIMo5AEHKOQBqyjkAi8o5ALTKOQDV
  yjkA/8o5AAD/OQAg/zkAQf85AGr/OQCL/zkAtP85ANX/OQD//zkAAAB7ACAAewBBAHsAagB7AIsA
  ewC0AHsA1QB7AP8AewAAMHsAIDB7AEEwewBqMHsAizB7ALQwewDVMHsA/zB7AABlewAgZXsAQWV7
  AGplewCLZXsAtGV7ANVlewD/ZXsAAJV7ACCVewBBlXsAapV7AIuVewC0lXsA1ZV7AP+VewAAynsA
  IMp7AEHKewBqynsAi8p7ALTKewDVynsA/8p7AAD/ewAg/3sAQf97AGr/ewCL/3sAtP97ANX/ewD/
  /3sAAAC9ACAAvQBBAL0AagC9AIsAvQC0AL0A1QC9AP8AvQAAML0AIDC9AEEwvQBqML0AizC9ALQw
  vQDVML0A/zC9AABlvQAgZb0AQWW9AGplvQCLZb0AtGW9ANVlvQD/Zb0AAJW9ACCVvQBBlb0AapW9
  AIuVvQC0lb0A1ZW9AP+VvQAAyr0AIMq9AEHKvQBqyr0Ai8q9ALTKvQDVyr0A/8q9AAD/vQAg/70A
  Qf+9AGr/vQCL/70AtP+9ANX/vQD//70AAAD/ACAA/wBBAP8AagD/AIsA/wC0AP8A1QD/AP8A/wAA
  MP8AIDD/AEEw/wBqMP8AizD/ALQw/wDVMP8A/zD/AABl/wAgZf8AQWX/AGpl/wCLZf8AtGX/ANVl
  /wD/Zf8AAJX/ACCV/wBBlf8AapX/AIuV/wC0lf8A1ZX/AP+V/wAAyv8AIMr/AEHK/wBqyv8Ai8r/
  ALTK/wDVyv8A/8r/AAD//wAg//8AQf//AGr//wCL//8AtP//ANX//wD///8A////AObm5gDV1dUA
  xcXFALS0tACkpKQAlJSUAIODgwBzc3MAYmJiAFJSUgBBQUEAMTExACAgIAAQEBAAAAAAAMfHx8fH
  x8fHx8fHx8fHx8fz9/f39/f39/f39/f39/fH8wlDhb9DhoaGhoaFhof4x8f5EkwSEYaGh0yGEkND
  88fHx/oRERG/hoZDEhEJ88fHx8fH9wkRQ7e/v0P788fHx8fHx8fH9wkJ+Pjzx8fHx8fHx8fHx8f3
  8ffHx8fHx8fHx8fHx8fH9/H3x8fHx8fHx8fHx8fHx/fx+8fHx8fHx8fHx8fHx8f38TD4c/PHx8fH
  x8fHx8fH9/Fpq+Oqc8fHx8fHx8fHx/fxcaGsc/PHx8fHx8fHx8f38Tnz88fHx8fHx8fHx8fHx/fH
  x8fHx8fHx8fHx8fHx8fHx8fHx8fHx8c=
     ";break;
  case "Locationless_Disabled":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAA/wD/
  AAAA/wCDg4MA5ubmACAwOQDFxcUAIGV7ACBlvQCLlb0AamV7ACAwewBqlb0Aasr/AEGVvQAAADkA
  c3NzAEFBQQAgMAAAIGUAAGplOQD/yr0A//+9AFJSUgDVynsAQWUAAIuVOQD/ynsAtMp7AAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABAQEB
  AQEBAQEBAQEBAQABAQIBAgECAQIBAgECAQEBARETGxUTFxcXFxcbAQEBAQEBGBkYEhcXGhkXAQEB
  BQEBABYSEhIVFxcTAQEBBQEBAQEAAhESExQVAQEBBQAAAQEAAAAAAhERAQEBAAAAAQEBAQAAAAAC
  AQEBAAAAAAABAQAAAAAAAQEBAAAAAAABAQEBAAAAAQEBEAAAAAAAAAEBAAAAAQEBAw4PCQUAAAEB
  AQEAAQEBAgMKCwwNCQAAAQEAAQEBAAIDBgcICQUAAQEBAQEBAAACAwQFBQAAAAABAQEBAAEAAQIB
  AAEAAQABAQABAQEBAQEBAQEBAQEBAQA=
     ";break;
  case "Lost_and_Found_Event":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAAAAAA
  ACAAAABBAAAAagAAAIsAAAC0AAAA1QAAAP8AAAAAMAAAIDAAAEEwAABqMAAAizAAALQwAADVMAAA
  /zAAAABlAAAgZQAAQWUAAGplAACLZQAAtGUAANVlAAD/ZQAAAJUAACCVAABBlQAAapUAAIuVAAC0
  lQAA1ZUAAP+VAAAAygAAIMoAAEHKAABqygAAi8oAALTKAADVygAA/8oAAAD/AAAg/wAAQf8AAGr/
  AACL/wAAtP8AANX/AAD//wAAAAA5ACAAOQBBADkAagA5AIsAOQC0ADkA1QA5AP8AOQAAMDkAIDA5
  AEEwOQBqMDkAizA5ALQwOQDVMDkA/zA5AABlOQAgZTkAQWU5AGplOQCLZTkAtGU5ANVlOQD/ZTkA
  AJU5ACCVOQBBlTkAapU5AIuVOQC0lTkA1ZU5AP+VOQAAyjkAIMo5AEHKOQBqyjkAi8o5ALTKOQDV
  yjkA/8o5AAD/OQAg/zkAQf85AGr/OQCL/zkAtP85ANX/OQD//zkAAAB7ACAAewBBAHsAagB7AIsA
  ewC0AHsA1QB7AP8AewAAMHsAIDB7AEEwewBqMHsAizB7ALQwewDVMHsA/zB7AABlewAgZXsAQWV7
  AGplewCLZXsAtGV7ANVlewD/ZXsAAJV7ACCVewBBlXsAapV7AIuVewC0lXsA1ZV7AP+VewAAynsA
  IMp7AEHKewBqynsAi8p7ALTKewDVynsA/8p7AAD/ewAg/3sAQf97AGr/ewCL/3sAtP97ANX/ewD/
  /3sAAAC9ACAAvQBBAL0AagC9AIsAvQC0AL0A1QC9AP8AvQAAML0AIDC9AEEwvQBqML0AizC9ALQw
  vQDVML0A/zC9AABlvQAgZb0AQWW9AGplvQCLZb0AtGW9ANVlvQD/Zb0AAJW9ACCVvQBBlb0AapW9
  AIuVvQC0lb0A1ZW9AP+VvQAAyr0AIMq9AEHKvQBqyr0Ai8q9ALTKvQDVyr0A/8q9AAD/vQAg/70A
  Qf+9AGr/vQCL/70AtP+9ANX/vQD//70AAAD/ACAA/wBBAP8AagD/AIsA/wC0AP8A1QD/AP8A/wAA
  MP8AIDD/AEEw/wBqMP8AizD/ALQw/wDVMP8A/zD/AABl/wAgZf8AQWX/AGpl/wCLZf8AtGX/ANVl
  /wD/Zf8AAJX/ACCV/wBBlf8AapX/AIuV/wC0lf8A1ZX/AP+V/wAAyv8AIMr/AEHK/wBqyv8Ai8r/
  ALTK/wDVyv8A/8r/AAD//wAg//8AQf//AGr//wCL//8AtP//ANX//wD///8A////AObm5gDV1dUA
  xcXFALS0tACkpKQAlJSUAIODgwBzc3MAYmJiAFJSUgBBQUEAMTExACAgIAAQEBAAAAAAAO/v7+/v
  7+/v7+/v7+/v7++/HR0dHR0dHR0dHYbk2trlhh0dHR0dHR0dHR2G5Nra5b+GhoaGhoaGhoaGv+7k
  5O7t7LT27Ozs7Ozs7O2+hYXx7OioOejo4KiosOjshVNTvuvoqDnosDlBcTlx7IVTU7Xr6Kg54Dl4
  6OjoQfaFU1OF6+ioOag56Ojo6LD2hVNTheKwqDl4Qejo6Ojo9oVTU4SyOXE5qDno6Ojo4PaFU1OE
  6eA5OeA5qOjo6HH2hVNTU+no4HHoqP1xeEE5tIVTU1Po6Ojo6OjgeXF44OyFU1NT6Ojo6Ojo6Ojo
  6OjshVNTU+zs7Ozs7Ozt7e3t7+/v7+8=
     ";break;
  case "Lost_and_Found_Event_Disabled":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAAi///
  AAAA/wD///8AAP//AGrKOQAAyv8AIJV7ACBlewAAlXsAtMp7AACVvQAgICAAIGU5ACAwOQCLyr0A
  lJSUAADKvQDVynsAi8r/ALSVAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAIBAQEB
  AQEBAQEBAQEBAQIBARMBEwETARMBEwESAQEBARMTExMTExMTExMRAQEBAQEBERERERERERERAQEB
  EgEBAA4PAAAAAAAAAQEBCQEBAQEKDQMDBQoKAQEBCQQEAQEDCg0DEA0MAQEBAAkEAQEBAQoNBQ0I
  AQEBDA8JBAQBAQMKDQoNAQEBAxAPCQQBAQEBCg0IAQEBAwMDDwkEBAEBDQcNAQEBAwMDBQ8JBAEB
  AQENAQEBCgMDAwcPCQQEAQEDAQEBCgsHCAwNDgkEAQEBAQEBAwMFBgcIBQAJBAQBAQEBAwEDAQMB
  AwEAAQQBAQABAQEBAQEBAQEBAQEBAQI=
     ";break;
  case "Maze":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAAAAAA
  ACAAAABBAAAAagAAAIsAAAC0AAAA1QAAAP8AAAAAMAAAIDAAAEEwAABqMAAAizAAALQwAADVMAAA
  /zAAAABlAAAgZQAAQWUAAGplAACLZQAAtGUAANVlAAD/ZQAAAJUAACCVAABBlQAAapUAAIuVAAC0
  lQAA1ZUAAP+VAAAAygAAIMoAAEHKAABqygAAi8oAALTKAADVygAA/8oAAAD/AAAg/wAAQf8AAGr/
  AACL/wAAtP8AANX/AAD//wAAAAA5ACAAOQBBADkAagA5AIsAOQC0ADkA1QA5AP8AOQAAMDkAIDA5
  AEEwOQBqMDkAizA5ALQwOQDVMDkA/zA5AABlOQAgZTkAQWU5AGplOQCLZTkAtGU5ANVlOQD/ZTkA
  AJU5ACCVOQBBlTkAapU5AIuVOQC0lTkA1ZU5AP+VOQAAyjkAIMo5AEHKOQBqyjkAi8o5ALTKOQDV
  yjkA/8o5AAD/OQAg/zkAQf85AGr/OQCL/zkAtP85ANX/OQD//zkAAAB7ACAAewBBAHsAagB7AIsA
  ewC0AHsA1QB7AP8AewAAMHsAIDB7AEEwewBqMHsAizB7ALQwewDVMHsA/zB7AABlewAgZXsAQWV7
  AGplewCLZXsAtGV7ANVlewD/ZXsAAJV7ACCVewBBlXsAapV7AIuVewC0lXsA1ZV7AP+VewAAynsA
  IMp7AEHKewBqynsAi8p7ALTKewDVynsA/8p7AAD/ewAg/3sAQf97AGr/ewCL/3sAtP97ANX/ewD/
  /3sAAAC9ACAAvQBBAL0AagC9AIsAvQC0AL0A1QC9AP8AvQAAML0AIDC9AEEwvQBqML0AizC9ALQw
  vQDVML0A/zC9AABlvQAgZb0AQWW9AGplvQCLZb0AtGW9ANVlvQD/Zb0AAJW9ACCVvQBBlb0AapW9
  AIuVvQC0lb0A1ZW9AP+VvQAAyr0AIMq9AEHKvQBqyr0Ai8q9ALTKvQDVyr0A/8q9AAD/vQAg/70A
  Qf+9AGr/vQCL/70AtP+9ANX/vQD//70AAAD/ACAA/wBBAP8AagD/AIsA/wC0AP8A1QD/AP8A/wAA
  MP8AIDD/AEEw/wBqMP8AizD/ALQw/wDVMP8A/zD/AABl/wAgZf8AQWX/AGpl/wCLZf8AtGX/ANVl
  /wD/Zf8AAJX/ACCV/wBBlf8AapX/AIuV/wC0lf8A1ZX/AP+V/wAAyv8AIMr/AEHK/wBqyv8Ai8r/
  ALTK/wDVyv8A/8r/AAD//wAg//8AQf//AGr//wCL//8AtP//ANX//wD///8A////AObm5gDV1dUA
  xcXFALS0tACkpKQAlJSUAIODgwBzc3MAYmJiAFJSUgBBQUEAMTExACAgIAAQEBAAAAAAAMfxx8fH
  x/U6Cgr58cfHx8fH8/THx/QLQzsMPPvHx8fH8u/3x8f5PHo8ekNE98fHx/jx8fHH+oI8Q0tDRPjH
  x8fv9vn58/uCS0OCejz3x8f183zvhPl8CUFCQ0MK8sfz9Pnv8oJB/PxKEUJC+cfH9e/4vUL8gkH9
  SkFB9vLHx/H2+0L38fqCSkpK9cfHx/Hv8vHxx8fH+0pKgvj39/pC9MfHx8fH9kqzaXpCSkqCvffH
  x8fHx/yCg4OzSvn3QvH4x8fHx8f4tUP5tULzx0rx+sfHx8fH9HtKS4T1x8f4QvXHx8fHx8f6qUJ8
  x8fHx/LHx8fHx8fH86mrx8fHx8fHx8c=
     ";break;
  case "Maze_Disabled":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAA/wD/
  AAAA/wBSUlIAQWU5ANXV1QC0tLQAapV7AEGVOQBqlTkAi8p7AKSkpABzc3MAtMq9AGplOQBiYmIA
  xcXFAObm5gBqynsAasq9AIODgwAgMHsAQZV7AEHKewC0/70AMTExACBlOQAgICAAlJSUAP///wCL
  lXsAIDAAAEFBQQCLMDkAi2U5AGowAABqMDkAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABAQEB
  AQEBAQEBAQEBAQABAQUBAAEiASMBIAEAAQEBARwTAAAOIBUgFQ0hAQEBAQEBEBAAAhYgDQgNAQEB
  AAEBGw4ODx8WCA0WAQEBAAEBAQEcCQ4dHhkDAQEBBAAPAQEcBBYZGBgHAQEBDgAAAQEBAQMYFhka
  AQEBGwQAABABAQMTEAIWAQEBCgAAABABAQEBAAAAAQEBFgsTEwIDBQEBAAAAAQEBFBUDBwcWFwEB
  AQEAAQEBERESBw4TAxALAQEAAQEBDA0ODAMPAAcQAQEBAQEBBQYHCAkKAAALAwoBAQEBAAECAQMB
  AAEAAQQBAQABAQEBAQEBAQEBAQEBAQA=
     ";break;
  case "Mega_Event":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAAAAAA
  ACAAAABBAAAAagAAAIsAAAC0AAAA1QAAAP8AAAAAMAAAIDAAAEEwAABqMAAAizAAALQwAADVMAAA
  /zAAAABlAAAgZQAAQWUAAGplAACLZQAAtGUAANVlAAD/ZQAAAJUAACCVAABBlQAAapUAAIuVAAC0
  lQAA1ZUAAP+VAAAAygAAIMoAAEHKAABqygAAi8oAALTKAADVygAA/8oAAAD/AAAg/wAAQf8AAGr/
  AACL/wAAtP8AANX/AAD//wAAAAA5ACAAOQBBADkAagA5AIsAOQC0ADkA1QA5AP8AOQAAMDkAIDA5
  AEEwOQBqMDkAizA5ALQwOQDVMDkA/zA5AABlOQAgZTkAQWU5AGplOQCLZTkAtGU5ANVlOQD/ZTkA
  AJU5ACCVOQBBlTkAapU5AIuVOQC0lTkA1ZU5AP+VOQAAyjkAIMo5AEHKOQBqyjkAi8o5ALTKOQDV
  yjkA/8o5AAD/OQAg/zkAQf85AGr/OQCL/zkAtP85ANX/OQD//zkAAAB7ACAAewBBAHsAagB7AIsA
  ewC0AHsA1QB7AP8AewAAMHsAIDB7AEEwewBqMHsAizB7ALQwewDVMHsA/zB7AABlewAgZXsAQWV7
  AGplewCLZXsAtGV7ANVlewD/ZXsAAJV7ACCVewBBlXsAapV7AIuVewC0lXsA1ZV7AP+VewAAynsA
  IMp7AEHKewBqynsAi8p7ALTKewDVynsA/8p7AAD/ewAg/3sAQf97AGr/ewCL/3sAtP97ANX/ewD/
  /3sAAAC9ACAAvQBBAL0AagC9AIsAvQC0AL0A1QC9AP8AvQAAML0AIDC9AEEwvQBqML0AizC9ALQw
  vQDVML0A/zC9AABlvQAgZb0AQWW9AGplvQCLZb0AtGW9ANVlvQD/Zb0AAJW9ACCVvQBBlb0AapW9
  AIuVvQC0lb0A1ZW9AP+VvQAAyr0AIMq9AEHKvQBqyr0Ai8q9ALTKvQDVyr0A/8q9AAD/vQAg/70A
  Qf+9AGr/vQCL/70AtP+9ANX/vQD//70AAAD/ACAA/wBBAP8AagD/AIsA/wC0AP8A1QD/AP8A/wAA
  MP8AIDD/AEEw/wBqMP8AizD/ALQw/wDVMP8A/zD/AABl/wAgZf8AQWX/AGpl/wCLZf8AtGX/ANVl
  /wD/Zf8AAJX/ACCV/wBBlf8AapX/AIuV/wC0lf8A1ZX/AP+V/wAAyv8AIMr/AEHK/wBqyv8Ai8r/
  ALTK/wDVyv8A/8r/AAD//wAg//8AQf//AGr//wCL//8AtP//ANX//wD///8A////AObm5gDV1dUA
  xcXFALS0tACkpKQAlJSUAIODgwBzc3MAYmJiAFJSUgBBQUEAMTExACAgIAAQEBAAAAAAAMfHx8fH
  x8fHx8fHx8fHx8fHx8fHx8fHx8fH/MfHx8fHx8fHx8fHx8fHx/77x8fHx8fHx8fHx/n7+/n6+/3H
  x8fHx8f6+fv5+Pf59vT9x8fHx/v8+fPx8fHx8fHx9/r8x8f59vHx8e/v7/Os8vH0+/z8+/Sj5uXx
  5fGso6Py8/f7+mqskK2jmaOsmZqamqKi+vqjpJCso5GampGtmqyao/r4c6Oko+bdo+aso6zdo636
  +vuG8e/v7+/v7+/v8fH4/Mf4+4a38e/v7+/xtrb5/MfHx/v7+YWGtraGhvg6+cfHx8fHx/vH+vr6
  +vz8OsfHx8fHx8fHx8fHx8fHx8fHx8c=
     ";break;
  case "Mega_Event_Disabled":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAA/wD/
  AAAA/wBSUlIAMTExAGJiYgC0ynsA1cp7ANXKvQBzc3MAQTA5AObm5gD///8AamV7AGplvQCLZb0A
  1cr/AIuVvQC0lf8AAAC9AEEwvQAgAL0AtJW9AEEwewAgML0AQWW9ALS0tAC0yv8A1dXVAMXFxQCD
  g4MAlJSUAEFBQQAQEBAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABAQEB
  AQEBAQEBAQEBAQABAQABAAEAAQABAwEAAQEBAQAAAAAAAAAAACAfAQEBAQEBAAAAAAQfHwQCAQEB
  AAEBAAACBB8ECB0EAQEBAAEBAQEDBBwKCgoKAQEBHQIDAQEEHgoKCgsLAQEBGwoZAQEBARkNDxoK
  AQEBDQ0bHB0BARYQEhUNAQEBFxMTExgBAQEBDhIQAQEBExQVExATDQEBDA0OAQEBDQ8QDRARDQEB
  AQEGAQEBCwsLCwsLCgoIAQEIAQEBCgsLCwsKBwcEAQEBAQEBBAUGBwcGBggJBAABAQEBAAEAAQIB
  AgEDAQABAQABAQEBAQEBAQEBAQEBAQA=
     ";break;
  case "Micro":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAAAAAA
  ACAAAABBAAAAagAAAIsAAAC0AAAA1QAAAP8AAAAAMAAAIDAAAEEwAABqMAAAizAAALQwAADVMAAA
  /zAAAABlAAAgZQAAQWUAAGplAACLZQAAtGUAANVlAAD/ZQAAAJUAACCVAABBlQAAapUAAIuVAAC0
  lQAA1ZUAAP+VAAAAygAAIMoAAEHKAABqygAAi8oAALTKAADVygAA/8oAAAD/AAAg/wAAQf8AAGr/
  AACL/wAAtP8AANX/AAD//wAAAAA5ACAAOQBBADkAagA5AIsAOQC0ADkA1QA5AP8AOQAAMDkAIDA5
  AEEwOQBqMDkAizA5ALQwOQDVMDkA/zA5AABlOQAgZTkAQWU5AGplOQCLZTkAtGU5ANVlOQD/ZTkA
  AJU5ACCVOQBBlTkAapU5AIuVOQC0lTkA1ZU5AP+VOQAAyjkAIMo5AEHKOQBqyjkAi8o5ALTKOQDV
  yjkA/8o5AAD/OQAg/zkAQf85AGr/OQCL/zkAtP85ANX/OQD//zkAAAB7ACAAewBBAHsAagB7AIsA
  ewC0AHsA1QB7AP8AewAAMHsAIDB7AEEwewBqMHsAizB7ALQwewDVMHsA/zB7AABlewAgZXsAQWV7
  AGplewCLZXsAtGV7ANVlewD/ZXsAAJV7ACCVewBBlXsAapV7AIuVewC0lXsA1ZV7AP+VewAAynsA
  IMp7AEHKewBqynsAi8p7ALTKewDVynsA/8p7AAD/ewAg/3sAQf97AGr/ewCL/3sAtP97ANX/ewD/
  /3sAAAC9ACAAvQBBAL0AagC9AIsAvQC0AL0A1QC9AP8AvQAAML0AIDC9AEEwvQBqML0AizC9ALQw
  vQDVML0A/zC9AABlvQAgZb0AQWW9AGplvQCLZb0AtGW9ANVlvQD/Zb0AAJW9ACCVvQBBlb0AapW9
  AIuVvQC0lb0A1ZW9AP+VvQAAyr0AIMq9AEHKvQBqyr0Ai8q9ALTKvQDVyr0A/8q9AAD/vQAg/70A
  Qf+9AGr/vQCL/70AtP+9ANX/vQD//70AAAD/ACAA/wBBAP8AagD/AIsA/wC0AP8A1QD/AP8A/wAA
  MP8AIDD/AEEw/wBqMP8AizD/ALQw/wDVMP8A/zD/AABl/wAgZf8AQWX/AGpl/wCLZf8AtGX/ANVl
  /wD/Zf8AAJX/ACCV/wBBlf8AapX/AIuV/wC0lf8A1ZX/AP+V/wAAyv8AIMr/AEHK/wBqyv8Ai8r/
  ALTK/wDVyv8A/8r/AAD//wAg//8AQf//AGr//wCL//8AtP//ANX//wD///8A////AObm5gDV1dUA
  xcXFALS0tACkpKQAlJSUAIODgwBzc3MAYmJiAFJSUgBBQUEAMTExACAgIAAQEBAAAAAAAMfHx8f0
  +v39/f369cfHx8fHx8fH+fz9/f39/Pvxx8fHx8fHx/k6/P39/Pz68cfHx8fHx8f5Ovz9/fz8+vHH
  x8fHx8fH+fz8/f38/Prxx8fHx8fHx/n8/P39/Pz68cfHx8fHx/H6/Pz9/fz8+vLHx8fHx8fx+vz9
  /f39/Pryx8fHx8fH8fr8/f39/fz78sfHx8fHx/H6/P39/f38+/PHx8fHx8fy+vz9/f39/Pvzx8fH
  x8fH8vv8/f39/fz79MfHx8fHx/P7/Pz8/Pz8+/THx8fHx8f0+Pj39/f3+Pj1x8fHx8fH9PT09PT0
  9PT09cfHx8fHx/Hy8/T09PTz8/HHx8c=
     ";break;
  case "Micro_Disabled":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAA/wD/
  AAAA/wC0tLQAc3NzAIODgwCkpKQAMTExAEFBQQAgICAA1dXVAMXFxQDm5uYAUlJSAGJiYgBBMDkA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABAQEB
  AQEBAQEBAQEBAQABAQABDQEIAQgBBgELAQEBAQAAAA0OBggIBgYMAQEBAQEBAAANDgYICAYGAQEB
  AAEBAAAADQYGCAgGAQEBAAEBAQEAAA0GBggIAQEBCwAAAQEAAAsMBgYIAQEBDAkAAQEBAQALDAYI
  AQEBBgwJAAABAQAACwwGAQEBCAYHCQABAQEBAAsMAQEBCAgGBwoAAAEBAAAJAQEBCAgIBgcKAAEB
  AQEAAQEBCAgICAYHAgAAAQEAAQEBBgYGBgYGBwIAAQEBAQEBAwMEBAQEAwMFAAABAQEBAgECAQIB
  AgECAQABAQABAQEBAQEBAQEBAQEBAQA=
     ";break;
  case "Multi":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAAAAAA
  ACAAAABBAAAAagAAAIsAAAC0AAAA1QAAAP8AAAAAMAAAIDAAAEEwAABqMAAAizAAALQwAADVMAAA
  /zAAAABlAAAgZQAAQWUAAGplAACLZQAAtGUAANVlAAD/ZQAAAJUAACCVAABBlQAAapUAAIuVAAC0
  lQAA1ZUAAP+VAAAAygAAIMoAAEHKAABqygAAi8oAALTKAADVygAA/8oAAAD/AAAg/wAAQf8AAGr/
  AACL/wAAtP8AANX/AAD//wAAAAA5ACAAOQBBADkAagA5AIsAOQC0ADkA1QA5AP8AOQAAMDkAIDA5
  AEEwOQBqMDkAizA5ALQwOQDVMDkA/zA5AABlOQAgZTkAQWU5AGplOQCLZTkAtGU5ANVlOQD/ZTkA
  AJU5ACCVOQBBlTkAapU5AIuVOQC0lTkA1ZU5AP+VOQAAyjkAIMo5AEHKOQBqyjkAi8o5ALTKOQDV
  yjkA/8o5AAD/OQAg/zkAQf85AGr/OQCL/zkAtP85ANX/OQD//zkAAAB7ACAAewBBAHsAagB7AIsA
  ewC0AHsA1QB7AP8AewAAMHsAIDB7AEEwewBqMHsAizB7ALQwewDVMHsA/zB7AABlewAgZXsAQWV7
  AGplewCLZXsAtGV7ANVlewD/ZXsAAJV7ACCVewBBlXsAapV7AIuVewC0lXsA1ZV7AP+VewAAynsA
  IMp7AEHKewBqynsAi8p7ALTKewDVynsA/8p7AAD/ewAg/3sAQf97AGr/ewCL/3sAtP97ANX/ewD/
  /3sAAAC9ACAAvQBBAL0AagC9AIsAvQC0AL0A1QC9AP8AvQAAML0AIDC9AEEwvQBqML0AizC9ALQw
  vQDVML0A/zC9AABlvQAgZb0AQWW9AGplvQCLZb0AtGW9ANVlvQD/Zb0AAJW9ACCVvQBBlb0AapW9
  AIuVvQC0lb0A1ZW9AP+VvQAAyr0AIMq9AEHKvQBqyr0Ai8q9ALTKvQDVyr0A/8q9AAD/vQAg/70A
  Qf+9AGr/vQCL/70AtP+9ANX/vQD//70AAAD/ACAA/wBBAP8AagD/AIsA/wC0AP8A1QD/AP8A/wAA
  MP8AIDD/AEEw/wBqMP8AizD/ALQw/wDVMP8A/zD/AABl/wAgZf8AQWX/AGpl/wCLZf8AtGX/ANVl
  /wD/Zf8AAJX/ACCV/wBBlf8AapX/AIuV/wC0lf8A1ZX/AP+V/wAAyv8AIMr/AEHK/wBqyv8Ai8r/
  ALTK/wDVyv8A/8r/AAD//wAg//8AQf//AGr//wCL//8AtP//ANX//wD///8A////AObm5gDV1dUA
  xcXFALS0tACkpKQAlJSUAIODgwBzc3MAYmJiAFJSUgBBQUEAMTExACAgIAAQEBAAAAAAAMfHx8fH
  x8fHx8fHx8fHx8fv8fLy8vLxx8fHx8fHx8fH8/f09PT0+vXvx8fHx8fHx/fv8vLx7/fz+PLHx8fH
  x8f39vj38+/37+/29/HHx8fH+DmoqEH2+O/v7/H49MfHx3JwcHBwcDj38fHx8ffxx8f1OXioqKhw
  cHH28vH09+/Hx/H5cdjQ0NhwcHL09fr48cfH9vL4qdjQ0KhwcPv68fbHx/bx7/NyqKioqXA4/fH2
  x/H6+Pj4+Ps5OTk5Of309sfzcHBwcHBwcHBwcHA4cPnH7/RzcahwcHBwcHBwcDg4x8fHx/Hy8vLy
  8vLy8vLy8sfHx8fHx8fHx8fHx8fHx8c=
     ";break;
  case "Multi_Disabled":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAA/wD/
  AAAA/wDV1dUAIGV7AACVvQAAZXsAADA5AMXFxQBSUlIAc3NzAEFBQQAgMDkAICAgALS0tACUlJQA
  5ubmACCVvQAAZf8AYmJiAACV/wBBZXsApKSkAACVewCDg4MA////ACBlOQAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABAQEB
  AQEBAQEBAQEBAQABAQIBAgEPAQABAAEAAQEBARcNDQ0NCBUYAAAAAQEBAQEBAgIPGBcHCQIAAQEB
  AAEBDgkXBxgXGBgOAQEBAAEBAQEEBBkOCRgYAQEBDQAAAQEFBQUFBQYXAQEBDxcPAQEBARYEBAQF
  AQEBAg8NFxgBAQ8SAxMRAQEBBRQNFQgBAQEBDgIJAQEBEQQFBQoIDwEBAA4PAQEBBAQEEAUGDAEB
  AQEIAQEBCQoLCwsLCwwNAQEHAQEBBQUFBQUFBQUGAQEBAQEBAwQFBQUFBQUFBQYBAQEBAAECAQIB
  AgECAQIBAQABAQEBAQEBAQEBAQEBAQA=
     ";break;
  case "Multi_Solved":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAA/wD/
  AObm5gDV1dUA////ALS0tABqZXsAIGV7AACVvQAAZXsAADA5AMXFxQBiYmIAUlJSAHNzcwBBQUEA
  IDA5ACAgIACUlJQAQWV7ACCVvQAAlf8AAGX/ACDKAACkpKQAAJV7ACBlOQCDg4MAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAFgAAAAAAAAADAQICAgIBFhYWAAAAAAAAChoEBAQEFhYWFhYAAAAAABoDAgIBAxYWFhYWFgAA
  AAAaEQ0aChYWFhYWFhYWAAAADQ8HBxkWFhYDAxYWFhYAABIICAgICBYWAQEBFhYWFgAXDxgHBwcI
  CAYRAgEWFhYWAAELBhQVFRQICBIEFxYWFgAAEQINExQVFQcICA4MFhEAABEBAwoSBwcHEwgJEAER
  AAEMDQ0NDQ4PDw8PDxAEEQAKCAgICAgICAgICAgJCAsAAwQFBgcICAgICAgICAkJAAAAAAECAgIC
  AgICAgICAgAAAAAAAAAAAAAAAAAAAAA=
     ";break;
  case "Mystery":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAAAAAA
  ACAAAABBAAAAagAAAIsAAAC0AAAA1QAAAP8AAAAAMAAAIDAAAEEwAABqMAAAizAAALQwAADVMAAA
  /zAAAABlAAAgZQAAQWUAAGplAACLZQAAtGUAANVlAAD/ZQAAAJUAACCVAABBlQAAapUAAIuVAAC0
  lQAA1ZUAAP+VAAAAygAAIMoAAEHKAABqygAAi8oAALTKAADVygAA/8oAAAD/AAAg/wAAQf8AAGr/
  AACL/wAAtP8AANX/AAD//wAAAAA5ACAAOQBBADkAagA5AIsAOQC0ADkA1QA5AP8AOQAAMDkAIDA5
  AEEwOQBqMDkAizA5ALQwOQDVMDkA/zA5AABlOQAgZTkAQWU5AGplOQCLZTkAtGU5ANVlOQD/ZTkA
  AJU5ACCVOQBBlTkAapU5AIuVOQC0lTkA1ZU5AP+VOQAAyjkAIMo5AEHKOQBqyjkAi8o5ALTKOQDV
  yjkA/8o5AAD/OQAg/zkAQf85AGr/OQCL/zkAtP85ANX/OQD//zkAAAB7ACAAewBBAHsAagB7AIsA
  ewC0AHsA1QB7AP8AewAAMHsAIDB7AEEwewBqMHsAizB7ALQwewDVMHsA/zB7AABlewAgZXsAQWV7
  AGplewCLZXsAtGV7ANVlewD/ZXsAAJV7ACCVewBBlXsAapV7AIuVewC0lXsA1ZV7AP+VewAAynsA
  IMp7AEHKewBqynsAi8p7ALTKewDVynsA/8p7AAD/ewAg/3sAQf97AGr/ewCL/3sAtP97ANX/ewD/
  /3sAAAC9ACAAvQBBAL0AagC9AIsAvQC0AL0A1QC9AP8AvQAAML0AIDC9AEEwvQBqML0AizC9ALQw
  vQDVML0A/zC9AABlvQAgZb0AQWW9AGplvQCLZb0AtGW9ANVlvQD/Zb0AAJW9ACCVvQBBlb0AapW9
  AIuVvQC0lb0A1ZW9AP+VvQAAyr0AIMq9AEHKvQBqyr0Ai8q9ALTKvQDVyr0A/8q9AAD/vQAg/70A
  Qf+9AGr/vQCL/70AtP+9ANX/vQD//70AAAD/ACAA/wBBAP8AagD/AIsA/wC0AP8A1QD/AP8A/wAA
  MP8AIDD/AEEw/wBqMP8AizD/ALQw/wDVMP8A/zD/AABl/wAgZf8AQWX/AGpl/wCLZf8AtGX/ANVl
  /wD/Zf8AAJX/ACCV/wBBlf8AapX/AIuV/wC0lf8A1ZX/AP+V/wAAyv8AIMr/AEHK/wBqyv8Ai8r/
  ALTK/wDVyv8A/8r/AAD//wAg//8AQf//AGr//wCL//8AtP//ANX//wD///8A////AObm5gDV1dUA
  xcXFALS0tACkpKQAlJSUAIODgwBzc3MAYmJiAFJSUgBBQUEAMTExACAgIAAQEBAAAAAAAMfHx8fH
  x/UB/vLHx8fHx8fHx8fHx/IADXT9x8fHx8fHx8fHx8f3Ag2u/sfHx8fHx8fHx8fH8QAN+f3Hx8fH
  x8fHx8fHx8fH9/HHx8fHx8fHx8fHx8fHxwz5x8fHx8fHx8fHx8fHx8cC/cfHx8fHx8fHx8fHx8fH
  9wwAx8fHx8fHx8fHx/H39Mf8AwL8x8fHx8fHx8f+Av7H8v4NA/LHx8fHx/H+DT6u9MfHDQ1Dx8fH
  x8f1AQ19/cfHxwwNfvXHx8fH9wMNAPLHx/YNDXb3x8fHx/EADQ0CxwIMDQ198cfHx8fH9w0NDQ0N
  DQ1G/sfHx8fHx8f3AgwNDUX8/sfHx8c=
     ";break;
  case "Mystery_Disabled":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAA/wD/
  AAAA/wCDg4MAtDAAANVlOQBBAAAAizAAALSVewDm5uYAAAAAANXV1QCUlJQA1WV7AKSkpAAgICAA
  1ZV7ABAQEAC0tLQAamU5AGoAAAAxMTEAYmJiANWVvQCLZXsAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABAQEB
  AQEBAQEBAQEBAQABAQABAAEJARcBAAEAAQEBAQAAAAACBQMWEAAAAQEBAQEBAAAACAkDFQ4AAQEB
  AAEBAAAAAAAAAggAAQEBAAEBAQEAAAAAAAYVAQEBAAAAAQEAAAAAAAAFAQEBAAAAAQEBAQAAAAAA
  AQEBAAAAAAABAQAAAAgCAQEBEwUUAAABAQEBAAAQAQEBChADEwoAAAEBAAgQAQEBEQAAAwMSAAEB
  AQENAQEBDgAAAAYDDw0AAQEAAQEBCQoAAAsDAwwCAQEBAQEBAwMFAAUGAwMHCAABAQEBAgEDAQMB
  AwEEAQABAQABAQEBAQEBAQEBAQEBAQA=
     ";break;
  case "Mystery_Solved":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAA/wD/
  AIODgwBBAAAAizAAALQwAAC0ZTkAMTExABAQEADVZTkA5ubmAAAAAAC0lXsAagAAANXV1QCUlJQA
  1WV7AKSkpAAgAAAAICAgANWVewDVMDkA1ZW9ALS0tABqZTkAIMoAAGJiYgAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  ABARGA0AAAAAAAAAAAAAAA0KGBgYAAAAAAAAAAAAAAABGBgYGBgAAAAAAAAAAAAACRgYGBgYGAAA
  AAAAAAAAABgYGBgYGBgYAAAAAAAAAAAYGBgZABgYGBgAAAAAAAAAABgYEgAAGBgYGAAAAAAAAAAA
  AQMKAAAYGBgYAAAAAAkBFgAGDAIGABgYGAAAAAAHAgcADQcEDA0AGAAAAAkHBBQVFgAABAQXAAAA
  AAAQEQQLEgAAAAMEExAAAAAAAQwECg0AAA4EBA8BAAAAAAkKBAQCAAIDBAQLCQAAAAAAAQQEBAQE
  BAQIBwAAAAAAAAABAgMEBAUGBwAAAAA=
     ";break;
  case "Parking_Area":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAAAAAA
  ACAAAABBAAAAagAAAIsAAAC0AAAA1QAAAP8AAAAAMAAAIDAAAEEwAABqMAAAizAAALQwAADVMAAA
  /zAAAABlAAAgZQAAQWUAAGplAACLZQAAtGUAANVlAAD/ZQAAAJUAACCVAABBlQAAapUAAIuVAAC0
  lQAA1ZUAAP+VAAAAygAAIMoAAEHKAABqygAAi8oAALTKAADVygAA/8oAAAD/AAAg/wAAQf8AAGr/
  AACL/wAAtP8AANX/AAD//wAAAAA5ACAAOQBBADkAagA5AIsAOQC0ADkA1QA5AP8AOQAAMDkAIDA5
  AEEwOQBqMDkAizA5ALQwOQDVMDkA/zA5AABlOQAgZTkAQWU5AGplOQCLZTkAtGU5ANVlOQD/ZTkA
  AJU5ACCVOQBBlTkAapU5AIuVOQC0lTkA1ZU5AP+VOQAAyjkAIMo5AEHKOQBqyjkAi8o5ALTKOQDV
  yjkA/8o5AAD/OQAg/zkAQf85AGr/OQCL/zkAtP85ANX/OQD//zkAAAB7ACAAewBBAHsAagB7AIsA
  ewC0AHsA1QB7AP8AewAAMHsAIDB7AEEwewBqMHsAizB7ALQwewDVMHsA/zB7AABlewAgZXsAQWV7
  AGplewCLZXsAtGV7ANVlewD/ZXsAAJV7ACCVewBBlXsAapV7AIuVewC0lXsA1ZV7AP+VewAAynsA
  IMp7AEHKewBqynsAi8p7ALTKewDVynsA/8p7AAD/ewAg/3sAQf97AGr/ewCL/3sAtP97ANX/ewD/
  /3sAAAC9ACAAvQBBAL0AagC9AIsAvQC0AL0A1QC9AP8AvQAAML0AIDC9AEEwvQBqML0AizC9ALQw
  vQDVML0A/zC9AABlvQAgZb0AQWW9AGplvQCLZb0AtGW9ANVlvQD/Zb0AAJW9ACCVvQBBlb0AapW9
  AIuVvQC0lb0A1ZW9AP+VvQAAyr0AIMq9AEHKvQBqyr0Ai8q9ALTKvQDVyr0A/8q9AAD/vQAg/70A
  Qf+9AGr/vQCL/70AtP+9ANX/vQD//70AAAD/ACAA/wBBAP8AagD/AIsA/wC0AP8A1QD/AP8A/wAA
  MP8AIDD/AEEw/wBqMP8AizD/ALQw/wDVMP8A/zD/AABl/wAgZf8AQWX/AGpl/wCLZf8AtGX/ANVl
  /wD/Zf8AAJX/ACCV/wBBlf8AapX/AIuV/wC0lf8A1ZX/AP+V/wAAyv8AIMr/AEHK/wBqyv8Ai8r/
  ALTK/wDVyv8A/8r/AAD//wAg//8AQf//AGr//wCL//8AtP//ANX//wD///8A////AObm5gDV1dUA
  xcXFALS0tACkpKQAlJSUAIODgwBzc3MAYmJiAFJSUgBBQUEAMTExACAgIAAQEBAAAAAAAMfHx8fH
  x8fHx8fHx8fHx8fHx/L18sfHx8fHx/L18cfHx/H49/jz8vPz8vT5+Pfxx/Jz+e/4qqurq6ty9+/5
  q/Gsq/j49+3l4+PitPj4q6qrrOPk4+Tk46rj5OPk4+SqqrSpsuKysrL39POysqqqqvHx5bWpq4d+
  hod9qavy8e/Hx8fHtKm3tra/q6rxx8fHx8fHx/Gqqaqqqqm1x8fHx8fHx8fH8eS0tLTlx8fHx8fH
  x8fHx8fHx8fHx8fHx8fHx8fHx8fHx8fHx8fHx8fHx8fHx8fHx8fHx8fHx8fHx8fHx8fHx8fHx8fH
  x8fHx8fHx8fHx8fHx8fHx8fHx8fHx8c=
     ";break;
  case "Parking_Area_Disabled":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAA/wD/
  AAAA/wCLyr0AtMr/AObm5gBBlb0AIJW9ALTKvQD/yr0AapW9AP/KewDVlXsA1dXVAP///wBByr0A
  Qcr/AIODgwCLyv8Aasr/AHNzcwC0//8AYmJiAEFlewDFxcUAtLS0AKSkpAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABAQEB
  AQEBAQEBAQEBAQABAQwBDAEAAQABAAEZAQEBAQQTEBMXDBcXDBgVAQEBAQEBFQ0TBQkJCQkWAQEB
  CQEBCRMTEBQDEhIPAQEBCQEBAQEREhEREgUSAQEBEhEFAQEGDg8ODg4QAQEBDgUFAQEBAQcGCQoL
  AQEBBgkMBA0BAQAAAgYIAQEBCQUEAAABAQEBAAQFAQEBBQYHAAAAAAEBAAAAAQEBAgIDAAAAAAEB
  AQEAAQEBAAAAAAAAAAAAAQEAAQEBAAAAAAAAAAAAAQEBAQEBAAAAAAAAAAAAAAABAQEBAAEAAQAB
  AAEAAQABAQABAQEBAQEBAQEBAQEBAQA=
     ";break;
  case "Parking_Area_Found":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAA/wD/
  AObm5gCLyv8Ai8q9ALTK/wBBlb0AIJW9ALTKvQAgygAA/8q9ANXKvQD//70AapW9AP/KewDVlXsA
  1cp7ALSVewBByr0AQcr/ALS0tADFxcUAi5W9AGrK/wBzc3MAg4ODANXV1QBqZXsAYmJiAP///wCk
  pKQAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAACAAAAAAAAAAAABkdGQAACAgIABkdAQAAAAEXGBcUCAgICAgbFxgBABkaGxwXBQgICAgICBwb
  DAEVDBcXGAgICAgICAgIDAUMFRYCFgIICAgWAggICAgFBQMGERIREQgIExQRCAgICAEBBAcGDA0O
  Dw0QBgwICAgIAAAAAwYJCgoLDAUBAAgICAAAAAEFBgUFBQYHAAAACAAAAAAAAQIDAwMEAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=
     ";break;
  case "Project_APE":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAAAAAA
  ACAAAABBAAAAagAAAIsAAAC0AAAA1QAAAP8AAAAAMAAAIDAAAEEwAABqMAAAizAAALQwAADVMAAA
  /zAAAABlAAAgZQAAQWUAAGplAACLZQAAtGUAANVlAAD/ZQAAAJUAACCVAABBlQAAapUAAIuVAAC0
  lQAA1ZUAAP+VAAAAygAAIMoAAEHKAABqygAAi8oAALTKAADVygAA/8oAAAD/AAAg/wAAQf8AAGr/
  AACL/wAAtP8AANX/AAD//wAAAAA5ACAAOQBBADkAagA5AIsAOQC0ADkA1QA5AP8AOQAAMDkAIDA5
  AEEwOQBqMDkAizA5ALQwOQDVMDkA/zA5AABlOQAgZTkAQWU5AGplOQCLZTkAtGU5ANVlOQD/ZTkA
  AJU5ACCVOQBBlTkAapU5AIuVOQC0lTkA1ZU5AP+VOQAAyjkAIMo5AEHKOQBqyjkAi8o5ALTKOQDV
  yjkA/8o5AAD/OQAg/zkAQf85AGr/OQCL/zkAtP85ANX/OQD//zkAAAB7ACAAewBBAHsAagB7AIsA
  ewC0AHsA1QB7AP8AewAAMHsAIDB7AEEwewBqMHsAizB7ALQwewDVMHsA/zB7AABlewAgZXsAQWV7
  AGplewCLZXsAtGV7ANVlewD/ZXsAAJV7ACCVewBBlXsAapV7AIuVewC0lXsA1ZV7AP+VewAAynsA
  IMp7AEHKewBqynsAi8p7ALTKewDVynsA/8p7AAD/ewAg/3sAQf97AGr/ewCL/3sAtP97ANX/ewD/
  /3sAAAC9ACAAvQBBAL0AagC9AIsAvQC0AL0A1QC9AP8AvQAAML0AIDC9AEEwvQBqML0AizC9ALQw
  vQDVML0A/zC9AABlvQAgZb0AQWW9AGplvQCLZb0AtGW9ANVlvQD/Zb0AAJW9ACCVvQBBlb0AapW9
  AIuVvQC0lb0A1ZW9AP+VvQAAyr0AIMq9AEHKvQBqyr0Ai8q9ALTKvQDVyr0A/8q9AAD/vQAg/70A
  Qf+9AGr/vQCL/70AtP+9ANX/vQD//70AAAD/ACAA/wBBAP8AagD/AIsA/wC0AP8A1QD/AP8A/wAA
  MP8AIDD/AEEw/wBqMP8AizD/ALQw/wDVMP8A/zD/AABl/wAgZf8AQWX/AGpl/wCLZf8AtGX/ANVl
  /wD/Zf8AAJX/ACCV/wBBlf8AapX/AIuV/wC0lf8A1ZX/AP+V/wAAyv8AIMr/AEHK/wBqyv8Ai8r/
  ALTK/wDVyv8A/8r/AAD//wAg//8AQf//AGr//wCL//8AtP//ANX//wD///8A////AObm5gDV1dUA
  xcXFALS0tACkpKQAlJSUAIODgwBzc3MAYmJiAFJSUgBBQUEAMTExACAgIAAQEBAAAAAAAMfH8Xx7
  e3t7e3t7e3t7fLXH8YN7hISEhISEhISEhIS1x/OCe4S0tLSEhISEhISEtcfzgnuEhISEhISEhISE
  hLXH84J7hISEhISEhISEhIS1x/OCe4R7hHuEe4SEe3uEtcfzgnuEe4R7hHuEhHyEhLXH84J7hHt7
  e4R7e4R7e4S1x/OCe4R7hHuEe4R7e4SEtcfzgnuEe3t7hHt7hHx7hLXH84J7hISEhISEhISEhIS1
  x/OCe4SEhISEhISEhISEtcfzgnuEhISEhISEhISEhLXH84J7e3t7e3t7e3t7e3u1x/OCgoKCgoKC
  goKCgoJ78cfze4KCgoKCgoKCgoJ78cc=
     ";break;
  case "Project_APE_Disabled":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAA/wD/
  AAAA/wBBynsAapV7AMXFxQCLynsAi5V7AIvKvQBqynsAtMq9AAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABAQEB
  AQEBAQEBAQEBAQkBAQgBBQEFAQUBBQEFAQEBAQQCAwUHBwcFBQUFAQEBAQEBAgMFBQUFBQUFAQEB
  BQEBBAIDBQUFBQUFAQEBBQEBAQECAwUDBQMFAQEBAwMFAQEEAgMFAwUDAQEBBQYFAQEBAQIDBQMD
  AQEBAwUDAwUBAQQCAwUDAQEBAwUDAwUBAQEBAgMFAQEBBQMDBQYDBQEBBAIDAQEBBQUFBQUFBQEB
  AQECAQEBBQUFBQUFBQUFAQEEAQEBBQUFBQUFBQUFAQEBAQEBAwMDAwMDAwMDAwMBAQEBAgECAQIB
  AgECAQIBAQABAQEBAQEBAQEBAQEBAQA=
     ";break;
  case "Question_to_Answer":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAAAAAA
  ACAAAABBAAAAagAAAIsAAAC0AAAA1QAAAP8AAAAAMAAAIDAAAEEwAABqMAAAizAAALQwAADVMAAA
  /zAAAABlAAAgZQAAQWUAAGplAACLZQAAtGUAANVlAAD/ZQAAAJUAACCVAABBlQAAapUAAIuVAAC0
  lQAA1ZUAAP+VAAAAygAAIMoAAEHKAABqygAAi8oAALTKAADVygAA/8oAAAD/AAAg/wAAQf8AAGr/
  AACL/wAAtP8AANX/AAD//wAAAAA5ACAAOQBBADkAagA5AIsAOQC0ADkA1QA5AP8AOQAAMDkAIDA5
  AEEwOQBqMDkAizA5ALQwOQDVMDkA/zA5AABlOQAgZTkAQWU5AGplOQCLZTkAtGU5ANVlOQD/ZTkA
  AJU5ACCVOQBBlTkAapU5AIuVOQC0lTkA1ZU5AP+VOQAAyjkAIMo5AEHKOQBqyjkAi8o5ALTKOQDV
  yjkA/8o5AAD/OQAg/zkAQf85AGr/OQCL/zkAtP85ANX/OQD//zkAAAB7ACAAewBBAHsAagB7AIsA
  ewC0AHsA1QB7AP8AewAAMHsAIDB7AEEwewBqMHsAizB7ALQwewDVMHsA/zB7AABlewAgZXsAQWV7
  AGplewCLZXsAtGV7ANVlewD/ZXsAAJV7ACCVewBBlXsAapV7AIuVewC0lXsA1ZV7AP+VewAAynsA
  IMp7AEHKewBqynsAi8p7ALTKewDVynsA/8p7AAD/ewAg/3sAQf97AGr/ewCL/3sAtP97ANX/ewD/
  /3sAAAC9ACAAvQBBAL0AagC9AIsAvQC0AL0A1QC9AP8AvQAAML0AIDC9AEEwvQBqML0AizC9ALQw
  vQDVML0A/zC9AABlvQAgZb0AQWW9AGplvQCLZb0AtGW9ANVlvQD/Zb0AAJW9ACCVvQBBlb0AapW9
  AIuVvQC0lb0A1ZW9AP+VvQAAyr0AIMq9AEHKvQBqyr0Ai8q9ALTKvQDVyr0A/8q9AAD/vQAg/70A
  Qf+9AGr/vQCL/70AtP+9ANX/vQD//70AAAD/ACAA/wBBAP8AagD/AIsA/wC0AP8A1QD/AP8A/wAA
  MP8AIDD/AEEw/wBqMP8AizD/ALQw/wDVMP8A/zD/AABl/wAgZf8AQWX/AGpl/wCLZf8AtGX/ANVl
  /wD/Zf8AAJX/ACCV/wBBlf8AapX/AIuV/wC0lf8A1ZX/AP+V/wAAyv8AIMr/AEHK/wBqyv8Ai8r/
  ALTK/wDVyv8A/8r/AAD//wAg//8AQf//AGr//wCL//8AtP//ANX//wD///8A////AObm5gDV1dUA
  xcXFALS0tACkpKQAlJSUAIODgwBzc3MAYmJiAFJSUgBBQUEAMTExACAgIAAQEBAAAAAAALXz8/Pz
  8/Pz8/O18/Pz8/Pzx8fHx/Hxx8fxtbXHx8fz88fHx/H2e/XytbS0tMfH8/PH8vW1e720e3vu7vHH
  x/Pz8faze7Pt7b20vb20tcfz8/K07bS17e297fK9tHvy8/Pxtb297b29vb29vb29x/Pzx/K0tL29
  vLy8vb29vcfz88fzerO0vLS0vLTx8fHH8/PHtLOztLSzs7Oze/LHx/Pzx/G0s7KzsrOzsrLux8fz
  88fH8YOCgoKCgoK0x8fH8/PHx/F7eIGCs3mz7sfHx7Xzx8fxs3h4s8fx8cfHx8fz88fHx8fy8sfH
  x8fHx8fH8/Pz87Xz87Xz8/PztfPz8/M=
     ";break;
  case "Question_to_Answer_Disabled":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAAxcXF
  AAAA/wD/AP8A1dXVAGrKvQAAlXsA5ubmACDKewBBynsAIJV7ANX//wCLyr0AQcq9AGqVewBBlXsA
  i/+9ALT/vQC0yr0AtP//AJSUlACkpKQAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABEBAQEB
  AQEBAQEBAQEBAQABAQIBAgEGAQIBEQECAQEBAQICAgYTDRQDEQsLAQEBAQEBAxQRDRALDQ0KAQEB
  AgEBBhMEDQQSEhALAQEBEQEBAQELEgsREhIQAQEBCw0DAQEGERAQEhAQAQEBEBAQAQEBAQMLCxAQ
  AQEBEBAQEAIBAQIADgQLAQEBDwsGBgYBAQEBCwQEAQEBBAQEDQMCAgEBAgYLAQEBDAQEDAwKAgEB
  AQECAQEBCAgICAgLAgICAQECAQEBBQcIBAkECgICAQEBAQEBBAUFBAIGBgICAgIBAQEBAgEDAQIB
  AgECAQIBAQABAQEBAQEBAQEBAQEBAQA=
     ";break;
  case "Question_to_Answer_Found":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAAxcXF
  ALTKvQD/AP8A1dXVAObm5gBqyr0AAJV7AGqVewAgynsAQcp7ACCVewDV//8Aasp7AIvKvQBByr0A
  IMoAAEGVewCL/70AtP+9ALT//wCUlJQApKSkAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAEAAAAA
  AAAADwABAAAAAAAAAgICAgQEDw8PAQECAgIAAAICAgQUDw8PDw8NDQICAAACAxUBBw8PDw8PDwQC
  AgAABBQFBw8PDw8PDw8PAQIAAAMNEw0PDw8SEw8PDw8DAAAEARISEw8PEhISDw8PDwAAAgMNDRIS
  EREREhIPDw8PAAIAEAUNEQ0NEQ0EBA8PDwACDQUFDQ0FBQUFBwMCDwAAAgQNBQ4FDgUFDg4LAgIA
  AAICBAwJCQkJCQkNAgICAAACAgQHBggJBQoFCwICAgEAAgIEBQYGBQIEBAICAgIAAAICAgIDAwIC
  AgICAgICAAAAAAEAAAEAAAAAAQAAAAA=
     ";break;
  case "Reference_Point":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAAAAAA
  ACAAAABBAAAAagAAAIsAAAC0AAAA1QAAAP8AAAAAMAAAIDAAAEEwAABqMAAAizAAALQwAADVMAAA
  /zAAAABlAAAgZQAAQWUAAGplAACLZQAAtGUAANVlAAD/ZQAAAJUAACCVAABBlQAAapUAAIuVAAC0
  lQAA1ZUAAP+VAAAAygAAIMoAAEHKAABqygAAi8oAALTKAADVygAA/8oAAAD/AAAg/wAAQf8AAGr/
  AACL/wAAtP8AANX/AAD//wAAAAA5ACAAOQBBADkAagA5AIsAOQC0ADkA1QA5AP8AOQAAMDkAIDA5
  AEEwOQBqMDkAizA5ALQwOQDVMDkA/zA5AABlOQAgZTkAQWU5AGplOQCLZTkAtGU5ANVlOQD/ZTkA
  AJU5ACCVOQBBlTkAapU5AIuVOQC0lTkA1ZU5AP+VOQAAyjkAIMo5AEHKOQBqyjkAi8o5ALTKOQDV
  yjkA/8o5AAD/OQAg/zkAQf85AGr/OQCL/zkAtP85ANX/OQD//zkAAAB7ACAAewBBAHsAagB7AIsA
  ewC0AHsA1QB7AP8AewAAMHsAIDB7AEEwewBqMHsAizB7ALQwewDVMHsA/zB7AABlewAgZXsAQWV7
  AGplewCLZXsAtGV7ANVlewD/ZXsAAJV7ACCVewBBlXsAapV7AIuVewC0lXsA1ZV7AP+VewAAynsA
  IMp7AEHKewBqynsAi8p7ALTKewDVynsA/8p7AAD/ewAg/3sAQf97AGr/ewCL/3sAtP97ANX/ewD/
  /3sAAAC9ACAAvQBBAL0AagC9AIsAvQC0AL0A1QC9AP8AvQAAML0AIDC9AEEwvQBqML0AizC9ALQw
  vQDVML0A/zC9AABlvQAgZb0AQWW9AGplvQCLZb0AtGW9ANVlvQD/Zb0AAJW9ACCVvQBBlb0AapW9
  AIuVvQC0lb0A1ZW9AP+VvQAAyr0AIMq9AEHKvQBqyr0Ai8q9ALTKvQDVyr0A/8q9AAD/vQAg/70A
  Qf+9AGr/vQCL/70AtP+9ANX/vQD//70AAAD/ACAA/wBBAP8AagD/AIsA/wC0AP8A1QD/AP8A/wAA
  MP8AIDD/AEEw/wBqMP8AizD/ALQw/wDVMP8A/zD/AABl/wAgZf8AQWX/AGpl/wCLZf8AtGX/ANVl
  /wD/Zf8AAJX/ACCV/wBBlf8AapX/AIuV/wC0lf8A1ZX/AP+V/wAAyv8AIMr/AEHK/wBqyv8Ai8r/
  ALTK/wDVyv8A/8r/AAD//wAg//8AQf//AGr//wCL//8AtP//ANX//wD///8A////AObm5gDV1dUA
  xcXFALS0tACkpKQAlJSUAIODgwBzc3MAYmJiAFJSUgBBQUEAMTExACAgIAAQEBAAAAAAAPPz8/Pz
  tfPz87W18/O18/Pzx8fH8bb09Ib0tsfHx8fz9Mfx8n2FtvHxtoV98cfH8/PH8n22x8fHx8fHtn3x
  x/Pz8X22x8fx8cfHx8e2fcfz87aFx8fxtn198sfHx4W28/P0tsfx80QUFEzxx8e2tvPz9PHH8X0U
  RBQUhsfH8obz8/Txx8d9FBQUFLbHx/KG8/P0tsfH8kwUFEzxx/G2tvPztoXHx8fxhrbx7/Hx9fK1
  88d9tsfHx8fHx/HvtoXH8/PH8X22x8fHx8fxtn3xx/Pzx8fxfX228vK2fX3xx8f088fHx8fytraG
  tvLHx8fH8/Pz8/Pz8/Pz8/O18/Pz8/M=
     ";break;
  case "Reference_Point_Disabled":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAAxcXF
  AAAA/wD/AP8A1dXVANXKvQC0lXsA5ubmAP///wC0ynsA1cp7AKSkpACLZQAAi5U5ALS0tACLZTkA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABAQEB
  AQEBAQEBAQEBAQABAQIBBgENAQkBBAECAQEBAQIGAwUIBAYGBAgFAQEBAQEBAwUEAgICAgICAQEB
  AgEBBgUEAgIGBgICAQEBBQEBAQEIAgIGBAUFAQEBAggEAQENBAIGAA4LAQEBAgIEAQEBAQYCBgUL
  AQEBCQICAwkBAQ0GAgIFAQEBCwQCAgMBAQEBBAICAQEBCwwGAgYEBAEBBAgCAQEBCQQGBwYGCgEB
  AQEFAQEBAgICAgYHBAgCAQECAQEBAgICAgIGBAUGAQEBAQEBBQUEAwMEBQUGAgIBAQEBAgEDAQQB
  BAECAQIBAQABAQEBAQEBAQEBAQEBAQA=
     ";break;
  case "Reference_Point_Found":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAAxcXF
  ALTKvQD/AP8A1dXVANXKvQDVynsA5ubmALSVewC0tLQA////ALTKewCkpKQAi5U5AItlAAAgygAA
  i2U5AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AQAADgEBAAABAAAAAgICBgQIDg4OBAICAgIACAIGAwcKDg4ODg4HBgICAAACAwcEAg4ODg4ODgcG
  AgAABgcEAg4ODg4ODg4OBwIAAAQKAgIODg4HAw4ODg4EAAAIBAIGAA4ODQwGDg4ODgAACAYCBgcN
  Dw0NBQIODg4OAAgGAgIHDQ0NDQQCAg4ODgAIBAICAwwNDQwGAgYEDgAABAoCAgIGBQQGCQYGCwMB
  AAIHBAICAgICAgYJBAoCAAACBgcEAgICAgIGBAcGAgAAAgIGBwcEAwMEBwcGAgIIAAICAgIDBAQF
  BAMCAgICAAAAAAAAAAAAAAABAAAAAAA=
     ";break;
  case "Stages_of_a_Multicache":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAAAAAA
  ACAAAABBAAAAagAAAIsAAAC0AAAA1QAAAP8AAAAAMAAAIDAAAEEwAABqMAAAizAAALQwAADVMAAA
  /zAAAABlAAAgZQAAQWUAAGplAACLZQAAtGUAANVlAAD/ZQAAAJUAACCVAABBlQAAapUAAIuVAAC0
  lQAA1ZUAAP+VAAAAygAAIMoAAEHKAABqygAAi8oAALTKAADVygAA/8oAAAD/AAAg/wAAQf8AAGr/
  AACL/wAAtP8AANX/AAD//wAAAAA5ACAAOQBBADkAagA5AIsAOQC0ADkA1QA5AP8AOQAAMDkAIDA5
  AEEwOQBqMDkAizA5ALQwOQDVMDkA/zA5AABlOQAgZTkAQWU5AGplOQCLZTkAtGU5ANVlOQD/ZTkA
  AJU5ACCVOQBBlTkAapU5AIuVOQC0lTkA1ZU5AP+VOQAAyjkAIMo5AEHKOQBqyjkAi8o5ALTKOQDV
  yjkA/8o5AAD/OQAg/zkAQf85AGr/OQCL/zkAtP85ANX/OQD//zkAAAB7ACAAewBBAHsAagB7AIsA
  ewC0AHsA1QB7AP8AewAAMHsAIDB7AEEwewBqMHsAizB7ALQwewDVMHsA/zB7AABlewAgZXsAQWV7
  AGplewCLZXsAtGV7ANVlewD/ZXsAAJV7ACCVewBBlXsAapV7AIuVewC0lXsA1ZV7AP+VewAAynsA
  IMp7AEHKewBqynsAi8p7ALTKewDVynsA/8p7AAD/ewAg/3sAQf97AGr/ewCL/3sAtP97ANX/ewD/
  /3sAAAC9ACAAvQBBAL0AagC9AIsAvQC0AL0A1QC9AP8AvQAAML0AIDC9AEEwvQBqML0AizC9ALQw
  vQDVML0A/zC9AABlvQAgZb0AQWW9AGplvQCLZb0AtGW9ANVlvQD/Zb0AAJW9ACCVvQBBlb0AapW9
  AIuVvQC0lb0A1ZW9AP+VvQAAyr0AIMq9AEHKvQBqyr0Ai8q9ALTKvQDVyr0A/8q9AAD/vQAg/70A
  Qf+9AGr/vQCL/70AtP+9ANX/vQD//70AAAD/ACAA/wBBAP8AagD/AIsA/wC0AP8A1QD/AP8A/wAA
  MP8AIDD/AEEw/wBqMP8AizD/ALQw/wDVMP8A/zD/AABl/wAgZf8AQWX/AGpl/wCLZf8AtGX/ANVl
  /wD/Zf8AAJX/ACCV/wBBlf8AapX/AIuV/wC0lf8A1ZX/AP+V/wAAyv8AIMr/AEHK/wBqyv8Ai8r/
  ALTK/wDVyv8A/8r/AAD//wAg//8AQf//AGr//wCL//8AtP//ANX//wD///8A////AObm5gDV1dUA
  xcXFALS0tACkpKQAlJSUAIODgwBzc3MAYmJiAFJSUgBBQUEAMTExACAgIAAQEBAAAAAAAMfHx8fH
  x8fHx8fH8cfHx8fHx8fHx8fHx8fxtazyx8fHx8fHx8fHx8fH9GhoosfHx8fHx8fHx8fHx/Krq6zH
  x8fHx8fHx/Ly8cfxtbS05cfHx8fHx/RxovTH8aKioaLHx8fHx8esaWirx/NxcXFp88fHx8e0tLWs
  8sf0aGhoaKzHx8f0oaKqtMfHq2hoaGirx8fxcWlxcavHx6xoaGhoq8fHrGhoaGisx8fxaWhoaPTH
  8XJoaGhorMfHx/KjcvXxx/FyaGhoaPTHx8fH8fHxx8fx92hoaHHxx8fHx8fHx8fH8fNzcqPyx8fH
  x8fHx8fHx8fx8vLxx8fHx8fHx8fHx8c=
     ";break;
  case "Stages_of_a_Multicache_Disabled":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAA/wD/
  AAAA/wBBZXsA1dXVAAAwewAgZXsA5ubmALS0tACLlb0AamW9AKSkpAAgMHsAapW9ACBlvQBBZb0A
  QZW9AIvKvQC0yr0AtMr/AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABAQEB
  AQEBAQEBAQEBAQABAQABAAEAAQABEQEDAQEBAQAAAAAAAAAABwQEAQEBAQEBAAAAAAAAAAMMAQEB
  AAEBAAAAAAMDBgAGAQEBEgEBAQEAAAcFDgcAAQEBDQ4AAQEAAAAICwQMAQEBBQULAQEBAQAQEBEI
  AQEBBAQEBAgBAQAHDQ4PAQEBDAQEBAQBAQEBBQsFAQEBAAgEBAQEDAEBCAQEAQEBAAAGCwQEBAEB
  AQEEAQEBCAAAAAMJAgoGAQECAQEBBAcAAAAABgYGAQEBAQEBBAUGAAAAAAAAAAABAQEBAgEDAQAB
  AAEAAQABAQABAQEBAQEBAQEBAQEBAQA=
     ";break;
  case "Stages_of_a_Multicache_Found":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAA/wD/
  AObm5gDV1dUAxcXFAGplewBBZXsAamW9AIODgwAAMHsAIGV7ALS0tACLlb0ApKSkACAwewBqlb0A
  IMoAACBlvQBBZb0AQZW9AIvKvQC0yr0AtMr/AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAADwAAAQAAAAAAAAAAAAAADw8PFAsCAAAAAAAAAAAADw8PDw8IEQAAAAAAAAAAAA8PDw8PDwsA
  AAAAAAAAAA8PDw8PDw8PFQAAAAAAAAoPDw8AAQ8PDw8AAAAAAAALDQ8PAAMJDw8PDwAAAAATExQL
  AgAKCAgPDw8PAAAKEBESEwAADggICA8PDwABCQ0JCQ4AAAsICAgIDwAACwgICAgLAAABDQgICAoA
  AQUICAgICwAAAAIGBQwBAAEFCAgICAoAAAAAAQEBAAABBwgICAkBAAAAAAAAAAAAAQMEBQYCAAAA
  AAAAAAAAAAABAgIBAAAAAAAAAAAAAAA=
     ";break;
  case "Traditional":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAAAAAA
  ACAAAABBAAAAagAAAIsAAAC0AAAA1QAAAP8AAAAAMAAAIDAAAEEwAABqMAAAizAAALQwAADVMAAA
  /zAAAABlAAAgZQAAQWUAAGplAACLZQAAtGUAANVlAAD/ZQAAAJUAACCVAABBlQAAapUAAIuVAAC0
  lQAA1ZUAAP+VAAAAygAAIMoAAEHKAABqygAAi8oAALTKAADVygAA/8oAAAD/AAAg/wAAQf8AAGr/
  AACL/wAAtP8AANX/AAD//wAAAAA5ACAAOQBBADkAagA5AIsAOQC0ADkA1QA5AP8AOQAAMDkAIDA5
  AEEwOQBqMDkAizA5ALQwOQDVMDkA/zA5AABlOQAgZTkAQWU5AGplOQCLZTkAtGU5ANVlOQD/ZTkA
  AJU5ACCVOQBBlTkAapU5AIuVOQC0lTkA1ZU5AP+VOQAAyjkAIMo5AEHKOQBqyjkAi8o5ALTKOQDV
  yjkA/8o5AAD/OQAg/zkAQf85AGr/OQCL/zkAtP85ANX/OQD//zkAAAB7ACAAewBBAHsAagB7AIsA
  ewC0AHsA1QB7AP8AewAAMHsAIDB7AEEwewBqMHsAizB7ALQwewDVMHsA/zB7AABlewAgZXsAQWV7
  AGplewCLZXsAtGV7ANVlewD/ZXsAAJV7ACCVewBBlXsAapV7AIuVewC0lXsA1ZV7AP+VewAAynsA
  IMp7AEHKewBqynsAi8p7ALTKewDVynsA/8p7AAD/ewAg/3sAQf97AGr/ewCL/3sAtP97ANX/ewD/
  /3sAAAC9ACAAvQBBAL0AagC9AIsAvQC0AL0A1QC9AP8AvQAAML0AIDC9AEEwvQBqML0AizC9ALQw
  vQDVML0A/zC9AABlvQAgZb0AQWW9AGplvQCLZb0AtGW9ANVlvQD/Zb0AAJW9ACCVvQBBlb0AapW9
  AIuVvQC0lb0A1ZW9AP+VvQAAyr0AIMq9AEHKvQBqyr0Ai8q9ALTKvQDVyr0A/8q9AAD/vQAg/70A
  Qf+9AGr/vQCL/70AtP+9ANX/vQD//70AAAD/ACAA/wBBAP8AagD/AIsA/wC0AP8A1QD/AP8A/wAA
  MP8AIDD/AEEw/wBqMP8AizD/ALQw/wDVMP8A/zD/AABl/wAgZf8AQWX/AGpl/wCLZf8AtGX/ANVl
  /wD/Zf8AAJX/ACCV/wBBlf8AapX/AIuV/wC0lf8A1ZX/AP+V/wAAyv8AIMr/AEHK/wBqyv8Ai8r/
  ALTK/wDVyv8A/8r/AAD//wAg//8AQf//AGr//wCL//8AtP//ANX//wD///8A////AObm5gDV1dUA
  xcXFALS0tACkpKQAlJSUAIODgwBzc3MAYmJiAFJSUgBBQUEAMTExACAgIAAQEBAAAAAAAMfHx8fH
  x8fHx8fHx8fHx8fHx8fHx8fHx8fHx8fHx8fH7/X39/f39/Pvx8fHx8fHx/b08fHx8fH7+PHHx8fH
  x8f47/Hz8vHv+PH49e/Hx8fH+PP29fTv7/jv7/T58u/Hx/r9QkJC+O/47+/v8ff38cf8/Pz7+zn7
  /PLv8fHv8/nx/Tn8/Pz8/P379/Lx8fH08/L5Qnt6enp6/Pv79vHx9PPH7/P6QkFBQUpC/Pv69PTz
  x8fH7/X7QkFBQUpC/Pv79MfHx8fH8ff7SkpKentB+/rHx8fHx8fH8fj5+fn5+fn1x8fHx8fHx8fH
  x8fHx8fHx8fHx8fHx8fHx8fHx8fHx8c=
     ";break;
  case "Traditional_Disabled":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAA/wD/
  AAAA/wDm5uYAc3NzAGJiYgCDg4MAQUFBAEGVOQBBlXsAapV7ACBlOQBBZTkAMTExAP///wDFxcUA
  UlJSALS0tACUlJQAIDA5ANXV1QAgICAApKSkAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABAQEB
  AQEBAQEBAQEBAQABAQABAAEAAQABAAEAAQEBARUFBQUFBQ4NAAAAAQEBAQEBAgICAgIGAwIAAQEB
  AAEBDQIOEwINAwIDAQEBAAEBAQERFRANDQMNAQEBEw0AAQEUCwsLAw0DAQEBAgUFAQEBAQwGBhIG
  AQEBAgINDgQBARIMDAwMAQEBBRMCAgIBAQEBCwkIAQEBDAYGEQICEAEBDQ4PAQEBCgcLDAYPEAEB
  AQEAAQEBCwoKCgcLDAYGAQEAAQEBAgUGBwcHCAkKAQEBAQEBAAAAAgMEBAQEBAQBAQEBAAEAAQAB
  AAEAAQABAQABAQEBAQEBAQEBAQEBAQA=
     ";break;
  case "Trailhead":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAAAAAA
  ACAAAABBAAAAagAAAIsAAAC0AAAA1QAAAP8AAAAAMAAAIDAAAEEwAABqMAAAizAAALQwAADVMAAA
  /zAAAABlAAAgZQAAQWUAAGplAACLZQAAtGUAANVlAAD/ZQAAAJUAACCVAABBlQAAapUAAIuVAAC0
  lQAA1ZUAAP+VAAAAygAAIMoAAEHKAABqygAAi8oAALTKAADVygAA/8oAAAD/AAAg/wAAQf8AAGr/
  AACL/wAAtP8AANX/AAD//wAAAAA5ACAAOQBBADkAagA5AIsAOQC0ADkA1QA5AP8AOQAAMDkAIDA5
  AEEwOQBqMDkAizA5ALQwOQDVMDkA/zA5AABlOQAgZTkAQWU5AGplOQCLZTkAtGU5ANVlOQD/ZTkA
  AJU5ACCVOQBBlTkAapU5AIuVOQC0lTkA1ZU5AP+VOQAAyjkAIMo5AEHKOQBqyjkAi8o5ALTKOQDV
  yjkA/8o5AAD/OQAg/zkAQf85AGr/OQCL/zkAtP85ANX/OQD//zkAAAB7ACAAewBBAHsAagB7AIsA
  ewC0AHsA1QB7AP8AewAAMHsAIDB7AEEwewBqMHsAizB7ALQwewDVMHsA/zB7AABlewAgZXsAQWV7
  AGplewCLZXsAtGV7ANVlewD/ZXsAAJV7ACCVewBBlXsAapV7AIuVewC0lXsA1ZV7AP+VewAAynsA
  IMp7AEHKewBqynsAi8p7ALTKewDVynsA/8p7AAD/ewAg/3sAQf97AGr/ewCL/3sAtP97ANX/ewD/
  /3sAAAC9ACAAvQBBAL0AagC9AIsAvQC0AL0A1QC9AP8AvQAAML0AIDC9AEEwvQBqML0AizC9ALQw
  vQDVML0A/zC9AABlvQAgZb0AQWW9AGplvQCLZb0AtGW9ANVlvQD/Zb0AAJW9ACCVvQBBlb0AapW9
  AIuVvQC0lb0A1ZW9AP+VvQAAyr0AIMq9AEHKvQBqyr0Ai8q9ALTKvQDVyr0A/8q9AAD/vQAg/70A
  Qf+9AGr/vQCL/70AtP+9ANX/vQD//70AAAD/ACAA/wBBAP8AagD/AIsA/wC0AP8A1QD/AP8A/wAA
  MP8AIDD/AEEw/wBqMP8AizD/ALQw/wDVMP8A/zD/AABl/wAgZf8AQWX/AGpl/wCLZf8AtGX/ANVl
  /wD/Zf8AAJX/ACCV/wBBlf8AapX/AIuV/wC0lf8A1ZX/AP+V/wAAyv8AIMr/AEHK/wBqyv8Ai8r/
  ALTK/wDVyv8A/8r/AAD//wAg//8AQf//AGr//wCL//8AtP//ANX//wD///8A////AObm5gDV1dUA
  xcXFALS0tACkpKQAlJSUAIODgwBzc3MAYmJiAFJSUgBBQUEAMTExACAgIAAQEBAAAAAAALa1tvP0
  tvO1tLbztbX08/O1x8fHx/Gqqavx7+7Hx8fz88fH8cfuqtuq8fHHx8fH9PPH8e7x8arcqvHxx8fH
  x/P0x/Hx8fKq26nx8cfHx8e18/Hz9LW0quOq8vHxx8fHtbW0q6KpqqqzqvPy8fHxx7W0quTk4+Tc
  26rz8/Py8se1tarv8e3utbOpqqqpq6nn87Wq29qhqqraqrTktO6q8bX0q6uiqqqh26vu7u7vqvHz
  8/Hx5vLzqrOpqqKq5KLx87XHx/Hx8qrjqaqhqqqr8fPzx8fu8fGqqqvl5vLy8fHz88fHx8fH7u7m
  8fHx8cfx8/Pz9PPztrW18/Tz87X08/Q=
     ";break;
  case "Trailhead_Disabled":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAAxcXF
  AAAA/wC0tLQA/wD/ANX//wDm5uYAQZW9AGqVvQC0yv8A1cr/ANXV1QBqyv8AIJW9ACBlvQBqyr0A
  QWW9AIvK/wBqlf8A////AEGV/wCLyr0AtP//AIuV/wC0yr0A1cq9AAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABgBAQEB
  AQEBAQEBAQEBAQABAQMBAwEGAQcBEgEDAQEBAQMDBQMEBhEGBQUDAQEBAQEBBQQFBQYWBgUFAQEB
  AwEBAwUFBQoGEQwFAQEBAwEBAQEAAhcUBgsGAQEBAwMDAQEUBw8MBgYOAQEBBQUFAQEBARAQCxAW
  AQEBAAAKCgMBAQYSBRUEAQEBBgYMBwwBAQEBERMNAQEBBhQQFAQGBQEBBwcPAQEBEQcEBAQSBgEB
  AQEFAQEBBg4MBg8GEA8FAQEDAQEBCgYLDAYNBgYHAQEBAQEBBQUGBgcICQoKBQUBAQEBAwEDAQQB
  BQEFAQMBAQABAQEBAQEBAQEBAQEBAQI=
     ";break;
  case "Trailhead_Found":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAAxcXF
  ALS0tADVyr0AtMq9AP8A/wDV//8A1cr/AObm5gBBlb0AapW9ALTK/wDV1dUAasr/ACCVvQAgZb0A
  asq9AEFlvQCLyv8AapX/AP///wBBlf8Ai8q9ACDKAAC0//8Ai5X/AAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAIDAgAB
  AgADFgIAAwMBAAADBAQEBAcIFhYWEwUEBAQAAAQEBwQFFhYWFhYEBAQEAQAEBwUHBxYWFhYWFgQE
  BAABBAcHBxYWFhYWFhYWBAQDAAcAAQMWFhYICxYWFhYEAwMVCRANCBYWCAALFhYWFgMVCBERDBEY
  EggAAAAWFhYWAwgTBxcFAw8NCAgNCRYWFgMIEhQOCAgUCBURFQUIFgMBCQkQCAgOEgkFBQUTCAcA
  AAcHBgsACA8NCBAIERAHAAMEBAcHCwgMDQgOCAgJBwAABAQFBwcICAkKBgsLBwcAAAQEBAQEBQUG
  BwcHBwQHAAAAAQAAAgMDAAEAAAMBAAE=
     ";break;
  case "Virtual":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAAAAAA
  ACAAAABBAAAAagAAAIsAAAC0AAAA1QAAAP8AAAAAMAAAIDAAAEEwAABqMAAAizAAALQwAADVMAAA
  /zAAAABlAAAgZQAAQWUAAGplAACLZQAAtGUAANVlAAD/ZQAAAJUAACCVAABBlQAAapUAAIuVAAC0
  lQAA1ZUAAP+VAAAAygAAIMoAAEHKAABqygAAi8oAALTKAADVygAA/8oAAAD/AAAg/wAAQf8AAGr/
  AACL/wAAtP8AANX/AAD//wAAAAA5ACAAOQBBADkAagA5AIsAOQC0ADkA1QA5AP8AOQAAMDkAIDA5
  AEEwOQBqMDkAizA5ALQwOQDVMDkA/zA5AABlOQAgZTkAQWU5AGplOQCLZTkAtGU5ANVlOQD/ZTkA
  AJU5ACCVOQBBlTkAapU5AIuVOQC0lTkA1ZU5AP+VOQAAyjkAIMo5AEHKOQBqyjkAi8o5ALTKOQDV
  yjkA/8o5AAD/OQAg/zkAQf85AGr/OQCL/zkAtP85ANX/OQD//zkAAAB7ACAAewBBAHsAagB7AIsA
  ewC0AHsA1QB7AP8AewAAMHsAIDB7AEEwewBqMHsAizB7ALQwewDVMHsA/zB7AABlewAgZXsAQWV7
  AGplewCLZXsAtGV7ANVlewD/ZXsAAJV7ACCVewBBlXsAapV7AIuVewC0lXsA1ZV7AP+VewAAynsA
  IMp7AEHKewBqynsAi8p7ALTKewDVynsA/8p7AAD/ewAg/3sAQf97AGr/ewCL/3sAtP97ANX/ewD/
  /3sAAAC9ACAAvQBBAL0AagC9AIsAvQC0AL0A1QC9AP8AvQAAML0AIDC9AEEwvQBqML0AizC9ALQw
  vQDVML0A/zC9AABlvQAgZb0AQWW9AGplvQCLZb0AtGW9ANVlvQD/Zb0AAJW9ACCVvQBBlb0AapW9
  AIuVvQC0lb0A1ZW9AP+VvQAAyr0AIMq9AEHKvQBqyr0Ai8q9ALTKvQDVyr0A/8q9AAD/vQAg/70A
  Qf+9AGr/vQCL/70AtP+9ANX/vQD//70AAAD/ACAA/wBBAP8AagD/AIsA/wC0AP8A1QD/AP8A/wAA
  MP8AIDD/AEEw/wBqMP8AizD/ALQw/wDVMP8A/zD/AABl/wAgZf8AQWX/AGpl/wCLZf8AtGX/ANVl
  /wD/Zf8AAJX/ACCV/wBBlf8AapX/AIuV/wC0lf8A1ZX/AP+V/wAAyv8AIMr/AEHK/wBqyv8Ai8r/
  ALTK/wDVyv8A/8r/AAD//wAg//8AQf//AGr//wCL//8AtP//ANX//wD///8A////AObm5gDV1dUA
  xcXFALS0tACkpKQAlJSUAIODgwBzc3MAYmJiAFJSUgBBQUEAMTExACAgIAAQEBAAAAAAAMfv+/r6
  +fP1+vv6+vjyx8fH7/P68fH59vHv7+/y+PXHx8fH9PXv7+/v7+/v7+/48sfHx+/47+/v7+/v7/H1
  +/XHx8fv+e/v8+/v7/H3+vLvx8fv+Pnv8jD27+/1OvHHx8fv+ff17/rAYPPx+Pn478fv9/P48fYw
  YGA58vjz9vTH8fj2+u/68/Hy+Pf2+vH57/c69fjv8fny9/Tv9Pz6+fH68/H37/IA9P347/P59P3z
  8sfx+O/yAPT9+O/z+fPz8sfH7/nv8/fv8/fv9Prxx8fHx8f38/Hv7+/x8vf478fHx8fH8fnz7+/x
  9Pn578fHx8fHx8fx9/r6+vn078fHx8c=
     ";break;
  case "Virtual_Disabled":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAA/wD/
  AAAA/wDm5uYAxcXFAP///wC0tLQAYmJiANXV1QCDg4MAc3NzAFJSUgAAAAAAICAgAKSkpAAxMTEA
  lJSUAAAAOQBBMDkAQUFBAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABAQEB
  AQEBAQEBAQEBAQABAQMBAgEGAQIBBAEHAQEBAQAABQ0EBAQEBAQEAQEBAQEBAAQJBAQEBAQEAQEB
  EgEBAAAEBgQEAwQEAQEBCgEBAQEECQYEBxAPAQEBEQIAAQEEBggNBAoBAQEBCQYJAQEBAQMJAg8Q
  AQEBBwkDDwUBAQkPCgQKAQEBCQgPCgIBAQEBDQkEAQEBCAUEBQ4KBgEBAwIIAQEBBQwJBAMGBQEB
  AQECAQEBCwUMCQQDBgMDAQEAAQEBAwgEAwgEBQoCAQEBAQEBAwIEBAQCBwgJBAABAQEBAgEDAQQB
  BQEGAQABAQABAQEBAQEBAQEBAQEBAQA=
     ";break;
  case "Webcam":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAAAAAA
  ACAAAABBAAAAagAAAIsAAAC0AAAA1QAAAP8AAAAAMAAAIDAAAEEwAABqMAAAizAAALQwAADVMAAA
  /zAAAABlAAAgZQAAQWUAAGplAACLZQAAtGUAANVlAAD/ZQAAAJUAACCVAABBlQAAapUAAIuVAAC0
  lQAA1ZUAAP+VAAAAygAAIMoAAEHKAABqygAAi8oAALTKAADVygAA/8oAAAD/AAAg/wAAQf8AAGr/
  AACL/wAAtP8AANX/AAD//wAAAAA5ACAAOQBBADkAagA5AIsAOQC0ADkA1QA5AP8AOQAAMDkAIDA5
  AEEwOQBqMDkAizA5ALQwOQDVMDkA/zA5AABlOQAgZTkAQWU5AGplOQCLZTkAtGU5ANVlOQD/ZTkA
  AJU5ACCVOQBBlTkAapU5AIuVOQC0lTkA1ZU5AP+VOQAAyjkAIMo5AEHKOQBqyjkAi8o5ALTKOQDV
  yjkA/8o5AAD/OQAg/zkAQf85AGr/OQCL/zkAtP85ANX/OQD//zkAAAB7ACAAewBBAHsAagB7AIsA
  ewC0AHsA1QB7AP8AewAAMHsAIDB7AEEwewBqMHsAizB7ALQwewDVMHsA/zB7AABlewAgZXsAQWV7
  AGplewCLZXsAtGV7ANVlewD/ZXsAAJV7ACCVewBBlXsAapV7AIuVewC0lXsA1ZV7AP+VewAAynsA
  IMp7AEHKewBqynsAi8p7ALTKewDVynsA/8p7AAD/ewAg/3sAQf97AGr/ewCL/3sAtP97ANX/ewD/
  /3sAAAC9ACAAvQBBAL0AagC9AIsAvQC0AL0A1QC9AP8AvQAAML0AIDC9AEEwvQBqML0AizC9ALQw
  vQDVML0A/zC9AABlvQAgZb0AQWW9AGplvQCLZb0AtGW9ANVlvQD/Zb0AAJW9ACCVvQBBlb0AapW9
  AIuVvQC0lb0A1ZW9AP+VvQAAyr0AIMq9AEHKvQBqyr0Ai8q9ALTKvQDVyr0A/8q9AAD/vQAg/70A
  Qf+9AGr/vQCL/70AtP+9ANX/vQD//70AAAD/ACAA/wBBAP8AagD/AIsA/wC0AP8A1QD/AP8A/wAA
  MP8AIDD/AEEw/wBqMP8AizD/ALQw/wDVMP8A/zD/AABl/wAgZf8AQWX/AGpl/wCLZf8AtGX/ANVl
  /wD/Zf8AAJX/ACCV/wBBlf8AapX/AIuV/wC0lf8A1ZX/AP+V/wAAyv8AIMr/AEHK/wBqyv8Ai8r/
  ALTK/wDVyv8A/8r/AAD//wAg//8AQf//AGr//wCL//8AtP//ANX//wD///8A////AObm5gDV1dUA
  xcXFALS0tACkpKQAlJSUAIODgwBzc3MAYmJiAFJSUgBBQUEAMTExACAgIAAQEBAAAAAAAMfHx/P9
  +Pf39/n6/fjvx8fHx8fH8fj38/T5+vTvx8fHx8fHx8fv+f37/O/Hx8fHx8fHx8fHx/n++vvvx8fH
  x8fHx8fHx+/6AP7778fHx8fHx8fHx/L5+ff2+Pr078fHx8fHx/L69PHx7+/x+fXvx8fHx+/69PH4
  +Pn68u/58cfHx8fz+PH48vL19vvx8vfHx8fH9/Tx9/P29vb69e/4x8fHx/jz8vb09vb3+fbv+MfH
  x8f29O/59fb4+Pvz7/fHx8fH8vnv9Pr4+Pv47/H1x8fHx+/49O/y9/j07+/278fHx8fH7/n17+/v
  7/H38sfHx8fHx8fv9fn4+Pj28cfHx8c=
     ";break;
  case "Webcam_Disabled":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAA/wD/
  AAAA/wD///8ApKSkAIODgwC0tLQA1dXVAHNzcwCUlJQAUlJSAEFBQQDm5uYAxcXFAGJiYgAAAAAA
  EBAQACAgIAAxMTEAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABAQEB
  AQEBAQEBAQEBAQABAQABCwEEAQUBCQECAQEBAQAAAAACDRAKEQIAAQEBAQEBAAAAAA0PCQoCAQEB
  AAEBAAAAAAIJDg8KAQEBAAEBAQEAAAYNDQQIAQEBAgAAAQEAAAYJBQsLAQEBDQMCAQEBAQIJBQsH
  AQEBBgINCwABAQAMBwsHAQEBCAoLBgQBAQEBBAULAQEBCAgJAwIHAAEBAAcMAQEBCAgEDQgCBwEB
  AQEIAQEBAwgHBwoMAgQAAQEAAQEBBQkHBwoHAgsDAQEBAQEBBQIGBAcFAgIIAgABAQEBAgEDAQIB
  AgEEAQABAQABAQEBAQEBAQEBAQEBAQA=
     ";break;
  case "Whereigo":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAAAAAA
  ACAAAABBAAAAagAAAIsAAAC0AAAA1QAAAP8AAAAAMAAAIDAAAEEwAABqMAAAizAAALQwAADVMAAA
  /zAAAABlAAAgZQAAQWUAAGplAACLZQAAtGUAANVlAAD/ZQAAAJUAACCVAABBlQAAapUAAIuVAAC0
  lQAA1ZUAAP+VAAAAygAAIMoAAEHKAABqygAAi8oAALTKAADVygAA/8oAAAD/AAAg/wAAQf8AAGr/
  AACL/wAAtP8AANX/AAD//wAAAAA5ACAAOQBBADkAagA5AIsAOQC0ADkA1QA5AP8AOQAAMDkAIDA5
  AEEwOQBqMDkAizA5ALQwOQDVMDkA/zA5AABlOQAgZTkAQWU5AGplOQCLZTkAtGU5ANVlOQD/ZTkA
  AJU5ACCVOQBBlTkAapU5AIuVOQC0lTkA1ZU5AP+VOQAAyjkAIMo5AEHKOQBqyjkAi8o5ALTKOQDV
  yjkA/8o5AAD/OQAg/zkAQf85AGr/OQCL/zkAtP85ANX/OQD//zkAAAB7ACAAewBBAHsAagB7AIsA
  ewC0AHsA1QB7AP8AewAAMHsAIDB7AEEwewBqMHsAizB7ALQwewDVMHsA/zB7AABlewAgZXsAQWV7
  AGplewCLZXsAtGV7ANVlewD/ZXsAAJV7ACCVewBBlXsAapV7AIuVewC0lXsA1ZV7AP+VewAAynsA
  IMp7AEHKewBqynsAi8p7ALTKewDVynsA/8p7AAD/ewAg/3sAQf97AGr/ewCL/3sAtP97ANX/ewD/
  /3sAAAC9ACAAvQBBAL0AagC9AIsAvQC0AL0A1QC9AP8AvQAAML0AIDC9AEEwvQBqML0AizC9ALQw
  vQDVML0A/zC9AABlvQAgZb0AQWW9AGplvQCLZb0AtGW9ANVlvQD/Zb0AAJW9ACCVvQBBlb0AapW9
  AIuVvQC0lb0A1ZW9AP+VvQAAyr0AIMq9AEHKvQBqyr0Ai8q9ALTKvQDVyr0A/8q9AAD/vQAg/70A
  Qf+9AGr/vQCL/70AtP+9ANX/vQD//70AAAD/ACAA/wBBAP8AagD/AIsA/wC0AP8A1QD/AP8A/wAA
  MP8AIDD/AEEw/wBqMP8AizD/ALQw/wDVMP8A/zD/AABl/wAgZf8AQWX/AGpl/wCLZf8AtGX/ANVl
  /wD/Zf8AAJX/ACCV/wBBlf8AapX/AIuV/wC0lf8A1ZX/AP+V/wAAyv8AIMr/AEHK/wBqyv8Ai8r/
  ALTK/wDVyv8A/8r/AAD//wAg//8AQf//AGr//wCL//8AtP//ANX//wD///8A////AObm5gDV1dUA
  xcXFALS0tACkpKQAlJSUAIODgwBzc3MAYmJiAFJSUgBBQUEAMTExACAgIAAQEBAAAAAAAMfHx8fx
  8/Lx9PTz78fHx8fHx+/09vR9RUV99PLx78fHx+/09X0UFBQUFBRF8vHvx8fy8kUUFBQUFBQUFETz
  8cfv8n0UFBQUFBQUFBQUffPv8/VEFBQUFEV9FBQUFBTz7/X0FBQUFEXx8RQUFBQUtu/29BQUFETx
  7+99FBQUFH7x9bYUFBQUTbbvthQUFBT08fHyRBQUFBQURH5EFBQU8+/x8n0UFBQUFBQUFBQUTfHH
  7/fzRBQUFBQUFBQURPLvx8fz9/NFFBQUFBQURLby8fPH7/T39PRNRERFffTz8+/Hx8fv8/f38vPy
  9ff39O/Hx8fHx8fv8/H08/Xz8cfHx8c=
     ";break;
  case "Whereigo_Disabled":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAA/wD/
  AAAA/wDFxcUAg4ODAKSkpAD///8AtLS0ALSVOQCLZTkAtGU5ALSVewCLZQAA1cq9ANXV1QDVlXsA
  5ubmAJSUlAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABAQEB
  AQEBAQEBAQEBAQABAQUBEAEKAQkBBgEPAQEBAQUGBAoLCwsLCwsJAQEBAQEBDQkLCwsLCwsLAQEB
  DwEBDQoLCwsLCwsLAQEBCgEBAQEICwsLCwkKAQEBCwsCAQEGCwsLCwkPAQEBCwsLAQEBAQsLCwgP
  AQEBCwsLCw4BAQwLCwsLAQEBDAsLCwsBAQEBCAsLAQEBCA4ICwsLAgEBDQoLAQEBCwsLCwsLBwEB
  AQECAQEBCwsLCwsLCA0FAQECAQEBCwsLCwsLCAwNAQEBAQEBBgYHCAgJCgYCAgUBAQEBAgEDAQIB
  BAEDAQUBAQABAQEBAQEBAQEBAQEBAQA=
     ";break;
  case "Whereigo_Solved":
    bmpdata = @"
  Qk02BQAAAAAAADYEAAAoAAAAEAAAABAAAAABAAgAAAAAAAAAAAASCwAAEgsAAAAAAAAAAAAA/wD/
  AP///wDFxcUA5ubmALS0tACkpKQAg4ODANXV1QC0lTkAi2U5ALRlOQC0lXsAi2UAANXKvQDVlXsA
  IMoAAJSUlAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
  AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAD
  AgcDDwQCAQAAAAAAAAEEEAQLDw8PBAcDAQAAAAEEBQsMDw8PDw8KBwMBAAAHBwoMDA8PDw8PDwkC
  AwABBwsMDA8PDw8PDw8PCwIBAgUJDAwPDw8LDA8PDw8CAQUEDAwMDA8PAwwMDw8PDwEQBAwMDAkD
  AQELDAwPDw8PBQ0MDAwMCA0BDQwMDA8PDwMHCQwMDAwMCQ4JDAwMDwEDBwsMDAwMDAwMDAwMCAMA
  AQYCCQwMDAwMDAwMCQcBAAACBgIKDAwMDAwMCQ0HAwIAAQQGBAQICQkKCwQCAgEAAAABAgYGBwIH
  BQYGBAEAAAAAAAABAgMEAgUCAwAAAAA=
     ";break;

       default:
         bmpdata=GetBmpData("Traditional");
         break;
      }
      return bmpdata;
    }
}
