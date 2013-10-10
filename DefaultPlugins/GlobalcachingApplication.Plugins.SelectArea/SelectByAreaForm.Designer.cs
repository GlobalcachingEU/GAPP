namespace GlobalcachingApplication.Plugins.SelectArea
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
            this.radioButtonAddToCurrent = new System.Windows.Forms.RadioButton();
            this.radioButtonWithinSelection = new System.Windows.Forms.RadioButton();
            this.radioButtonNewSearch = new System.Windows.Forms.RadioButton();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.buttonWholeArea);
            this.panel1.Controls.Add(this.buttonWithinRadius);
            this.panel1.Controls.Add(this.radioButtonAddToCurrent);
            this.panel1.Controls.Add(this.radioButtonWithinSelection);
            this.panel1.Controls.Add(this.radioButtonNewSearch);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1053, 92);
            this.panel1.TabIndex = 0;
            // 
            // buttonWholeArea
            // 
            this.buttonWholeArea.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonWholeArea.Location = new System.Drawing.Point(347, 44);
            this.buttonWholeArea.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.buttonWholeArea.Name = "buttonWholeArea";
            this.buttonWholeArea.Size = new System.Drawing.Size(224, 28);
            this.buttonWholeArea.TabIndex = 8;
            this.buttonWholeArea.Text = "Select whole area";
            this.buttonWholeArea.UseVisualStyleBackColor = true;
            this.buttonWholeArea.Click += new System.EventHandler(this.buttonWholeArea_Click);
            // 
            // buttonWithinRadius
            // 
            this.buttonWithinRadius.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonWithinRadius.Location = new System.Drawing.Point(20, 44);
            this.buttonWithinRadius.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.buttonWithinRadius.Name = "buttonWithinRadius";
            this.buttonWithinRadius.Size = new System.Drawing.Size(217, 28);
            this.buttonWithinRadius.TabIndex = 7;
            this.buttonWithinRadius.Text = "Select within radius";
            this.buttonWithinRadius.UseVisualStyleBackColor = true;
            this.buttonWithinRadius.Click += new System.EventHandler(this.buttonWithinRadius_Click);
            // 
            // radioButtonAddToCurrent
            // 
            this.radioButtonAddToCurrent.AutoSize = true;
            this.radioButtonAddToCurrent.Location = new System.Drawing.Point(388, 15);
            this.radioButtonAddToCurrent.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.radioButtonAddToCurrent.Name = "radioButtonAddToCurrent";
            this.radioButtonAddToCurrent.Size = new System.Drawing.Size(179, 21);
            this.radioButtonAddToCurrent.TabIndex = 6;
            this.radioButtonAddToCurrent.TabStop = true;
            this.radioButtonAddToCurrent.Text = "Add to current selection";
            this.radioButtonAddToCurrent.UseVisualStyleBackColor = true;
            // 
            // radioButtonWithinSelection
            // 
            this.radioButtonWithinSelection.AutoSize = true;
            this.radioButtonWithinSelection.Location = new System.Drawing.Point(177, 15);
            this.radioButtonWithinSelection.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.radioButtonWithinSelection.Name = "radioButtonWithinSelection";
            this.radioButtonWithinSelection.Size = new System.Drawing.Size(173, 21);
            this.radioButtonWithinSelection.TabIndex = 5;
            this.radioButtonWithinSelection.Text = "Search within selection";
            this.radioButtonWithinSelection.UseVisualStyleBackColor = true;
            // 
            // radioButtonNewSearch
            // 
            this.radioButtonNewSearch.AutoSize = true;
            this.radioButtonNewSearch.Checked = true;
            this.radioButtonNewSearch.Location = new System.Drawing.Point(20, 15);
            this.radioButtonNewSearch.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.radioButtonNewSearch.Name = "radioButtonNewSearch";
            this.radioButtonNewSearch.Size = new System.Drawing.Size(103, 21);
            this.radioButtonNewSearch.TabIndex = 4;
            this.radioButtonNewSearch.TabStop = true;
            this.radioButtonNewSearch.Text = "New search";
            this.radioButtonNewSearch.UseVisualStyleBackColor = true;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Location = new System.Drawing.Point(0, 747);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 19, 0);
            this.statusStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.statusStrip1.Size = new System.Drawing.Size(1053, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // webBrowser1
            // 
            this.webBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser1.Location = new System.Drawing.Point(0, 92);
            this.webBrowser1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(27, 25);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(1053, 655);
            this.webBrowser1.TabIndex = 2;
            // 
            // SelectByAreaForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1053, 769);
            this.Controls.Add(this.webBrowser1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.panel1);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "SelectByAreaForm";
            this.Text = "SelectByAreaForm";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.WebBrowser webBrowser1;
        private System.Windows.Forms.Button buttonWholeArea;
        private System.Windows.Forms.Button buttonWithinRadius;
        private System.Windows.Forms.RadioButton radioButtonAddToCurrent;
        private System.Windows.Forms.RadioButton radioButtonWithinSelection;
        private System.Windows.Forms.RadioButton radioButtonNewSearch;
    }
}