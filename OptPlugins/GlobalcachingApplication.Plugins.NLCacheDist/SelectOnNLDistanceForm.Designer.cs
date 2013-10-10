namespace GlobalcachingApplication.Plugins.NLCacheDist
{
    partial class SelectOnNLDistanceForm
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radioButtonAddToCurrent = new System.Windows.Forms.RadioButton();
            this.radioButtonWithinSelection = new System.Windows.Forms.RadioButton();
            this.radioButtonNewSearch = new System.Windows.Forms.RadioButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.numericUpDownMax = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownMin = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonOK = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMax)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMin)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radioButtonAddToCurrent);
            this.groupBox1.Controls.Add(this.radioButtonWithinSelection);
            this.groupBox1.Controls.Add(this.radioButtonNewSearch);
            this.groupBox1.Location = new System.Drawing.Point(6, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(266, 92);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Selection";
            // 
            // radioButtonAddToCurrent
            // 
            this.radioButtonAddToCurrent.AutoSize = true;
            this.radioButtonAddToCurrent.Location = new System.Drawing.Point(22, 65);
            this.radioButtonAddToCurrent.Name = "radioButtonAddToCurrent";
            this.radioButtonAddToCurrent.Size = new System.Drawing.Size(137, 17);
            this.radioButtonAddToCurrent.TabIndex = 2;
            this.radioButtonAddToCurrent.TabStop = true;
            this.radioButtonAddToCurrent.Text = "Add to current selection";
            this.radioButtonAddToCurrent.UseVisualStyleBackColor = true;
            // 
            // radioButtonWithinSelection
            // 
            this.radioButtonWithinSelection.AutoSize = true;
            this.radioButtonWithinSelection.Location = new System.Drawing.Point(22, 42);
            this.radioButtonWithinSelection.Name = "radioButtonWithinSelection";
            this.radioButtonWithinSelection.Size = new System.Drawing.Size(134, 17);
            this.radioButtonWithinSelection.TabIndex = 1;
            this.radioButtonWithinSelection.Text = "Search within selection";
            this.radioButtonWithinSelection.UseVisualStyleBackColor = true;
            // 
            // radioButtonNewSearch
            // 
            this.radioButtonNewSearch.AutoSize = true;
            this.radioButtonNewSearch.Checked = true;
            this.radioButtonNewSearch.Location = new System.Drawing.Point(22, 19);
            this.radioButtonNewSearch.Name = "radioButtonNewSearch";
            this.radioButtonNewSearch.Size = new System.Drawing.Size(82, 17);
            this.radioButtonNewSearch.TabIndex = 0;
            this.radioButtonNewSearch.TabStop = true;
            this.radioButtonNewSearch.Text = "New search";
            this.radioButtonNewSearch.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.numericUpDownMax);
            this.groupBox2.Controls.Add(this.numericUpDownMin);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Location = new System.Drawing.Point(6, 111);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(266, 115);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Find";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(221, 67);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(21, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "km";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(221, 31);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(21, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "km";
            // 
            // numericUpDownMax
            // 
            this.numericUpDownMax.DecimalPlaces = 1;
            this.numericUpDownMax.Location = new System.Drawing.Point(137, 60);
            this.numericUpDownMax.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDownMax.Name = "numericUpDownMax";
            this.numericUpDownMax.Size = new System.Drawing.Size(78, 20);
            this.numericUpDownMax.TabIndex = 3;
            this.numericUpDownMax.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // numericUpDownMin
            // 
            this.numericUpDownMin.DecimalPlaces = 1;
            this.numericUpDownMin.Location = new System.Drawing.Point(137, 24);
            this.numericUpDownMin.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDownMin.Name = "numericUpDownMin";
            this.numericUpDownMin.Size = new System.Drawing.Size(78, 20);
            this.numericUpDownMin.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(34, 67);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(73, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Max. distance";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(34, 31);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(70, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Min. distance";
            // 
            // buttonOK
            // 
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(106, 242);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 4;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(121, 31);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(10, 13);
            this.label5.TabIndex = 6;
            this.label5.Text = ":";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(121, 67);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(10, 13);
            this.label6.TabIndex = 7;
            this.label6.Text = ":";
            // 
            // SelectOnNLDistanceForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 277);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "SelectOnNLDistanceForm";
            this.Text = "Search for geocache distance";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMax)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMin)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonOK;
        public System.Windows.Forms.RadioButton radioButtonAddToCurrent;
        public System.Windows.Forms.RadioButton radioButtonWithinSelection;
        public System.Windows.Forms.RadioButton radioButtonNewSearch;
        public System.Windows.Forms.NumericUpDown numericUpDownMax;
        public System.Windows.Forms.NumericUpDown numericUpDownMin;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
    }
}