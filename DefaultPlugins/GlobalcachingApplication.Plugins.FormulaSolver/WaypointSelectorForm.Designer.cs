namespace GlobalcachingApplication.Plugins.FormulaSolver
{
    partial class WaypointSelectorForm
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
            this.lbWaypoints = new System.Windows.Forms.ListBox();
            this.bnInsert = new System.Windows.Forms.Button();
            this.bnCancel = new System.Windows.Forms.Button();
            this.lblWaypoint = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lbWaypoints
            // 
            this.lbWaypoints.FormattingEnabled = true;
            this.lbWaypoints.Location = new System.Drawing.Point(12, 25);
            this.lbWaypoints.Name = "lbWaypoints";
            this.lbWaypoints.Size = new System.Drawing.Size(260, 186);
            this.lbWaypoints.TabIndex = 0;
            this.lbWaypoints.SelectedIndexChanged += new System.EventHandler(this.lbWaypoints_SelectedIndexChanged);
            this.lbWaypoints.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lbWaypoints_MouseDoubleClick);
            // 
            // bnInsert
            // 
            this.bnInsert.Location = new System.Drawing.Point(116, 226);
            this.bnInsert.Name = "bnInsert";
            this.bnInsert.Size = new System.Drawing.Size(75, 23);
            this.bnInsert.TabIndex = 1;
            this.bnInsert.Text = "Insert";
            this.bnInsert.UseVisualStyleBackColor = true;
            this.bnInsert.Click += new System.EventHandler(this.bnInsert_Click);
            // 
            // bnCancel
            // 
            this.bnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.bnCancel.Location = new System.Drawing.Point(197, 226);
            this.bnCancel.Name = "bnCancel";
            this.bnCancel.Size = new System.Drawing.Size(75, 23);
            this.bnCancel.TabIndex = 2;
            this.bnCancel.Text = "Cancel";
            this.bnCancel.UseVisualStyleBackColor = true;
            this.bnCancel.Click += new System.EventHandler(this.bnCancel_Click);
            // 
            // lblWaypoint
            // 
            this.lblWaypoint.AutoSize = true;
            this.lblWaypoint.Location = new System.Drawing.Point(9, 9);
            this.lblWaypoint.Name = "lblWaypoint";
            this.lblWaypoint.Size = new System.Drawing.Size(52, 13);
            this.lblWaypoint.TabIndex = 3;
            this.lblWaypoint.Text = "Waypoint";
            // 
            // WaypointSelectorForm
            // 
            this.AcceptButton = this.bnInsert;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.bnCancel;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.lblWaypoint);
            this.Controls.Add(this.bnCancel);
            this.Controls.Add(this.bnInsert);
            this.Controls.Add(this.lbWaypoints);
            this.Name = "WaypointSelectorForm";
            this.Text = "Select Waypoint";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox lbWaypoints;
        private System.Windows.Forms.Button bnInsert;
        private System.Windows.Forms.Button bnCancel;
        private System.Windows.Forms.Label lblWaypoint;
    }
}