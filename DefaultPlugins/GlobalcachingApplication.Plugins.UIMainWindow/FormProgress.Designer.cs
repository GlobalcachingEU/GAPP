namespace GlobalcachingApplication.Plugins.UIMainWindow
{
    partial class FormProgress
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
            this.labelProgressTitle = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.labelMin = new System.Windows.Forms.Label();
            this.labelMax = new System.Windows.Forms.Label();
            this.labelPos = new System.Windows.Forms.Label();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // labelProgressTitle
            // 
            this.labelProgressTitle.Location = new System.Drawing.Point(12, 9);
            this.labelProgressTitle.Name = "labelProgressTitle";
            this.labelProgressTitle.Size = new System.Drawing.Size(386, 24);
            this.labelProgressTitle.TabIndex = 0;
            this.labelProgressTitle.Text = "label1";
            this.labelProgressTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(15, 36);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(383, 23);
            this.progressBar1.TabIndex = 1;
            // 
            // labelMin
            // 
            this.labelMin.Location = new System.Drawing.Point(14, 64);
            this.labelMin.Name = "labelMin";
            this.labelMin.Size = new System.Drawing.Size(25, 20);
            this.labelMin.TabIndex = 2;
            this.labelMin.Text = "-";
            // 
            // labelMax
            // 
            this.labelMax.Location = new System.Drawing.Point(296, 64);
            this.labelMax.Name = "labelMax";
            this.labelMax.Size = new System.Drawing.Size(104, 20);
            this.labelMax.TabIndex = 3;
            this.labelMax.Text = "-";
            this.labelMax.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // labelPos
            // 
            this.labelPos.Location = new System.Drawing.Point(151, 64);
            this.labelPos.Name = "labelPos";
            this.labelPos.Size = new System.Drawing.Size(100, 23);
            this.labelPos.TabIndex = 4;
            this.labelPos.Text = "-";
            this.labelPos.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(144, 90);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(118, 23);
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // FormProgress
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(410, 125);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.labelPos);
            this.Controls.Add(this.labelMax);
            this.Controls.Add(this.labelMin);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.labelProgressTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormProgress";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Progress";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormProgress_FormClosing);
            this.Shown += new System.EventHandler(this.FormProgress_Shown);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labelProgressTitle;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label labelMin;
        private System.Windows.Forms.Label labelMax;
        private System.Windows.Forms.Label labelPos;
        private System.Windows.Forms.Button buttonCancel;
    }
}