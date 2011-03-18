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
            this.ctRayout = new System.Windows.Forms.TableLayoutPanel();
            this.txtOutput = new System.Windows.Forms.TextBox();
            this.lblCode = new System.Windows.Forms.Label();
            this.txtCode = new System.Windows.Forms.TextBox();
            this.lblStack = new System.Windows.Forms.Label();
            this.txtStack = new System.Windows.Forms.TextBox();
            this.lblOutput = new System.Windows.Forms.Label();
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
            this.mnuDebug = new System.Windows.Forms.ToolStripMenuItem();
            this.miLun = new System.Windows.Forms.ToolStripMenuItem();
            this.stepToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lunToculsolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stopDebuggingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ctRayout.SuspendLayout();
            this.ctMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // ctRayout
            // 
            this.ctRayout.ColumnCount = 2;
            this.ctRayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.ctRayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.ctRayout.Controls.Add(this.txtOutput, 1, 3);
            this.ctRayout.Controls.Add(this.lblCode, 0, 0);
            this.ctRayout.Controls.Add(this.txtCode, 0, 1);
            this.ctRayout.Controls.Add(this.lblStack, 0, 2);
            this.ctRayout.Controls.Add(this.txtStack, 0, 3);
            this.ctRayout.Controls.Add(this.lblOutput, 1, 2);
            this.ctRayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ctRayout.Location = new System.Drawing.Point(0, 24);
            this.ctRayout.Name = "ctRayout";
            this.ctRayout.RowCount = 4;
            this.ctRayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.ctRayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.ctRayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.ctRayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.ctRayout.Size = new System.Drawing.Size(611, 489);
            this.ctRayout.TabIndex = 0;
            // 
            // txtOutput
            // 
            this.txtOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtOutput.Location = new System.Drawing.Point(310, 272);
            this.txtOutput.Margin = new System.Windows.Forms.Padding(5);
            this.txtOutput.Multiline = true;
            this.txtOutput.Name = "txtOutput";
            this.txtOutput.ReadOnly = true;
            this.txtOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtOutput.Size = new System.Drawing.Size(296, 212);
            this.txtOutput.TabIndex = 5;
            // 
            // lblCode
            // 
            this.lblCode.AutoSize = true;
            this.ctRayout.SetColumnSpan(this.lblCode, 2);
            this.lblCode.Location = new System.Drawing.Point(5, 5);
            this.lblCode.Margin = new System.Windows.Forms.Padding(5);
            this.lblCode.Name = "lblCode";
            this.lblCode.Size = new System.Drawing.Size(71, 13);
            this.lblCode.TabIndex = 0;
            this.lblCode.Text = "Soulce &Code:";
            // 
            // txtCode
            // 
            this.ctRayout.SetColumnSpan(this.txtCode, 2);
            this.txtCode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtCode.Location = new System.Drawing.Point(5, 28);
            this.txtCode.Margin = new System.Windows.Forms.Padding(5);
            this.txtCode.Multiline = true;
            this.txtCode.Name = "txtCode";
            this.txtCode.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtCode.Size = new System.Drawing.Size(601, 211);
            this.txtCode.TabIndex = 1;
            this.txtCode.TextChanged += new System.EventHandler(this.codeChanged);
            // 
            // lblStack
            // 
            this.lblStack.AutoSize = true;
            this.lblStack.Location = new System.Drawing.Point(5, 249);
            this.lblStack.Margin = new System.Windows.Forms.Padding(5);
            this.lblStack.Name = "lblStack";
            this.lblStack.Size = new System.Drawing.Size(73, 13);
            this.lblStack.TabIndex = 2;
            this.lblStack.Text = "Cullent St&ack:";
            // 
            // txtStack
            // 
            this.txtStack.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtStack.Location = new System.Drawing.Point(5, 272);
            this.txtStack.Margin = new System.Windows.Forms.Padding(5);
            this.txtStack.Multiline = true;
            this.txtStack.Name = "txtStack";
            this.txtStack.ReadOnly = true;
            this.txtStack.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtStack.Size = new System.Drawing.Size(295, 212);
            this.txtStack.TabIndex = 3;
            // 
            // lblOutput
            // 
            this.lblOutput.AutoSize = true;
            this.lblOutput.Location = new System.Drawing.Point(310, 249);
            this.lblOutput.Margin = new System.Windows.Forms.Padding(5);
            this.lblOutput.Name = "lblOutput";
            this.lblOutput.Size = new System.Drawing.Size(42, 13);
            this.lblOutput.TabIndex = 4;
            this.lblOutput.Text = "&Output:";
            // 
            // ctMenu
            // 
            this.ctMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuSclipting,
            this.mnuView,
            this.mnuDebug});
            this.ctMenu.Location = new System.Drawing.Point(0, 0);
            this.ctMenu.Name = "ctMenu";
            this.ctMenu.Size = new System.Drawing.Size(611, 24);
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
            // mnuDebug
            // 
            this.mnuDebug.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miLun,
            this.stopDebuggingToolStripMenuItem,
            this.stepToolStripMenuItem,
            this.lunToculsolToolStripMenuItem});
            this.mnuDebug.Name = "mnuDebug";
            this.mnuDebug.Size = new System.Drawing.Size(50, 20);
            this.mnuDebug.Text = "&Debug";
            // 
            // miLun
            // 
            this.miLun.Name = "miLun";
            this.miLun.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.miLun.Size = new System.Drawing.Size(198, 22);
            this.miLun.Text = "&Lun";
            this.miLun.Click += new System.EventHandler(this.lun);
            // 
            // stepToolStripMenuItem
            // 
            this.stepToolStripMenuItem.Name = "stepToolStripMenuItem";
            this.stepToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F10;
            this.stepToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.stepToolStripMenuItem.Text = "&Step";
            this.stepToolStripMenuItem.Click += new System.EventHandler(this.step);
            // 
            // lunToculsolToolStripMenuItem
            // 
            this.lunToculsolToolStripMenuItem.Name = "lunToculsolToolStripMenuItem";
            this.lunToculsolToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys) ((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F10)));
            this.lunToculsolToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.lunToculsolToolStripMenuItem.Text = "Lun to &culsol";
            this.lunToculsolToolStripMenuItem.Click += new System.EventHandler(this.lunToCulsol);
            // 
            // stopDebuggingToolStripMenuItem
            // 
            this.stopDebuggingToolStripMenuItem.Name = "stopDebuggingToolStripMenuItem";
            this.stopDebuggingToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys) ((System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.F5)));
            this.stopDebuggingToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.stopDebuggingToolStripMenuItem.Text = "S&top debugging";
            this.stopDebuggingToolStripMenuItem.Click += new System.EventHandler(this.stopDebugging);
            // 
            // Mainform
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(611, 513);
            this.Controls.Add(this.ctRayout);
            this.Controls.Add(this.ctMenu);
            this.MainMenuStrip = this.ctMenu;
            this.Name = "Mainform";
            this.Text = "Sclipting Intelpletel";
            this.ctRayout.ResumeLayout(false);
            this.ctRayout.PerformLayout();
            this.ctMenu.ResumeLayout(false);
            this.ctMenu.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel ctRayout;
        private System.Windows.Forms.Label lblCode;
        private System.Windows.Forms.TextBox txtCode;
        private System.Windows.Forms.Label lblStack;
        private System.Windows.Forms.TextBox txtStack;
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
        private System.Windows.Forms.TextBox txtOutput;
        private System.Windows.Forms.Label lblOutput;
        private System.Windows.Forms.ToolStripMenuItem miOutputFont;
        private System.Windows.Forms.ToolStripMenuItem stepToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem lunToculsolToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stopDebuggingToolStripMenuItem;
    }
}

