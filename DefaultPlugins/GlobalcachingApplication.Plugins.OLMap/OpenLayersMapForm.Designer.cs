namespace GlobalcachingApplication.Plugins.OLMap
{
    partial class OpenLayersMapForm
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
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.radioButtonAll = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.radioButtonSelected = new System.Windows.Forms.RadioButton();
            this.radioButtonActive = new System.Windows.Forms.RadioButton();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.radioButtonAll);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.radioButtonSelected);
            this.panel1.Controls.Add(this.radioButtonActive);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(684, 30);
            this.panel1.TabIndex = 0;
            // 
            // webBrowser1
            // 
            this.webBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser1.Location = new System.Drawing.Point(0, 30);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(684, 381);
            this.webBrowser1.TabIndex = 1;
            this.webBrowser1.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.webBrowser1_DocumentCompleted);
            // 
            // timer1
            // 
            this.timer1.Interval = 300;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // radioButtonAll
            // 
            this.radioButtonAll.AutoSize = true;
            this.radioButtonAll.Location = new System.Drawing.Point(467, 4);
            this.radioButtonAll.Margin = new System.Windows.Forms.Padding(4);
            this.radioButtonAll.Name = "radioButtonAll";
            this.radioButtonAll.Size = new System.Drawing.Size(44, 21);
            this.radioButtonAll.TabIndex = 15;
            this.radioButtonAll.TabStop = true;
            this.radioButtonAll.Text = "All";
            this.radioButtonAll.UseVisualStyleBackColor = true;
            this.radioButtonAll.CheckedChanged += new System.EventHandler(this.radioButtonActive_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 4);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(113, 17);
            this.label1.TabIndex = 16;
            this.label1.Text = "Load geocaches";
            // 
            // radioButtonSelected
            // 
            this.radioButtonSelected.AutoSize = true;
            this.radioButtonSelected.Location = new System.Drawing.Point(316, 4);
            this.radioButtonSelected.Margin = new System.Windows.Forms.Padding(4);
            this.radioButtonSelected.Name = "radioButtonSelected";
            this.radioButtonSelected.Size = new System.Drawing.Size(84, 21);
            this.radioButtonSelected.TabIndex = 14;
            this.radioButtonSelected.TabStop = true;
            this.radioButtonSelected.Text = "Selected";
            this.radioButtonSelected.UseVisualStyleBackColor = true;
            this.radioButtonSelected.CheckedChanged += new System.EventHandler(this.radioButtonActive_CheckedChanged);
            // 
            // radioButtonActive
            // 
            this.radioButtonActive.AutoSize = true;
            this.radioButtonActive.Checked = true;
            this.radioButtonActive.Location = new System.Drawing.Point(188, 4);
            this.radioButtonActive.Margin = new System.Windows.Forms.Padding(4);
            this.radioButtonActive.Name = "radioButtonActive";
            this.radioButtonActive.Size = new System.Drawing.Size(67, 21);
            this.radioButtonActive.TabIndex = 13;
            this.radioButtonActive.TabStop = true;
            this.radioButtonActive.Text = "Active";
            this.radioButtonActive.UseVisualStyleBackColor = true;
            this.radioButtonActive.CheckedChanged += new System.EventHandler(this.radioButtonActive_CheckedChanged);
            // 
            // OpenLayersMapForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(684, 411);
            this.Controls.Add(this.webBrowser1);
            this.Controls.Add(this.panel1);
            this.Name = "OpenLayersMapForm";
            this.ShowIcon = false;
            this.Text = "OpenLayersMapForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OpenLayersMapForm_FormClosing);
            this.LocationChanged += new System.EventHandler(this.OpenLayersMapForm_LocationChanged);
            this.SizeChanged += new System.EventHandler(this.OpenLayersMapForm_SizeChanged);
            this.Enter += new System.EventHandler(this.OpenLayersMapForm_Enter);
            this.Leave += new System.EventHandler(this.OpenLayersMapForm_Leave);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.WebBrowser webBrowser1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.RadioButton radioButtonAll;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton radioButtonSelected;
        private System.Windows.Forms.RadioButton radioButtonActive;
    }
}