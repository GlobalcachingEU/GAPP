namespace GlobalcachingApplication.Plugins.ExportGarminPOI
{
    partial class GarminPOIExportForm
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnPOIExportPath = new System.Windows.Forms.Button();
            this.tbPOIExportPath = new System.Windows.Forms.TextBox();
            this.lbPOIExportPath = new System.Windows.Forms.Label();
            this.buttonOK = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.grpExport = new System.Windows.Forms.GroupBox();
            this.cbExportWaypointPOIs = new System.Windows.Forms.CheckBox();
            this.cbExportGeocachePOIs = new System.Windows.Forms.CheckBox();
            this.cbClearExportDirectory = new System.Windows.Forms.CheckBox();
            this.grpLimits = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.numDescriptionLengthLimit = new System.Windows.Forms.NumericUpDown();
            this.lbDescriptionLengthLimit = new System.Windows.Forms.Label();
            this.lbNameLengthLimit = new System.Windows.Forms.Label();
            this.numNameLengthLimit = new System.Windows.Forms.NumericUpDown();
            this.grpPOILoader = new System.Windows.Forms.GroupBox();
            this.cbRunPOILoaderSilently = new System.Windows.Forms.CheckBox();
            this.cbPassDirectoryToPOILoader = new System.Windows.Forms.CheckBox();
            this.llbPOILoaderWeb = new System.Windows.Forms.LinkLabel();
            this.btnPOILoaderFilename = new System.Windows.Forms.Button();
            this.tbPOILoaderFilename = new System.Windows.Forms.TextBox();
            this.cbRunPOILoader = new System.Windows.Forms.CheckBox();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.grpName = new System.Windows.Forms.GroupBox();
            this.rbPOINameGCCode = new System.Windows.Forms.RadioButton();
            this.rbPOINameGCName = new System.Windows.Forms.RadioButton();
            this.grpExport.SuspendLayout();
            this.grpLimits.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numDescriptionLengthLimit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numNameLengthLimit)).BeginInit();
            this.grpPOILoader.SuspendLayout();
            this.grpName.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnPOIExportPath
            // 
            this.btnPOIExportPath.Location = new System.Drawing.Point(620, 50);
            this.btnPOIExportPath.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnPOIExportPath.Name = "btnPOIExportPath";
            this.btnPOIExportPath.Size = new System.Drawing.Size(44, 28);
            this.btnPOIExportPath.TabIndex = 2;
            this.btnPOIExportPath.Text = "...";
            this.btnPOIExportPath.UseVisualStyleBackColor = true;
            this.btnPOIExportPath.Click += new System.EventHandler(this.btnPOIExportPath_Click);
            // 
            // tbPOIExportPath
            // 
            this.tbPOIExportPath.Location = new System.Drawing.Point(23, 50);
            this.tbPOIExportPath.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbPOIExportPath.Name = "tbPOIExportPath";
            this.tbPOIExportPath.ReadOnly = true;
            this.tbPOIExportPath.Size = new System.Drawing.Size(588, 22);
            this.tbPOIExportPath.TabIndex = 1;
            this.tbPOIExportPath.TextChanged += new System.EventHandler(this.tbPOIExportPath_TextChanged);
            // 
            // lbPOIExportPath
            // 
            this.lbPOIExportPath.AutoSize = true;
            this.lbPOIExportPath.Location = new System.Drawing.Point(19, 30);
            this.lbPOIExportPath.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbPOIExportPath.Name = "lbPOIExportPath";
            this.lbPOIExportPath.Size = new System.Drawing.Size(266, 17);
            this.lbPOIExportPath.TabIndex = 6;
            this.lbPOIExportPath.Text = "Target directory for POI Loader CSV files";
            this.lbPOIExportPath.Click += new System.EventHandler(this.lblPOIExportPath_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Enabled = false;
            this.buttonOK.Location = new System.Drawing.Point(295, 569);
            this.buttonOK.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(116, 28);
            this.buttonOK.TabIndex = 15;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // grpExport
            // 
            this.grpExport.Controls.Add(this.cbExportWaypointPOIs);
            this.grpExport.Controls.Add(this.cbExportGeocachePOIs);
            this.grpExport.Controls.Add(this.cbClearExportDirectory);
            this.grpExport.Controls.Add(this.btnPOIExportPath);
            this.grpExport.Controls.Add(this.lbPOIExportPath);
            this.grpExport.Controls.Add(this.tbPOIExportPath);
            this.grpExport.Location = new System.Drawing.Point(16, 15);
            this.grpExport.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.grpExport.Name = "grpExport";
            this.grpExport.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.grpExport.Size = new System.Drawing.Size(672, 177);
            this.grpExport.TabIndex = 9;
            this.grpExport.TabStop = false;
            this.grpExport.Text = "Export";
            // 
            // cbExportWaypointPOIs
            // 
            this.cbExportWaypointPOIs.AutoSize = true;
            this.cbExportWaypointPOIs.Location = new System.Drawing.Point(23, 139);
            this.cbExportWaypointPOIs.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cbExportWaypointPOIs.Name = "cbExportWaypointPOIs";
            this.cbExportWaypointPOIs.Size = new System.Drawing.Size(217, 26);
            this.cbExportWaypointPOIs.TabIndex = 5;
            this.cbExportWaypointPOIs.Text = "Export waypoint POIs";
            this.cbExportWaypointPOIs.UseVisualStyleBackColor = true;
            this.cbExportWaypointPOIs.CheckedChanged += new System.EventHandler(this.cbExportWaypointPOIs_CheckedChanged);
            // 
            // cbExportGeocachePOIs
            // 
            this.cbExportGeocachePOIs.AutoSize = true;
            this.cbExportGeocachePOIs.Location = new System.Drawing.Point(23, 111);
            this.cbExportGeocachePOIs.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cbExportGeocachePOIs.Name = "cbExportGeocachePOIs";
            this.cbExportGeocachePOIs.Size = new System.Drawing.Size(227, 26);
            this.cbExportGeocachePOIs.TabIndex = 4;
            this.cbExportGeocachePOIs.Text = "Export geocache POIs";
            this.cbExportGeocachePOIs.UseVisualStyleBackColor = true;
            this.cbExportGeocachePOIs.CheckedChanged += new System.EventHandler(this.cbExportGeocachePOIs_CheckedChanged);
            // 
            // cbClearExportDirectory
            // 
            this.cbClearExportDirectory.AutoSize = true;
            this.cbClearExportDirectory.Location = new System.Drawing.Point(23, 82);
            this.cbClearExportDirectory.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cbClearExportDirectory.Name = "cbClearExportDirectory";
            this.cbClearExportDirectory.Size = new System.Drawing.Size(280, 26);
            this.cbClearExportDirectory.TabIndex = 3;
            this.cbClearExportDirectory.Text = "Clear directory before export";
            this.cbClearExportDirectory.UseVisualStyleBackColor = true;
            // 
            // grpLimits
            // 
            this.grpLimits.Controls.Add(this.label2);
            this.grpLimits.Controls.Add(this.label1);
            this.grpLimits.Controls.Add(this.numDescriptionLengthLimit);
            this.grpLimits.Controls.Add(this.lbDescriptionLengthLimit);
            this.grpLimits.Controls.Add(this.lbNameLengthLimit);
            this.grpLimits.Controls.Add(this.numNameLengthLimit);
            this.grpLimits.Location = new System.Drawing.Point(16, 262);
            this.grpLimits.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.grpLimits.Name = "grpLimits";
            this.grpLimits.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.grpLimits.Size = new System.Drawing.Size(672, 97);
            this.grpLimits.TabIndex = 11;
            this.grpLimits.TabStop = false;
            this.grpLimits.Text = "Limits";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(403, 58);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(218, 17);
            this.label2.TabIndex = 5;
            this.label2.Text = "Vista Hcx: 84, Oregon/Nüvi: 1023";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(403, 31);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(239, 17);
            this.label1.TabIndex = 4;
            this.label1.Text = "Vista Hcx: 22-40, Oregon/Nüvi: 1023";
            // 
            // numDescriptionLengthLimit
            // 
            this.numDescriptionLengthLimit.Location = new System.Drawing.Point(312, 58);
            this.numDescriptionLengthLimit.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.numDescriptionLengthLimit.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numDescriptionLengthLimit.Minimum = new decimal(new int[] {
            16,
            0,
            0,
            0});
            this.numDescriptionLengthLimit.Name = "numDescriptionLengthLimit";
            this.numDescriptionLengthLimit.Size = new System.Drawing.Size(83, 22);
            this.numDescriptionLengthLimit.TabIndex = 9;
            this.numDescriptionLengthLimit.Value = new decimal(new int[] {
            16,
            0,
            0,
            0});
            // 
            // lbDescriptionLengthLimit
            // 
            this.lbDescriptionLengthLimit.AutoSize = true;
            this.lbDescriptionLengthLimit.Location = new System.Drawing.Point(19, 60);
            this.lbDescriptionLengthLimit.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbDescriptionLengthLimit.Name = "lbDescriptionLengthLimit";
            this.lbDescriptionLengthLimit.Size = new System.Drawing.Size(184, 17);
            this.lbDescriptionLengthLimit.TabIndex = 2;
            this.lbDescriptionLengthLimit.Text = "Max. POI description length:";
            // 
            // lbNameLengthLimit
            // 
            this.lbNameLengthLimit.AutoSize = true;
            this.lbNameLengthLimit.Location = new System.Drawing.Point(19, 31);
            this.lbNameLengthLimit.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbNameLengthLimit.Name = "lbNameLengthLimit";
            this.lbNameLengthLimit.Size = new System.Drawing.Size(150, 17);
            this.lbNameLengthLimit.TabIndex = 1;
            this.lbNameLengthLimit.Text = "Max. POI name length:";
            // 
            // numNameLengthLimit
            // 
            this.numNameLengthLimit.Location = new System.Drawing.Point(312, 28);
            this.numNameLengthLimit.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.numNameLengthLimit.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numNameLengthLimit.Minimum = new decimal(new int[] {
            6,
            0,
            0,
            0});
            this.numNameLengthLimit.Name = "numNameLengthLimit";
            this.numNameLengthLimit.Size = new System.Drawing.Size(83, 22);
            this.numNameLengthLimit.TabIndex = 8;
            this.numNameLengthLimit.Value = new decimal(new int[] {
            6,
            0,
            0,
            0});
            // 
            // grpPOILoader
            // 
            this.grpPOILoader.Controls.Add(this.cbRunPOILoaderSilently);
            this.grpPOILoader.Controls.Add(this.cbPassDirectoryToPOILoader);
            this.grpPOILoader.Controls.Add(this.llbPOILoaderWeb);
            this.grpPOILoader.Controls.Add(this.btnPOILoaderFilename);
            this.grpPOILoader.Controls.Add(this.tbPOILoaderFilename);
            this.grpPOILoader.Controls.Add(this.cbRunPOILoader);
            this.grpPOILoader.Location = new System.Drawing.Point(16, 367);
            this.grpPOILoader.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.grpPOILoader.Name = "grpPOILoader";
            this.grpPOILoader.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.grpPOILoader.Size = new System.Drawing.Size(672, 183);
            this.grpPOILoader.TabIndex = 12;
            this.grpPOILoader.TabStop = false;
            this.grpPOILoader.Text = "POI Loader";
            // 
            // cbRunPOILoaderSilently
            // 
            this.cbRunPOILoaderSilently.AutoSize = true;
            this.cbRunPOILoaderSilently.Location = new System.Drawing.Point(23, 123);
            this.cbRunPOILoaderSilently.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cbRunPOILoaderSilently.Name = "cbRunPOILoaderSilently";
            this.cbRunPOILoaderSilently.Size = new System.Drawing.Size(137, 26);
            this.cbRunPOILoaderSilently.TabIndex = 14;
            this.cbRunPOILoaderSilently.Text = "Run silently";
            this.cbRunPOILoaderSilently.UseVisualStyleBackColor = true;
            // 
            // cbPassDirectoryToPOILoader
            // 
            this.cbPassDirectoryToPOILoader.AutoSize = true;
            this.cbPassDirectoryToPOILoader.Location = new System.Drawing.Point(23, 95);
            this.cbPassDirectoryToPOILoader.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cbPassDirectoryToPOILoader.Name = "cbPassDirectoryToPOILoader";
            this.cbPassDirectoryToPOILoader.Size = new System.Drawing.Size(283, 26);
            this.cbPassDirectoryToPOILoader.TabIndex = 13;
            this.cbPassDirectoryToPOILoader.Text = "Pass directory to POI Loader";
            this.cbPassDirectoryToPOILoader.UseVisualStyleBackColor = true;
            // 
            // llbPOILoaderWeb
            // 
            this.llbPOILoaderWeb.AutoSize = true;
            this.llbPOILoaderWeb.Location = new System.Drawing.Point(373, 151);
            this.llbPOILoaderWeb.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.llbPOILoaderWeb.Name = "llbPOILoaderWeb";
            this.llbPOILoaderWeb.Size = new System.Drawing.Size(274, 17);
            this.llbPOILoaderWeb.TabIndex = 15;
            this.llbPOILoaderWeb.TabStop = true;
            this.llbPOILoaderWeb.Text = "http://www.garmin.com/products/poiloader/";
            this.llbPOILoaderWeb.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llbPOILoaderWeb_LinkClicked);
            // 
            // btnPOILoaderFilename
            // 
            this.btnPOILoaderFilename.Location = new System.Drawing.Point(620, 48);
            this.btnPOILoaderFilename.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnPOILoaderFilename.Name = "btnPOILoaderFilename";
            this.btnPOILoaderFilename.Size = new System.Drawing.Size(44, 28);
            this.btnPOILoaderFilename.TabIndex = 12;
            this.btnPOILoaderFilename.Text = "...";
            this.btnPOILoaderFilename.UseVisualStyleBackColor = true;
            this.btnPOILoaderFilename.Click += new System.EventHandler(this.btnPOILoaderFilename_Click);
            // 
            // tbPOILoaderFilename
            // 
            this.tbPOILoaderFilename.Location = new System.Drawing.Point(23, 52);
            this.tbPOILoaderFilename.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbPOILoaderFilename.Name = "tbPOILoaderFilename";
            this.tbPOILoaderFilename.ReadOnly = true;
            this.tbPOILoaderFilename.Size = new System.Drawing.Size(588, 22);
            this.tbPOILoaderFilename.TabIndex = 11;
            // 
            // cbRunPOILoader
            // 
            this.cbRunPOILoader.AutoSize = true;
            this.cbRunPOILoader.Location = new System.Drawing.Point(23, 23);
            this.cbRunPOILoader.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cbRunPOILoader.Name = "cbRunPOILoader";
            this.cbRunPOILoader.Size = new System.Drawing.Size(277, 26);
            this.cbRunPOILoader.TabIndex = 10;
            this.cbRunPOILoader.Text = "Run POI Loader after export";
            this.cbRunPOILoader.UseVisualStyleBackColor = true;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.AddExtension = false;
            this.openFileDialog1.DefaultExt = "exe";
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.Filter = "Exe files (*.exe)|*.exe|Batch files (*.cmd)|*.cmd|Batch files (*.bat)|*.bat";
            // 
            // grpName
            // 
            this.grpName.Controls.Add(this.rbPOINameGCCode);
            this.grpName.Controls.Add(this.rbPOINameGCName);
            this.grpName.Location = new System.Drawing.Point(16, 199);
            this.grpName.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.grpName.Name = "grpName";
            this.grpName.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.grpName.Size = new System.Drawing.Size(672, 55);
            this.grpName.TabIndex = 10;
            this.grpName.TabStop = false;
            this.grpName.Text = "POI name";
            // 
            // rbPOINameGCCode
            // 
            this.rbPOINameGCCode.AutoSize = true;
            this.rbPOINameGCCode.Location = new System.Drawing.Point(151, 23);
            this.rbPOINameGCCode.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.rbPOINameGCCode.Name = "rbPOINameGCCode";
            this.rbPOINameGCCode.Size = new System.Drawing.Size(83, 26);
            this.rbPOINameGCCode.TabIndex = 7;
            this.rbPOINameGCCode.TabStop = true;
            this.rbPOINameGCCode.Text = "Code";
            this.rbPOINameGCCode.UseVisualStyleBackColor = true;
            // 
            // rbPOINameGCName
            // 
            this.rbPOINameGCName.AutoSize = true;
            this.rbPOINameGCName.Location = new System.Drawing.Point(23, 23);
            this.rbPOINameGCName.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.rbPOINameGCName.Name = "rbPOINameGCName";
            this.rbPOINameGCName.Size = new System.Drawing.Size(88, 26);
            this.rbPOINameGCName.TabIndex = 6;
            this.rbPOINameGCName.TabStop = true;
            this.rbPOINameGCName.Text = "Name";
            this.rbPOINameGCName.UseVisualStyleBackColor = true;
            this.rbPOINameGCName.CheckedChanged += new System.EventHandler(this.radioButton1_CheckedChanged);
            // 
            // GarminPOIExportForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(704, 617);
            this.Controls.Add(this.grpName);
            this.Controls.Add(this.grpPOILoader);
            this.Controls.Add(this.grpLimits);
            this.Controls.Add(this.grpExport);
            this.Controls.Add(this.buttonOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GarminPOIExportForm";
            this.ShowIcon = false;
            this.Text = "Export Garmin POI";
            this.grpExport.ResumeLayout(false);
            this.grpExport.PerformLayout();
            this.grpLimits.ResumeLayout(false);
            this.grpLimits.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numDescriptionLengthLimit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numNameLengthLimit)).EndInit();
            this.grpPOILoader.ResumeLayout(false);
            this.grpPOILoader.PerformLayout();
            this.grpName.ResumeLayout(false);
            this.grpName.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnPOIExportPath;
        private System.Windows.Forms.TextBox tbPOIExportPath;
        private System.Windows.Forms.Label lbPOIExportPath;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.GroupBox grpExport;
        private System.Windows.Forms.CheckBox cbClearExportDirectory;
        private System.Windows.Forms.CheckBox cbExportGeocachePOIs;
        private System.Windows.Forms.CheckBox cbExportWaypointPOIs;
        private System.Windows.Forms.GroupBox grpLimits;
        private System.Windows.Forms.NumericUpDown numNameLengthLimit;
        private System.Windows.Forms.NumericUpDown numDescriptionLengthLimit;
        private System.Windows.Forms.Label lbDescriptionLengthLimit;
        private System.Windows.Forms.Label lbNameLengthLimit;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox grpPOILoader;
        private System.Windows.Forms.Button btnPOILoaderFilename;
        private System.Windows.Forms.TextBox tbPOILoaderFilename;
        private System.Windows.Forms.CheckBox cbRunPOILoader;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.LinkLabel llbPOILoaderWeb;
        private System.Windows.Forms.CheckBox cbRunPOILoaderSilently;
        private System.Windows.Forms.CheckBox cbPassDirectoryToPOILoader;
        private System.Windows.Forms.GroupBox grpName;
        private System.Windows.Forms.RadioButton rbPOINameGCName;
        private System.Windows.Forms.RadioButton rbPOINameGCCode;
    }
}