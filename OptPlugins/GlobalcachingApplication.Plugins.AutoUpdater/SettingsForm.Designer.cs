namespace GlobalcachingApplication.Plugins.AutoUpdater
{
    partial class SettingsForm
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
            this.settingsPanel1 = new GlobalcachingApplication.Plugins.AutoUpdater.SettingsPanel();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // settingsPanel1
            // 
            this.settingsPanel1.Location = new System.Drawing.Point(3, 1);
            this.settingsPanel1.Name = "settingsPanel1";
            this.settingsPanel1.Size = new System.Drawing.Size(443, 225);
            this.settingsPanel1.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(157, 233);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(123, 32);
            this.button1.TabIndex = 1;
            this.button1.Text = "OK";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(447, 286);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.settingsPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.Text = "SettingsForm";
            this.ResumeLayout(false);

        }

        #endregion

        private SettingsPanel settingsPanel1;
        private System.Windows.Forms.Button button1;
    }
}