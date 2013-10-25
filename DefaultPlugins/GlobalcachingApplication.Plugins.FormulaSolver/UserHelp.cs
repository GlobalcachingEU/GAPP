using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.IO;

namespace GlobalcachingApplication.Plugins.FormulaSolver
{
    public partial class UserHelp : Form
    {
        public UserHelp()
        {
            InitializeComponent();
        }

        private void UserHelp_Load(object sender, EventArgs e)
        {
            Assembly ass = Assembly.GetExecutingAssembly();
            Stream str = ass.GetManifestResourceStream("GlobalcachingApplication.Plugins.FormulaSolver.Documentation.DE-de.formulasolverdocs_de.html");
            webHelp.DocumentStream = str;
        }
    }
}
