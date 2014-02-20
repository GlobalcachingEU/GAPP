using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Chat
{
    public class UserInRoomInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string Username { get; set; }
        public string ID { get; set; }
        public bool present { get; set; }
        public bool CanBeFollowed { get; set; }
        public string ActiveGeocache { get; set; }
        public bool FollowThisUser { get; set; }
        public int SelectionCount { get; set; }

        public string Text
        {
            get { return this.ToString(); }
        }

        public void UpdateText()
        {
            if (PropertyChanged!=null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("Text"));
            }
        }

        public override string ToString()
        {
            if (CanBeFollowed)
            {
                return string.Format("{2}{0} ({1}, {3})", Username, ActiveGeocache ?? "-", FollowThisUser ? "+" : "", SelectionCount >= 0 ? SelectionCount.ToString() : "?");
            }
            else
            {
                return Username;
            }
        }

    }
}
