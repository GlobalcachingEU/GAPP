namespace GlobalcachingApplication.Plugins.Gallery
{
    partial class ImageGalleryForm
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.radioButtonAscending = new System.Windows.Forms.RadioButton();
            this.radioButtonDescending = new System.Windows.Forms.RadioButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.radioButtonName = new System.Windows.Forms.RadioButton();
            this.radioButtonDate = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.textBoxFilter = new System.Windows.Forms.TextBox();
            this.radioButtonText = new System.Windows.Forms.RadioButton();
            this.radioButtonActive = new System.Windows.Forms.RadioButton();
            this.radioButtonAll = new System.Windows.Forms.RadioButton();
            this.button1 = new System.Windows.Forms.Button();
            this.buttonUpdate = new System.Windows.Forms.Button();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.panel1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.groupBox3);
            this.panel1.Controls.Add(this.groupBox2);
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Controls.Add(this.button1);
            this.panel1.Controls.Add(this.buttonUpdate);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(796, 112);
            this.panel1.TabIndex = 0;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.radioButtonAscending);
            this.groupBox3.Controls.Add(this.radioButtonDescending);
            this.groupBox3.Location = new System.Drawing.Point(388, 33);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(136, 71);
            this.groupBox3.TabIndex = 4;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Sort order";
            // 
            // radioButtonAscending
            // 
            this.radioButtonAscending.AutoSize = true;
            this.radioButtonAscending.Location = new System.Drawing.Point(7, 44);
            this.radioButtonAscending.Name = "radioButtonAscending";
            this.radioButtonAscending.Size = new System.Drawing.Size(75, 17);
            this.radioButtonAscending.TabIndex = 1;
            this.radioButtonAscending.TabStop = true;
            this.radioButtonAscending.Text = "Ascending";
            this.radioButtonAscending.UseVisualStyleBackColor = true;
            this.radioButtonAscending.CheckedChanged += new System.EventHandler(this.radioButtonDescending_CheckedChanged);
            // 
            // radioButtonDescending
            // 
            this.radioButtonDescending.AutoSize = true;
            this.radioButtonDescending.Checked = true;
            this.radioButtonDescending.Location = new System.Drawing.Point(7, 20);
            this.radioButtonDescending.Name = "radioButtonDescending";
            this.radioButtonDescending.Size = new System.Drawing.Size(82, 17);
            this.radioButtonDescending.TabIndex = 0;
            this.radioButtonDescending.TabStop = true;
            this.radioButtonDescending.Text = "Descending";
            this.radioButtonDescending.UseVisualStyleBackColor = true;
            this.radioButtonDescending.CheckedChanged += new System.EventHandler(this.radioButtonDescending_CheckedChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.radioButtonName);
            this.groupBox2.Controls.Add(this.radioButtonDate);
            this.groupBox2.Location = new System.Drawing.Point(241, 33);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(141, 71);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Sort on";
            // 
            // radioButtonName
            // 
            this.radioButtonName.AutoSize = true;
            this.radioButtonName.Location = new System.Drawing.Point(7, 44);
            this.radioButtonName.Name = "radioButtonName";
            this.radioButtonName.Size = new System.Drawing.Size(53, 17);
            this.radioButtonName.TabIndex = 1;
            this.radioButtonName.TabStop = true;
            this.radioButtonName.Text = "Name";
            this.radioButtonName.UseVisualStyleBackColor = true;
            this.radioButtonName.CheckedChanged += new System.EventHandler(this.radioButtonDate_CheckedChanged);
            // 
            // radioButtonDate
            // 
            this.radioButtonDate.AutoSize = true;
            this.radioButtonDate.Checked = true;
            this.radioButtonDate.Location = new System.Drawing.Point(7, 20);
            this.radioButtonDate.Name = "radioButtonDate";
            this.radioButtonDate.Size = new System.Drawing.Size(48, 17);
            this.radioButtonDate.TabIndex = 0;
            this.radioButtonDate.TabStop = true;
            this.radioButtonDate.Text = "Date";
            this.radioButtonDate.UseVisualStyleBackColor = true;
            this.radioButtonDate.CheckedChanged += new System.EventHandler(this.radioButtonDate_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.textBoxFilter);
            this.groupBox1.Controls.Add(this.radioButtonText);
            this.groupBox1.Controls.Add(this.radioButtonActive);
            this.groupBox1.Controls.Add(this.radioButtonAll);
            this.groupBox1.Location = new System.Drawing.Point(3, 33);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(232, 71);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Show";
            // 
            // textBoxFilter
            // 
            this.textBoxFilter.Location = new System.Drawing.Point(84, 42);
            this.textBoxFilter.Name = "textBoxFilter";
            this.textBoxFilter.Size = new System.Drawing.Size(136, 20);
            this.textBoxFilter.TabIndex = 3;
            this.textBoxFilter.TextChanged += new System.EventHandler(this.textBoxFilter_TextChanged);
            // 
            // radioButtonText
            // 
            this.radioButtonText.AutoSize = true;
            this.radioButtonText.Location = new System.Drawing.Point(9, 42);
            this.radioButtonText.Name = "radioButtonText";
            this.radioButtonText.Size = new System.Drawing.Size(46, 17);
            this.radioButtonText.TabIndex = 2;
            this.radioButtonText.TabStop = true;
            this.radioButtonText.Text = "Text";
            this.radioButtonText.UseVisualStyleBackColor = true;
            this.radioButtonText.CheckedChanged += new System.EventHandler(this.radioButtonAll_CheckedChanged);
            // 
            // radioButtonActive
            // 
            this.radioButtonActive.AutoSize = true;
            this.radioButtonActive.Location = new System.Drawing.Point(114, 19);
            this.radioButtonActive.Name = "radioButtonActive";
            this.radioButtonActive.Size = new System.Drawing.Size(106, 17);
            this.radioButtonActive.TabIndex = 1;
            this.radioButtonActive.TabStop = true;
            this.radioButtonActive.Text = "Active geocache";
            this.radioButtonActive.UseVisualStyleBackColor = true;
            this.radioButtonActive.CheckedChanged += new System.EventHandler(this.radioButtonAll_CheckedChanged);
            // 
            // radioButtonAll
            // 
            this.radioButtonAll.AutoSize = true;
            this.radioButtonAll.Checked = true;
            this.radioButtonAll.Location = new System.Drawing.Point(9, 19);
            this.radioButtonAll.Name = "radioButtonAll";
            this.radioButtonAll.Size = new System.Drawing.Size(36, 17);
            this.radioButtonAll.TabIndex = 0;
            this.radioButtonAll.TabStop = true;
            this.radioButtonAll.Text = "All";
            this.radioButtonAll.UseVisualStyleBackColor = true;
            this.radioButtonAll.CheckedChanged += new System.EventHandler(this.radioButtonAll_CheckedChanged);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(3, 3);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(29, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "^";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // buttonUpdate
            // 
            this.buttonUpdate.Location = new System.Drawing.Point(51, 3);
            this.buttonUpdate.Name = "buttonUpdate";
            this.buttonUpdate.Size = new System.Drawing.Size(122, 23);
            this.buttonUpdate.TabIndex = 0;
            this.buttonUpdate.Text = "Update";
            this.buttonUpdate.UseVisualStyleBackColor = true;
            this.buttonUpdate.Click += new System.EventHandler(this.buttonUpdate_Click);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoScroll = true;
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 112);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(796, 444);
            this.flowLayoutPanel1.TabIndex = 1;
            this.flowLayoutPanel1.MouseEnter += new System.EventHandler(this.flowLayoutPanel1_MouseEnter);
            // 
            // timer1
            // 
            this.timer1.Interval = 500;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // ImageGalleryForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(796, 556);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.panel1);
            this.Name = "ImageGalleryForm";
            this.ShowIcon = false;
            this.Text = "ImageGalleryForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ImageGalleryForm_FormClosing);
            this.Shown += new System.EventHandler(this.ImageGalleryForm_Shown);
            this.panel1.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button buttonUpdate;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radioButtonActive;
        private System.Windows.Forms.RadioButton radioButtonAll;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.RadioButton radioButtonAscending;
        private System.Windows.Forms.RadioButton radioButtonDescending;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton radioButtonName;
        private System.Windows.Forms.RadioButton radioButtonDate;
        private System.Windows.Forms.TextBox textBoxFilter;
        private System.Windows.Forms.RadioButton radioButtonText;
        private System.Windows.Forms.Timer timer1;
    }
}