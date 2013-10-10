namespace GlobalcachingApplication.Plugins.SimpleCacheList
{
    partial class SimpleCacheListForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SimpleCacheListForm));
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.button5 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.button4 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.checkedListBoxVisibleColumns = new System.Windows.Forms.CheckedListBox();
            this.checkBoxShowSelectedOnly = new System.Windows.Forms.CheckBox();
            this.buttonFilter = new System.Windows.Forms.Button();
            this.checkBoxShowFlaggedOnly = new System.Windows.Forms.CheckBox();
            this.textBoxFilter = new System.Windows.Forms.TextBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.button1 = new System.Windows.Forms.Button();
            this.elementHost1 = new System.Windows.Forms.Integration.ElementHost();
            this.cacheListControl1 = new GlobalcachingApplication.Plugins.SimpleCacheList.CacheListControl();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.viewGeocacheToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewGeocacheFromOriginalLocationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setCenterLocationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.geocacheEditorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.waypointEditorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editNoteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setAdditionalCoordsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearAdditionalCoordsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.deleteGeocacheToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panel1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel2.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.AutoScroll = true;
            this.panel1.Controls.Add(this.panel3);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1123, 114);
            this.panel1.TabIndex = 0;
            this.panel1.MouseEnter += new System.EventHandler(this.panel1_MouseEnter);
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.button5);
            this.panel3.Controls.Add(this.textBox1);
            this.panel3.Controls.Add(this.button4);
            this.panel3.Controls.Add(this.button3);
            this.panel3.Controls.Add(this.comboBox1);
            this.panel3.Controls.Add(this.label1);
            this.panel3.Controls.Add(this.button2);
            this.panel3.Controls.Add(this.checkedListBoxVisibleColumns);
            this.panel3.Controls.Add(this.checkBoxShowSelectedOnly);
            this.panel3.Controls.Add(this.buttonFilter);
            this.panel3.Controls.Add(this.checkBoxShowFlaggedOnly);
            this.panel3.Controls.Add(this.textBoxFilter);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel3.Location = new System.Drawing.Point(31, 0);
            this.panel3.Margin = new System.Windows.Forms.Padding(4);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(1060, 114);
            this.panel3.TabIndex = 6;
            // 
            // button5
            // 
            this.button5.Enabled = false;
            this.button5.Image = global::GlobalcachingApplication.Plugins.SimpleCacheList.Properties.Resources.save;
            this.button5.Location = new System.Drawing.Point(976, 61);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(31, 23);
            this.button5.TabIndex = 11;
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(811, 62);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(159, 22);
            this.textBox1.TabIndex = 10;
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // button4
            // 
            this.button4.Enabled = false;
            this.button4.Image = global::GlobalcachingApplication.Plugins.SimpleCacheList.Properties.Resources.delete;
            this.button4.Location = new System.Drawing.Point(1014, 32);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(31, 23);
            this.button4.TabIndex = 9;
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button3
            // 
            this.button3.Enabled = false;
            this.button3.Image = global::GlobalcachingApplication.Plugins.SimpleCacheList.Properties.Resources.apply;
            this.button3.Location = new System.Drawing.Point(977, 32);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(31, 23);
            this.button3.TabIndex = 8;
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // comboBox1
            // 
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(811, 32);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(159, 24);
            this.comboBox1.TabIndex = 7;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(808, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(49, 17);
            this.label1.TabIndex = 6;
            this.label1.Text = "Preset";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(251, 66);
            this.button2.Margin = new System.Windows.Forms.Padding(4);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(29, 28);
            this.button2.TabIndex = 5;
            this.button2.Text = "S";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // checkedListBoxVisibleColumns
            // 
            this.checkedListBoxVisibleColumns.CheckOnClick = true;
            this.checkedListBoxVisibleColumns.FormattingEnabled = true;
            this.checkedListBoxVisibleColumns.Location = new System.Drawing.Point(296, 11);
            this.checkedListBoxVisibleColumns.Margin = new System.Windows.Forms.Padding(4);
            this.checkedListBoxVisibleColumns.MultiColumn = true;
            this.checkedListBoxVisibleColumns.Name = "checkedListBoxVisibleColumns";
            this.checkedListBoxVisibleColumns.Size = new System.Drawing.Size(504, 89);
            this.checkedListBoxVisibleColumns.TabIndex = 1;
            this.checkedListBoxVisibleColumns.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.checkedListBoxVisibleColumns_ItemCheck);
            // 
            // checkBoxShowSelectedOnly
            // 
            this.checkBoxShowSelectedOnly.AutoSize = true;
            this.checkBoxShowSelectedOnly.Location = new System.Drawing.Point(16, 12);
            this.checkBoxShowSelectedOnly.Margin = new System.Windows.Forms.Padding(4);
            this.checkBoxShowSelectedOnly.Name = "checkBoxShowSelectedOnly";
            this.checkBoxShowSelectedOnly.Size = new System.Drawing.Size(151, 21);
            this.checkBoxShowSelectedOnly.TabIndex = 0;
            this.checkBoxShowSelectedOnly.Text = "Show selected only";
            this.checkBoxShowSelectedOnly.UseVisualStyleBackColor = true;
            this.checkBoxShowSelectedOnly.CheckedChanged += new System.EventHandler(this.checkBoxShowSelectedOnly_CheckedChanged);
            // 
            // buttonFilter
            // 
            this.buttonFilter.Image = global::GlobalcachingApplication.Plugins.SimpleCacheList.Properties.Resources.filter;
            this.buttonFilter.Location = new System.Drawing.Point(212, 66);
            this.buttonFilter.Margin = new System.Windows.Forms.Padding(4);
            this.buttonFilter.Name = "buttonFilter";
            this.buttonFilter.Size = new System.Drawing.Size(31, 28);
            this.buttonFilter.TabIndex = 4;
            this.buttonFilter.UseVisualStyleBackColor = true;
            this.buttonFilter.Click += new System.EventHandler(this.buttonFilter_Click);
            // 
            // checkBoxShowFlaggedOnly
            // 
            this.checkBoxShowFlaggedOnly.AutoSize = true;
            this.checkBoxShowFlaggedOnly.Location = new System.Drawing.Point(16, 41);
            this.checkBoxShowFlaggedOnly.Margin = new System.Windows.Forms.Padding(4);
            this.checkBoxShowFlaggedOnly.Name = "checkBoxShowFlaggedOnly";
            this.checkBoxShowFlaggedOnly.Size = new System.Drawing.Size(145, 21);
            this.checkBoxShowFlaggedOnly.TabIndex = 2;
            this.checkBoxShowFlaggedOnly.Text = "Show flagged only";
            this.checkBoxShowFlaggedOnly.UseVisualStyleBackColor = true;
            this.checkBoxShowFlaggedOnly.CheckedChanged += new System.EventHandler(this.checkBoxShowFlaggedOnly_CheckedChanged);
            // 
            // textBoxFilter
            // 
            this.textBoxFilter.Location = new System.Drawing.Point(16, 70);
            this.textBoxFilter.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxFilter.Name = "textBoxFilter";
            this.textBoxFilter.Size = new System.Drawing.Size(187, 22);
            this.textBoxFilter.TabIndex = 3;
            this.textBoxFilter.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxFilter_KeyPress);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.button1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Margin = new System.Windows.Forms.Padding(4);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(31, 114);
            this.panel2.TabIndex = 5;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(1, 0);
            this.button1.Margin = new System.Windows.Forms.Padding(4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(29, 28);
            this.button1.TabIndex = 0;
            this.button1.Text = "^";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // elementHost1
            // 
            this.elementHost1.ContextMenuStrip = this.contextMenuStrip1;
            this.elementHost1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.elementHost1.Location = new System.Drawing.Point(0, 114);
            this.elementHost1.Margin = new System.Windows.Forms.Padding(4);
            this.elementHost1.Name = "elementHost1";
            this.elementHost1.Size = new System.Drawing.Size(1123, 516);
            this.elementHost1.TabIndex = 3;
            this.elementHost1.Text = "elementHost1";
            this.elementHost1.Child = this.cacheListControl1;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.viewGeocacheToolStripMenuItem,
            this.viewGeocacheFromOriginalLocationToolStripMenuItem,
            this.setCenterLocationToolStripMenuItem,
            this.toolStripMenuItem1,
            this.geocacheEditorToolStripMenuItem,
            this.waypointEditorToolStripMenuItem,
            this.editNoteToolStripMenuItem,
            this.setAdditionalCoordsToolStripMenuItem,
            this.clearAdditionalCoordsToolStripMenuItem,
            this.toolStripMenuItem2,
            this.deleteGeocacheToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(328, 254);
            // 
            // viewGeocacheToolStripMenuItem
            // 
            this.viewGeocacheToolStripMenuItem.Name = "viewGeocacheToolStripMenuItem";
            this.viewGeocacheToolStripMenuItem.Size = new System.Drawing.Size(327, 24);
            this.viewGeocacheToolStripMenuItem.Text = "View geocache";
            this.viewGeocacheToolStripMenuItem.Click += new System.EventHandler(this.viewGeocacheToolStripMenuItem_Click);
            // 
            // viewGeocacheFromOriginalLocationToolStripMenuItem
            // 
            this.viewGeocacheFromOriginalLocationToolStripMenuItem.Name = "viewGeocacheFromOriginalLocationToolStripMenuItem";
            this.viewGeocacheFromOriginalLocationToolStripMenuItem.Size = new System.Drawing.Size(327, 24);
            this.viewGeocacheFromOriginalLocationToolStripMenuItem.Text = "View geocache from original location";
            this.viewGeocacheFromOriginalLocationToolStripMenuItem.Click += new System.EventHandler(this.viewGeocacheFromOriginalLocationToolStripMenuItem_Click);
            // 
            // setCenterLocationToolStripMenuItem
            // 
            this.setCenterLocationToolStripMenuItem.Name = "setCenterLocationToolStripMenuItem";
            this.setCenterLocationToolStripMenuItem.Size = new System.Drawing.Size(327, 24);
            this.setCenterLocationToolStripMenuItem.Text = "Set center location";
            this.setCenterLocationToolStripMenuItem.Click += new System.EventHandler(this.setCenterLocationToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(324, 6);
            // 
            // geocacheEditorToolStripMenuItem
            // 
            this.geocacheEditorToolStripMenuItem.Name = "geocacheEditorToolStripMenuItem";
            this.geocacheEditorToolStripMenuItem.Size = new System.Drawing.Size(327, 24);
            this.geocacheEditorToolStripMenuItem.Text = "Geocache editor";
            this.geocacheEditorToolStripMenuItem.Click += new System.EventHandler(this.geocacheEditorToolStripMenuItem_Click);
            // 
            // waypointEditorToolStripMenuItem
            // 
            this.waypointEditorToolStripMenuItem.Name = "waypointEditorToolStripMenuItem";
            this.waypointEditorToolStripMenuItem.Size = new System.Drawing.Size(327, 24);
            this.waypointEditorToolStripMenuItem.Text = "Waypoint editor";
            this.waypointEditorToolStripMenuItem.Click += new System.EventHandler(this.waypointEditorToolStripMenuItem_Click);
            // 
            // editNoteToolStripMenuItem
            // 
            this.editNoteToolStripMenuItem.Name = "editNoteToolStripMenuItem";
            this.editNoteToolStripMenuItem.Size = new System.Drawing.Size(327, 24);
            this.editNoteToolStripMenuItem.Text = "Edit note";
            this.editNoteToolStripMenuItem.Click += new System.EventHandler(this.editNoteToolStripMenuItem_Click);
            // 
            // setAdditionalCoordsToolStripMenuItem
            // 
            this.setAdditionalCoordsToolStripMenuItem.Name = "setAdditionalCoordsToolStripMenuItem";
            this.setAdditionalCoordsToolStripMenuItem.Size = new System.Drawing.Size(327, 24);
            this.setAdditionalCoordsToolStripMenuItem.Text = "Set additional coords";
            this.setAdditionalCoordsToolStripMenuItem.Click += new System.EventHandler(this.setAdditionalCoordsToolStripMenuItem_Click);
            // 
            // clearAdditionalCoordsToolStripMenuItem
            // 
            this.clearAdditionalCoordsToolStripMenuItem.Name = "clearAdditionalCoordsToolStripMenuItem";
            this.clearAdditionalCoordsToolStripMenuItem.Size = new System.Drawing.Size(327, 24);
            this.clearAdditionalCoordsToolStripMenuItem.Text = "Clear additional coords";
            this.clearAdditionalCoordsToolStripMenuItem.Click += new System.EventHandler(this.clearAdditionalCoordsToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(324, 6);
            // 
            // deleteGeocacheToolStripMenuItem
            // 
            this.deleteGeocacheToolStripMenuItem.Name = "deleteGeocacheToolStripMenuItem";
            this.deleteGeocacheToolStripMenuItem.Size = new System.Drawing.Size(327, 24);
            this.deleteGeocacheToolStripMenuItem.Text = "Delete";
            this.deleteGeocacheToolStripMenuItem.Click += new System.EventHandler(this.deleteGeocacheToolStripMenuItem_Click);
            // 
            // SimpleCacheListForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1123, 630);
            this.Controls.Add(this.elementHost1);
            this.Controls.Add(this.panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(5);
            this.Name = "SimpleCacheListForm";
            this.ShowIcon = false;
            this.Text = "SimpleCacheListForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SimpleCacheListForm_FormClosing);
            this.Shown += new System.EventHandler(this.SimpleCacheListForm_Shown);
            this.EnabledChanged += new System.EventHandler(this.SimpleCacheListForm_EnabledChanged);
            this.LocationChanged += new System.EventHandler(this.SimpleCacheListForm_LocationChanged);
            this.SizeChanged += new System.EventHandler(this.SimpleCacheListForm_SizeChanged);
            this.panel1.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Integration.ElementHost elementHost1;
        private CacheListControl cacheListControl1;
        private System.Windows.Forms.CheckBox checkBoxShowSelectedOnly;
        private System.Windows.Forms.CheckedListBox checkedListBoxVisibleColumns;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem viewGeocacheToolStripMenuItem;
        private System.Windows.Forms.CheckBox checkBoxShowFlaggedOnly;
        private System.Windows.Forms.ToolStripMenuItem editNoteToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem setCenterLocationToolStripMenuItem;
        private System.Windows.Forms.Button buttonFilter;
        private System.Windows.Forms.TextBox textBoxFilter;
        private System.Windows.Forms.ToolStripMenuItem setAdditionalCoordsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearAdditionalCoordsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewGeocacheFromOriginalLocationToolStripMenuItem;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.ToolStripMenuItem geocacheEditorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem waypointEditorToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem deleteGeocacheToolStripMenuItem;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Label label1;
    }
}