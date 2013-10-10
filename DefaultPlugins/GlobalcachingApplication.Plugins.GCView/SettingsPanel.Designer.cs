namespace GlobalcachingApplication.Plugins.GCView
{
    partial class SettingsPanel
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
            this.comboBoxTemplates = new System.Windows.Forms.ComboBox();
            this.buttonEditTemplate = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // comboBoxTemplates
            // 
            this.comboBoxTemplates.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxTemplates.FormattingEnabled = true;
            this.comboBoxTemplates.Location = new System.Drawing.Point(15, 27);
            this.comboBoxTemplates.Name = "comboBoxTemplates";
            this.comboBoxTemplates.Size = new System.Drawing.Size(214, 21);
            this.comboBoxTemplates.TabIndex = 3;
            // 
            // buttonEditTemplate
            // 
            this.buttonEditTemplate.Location = new System.Drawing.Point(235, 26);
            this.buttonEditTemplate.Name = "buttonEditTemplate";
            this.buttonEditTemplate.Size = new System.Drawing.Size(99, 23);
            this.buttonEditTemplate.TabIndex = 2;
            this.buttonEditTemplate.Text = "Edit template";
            this.buttonEditTemplate.UseVisualStyleBackColor = true;
            this.buttonEditTemplate.Click += new System.EventHandler(this.buttonEditTemplate_Click);
            // 
            // SettingsPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.comboBoxTemplates);
            this.Controls.Add(this.buttonEditTemplate);
            this.Name = "SettingsPanel";
            this.Size = new System.Drawing.Size(385, 150);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBoxTemplates;
        private System.Windows.Forms.Button buttonEditTemplate;
    }
}
