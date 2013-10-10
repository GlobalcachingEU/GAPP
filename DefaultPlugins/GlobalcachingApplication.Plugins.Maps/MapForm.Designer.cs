namespace GlobalcachingApplication.Plugins.Maps
{
    partial class MapForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MapForm));
            this.panel1 = new System.Windows.Forms.Panel();
            this.radioButtonAll = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.radioButtonSelected = new System.Windows.Forms.RadioButton();
            this.radioButtonActive = new System.Windows.Forms.RadioButton();
            this.pictureBoxGC = new System.Windows.Forms.PictureBox();
            this.linkLabelGC = new System.Windows.Forms.LinkLabel();
            this.elementHost1 = new System.Windows.Forms.Integration.ElementHost();
            this.mapContainerControl1 = new GlobalcachingApplication.Plugins.Maps.MapContainerControl();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxGC)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.radioButtonAll);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.radioButtonSelected);
            this.panel1.Controls.Add(this.radioButtonActive);
            this.panel1.Controls.Add(this.pictureBoxGC);
            this.panel1.Controls.Add(this.linkLabelGC);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(769, 50);
            this.panel1.TabIndex = 0;
            // 
            // radioButtonAll
            // 
            this.radioButtonAll.AutoSize = true;
            this.radioButtonAll.Location = new System.Drawing.Point(592, 5);
            this.radioButtonAll.Margin = new System.Windows.Forms.Padding(4);
            this.radioButtonAll.Name = "radioButtonAll";
            this.radioButtonAll.Size = new System.Drawing.Size(44, 21);
            this.radioButtonAll.TabIndex = 2;
            this.radioButtonAll.TabStop = true;
            this.radioButtonAll.Text = "All";
            this.radioButtonAll.UseVisualStyleBackColor = true;
            this.radioButtonAll.CheckedChanged += new System.EventHandler(this.radioButtonSelected_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(133, 5);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(115, 17);
            this.label1.TabIndex = 12;
            this.label1.Text = "Show geocaches";
            // 
            // radioButtonSelected
            // 
            this.radioButtonSelected.AutoSize = true;
            this.radioButtonSelected.Location = new System.Drawing.Point(441, 5);
            this.radioButtonSelected.Margin = new System.Windows.Forms.Padding(4);
            this.radioButtonSelected.Name = "radioButtonSelected";
            this.radioButtonSelected.Size = new System.Drawing.Size(84, 21);
            this.radioButtonSelected.TabIndex = 1;
            this.radioButtonSelected.TabStop = true;
            this.radioButtonSelected.Text = "Selected";
            this.radioButtonSelected.UseVisualStyleBackColor = true;
            this.radioButtonSelected.CheckedChanged += new System.EventHandler(this.radioButtonSelected_CheckedChanged);
            // 
            // radioButtonActive
            // 
            this.radioButtonActive.AutoSize = true;
            this.radioButtonActive.Checked = true;
            this.radioButtonActive.Location = new System.Drawing.Point(313, 5);
            this.radioButtonActive.Margin = new System.Windows.Forms.Padding(4);
            this.radioButtonActive.Name = "radioButtonActive";
            this.radioButtonActive.Size = new System.Drawing.Size(67, 21);
            this.radioButtonActive.TabIndex = 0;
            this.radioButtonActive.TabStop = true;
            this.radioButtonActive.Text = "Active";
            this.radioButtonActive.UseVisualStyleBackColor = true;
            this.radioButtonActive.CheckedChanged += new System.EventHandler(this.radioButtonSelected_CheckedChanged);
            // 
            // pictureBoxGC
            // 
            this.pictureBoxGC.Location = new System.Drawing.Point(4, 5);
            this.pictureBoxGC.Margin = new System.Windows.Forms.Padding(4);
            this.pictureBoxGC.Name = "pictureBoxGC";
            this.pictureBoxGC.Size = new System.Drawing.Size(32, 32);
            this.pictureBoxGC.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBoxGC.TabIndex = 9;
            this.pictureBoxGC.TabStop = false;
            // 
            // linkLabelGC
            // 
            this.linkLabelGC.AutoSize = true;
            this.linkLabelGC.Location = new System.Drawing.Point(55, 28);
            this.linkLabelGC.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.linkLabelGC.Name = "linkLabelGC";
            this.linkLabelGC.Size = new System.Drawing.Size(13, 17);
            this.linkLabelGC.TabIndex = 10;
            this.linkLabelGC.TabStop = true;
            this.linkLabelGC.Text = "-";
            this.linkLabelGC.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelGC_LinkClicked);
            // 
            // elementHost1
            // 
            this.elementHost1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.elementHost1.Location = new System.Drawing.Point(0, 50);
            this.elementHost1.Margin = new System.Windows.Forms.Padding(4);
            this.elementHost1.Name = "elementHost1";
            this.elementHost1.Size = new System.Drawing.Size(769, 463);
            this.elementHost1.TabIndex = 1;
            this.elementHost1.Text = "elementHost1";
            this.elementHost1.Child = this.mapContainerControl1;
            // 
            // MapForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(769, 513);
            this.Controls.Add(this.elementHost1);
            this.Controls.Add(this.panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "MapForm";
            this.Text = "MapForm";
            this.Activated += new System.EventHandler(this.MapForm_Activated);
            this.Deactivate += new System.EventHandler(this.MapForm_Deactivate);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MapForm_FormClosing);
            this.LocationChanged += new System.EventHandler(this.MapForm_LocationChanged);
            this.SizeChanged += new System.EventHandler(this.MapForm_SizeChanged);
            this.VisibleChanged += new System.EventHandler(this.MapForm_VisibleChanged);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxGC)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Integration.ElementHost elementHost1;
        private MapContainerControl mapContainerControl1;
        private System.Windows.Forms.PictureBox pictureBoxGC;
        private System.Windows.Forms.LinkLabel linkLabelGC;
        private System.Windows.Forms.RadioButton radioButtonAll;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton radioButtonSelected;
        private System.Windows.Forms.RadioButton radioButtonActive;
    }
}