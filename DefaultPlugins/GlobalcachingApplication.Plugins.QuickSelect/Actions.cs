using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Plugins.QuickSelect
{
    public class Actions : Utils.BasePlugin.Plugin
    {
        public const string ACTION_CLEAR = "Quick Select|Clear selection";
        public const string ACTION_INVERT = "Quick Select|Invert selection";
        public const string ACTION_SELECTALL = "Quick Select|Select All";
        public const string ACTION_FOUND = "Quick Select|Select found";
        public const string ACTION_NOTFOUND = "Quick Select|Select not found";
        public const string ACTION_ARCHIVED = "Quick Select|Select archived";
        public const string ACTION_AVAILABLE = "Quick Select|Select available";
        public const string ACTION_FLAGGED = "Quick Select|Select flagged";
        public const string ACTION_NOTES = "Quick Select|Select notes";
        public const string ACTION_GCCOMNOTES = "Quick Select|Select geocaching.com notes";
        public const string ACTION_CORCOORDS = "Quick Select|Select with corrected coords";
        public const string ACTION_MULTFOUNDS = "Quick Select|Caches with multiple founds";
        public const string ACTION_USERWAYPOINTS = "Quick Select|Select with User waypoints";
        public const string ACTION_YOUOWN = "Quick Select|Select you own";
        public const string ACTION_CHANGED = "Quick Select|Select changed geocaches";
        public const string ACTION_LOGIMAGES = "Quick Select|Select with log images";

        public override bool Initialize(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_CLEAR);
            AddAction(ACTION_INVERT);
            AddAction(ACTION_SELECTALL);
            AddAction(ACTION_FOUND);
            AddAction(ACTION_YOUOWN);
            AddAction(ACTION_NOTFOUND);
            AddAction(ACTION_ARCHIVED);
            AddAction(ACTION_AVAILABLE);
            AddAction(ACTION_FLAGGED);
            AddAction(ACTION_NOTES);
            AddAction(ACTION_GCCOMNOTES);
            AddAction(ACTION_CORCOORDS);
            AddAction(ACTION_LOGIMAGES);
            AddAction(ACTION_MULTFOUNDS);
            AddAction(ACTION_USERWAYPOINTS);
            AddAction(ACTION_CHANGED);
            return base.Initialize(core);
        }

        public override Framework.PluginType PluginType
        {
            get
            {
                return Framework.PluginType.GeocacheSelectFilter;
            }
        }

        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result)
            {
                Core.Geocaches.BeginUpdate();
                if (action == ACTION_CLEAR)
                {
                    foreach (Framework.Data.Geocache gc in Core.Geocaches)
                    {
                        gc.Selected = false;
                    }
                }
                else if (action == ACTION_CLEAR)
                {
                    foreach (Framework.Data.Geocache gc in Core.Geocaches)
                    {
                        gc.Selected = false;
                    }
                }
                else if (action == ACTION_SELECTALL)
                {
                    foreach (Framework.Data.Geocache gc in Core.Geocaches)
                    {
                        gc.Selected = true;
                    }
                }
                else if (action == ACTION_INVERT)
                {
                    foreach (Framework.Data.Geocache gc in Core.Geocaches)
                    {
                        gc.Selected = !gc.Selected;
                    }
                }
                else if (action == ACTION_FOUND)
                {
                    //List<Framework.Data.Geocache> gcList = Utils.DataAccess.GetFoundGeocaches(Core.Geocaches, Core.Logs, Core.GeocachingComAccount);
                    //foreach (Framework.Data.Geocache gc in gcList)
                    //{
                    //    gc.Selected = true;
                    //}
                    foreach (Framework.Data.Geocache gc in Core.Geocaches)
                    {
                        gc.Selected = gc.Found;
                    }
                }
                else if (action == ACTION_NOTFOUND)
                {
                    foreach (Framework.Data.Geocache gc in Core.Geocaches)
                    {
                        //gc.Selected = !Utils.DataAccess.IsGeocacheFound(Core, gc, Core.GeocachingComAccount.AccountName);
                        gc.Selected = !gc.Found;
                    }
                }
                else if (action == ACTION_LOGIMAGES)
                {
                    foreach (Framework.Data.Geocache gc in Core.Geocaches)
                    {
                        bool select = false;
                        List<Framework.Data.Log> logs = Utils.DataAccess.GetLogs(Core.Logs, gc.Code);
                        if (logs != null)
                        {
                            foreach (Framework.Data.Log l in logs)
                            {
                                if (Utils.DataAccess.GetLogImages(Core.LogImages, l.ID).Count > 0)
                                {
                                    select = true;
                                    break;
                                }
                            }
                        }
                        gc.Selected = select;
                    }
                }
                else if (action == ACTION_YOUOWN)
                {
                    foreach (Framework.Data.Geocache gc in Core.Geocaches)
                    {
                        gc.Selected = gc.IsOwn;
                    }
                }
                else if (action == ACTION_ARCHIVED)
                {
                    foreach (Framework.Data.Geocache gc in Core.Geocaches)
                    {
                        gc.Selected = gc.Archived;
                    }
                }
                else if (action == ACTION_AVAILABLE)
                {
                    foreach (Framework.Data.Geocache gc in Core.Geocaches)
                    {
                        gc.Selected = !gc.Archived && gc.Available;
                    }
                }
                else if (action == ACTION_FLAGGED)
                {
                    foreach (Framework.Data.Geocache gc in Core.Geocaches)
                    {
                        gc.Selected = gc.Flagged;
                    }
                }
                else if (action == ACTION_CHANGED)
                {
                    foreach (Framework.Data.Geocache gc in Core.Geocaches)
                    {
                        gc.Selected = !gc.Saved;
                    }
                }
                else if (action == ACTION_NOTES)
                {
                    foreach (Framework.Data.Geocache gc in Core.Geocaches)
                    {
                        gc.Selected = (!string.IsNullOrEmpty(gc.PersonaleNote) || !string.IsNullOrEmpty(gc.Notes));
                    }
                }
                else if (action == ACTION_GCCOMNOTES)
                {
                    foreach (Framework.Data.Geocache gc in Core.Geocaches)
                    {
                        gc.Selected = !string.IsNullOrEmpty(gc.PersonaleNote);
                    }
                }
                else if (action == ACTION_CORCOORDS)
                {
                    foreach (Framework.Data.Geocache gc in Core.Geocaches)
                    {
                        gc.Selected = (gc.ContainsCustomLatLon || gc.CustomCoords);
                    }
                }
                else if (action == ACTION_MULTFOUNDS)
                {
                    foreach (Framework.Data.Geocache gc in Core.Geocaches)
                    {
                        gc.Selected = false;
                    }
                    string myName = Core.GeocachingComAccount.AccountName;
                    var fc = from Framework.Data.Geocache gc in Core.Geocaches where gc.Found select gc;
                    var list = from Framework.Data.Log l in Core.Logs
                               join Framework.Data.Geocache g in fc on l.GeocacheCode equals g.Code
                               where l.Finder == myName && l.LogType.AsFound
                               group g by g.Code into gr
                               select new { GeocacheCode = gr.Key, Founds = gr };
                    foreach (var li in list)
                    {
                        if (li.Founds.Count() > 1)
                        {
                            Utils.DataAccess.GetGeocache(Core.Geocaches, li.GeocacheCode).Selected = true;
                        }
                    }

                }
                else if (action == ACTION_USERWAYPOINTS)
                {
                    foreach (Framework.Data.Geocache gc in Core.Geocaches)
                    {
                        gc.Selected = gc.HasUserWaypoints;
                    }
                }
                Core.Geocaches.EndUpdate();
            }
            return result;
        }
    }
}
