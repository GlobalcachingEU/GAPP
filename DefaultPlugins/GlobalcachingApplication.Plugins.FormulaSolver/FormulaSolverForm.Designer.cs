namespace GlobalcachingApplication.Plugins.FormulaSolver
{
    partial class FormulaSolverForm
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormulaSolverForm));
            this.splitContainerFormula = new System.Windows.Forms.SplitContainer();
            this.tbFormula = new System.Windows.Forms.TextBox();
            this.tbSolutions = new System.Windows.Forms.TextBox();
            this.bnInsertFormula = new System.Windows.Forms.Button();
            this.bnInsertWaypoint = new System.Windows.Forms.Button();
            this.bnSolve = new System.Windows.Forms.Button();
            this.bnAsCenter = new System.Windows.Forms.Button();
            this.bnAsWaypoint = new System.Windows.Forms.Button();
            this.formulaSolverFormBindingSource = new System.Windows.Forms.BindingSource(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerFormula)).BeginInit();
            this.splitContainerFormula.Panel1.SuspendLayout();
            this.splitContainerFormula.Panel2.SuspendLayout();
            this.splitContainerFormula.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.formulaSolverFormBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainerFormula
            // 
            this.splitContainerFormula.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainerFormula.Location = new System.Drawing.Point(12, 41);
            this.splitContainerFormula.Name = "splitContainerFormula";
            // 
            // splitContainerFormula.Panel1
            // 
            this.splitContainerFormula.Panel1.Controls.Add(this.tbFormula);
            // 
            // splitContainerFormula.Panel2
            // 
            this.splitContainerFormula.Panel2.Controls.Add(this.tbSolutions);
            this.splitContainerFormula.Size = new System.Drawing.Size(744, 476);
            this.splitContainerFormula.SplitterDistance = 373;
            this.splitContainerFormula.TabIndex = 0;
            // 
            // tbFormula
            // 
            this.tbFormula.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbFormula.Location = new System.Drawing.Point(3, 3);
            this.tbFormula.Multiline = true;
            this.tbFormula.Name = "tbFormula";
            this.tbFormula.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbFormula.Size = new System.Drawing.Size(367, 470);
            this.tbFormula.TabIndex = 0;
            this.tbFormula.WordWrap = false;
            // 
            // tbSolutions
            // 
            this.tbSolutions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbSolutions.Location = new System.Drawing.Point(3, 3);
            this.tbSolutions.Multiline = true;
            this.tbSolutions.Name = "tbSolutions";
            this.tbSolutions.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbSolutions.Size = new System.Drawing.Size(361, 470);
            this.tbSolutions.TabIndex = 0;
            this.tbSolutions.WordWrap = false;
            // 
            // bnInsertFormula
            // 
            this.bnInsertFormula.Location = new System.Drawing.Point(16, 12);
            this.bnInsertFormula.Name = "bnInsertFormula";
            this.bnInsertFormula.Size = new System.Drawing.Size(110, 23);
            this.bnInsertFormula.TabIndex = 1;
            this.bnInsertFormula.Text = "Insert Formula";
            this.bnInsertFormula.UseVisualStyleBackColor = true;
            this.bnInsertFormula.Click += new System.EventHandler(this.bnInsertFormula_Click);
            // 
            // bnInsertWaypoint
            // 
            this.bnInsertWaypoint.Location = new System.Drawing.Point(141, 12);
            this.bnInsertWaypoint.Name = "bnInsertWaypoint";
            this.bnInsertWaypoint.Size = new System.Drawing.Size(110, 23);
            this.bnInsertWaypoint.TabIndex = 2;
            this.bnInsertWaypoint.Text = "Insert Waypoint";
            this.bnInsertWaypoint.UseVisualStyleBackColor = true;
            this.bnInsertWaypoint.Click += new System.EventHandler(this.bnWaypoint_Click);
            // 
            // bnSolve
            // 
            this.bnSolve.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.bnSolve.Location = new System.Drawing.Point(290, 12);
            this.bnSolve.Name = "bnSolve";
            this.bnSolve.Size = new System.Drawing.Size(93, 23);
            this.bnSolve.TabIndex = 3;
            this.bnSolve.Text = "Solve";
            this.bnSolve.UseVisualStyleBackColor = true;
            this.bnSolve.Click += new System.EventHandler(this.bnSolve_Click);
            // 
            // bnAsCenter
            // 
            this.bnAsCenter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bnAsCenter.Location = new System.Drawing.Point(660, 12);
            this.bnAsCenter.Name = "bnAsCenter";
            this.bnAsCenter.Size = new System.Drawing.Size(93, 23);
            this.bnAsCenter.TabIndex = 4;
            this.bnAsCenter.Text = "As Center";
            this.bnAsCenter.UseVisualStyleBackColor = true;
            this.bnAsCenter.Click += new System.EventHandler(this.bnAsCenter_Click);
            // 
            // bnAsWaypoint
            // 
            this.bnAsWaypoint.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bnAsWaypoint.Enabled = false;
            this.bnAsWaypoint.Location = new System.Drawing.Point(561, 12);
            this.bnAsWaypoint.Name = "bnAsWaypoint";
            this.bnAsWaypoint.Size = new System.Drawing.Size(93, 23);
            this.bnAsWaypoint.TabIndex = 5;
            this.bnAsWaypoint.Text = "As Waypoint";
            this.bnAsWaypoint.UseVisualStyleBackColor = true;
            this.bnAsWaypoint.Visible = false;
            this.bnAsWaypoint.Click += new System.EventHandler(this.bnAsWaypoint_Click);
            // 
            // formulaSolverFormBindingSource
            // 
            this.formulaSolverFormBindingSource.DataSource = typeof(GlobalcachingApplication.Plugins.FormulaSolver.FormulaSolverForm);
            // 
            // FormulaSolverForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(768, 529);
            this.Controls.Add(this.bnAsWaypoint);
            this.Controls.Add(this.bnAsCenter);
            this.Controls.Add(this.bnSolve);
            this.Controls.Add(this.bnInsertWaypoint);
            this.Controls.Add(this.bnInsertFormula);
            this.Controls.Add(this.splitContainerFormula);
            this.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.formulaSolverFormBindingSource, "activeTitle", true));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(630, 300);
            this.Name = "FormulaSolverForm";
            this.Text = "FormulaSolverForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormulaSolverForm_FormClosing);
            this.LocationChanged += new System.EventHandler(this.FormulaSolverForm_LocationOrSizeChanged);
            this.SizeChanged += new System.EventHandler(this.FormulaSolverForm_LocationOrSizeChanged);
            this.splitContainerFormula.Panel1.ResumeLayout(false);
            this.splitContainerFormula.Panel1.PerformLayout();
            this.splitContainerFormula.Panel2.ResumeLayout(false);
            this.splitContainerFormula.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerFormula)).EndInit();
            this.splitContainerFormula.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.formulaSolverFormBindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainerFormula;
        private System.Windows.Forms.TextBox tbFormula;
        private System.Windows.Forms.TextBox tbSolutions;
        private System.Windows.Forms.Button bnInsertFormula;
        private System.Windows.Forms.Button bnInsertWaypoint;
        private System.Windows.Forms.Button bnSolve;
        private System.Windows.Forms.Button bnAsCenter;
        private System.Windows.Forms.Button bnAsWaypoint;
        private System.Windows.Forms.BindingSource formulaSolverFormBindingSource;
    }
}