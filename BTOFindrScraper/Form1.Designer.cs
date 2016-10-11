namespace BTOFindrScraper
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
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.btnProjects = new System.Windows.Forms.Button();
            this.btnBlocks = new System.Windows.Forms.Button();
            this.btnUnits = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // webBrowser1
            // 
            this.webBrowser1.Location = new System.Drawing.Point(12, 12);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(461, 205);
            this.webBrowser1.TabIndex = 0;
            this.webBrowser1.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.webBrowser1_DocumentCompleted);
            // 
            // btnProjects
            // 
            this.btnProjects.Location = new System.Drawing.Point(479, 12);
            this.btnProjects.Name = "btnProjects";
            this.btnProjects.Size = new System.Drawing.Size(111, 23);
            this.btnProjects.TabIndex = 1;
            this.btnProjects.Text = "Scrape Projects";
            this.btnProjects.UseVisualStyleBackColor = true;
            this.btnProjects.Click += new System.EventHandler(this.btnProjects_Click);
            // 
            // btnBlocks
            // 
            this.btnBlocks.Location = new System.Drawing.Point(480, 42);
            this.btnBlocks.Name = "btnBlocks";
            this.btnBlocks.Size = new System.Drawing.Size(110, 23);
            this.btnBlocks.TabIndex = 2;
            this.btnBlocks.Text = "Scrape Blocks";
            this.btnBlocks.UseVisualStyleBackColor = true;
            this.btnBlocks.Click += new System.EventHandler(this.btnBlocks_Click);
            // 
            // btnUnits
            // 
            this.btnUnits.Location = new System.Drawing.Point(480, 72);
            this.btnUnits.Name = "btnUnits";
            this.btnUnits.Size = new System.Drawing.Size(110, 23);
            this.btnUnits.TabIndex = 3;
            this.btnUnits.Text = "Scrape Units";
            this.btnUnits.UseVisualStyleBackColor = true;
            this.btnUnits.Click += new System.EventHandler(this.btnUnits_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(13, 224);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(37, 13);
            this.lblStatus.TabIndex = 5;
            this.lblStatus.Text = "Status";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(603, 249);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnUnits);
            this.Controls.Add(this.btnBlocks);
            this.Controls.Add(this.btnProjects);
            this.Controls.Add(this.webBrowser1);
            this.Name = "Form1";
            this.Text = "BTOFindr Scraper";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.WebBrowser webBrowser1;
        private System.Windows.Forms.Button btnProjects;
        private System.Windows.Forms.Button btnBlocks;
        private System.Windows.Forms.Button btnUnits;
        private System.Windows.Forms.Label lblStatus;
    }
}

