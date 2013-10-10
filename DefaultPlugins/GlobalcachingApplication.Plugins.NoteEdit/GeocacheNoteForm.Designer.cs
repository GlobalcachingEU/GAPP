namespace GlobalcachingApplication.Plugins.NoteEdit
{
    partial class GeocacheNoteForm
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
            this.label4 = new System.Windows.Forms.Label();
            this.labelActiveGeocache = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.htmlEditorControl1 = new Globalcaching.HtmlEditor.HtmlEditorControl();
            this.panel3 = new System.Windows.Forms.Panel();
            this.buttonSaveNote = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.labelActiveGeocache);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(519, 35);
            this.panel1.TabIndex = 0;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(84, 13);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(10, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = ":";
            // 
            // labelActiveGeocache
            // 
            this.labelActiveGeocache.AutoSize = true;
            this.labelActiveGeocache.Location = new System.Drawing.Point(100, 13);
            this.labelActiveGeocache.Name = "labelActiveGeocache";
            this.labelActiveGeocache.Size = new System.Drawing.Size(10, 13);
            this.labelActiveGeocache.TabIndex = 1;
            this.labelActiveGeocache.Text = "-";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 13);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(50, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Notes for";
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 35);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.textBox1);
            this.splitContainer1.Panel1.Controls.Add(this.panel2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.htmlEditorControl1);
            this.splitContainer1.Panel2.Controls.Add(this.panel3);
            this.splitContainer1.Size = new System.Drawing.Size(519, 419);
            this.splitContainer1.SplitterDistance = 92;
            this.splitContainer1.TabIndex = 1;
            // 
            // textBox1
            // 
            this.textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox1.Location = new System.Drawing.Point(0, 25);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(515, 63);
            this.textBox1.TabIndex = 1;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.label1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(515, 25);
            this.panel2.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(169, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Personal note on geocaching.com";
            // 
            // htmlEditorControl1
            // 
            this.htmlEditorControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.htmlEditorControl1.Location = new System.Drawing.Point(0, 34);
            this.htmlEditorControl1.Name = "htmlEditorControl1";
            this.htmlEditorControl1.Size = new System.Drawing.Size(515, 285);
            this.htmlEditorControl1.TabIndex = 1;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.buttonSaveNote);
            this.panel3.Controls.Add(this.label2);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel3.Location = new System.Drawing.Point(0, 0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(515, 34);
            this.panel3.TabIndex = 0;
            // 
            // buttonSaveNote
            // 
            this.buttonSaveNote.Location = new System.Drawing.Point(48, 5);
            this.buttonSaveNote.Name = "buttonSaveNote";
            this.buttonSaveNote.Size = new System.Drawing.Size(89, 23);
            this.buttonSaveNote.TabIndex = 1;
            this.buttonSaveNote.Text = "Save";
            this.buttonSaveNote.UseVisualStyleBackColor = true;
            this.buttonSaveNote.Click += new System.EventHandler(this.buttonSaveNote_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(30, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Note";
            // 
            // GeocacheNoteForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(519, 454);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.panel1);
            this.Name = "GeocacheNoteForm";
            this.ShowIcon = false;
            this.Text = "Geocache Note Editor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.GeocacheNoteForm_FormClosing);
            this.LocationChanged += new System.EventHandler(this.GeocacheNoteForm_LocationChanged);
            this.SizeChanged += new System.EventHandler(this.GeocacheNoteForm_SizeChanged);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Label label2;
        private Globalcaching.HtmlEditor.HtmlEditorControl htmlEditorControl1;
        private System.Windows.Forms.Label labelActiveGeocache;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button buttonSaveNote;
        private System.Windows.Forms.Label label4;
    }
}