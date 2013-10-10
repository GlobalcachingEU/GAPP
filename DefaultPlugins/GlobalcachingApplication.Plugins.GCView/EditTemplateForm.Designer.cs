namespace GlobalcachingApplication.Plugins.GCView
{
    partial class EditTemplateForm
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.buttonOK = new System.Windows.Forms.Button();
            this.textBoxTemplate = new System.Windows.Forms.TextBox();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.buttonOK);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 312);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(431, 28);
            this.panel1.TabIndex = 0;
            // 
            // buttonOK
            // 
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Dock = System.Windows.Forms.DockStyle.Right;
            this.buttonOK.Location = new System.Drawing.Point(356, 0);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 28);
            this.buttonOK.TabIndex = 0;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            // 
            // textBoxTemplate
            // 
            this.textBoxTemplate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxTemplate.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxTemplate.Location = new System.Drawing.Point(0, 0);
            this.textBoxTemplate.Multiline = true;
            this.textBoxTemplate.Name = "textBoxTemplate";
            this.textBoxTemplate.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxTemplate.Size = new System.Drawing.Size(431, 312);
            this.textBoxTemplate.TabIndex = 1;
            // 
            // EditTemplateForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(431, 340);
            this.Controls.Add(this.textBoxTemplate);
            this.Controls.Add(this.panel1);
            this.MinimizeBox = false;
            this.Name = "EditTemplateForm";
            this.ShowIcon = false;
            this.Text = "Edit Template";
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button buttonOK;
        internal System.Windows.Forms.TextBox textBoxTemplate;
    }
}