namespace GlobalcachingApplication.Utils.BasePlugin
{
    partial class BaseUIChildWindowForm
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
            this.SuspendLayout();
            // 
            // BaseUIChildWindowForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(379, 325);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "BaseUIChildWindowForm";
            this.Text = "BaseUIChildWindowForm";
            this.Activated += new System.EventHandler(this.BaseUIChildWindowForm_Activated);
            this.Deactivate += new System.EventHandler(this.BaseUIChildWindowForm_Deactivate);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.BaseUIChildWindowForm_FormClosed);
            this.LocationChanged += new System.EventHandler(this.BaseUIChildWindowForm_LocationChanged);
            this.VisibleChanged += new System.EventHandler(this.BaseUIChildWindowForm_VisibleChanged);
            this.ResumeLayout(false);

        }

        #endregion
    }
}