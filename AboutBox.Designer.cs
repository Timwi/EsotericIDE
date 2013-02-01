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
            this.components = new System.ComponentModel.Container();
            this.lblVersion = new System.Windows.Forms.Label();
            this.lblCopyright = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.lblProductName = new System.Windows.Forms.Label();
            this.pnlProduct = new System.Windows.Forms.Panel();
            this.lblDescription = new System.Windows.Forms.Label();
            this.lblURL = new System.Windows.Forms.Label();
            this.pnlMain = new System.Windows.Forms.TableLayoutPanel();
            this.lblCredits = new System.Windows.Forms.Label();
            this.ctLayoutMain = new System.Windows.Forms.TableLayoutPanel();
            this.ctLogo = new System.Windows.Forms.PictureBox();
            this.ctLayoutBottom = new System.Windows.Forms.TableLayoutPanel();
            this.ctLayoutBottomLeft = new System.Windows.Forms.FlowLayoutPanel();
            this.ctTimer = new System.Windows.Forms.Timer(this.components);
            this.ctLayoutMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ctLogo)).BeginInit();
            this.ctLayoutBottom.SuspendLayout();
            this.ctLayoutBottomLeft.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblVersion
            // 
            this.lblVersion.AutoSize = true;
            this.lblVersion.Font = new System.Drawing.Font("Candara", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblVersion.Location = new System.Drawing.Point(5, 34);
            this.lblVersion.Margin = new System.Windows.Forms.Padding(5);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(54, 18);
            this.lblVersion.TabIndex = 0;
            this.lblVersion.Tag = "notranslate";
            this.lblVersion.Text = "Version";
            this.lblVersion.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblCopyright
            // 
            this.lblCopyright.AutoSize = true;
            this.lblCopyright.Font = new System.Drawing.Font("Candara", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCopyright.Location = new System.Drawing.Point(5, 90);
            this.lblCopyright.Margin = new System.Windows.Forms.Padding(5);
            this.lblCopyright.Name = "lblCopyright";
            this.lblCopyright.Size = new System.Drawing.Size(68, 18);
            this.lblCopyright.TabIndex = 21;
            this.lblCopyright.Tag = "notranslate";
            this.lblCopyright.Text = "Copyright";
            this.lblCopyright.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Font = new System.Drawing.Font("Candara", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOK.Location = new System.Drawing.Point(808, 162);
            this.btnOK.Margin = new System.Windows.Forms.Padding(5);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 26);
            this.btnOK.TabIndex = 24;
            this.btnOK.Text = "OK";
            // 
            // lblProductName
            // 
            this.lblProductName.AutoSize = true;
            this.lblProductName.Font = new System.Drawing.Font("Candara", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblProductName.Location = new System.Drawing.Point(5, 5);
            this.lblProductName.Margin = new System.Windows.Forms.Padding(5);
            this.lblProductName.Name = "lblProductName";
            this.lblProductName.Size = new System.Drawing.Size(107, 19);
            this.lblProductName.TabIndex = 20;
            this.lblProductName.Tag = "notranslate";
            this.lblProductName.Text = "Product Name";
            this.lblProductName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlProduct
            // 
            this.pnlProduct.AutoSize = true;
            this.pnlProduct.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlProduct.Location = new System.Drawing.Point(148, 78);
            this.pnlProduct.Margin = new System.Windows.Forms.Padding(0);
            this.pnlProduct.Name = "pnlProduct";
            this.pnlProduct.Size = new System.Drawing.Size(0, 0);
            this.pnlProduct.TabIndex = 26;
            this.pnlProduct.Tag = "notranslate";
            // 
            // lblDescription
            // 
            this.lblDescription.AutoSize = true;
            this.lblDescription.Font = new System.Drawing.Font("Candara", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDescription.Location = new System.Drawing.Point(5, 62);
            this.lblDescription.Margin = new System.Windows.Forms.Padding(5);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(79, 18);
            this.lblDescription.TabIndex = 27;
            this.lblDescription.Tag = "notranslate";
            this.lblDescription.Text = "Description";
            this.lblDescription.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblURL
            // 
            this.lblURL.AutoSize = true;
            this.lblURL.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblURL.Font = new System.Drawing.Font("Candara", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblURL.ForeColor = System.Drawing.Color.Blue;
            this.lblURL.Location = new System.Drawing.Point(5, 118);
            this.lblURL.Margin = new System.Windows.Forms.Padding(5);
            this.lblURL.Name = "lblURL";
            this.lblURL.Size = new System.Drawing.Size(240, 18);
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
            this.pnlMain.Location = new System.Drawing.Point(73, 259);
            this.pnlMain.Margin = new System.Windows.Forms.Padding(0);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.RowCount = 3;
            this.pnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.pnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.pnlMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.pnlMain.Size = new System.Drawing.Size(0, 0);
            this.pnlMain.TabIndex = 27;
            this.pnlMain.Tag = "notranslate";
            // 
            // lblCredits
            // 
            this.lblCredits.AutoSize = true;
            this.lblCredits.Font = new System.Drawing.Font("Candara", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCredits.Location = new System.Drawing.Point(5, 146);
            this.lblCredits.Margin = new System.Windows.Forms.Padding(5);
            this.lblCredits.Name = "lblCredits";
            this.lblCredits.Size = new System.Drawing.Size(148, 36);
            this.lblCredits.TabIndex = 26;
            this.lblCredits.Text = "Credits:\r\n    Programming: Timwi";
            // 
            // ctLayoutMain
            // 
            this.ctLayoutMain.AutoSize = true;
            this.ctLayoutMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ctLayoutMain.ColumnCount = 1;
            this.ctLayoutMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.ctLayoutMain.Controls.Add(this.ctLogo, 0, 0);
            this.ctLayoutMain.Controls.Add(this.ctLayoutBottom, 0, 1);
            this.ctLayoutMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ctLayoutMain.Location = new System.Drawing.Point(0, 0);
            this.ctLayoutMain.Margin = new System.Windows.Forms.Padding(0);
            this.ctLayoutMain.Name = "ctLayoutMain";
            this.ctLayoutMain.RowCount = 2;
            this.ctLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.ctLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.ctLayoutMain.Size = new System.Drawing.Size(894, 761);
            this.ctLayoutMain.TabIndex = 28;
            // 
            // ctLogo
            // 
            this.ctLogo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.ctLogo.Location = new System.Drawing.Point(372, 0);
            this.ctLogo.Margin = new System.Windows.Forms.Padding(0);
            this.ctLogo.Name = "ctLogo";
            this.ctLogo.Size = new System.Drawing.Size(150, 150);
            this.ctLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.ctLogo.TabIndex = 12;
            this.ctLogo.TabStop = false;
            this.ctLogo.Tag = "notranslate";
            // 
            // ctLayoutBottom
            // 
            this.ctLayoutBottom.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.ctLayoutBottom.AutoSize = true;
            this.ctLayoutBottom.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ctLayoutBottom.ColumnCount = 2;
            this.ctLayoutBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.ctLayoutBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.ctLayoutBottom.Controls.Add(this.ctLayoutBottomLeft, 0, 0);
            this.ctLayoutBottom.Controls.Add(this.btnOK, 1, 0);
            this.ctLayoutBottom.Location = new System.Drawing.Point(3, 359);
            this.ctLayoutBottom.Name = "ctLayoutBottom";
            this.ctLayoutBottom.RowCount = 1;
            this.ctLayoutBottom.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.ctLayoutBottom.Size = new System.Drawing.Size(888, 193);
            this.ctLayoutBottom.TabIndex = 28;
            // 
            // ctLayoutBottomLeft
            // 
            this.ctLayoutBottomLeft.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ctLayoutBottomLeft.AutoSize = true;
            this.ctLayoutBottomLeft.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ctLayoutBottomLeft.Controls.Add(this.lblProductName);
            this.ctLayoutBottomLeft.Controls.Add(this.lblVersion);
            this.ctLayoutBottomLeft.Controls.Add(this.lblDescription);
            this.ctLayoutBottomLeft.Controls.Add(this.lblCopyright);
            this.ctLayoutBottomLeft.Controls.Add(this.lblURL);
            this.ctLayoutBottomLeft.Controls.Add(this.lblCredits);
            this.ctLayoutBottomLeft.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.ctLayoutBottomLeft.Font = new System.Drawing.Font("Candara", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ctLayoutBottomLeft.Location = new System.Drawing.Point(3, 3);
            this.ctLayoutBottomLeft.Name = "ctLayoutBottomLeft";
            this.ctLayoutBottomLeft.Size = new System.Drawing.Size(615, 187);
            this.ctLayoutBottomLeft.TabIndex = 29;
            // 
            // ctTimer
            // 
            this.ctTimer.Enabled = true;
            this.ctTimer.Tick += new System.EventHandler(this.ctTimer_Tick);
            // 
            // AboutBox
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.CancelButton = this.btnOK;
            this.ClientSize = new System.Drawing.Size(894, 761);
            this.Controls.Add(this.ctLayoutMain);
            this.Controls.Add(this.pnlProduct);
            this.Controls.Add(this.pnlMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutBox";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "About Esoteric IDE";
            this.ctLayoutMain.ResumeLayout(false);
            this.ctLayoutMain.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ctLogo)).EndInit();
            this.ctLayoutBottom.ResumeLayout(false);
            this.ctLayoutBottom.PerformLayout();
            this.ctLayoutBottomLeft.ResumeLayout(false);
            this.ctLayoutBottomLeft.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.Label lblCopyright;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Label lblProductName;
        private System.Windows.Forms.Panel pnlProduct;
        private System.Windows.Forms.Label lblURL;
        private System.Windows.Forms.TableLayoutPanel pnlMain;
        private System.Windows.Forms.Label lblCredits;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.PictureBox ctLogo;
        private System.Windows.Forms.TableLayoutPanel ctLayoutMain;
        private System.Windows.Forms.FlowLayoutPanel ctLayoutBottomLeft;
        private System.Windows.Forms.TableLayoutPanel ctLayoutBottom;
        private System.Windows.Forms.Timer ctTimer;
    }
}
