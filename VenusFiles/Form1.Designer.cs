namespace VenusFiles
{
    partial class Form1
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
            this.components = new System.ComponentModel.Container();
            this.btnCreateVenus = new System.Windows.Forms.Button();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.tmrStartVenus = new System.Windows.Forms.Timer(this.components);
            this.tmrPause = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // btnCreateVenus
            // 
            this.btnCreateVenus.Enabled = false;
            this.btnCreateVenus.Location = new System.Drawing.Point(12, 120);
            this.btnCreateVenus.Name = "btnCreateVenus";
            this.btnCreateVenus.Size = new System.Drawing.Size(172, 54);
            this.btnCreateVenus.TabIndex = 0;
            this.btnCreateVenus.Text = "Create Venus";
            this.btnCreateVenus.UseVisualStyleBackColor = true;
            this.btnCreateVenus.Click += new System.EventHandler(this.btnCreateVenus_Click);
            // 
            // listBox1
            // 
            this.listBox1.FormattingEnabled = true;
            this.listBox1.Location = new System.Drawing.Point(12, 12);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(172, 95);
            this.listBox1.TabIndex = 1;
            // 
            // tmrStartVenus
            // 
            this.tmrStartVenus.Interval = 1000;
            this.tmrStartVenus.Tick += new System.EventHandler(this.tmrStartVenus_Tick);
            // 
            // tmrPause
            // 
            this.tmrPause.Tick += new System.EventHandler(this.tmrPause_Tick);
            // 
            // tftp1
            // 
            // 
            // http1
            // 
            // 
            // ftp1
            // 
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(196, 186);
            this.Controls.Add(this.listBox1);
            this.Controls.Add(this.btnCreateVenus);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnCreateVenus;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Timer tmrStartVenus;
        private System.Windows.Forms.Timer tmrPause;
    }
}

