namespace GlobalcachingApplication.Plugins.Gallery
{
    partial class ImageInfoItem
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.linkLabelGC = new System.Windows.Forms.LinkLabel();
            this.linkLabelLG = new System.Windows.Forms.LinkLabel();
            this.labelDate = new System.Windows.Forms.Label();
            this.labelName = new System.Windows.Forms.Label();
            this.labelDescription = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(34, 3);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(100, 75);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // linkLabelGC
            // 
            this.linkLabelGC.Location = new System.Drawing.Point(3, 123);
            this.linkLabelGC.Name = "linkLabelGC";
            this.linkLabelGC.Size = new System.Drawing.Size(172, 17);
            this.linkLabelGC.TabIndex = 1;
            this.linkLabelGC.TabStop = true;
            this.linkLabelGC.Text = "linkLabel1";
            this.linkLabelGC.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.linkLabelGC.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelGC_LinkClicked);
            // 
            // linkLabelLG
            // 
            this.linkLabelLG.Location = new System.Drawing.Point(3, 140);
            this.linkLabelLG.Name = "linkLabelLG";
            this.linkLabelLG.Size = new System.Drawing.Size(172, 17);
            this.linkLabelLG.TabIndex = 2;
            this.linkLabelLG.TabStop = true;
            this.linkLabelLG.Text = "linkLabel2";
            this.linkLabelLG.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.linkLabelLG.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelLG_LinkClicked);
            // 
            // labelDate
            // 
            this.labelDate.Location = new System.Drawing.Point(3, 102);
            this.labelDate.Name = "labelDate";
            this.labelDate.Size = new System.Drawing.Size(167, 21);
            this.labelDate.TabIndex = 3;
            this.labelDate.Text = "label1";
            this.labelDate.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelName
            // 
            this.labelName.Location = new System.Drawing.Point(3, 81);
            this.labelName.Name = "labelName";
            this.labelName.Size = new System.Drawing.Size(167, 21);
            this.labelName.TabIndex = 4;
            this.labelName.Text = "label1";
            this.labelName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelDescription
            // 
            this.labelDescription.Location = new System.Drawing.Point(3, 170);
            this.labelDescription.Name = "labelDescription";
            this.labelDescription.Size = new System.Drawing.Size(167, 54);
            this.labelDescription.TabIndex = 5;
            this.labelDescription.Text = "label1";
            this.labelDescription.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // ImageInfoItem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.Controls.Add(this.labelDescription);
            this.Controls.Add(this.labelName);
            this.Controls.Add(this.labelDate);
            this.Controls.Add(this.linkLabelLG);
            this.Controls.Add(this.linkLabelGC);
            this.Controls.Add(this.pictureBox1);
            this.Name = "ImageInfoItem";
            this.Size = new System.Drawing.Size(173, 160);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.LinkLabel linkLabelGC;
        private System.Windows.Forms.LinkLabel linkLabelLG;
        private System.Windows.Forms.Label labelDate;
        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.Label labelDescription;
    }
}
