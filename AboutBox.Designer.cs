namespace EsotericIDE
{
    partial class AboutBox
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
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
            this.ctLogo = new System.Windows.Forms.PictureBox();
            this.lblVersion = new System.Windows.Forms.Label();
            this.lblCopyright = new System.Windows.Forms.Label();
            this.lblCompanyName = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.lblProductName = new System.Windows.Forms.Label();
            this.pnlProduct = new System.Windows.Forms.Panel();
            this.lblURL = new System.Windows.Forms.Label();
            this.pnlMain = new System.Windows.Forms.TableLayoutPanel();
            this.lblCredits = new System.Windows.Forms.Label();
            this.lblDescription = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize) (this.ctLogo)).BeginInit();
            this.pnlProduct.SuspendLayout();
            this.pnlMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // ctLogo
            // 
            this.ctLogo.Location = new System.Drawing.Point(10, 10);
            this.ctLogo.Margin = new System.Windows.Forms.Padding(0);
            this.ctLogo.Name = "ctLogo";
            this.ctLogo.Size = new System.Drawing.Size(150, 150);
            this.ctLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.ctLogo.TabIndex = 12;
            this.ctLogo.TabStop = false;
            this.ctLogo.Tag = "notranslate";
            // 
            // lblVersion
            // 
            this.lblVersion.AutoSize = true;
            this.lblVersion.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (204)));
            this.lblVersion.Location = new System.Drawing.Point(-3, 18);
            this.lblVersion.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(56, 17);
            this.lblVersion.TabIndex = 0;
            this.lblVersion.Tag = "notranslate";
            this.lblVersion.Text = "Version";
            this.lblVersion.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblCopyright
            // 
            this.lblCopyright.AutoSize = true;
            this.lblCopyright.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (204)));
            this.lblCopyright.Location = new System.Drawing.Point(-3, 70);
            this.lblCopyright.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
            this.lblCopyright.Name = "lblCopyright";
            this.lblCopyright.Size = new System.Drawing.Size(70, 17);
            this.lblCopyright.TabIndex = 21;
            this.lblCopyright.Tag = "notranslate";
            this.lblCopyright.Text = "Copyright";
            this.lblCopyright.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblCompanyName
            // 
            this.lblCompanyName.AutoSize = true;
            this.lblCompanyName.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (204)));
            this.lblCompanyName.Location = new System.Drawing.Point(10, 166);
            this.lblCompanyName.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
            this.lblCompanyName.MaximumSize = new System.Drawing.Size(0, 17);
            this.lblCompanyName.Name = "lblCompanyName";
            this.lblCompanyName.Size = new System.Drawing.Size(114, 17);
            this.lblCompanyName.TabIndex = 22;
            this.lblCompanyName.Tag = "notranslate";
            this.lblCompanyName.Text = "Company Name";
            this.lblCompanyName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblCompanyName.Visible = false;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (204)));
            this.btnOK.Location = new System.Drawing.Point(0, 159);
            this.btnOK.Margin = new System.Windows.Forms.Padding(0);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(208, 26);
            this.btnOK.TabIndex = 24;
            this.btnOK.Text = "OK";
            // 
            // lblProductName
            // 
            this.lblProductName.AutoSize = true;
            this.lblProductName.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte) (204)));
            this.lblProductName.Location = new System.Drawing.Point(-3, 0);
            this.lblProductName.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
            this.lblProductName.Name = "lblProductName";
            this.lblProductName.Size = new System.Drawing.Size(108, 18);
            this.lblProductName.TabIndex = 20;
            this.lblProductName.Tag = "notranslate";
            this.lblProductName.Text = "Product Name";
            this.lblProductName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlProduct
            // 
            this.pnlProduct.AutoSize = true;
            this.pnlProduct.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlProduct.Controls.Add(this.lblDescription);
            this.pnlProduct.Controls.Add(this.lblURL);
            this.pnlProduct.Controls.Add(this.lblProductName);
            this.pnlProduct.Controls.Add(this.lblVersion);
            this.pnlProduct.Controls.Add(this.lblCopyright);
            this.pnlProduct.Location = new System.Drawing.Point(0, 0);
            this.pnlProduct.Margin = new System.Windows.Forms.Padding(0);
            this.pnlProduct.Name = "pnlProduct";
            this.pnlProduct.Size = new System.Drawing.Size(203, 102);
            this.pnlProduct.TabIndex = 26;
            this.pnlProduct.Tag = "notranslate";
            // 
            // lblURL
            // 
            this.lblURL.AutoSize = true;
            this.lblURL.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblURL.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte) (204)));
            this.lblURL.ForeColor = System.Drawing.Color.Blue;
            this.lblURL.Location = new System.Drawing.Point(-3, 87);
            this.lblURL.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
            this.lblURL.Name = "lblURL";
            this.lblURL.Size = new System.Drawing.Size(203, 15);
            this.lblURL.TabIndex = 26;
            this.lblURL.Tag = "notranslate";
            this.lblURL.Text = "http://www.cutebits.com/EsotericIDE";
            this.lblURL.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblURL.Click += new System.EventHandler(this.clickUrl);
            // 
            // pnlMain
            // 
            this.pnlMain.AutoSize = true;
            this.pnlMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlMain.ColumnCount = 1;
            this.pnlMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.pnlMain.Controls.Add(this.lblCredits, 0, 1);
            this.pnlMain.Controls.Add(this.pnlProduct, 0, 0);
            this.pnlMain.Controls.Add(this.btnOK, 0, 2);
            this.pnlMain.Location = new System.Drawing.Point(163, 10);
            this.pnlMain.Margin = new System.Windows.Forms.Padding(0);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.RowCount = 3;
            this.pnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.pnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.pnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.pnlMain.Size = new System.Drawing.Size(208, 185);
            this.pnlMain.TabIndex = 27;
            this.pnlMain.Tag = "notranslate";
            // 
            // lblCredits
            // 
            this.lblCredits.AutoSize = true;
            this.lblCredits.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (204)));
            this.lblCredits.Location = new System.Drawing.Point(0, 112);
            this.lblCredits.Margin = new System.Windows.Forms.Padding(0, 10, 0, 15);
            this.lblCredits.Name = "lblCredits";
            this.lblCredits.Size = new System.Drawing.Size(142, 32);
            this.lblCredits.TabIndex = 26;
            this.lblCredits.Text = "Credits:\r\n    Programming: Timwi";
            // 
            // lblDescription
            // 
            this.lblDescription.AutoSize = true;
            this.lblDescription.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (204)));
            this.lblDescription.Location = new System.Drawing.Point(-3, 44);
            this.lblDescription.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(82, 17);
            this.lblDescription.TabIndex = 27;
            this.lblDescription.Tag = "notranslate";
            this.lblDescription.Text = "Description";
            this.lblDescription.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // AboutBox
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.CancelButton = this.btnOK;
            this.ClientSize = new System.Drawing.Size(431, 284);
            this.Controls.Add(this.pnlMain);
            this.Controls.Add(this.lblCompanyName);
            this.Controls.Add(this.ctLogo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutBox";
            this.Padding = new System.Windows.Forms.Padding(10);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "About Esoteric IDE";
            ((System.ComponentModel.ISupportInitialize) (this.ctLogo)).EndInit();
            this.pnlProduct.ResumeLayout(false);
            this.pnlProduct.PerformLayout();
            this.pnlMain.ResumeLayout(false);
            this.pnlMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox ctLogo;
        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.Label lblCopyright;
        private System.Windows.Forms.Label lblCompanyName;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Label lblProductName;
        private System.Windows.Forms.Panel pnlProduct;
        private System.Windows.Forms.Label lblURL;
        private System.Windows.Forms.TableLayoutPanel pnlMain;
        private System.Windows.Forms.Label lblCredits;
        private System.Windows.Forms.Label lblDescription;
    }
}
