using System.ComponentModel;

namespace ComPort.Scanner
{
    partial class FormAbout
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormAbout));
            this.lblName = new System.Windows.Forms.Label();
            this.btClose = new System.Windows.Forms.Button();
            this.btCoffee = new System.Windows.Forms.Button();
            this.lblVersion = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblName
            // 
            this.lblName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblName.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.lblName.Location = new System.Drawing.Point(16, 7);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(168, 23);
            this.lblName.TabIndex = 0;
            this.lblName.Text = "Com Ports Monitor";
            this.lblName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btClose
            // 
            this.btClose.Location = new System.Drawing.Point(63, 102);
            this.btClose.Name = "btClose";
            this.btClose.Size = new System.Drawing.Size(75, 23);
            this.btClose.TabIndex = 3;
            this.btClose.Text = "Close";
            this.btClose.UseVisualStyleBackColor = true;
            this.btClose.Click += new System.EventHandler(this.OnCloseClick);
            // 
            // btCoffee
            // 
            this.btCoffee.Image = ((System.Drawing.Image)(resources.GetObject("btCoffee.Image")));
            this.btCoffee.Location = new System.Drawing.Point(16, 56);
            this.btCoffee.Name = "btCoffee";
            this.btCoffee.Size = new System.Drawing.Size(168, 40);
            this.btCoffee.TabIndex = 2;
            this.btCoffee.UseVisualStyleBackColor = true;
            this.btCoffee.Click += new System.EventHandler(this.OnCoffeeClick);
            // 
            // lblVersion
            // 
            this.lblVersion.ForeColor = System.Drawing.SystemColors.Control;
            this.lblVersion.Location = new System.Drawing.Point(16, 30);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(168, 23);
            this.lblVersion.TabIndex = 1;
            this.lblVersion.Text = "Version: ";
            this.lblVersion.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // FormAbout
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.MidnightBlue;
            this.ClientSize = new System.Drawing.Size(210, 137);
            this.Controls.Add(this.lblVersion);
            this.Controls.Add(this.btCoffee);
            this.Controls.Add(this.btClose);
            this.Controls.Add(this.lblName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormAbout";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "About Com Ports";
            this.Load += new System.EventHandler(this.OnFormLoad);
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.Label lblVersion;

        private System.Windows.Forms.Button btCoffee;

        private System.Windows.Forms.Button btClose;

        private System.Windows.Forms.Label lblName;

        #endregion
    }
}