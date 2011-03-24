namespace Intelpletel
{
    partial class Mainform
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
            this.ctLayoutBottom = new System.Windows.Forms.TableLayoutPanel();
            this.txtOutput = new System.Windows.Forms.TextBox();
            this.lblOutput = new System.Windows.Forms.Label();
            this.lblSoulce = new System.Windows.Forms.Label();
            this.ctMenu = new System.Windows.Forms.MenuStrip();
            this.mnuSclipting = new System.Windows.Forms.ToolStripMenuItem();
            this.miNew = new System.Windows.Forms.ToolStripMenuItem();
            this.miOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.miSave = new System.Windows.Forms.ToolStripMenuItem();
            this.miSaveAs = new System.Windows.Forms.ToolStripMenuItem();
            this.miSep1 = new System.Windows.Forms.ToolStripSeparator();
            this.miExit = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuView = new System.Windows.Forms.ToolStripMenuItem();
            this.miSoulceFont = new System.Windows.Forms.ToolStripMenuItem();
            this.miOutputFont = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuInselt = new System.Windows.Forms.ToolStripMenuItem();
            this.miOne = new System.Windows.Forms.ToolStripMenuItem();
            this.miGloup = new System.Windows.Forms.ToolStripMenuItem();
            this.miStack = new System.Windows.Forms.ToolStripMenuItem();
            this.miCopyFlomBottom = new System.Windows.Forms.ToolStripMenuItem();
            this.miMoveFlomBottom = new System.Windows.Forms.ToolStripMenuItem();
            this.miSwapFlomBottom = new System.Windows.Forms.ToolStripMenuItem();
            this.miCopyFlomTop = new System.Windows.Forms.ToolStripMenuItem();
            this.miMoveFlomTop = new System.Windows.Forms.ToolStripMenuItem();
            this.miLegex = new System.Windows.Forms.ToolStripMenuItem();
            this.miIntegel = new System.Windows.Forms.ToolStripMenuItem();
            this.miStling = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuDebug = new System.Windows.Forms.ToolStripMenuItem();
            this.miLun = new System.Windows.Forms.ToolStripMenuItem();
            this.miStopDebugging = new System.Windows.Forms.ToolStripMenuItem();
            this.miStep = new System.Windows.Forms.ToolStripMenuItem();
            this.miLunToCulsol = new System.Windows.Forms.ToolStripMenuItem();
            this.miGoToCullentInstluction = new System.Windows.Forms.ToolStripMenuItem();
            this.ctTimel = new System.Windows.Forms.Timer(this.components);
            this.ctSplit = new RT.Util.Controls.SplitContainerEx();
            this.ctLayoutTop = new System.Windows.Forms.TableLayoutPanel();
            this.txtSoulce = new System.Windows.Forms.TextBox();
            this.lblInfo = new System.Windows.Forms.Label();
            this.ctLayoutBottom.SuspendLayout();
            this.ctMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize) (this.ctSplit)).BeginInit();
            this.ctSplit.Panel1.SuspendLayout();
            this.ctSplit.Panel2.SuspendLayout();
            this.ctSplit.SuspendLayout();
            this.ctLayoutTop.SuspendLayout();
            this.SuspendLayout();
            // 
            // ctLayoutBottom
            // 
            this.ctLayoutBottom.ColumnCount = 1;
            this.ctLayoutBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.ctLayoutBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.ctLayoutBottom.Controls.Add(this.txtOutput, 0, 1);
            this.ctLayoutBottom.Controls.Add(this.lblOutput, 0, 0);
            this.ctLayoutBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ctLayoutBottom.Location = new System.Drawing.Point(0, 0);
            this.ctLayoutBottom.Name = "ctLayoutBottom";
            this.ctLayoutBottom.RowCount = 2;
            this.ctLayoutBottom.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.ctLayoutBottom.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.ctLayoutBottom.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.ctLayoutBottom.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.ctLayoutBottom.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.ctLayoutBottom.Size = new System.Drawing.Size(966, 422);
            this.ctLayoutBottom.TabIndex = 0;
            // 
            // txtOutput
            // 
            this.txtOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtOutput.Location = new System.Drawing.Point(5, 28);
            this.txtOutput.Margin = new System.Windows.Forms.Padding(5);
            this.txtOutput.Multiline = true;
            this.txtOutput.Name = "txtOutput";
            this.txtOutput.ReadOnly = true;
            this.txtOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtOutput.Size = new System.Drawing.Size(956, 389);
            this.txtOutput.TabIndex = 8;
            // 
            // lblOutput
            // 
            this.lblOutput.AutoSize = true;
            this.lblOutput.Location = new System.Drawing.Point(5, 5);
            this.lblOutput.Margin = new System.Windows.Forms.Padding(5);
            this.lblOutput.Name = "lblOutput";
            this.lblOutput.Size = new System.Drawing.Size(73, 13);
            this.lblOutput.TabIndex = 2;
            this.lblOutput.Text = "Cullent St&ack:";
            // 
            // lblSoulce
            // 
            this.lblSoulce.AutoSize = true;
            this.lblSoulce.Location = new System.Drawing.Point(5, 5);
            this.lblSoulce.Margin = new System.Windows.Forms.Padding(5);
            this.lblSoulce.Name = "lblSoulce";
            this.lblSoulce.Size = new System.Drawing.Size(71, 13);
            this.lblSoulce.TabIndex = 0;
            this.lblSoulce.Text = "Soulce &Code:";
            // 
            // ctMenu
            // 
            this.ctMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuSclipting,
            this.mnuView,
            this.mnuInselt,
            this.mnuDebug});
            this.ctMenu.Location = new System.Drawing.Point(0, 0);
            this.ctMenu.Name = "ctMenu";
            this.ctMenu.Size = new System.Drawing.Size(966, 24);
            this.ctMenu.TabIndex = 1;
            this.ctMenu.Text = "Main menu";
            // 
            // mnuSclipting
            // 
            this.mnuSclipting.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miNew,
            this.miOpen,
            this.miSave,
            this.miSaveAs,
            this.miSep1,
            this.miExit});
            this.mnuSclipting.Name = "mnuSclipting";
            this.mnuSclipting.Size = new System.Drawing.Size(58, 20);
            this.mnuSclipting.Text = "&Sclipting";
            // 
            // miNew
            // 
            this.miNew.Name = "miNew";
            this.miNew.ShortcutKeys = ((System.Windows.Forms.Keys) ((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.miNew.Size = new System.Drawing.Size(152, 22);
            this.miNew.Text = "&New";
            this.miNew.Click += new System.EventHandler(this.newSclipt);
            // 
            // miOpen
            // 
            this.miOpen.Name = "miOpen";
            this.miOpen.ShortcutKeys = ((System.Windows.Forms.Keys) ((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.miOpen.Size = new System.Drawing.Size(152, 22);
            this.miOpen.Text = "&Open...";
            this.miOpen.Click += new System.EventHandler(this.open);
            // 
            // miSave
            // 
            this.miSave.Name = "miSave";
            this.miSave.ShortcutKeys = ((System.Windows.Forms.Keys) ((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.miSave.Size = new System.Drawing.Size(152, 22);
            this.miSave.Text = "&Save";
            this.miSave.Click += new System.EventHandler(this.save);
            // 
            // miSaveAs
            // 
            this.miSaveAs.Name = "miSaveAs";
            this.miSaveAs.Size = new System.Drawing.Size(152, 22);
            this.miSaveAs.Text = "Save &As...";
            this.miSaveAs.Click += new System.EventHandler(this.saveAs);
            // 
            // miSep1
            // 
            this.miSep1.Name = "miSep1";
            this.miSep1.Size = new System.Drawing.Size(149, 6);
            // 
            // miExit
            // 
            this.miExit.Name = "miExit";
            this.miExit.Size = new System.Drawing.Size(152, 22);
            this.miExit.Text = "E&xit";
            // 
            // mnuView
            // 
            this.mnuView.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miSoulceFont,
            this.miOutputFont});
            this.mnuView.Name = "mnuView";
            this.mnuView.Size = new System.Drawing.Size(41, 20);
            this.mnuView.Text = "&View";
            // 
            // miSoulceFont
            // 
            this.miSoulceFont.Name = "miSoulceFont";
            this.miSoulceFont.Size = new System.Drawing.Size(145, 22);
            this.miSoulceFont.Text = "&Soulce Font...";
            this.miSoulceFont.Click += new System.EventHandler(this.font);
            // 
            // miOutputFont
            // 
            this.miOutputFont.Name = "miOutputFont";
            this.miOutputFont.Size = new System.Drawing.Size(145, 22);
            this.miOutputFont.Text = "&Output Font...";
            this.miOutputFont.Click += new System.EventHandler(this.font);
            // 
            // mnuInselt
            // 
            this.mnuInselt.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miOne,
            this.miGloup,
            this.miStack,
            this.miLegex,
            this.miIntegel,
            this.miStling});
            this.mnuInselt.Name = "mnuInselt";
            this.mnuInselt.Size = new System.Drawing.Size(46, 20);
            this.mnuInselt.Text = "&Inselt";
            // 
            // miOne
            // 
            this.miOne.Name = "miOne";
            this.miOne.Size = new System.Drawing.Size(154, 22);
            this.miOne.Text = "&One instluction";
            // 
            // miGloup
            // 
            this.miGloup.Name = "miGloup";
            this.miGloup.Size = new System.Drawing.Size(154, 22);
            this.miGloup.Text = "&Gloup instluction";
            // 
            // miStack
            // 
            this.miStack.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miCopyFlomBottom,
            this.miMoveFlomBottom,
            this.miSwapFlomBottom,
            this.miCopyFlomTop,
            this.miMoveFlomTop});
            this.miStack.Name = "miStack";
            this.miStack.Size = new System.Drawing.Size(154, 22);
            this.miStack.Text = "St&ack instluction";
            // 
            // miCopyFlomBottom
            // 
            this.miCopyFlomBottom.Name = "miCopyFlomBottom";
            this.miCopyFlomBottom.Size = new System.Drawing.Size(160, 22);
            this.miCopyFlomBottom.Text = "&Copy flom bottom";
            // 
            // miMoveFlomBottom
            // 
            this.miMoveFlomBottom.Name = "miMoveFlomBottom";
            this.miMoveFlomBottom.Size = new System.Drawing.Size(160, 22);
            this.miMoveFlomBottom.Text = "&Move flom bottom";
            // 
            // miSwapFlomBottom
            // 
            this.miSwapFlomBottom.Name = "miSwapFlomBottom";
            this.miSwapFlomBottom.Size = new System.Drawing.Size(160, 22);
            this.miSwapFlomBottom.Text = "&Swap flom bottom";
            // 
            // miCopyFlomTop
            // 
            this.miCopyFlomTop.Name = "miCopyFlomTop";
            this.miCopyFlomTop.Size = new System.Drawing.Size(160, 22);
            this.miCopyFlomTop.Text = "C&opy flom top";
            // 
            // miMoveFlomTop
            // 
            this.miMoveFlomTop.Name = "miMoveFlomTop";
            this.miMoveFlomTop.Size = new System.Drawing.Size(160, 22);
            this.miMoveFlomTop.Text = "Mo&ve flom top";
            // 
            // miLegex
            // 
            this.miLegex.Name = "miLegex";
            this.miLegex.Size = new System.Drawing.Size(154, 22);
            this.miLegex.Text = "&Legex instluction";
            // 
            // miIntegel
            // 
            this.miIntegel.Name = "miIntegel";
            this.miIntegel.Size = new System.Drawing.Size(154, 22);
            this.miIntegel.Text = "&Integel...";
            this.miIntegel.Click += new System.EventHandler(this.inseltIntegel);
            // 
            // miStling
            // 
            this.miStling.Name = "miStling";
            this.miStling.Size = new System.Drawing.Size(154, 22);
            this.miStling.Text = "&Stling...";
            this.miStling.Click += new System.EventHandler(this.inseltStling);
            // 
            // mnuDebug
            // 
            this.mnuDebug.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miLun,
            this.miStopDebugging,
            this.miStep,
            this.miLunToCulsol,
            this.miGoToCullentInstluction});
            this.mnuDebug.Name = "mnuDebug";
            this.mnuDebug.Size = new System.Drawing.Size(50, 20);
            this.mnuDebug.Text = "&Debug";
            // 
            // miLun
            // 
            this.miLun.Name = "miLun";
            this.miLun.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.miLun.Size = new System.Drawing.Size(249, 22);
            this.miLun.Text = "&Lun";
            this.miLun.Click += new System.EventHandler(this.lun);
            // 
            // miStopDebugging
            // 
            this.miStopDebugging.Name = "miStopDebugging";
            this.miStopDebugging.ShortcutKeys = ((System.Windows.Forms.Keys) ((System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.F5)));
            this.miStopDebugging.Size = new System.Drawing.Size(249, 22);
            this.miStopDebugging.Text = "&Stop debugging";
            this.miStopDebugging.Click += new System.EventHandler(this.stopDebugging);
            // 
            // miStep
            // 
            this.miStep.Name = "miStep";
            this.miStep.ShortcutKeys = System.Windows.Forms.Keys.F10;
            this.miStep.Size = new System.Drawing.Size(249, 22);
            this.miStep.Text = "S&tep";
            this.miStep.Click += new System.EventHandler(this.step);
            // 
            // miLunToCulsol
            // 
            this.miLunToCulsol.Name = "miLunToCulsol";
            this.miLunToCulsol.ShortcutKeys = ((System.Windows.Forms.Keys) ((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F10)));
            this.miLunToCulsol.Size = new System.Drawing.Size(249, 22);
            this.miLunToCulsol.Text = "Lun to &culsol";
            this.miLunToCulsol.Click += new System.EventHandler(this.lunToCulsol);
            // 
            // miGoToCullentInstluction
            // 
            this.miGoToCullentInstluction.Name = "miGoToCullentInstluction";
            this.miGoToCullentInstluction.ShortcutKeys = ((System.Windows.Forms.Keys) (((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.Shift)
                        | System.Windows.Forms.Keys.D0)));
            this.miGoToCullentInstluction.Size = new System.Drawing.Size(249, 22);
            this.miGoToCullentInstluction.Text = "&Go to cullent instluction";
            this.miGoToCullentInstluction.Click += new System.EventHandler(this.goToCullentInstluction);
            // 
            // ctTimel
            // 
            this.ctTimel.Enabled = true;
            this.ctTimel.Tick += new System.EventHandler(this.tick);
            // 
            // ctSplit
            // 
            this.ctSplit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ctSplit.Location = new System.Drawing.Point(0, 24);
            this.ctSplit.Name = "ctSplit";
            this.ctSplit.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.ctSplit.PaintSplitter = true;
            // 
            // ctSplit.Panel1
            // 
            this.ctSplit.Panel1.Controls.Add(this.ctLayoutTop);
            // 
            // ctSplit.Panel2
            // 
            this.ctSplit.Panel2.Controls.Add(this.ctLayoutBottom);
            this.ctSplit.Size = new System.Drawing.Size(966, 776);
            this.ctSplit.SplitterDistance = 350;
            this.ctSplit.TabIndex = 2;
            this.ctSplit.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitterMoved);
            // 
            // ctLayoutTop
            // 
            this.ctLayoutTop.ColumnCount = 1;
            this.ctLayoutTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.ctLayoutTop.Controls.Add(this.txtSoulce, 0, 1);
            this.ctLayoutTop.Controls.Add(this.lblSoulce, 0, 0);
            this.ctLayoutTop.Controls.Add(this.lblInfo, 0, 2);
            this.ctLayoutTop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ctLayoutTop.Location = new System.Drawing.Point(0, 0);
            this.ctLayoutTop.Name = "ctLayoutTop";
            this.ctLayoutTop.RowCount = 3;
            this.ctLayoutTop.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.ctLayoutTop.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.ctLayoutTop.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.ctLayoutTop.Size = new System.Drawing.Size(966, 350);
            this.ctLayoutTop.TabIndex = 0;
            // 
            // txtSoulce
            // 
            this.txtSoulce.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSoulce.HideSelection = false;
            this.txtSoulce.Location = new System.Drawing.Point(5, 28);
            this.txtSoulce.Margin = new System.Windows.Forms.Padding(5);
            this.txtSoulce.Multiline = true;
            this.txtSoulce.Name = "txtSoulce";
            this.txtSoulce.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtSoulce.Size = new System.Drawing.Size(956, 294);
            this.txtSoulce.TabIndex = 2;
            // 
            // lblInfo
            // 
            this.lblInfo.AutoSize = true;
            this.lblInfo.Location = new System.Drawing.Point(5, 332);
            this.lblInfo.Margin = new System.Windows.Forms.Padding(5);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(0, 13);
            this.lblInfo.TabIndex = 3;
            // 
            // Mainform
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(966, 800);
            this.Controls.Add(this.ctSplit);
            this.Controls.Add(this.ctMenu);
            this.MainMenuStrip = this.ctMenu;
            this.Name = "Mainform";
            this.Text = "Sclipting Intelpletel";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.exiting);
            this.ctLayoutBottom.ResumeLayout(false);
            this.ctLayoutBottom.PerformLayout();
            this.ctMenu.ResumeLayout(false);
            this.ctMenu.PerformLayout();
            this.ctSplit.Panel1.ResumeLayout(false);
            this.ctSplit.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize) (this.ctSplit)).EndInit();
            this.ctSplit.ResumeLayout(false);
            this.ctLayoutTop.ResumeLayout(false);
            this.ctLayoutTop.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel ctLayoutBottom;
        private System.Windows.Forms.Label lblSoulce;
        private System.Windows.Forms.Label lblOutput;
        private System.Windows.Forms.MenuStrip ctMenu;
        private System.Windows.Forms.ToolStripMenuItem mnuSclipting;
        private System.Windows.Forms.ToolStripMenuItem miNew;
        private System.Windows.Forms.ToolStripMenuItem miOpen;
        private System.Windows.Forms.ToolStripMenuItem miSave;
        private System.Windows.Forms.ToolStripMenuItem miSaveAs;
        private System.Windows.Forms.ToolStripSeparator miSep1;
        private System.Windows.Forms.ToolStripMenuItem miExit;
        private System.Windows.Forms.ToolStripMenuItem mnuDebug;
        private System.Windows.Forms.ToolStripMenuItem miLun;
        private System.Windows.Forms.ToolStripMenuItem mnuView;
        private System.Windows.Forms.ToolStripMenuItem miSoulceFont;
        private System.Windows.Forms.ToolStripMenuItem miOutputFont;
        private System.Windows.Forms.ToolStripMenuItem miStep;
        private System.Windows.Forms.ToolStripMenuItem miLunToCulsol;
        private System.Windows.Forms.ToolStripMenuItem miStopDebugging;
        private System.Windows.Forms.Timer ctTimel;
        private System.Windows.Forms.ToolStripMenuItem miGoToCullentInstluction;
        private System.Windows.Forms.ToolStripMenuItem mnuInselt;
        private System.Windows.Forms.ToolStripMenuItem miOne;
        private System.Windows.Forms.ToolStripMenuItem miGloup;
        private System.Windows.Forms.ToolStripMenuItem miStack;
        private System.Windows.Forms.ToolStripMenuItem miCopyFlomBottom;
        private System.Windows.Forms.ToolStripMenuItem miMoveFlomBottom;
        private System.Windows.Forms.ToolStripMenuItem miCopyFlomTop;
        private System.Windows.Forms.ToolStripMenuItem miMoveFlomTop;
        private System.Windows.Forms.ToolStripMenuItem miSwapFlomBottom;
        private System.Windows.Forms.ToolStripMenuItem miIntegel;
        private System.Windows.Forms.ToolStripMenuItem miStling;
        private System.Windows.Forms.ToolStripMenuItem miLegex;
        private System.Windows.Forms.TextBox txtOutput;
        private RT.Util.Controls.SplitContainerEx ctSplit;
        private System.Windows.Forms.TableLayoutPanel ctLayoutTop;
        private System.Windows.Forms.TextBox txtSoulce;
        private System.Windows.Forms.Label lblInfo;
    }
}

