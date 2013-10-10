using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Framework.EventArguments
{
    public class FileDropEventArgs
    {
        public string[] FilePath { get; private set; }

        public FileDropEventArgs(string[] filePath)
        {
            FilePath = filePath;
        }
    }

    public delegate void FileDropEventHandler(object sender, FileDropEventArgs e);
}
