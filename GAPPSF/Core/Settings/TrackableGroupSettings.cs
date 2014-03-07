using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core
{
    public partial class Settings
    {
        public int TrackableGroupWindowWidth
        {
            get { return int.Parse(GetProperty("700")); }
            set { SetProperty(value.ToString()); }
        }
        public int TrackableGroupWindowHeight
        {
            get { return int.Parse(GetProperty("700")); }
            set { SetProperty(value.ToString()); }
        }
        public int TrackableGroupWindowTop
        {
            get { return int.Parse(GetProperty("100")); }
            set { SetProperty(value.ToString()); }
        }
        public int TrackableGroupWindowLeft
        {
            get { return int.Parse(GetProperty("100")); }
            set { SetProperty(value.ToString()); }
        }

        public List<UIControls.Trackables.TrackableGroup> GetTrackableGroups()
        {
            return _settingsStorage.GetTrackableGroups();
        }
        public void AddTrackableGroup(UIControls.Trackables.TrackableGroup grp)
        {
            _settingsStorage.AddTrackableGroup(grp);
        }
        public void DeleteTrackableGroup(UIControls.Trackables.TrackableGroup grp)
        {
            _settingsStorage.DeleteTrackableGroup(grp);
        }
        public List<UIControls.Trackables.TrackableItem> GetTrackables(UIControls.Trackables.TrackableGroup grp)
        {
            return _settingsStorage.GetTrackables(grp);
        }
        public void AddUpdateTrackable(UIControls.Trackables.TrackableGroup grp, UIControls.Trackables.TrackableItem trackable)
        {
            _settingsStorage.AddUpdateTrackable(grp, trackable);
        }
        public void DeleteTrackable(UIControls.Trackables.TrackableGroup grp, UIControls.Trackables.TrackableItem trackable)
        {
            _settingsStorage.DeleteTrackable(grp, trackable);
        }
        public List<UIControls.Trackables.TravelItem> GetTrackableTravels(UIControls.Trackables.TrackableItem trackable)
        {
            return _settingsStorage.GetTrackableTravels(trackable);
        }
        public void UpdateTrackableTravels(UIControls.Trackables.TrackableItem trackable, List<UIControls.Trackables.TravelItem> travels)
        {
            _settingsStorage.UpdateTrackableTravels(trackable, travels);
        }
        public List<UIControls.Trackables.LogItem> GetTrackableLogs(UIControls.Trackables.TrackableItem trackable)
        {
            return _settingsStorage.GetTrackableLogs(trackable);
        }
        public void UpdateTrackableLogs(UIControls.Trackables.TrackableItem trackable, List<UIControls.Trackables.LogItem> logs)
        {
            _settingsStorage.UpdateTrackableLogs(trackable, logs);
        }
        public byte[] GetTrackableIconData(string iconUrl)
        {
            return _settingsStorage.GetTrackableIconData(iconUrl);
        }

    }
}
