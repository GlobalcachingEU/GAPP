using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Microsoft.VisualBasic.ApplicationServices;
using CefSharp;

namespace GlobalcachingApplication
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new FormMain());

            string[] args = Environment.GetCommandLineArgs();
            SingleInstanceController controller = new SingleInstanceController();
            controller.Run(args);
        }
    }

    public class SingleInstanceController : WindowsFormsApplicationBase
    {
        public SingleInstanceController()
        {
            IsSingleInstance = true;

            StartupNextInstance += this_StartupNextInstance;
        }

        void this_StartupNextInstance(object sender, StartupNextInstanceEventArgs e)
        {
            FormMain form = MainForm as FormMain; //My derived form type
            form.ProcessCommandLine((from string s in e.CommandLine select s).ToArray());
        }

        protected override void OnCreateMainForm()
        {
            var settings = new CefSettings();
            if (!Cef.Initialize(settings))
            {
                throw new Exception("Unable to Initialize Cef");
            } 
            MainForm = new FormMain();
        }
    }
}
