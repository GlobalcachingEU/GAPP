namespace GlobalcachingApplication.Plugins.StatsGen
{
    partial class StatisticsGeneratorForm
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button3 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.buttonGenerate = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.textBoxGeneratedScript = new System.Windows.Forms.TextBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.panelEdit = new System.Windows.Forms.Panel();
            this.textBoxScript = new System.Windows.Forms.TextBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.buttonDeleteScript = new System.Windows.Forms.Button();
            this.buttonSaveScript = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.labelScriptType = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxScriptName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.textBoxHtml = new System.Windows.Forms.TextBox();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.button4 = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.panelEdit.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Controls.Add(this.buttonGenerate);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(975, 71);
            this.panel1.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.button4);
            this.groupBox1.Controls.Add(this.button3);
            this.groupBox1.Controls.Add(this.button2);
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Location = new System.Drawing.Point(159, 5);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox1.Size = new System.Drawing.Size(684, 59);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Create";
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(345, 25);
            this.button3.Margin = new System.Windows.Forms.Padding(4);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(168, 28);
            this.button3.TabIndex = 2;
            this.button3.Text = "Statistics";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(184, 25);
            this.button2.Margin = new System.Windows.Forms.Padding(4);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(153, 28);
            this.button2.TabIndex = 1;
            this.button2.Text = "Extension";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(23, 25);
            this.button1.Margin = new System.Windows.Forms.Padding(4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(153, 28);
            this.button1.TabIndex = 0;
            this.button1.Text = "Template";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // buttonGenerate
            // 
            this.buttonGenerate.Location = new System.Drawing.Point(16, 30);
            this.buttonGenerate.Margin = new System.Windows.Forms.Padding(4);
            this.buttonGenerate.Name = "buttonGenerate";
            this.buttonGenerate.Size = new System.Drawing.Size(135, 28);
            this.buttonGenerate.TabIndex = 0;
            this.buttonGenerate.Text = "Generate";
            this.buttonGenerate.UseVisualStyleBackColor = true;
            this.buttonGenerate.Click += new System.EventHandler(this.buttonGenerate_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Location = new System.Drawing.Point(0, 681);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 19, 0);
            this.statusStrip1.Size = new System.Drawing.Size(975, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 71);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.treeView1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tabControl1);
            this.splitContainer1.Size = new System.Drawing.Size(975, 610);
            this.splitContainer1.SplitterDistance = 288;
            this.splitContainer1.SplitterWidth = 5;
            this.splitContainer1.TabIndex = 2;
            // 
            // treeView1
            // 
            this.treeView1.AllowDrop = true;
            this.treeView1.CheckBoxes = true;
            this.treeView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView1.HideSelection = false;
            this.treeView1.Location = new System.Drawing.Point(0, 0);
            this.treeView1.Margin = new System.Windows.Forms.Padding(4);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(284, 606);
            this.treeView1.TabIndex = 0;
            this.treeView1.BeforeCheck += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeView1_BeforeCheck);
            this.treeView1.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterCheck);
            this.treeView1.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.treeView1_ItemDrag);
            this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
            this.treeView1.DragDrop += new System.Windows.Forms.DragEventHandler(this.treeView1_DragDrop);
            this.treeView1.DragOver += new System.Windows.Forms.DragEventHandler(this.treeView1_DragOver);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(4);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(678, 606);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.textBoxGeneratedScript);
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage1.Size = new System.Drawing.Size(670, 577);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Script";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // textBoxGeneratedScript
            // 
            this.textBoxGeneratedScript.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxGeneratedScript.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxGeneratedScript.Location = new System.Drawing.Point(4, 4);
            this.textBoxGeneratedScript.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxGeneratedScript.Multiline = true;
            this.textBoxGeneratedScript.Name = "textBoxGeneratedScript";
            this.textBoxGeneratedScript.ReadOnly = true;
            this.textBoxGeneratedScript.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxGeneratedScript.Size = new System.Drawing.Size(662, 569);
            this.textBoxGeneratedScript.TabIndex = 0;
            this.textBoxGeneratedScript.WordWrap = false;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.panelEdit);
            this.tabPage2.Location = new System.Drawing.Point(4, 25);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage2.Size = new System.Drawing.Size(670, 577);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Edit";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // panelEdit
            // 
            this.panelEdit.Controls.Add(this.textBoxScript);
            this.panelEdit.Controls.Add(this.panel3);
            this.panelEdit.Controls.Add(this.panel2);
            this.panelEdit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelEdit.Location = new System.Drawing.Point(4, 4);
            this.panelEdit.Margin = new System.Windows.Forms.Padding(4);
            this.panelEdit.Name = "panelEdit";
            this.panelEdit.Size = new System.Drawing.Size(660, 562);
            this.panelEdit.TabIndex = 0;
            // 
            // textBoxScript
            // 
            this.textBoxScript.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxScript.Location = new System.Drawing.Point(0, 94);
            this.textBoxScript.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxScript.Multiline = true;
            this.textBoxScript.Name = "textBoxScript";
            this.textBoxScript.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxScript.Size = new System.Drawing.Size(660, 426);
            this.textBoxScript.TabIndex = 5;
            this.textBoxScript.WordWrap = false;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.buttonDeleteScript);
            this.panel3.Controls.Add(this.buttonSaveScript);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel3.Location = new System.Drawing.Point(0, 520);
            this.panel3.Margin = new System.Windows.Forms.Padding(4);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(660, 42);
            this.panel3.TabIndex = 4;
            // 
            // buttonDeleteScript
            // 
            this.buttonDeleteScript.Location = new System.Drawing.Point(372, 7);
            this.buttonDeleteScript.Margin = new System.Windows.Forms.Padding(4);
            this.buttonDeleteScript.Name = "buttonDeleteScript";
            this.buttonDeleteScript.Size = new System.Drawing.Size(161, 28);
            this.buttonDeleteScript.TabIndex = 1;
            this.buttonDeleteScript.Text = "Delete";
            this.buttonDeleteScript.UseVisualStyleBackColor = true;
            this.buttonDeleteScript.Click += new System.EventHandler(this.buttonDeleteScript_Click);
            // 
            // buttonSaveScript
            // 
            this.buttonSaveScript.Location = new System.Drawing.Point(19, 7);
            this.buttonSaveScript.Margin = new System.Windows.Forms.Padding(4);
            this.buttonSaveScript.Name = "buttonSaveScript";
            this.buttonSaveScript.Size = new System.Drawing.Size(155, 28);
            this.buttonSaveScript.TabIndex = 0;
            this.buttonSaveScript.Text = "Save";
            this.buttonSaveScript.UseVisualStyleBackColor = true;
            this.buttonSaveScript.Click += new System.EventHandler(this.buttonSaveScript_Click);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.labelScriptType);
            this.panel2.Controls.Add(this.label3);
            this.panel2.Controls.Add(this.label4);
            this.panel2.Controls.Add(this.textBoxScriptName);
            this.panel2.Controls.Add(this.label2);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Margin = new System.Windows.Forms.Padding(4);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(660, 94);
            this.panel2.TabIndex = 3;
            // 
            // labelScriptType
            // 
            this.labelScriptType.AutoSize = true;
            this.labelScriptType.Location = new System.Drawing.Point(141, 10);
            this.labelScriptType.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelScriptType.Name = "labelScriptType";
            this.labelScriptType.Size = new System.Drawing.Size(13, 17);
            this.labelScriptType.TabIndex = 5;
            this.labelScriptType.Text = "-";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(120, 10);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(12, 17);
            this.label3.TabIndex = 4;
            this.label3.Text = ":";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(29, 10);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(40, 17);
            this.label4.TabIndex = 3;
            this.label4.Text = "Type";
            // 
            // textBoxScriptName
            // 
            this.textBoxScriptName.Location = new System.Drawing.Point(141, 42);
            this.textBoxScriptName.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxScriptName.Name = "textBoxScriptName";
            this.textBoxScriptName.Size = new System.Drawing.Size(264, 22);
            this.textBoxScriptName.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(120, 46);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(12, 17);
            this.label2.TabIndex = 2;
            this.label2.Text = ":";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(29, 46);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(45, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Name";
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.textBoxHtml);
            this.tabPage3.Location = new System.Drawing.Point(4, 25);
            this.tabPage3.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(670, 577);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "HTML";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // textBoxHtml
            // 
            this.textBoxHtml.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxHtml.Location = new System.Drawing.Point(0, 0);
            this.textBoxHtml.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxHtml.Multiline = true;
            this.textBoxHtml.Name = "textBoxHtml";
            this.textBoxHtml.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxHtml.Size = new System.Drawing.Size(668, 570);
            this.textBoxHtml.TabIndex = 0;
            this.textBoxHtml.WordWrap = false;
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.webBrowser1);
            this.tabPage4.Location = new System.Drawing.Point(4, 25);
            this.tabPage4.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Size = new System.Drawing.Size(670, 577);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Preview";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // webBrowser1
            // 
            this.webBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser1.Location = new System.Drawing.Point(0, 0);
            this.webBrowser1.Margin = new System.Windows.Forms.Padding(4);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(27, 25);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(665, 567);
            this.webBrowser1.TabIndex = 0;
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(520, 25);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(144, 28);
            this.button4.TabIndex = 3;
            this.button4.Text = "Skin";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // StatisticsGeneratorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(975, 703);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.panel1);
            this.Margin = new System.Windows.Forms.Padding(5);
            this.Name = "StatisticsGeneratorForm";
            this.ShowIcon = false;
            this.Text = "StatisticsGeneratorForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.StatisticsGeneratorForm_FormClosing);
            this.Shown += new System.EventHandler(this.StatisticsGeneratorForm_Shown);
            this.LocationChanged += new System.EventHandler(this.StatisticsGeneratorForm_LocationChanged);
            this.SizeChanged += new System.EventHandler(this.StatisticsGeneratorForm_SizeChanged);
            this.panel1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.panelEdit.ResumeLayout(false);
            this.panelEdit.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.tabPage4.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.TextBox textBoxGeneratedScript;
        private System.Windows.Forms.Button buttonGenerate;
        private System.Windows.Forms.TextBox textBoxHtml;
        private System.Windows.Forms.WebBrowser webBrowser1;
        private System.Windows.Forms.Panel panelEdit;
        private System.Windows.Forms.TextBox textBoxScript;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.TextBox textBoxScriptName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelScriptType;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button buttonSaveScript;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button buttonDeleteScript;
        private System.Windows.Forms.Button button4;
    }
}