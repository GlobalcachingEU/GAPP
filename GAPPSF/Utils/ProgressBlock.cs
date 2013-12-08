using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Utils
{
    public class ProgressBlock: IDisposable
    {
        static int level = 0;

        public ProgressBlock(string title, string action, int max, int value, bool canCancel)
        {
            level++;
            if (level == 1)
            {
                Dialogs.ProgessWindow.Instance.Start(
                    Localization.TranslationManager.Instance.TranslateText(title), 
                    Localization.TranslationManager.Instance.TranslateText(action), 
                    max, value, canCancel);
            }
            else
            {
                Dialogs.ProgessWindow.Instance.StartSub(Localization.TranslationManager.Instance.TranslateText(action), max, value);
            }
        }
        public ProgressBlock(string title, string action, int max, int value)
            : this(title, action, max, value, false)
        {
        }
        public ProgressBlock(string action, int max, int value)
            : this(null, action, max, value, false)
        {
        }

        public bool Update(string action, int max, int value)
        {
            if (level == 2)
            {
                Dialogs.ProgessWindow.Instance.ChangeProgressSub(Localization.TranslationManager.Instance.TranslateText(action), value, max);
            }
            else
            {
                Dialogs.ProgessWindow.Instance.ChangeProgress(Localization.TranslationManager.Instance.TranslateText(action), value, max);
            }
            return !Dialogs.ProgessWindow.Instance.Canceled;
        }

        public void Dispose()
        {
            if (level == 2)
            {
                Dialogs.ProgessWindow.Instance.StopSub();
            }
            else
            {
                Dialogs.ProgessWindow.Instance.Stop();
            }
            level--;
        }
    }
}
