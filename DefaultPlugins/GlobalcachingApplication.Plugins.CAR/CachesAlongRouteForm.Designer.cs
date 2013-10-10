namespace GlobalcachingApplication.Plugins.CAR
{
    partial class CachesAlongRouteForm
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
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.radioButtonMiles = new System.Windows.Forms.RadioButton();
            this.numericUpDownDist = new System.Windows.Forms.NumericUpDown();
            this.buttonSelect = new System.Windows.Forms.Button();
            this.radioButtonKm = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radioButtonAddToCurrent = new System.Windows.Forms.RadioButton();
            this.radioButtonWithinSelection = new System.Windows.Forms.RadioButton();
            this.radioButtonNewSearch = new System.Windows.Forms.RadioButton();
            this.buttonReload = new System.Windows.Forms.Button();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.panel1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDist)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.groupBox2);
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(596, 125);
            this.panel1.TabIndex = 0;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.radioButtonMiles);
            this.groupBox2.Controls.Add(this.numericUpDownDist);
            this.groupBox2.Controls.Add(this.buttonSelect);
            this.groupBox2.Controls.Add(this.radioButtonKm);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox2.Location = new System.Drawing.Point(0, 61);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(596, 57);
            this.groupBox2.TabIndex = 12;
            this.groupBox2.TabStop = false;
            // 
            // radioButtonMiles
            // 
            this.radioButtonMiles.AutoSize = true;
            this.radioButtonMiles.Location = new System.Drawing.Point(226, 22);
            this.radioButtonMiles.Name = "radioButtonMiles";
            this.radioButtonMiles.Size = new System.Drawing.Size(48, 17);
            this.radioButtonMiles.TabIndex = 10;
            this.radioButtonMiles.TabStop = true;
            this.radioButtonMiles.Text = "miles";
            this.radioButtonMiles.UseVisualStyleBackColor = true;
            // 
            // numericUpDownDist
            // 
            this.numericUpDownDist.DecimalPlaces = 1;
            this.numericUpDownDist.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numericUpDownDist.Location = new System.Drawing.Point(94, 21);
            this.numericUpDownDist.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numericUpDownDist.Name = "numericUpDownDist";
            this.numericUpDownDist.Size = new System.Drawing.Size(57, 20);
            this.numericUpDownDist.TabIndex = 8;
            this.numericUpDownDist.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // buttonSelect
            // 
            this.buttonSelect.Enabled = false;
            this.buttonSelect.Location = new System.Drawing.Point(12, 19);
            this.buttonSelect.Name = "buttonSelect";
            this.buttonSelect.Size = new System.Drawing.Size(75, 23);
            this.buttonSelect.TabIndex = 7;
            this.buttonSelect.Text = "Select";
            this.buttonSelect.UseVisualStyleBackColor = true;
            this.buttonSelect.Click += new System.EventHandler(this.buttonSelect_Click);
            // 
            // radioButtonKm
            // 
            this.radioButtonKm.AutoSize = true;
            this.radioButtonKm.Checked = true;
            this.radioButtonKm.Location = new System.Drawing.Point(171, 22);
            this.radioButtonKm.Name = "radioButtonKm";
            this.radioButtonKm.Size = new System.Drawing.Size(39, 17);
            this.radioButtonKm.TabIndex = 9;
            this.radioButtonKm.TabStop = true;
            this.radioButtonKm.Text = "km";
            this.radioButtonKm.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radioButtonAddToCurrent);
            this.groupBox1.Controls.Add(this.radioButtonWithinSelection);
            this.groupBox1.Controls.Add(this.radioButtonNewSearch);
            this.groupBox1.Controls.Add(this.buttonReload);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(596, 61);
            this.groupBox1.TabIndex = 11;
            this.groupBox1.TabStop = false;
            // 
            // radioButtonAddToCurrent
            // 
            this.radioButtonAddToCurrent.AutoSize = true;
            this.radioButtonAddToCurrent.Location = new System.Drawing.Point(367, 28);
            this.radioButtonAddToCurrent.Name = "radioButtonAddToCurrent";
            this.radioButtonAddToCurrent.Size = new System.Drawing.Size(137, 17);
            this.radioButtonAddToCurrent.TabIndex = 6;
            this.radioButtonAddToCurrent.Text = "Add to current selection";
            this.radioButtonAddToCurrent.UseVisualStyleBackColor = true;
            // 
            // radioButtonWithinSelection
            // 
            this.radioButtonWithinSelection.AutoSize = true;
            this.radioButtonWithinSelection.Location = new System.Drawing.Point(209, 28);
            this.radioButtonWithinSelection.Name = "radioButtonWithinSelection";
            this.radioButtonWithinSelection.Size = new System.Drawing.Size(134, 17);
            this.radioButtonWithinSelection.TabIndex = 5;
            this.radioButtonWithinSelection.Text = "Search within selection";
            this.radioButtonWithinSelection.UseVisualStyleBackColor = true;
            // 
            // radioButtonNewSearch
            // 
            this.radioButtonNewSearch.AutoSize = true;
            this.radioButtonNewSearch.Checked = true;
            this.radioButtonNewSearch.Location = new System.Drawing.Point(91, 28);
            this.radioButtonNewSearch.Name = "radioButtonNewSearch";
            this.radioButtonNewSearch.Size = new System.Drawing.Size(82, 17);
            this.radioButtonNewSearch.TabIndex = 4;
            this.radioButtonNewSearch.TabStop = true;
            this.radioButtonNewSearch.Text = "New search";
            this.radioButtonNewSearch.UseVisualStyleBackColor = true;
            // 
            // buttonReload
            // 
            this.buttonReload.Location = new System.Drawing.Point(9, 22);
            this.buttonReload.Name = "buttonReload";
            this.buttonReload.Size = new System.Drawing.Size(75, 23);
            this.buttonReload.TabIndex = 0;
            this.buttonReload.Text = "Restart";
            this.buttonReload.UseVisualStyleBackColor = true;
            this.buttonReload.Click += new System.EventHandler(this.buttonReload_Click);
            // 
            // webBrowser1
            // 
            this.webBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser1.Location = new System.Drawing.Point(0, 125);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(596, 340);
            this.webBrowser1.TabIndex = 2;
            // 
            // timer1
            // 
            this.timer1.Interval = 500;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // CachesAlongRouteForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(596, 465);
            this.Controls.Add(this.webBrowser1);
            this.Controls.Add(this.panel1);
            this.Name = "CachesAlongRouteForm";
            this.ShowIcon = false;
            this.Text = "Caches Along Route";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CachesAlongRouteForm_FormClosing);
            this.Shown += new System.EventHandler(this.CachesAlongRouteForm_Shown);
            this.LocationChanged += new System.EventHandler(this.CachesAlongRouteForm_LocationChanged);
            this.SizeChanged += new System.EventHandler(this.CachesAlongRouteForm_SizeChanged);
            this.Enter += new System.EventHandler(this.CachesAlongRouteForm_Enter);
            this.Leave += new System.EventHandler(this.CachesAlongRouteForm_Leave);
            this.panel1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownDist)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.WebBrowser webBrowser1;
        private System.Windows.Forms.Button buttonReload;
        private System.Windows.Forms.RadioButton radioButtonAddToCurrent;
        private System.Windows.Forms.RadioButton radioButtonWithinSelection;
        private System.Windows.Forms.RadioButton radioButtonNewSearch;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button buttonSelect;
        private System.Windows.Forms.NumericUpDown numericUpDownDist;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton radioButtonMiles;
        private System.Windows.Forms.RadioButton radioButtonKm;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}