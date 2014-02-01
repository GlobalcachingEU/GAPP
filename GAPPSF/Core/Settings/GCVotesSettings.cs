using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core
{
    public partial class Settings
    {
        public double? GetGCVoteMedian(string gcCode)
        {
            return _settingsStorage.GetGCVoteMedian(gcCode);
        }
        public double? GetGCVoteAverage(string gcCode)
        {
            return _settingsStorage.GetGCVoteAverage(gcCode);
        }
        public int? GetGCVoteCount(string gcCode)
        {
            return _settingsStorage.GetGCVoteCount(gcCode);
        }
        public double? GetGCVoteUser(string gcCode)
        {
            return _settingsStorage.GetGCVoteUser(gcCode);
        }
        public void SetGCVote(string gcCode, double median, double average, int cnt, double? user)
        {
            _settingsStorage.SetGCVote(gcCode, median, average, cnt, user);
        }
        public void ClearGCVotes()
        {
            _settingsStorage.ClearGCVotes();
        }
    }
}
