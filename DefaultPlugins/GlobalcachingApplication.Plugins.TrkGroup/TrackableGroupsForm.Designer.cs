namespace GlobalcachingApplication.Plugins.TrkGroup
{
    partial class TrackableGroupsForm
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
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.panel3 = new System.Windows.Forms.Panel();
            this.buttonAddYouOwn = new System.Windows.Forms.Button();
            this.buttonAddTrackables = new System.Windows.Forms.Button();
            this.textBoxTBCodes = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader9 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.goToTrackablePageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.goToGeocacheToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showRouteOfSelectedTrackableToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBoxGroup = new System.Windows.Forms.ComboBox();
            this.textBoxGroupName = new System.Windows.Forms.TextBox();
            this.buttonGroupCreate = new System.Windows.Forms.Button();
            this.buttonGroupRename = new System.Windows.Forms.Button();
            this.buttonGroupDelete = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.trackablesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showAllOnMapToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showSelectedOnMapToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showRouteOfSelectedTrackableToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.updateSelectedTrackablesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.updateAllTrackablesInGroupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.deleteSelectedTrackablesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panel3.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // imageList1
            // 
            this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageList1.ImageSize = new System.Drawing.Size(24, 24);
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.buttonAddYouOwn);
            this.panel3.Controls.Add(this.buttonAddTrackables);
            this.panel3.Controls.Add(this.textBoxTBCodes);
            this.panel3.Controls.Add(this.label3);
            this.panel3.Controls.Add(this.label4);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel3.Location = new System.Drawing.Point(0, 0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(732, 42);
            this.panel3.TabIndex = 0;
            // 
            // buttonAddYouOwn
            // 
            this.buttonAddYouOwn.Enabled = false;
            this.buttonAddYouOwn.Location = new System.Drawing.Point(464, 8);
            this.buttonAddYouOwn.Name = "buttonAddYouOwn";
            this.buttonAddYouOwn.Size = new System.Drawing.Size(125, 23);
            this.buttonAddYouOwn.TabIndex = 6;
            this.buttonAddYouOwn.Text = "Add your own";
            this.buttonAddYouOwn.UseVisualStyleBackColor = true;
            this.buttonAddYouOwn.Click += new System.EventHandler(this.buttonAddYouOwn_Click);
            // 
            // buttonAddTrackables
            // 
            this.buttonAddTrackables.Enabled = false;
            this.buttonAddTrackables.Location = new System.Drawing.Point(366, 8);
            this.buttonAddTrackables.Name = "buttonAddTrackables";
            this.buttonAddTrackables.Size = new System.Drawing.Size(92, 23);
            this.buttonAddTrackables.TabIndex = 5;
            this.buttonAddTrackables.Text = "Add";
            this.buttonAddTrackables.UseVisualStyleBackColor = true;
            this.buttonAddTrackables.Click += new System.EventHandler(this.buttonAddTrackables_Click);
            // 
            // textBoxTBCodes
            // 
            this.textBoxTBCodes.Enabled = false;
            this.textBoxTBCodes.Location = new System.Drawing.Point(135, 10);
            this.textBoxTBCodes.Name = "textBoxTBCodes";
            this.textBoxTBCodes.Size = new System.Drawing.Size(225, 20);
            this.textBoxTBCodes.TabIndex = 4;
            this.textBoxTBCodes.TextChanged += new System.EventHandler(this.textBoxTBCodes_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(119, 13);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(10, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = ":";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(11, 13);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(94, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "Trackable Code(s)";
            // 
            // listView1
            // 
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader3,
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader9,
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader6,
            this.columnHeader7,
            this.columnHeader8});
            this.listView1.ContextMenuStrip = this.contextMenuStrip1;
            this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView1.FullRowSelect = true;
            this.listView1.HideSelection = false;
            this.listView1.LargeImageList = this.imageList1;
            this.listView1.Location = new System.Drawing.Point(0, 42);
            this.listView1.Name = "listView1";
            this.listView1.OwnerDraw = true;
            this.listView1.Size = new System.Drawing.Size(732, 473);
            this.listView1.SmallImageList = this.imageList1;
            this.listView1.TabIndex = 1;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView1_ColumnClick);
            this.listView1.DrawColumnHeader += new System.Windows.Forms.DrawListViewColumnHeaderEventHandler(this.listView1_DrawColumnHeader);
            this.listView1.DrawItem += new System.Windows.Forms.DrawListViewItemEventHandler(this.listView1_DrawItem);
            this.listView1.DrawSubItem += new System.Windows.Forms.DrawListViewSubItemEventHandler(this.listView1_DrawSubItem);
            this.listView1.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Icon";
            this.columnHeader3.Width = 42;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Code";
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Name";
            this.columnHeader2.Width = 149;
            // 
            // columnHeader9
            // 
            this.columnHeader9.Text = "Owner";
            this.columnHeader9.Width = 103;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Last Cache";
            this.columnHeader4.Width = 75;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Hops";
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "Drops";
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "Disc.";
            // 
            // columnHeader8
            // 
            this.columnHeader8.Text = "Km";
            this.columnHeader8.Width = 68;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.goToTrackablePageToolStripMenuItem,
            this.goToGeocacheToolStripMenuItem,
            this.showRouteOfSelectedTrackableToolStripMenuItem1,
            this.toolStripMenuItem3,
            this.deleteToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(184, 120);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // goToTrackablePageToolStripMenuItem
            // 
            this.goToTrackablePageToolStripMenuItem.Name = "goToTrackablePageToolStripMenuItem";
            this.goToTrackablePageToolStripMenuItem.Size = new System.Drawing.Size(245, 22);
            this.goToTrackablePageToolStripMenuItem.Text = "Go to trackable page";
            this.goToTrackablePageToolStripMenuItem.Click += new System.EventHandler(this.goToTrackablePageToolStripMenuItem_Click);
            // 
            // goToGeocacheToolStripMenuItem
            // 
            this.goToGeocacheToolStripMenuItem.Name = "goToGeocacheToolStripMenuItem";
            this.goToGeocacheToolStripMenuItem.Size = new System.Drawing.Size(245, 22);
            this.goToGeocacheToolStripMenuItem.Text = "Go to geocache";
            this.goToGeocacheToolStripMenuItem.Click += new System.EventHandler(this.goToGeocacheToolStripMenuItem_Click);
            // 
            // showRouteOfSelectedTrackableToolStripMenuItem1
            // 
            this.showRouteOfSelectedTrackableToolStripMenuItem1.Name = "showRouteOfSelectedTrackableToolStripMenuItem1";
            this.showRouteOfSelectedTrackableToolStripMenuItem1.Size = new System.Drawing.Size(183, 22);
            this.showRouteOfSelectedTrackableToolStripMenuItem1.Text = "Show route";
            this.showRouteOfSelectedTrackableToolStripMenuItem1.Click += new System.EventHandler(this.showRouteOfSelectedTrackableToolStripMenuItem_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(242, 6);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(245, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteSelectedTrackablesToolStripMenuItem_Click);
            // 
            // panel2
            // 
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel2.Controls.Add(this.listView1);
            this.panel2.Controls.Add(this.panel3);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 92);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(736, 519);
            this.panel2.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(85, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Trackable group";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(119, 11);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(10, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = ":";
            // 
            // comboBoxGroup
            // 
            this.comboBoxGroup.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxGroup.FormattingEnabled = true;
            this.comboBoxGroup.Location = new System.Drawing.Point(135, 8);
            this.comboBoxGroup.Name = "comboBoxGroup";
            this.comboBoxGroup.Size = new System.Drawing.Size(225, 21);
            this.comboBoxGroup.TabIndex = 2;
            this.comboBoxGroup.SelectedValueChanged += new System.EventHandler(this.comboBoxGroup_SelectedValueChanged);
            // 
            // textBoxGroupName
            // 
            this.textBoxGroupName.Location = new System.Drawing.Point(135, 36);
            this.textBoxGroupName.Name = "textBoxGroupName";
            this.textBoxGroupName.Size = new System.Drawing.Size(225, 20);
            this.textBoxGroupName.TabIndex = 3;
            this.textBoxGroupName.TextChanged += new System.EventHandler(this.textBoxGroupName_TextChanged);
            // 
            // buttonGroupCreate
            // 
            this.buttonGroupCreate.Enabled = false;
            this.buttonGroupCreate.Location = new System.Drawing.Point(366, 36);
            this.buttonGroupCreate.Name = "buttonGroupCreate";
            this.buttonGroupCreate.Size = new System.Drawing.Size(93, 23);
            this.buttonGroupCreate.TabIndex = 4;
            this.buttonGroupCreate.Text = "Create";
            this.buttonGroupCreate.UseVisualStyleBackColor = true;
            this.buttonGroupCreate.Click += new System.EventHandler(this.buttonGroupCreate_Click);
            // 
            // buttonGroupRename
            // 
            this.buttonGroupRename.Enabled = false;
            this.buttonGroupRename.Location = new System.Drawing.Point(465, 36);
            this.buttonGroupRename.Name = "buttonGroupRename";
            this.buttonGroupRename.Size = new System.Drawing.Size(124, 23);
            this.buttonGroupRename.TabIndex = 5;
            this.buttonGroupRename.Text = "Rename";
            this.buttonGroupRename.UseVisualStyleBackColor = true;
            this.buttonGroupRename.Click += new System.EventHandler(this.buttonGroupRename_Click);
            // 
            // buttonGroupDelete
            // 
            this.buttonGroupDelete.Enabled = false;
            this.buttonGroupDelete.Location = new System.Drawing.Point(366, 7);
            this.buttonGroupDelete.Name = "buttonGroupDelete";
            this.buttonGroupDelete.Size = new System.Drawing.Size(93, 23);
            this.buttonGroupDelete.TabIndex = 6;
            this.buttonGroupDelete.Text = "Delete";
            this.buttonGroupDelete.UseVisualStyleBackColor = true;
            this.buttonGroupDelete.Click += new System.EventHandler(this.buttonGroupDelete_Click);
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel1.Controls.Add(this.buttonGroupDelete);
            this.panel1.Controls.Add(this.buttonGroupRename);
            this.panel1.Controls.Add(this.buttonGroupCreate);
            this.panel1.Controls.Add(this.textBoxGroupName);
            this.panel1.Controls.Add(this.comboBoxGroup);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 24);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(736, 68);
            this.panel1.TabIndex = 1;
            // 
            // menuStrip1
            // 
            this.menuStrip1.AllowMerge = false;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.trackablesToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(736, 24);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // trackablesToolStripMenuItem
            // 
            this.trackablesToolStripMenuItem.BackColor = System.Drawing.Color.GreenYellow;
            this.trackablesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showAllOnMapToolStripMenuItem,
            this.showSelectedOnMapToolStripMenuItem,
            this.showRouteOfSelectedTrackableToolStripMenuItem,
            this.toolStripMenuItem1,
            this.updateSelectedTrackablesToolStripMenuItem,
            this.updateAllTrackablesInGroupToolStripMenuItem,
            this.toolStripMenuItem2,
            this.deleteSelectedTrackablesToolStripMenuItem});
            this.trackablesToolStripMenuItem.Name = "trackablesToolStripMenuItem";
            this.trackablesToolStripMenuItem.Size = new System.Drawing.Size(75, 20);
            this.trackablesToolStripMenuItem.Text = "Trackables";
            // 
            // showAllOnMapToolStripMenuItem
            // 
            this.showAllOnMapToolStripMenuItem.Enabled = false;
            this.showAllOnMapToolStripMenuItem.Name = "showAllOnMapToolStripMenuItem";
            this.showAllOnMapToolStripMenuItem.Size = new System.Drawing.Size(245, 22);
            this.showAllOnMapToolStripMenuItem.Text = "Show group on map...";
            this.showAllOnMapToolStripMenuItem.Click += new System.EventHandler(this.showAllOnMapToolStripMenuItem_Click);
            // 
            // showSelectedOnMapToolStripMenuItem
            // 
            this.showSelectedOnMapToolStripMenuItem.Enabled = false;
            this.showSelectedOnMapToolStripMenuItem.Name = "showSelectedOnMapToolStripMenuItem";
            this.showSelectedOnMapToolStripMenuItem.Size = new System.Drawing.Size(245, 22);
            this.showSelectedOnMapToolStripMenuItem.Text = "Show selected on map...";
            this.showSelectedOnMapToolStripMenuItem.Click += new System.EventHandler(this.showSelectedOnMapToolStripMenuItem_Click);
            // 
            // showRouteOfSelectedTrackableToolStripMenuItem
            // 
            this.showRouteOfSelectedTrackableToolStripMenuItem.Enabled = false;
            this.showRouteOfSelectedTrackableToolStripMenuItem.Name = "showRouteOfSelectedTrackableToolStripMenuItem";
            this.showRouteOfSelectedTrackableToolStripMenuItem.Size = new System.Drawing.Size(245, 22);
            this.showRouteOfSelectedTrackableToolStripMenuItem.Text = "Show route of selected trackable";
            this.showRouteOfSelectedTrackableToolStripMenuItem.Click += new System.EventHandler(this.showRouteOfSelectedTrackableToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(242, 6);
            // 
            // updateSelectedTrackablesToolStripMenuItem
            // 
            this.updateSelectedTrackablesToolStripMenuItem.Enabled = false;
            this.updateSelectedTrackablesToolStripMenuItem.Name = "updateSelectedTrackablesToolStripMenuItem";
            this.updateSelectedTrackablesToolStripMenuItem.Size = new System.Drawing.Size(245, 22);
            this.updateSelectedTrackablesToolStripMenuItem.Text = "Update selected trackables";
            this.updateSelectedTrackablesToolStripMenuItem.Click += new System.EventHandler(this.updateSelectedTrackablesToolStripMenuItem_Click);
            // 
            // updateAllTrackablesInGroupToolStripMenuItem
            // 
            this.updateAllTrackablesInGroupToolStripMenuItem.Enabled = false;
            this.updateAllTrackablesInGroupToolStripMenuItem.Name = "updateAllTrackablesInGroupToolStripMenuItem";
            this.updateAllTrackablesInGroupToolStripMenuItem.Size = new System.Drawing.Size(245, 22);
            this.updateAllTrackablesInGroupToolStripMenuItem.Text = "Update all trackables in group";
            this.updateAllTrackablesInGroupToolStripMenuItem.Click += new System.EventHandler(this.updateAllTrackablesInGroupToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(242, 6);
            // 
            // deleteSelectedTrackablesToolStripMenuItem
            // 
            this.deleteSelectedTrackablesToolStripMenuItem.Enabled = false;
            this.deleteSelectedTrackablesToolStripMenuItem.Name = "deleteSelectedTrackablesToolStripMenuItem";
            this.deleteSelectedTrackablesToolStripMenuItem.Size = new System.Drawing.Size(245, 22);
            this.deleteSelectedTrackablesToolStripMenuItem.Text = "Delete selected trackables";
            this.deleteSelectedTrackablesToolStripMenuItem.Click += new System.EventHandler(this.deleteSelectedTrackablesToolStripMenuItem_Click);
            // 
            // TrackableGroupsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(736, 611);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.menuStrip1);
            this.Name = "TrackableGroupsForm";
            this.ShowIcon = false;
            this.Text = "TrackableGroupsForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TrackableGroupsForm_FormClosing);
            this.Shown += new System.EventHandler(this.TrackableGroupsForm_Shown);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.contextMenuStrip1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button buttonAddYouOwn;
        private System.Windows.Forms.Button buttonAddTrackables;
        private System.Windows.Forms.TextBox textBoxTBCodes;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBoxGroup;
        private System.Windows.Forms.TextBox textBoxGroupName;
        private System.Windows.Forms.Button buttonGroupCreate;
        private System.Windows.Forms.Button buttonGroupRename;
        private System.Windows.Forms.Button buttonGroupDelete;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem trackablesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showAllOnMapToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showSelectedOnMapToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem deleteSelectedTrackablesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem updateSelectedTrackablesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem updateAllTrackablesInGroupToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.ColumnHeader columnHeader9;
        private System.Windows.Forms.ToolStripMenuItem showRouteOfSelectedTrackableToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem goToGeocacheToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showRouteOfSelectedTrackableToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem goToTrackablePageToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
    }
}