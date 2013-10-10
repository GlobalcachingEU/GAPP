namespace GlobalcachingApplication.Plugins.Geometry
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
            this.labelLevel = new System.Windows.Forms.Label();
            this.comboBoxLevel = new System.Windows.Forms.ComboBox();
            this.comboBoxAreaName = new System.Windows.Forms.ComboBox();
            this.labelArea = new System.Windows.Forms.Label();
            this.labelParent = new System.Windows.Forms.Label();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.buttonDelete = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.buttonRestore = new System.Windows.Forms.Button();
            this.comboBoxParent = new System.Windows.Forms.ComboBox();
            this.comboBoxParentOfArea = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonSetParent = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // labelLevel
            // 
            this.labelLevel.AutoSize = true;
            this.labelLevel.Location = new System.Drawing.Point(12, 17);
            this.labelLevel.Name = "labelLevel";
            this.labelLevel.Size = new System.Drawing.Size(33, 13);
            this.labelLevel.TabIndex = 0;
            this.labelLevel.Text = "Level";
            // 
            // comboBoxLevel
            // 
            this.comboBoxLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxLevel.FormattingEnabled = true;
            this.comboBoxLevel.Location = new System.Drawing.Point(73, 14);
            this.comboBoxLevel.Name = "comboBoxLevel";
            this.comboBoxLevel.Size = new System.Drawing.Size(165, 21);
            this.comboBoxLevel.TabIndex = 1;
            this.comboBoxLevel.SelectedIndexChanged += new System.EventHandler(this.comboBoxLevel_SelectedIndexChanged);
            // 
            // comboBoxAreaName
            // 
            this.comboBoxAreaName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxAreaName.FormattingEnabled = true;
            this.comboBoxAreaName.Location = new System.Drawing.Point(73, 99);
            this.comboBoxAreaName.Name = "comboBoxAreaName";
            this.comboBoxAreaName.Size = new System.Drawing.Size(165, 21);
            this.comboBoxAreaName.TabIndex = 2;
            this.comboBoxAreaName.SelectedIndexChanged += new System.EventHandler(this.comboBoxAreaName_SelectedIndexChanged);
            // 
            // labelArea
            // 
            this.labelArea.AutoSize = true;
            this.labelArea.Location = new System.Drawing.Point(12, 102);
            this.labelArea.Name = "labelArea";
            this.labelArea.Size = new System.Drawing.Size(29, 13);
            this.labelArea.TabIndex = 3;
            this.labelArea.Text = "Area";
            // 
            // labelParent
            // 
            this.labelParent.AutoSize = true;
            this.labelParent.Location = new System.Drawing.Point(261, 17);
            this.labelParent.Name = "labelParent";
            this.labelParent.Size = new System.Drawing.Size(38, 13);
            this.labelParent.TabIndex = 4;
            this.labelParent.Text = "Parent";
            // 
            // buttonAdd
            // 
            this.buttonAdd.Location = new System.Drawing.Point(73, 53);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(165, 23);
            this.buttonAdd.TabIndex = 6;
            this.buttonAdd.Text = "Add / Create";
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // buttonDelete
            // 
            this.buttonDelete.Location = new System.Drawing.Point(73, 126);
            this.buttonDelete.Name = "buttonDelete";
            this.buttonDelete.Size = new System.Drawing.Size(165, 23);
            this.buttonDelete.TabIndex = 7;
            this.buttonDelete.Text = "Delete";
            this.buttonDelete.UseVisualStyleBackColor = true;
            this.buttonDelete.Click += new System.EventHandler(this.buttonDelete_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.Filter = "*.txt|*.txt";
            this.openFileDialog1.Multiselect = true;
            // 
            // buttonRestore
            // 
            this.buttonRestore.Location = new System.Drawing.Point(170, 180);
            this.buttonRestore.Name = "buttonRestore";
            this.buttonRestore.Size = new System.Drawing.Size(171, 23);
            this.buttonRestore.TabIndex = 8;
            this.buttonRestore.Text = "Restore default";
            this.buttonRestore.UseVisualStyleBackColor = true;
            this.buttonRestore.Click += new System.EventHandler(this.buttonRestore_Click);
            // 
            // comboBoxParent
            // 
            this.comboBoxParent.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxParent.FormattingEnabled = true;
            this.comboBoxParent.Location = new System.Drawing.Point(331, 14);
            this.comboBoxParent.Name = "comboBoxParent";
            this.comboBoxParent.Size = new System.Drawing.Size(171, 21);
            this.comboBoxParent.TabIndex = 9;
            // 
            // comboBoxParentOfArea
            // 
            this.comboBoxParentOfArea.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxParentOfArea.FormattingEnabled = true;
            this.comboBoxParentOfArea.Location = new System.Drawing.Point(331, 99);
            this.comboBoxParentOfArea.Name = "comboBoxParentOfArea";
            this.comboBoxParentOfArea.Size = new System.Drawing.Size(171, 21);
            this.comboBoxParentOfArea.TabIndex = 11;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(261, 102);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "Parent";
            // 
            // buttonSetParent
            // 
            this.buttonSetParent.Location = new System.Drawing.Point(331, 127);
            this.buttonSetParent.Name = "buttonSetParent";
            this.buttonSetParent.Size = new System.Drawing.Size(171, 23);
            this.buttonSetParent.TabIndex = 12;
            this.buttonSetParent.Text = "Set";
            this.buttonSetParent.UseVisualStyleBackColor = true;
            this.buttonSetParent.Click += new System.EventHandler(this.buttonSetParent_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(315, 17);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(10, 13);
            this.label2.TabIndex = 13;
            this.label2.Text = ":";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(315, 102);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(10, 13);
            this.label3.TabIndex = 14;
            this.label3.Text = ":";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(57, 17);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(10, 13);
            this.label4.TabIndex = 15;
            this.label4.Text = ":";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(57, 102);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(10, 13);
            this.label5.TabIndex = 16;
            this.label5.Text = ":";
            // 
            // SettingsPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.buttonSetParent);
            this.Controls.Add(this.comboBoxParentOfArea);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comboBoxParent);
            this.Controls.Add(this.buttonRestore);
            this.Controls.Add(this.buttonDelete);
            this.Controls.Add(this.buttonAdd);
            this.Controls.Add(this.labelParent);
            this.Controls.Add(this.labelArea);
            this.Controls.Add(this.comboBoxAreaName);
            this.Controls.Add(this.comboBoxLevel);
            this.Controls.Add(this.labelLevel);
            this.Name = "SettingsPanel";
            this.Size = new System.Drawing.Size(521, 223);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelLevel;
        private System.Windows.Forms.ComboBox comboBoxLevel;
        private System.Windows.Forms.ComboBox comboBoxAreaName;
        private System.Windows.Forms.Label labelArea;
        private System.Windows.Forms.Label labelParent;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.Button buttonDelete;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button buttonRestore;
        private System.Windows.Forms.ComboBox comboBoxParent;
        private System.Windows.Forms.ComboBox comboBoxParentOfArea;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonSetParent;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
    }
}
