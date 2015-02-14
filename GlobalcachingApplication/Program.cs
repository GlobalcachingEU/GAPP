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
            settings.LogSeverity = LogSeverity.Disable;
            settings.RegisterScheme(new CefCustomScheme
            {
                SchemeName = CefSharpSchemeHandlerFactory.SchemeName,
                SchemeHandlerFactory = new CefSharpSchemeHandlerFactory()
            });
            if (!Cef.Initialize(settings))
            {
                throw new Exception("Unable to Initialize Cef");
            } 
            MainForm = new FormMain();
        }
    }

    internal class CefSharpSchemeHandlerFactory : ISchemeHandlerFactory
    {
        public const string SchemeName = "gapp";

        public ISchemeHandler Create()
        {
            return new CefSharpSchemeHandler();
        }
    }

    internal class CefSharpSchemeHandler : ISchemeHandler
    {
        public CefSharpSchemeHandler()
        {
        }

        public bool ProcessRequestAsync(IRequest request, ISchemeHandlerResponse response, OnRequestCompletedHandler requestCompletedCallback)
        {
            //gapp:// ignore this
            // The 'host' portion is entirely ignored by this scheme handler.
            try
            {
                var uri = new Uri(request.Url);
                var fileName = uri.ToString().Substring(7).Replace('/', '\\').Insert(1, ":");
                if (System.IO.File.Exists(fileName))
                {
                    response.ResponseStream = System.IO.File.OpenRead(fileName);
                    response.MimeType = GetMimeType(fileName);
                    requestCompletedCallback.BeginInvoke(requestCompletedCallback.EndInvoke, null);

                    return true;
                }
            }
            catch
            {
            }
            return false;
        }

        private string GetMimeType(string fileName)
        {
            return ResourceHandler.GetMimeType(System.IO.Path.GetExtension(fileName));
        }
    }
}
