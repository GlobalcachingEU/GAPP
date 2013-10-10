namespace GlobalcachingApplication.Plugins.ICal
{
    partial class AddToCalendarForm
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
            this.buttonAddToCalendar = new System.Windows.Forms.Button();
            this.checkedListBox1 = new System.Windows.Forms.CheckedListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
            this.dateTimePicker2 = new System.Windows.Forms.DateTimePicker();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.labelEventName = new System.Windows.Forms.Label();
            this.buttonAddNextToCalendar = new System.Windows.Forms.Button();
            this.buttonSave = new System.Windows.Forms.Button();
            this.checkBoxOutlook = new System.Windows.Forms.CheckBox();
            this.label13 = new System.Windows.Forms.Label();
            this.checkBoxGoogleAgenda = new System.Windows.Forms.CheckBox();
            this.checkBoxOpenGoogleCalendar = new System.Windows.Forms.CheckBox();
            this.labelICSFilename = new System.Windows.Forms.Label();
            this.labelGoogleExpl = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // buttonAddToCalendar
            // 
            this.buttonAddToCalendar.Location = new System.Drawing.Point(221, 262);
            this.buttonAddToCalendar.Name = "buttonAddToCalendar";
            this.buttonAddToCalendar.Size = new System.Drawing.Size(209, 23);
            this.buttonAddToCalendar.TabIndex = 0;
            this.buttonAddToCalendar.Text = "Add all to calendar";
            this.buttonAddToCalendar.UseVisualStyleBackColor = true;
            this.buttonAddToCalendar.Click += new System.EventHandler(this.buttonAddToCalendar_Click);
            // 
            // checkedListBox1
            // 
            this.checkedListBox1.FormattingEnabled = true;
            this.checkedListBox1.Location = new System.Drawing.Point(12, 12);
            this.checkedListBox1.Name = "checkedListBox1";
            this.checkedListBox1.Size = new System.Drawing.Size(209, 199);
            this.checkedListBox1.TabIndex = 1;
            this.checkedListBox1.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.checkedListBox1_ItemCheck);
            this.checkedListBox1.SelectedIndexChanged += new System.EventHandler(this.checkedListBox1_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(227, 48);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Start";
            // 
            // dateTimePicker1
            // 
            this.dateTimePicker1.CustomFormat = "ddddd, MMMM dd, yyyy HH:mm";
            this.dateTimePicker1.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePicker1.Location = new System.Drawing.Point(344, 42);
            this.dateTimePicker1.Name = "dateTimePicker1";
            this.dateTimePicker1.Size = new System.Drawing.Size(253, 20);
            this.dateTimePicker1.TabIndex = 3;
            // 
            // dateTimePicker2
            // 
            this.dateTimePicker2.CustomFormat = "ddddd, MMMM dd, yyyy HH:mm";
            this.dateTimePicker2.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePicker2.Location = new System.Drawing.Point(344, 68);
            this.dateTimePicker2.Name = "dateTimePicker2";
            this.dateTimePicker2.Size = new System.Drawing.Size(253, 20);
            this.dateTimePicker2.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(227, 74);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(26, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "End";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(328, 74);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(10, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = ":";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(328, 48);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(10, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = ":";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(328, 97);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(10, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = ":";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(227, 97);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(48, 13);
            this.label6.TabIndex = 8;
            this.label6.Text = "Location";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(344, 94);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(253, 20);
            this.textBox1.TabIndex = 10;
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(344, 120);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(253, 20);
            this.textBox2.TabIndex = 13;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(328, 123);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(10, 13);
            this.label7.TabIndex = 12;
            this.label7.Text = ":";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(227, 123);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(50, 13);
            this.label8.TabIndex = 11;
            this.label8.Text = "Summary";
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(344, 146);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(253, 20);
            this.textBox3.TabIndex = 16;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(328, 149);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(10, 13);
            this.label9.TabIndex = 15;
            this.label9.Text = ":";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(227, 149);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(60, 13);
            this.label10.TabIndex = 14;
            this.label10.Text = "Description";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(328, 22);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(10, 13);
            this.label11.TabIndex = 18;
            this.label11.Text = ":";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(227, 22);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(35, 13);
            this.label12.TabIndex = 17;
            this.label12.Text = "Event";
            // 
            // labelEventName
            // 
            this.labelEventName.AutoSize = true;
            this.labelEventName.Location = new System.Drawing.Point(344, 22);
            this.labelEventName.Name = "labelEventName";
            this.labelEventName.Size = new System.Drawing.Size(10, 13);
            this.labelEventName.TabIndex = 19;
            this.labelEventName.Text = "-";
            // 
            // buttonAddNextToCalendar
            // 
            this.buttonAddNextToCalendar.Location = new System.Drawing.Point(221, 233);
            this.buttonAddNextToCalendar.Name = "buttonAddNextToCalendar";
            this.buttonAddNextToCalendar.Size = new System.Drawing.Size(209, 23);
            this.buttonAddNextToCalendar.TabIndex = 20;
            this.buttonAddNextToCalendar.Text = "Add next to calendar";
            this.buttonAddNextToCalendar.UseVisualStyleBackColor = true;
            this.buttonAddNextToCalendar.Click += new System.EventHandler(this.buttonAddNextToCalendar_Click);
            // 
            // buttonSave
            // 
            this.buttonSave.Location = new System.Drawing.Point(344, 172);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(124, 23);
            this.buttonSave.TabIndex = 21;
            this.buttonSave.Text = "Save";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // checkBoxOutlook
            // 
            this.checkBoxOutlook.AutoSize = true;
            this.checkBoxOutlook.Location = new System.Drawing.Point(15, 239);
            this.checkBoxOutlook.Name = "checkBoxOutlook";
            this.checkBoxOutlook.Size = new System.Drawing.Size(63, 17);
            this.checkBoxOutlook.TabIndex = 22;
            this.checkBoxOutlook.Text = "Outlook";
            this.checkBoxOutlook.UseVisualStyleBackColor = true;
            this.checkBoxOutlook.CheckedChanged += new System.EventHandler(this.checkBoxOutlook_CheckedChanged);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(12, 214);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(38, 13);
            this.label13.TabIndex = 23;
            this.label13.Text = "Add to";
            // 
            // checkBoxGoogleAgenda
            // 
            this.checkBoxGoogleAgenda.AutoSize = true;
            this.checkBoxGoogleAgenda.Location = new System.Drawing.Point(15, 263);
            this.checkBoxGoogleAgenda.Name = "checkBoxGoogleAgenda";
            this.checkBoxGoogleAgenda.Size = new System.Drawing.Size(99, 17);
            this.checkBoxGoogleAgenda.TabIndex = 24;
            this.checkBoxGoogleAgenda.Text = "Google agenda";
            this.checkBoxGoogleAgenda.UseVisualStyleBackColor = true;
            this.checkBoxGoogleAgenda.CheckedChanged += new System.EventHandler(this.checkBoxGoogleAgenda_CheckedChanged);
            // 
            // checkBoxOpenGoogleCalendar
            // 
            this.checkBoxOpenGoogleCalendar.AutoSize = true;
            this.checkBoxOpenGoogleCalendar.Location = new System.Drawing.Point(64, 286);
            this.checkBoxOpenGoogleCalendar.Name = "checkBoxOpenGoogleCalendar";
            this.checkBoxOpenGoogleCalendar.Size = new System.Drawing.Size(133, 17);
            this.checkBoxOpenGoogleCalendar.TabIndex = 25;
            this.checkBoxOpenGoogleCalendar.Text = "Open Google calendar";
            this.checkBoxOpenGoogleCalendar.UseVisualStyleBackColor = true;
            this.checkBoxOpenGoogleCalendar.CheckedChanged += new System.EventHandler(this.checkBoxOpenGoogleCalendar_CheckedChanged);
            // 
            // labelICSFilename
            // 
            this.labelICSFilename.AutoSize = true;
            this.labelICSFilename.Location = new System.Drawing.Point(64, 310);
            this.labelICSFilename.Name = "labelICSFilename";
            this.labelICSFilename.Size = new System.Drawing.Size(10, 13);
            this.labelICSFilename.TabIndex = 26;
            this.labelICSFilename.Text = "-";
            // 
            // labelGoogleExpl
            // 
            this.labelGoogleExpl.AutoSize = true;
            this.labelGoogleExpl.Location = new System.Drawing.Point(64, 326);
            this.labelGoogleExpl.Name = "labelGoogleExpl";
            this.labelGoogleExpl.Size = new System.Drawing.Size(10, 13);
            this.labelGoogleExpl.TabIndex = 27;
            this.labelGoogleExpl.Text = "-";
            // 
            // AddToCalendarForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(608, 348);
            this.Controls.Add(this.labelGoogleExpl);
            this.Controls.Add(this.labelICSFilename);
            this.Controls.Add(this.checkBoxOpenGoogleCalendar);
            this.Controls.Add(this.checkBoxGoogleAgenda);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.checkBoxOutlook);
            this.Controls.Add(this.buttonSave);
            this.Controls.Add(this.buttonAddNextToCalendar);
            this.Controls.Add(this.labelEventName);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.textBox3);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.dateTimePicker2);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.dateTimePicker1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.checkedListBox1);
            this.Controls.Add(this.buttonAddToCalendar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AddToCalendarForm";
            this.ShowIcon = false;
            this.Text = "AddToCalendarForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonAddToCalendar;
        private System.Windows.Forms.CheckedListBox checkedListBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DateTimePicker dateTimePicker1;
        private System.Windows.Forms.DateTimePicker dateTimePicker2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label labelEventName;
        private System.Windows.Forms.Button buttonAddNextToCalendar;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.CheckBox checkBoxOutlook;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.CheckBox checkBoxGoogleAgenda;
        private System.Windows.Forms.CheckBox checkBoxOpenGoogleCalendar;
        private System.Windows.Forms.Label labelICSFilename;
        private System.Windows.Forms.Label labelGoogleExpl;
    }
}