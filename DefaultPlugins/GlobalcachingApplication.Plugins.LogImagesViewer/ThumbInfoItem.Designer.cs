namespace GlobalcachingApplication.Plugins.LogImagesViewer
{
    partial class ThumbInfoItem
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
            this.labelName = new System.Windows.Forms.Label();
            this.labelDate = new System.Windows.Forms.Label();
            this.linkLabelLG = new System.Windows.Forms.LinkLabel();
            this.linkLabelGC = new System.Windows.Forms.LinkLabel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // labelName
            // 
            this.labelName.Location = new System.Drawing.Point(1, 100);
            this.labelName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelName.Name = "labelName";
            this.labelName.Size = new System.Drawing.Size(223, 26);
            this.labelName.TabIndex = 9;
            this.labelName.Text = "label1";
            this.labelName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelDate
            // 
            this.labelDate.Location = new System.Drawing.Point(1, 126);
            this.labelDate.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelDate.Name = "labelDate";
            this.labelDate.Size = new System.Drawing.Size(223, 26);
            this.labelDate.TabIndex = 8;
            this.labelDate.Text = "label1";
            this.labelDate.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // linkLabelLG
            // 
            this.linkLabelLG.Location = new System.Drawing.Point(1, 205);
            this.linkLabelLG.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.linkLabelLG.Name = "linkLabelLG";
            this.linkLabelLG.Size = new System.Drawing.Size(229, 21);
            this.linkLabelLG.TabIndex = 7;
            this.linkLabelLG.TabStop = true;
            this.linkLabelLG.Text = "linkLabel2";
            this.linkLabelLG.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.linkLabelLG.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelLG_LinkClicked);
            // 
            // linkLabelGC
            // 
            this.linkLabelGC.Location = new System.Drawing.Point(1, 178);
            this.linkLabelGC.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.linkLabelGC.Name = "linkLabelGC";
            this.linkLabelGC.Size = new System.Drawing.Size(229, 21);
            this.linkLabelGC.TabIndex = 6;
            this.linkLabelGC.TabStop = true;
            this.linkLabelGC.Text = "linkLabel1";
            this.linkLabelGC.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.linkLabelGC.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelGC_LinkClicked);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(42, 4);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(4);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(133, 92);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox1.TabIndex = 5;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(1, 152);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(223, 26);
            this.label1.TabIndex = 10;
            this.label1.Text = "label1";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ThumbInfoItem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.Controls.Add(this.label1);
            this.Controls.Add(this.labelName);
            this.Controls.Add(this.labelDate);
            this.Controls.Add(this.linkLabelLG);
            this.Controls.Add(this.linkLabelGC);
            this.Controls.Add(this.pictureBox1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "ThumbInfoItem";
            this.Size = new System.Drawing.Size(231, 232);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.Label labelDate;
        private System.Windows.Forms.LinkLabel linkLabelLG;
        private System.Windows.Forms.LinkLabel linkLabelGC;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label1;
    }
}
