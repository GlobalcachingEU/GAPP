using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using ICSharpCode.SharpZipLib.Zip;

namespace GlobalcachingApplication.Updater
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            string exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string updatePath = System.IO.Path.Combine(new string[] { System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "GAPP", "Update" });

            //step 1: close Globalcaching.Application
            labelInfo.Text = "Stopping application...";
            labelInfo.Refresh();
            Process[] procs = System.Diagnostics.Process.GetProcessesByName("GAPP");
            DateTime startWait = DateTime.Now;
            TimeSpan maxWait = new TimeSpan(0, 0, 5);
            while (procs != null && procs.Length > 0 && (DateTime.Now - startWait) <= maxWait)
            {
                System.Threading.Thread.Sleep(1000);
                procs = System.Diagnostics.Process.GetProcessesByName("GAPP");
            }
            if (procs != null && procs.Length > 0)
            {
                foreach (Process p in procs)
                {
                    p.Kill();
                }
            }
            System.Threading.Thread.Sleep(1000);
            procs = System.Diagnostics.Process.GetProcessesByName("GAPP");
            if (procs != null && procs.Length > 0)
            {
                MessageBox.Show("Unable to close the application", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if (Directory.Exists(updatePath))
            {
                //step 2: copy all files with subfolders (with override) from AppData\GlobalcachingApplication\Update to exec\
                labelInfo.Text = "Copying files...";
                labelInfo.Refresh();
                string[] updateFiles = Directory.GetFiles(updatePath, "*.zip");
                if (updateFiles != null)
                {
                    foreach (string fn in updateFiles)
                    {
                        UnZipFiles(fn, exePath, true);
                    }
                }

                //step 3: deleting update files AppData\GlobalcachingApplication\Update
                labelInfo.Text = "Deleting update files...";
                labelInfo.Refresh();
            }

            //step 4: start Globalcaching.Application
            labelInfo.Text = "Starting application...";
            labelInfo.Refresh();
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = Path.Combine(exePath, "GAPP.exe");
            psi.WorkingDirectory = exePath;
            Process.Start(psi);

            //step 4: close Updater
            Close();
        }

        public static void UnZipFiles(string zipPathAndFile, string outputFolder, bool deleteZipFile)
        {
            ZipInputStream s = new ZipInputStream(File.OpenRead(zipPathAndFile));
            ZipEntry theEntry;
            string tmpEntry = String.Empty;
            while ((theEntry = s.GetNextEntry()) != null)
            {
                string directoryName = outputFolder;
                string fileName = Path.GetFileName(theEntry.Name);
                // create directory 
                if (directoryName != "")
                {
                    Directory.CreateDirectory(directoryName);
                }
                if (fileName != String.Empty)
                {
                    if (theEntry.Name.IndexOf("GlobalcachingApplication.Updater", StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        string fullPath = Path.Combine(directoryName , theEntry.Name);
                        fullPath = fullPath.Replace("\\ ", "\\");
                        string fullDirPath = Path.GetDirectoryName(fullPath);
                        if (!Directory.Exists(fullDirPath)) Directory.CreateDirectory(fullDirPath);
                        FileStream streamWriter = File.Create(fullPath);
                        int size = 2048;
                        byte[] data = new byte[2048];
                        while (true)
                        {
                            size = s.Read(data, 0, data.Length);
                            if (size > 0)
                            {
                                streamWriter.Write(data, 0, size);
                            }
                            else
                            {
                                break;
                            }
                        }
                        streamWriter.Close();
                    }
                }
            }
            s.Close();
            if (deleteZipFile)
                File.Delete(zipPathAndFile);
        }
    }
}
