namespace EsotericIDE
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
            this.txtOutput = new System.Windows.Forms.TextBox();
            this.lblSource = new System.Windows.Forms.Label();
            this.ctTimer = new System.Windows.Forms.Timer(this.components);
            this.ctSplit = new RT.Util.Controls.SplitContainerEx();
            this.ctLayoutTop = new System.Windows.Forms.TableLayoutPanel();
            this.txtSource = new System.Windows.Forms.TextBox();
            this.lblInfo = new System.Windows.Forms.Label();
            this.ctTabs = new System.Windows.Forms.TabControl();
            this.tabExecutionState = new System.Windows.Forms.TabPage();
            this.txtExecutionState = new System.Windows.Forms.TextBox();
            this.tabOutput = new System.Windows.Forms.TabPage();
            this.tabBreakpoints = new System.Windows.Forms.TabPage();
            this.lstBreakpoints = new RT.Util.Controls.ListBoxEx();
            this.mnuFile = new System.Windows.Forms.ToolStripMenuItem();
            this.miNew = new System.Windows.Forms.ToolStripMenuItem();
            this.miOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.miSave = new System.Windows.Forms.ToolStripMenuItem();
            this.miSaveAs = new System.Windows.Forms.ToolStripMenuItem();
            this.miSep1 = new System.Windows.Forms.ToolStripSeparator();
            this.miExit = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuView = new System.Windows.Forms.ToolStripMenuItem();
            this.miSourceFont = new System.Windows.Forms.ToolStripMenuItem();
            this.miSep3 = new System.Windows.Forms.ToolStripSeparator();
            this.miExecutionState = new System.Windows.Forms.ToolStripMenuItem();
            this.miExecutionStateFont = new System.Windows.Forms.ToolStripMenuItem();
            this.miSep4 = new System.Windows.Forms.ToolStripSeparator();
            this.miOutput = new System.Windows.Forms.ToolStripMenuItem();
            this.miOutputFont = new System.Windows.Forms.ToolStripMenuItem();
            this.miSep5 = new System.Windows.Forms.ToolStripSeparator();
            this.breakpointsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuInsert = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuDebug = new System.Windows.Forms.ToolStripMenuItem();
            this.miRun = new System.Windows.Forms.ToolStripMenuItem();
            this.miStopDebugging = new System.Windows.Forms.ToolStripMenuItem();
            this.miStep = new System.Windows.Forms.ToolStripMenuItem();
            this.miRunToCursor = new System.Windows.Forms.ToolStripMenuItem();
            this.miGoToCurrentInstruction = new System.Windows.Forms.ToolStripMenuItem();
            this.miToggleBreakpoint = new System.Windows.Forms.ToolStripMenuItem();
            this.miSep2 = new System.Windows.Forms.ToolStripSeparator();
            this.miInput = new System.Windows.Forms.ToolStripMenuItem();
            this.miClearInput = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.miSaveWhenRun = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuHelp = new System.Windows.Forms.ToolStripMenuItem();
            this.miAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.ctMenu = new System.Windows.Forms.MenuStrip();
            ((System.ComponentModel.ISupportInitialize) (this.ctSplit)).BeginInit();
            this.ctSplit.Panel1.SuspendLayout();
            this.ctSplit.Panel2.SuspendLayout();
            this.ctSplit.SuspendLayout();
            this.ctLayoutTop.SuspendLayout();
            this.ctTabs.SuspendLayout();
            this.tabExecutionState.SuspendLayout();
            this.tabOutput.SuspendLayout();
            this.tabBreakpoints.SuspendLayout();
            this.ctMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtOutput
            // 
            this.txtOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtOutput.Location = new System.Drawing.Point(3, 3);
            this.txtOutput.Margin = new System.Windows.Forms.Padding(5);
            this.txtOutput.Multiline = true;
            this.txtOutput.Name = "txtOutput";
            this.txtOutput.ReadOnly = true;
            this.txtOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtOutput.Size = new System.Drawing.Size(952, 390);
            this.txtOutput.TabIndex = 8;
            // 
            // lblSource
            // 
            this.lblSource.AutoSize = true;
            this.lblSource.Location = new System.Drawing.Point(5, 5);
            this.lblSource.Margin = new System.Windows.Forms.Padding(5);
            this.lblSource.Name = "lblSource";
            this.lblSource.Size = new System.Drawing.Size(71, 13);
            this.lblSource.TabIndex = 0;
            this.lblSource.Text = "Source &code:";
            // 
            // ctTimer
            // 
            this.ctTimer.Enabled = true;
            this.ctTimer.Tick += new System.EventHandler(this.tick);
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
            this.ctSplit.Panel2.Controls.Add(this.ctTabs);
            this.ctSplit.Size = new System.Drawing.Size(966, 776);
            this.ctSplit.SplitterDistance = 350;
            this.ctSplit.TabIndex = 2;
            this.ctSplit.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitterMoved);
            // 
            // ctLayoutTop
            // 
            this.ctLayoutTop.ColumnCount = 1;
            this.ctLayoutTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.ctLayoutTop.Controls.Add(this.txtSource, 0, 1);
            this.ctLayoutTop.Controls.Add(this.lblSource, 0, 0);
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
            // txtSource
            // 
            this.txtSource.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSource.HideSelection = false;
            this.txtSource.Location = new System.Drawing.Point(5, 28);
            this.txtSource.Margin = new System.Windows.Forms.Padding(5);
            this.txtSource.Multiline = true;
            this.txtSource.Name = "txtSource";
            this.txtSource.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtSource.Size = new System.Drawing.Size(956, 294);
            this.txtSource.TabIndex = 2;
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
            // ctTabs
            // 
            this.ctTabs.Controls.Add(this.tabExecutionState);
            this.ctTabs.Controls.Add(this.tabOutput);
            this.ctTabs.Controls.Add(this.tabBreakpoints);
            this.ctTabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ctTabs.Location = new System.Drawing.Point(0, 0);
            this.ctTabs.Name = "ctTabs";
            this.ctTabs.SelectedIndex = 0;
            this.ctTabs.Size = new System.Drawing.Size(966, 422);
            this.ctTabs.TabIndex = 1;
            // 
            // tabExecutionState
            // 
            this.tabExecutionState.Controls.Add(this.txtExecutionState);
            this.tabExecutionState.Location = new System.Drawing.Point(4, 22);
            this.tabExecutionState.Name = "tabExecutionState";
            this.tabExecutionState.Padding = new System.Windows.Forms.Padding(3);
            this.tabExecutionState.Size = new System.Drawing.Size(958, 396);
            this.tabExecutionState.TabIndex = 1;
            this.tabExecutionState.Text = "Execution State";
            this.tabExecutionState.UseVisualStyleBackColor = true;
            // 
            // txtExecutionState
            // 
            this.txtExecutionState.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtExecutionState.Location = new System.Drawing.Point(3, 3);
            this.txtExecutionState.Margin = new System.Windows.Forms.Padding(5);
            this.txtExecutionState.Multiline = true;
            this.txtExecutionState.Name = "txtExecutionState";
            this.txtExecutionState.ReadOnly = true;
            this.txtExecutionState.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtExecutionState.Size = new System.Drawing.Size(952, 390);
            this.txtExecutionState.TabIndex = 9;
            // 
            // tabOutput
            // 
            this.tabOutput.Controls.Add(this.txtOutput);
            this.tabOutput.Location = new System.Drawing.Point(4, 22);
            this.tabOutput.Name = "tabOutput";
            this.tabOutput.Padding = new System.Windows.Forms.Padding(3);
            this.tabOutput.Size = new System.Drawing.Size(958, 396);
            this.tabOutput.TabIndex = 0;
            this.tabOutput.Text = "Output";
            this.tabOutput.UseVisualStyleBackColor = true;
            // 
            // tabBreakpoints
            // 
            this.tabBreakpoints.Controls.Add(this.lstBreakpoints);
            this.tabBreakpoints.Location = new System.Drawing.Point(4, 22);
            this.tabBreakpoints.Name = "tabBreakpoints";
            this.tabBreakpoints.Size = new System.Drawing.Size(958, 396);
            this.tabBreakpoints.TabIndex = 2;
            this.tabBreakpoints.Text = "Breakpoints";
            this.tabBreakpoints.UseVisualStyleBackColor = true;
            // 
            // lstBreakpoints
            // 
            this.lstBreakpoints.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstBreakpoints.FormattingEnabled = true;
            this.lstBreakpoints.Location = new System.Drawing.Point(0, 0);
            this.lstBreakpoints.Name = "lstBreakpoints";
            this.lstBreakpoints.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lstBreakpoints.Size = new System.Drawing.Size(958, 396);
            this.lstBreakpoints.TabIndex = 0;
            this.lstBreakpoints.SelectedIndexChanged += new System.EventHandler(this.breakpointsSelectedIndexChanged);
            this.lstBreakpoints.KeyDown += new System.Windows.Forms.KeyEventHandler(this.breakpointsKeyDown);
            // 
            // mnuFile
            // 
            this.mnuFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miNew,
            this.miOpen,
            this.miSave,
            this.miSaveAs,
            this.miSep1,
            this.miExit});
            this.mnuFile.Name = "mnuFile";
            this.mnuFile.Size = new System.Drawing.Size(35, 20);
            this.mnuFile.Text = "&File";
            // 
            // miNew
            // 
            this.miNew.Name = "miNew";
            this.miNew.ShortcutKeys = ((System.Windows.Forms.Keys) ((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.miNew.Size = new System.Drawing.Size(152, 22);
            this.miNew.Text = "&New";
            this.miNew.Click += new System.EventHandler(this.newFile);
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
            this.miSourceFont,
            this.miSep3,
            this.miExecutionState,
            this.miExecutionStateFont,
            this.miSep4,
            this.miOutput,
            this.miOutputFont,
            this.miSep5,
            this.breakpointsToolStripMenuItem});
            this.mnuView.Name = "mnuView";
            this.mnuView.Size = new System.Drawing.Size(41, 20);
            this.mnuView.Text = "&View";
            // 
            // miSourceFont
            // 
            this.miSourceFont.Name = "miSourceFont";
            this.miSourceFont.Size = new System.Drawing.Size(209, 22);
            this.miSourceFont.Text = "&Source Font...";
            this.miSourceFont.Click += new System.EventHandler(this.font);
            // 
            // miSep3
            // 
            this.miSep3.Name = "miSep3";
            this.miSep3.Size = new System.Drawing.Size(206, 6);
            // 
            // miExecutionState
            // 
            this.miExecutionState.Name = "miExecutionState";
            this.miExecutionState.ShortcutKeys = ((System.Windows.Forms.Keys) (((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt)
                        | System.Windows.Forms.Keys.E)));
            this.miExecutionState.Size = new System.Drawing.Size(209, 22);
            this.miExecutionState.Text = "&Execution State";
            this.miExecutionState.Click += new System.EventHandler(this.viewExecutionState);
            // 
            // miExecutionStateFont
            // 
            this.miExecutionStateFont.Name = "miExecutionStateFont";
            this.miExecutionStateFont.Size = new System.Drawing.Size(209, 22);
            this.miExecutionStateFont.Text = "E&xecution State Font...";
            // 
            // miSep4
            // 
            this.miSep4.Name = "miSep4";
            this.miSep4.Size = new System.Drawing.Size(206, 6);
            // 
            // miOutput
            // 
            this.miOutput.Name = "miOutput";
            this.miOutput.ShortcutKeys = ((System.Windows.Forms.Keys) (((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt)
                        | System.Windows.Forms.Keys.O)));
            this.miOutput.Size = new System.Drawing.Size(209, 22);
            this.miOutput.Text = "&Output";
            this.miOutput.Click += new System.EventHandler(this.viewOutput);
            // 
            // miOutputFont
            // 
            this.miOutputFont.Name = "miOutputFont";
            this.miOutputFont.Size = new System.Drawing.Size(209, 22);
            this.miOutputFont.Text = "O&utput Font...";
            this.miOutputFont.Click += new System.EventHandler(this.font);
            // 
            // miSep5
            // 
            this.miSep5.Name = "miSep5";
            this.miSep5.Size = new System.Drawing.Size(206, 6);
            // 
            // breakpointsToolStripMenuItem
            // 
            this.breakpointsToolStripMenuItem.Name = "breakpointsToolStripMenuItem";
            this.breakpointsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys) (((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt)
                        | System.Windows.Forms.Keys.B)));
            this.breakpointsToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
            this.breakpointsToolStripMenuItem.Text = "&Breakpoints";
            this.breakpointsToolStripMenuItem.Click += new System.EventHandler(this.viewBreakpoints);
            // 
            // mnuInsert
            // 
            this.mnuInsert.Name = "mnuInsert";
            this.mnuInsert.Size = new System.Drawing.Size(48, 20);
            this.mnuInsert.Text = "&Insert";
            // 
            // mnuDebug
            // 
            this.mnuDebug.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miRun,
            this.miStopDebugging,
            this.miStep,
            this.miRunToCursor,
            this.miGoToCurrentInstruction,
            this.miToggleBreakpoint,
            this.miSep2,
            this.miInput,
            this.miClearInput});
            this.mnuDebug.Name = "mnuDebug";
            this.mnuDebug.Size = new System.Drawing.Size(50, 20);
            this.mnuDebug.Text = "&Debug";
            // 
            // miRun
            // 
            this.miRun.Name = "miRun";
            this.miRun.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.miRun.Size = new System.Drawing.Size(225, 22);
            this.miRun.Text = "&Run";
            this.miRun.Click += new System.EventHandler(this.run);
            // 
            // miStopDebugging
            // 
            this.miStopDebugging.Name = "miStopDebugging";
            this.miStopDebugging.ShortcutKeys = ((System.Windows.Forms.Keys) ((System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.F5)));
            this.miStopDebugging.Size = new System.Drawing.Size(225, 22);
            this.miStopDebugging.Text = "&Stop debugging";
            this.miStopDebugging.Click += new System.EventHandler(this.stopDebugging);
            // 
            // miStep
            // 
            this.miStep.Name = "miStep";
            this.miStep.ShortcutKeys = System.Windows.Forms.Keys.F10;
            this.miStep.Size = new System.Drawing.Size(225, 22);
            this.miStep.Text = "S&tep";
            this.miStep.Click += new System.EventHandler(this.step);
            // 
            // miRunToCursor
            // 
            this.miRunToCursor.Name = "miRunToCursor";
            this.miRunToCursor.ShortcutKeys = ((System.Windows.Forms.Keys) ((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F10)));
            this.miRunToCursor.Size = new System.Drawing.Size(225, 22);
            this.miRunToCursor.Text = "Run to &cursor";
            this.miRunToCursor.Click += new System.EventHandler(this.runToCursor);
            // 
            // miGoToCurrentInstruction
            // 
            this.miGoToCurrentInstruction.Name = "miGoToCurrentInstruction";
            this.miGoToCurrentInstruction.ShortcutKeys = ((System.Windows.Forms.Keys) ((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.D0)));
            this.miGoToCurrentInstruction.Size = new System.Drawing.Size(225, 22);
            this.miGoToCurrentInstruction.Text = "&Go to current instruction";
            this.miGoToCurrentInstruction.Click += new System.EventHandler(this.goToCurrentInstruction);
            // 
            // miToggleBreakpoint
            // 
            this.miToggleBreakpoint.Name = "miToggleBreakpoint";
            this.miToggleBreakpoint.ShortcutKeys = System.Windows.Forms.Keys.F9;
            this.miToggleBreakpoint.Size = new System.Drawing.Size(225, 22);
            this.miToggleBreakpoint.Text = "Toggle &breakpoint";
            this.miToggleBreakpoint.Click += new System.EventHandler(this.toggleBreakpoint);
            // 
            // miSep2
            // 
            this.miSep2.Name = "miSep2";
            this.miSep2.Size = new System.Drawing.Size(222, 6);
            // 
            // miInput
            // 
            this.miInput.Name = "miInput";
            this.miInput.Size = new System.Drawing.Size(225, 22);
            this.miInput.Text = "&Input...";
            this.miInput.Click += new System.EventHandler(this.input);
            // 
            // miClearInput
            // 
            this.miClearInput.Name = "miClearInput";
            this.miClearInput.Size = new System.Drawing.Size(225, 22);
            this.miClearInput.Text = "C&lear input";
            this.miClearInput.Click += new System.EventHandler(this.clearInput);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miSaveWhenRun});
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(56, 20);
            this.optionsToolStripMenuItem.Text = "&Options";
            // 
            // miSaveWhenRun
            // 
            this.miSaveWhenRun.Checked = true;
            this.miSaveWhenRun.CheckState = System.Windows.Forms.CheckState.Checked;
            this.miSaveWhenRun.Name = "miSaveWhenRun";
            this.miSaveWhenRun.Size = new System.Drawing.Size(149, 22);
            this.miSaveWhenRun.Text = "&Save when Run";
            this.miSaveWhenRun.Click += new System.EventHandler(this.toggleSaveWhenRun);
            // 
            // mnuHelp
            // 
            this.mnuHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miAbout});
            this.mnuHelp.Name = "mnuHelp";
            this.mnuHelp.Size = new System.Drawing.Size(40, 20);
            this.mnuHelp.Text = "&Help";
            // 
            // miAbout
            // 
            this.miAbout.Name = "miAbout";
            this.miAbout.Size = new System.Drawing.Size(115, 22);
            this.miAbout.Text = "&About...";
            this.miAbout.Click += new System.EventHandler(this.about);
            // 
            // ctMenu
            // 
            this.ctMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuFile,
            this.mnuView,
            this.mnuInsert,
            this.mnuDebug,
            this.optionsToolStripMenuItem,
            this.mnuHelp});
            this.ctMenu.Location = new System.Drawing.Point(0, 0);
            this.ctMenu.Name = "ctMenu";
            this.ctMenu.Size = new System.Drawing.Size(966, 24);
            this.ctMenu.TabIndex = 1;
            this.ctMenu.Text = "Main menu";
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
            this.Text = "Esoteric IDE";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.exiting);
            this.ctSplit.Panel1.ResumeLayout(false);
            this.ctSplit.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize) (this.ctSplit)).EndInit();
            this.ctSplit.ResumeLayout(false);
            this.ctLayoutTop.ResumeLayout(false);
            this.ctLayoutTop.PerformLayout();
            this.ctTabs.ResumeLayout(false);
            this.tabExecutionState.ResumeLayout(false);
            this.tabExecutionState.PerformLayout();
            this.tabOutput.ResumeLayout(false);
            this.tabOutput.PerformLayout();
            this.tabBreakpoints.ResumeLayout(false);
            this.ctMenu.ResumeLayout(false);
            this.ctMenu.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblSource;
        private System.Windows.Forms.Timer ctTimer;
        private System.Windows.Forms.TextBox txtOutput;
        private RT.Util.Controls.SplitContainerEx ctSplit;
        private System.Windows.Forms.TableLayoutPanel ctLayoutTop;
        private System.Windows.Forms.TextBox txtSource;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.ToolStripMenuItem mnuFile;
        private System.Windows.Forms.ToolStripMenuItem miNew;
        private System.Windows.Forms.ToolStripMenuItem miOpen;
        private System.Windows.Forms.ToolStripMenuItem miSave;
        private System.Windows.Forms.ToolStripMenuItem miSaveAs;
        private System.Windows.Forms.ToolStripSeparator miSep1;
        private System.Windows.Forms.ToolStripMenuItem miExit;
        private System.Windows.Forms.ToolStripMenuItem mnuView;
        private System.Windows.Forms.ToolStripMenuItem miSourceFont;
        private System.Windows.Forms.ToolStripMenuItem miOutputFont;
        private System.Windows.Forms.ToolStripMenuItem mnuInsert;
        private System.Windows.Forms.ToolStripMenuItem mnuDebug;
        private System.Windows.Forms.ToolStripMenuItem miRun;
        private System.Windows.Forms.ToolStripMenuItem miStopDebugging;
        private System.Windows.Forms.ToolStripMenuItem miStep;
        private System.Windows.Forms.ToolStripMenuItem miRunToCursor;
        private System.Windows.Forms.ToolStripMenuItem miGoToCurrentInstruction;
        private System.Windows.Forms.ToolStripSeparator miSep2;
        private System.Windows.Forms.ToolStripMenuItem miInput;
        private System.Windows.Forms.ToolStripMenuItem miClearInput;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem miSaveWhenRun;
        private System.Windows.Forms.ToolStripMenuItem mnuHelp;
        private System.Windows.Forms.ToolStripMenuItem miAbout;
        private System.Windows.Forms.MenuStrip ctMenu;
        private System.Windows.Forms.TabControl ctTabs;
        private System.Windows.Forms.TabPage tabExecutionState;
        private System.Windows.Forms.TextBox txtExecutionState;
        private System.Windows.Forms.TabPage tabOutput;
        private System.Windows.Forms.TabPage tabBreakpoints;
        private System.Windows.Forms.ToolStripMenuItem miExecutionStateFont;
        private RT.Util.Controls.ListBoxEx lstBreakpoints;
        private System.Windows.Forms.ToolStripSeparator miSep3;
        private System.Windows.Forms.ToolStripMenuItem miExecutionState;
        private System.Windows.Forms.ToolStripSeparator miSep4;
        private System.Windows.Forms.ToolStripMenuItem miOutput;
        private System.Windows.Forms.ToolStripSeparator miSep5;
        private System.Windows.Forms.ToolStripMenuItem breakpointsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem miToggleBreakpoint;
    }
}

