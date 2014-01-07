using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GAPPSF.UIControls.FormulaSolver.FormulaInterpreter
{
    public class WriteToFile
    {
        private readonly string fileName;
        private readonly string path;

        public WriteToFile(string fileName): this(fileName, null) { }
        public WriteToFile(string fileName, string path)
        {
            this.fileName = fileName;
            if (path == null)
            {
                this.path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
            else
            {
                this.path = path;
            }
        }

        public void AppendString(string toAppend)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("= = = = = = = = = =");
            sb.AppendLine(toAppend);
            sb.AppendLine();
            sb.AppendLine();

            using (StreamWriter outfile = new StreamWriter(path + @"\" + fileName, true))
            {
                outfile.Write(sb.ToString());
            }
        }
    }
}
