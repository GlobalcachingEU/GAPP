namespace GlobalcachingApplication.Plugins.TxtSearch
{
    partial class GeocacheTextSearchForm
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
            this.buttonOK = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radioButtonAddToCurrent = new System.Windows.Forms.RadioButton();
            this.radioButtonWithinSelection = new System.Windows.Forms.RadioButton();
            this.radioButtonNewSearch = new System.Windows.Forms.RadioButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.radioButtonOwner = new System.Windows.Forms.RadioButton();
            this.radioButtonDescription = new System.Windows.Forms.RadioButton();
            this.radioButtonCode = new System.Windows.Forms.RadioButton();
            this.radioButtonName = new System.Windows.Forms.RadioButton();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.textBoxFind = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.checkBoxCaseSensative = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Enabled = false;
            this.buttonOK.Location = new System.Drawing.Point(107, 360);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 0;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radioButtonAddToCurrent);
            this.groupBox1.Controls.Add(this.radioButtonWithinSelection);
            this.groupBox1.Controls.Add(this.radioButtonNewSearch);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(266, 92);
            this.groupBox1.TabIndex = 1;
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
            this.groupBox2.Controls.Add(this.radioButtonOwner);
            this.groupBox2.Controls.Add(this.radioButtonDescription);
            this.groupBox2.Controls.Add(this.radioButtonCode);
            this.groupBox2.Controls.Add(this.radioButtonName);
            this.groupBox2.Location = new System.Drawing.Point(12, 110);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(266, 129);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Field";
            // 
            // radioButtonOwner
            // 
            this.radioButtonOwner.AutoSize = true;
            this.radioButtonOwner.Location = new System.Drawing.Point(22, 92);
            this.radioButtonOwner.Name = "radioButtonOwner";
            this.radioButtonOwner.Size = new System.Drawing.Size(56, 17);
            this.radioButtonOwner.TabIndex = 3;
            this.radioButtonOwner.TabStop = true;
            this.radioButtonOwner.Text = "Owner";
            this.radioButtonOwner.UseVisualStyleBackColor = true;
            // 
            // radioButtonDescription
            // 
            this.radioButtonDescription.AutoSize = true;
            this.radioButtonDescription.Location = new System.Drawing.Point(22, 68);
            this.radioButtonDescription.Name = "radioButtonDescription";
            this.radioButtonDescription.Size = new System.Drawing.Size(78, 17);
            this.radioButtonDescription.TabIndex = 2;
            this.radioButtonDescription.TabStop = true;
            this.radioButtonDescription.Text = "Description";
            this.radioButtonDescription.UseVisualStyleBackColor = true;
            // 
            // radioButtonCode
            // 
            this.radioButtonCode.AutoSize = true;
            this.radioButtonCode.Location = new System.Drawing.Point(22, 44);
            this.radioButtonCode.Name = "radioButtonCode";
            this.radioButtonCode.Size = new System.Drawing.Size(50, 17);
            this.radioButtonCode.TabIndex = 1;
            this.radioButtonCode.TabStop = true;
            this.radioButtonCode.Text = "Code";
            this.radioButtonCode.UseVisualStyleBackColor = true;
            // 
            // radioButtonName
            // 
            this.radioButtonName.AutoSize = true;
            this.radioButtonName.Checked = true;
            this.radioButtonName.Location = new System.Drawing.Point(22, 20);
            this.radioButtonName.Name = "radioButtonName";
            this.radioButtonName.Size = new System.Drawing.Size(53, 17);
            this.radioButtonName.TabIndex = 0;
            this.radioButtonName.TabStop = true;
            this.radioButtonName.Text = "Name";
            this.radioButtonName.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.textBoxFind);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Controls.Add(this.checkBoxCaseSensative);
            this.groupBox3.Location = new System.Drawing.Point(13, 258);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(265, 85);
            this.groupBox3.TabIndex = 3;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Find";
            // 
            // textBoxFind
            // 
            this.textBoxFind.Location = new System.Drawing.Point(71, 27);
            this.textBoxFind.Name = "textBoxFind";
            this.textBoxFind.Size = new System.Drawing.Size(170, 20);
            this.textBoxFind.TabIndex = 2;
            this.textBoxFind.TextChanged += new System.EventHandler(this.textBoxFind_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(27, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Find";
            // 
            // checkBoxCaseSensative
            // 
            this.checkBoxCaseSensative.AutoSize = true;
            this.checkBoxCaseSensative.Location = new System.Drawing.Point(71, 50);
            this.checkBoxCaseSensative.Name = "checkBoxCaseSensative";
            this.checkBoxCaseSensative.Size = new System.Drawing.Size(98, 17);
            this.checkBoxCaseSensative.TabIndex = 0;
            this.checkBoxCaseSensative.Text = "Case sensative";
            this.checkBoxCaseSensative.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(55, 30);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(10, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = ":";
            // 
            // GeocacheTextSearchForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(291, 395);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.buttonOK);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GeocacheTextSearchForm";
            this.ShowIcon = false;
            this.Text = "Geocache Search: Text";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radioButtonAddToCurrent;
        private System.Windows.Forms.RadioButton radioButtonWithinSelection;
        private System.Windows.Forms.RadioButton radioButtonNewSearch;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton radioButtonOwner;
        private System.Windows.Forms.RadioButton radioButtonDescription;
        private System.Windows.Forms.RadioButton radioButtonCode;
        private System.Windows.Forms.RadioButton radioButtonName;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox textBoxFind;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkBoxCaseSensative;
        private System.Windows.Forms.Label label2;
    }
}