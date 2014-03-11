namespace GlobalcachingApplication.Plugins.APIGetC
{
    partial class SelectByAreaForm
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
            this.buttonWholeArea = new System.Windows.Forms.Button();
            this.buttonWithinRadius = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.buttonWholeArea);
            this.panel1.Controls.Add(this.buttonWithinRadius);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(790, 50);
            this.panel1.TabIndex = 0;
            // 
            // buttonWholeArea
            // 
            this.buttonWholeArea.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonWholeArea.Location = new System.Drawing.Point(253, 12);
            this.buttonWholeArea.Name = "buttonWholeArea";
            this.buttonWholeArea.Size = new System.Drawing.Size(168, 23);
            this.buttonWholeArea.TabIndex = 8;
            this.buttonWholeArea.Text = "Select whole area";
            this.buttonWholeArea.UseVisualStyleBackColor = true;
            this.buttonWholeArea.Visible = false;
            this.buttonWholeArea.Click += new System.EventHandler(this.buttonWholeArea_Click);
            // 
            // buttonWithinRadius
            // 
            this.buttonWithinRadius.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonWithinRadius.Location = new System.Drawing.Point(12, 12);
            this.buttonWithinRadius.Name = "buttonWithinRadius";
            this.buttonWithinRadius.Size = new System.Drawing.Size(163, 23);
            this.buttonWithinRadius.TabIndex = 7;
            this.buttonWithinRadius.Text = "Select within radius";
            this.buttonWithinRadius.UseVisualStyleBackColor = true;
            this.buttonWithinRadius.Click += new System.EventHandler(this.buttonWithinRadius_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Location = new System.Drawing.Point(0, 603);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.statusStrip1.Size = new System.Drawing.Size(790, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // webBrowser1
            // 
            this.webBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser1.Location = new System.Drawing.Point(0, 50);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.ScriptErrorsSuppressed = true;
            this.webBrowser1.Size = new System.Drawing.Size(790, 553);
            this.webBrowser1.TabIndex = 2;
            // 
            // SelectByAreaForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(790, 625);
            this.Controls.Add(this.webBrowser1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.panel1);
            this.MinimizeBox = false;
            this.Name = "SelectByAreaForm";
            this.ShowIcon = false;
            this.Text = "SelectByAreaForm";
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.WebBrowser webBrowser1;
        private System.Windows.Forms.Button buttonWholeArea;
        private System.Windows.Forms.Button buttonWithinRadius;
    }
}