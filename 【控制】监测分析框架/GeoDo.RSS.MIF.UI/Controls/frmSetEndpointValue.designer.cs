﻿namespace GeoDo.RSS.MIF.UI
{
    partial class frmSetEndpointValue
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.doubleTextBox2 = new GeoDo.RSS.MIF.UI.DoubleTextBox();
            this.doubleTextBox1 = new GeoDo.RSS.MIF.UI.DoubleTextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.doubleTextBox3 = new GeoDo.RSS.MIF.UI.DoubleTextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(2, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "最小值";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(126, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "最大值";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(126, 38);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(55, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "确定";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // doubleTextBox2
            // 
            this.doubleTextBox2.DefaultValue = 0D;
            this.doubleTextBox2.FineTuning = 1D;
            this.doubleTextBox2.Location = new System.Drawing.Point(173, 9);
            this.doubleTextBox2.Name = "doubleTextBox2";
            this.doubleTextBox2.Size = new System.Drawing.Size(70, 21);
            this.doubleTextBox2.TabIndex = 1;
            this.doubleTextBox2.Text = "0";
            this.doubleTextBox2.Value = 0D;
            // 
            // doubleTextBox1
            // 
            this.doubleTextBox1.DefaultValue = 0D;
            this.doubleTextBox1.FineTuning = 1D;
            this.doubleTextBox1.Location = new System.Drawing.Point(50, 10);
            this.doubleTextBox1.Name = "doubleTextBox1";
            this.doubleTextBox1.Size = new System.Drawing.Size(70, 21);
            this.doubleTextBox1.TabIndex = 0;
            this.doubleTextBox1.Text = "0";
            this.doubleTextBox1.Value = 0D;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(2, 46);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 12);
            this.label3.TabIndex = 0;
            this.label3.Text = "微调值";
            // 
            // doubleTextBox3
            // 
            this.doubleTextBox3.DefaultValue = 0D;
            this.doubleTextBox3.FineTuning = 1D;
            this.doubleTextBox3.Location = new System.Drawing.Point(50, 40);
            this.doubleTextBox3.Name = "doubleTextBox3";
            this.doubleTextBox3.Size = new System.Drawing.Size(70, 21);
            this.doubleTextBox3.TabIndex = 0;
            this.doubleTextBox3.Text = "1";
            this.doubleTextBox3.Value = 1D;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(188, 38);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(55, 23);
            this.button2.TabIndex = 2;
            this.button2.Text = "取消";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // frmSetEndpointValue
            // 
            this.AcceptButton = this.button1;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(248, 70);
            this.Controls.Add(this.doubleTextBox2);
            this.Controls.Add(this.doubleTextBox3);
            this.Controls.Add(this.doubleTextBox1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "frmSetEndpointValue";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "设置端值...";
            this.Load += new System.EventHandler(this.frmSetEndpointValue_Load_1);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button1;
        private DoubleTextBox doubleTextBox1;
        private DoubleTextBox doubleTextBox2;
        private System.Windows.Forms.Label label3;
        private DoubleTextBox doubleTextBox3;
        private System.Windows.Forms.Button button2;
    }
}