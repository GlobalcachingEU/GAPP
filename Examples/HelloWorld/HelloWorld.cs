using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GlobalcachingApplication.Framework;
using GlobalcachingApplication.Framework.Interfaces;
using GlobalcachingApplication.Utils;
using GlobalcachingApplication.Utils.BasePlugin;

namespace Examples.HelloWorld
{
    /// <summary>
    /// The Hello World demo plugin
    /// </summary>
    public class HelloWorld : Plugin
    {
        public const string ACTION_PERFORM = "Hello World 1";
        public const string ACTION_SUBPERFORM1 = "Hello World|Sub action 2";
        public const string ACTION_SUBPERFORM2 = "Hello World|Sub action 3";

        public const string ACTION_PERFORM_MSG = "Hello World 1!";
        public const string ACTION_SUBPERFORM1_MSG = "Hello World 2!";
        public const string ACTION_SUBPERFORM2_MSG = "Hello World 3!";

        /// <summary>
        /// Initialize the plugin
        /// Actions should be added here and additionel text for language translations
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public override bool Initialize(ICore core)
        {
            AddAction(ACTION_PERFORM);
            AddAction(ACTION_SUBPERFORM1);
            AddAction(ACTION_SUBPERFORM2);

            core.LanguageItems.AddText(ACTION_PERFORM_MSG);
            core.LanguageItems.AddText(ACTION_SUBPERFORM1_MSG);
            core.LanguageItems.AddText(ACTION_SUBPERFORM2_MSG);

            return base.Initialize(core);
        }

        /// <summary>
        /// Define the plugin type. This will provide help for the User Interface to position the menu items.
        /// </summary>
        public override PluginType PluginType
        {
            get { return PluginType.Action; }
        }

        /// <summary>
        /// Action to execute
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result && action == ACTION_PERFORM)
            {
                System.Windows.Forms.MessageBox.Show(LanguageSupport.Instance.GetTranslation(ACTION_PERFORM_MSG));
            }
            else if (result && action == ACTION_SUBPERFORM1)
            {
                System.Windows.Forms.MessageBox.Show(LanguageSupport.Instance.GetTranslation(ACTION_SUBPERFORM1_MSG));
            }
            else if (result && action == ACTION_SUBPERFORM1)
            {
                System.Windows.Forms.MessageBox.Show(LanguageSupport.Instance.GetTranslation(ACTION_SUBPERFORM2_MSG));
            }
            return result;
        }

    }
}
