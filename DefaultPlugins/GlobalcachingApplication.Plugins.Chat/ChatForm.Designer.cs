namespace GlobalcachingApplication.Plugins.Chat
{
    partial class ChatForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChatForm));
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabelConnectionStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelGettingGeocache = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelRequestSelection = new System.Windows.Forms.ToolStripStatusLabel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.checkBoxDecoupleWindow = new System.Windows.Forms.CheckBox();
            this.checkBoxCanFollow = new System.Windows.Forms.CheckBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.buttonClear = new System.Windows.Forms.Button();
            this.checkBoxPlaySound = new System.Windows.Forms.CheckBox();
            this.buttonTxtColor = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.buttonCopySelection = new System.Windows.Forms.Button();
            this.linkLabelUnavailableCache = new System.Windows.Forms.LinkLabel();
            this.buttonFollow = new System.Windows.Forms.Button();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.listBox2 = new System.Windows.Forms.ListBox();
            this.panel4 = new System.Windows.Forms.Panel();
            this.textBoxRoomName = new System.Windows.Forms.TextBox();
            this.labelActiveRoom = new System.Windows.Forms.Label();
            this.buttonCreateJoinRoom = new System.Windows.Forms.Button();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.timerCopySelection = new System.Windows.Forms.Timer(this.components);
            this.timerConnectionRetry = new System.Windows.Forms.Timer(this.components);
            this.statusStrip1.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.panel2.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.panel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabelConnectionStatus,
            this.toolStripStatusLabelGettingGeocache,
            this.toolStripStatusLabelRequestSelection});
            this.statusStrip1.Location = new System.Drawing.Point(0, 597);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 19, 0);
            this.statusStrip1.Size = new System.Drawing.Size(957, 29);
            this.statusStrip1.TabIndex = 0;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabelConnectionStatus
            // 
            this.toolStripStatusLabelConnectionStatus.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.toolStripStatusLabelConnectionStatus.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenInner;
            this.toolStripStatusLabelConnectionStatus.Name = "toolStripStatusLabelConnectionStatus";
            this.toolStripStatusLabelConnectionStatus.Size = new System.Drawing.Size(155, 24);
            this.toolStripStatusLabelConnectionStatus.Text = "toolStripStatusLabel1";
            // 
            // toolStripStatusLabelGettingGeocache
            // 
            this.toolStripStatusLabelGettingGeocache.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.toolStripStatusLabelGettingGeocache.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenInner;
            this.toolStripStatusLabelGettingGeocache.Name = "toolStripStatusLabelGettingGeocache";
            this.toolStripStatusLabelGettingGeocache.Size = new System.Drawing.Size(19, 24);
            this.toolStripStatusLabelGettingGeocache.Text = "-";
            // 
            // toolStripStatusLabelRequestSelection
            // 
            this.toolStripStatusLabelRequestSelection.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.toolStripStatusLabelRequestSelection.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenInner;
            this.toolStripStatusLabelRequestSelection.Name = "toolStripStatusLabelRequestSelection";
            this.toolStripStatusLabelRequestSelection.Size = new System.Drawing.Size(19, 24);
            this.toolStripStatusLabelRequestSelection.Text = "-";
            this.toolStripStatusLabelRequestSelection.Visible = false;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.checkBoxDecoupleWindow);
            this.panel1.Controls.Add(this.checkBoxCanFollow);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(957, 31);
            this.panel1.TabIndex = 1;
            // 
            // checkBoxDecoupleWindow
            // 
            this.checkBoxDecoupleWindow.AutoSize = true;
            this.checkBoxDecoupleWindow.Location = new System.Drawing.Point(241, 4);
            this.checkBoxDecoupleWindow.Margin = new System.Windows.Forms.Padding(4);
            this.checkBoxDecoupleWindow.Name = "checkBoxDecoupleWindow";
            this.checkBoxDecoupleWindow.Size = new System.Drawing.Size(139, 21);
            this.checkBoxDecoupleWindow.TabIndex = 1;
            this.checkBoxDecoupleWindow.Text = "Decouple window";
            this.checkBoxDecoupleWindow.UseVisualStyleBackColor = true;
            this.checkBoxDecoupleWindow.Visible = false;
            this.checkBoxDecoupleWindow.CheckedChanged += new System.EventHandler(this.checkBoxDecoupleWindow_CheckedChanged);
            // 
            // checkBoxCanFollow
            // 
            this.checkBoxCanFollow.AutoSize = true;
            this.checkBoxCanFollow.Location = new System.Drawing.Point(4, 4);
            this.checkBoxCanFollow.Margin = new System.Windows.Forms.Padding(4);
            this.checkBoxCanFollow.Name = "checkBoxCanFollow";
            this.checkBoxCanFollow.Size = new System.Drawing.Size(135, 21);
            this.checkBoxCanFollow.TabIndex = 0;
            this.checkBoxCanFollow.Text = "I can be followed";
            this.checkBoxCanFollow.UseVisualStyleBackColor = true;
            this.checkBoxCanFollow.CheckedChanged += new System.EventHandler(this.checkBoxCanFollow_CheckedChanged);
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 31);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tabControl1);
            this.splitContainer1.Size = new System.Drawing.Size(957, 566);
            this.splitContainer1.SplitterDistance = 711;
            this.splitContainer1.SplitterWidth = 5;
            this.splitContainer1.TabIndex = 2;
            this.splitContainer1.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitContainer1_SplitterMoved);
            // 
            // splitContainer2
            // 
            this.splitContainer2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Margin = new System.Windows.Forms.Padding(4);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.richTextBox1);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.textBox1);
            this.splitContainer2.Panel2.Controls.Add(this.panel2);
            this.splitContainer2.Size = new System.Drawing.Size(711, 566);
            this.splitContainer2.SplitterDistance = 457;
            this.splitContainer2.SplitterWidth = 5;
            this.splitContainer2.TabIndex = 0;
            this.splitContainer2.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitContainer2_SplitterMoved);
            // 
            // richTextBox1
            // 
            this.richTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBox1.Location = new System.Drawing.Point(0, 0);
            this.richTextBox1.Margin = new System.Windows.Forms.Padding(4);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(707, 453);
            this.richTextBox1.TabIndex = 0;
            this.richTextBox1.Text = "";
            // 
            // textBox1
            // 
            this.textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox1.Location = new System.Drawing.Point(0, 38);
            this.textBox1.Margin = new System.Windows.Forms.Padding(4);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(707, 62);
            this.textBox1.TabIndex = 1;
            this.textBox1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox1_KeyPress);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.buttonClear);
            this.panel2.Controls.Add(this.checkBoxPlaySound);
            this.panel2.Controls.Add(this.buttonTxtColor);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Margin = new System.Windows.Forms.Padding(4);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(707, 38);
            this.panel2.TabIndex = 0;
            // 
            // buttonClear
            // 
            this.buttonClear.Location = new System.Drawing.Point(333, 2);
            this.buttonClear.Margin = new System.Windows.Forms.Padding(4);
            this.buttonClear.Name = "buttonClear";
            this.buttonClear.Size = new System.Drawing.Size(143, 28);
            this.buttonClear.TabIndex = 2;
            this.buttonClear.Text = "Clear";
            this.buttonClear.UseVisualStyleBackColor = true;
            this.buttonClear.Click += new System.EventHandler(this.buttonClear_Click);
            // 
            // checkBoxPlaySound
            // 
            this.checkBoxPlaySound.AutoSize = true;
            this.checkBoxPlaySound.Location = new System.Drawing.Point(175, 10);
            this.checkBoxPlaySound.Margin = new System.Windows.Forms.Padding(4);
            this.checkBoxPlaySound.Name = "checkBoxPlaySound";
            this.checkBoxPlaySound.Size = new System.Drawing.Size(107, 21);
            this.checkBoxPlaySound.TabIndex = 1;
            this.checkBoxPlaySound.Text = "Play sounds";
            this.checkBoxPlaySound.UseVisualStyleBackColor = true;
            // 
            // buttonTxtColor
            // 
            this.buttonTxtColor.Location = new System.Drawing.Point(4, 4);
            this.buttonTxtColor.Margin = new System.Windows.Forms.Padding(4);
            this.buttonTxtColor.Name = "buttonTxtColor";
            this.buttonTxtColor.Size = new System.Drawing.Size(140, 28);
            this.buttonTxtColor.TabIndex = 0;
            this.buttonTxtColor.Text = "Text Color";
            this.buttonTxtColor.UseVisualStyleBackColor = true;
            this.buttonTxtColor.Click += new System.EventHandler(this.buttonTxtColor_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(4);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(237, 562);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.listBox1);
            this.tabPage1.Controls.Add(this.panel3);
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage1.Size = new System.Drawing.Size(229, 533);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Users";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // listBox1
            // 
            this.listBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBox1.FormattingEnabled = true;
            this.listBox1.ItemHeight = 16;
            this.listBox1.Location = new System.Drawing.Point(4, 99);
            this.listBox1.Margin = new System.Windows.Forms.Padding(4);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(221, 430);
            this.listBox1.TabIndex = 1;
            this.listBox1.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.buttonCopySelection);
            this.panel3.Controls.Add(this.linkLabelUnavailableCache);
            this.panel3.Controls.Add(this.buttonFollow);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel3.Location = new System.Drawing.Point(4, 4);
            this.panel3.Margin = new System.Windows.Forms.Padding(4);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(221, 95);
            this.panel3.TabIndex = 0;
            // 
            // buttonCopySelection
            // 
            this.buttonCopySelection.Enabled = false;
            this.buttonCopySelection.Location = new System.Drawing.Point(4, 39);
            this.buttonCopySelection.Name = "buttonCopySelection";
            this.buttonCopySelection.Size = new System.Drawing.Size(208, 28);
            this.buttonCopySelection.TabIndex = 2;
            this.buttonCopySelection.Text = "Copy selection";
            this.buttonCopySelection.UseVisualStyleBackColor = true;
            this.buttonCopySelection.Click += new System.EventHandler(this.buttonCopySelection_Click);
            // 
            // linkLabelUnavailableCache
            // 
            this.linkLabelUnavailableCache.AutoSize = true;
            this.linkLabelUnavailableCache.Location = new System.Drawing.Point(4, 70);
            this.linkLabelUnavailableCache.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.linkLabelUnavailableCache.Name = "linkLabelUnavailableCache";
            this.linkLabelUnavailableCache.Size = new System.Drawing.Size(72, 17);
            this.linkLabelUnavailableCache.TabIndex = 1;
            this.linkLabelUnavailableCache.TabStop = true;
            this.linkLabelUnavailableCache.Text = "linkLabel1";
            this.linkLabelUnavailableCache.Visible = false;
            this.linkLabelUnavailableCache.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelUnavailableCache_LinkClicked);
            // 
            // buttonFollow
            // 
            this.buttonFollow.Enabled = false;
            this.buttonFollow.Location = new System.Drawing.Point(4, 4);
            this.buttonFollow.Margin = new System.Windows.Forms.Padding(4);
            this.buttonFollow.Name = "buttonFollow";
            this.buttonFollow.Size = new System.Drawing.Size(208, 28);
            this.buttonFollow.TabIndex = 0;
            this.buttonFollow.Text = "Follow";
            this.buttonFollow.UseVisualStyleBackColor = true;
            this.buttonFollow.Click += new System.EventHandler(this.buttonFollow_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.listBox2);
            this.tabPage2.Controls.Add(this.panel4);
            this.tabPage2.Location = new System.Drawing.Point(4, 25);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage2.Size = new System.Drawing.Size(229, 533);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Rooms";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // listBox2
            // 
            this.listBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBox2.FormattingEnabled = true;
            this.listBox2.ItemHeight = 16;
            this.listBox2.Location = new System.Drawing.Point(4, 106);
            this.listBox2.Margin = new System.Windows.Forms.Padding(4);
            this.listBox2.Name = "listBox2";
            this.listBox2.Size = new System.Drawing.Size(221, 423);
            this.listBox2.TabIndex = 1;
            this.listBox2.SelectedIndexChanged += new System.EventHandler(this.listBox2_SelectedIndexChanged);
            this.listBox2.DoubleClick += new System.EventHandler(this.listBox2_DoubleClick);
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.textBoxRoomName);
            this.panel4.Controls.Add(this.labelActiveRoom);
            this.panel4.Controls.Add(this.buttonCreateJoinRoom);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel4.Location = new System.Drawing.Point(4, 4);
            this.panel4.Margin = new System.Windows.Forms.Padding(4);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(221, 102);
            this.panel4.TabIndex = 0;
            // 
            // textBoxRoomName
            // 
            this.textBoxRoomName.Location = new System.Drawing.Point(8, 70);
            this.textBoxRoomName.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxRoomName.Name = "textBoxRoomName";
            this.textBoxRoomName.Size = new System.Drawing.Size(203, 22);
            this.textBoxRoomName.TabIndex = 2;
            this.textBoxRoomName.TextChanged += new System.EventHandler(this.textBoxRoomName_TextChanged);
            // 
            // labelActiveRoom
            // 
            this.labelActiveRoom.Location = new System.Drawing.Point(4, 0);
            this.labelActiveRoom.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelActiveRoom.Name = "labelActiveRoom";
            this.labelActiveRoom.Size = new System.Drawing.Size(208, 22);
            this.labelActiveRoom.TabIndex = 1;
            this.labelActiveRoom.Text = "-";
            this.labelActiveRoom.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // buttonCreateJoinRoom
            // 
            this.buttonCreateJoinRoom.Location = new System.Drawing.Point(4, 38);
            this.buttonCreateJoinRoom.Margin = new System.Windows.Forms.Padding(4);
            this.buttonCreateJoinRoom.Name = "buttonCreateJoinRoom";
            this.buttonCreateJoinRoom.Size = new System.Drawing.Size(208, 28);
            this.buttonCreateJoinRoom.TabIndex = 0;
            this.buttonCreateJoinRoom.Text = "Create/Join";
            this.buttonCreateJoinRoom.UseVisualStyleBackColor = true;
            this.buttonCreateJoinRoom.Click += new System.EventHandler(this.buttonCreateJoinRoom_Click);
            // 
            // timerCopySelection
            // 
            this.timerCopySelection.Interval = 1000;
            this.timerCopySelection.Tick += new System.EventHandler(this.timerCopySelection_Tick);
            // 
            // timerConnectionRetry
            // 
            this.timerConnectionRetry.Interval = 2000;
            this.timerConnectionRetry.Tick += new System.EventHandler(this.timerConnectionRetry_Tick);
            // 
            // ChatForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(957, 626);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.statusStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(5);
            this.Name = "ChatForm";
            this.ShowIcon = false;
            this.Text = "ChatForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ChatForm_FormClosing);
            this.Shown += new System.EventHandler(this.ChatForm_Shown);
            this.LocationChanged += new System.EventHandler(this.ChatForm_LocationChanged);
            this.SizeChanged += new System.EventHandler(this.ChatForm_SizeChanged);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.ListBox listBox2;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelConnectionStatus;
        private System.Windows.Forms.CheckBox checkBoxCanFollow;
        private System.Windows.Forms.Button buttonFollow;
        private System.Windows.Forms.TextBox textBoxRoomName;
        private System.Windows.Forms.Label labelActiveRoom;
        private System.Windows.Forms.Button buttonCreateJoinRoom;
        private System.Windows.Forms.Button buttonTxtColor;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.CheckBox checkBoxPlaySound;
        private System.Windows.Forms.CheckBox checkBoxDecoupleWindow;
        private System.Windows.Forms.Button buttonClear;
        private System.Windows.Forms.LinkLabel linkLabelUnavailableCache;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelGettingGeocache;
        private System.Windows.Forms.Button buttonCopySelection;
        private System.Windows.Forms.Timer timerCopySelection;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelRequestSelection;
        private System.Windows.Forms.Timer timerConnectionRetry;
    }
}