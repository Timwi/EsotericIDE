using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using RT.Util;
using RT.Util.Controls;
using RT.Util.Dialogs;
using RT.Util.ExtensionMethods;
using RT.Util.Forms;

namespace EsotericIDE
{
    partial class Mainform : ManagedForm, IIde
    {
        private bool _splitterDistanceBugWorkaround;

        public Mainform(EsotericIDE.Settings settings)
            : base(settings.FormSettings)
        {
            InitializeComponent();
            Icon = Resources.EsotericIDEIcon;
            init();
        }

        private void init()
        {
            ctMenu.Renderer = new NativeToolStripRenderer();
            if (EsotericIDEProgram.Settings.SourceFont != null)
                txtSource.Font = EsotericIDEProgram.Settings.SourceFont.Font;
            if (EsotericIDEProgram.Settings.OutputFont != null)
                txtOutput.Font = EsotericIDEProgram.Settings.OutputFont.Font;

            miWordwrap.Checked = txtSource.WordWrap = EsotericIDEProgram.Settings.WordWrap;

            tabWatch.Controls.Add(notRunningLabel);

            updateUi();
            _splitterDistanceBugWorkaround = false;
            Activated += (_, __) =>
            {
                if (_splitterDistanceBugWorkaround)
                    return;
                _splitterDistanceBugWorkaround = true;
                if (EsotericIDEProgram.Settings.SplitterPercent != 0)
                    ctSplit.SplitterDistance = (int) (ctSplit.Height * EsotericIDEProgram.Settings.SplitterPercent);
            };

            _languages = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => typeof(ProgrammingLanguage).IsAssignableFrom(t) && !t.IsAbstract)
                .Select(t => (ProgrammingLanguage) Activator.CreateInstance(t))
                .OrderBy(t => t.LanguageName)
                .ToArray();
            LanguageSettings settings;
            foreach (var lang in _languages)
                if (EsotericIDEProgram.Settings.LanguageSettings.TryGetValue(lang.LanguageName, out settings) && settings != null)
                    lang.Settings = settings;
            cmbLanguage.Items.AddRange(_languages);

            ToolStripMenuItem[] currentLanguageSpecificMenus = null;
            cmbLanguage.SelectedIndexChanged += (_, __) =>
            {
                if (_currentLanguage != null)
                    EsotericIDEProgram.Settings.LanguageSettings[_currentLanguage.LanguageName] = _currentLanguage.Settings;
                _currentLanguage = (ProgrammingLanguage) cmbLanguage.SelectedItem;
                EsotericIDEProgram.Settings.LastLanguageName = _currentLanguage.LanguageName;
                if (currentLanguageSpecificMenus != null)
                    foreach (var menuItem in currentLanguageSpecificMenus)
                        ctMenu.Items.Remove(menuItem);
                currentLanguageSpecificMenus = _currentLanguage.CreateMenus(this);
                ctMenu.Items.AddRange(currentLanguageSpecificMenus);
            };
            var ll = _languages.IndexOf(lang => lang.LanguageName == EsotericIDEProgram.Settings.LastLanguageName);
            cmbLanguage.SelectedIndex = ll == -1 ? 0 : ll;
        }

        private ProgrammingLanguage[] _languages;
        private ProgrammingLanguage _currentLanguage;

        private string _input
        {
            get { return EsotericIDEProgram.Settings.DebugInput; }
            set
            {
                EsotericIDEProgram.Settings.DebugInput = value;
                updateUi();
            }
        }

        private bool _saveWhenRun
        {
            get { return EsotericIDEProgram.Settings.SaveWhenRun; }
            set
            {
                EsotericIDEProgram.Settings.SaveWhenRun = value;
                updateUi();
            }
        }

        private bool _wordWrap
        {
            get { return EsotericIDEProgram.Settings.WordWrap; }
            set
            {
                EsotericIDEProgram.Settings.WordWrap = value;
                updateUi();
            }
        }

        private string _currentFilePathBacking;
        private string _currentFilePath
        {
            get { return _currentFilePathBacking; }
            set
            {
                _currentFilePathBacking = value;
                updateUi();
            }
        }

        private bool _anyChangesBacking;
        private bool _anyChanges
        {
            get { return _anyChangesBacking; }
            set
            {
                _anyChangesBacking = value;
                updateUi();
            }
        }

        private ExecutionEnvironment _currentEnvironmentBacking;
        private ExecutionEnvironment _env
        {
            get { return _currentEnvironmentBacking; }
            set
            {
                _currentEnvironmentBacking = value;
                updateUi();
            }
        }

        private Position _currentPosition;
        private DateTime _lastFileTime;
        private int? _runToCursorBreakpoint;

        private void updateUi()
        {
            // Construct the window titlebar
            var text = _currentFilePath == null ? "(unnamed)" : _currentFilePath;
            text += " — Esoteric IDE";
            if (_anyChanges)
                text += " •";
            if (_env != null)
                text += " (running)";
            Text = text;

            txtSource.ReadOnly = _env != null;
            miGoToCurrentInstruction.Visible = _env != null;
            miStopDebugging.Visible = _env != null;
            miClearInput.Visible = _input != null;
            miSaveWhenRun.Checked = _saveWhenRun;
            txtSource.WordWrap = _wordWrap;
            miWordwrap.Checked = _wordWrap;
            txtSource.ScrollBars = _wordWrap ? ScrollBars.Vertical : ScrollBars.Both;
        }

        private bool canDestroy()
        {
            int result;

            if (_env != null)
            {
                result = DlgMessage.Show("Cancel debugging?", "Esoteric IDE", DlgType.Question, "&Yes", "&No");
                if (result == 1)
                    return false;

                // Tell the executing program to stop
                _env.State = ExecutionState.Stop;
                _env.Continue(true);

                // The stopped program will have triggered an executionFinished event, which needs to be processed before the form is disposed
                Application.DoEvents();
            }

            if (!_anyChanges)
                return true;

            result = DlgMessage.Show("Would you like to save your changes to this file?", "Esoteric IDE", DlgType.Question, "&Save", "&Discard", "&Cancel");
            if (result == 2)
                return false;
            if (result == 1)
                return true;

            return save() == DialogResult.OK;
        }

        private DialogResult save()
        {
            if (_currentFilePath == null)
                return saveAs();
            saveCore();
            return DialogResult.OK;
        }

        private DialogResult saveAs()
        {
            using (var save = new SaveFileDialog { Title = "Save file", DefaultExt = _currentLanguage.DefaultFileExtension, Filter = "{1} (*.{0})|*.{0}".Fmt(_currentLanguage.DefaultFileExtension, _currentLanguage.LanguageName) })
            {
                var result = save.ShowDialog();
                if (result == DialogResult.OK)
                {
                    _currentFilePath = save.FileName;
                    saveCore();
                }
                return result;
            }
        }

        private void saveCore()
        {
            File.WriteAllText(_currentFilePath, txtSource.Text);
            _lastFileTime = File.GetLastWriteTimeUtc(_currentFilePath);
            _anyChanges = false;
        }

        private void newFile(object _, EventArgs __)
        {
            if (!canDestroy())
                return;
            _currentFilePath = null;
            _anyChanges = false;

            txtSource.Text = "";
            txtOutput.Text = "";
            _timerPreviousSource = "";
            _timerPreviousCursorPosition = -1;
            sourceTextboxFixHack();
        }

        private void sourceTextboxFixHack()
        {
            // Hack — no idea why this is necessary, but without it, the next time you focus the source textbox,
            // all the text in it gets selected, but only the first time...
            ctTabs.Focus();
            txtSource.Focus();
            txtSource.SelectionStart = 0;
            txtSource.SelectionLength = 0;
        }

        private void open(object _, EventArgs __)
        {
            if (!canDestroy())
                return;

            var filter = _languages
                .OrderByDescending(lang => lang == _currentLanguage)
                .ThenBy(lang => lang.LanguageName)
                .Select(lang => "{1} (*.{0})|*.{0}".Fmt(lang.DefaultFileExtension, lang.LanguageName))
                .JoinString("|")
                + "|All files (*.*)|*.*";

            using (var open = new OpenFileDialog { Title = "Open file", DefaultExt = _currentLanguage.DefaultFileExtension, Filter = filter })
            {
                if (EsotericIDEProgram.Settings.LastDirectory != null)
                    try { open.InitialDirectory = EsotericIDEProgram.Settings.LastDirectory; }
                    catch { }
                if (open.ShowDialog() == DialogResult.Cancel)
                    return;
                EsotericIDEProgram.Settings.LastDirectory = Path.GetDirectoryName(open.FileName);

                openCore(open.FileName);
            }
        }

        private void openCore(string filePath)
        {
            if (!File.Exists(filePath))
            {
                DlgMessage.Show("The specified file does not exist.", "Error", DlgType.Error);
                return;
            }
            _currentFilePath = filePath;
            txtSource.Text = File.ReadAllText(_currentFilePath).UnifyLineEndings();
            txtOutput.Text = "";
            _timerPreviousSource = txtSource.Text;
            _timerPreviousCursorPosition = -1;
            sourceTextboxFixHack();
            _anyChanges = false;
            _lastFileTime = File.GetLastWriteTimeUtc(_currentFilePath);
        }

        private void save(object _, EventArgs __)
        {
            save();
        }

        private void saveAs(object _, EventArgs __)
        {
            saveAs();
        }

        private void font(object sender, EventArgs __)
        {
            if (sender == miSourceFont)
                font(ref EsotericIDEProgram.Settings.SourceFont, f => txtSource.Font = f.Font);
            else if (sender == miOutputFont)
                font(ref EsotericIDEProgram.Settings.OutputFont, f => txtOutput.Font = f.Font);
            else if (sender == miWatchFont)
                font(ref EsotericIDEProgram.Settings.WatchFont, f =>
                {
                    if (_env != null)
                        _env.SetWatchWindowFont(f);
                    else
                    {
                        // Assume that the notRunningLabel is there
                        tabWatch.Controls[0].Font = f.Font;
                        tabWatch.Controls[0].ForeColor = f.Color;
                    }
                });
        }

        private void font(ref FontSpec fontSpec, Action<FontSpec> set)
        {
            using (var fontDlg = new FontDialog { ShowColor = true })
            {
                if (fontSpec != null)
                    fontDlg.Font = fontSpec.Font;
                if (fontDlg.ShowDialog() == DialogResult.OK)
                {
                    fontSpec = fontSpec == null ? new FontSpec(fontDlg.Font, Color.Black) : fontSpec.SetFont(fontDlg.Font);
                    set(fontSpec);
                }
            }
        }

        private bool compile(ExecutionState state)
        {
            removeRunToCursorBreakpoint();
            _currentPosition = null;

            if (_env != null)
            {
                _env.State = state;
                return true;
            }

            if (_saveWhenRun)
                save();

            try
            {
                _env = _currentLanguage.Compile(txtSource.Text, _input ?? "");
                _env.OriginalSource = txtSource.Text;
                _env.State = state;
                _env.DebuggerBreak += p => { this.BeginInvoke(new Action(() => debuggerBreak(p))); };
                _env.ExecutionFinished += (c, e) => { this.BeginInvoke(new Action(() => executionFinished(c, e))); };
                foreach (int bp in lstBreakpoints.Items)
                    _env.AddBreakpoint(bp);
                _env.BreakpointsChanged += () => { this.BeginInvoke(new Action(() => breakpointsChanged())); };
                tabWatch.Controls.Clear();
                tabWatch.Controls.Add(_env.InitializeWatchWindow());
                if (EsotericIDEProgram.Settings.WatchFont != null)
                    _env.SetWatchWindowFont(EsotericIDEProgram.Settings.WatchFont);
                return true;
            }
            catch (CompileException e)
            {
                if (e.Index != null)
                {
                    txtSource.Focus();
                    txtSource.SelectionStart = e.Index.Value;
                    txtSource.SelectionLength = e.Length ?? 0;
                    txtSource.ScrollToCaret();
                }
                DlgMessage.Show("Compilation failed:" + Environment.NewLine + e.Message, "Esoteric IDE", DlgType.Error, "&OK");
                return false;
            }
            catch (Exception e)
            {
                DlgMessage.Show("Compilation failed:" + Environment.NewLine + e.Message + " (" + e.GetType().FullName + ")", "Esoteric IDE", DlgType.Error, "&OK");
                return false;
            }
        }

        private void run(object _, EventArgs __)
        {
            if (!compile(ExecutionState.Running))
                return;
            _env.Continue();
        }

        private Control notRunningLabel
        {
            get
            {
                var ret = new Label
                {
                    Text = "(not running)",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                if (EsotericIDEProgram.Settings.WatchFont != null)
                {
                    ret.Font = EsotericIDEProgram.Settings.WatchFont.Font;
                    ret.ForeColor = EsotericIDEProgram.Settings.WatchFont.Color;
                }
                return ret;
            }
        }

        private void executionFinished(bool canceled, RuntimeError runtimeError)
        {
            removeRunToCursorBreakpoint();
            tabWatch.Controls.Clear();
            tabWatch.Controls.Add(notRunningLabel);

            var sel = txtSource.SelectionStart;
            var len = txtSource.SelectionLength;
            txtSource.Text = _env.OriginalSource;
            txtOutput.Text = _env.Output.UnifyLineEndings();
            if (canceled && runtimeError == null)
                txtOutput.Text += Environment.NewLine + Environment.NewLine + "Execution stopped.";

            _env = null;
            _currentPosition = null;

            ctTabs.SelectedTab = tabOutput;
            txtSource.Focus();
            txtSource.SelectionStart = sel;
            txtSource.SelectionLength = len;
            txtSource.ScrollToCaret();

            // In case the file has changed since the last time we ran the program
            checkFileChanged();

            if (runtimeError != null)
            {
                var msg = "A run-time error occurred:{0}{0}{1}".Fmt(Environment.NewLine, runtimeError.Message);
                txtOutput.Text += Environment.NewLine + Environment.NewLine + msg;
                txtSource.Focus();
                if (runtimeError.Position != null)
                {
                    txtSource.Focus();
                    txtSource.SelectionStart = runtimeError.Position.Index;
                    txtSource.SelectionLength = runtimeError.Position.Length;
                    txtSource.ScrollToCaret();
                }
                DlgMessage.Show(msg, "Run-time error", DlgType.Error, "&OK");
            }
        }

        private void breakpointsChanged()
        {
            lstBreakpoints.BeginUpdate();
            try
            {
                lstBreakpoints.Items.Clear();
                if (_env != null)
                    foreach (var bp in _env.Breakpoints)
                        lstBreakpoints.Items.Add(bp);
            }
            finally
            {
                lstBreakpoints.EndUpdate();
            }
        }

        private void step(object _, EventArgs __)
        {
            if (!compile(ExecutionState.Debugging))
                return;
            _env.Continue();
        }

        private void debuggerBreak(Position position)
        {
            removeRunToCursorBreakpoint();
            _currentPosition = position;
            _env.UpdateWatch();
            var newSource = _env.ModifiedSource;
            if (newSource != null)
                txtSource.Text = newSource;
            txtOutput.Text = _env.Output.UnifyLineEndings();
            ctTabs.SelectedTab = tabWatch;
            goToCurrentInstruction();
        }

        private void runToCursor(object _, EventArgs __)
        {
            if (_env != null && _env.State != ExecutionState.Debugging)
                return;
            if (!compile(ExecutionState.Debugging))
                return;
            var to = txtSource.SelectionStart;
            if (!_env.Breakpoints.Contains(to))
            {
                _env.AddBreakpoint(to);
                _runToCursorBreakpoint = to;
            }
            _env.State = ExecutionState.Running;
            _env.Continue();
        }

        private void stopDebugging(object _, EventArgs __)
        {
            if (_env == null)
                return;
            removeRunToCursorBreakpoint();
            _env.State = ExecutionState.Stop;
            _env.Continue();
        }

        private void removeRunToCursorBreakpoint()
        {
            if (_runToCursorBreakpoint == null)
                return;
            if (_env != null)
                _env.RemoveBreakpoint(_runToCursorBreakpoint.Value);
            else
                lstBreakpoints.Items.Remove(_runToCursorBreakpoint.Value);
            _runToCursorBreakpoint = null;
        }

        private void exiting(object _, FormClosingEventArgs e)
        {
            var settings = _currentLanguage.Settings;
            if (settings != null)
                EsotericIDEProgram.Settings.LanguageSettings[_currentLanguage.LanguageName] = settings;
            else
                EsotericIDEProgram.Settings.LanguageSettings.Remove(_currentLanguage.LanguageName);
            if (!canDestroy())
                e.Cancel = true;
        }

        private string _timerPreviousSource = "";
        private int _timerPreviousCursorPosition = -1;

        private void tick(object _, EventArgs __)
        {
            var cursorPos = txtSource.SelectionStart;
            var source = txtSource.Text;

            if (cursorPos == _timerPreviousCursorPosition && source == _timerPreviousSource)
                return;
            _timerPreviousCursorPosition = cursorPos;
            if (_env == null)
            {
                if (_timerPreviousSource != source)
                    _anyChanges = true;
                _timerPreviousSource = source;
            }
            if (cursorPos < 0 || cursorPos > source.Length)
                lblInfo.Text = "";
            else
                lblInfo.Text = _currentLanguage.GetInfo(source, cursorPos);
        }

        private void goToCurrentInstruction(object _ = null, EventArgs __ = null)
        {
            if (_env == null || _currentPosition == null)
                return;
            txtSource.Focus();
            txtSource.SelectionStart = _currentPosition.Index;
            txtSource.SelectionLength = _currentPosition.Length;
            txtSource.ScrollToCaret();
        }

        private void splitterMoved(object _, EventArgs __)
        {
            if (_splitterDistanceBugWorkaround)
                EsotericIDEProgram.Settings.SplitterPercent = (double) ctSplit.SplitterDistance / ctSplit.Height;
        }

        private void about(object _, EventArgs __)
        {
            using (var a = new AboutBox())
                a.ShowDialog();
        }

        private void input(object _, EventArgs __)
        {
            _input = InputBox.GetLine("Please type the input to the program:", _input?.UnifyLineEndings(), "Esoteric IDE", "&OK", "&Cancel", useMultilineBox: true)?.Replace("\r", "") ?? _input;
        }

        private void clearInput(object _, EventArgs __)
        {
            _input = null;
        }

        private void toggleSaveWhenRun(object _, EventArgs __)
        {
            _saveWhenRun = !_saveWhenRun;
        }

        private void viewWatch(object _, EventArgs __)
        {
            ctTabs.SelectedTab = tabWatch;
        }

        private void viewOutput(object _, EventArgs __)
        {
            ctTabs.SelectedTab = tabOutput;
            txtOutput.Focus();
        }

        private void viewBreakpoints(object _, EventArgs __)
        {
            ctTabs.SelectedTab = tabBreakpoints;
            lstBreakpoints.Focus();
        }

        private void breakpointsKeyDown(object _, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && !e.Shift && !e.Alt && !e.Control)
            {
                lstBreakpoints.BeginUpdate();
                try
                {
                    var toDelete = lstBreakpoints.SelectedItems.Cast<int>().ToArray();
                    foreach (var del in toDelete)
                        if (_env == null)
                            lstBreakpoints.Items.Remove(del);
                        else
                            _env.RemoveBreakpoint(del);
                }
                finally
                {
                    lstBreakpoints.EndUpdate();
                }
            }
        }

        private void toggleBreakpoint(object _, EventArgs __)
        {
            if (_env == null)
            {
                var any = lstBreakpoints.Items.OfType<int>().Where(i => i >= txtSource.SelectionStart && i < txtSource.SelectionStart + txtSource.SelectionLength.ClipMin(1)).ToArray();
                if (any.Length > 0)
                    foreach (var bp in any)
                        lstBreakpoints.Items.Remove(bp);
                else
                    lstBreakpoints.Items.Add(txtSource.SelectionStart);
            }
            else
            {
                var any = false;
                for (int i = 0; i < txtSource.SelectionLength.ClipMin(1); i++)
                    any = any | _env.RemoveBreakpoint(txtSource.SelectionStart + i);
                if (!any)
                    _env.AddBreakpoint(txtSource.SelectionStart);
            }
        }

        private void breakpointsSelectedIndexChanged(object _, EventArgs __)
        {
            if (lstBreakpoints.SelectedItems.Count == 1)
            {
                txtSource.SelectionStart = (int) lstBreakpoints.SelectedItems[0];
                txtSource.SelectionLength = 1;
            }
        }

        private void exit(object _, EventArgs __)
        {
            Close();
        }

        private void selectProgrammingLanguage(object _, EventArgs __)
        {
            cmbLanguage.Focus();
        }

        private void sourceKeyDown(object _, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A && e.Control && !e.Alt && !e.Shift)
            {
                txtSource.SelectionStart = 0;
                txtSource.SelectionLength = txtSource.TextLength;
            }
        }

        private void toggleWordwrap(object sender, EventArgs e)
        {
            _wordWrap = !_wordWrap;
        }

        private bool _checkFileChangedBusy = false;
        private void activated(object sender, EventArgs e)
        {
            if (_env == null)
                checkFileChanged();
        }

        private void checkFileChanged()
        {
            // Make sure this is not called while it is already running, which would
            // happen because dismissing the DlgMessage triggers activated()
            if (!_checkFileChangedBusy)
            {
                _checkFileChangedBusy = true;
                if (_currentFilePath != null &&
                    File.Exists(_currentFilePath) &&
                    File.GetLastWriteTimeUtc(_currentFilePath) > _lastFileTime &&
                    DlgMessage.Show("The file has changed on disk. Reload?", "File has changed", DlgType.Question, "&Reload", "&Cancel") == 0)
                    openCore(_currentFilePath);
                _checkFileChangedBusy = false;
            }
        }

        private void revert(object sender, EventArgs e)
        {
            if (_currentFilePath != null && DlgMessage.Show("Revert all changes made since last save?", "Revert", DlgType.Question, "&Revert", "&Cancel") == 0)
                openCore(_currentFilePath);
        }

        public string GetSource() { return txtSource.Text; }
        public void ReplaceSource(string newSource) { txtSource.Text = newSource; }
        public string GetSelectedText() { return txtSource.SelectedText; }
        public void InsertText(string text) { txtSource.SelectedText = text; }
        public ExecutionEnvironment GetEnvironment() { return _env; }
    }
}
