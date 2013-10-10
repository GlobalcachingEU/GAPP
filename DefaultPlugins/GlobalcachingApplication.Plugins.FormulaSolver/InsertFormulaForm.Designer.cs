namespace GlobalcachingApplication.Plugins.FormulaSolver
{
    partial class InsertFormulaForm
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
            this.lblGroup = new System.Windows.Forms.GroupBox();
            this.lbGroup = new System.Windows.Forms.ListBox();
            this.lblFunctions = new System.Windows.Forms.GroupBox();
            this.lbFunction = new System.Windows.Forms.ListBox();
            this.lblOtherNames = new System.Windows.Forms.GroupBox();
            this.lbAltNames = new System.Windows.Forms.ListBox();
            this.lblDescription = new System.Windows.Forms.GroupBox();
            this.tbDescription = new System.Windows.Forms.TextBox();
            this.bnInsert = new System.Windows.Forms.Button();
            this.bnCancel = new System.Windows.Forms.Button();
            this.lblGroup.SuspendLayout();
            this.lblFunctions.SuspendLayout();
            this.lblOtherNames.SuspendLayout();
            this.lblDescription.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblGroup
            // 
            this.lblGroup.Controls.Add(this.lbGroup);
            this.lblGroup.Location = new System.Drawing.Point(12, 12);
            this.lblGroup.Name = "lblGroup";
            this.lblGroup.Size = new System.Drawing.Size(200, 161);
            this.lblGroup.TabIndex = 0;
            this.lblGroup.TabStop = false;
            this.lblGroup.Text = "Group";
            // 
            // lbGroup
            // 
            this.lbGroup.FormattingEnabled = true;
            this.lbGroup.Location = new System.Drawing.Point(6, 19);
            this.lbGroup.Name = "lbGroup";
            this.lbGroup.Size = new System.Drawing.Size(188, 134);
            this.lbGroup.Sorted = true;
            this.lbGroup.TabIndex = 0;
            // 
            // lblFunctions
            // 
            this.lblFunctions.Controls.Add(this.lbFunction);
            this.lblFunctions.Location = new System.Drawing.Point(218, 12);
            this.lblFunctions.Name = "lblFunctions";
            this.lblFunctions.Size = new System.Drawing.Size(301, 161);
            this.lblFunctions.TabIndex = 1;
            this.lblFunctions.TabStop = false;
            this.lblFunctions.Text = "Functions";
            // 
            // lbFunction
            // 
            this.lbFunction.FormattingEnabled = true;
            this.lbFunction.Location = new System.Drawing.Point(6, 19);
            this.lbFunction.Name = "lbFunction";
            this.lbFunction.Size = new System.Drawing.Size(289, 134);
            this.lbFunction.Sorted = true;
            this.lbFunction.TabIndex = 1;
            this.lbFunction.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lbFunction_MouseDoubleClick);
            // 
            // lblOtherNames
            // 
            this.lblOtherNames.Controls.Add(this.lbAltNames);
            this.lblOtherNames.Location = new System.Drawing.Point(12, 179);
            this.lblOtherNames.Name = "lblOtherNames";
            this.lblOtherNames.Size = new System.Drawing.Size(200, 110);
            this.lblOtherNames.TabIndex = 2;
            this.lblOtherNames.TabStop = false;
            this.lblOtherNames.Text = "Other Names";
            // 
            // lbAltNames
            // 
            this.lbAltNames.FormattingEnabled = true;
            this.lbAltNames.Location = new System.Drawing.Point(6, 19);
            this.lbAltNames.Name = "lbAltNames";
            this.lbAltNames.Size = new System.Drawing.Size(188, 82);
            this.lbAltNames.Sorted = true;
            this.lbAltNames.TabIndex = 2;
            // 
            // lblDescription
            // 
            this.lblDescription.Controls.Add(this.tbDescription);
            this.lblDescription.Location = new System.Drawing.Point(218, 179);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(301, 110);
            this.lblDescription.TabIndex = 3;
            this.lblDescription.TabStop = false;
            this.lblDescription.Text = "Description";
            // 
            // tbDescription
            // 
            this.tbDescription.Location = new System.Drawing.Point(6, 19);
            this.tbDescription.Multiline = true;
            this.tbDescription.Name = "tbDescription";
            this.tbDescription.ReadOnly = true;
            this.tbDescription.Size = new System.Drawing.Size(289, 82);
            this.tbDescription.TabIndex = 0;
            this.tbDescription.TabStop = false;
            // 
            // bnInsert
            // 
            this.bnInsert.Location = new System.Drawing.Point(363, 302);
            this.bnInsert.Name = "bnInsert";
            this.bnInsert.Size = new System.Drawing.Size(75, 23);
            this.bnInsert.TabIndex = 3;
            this.bnInsert.Text = "Insert";
            this.bnInsert.UseVisualStyleBackColor = true;
            this.bnInsert.Click += new System.EventHandler(this.bnInsert_Click);
            // 
            // bnCancel
            // 
            this.bnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.bnCancel.Location = new System.Drawing.Point(444, 302);
            this.bnCancel.Name = "bnCancel";
            this.bnCancel.Size = new System.Drawing.Size(75, 23);
            this.bnCancel.TabIndex = 4;
            this.bnCancel.Text = "Cancel";
            this.bnCancel.UseVisualStyleBackColor = true;
            // 
            // InsertFormulaForm
            // 
            this.AcceptButton = this.bnInsert;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.bnCancel;
            this.ClientSize = new System.Drawing.Size(531, 337);
            this.Controls.Add(this.bnCancel);
            this.Controls.Add(this.bnInsert);
            this.Controls.Add(this.lblDescription);
            this.Controls.Add(this.lblOtherNames);
            this.Controls.Add(this.lblFunctions);
            this.Controls.Add(this.lblGroup);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "InsertFormulaForm";
            this.Text = "Insert Formula";
            this.lblGroup.ResumeLayout(false);
            this.lblFunctions.ResumeLayout(false);
            this.lblOtherNames.ResumeLayout(false);
            this.lblDescription.ResumeLayout(false);
            this.lblDescription.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox lblGroup;
        private System.Windows.Forms.GroupBox lblFunctions;
        private System.Windows.Forms.GroupBox lblOtherNames;
        private System.Windows.Forms.GroupBox lblDescription;
        private System.Windows.Forms.ListBox lbGroup;
        private System.Windows.Forms.ListBox lbFunction;
        private System.Windows.Forms.Button bnInsert;
        private System.Windows.Forms.Button bnCancel;
        private System.Windows.Forms.ListBox lbAltNames;
        private System.Windows.Forms.TextBox tbDescription;
    }
}