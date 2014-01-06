using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core
{
    public partial class Settings
    {
        public List<Attachement.Item> GetAttachements(string gcCode)
        {
            return _settingsStorage.GetAttachements(gcCode);
        }
        public void AddAttachement(Attachement.Item item)
        {
            _settingsStorage.AddAttachement(item);
        }
        public void DeleteAttachement(Attachement.Item item)
        {
            _settingsStorage.DeleteAttachement(item);
        }

        public int AttachementWindowWidth
        {
            get { return int.Parse(GetProperty("700")); }
            set { SetProperty(value.ToString()); }
        }
        public int AttachementWindowHeight
        {
            get { return int.Parse(GetProperty("700")); }
            set { SetProperty(value.ToString()); }
        }
        public int AttachementWindowTop
        {
            get { return int.Parse(GetProperty("100")); }
            set { SetProperty(value.ToString()); }
        }
        public int AttachementWindowLeft
        {
            get { return int.Parse(GetProperty("100")); }
            set { SetProperty(value.ToString()); }
        }

    }
}
