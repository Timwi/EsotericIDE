using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using RT.Util.Controls;
using RT.Util.Dialogs;
using RT.Util.ExtensionMethods;
using RT.Util.Forms;

namespace EsotericIDE
{
    partial class Mainform : ManagedForm
    {
        private bool _splitterDistanceBugWorkaround;

        public Mainform(EsotericIDE.Settings settings)
            : base(settings.FormSettings)
        {
            InitializeComponent();
            init();
        }

        private void init()
        {
            ctMenu.Renderer = new NativeToolStripRenderer();
            if (EsotericIDEProgram.Settings.SourceFontName != null)
                txtSource.Font = new Font(EsotericIDEProgram.Settings.SourceFontName, EsotericIDEProgram.Settings.SourceFontSize);
            if (EsotericIDEProgram.Settings.ExecutionStateFontName != null)
                txtExecutionState.Font = new Font(EsotericIDEProgram.Settings.ExecutionStateFontName, EsotericIDEProgram.Settings.ExecutionStateFontSize);
            if (EsotericIDEProgram.Settings.OutputFontName != null)
                txtOutput.Font = new Font(EsotericIDEProgram.Settings.OutputFontName, EsotericIDEProgram.Settings.OutputFontSize);

            txtExecutionState.Text = "(not running)";

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

            if (EsotericIDEProgram.Settings.LanguageSettings == null)
                EsotericIDEProgram.Settings.LanguageSettings = new Dictionary<string, LanguageSettings>();
            _languages = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => typeof(ProgrammingLanguage).IsAssignableFrom(t) && !t.IsAbstract)
                .Select(t => (ProgrammingLanguage) Activator.CreateInstance(t))
                .OrderBy(t => t.LanguageName)
                .ToArray();
            LanguageSettings settings;
            foreach (var lang in _languages)
                if (EsotericIDEProgram.Settings.LanguageSettings.TryGetValue(lang.LanguageName, out settings) && settings != null)
                    lang.SetSettings(settings);
            cmbLanguage.Items.AddRange(_languages);

            ToolStripMenuItem[] currentLanguageSpecificMenus = null;
            cmbLanguage.SelectedIndexChanged += (_, __) =>
            {
                _currentLanguage = (ProgrammingLanguage) cmbLanguage.SelectedItem;
                EsotericIDEProgram.Settings.LanguageSettings[_currentLanguage.LanguageName] = _currentLanguage.GetSettings();
                EsotericIDEProgram.Settings.LastLanguageName = _currentLanguage.LanguageName;
                if (currentLanguageSpecificMenus != null)
                    foreach (var menuItem in currentLanguageSpecificMenus)
                        ctMenu.Items.Remove(menuItem);
                currentLanguageSpecificMenus = _currentLanguage.CreateMenus(() => txtSource.SelectedText, s => { txtSource.SelectedText = s; });
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
        private ExecutionEnvironment _currentEnvironment
        {
            get { return _currentEnvironmentBacking; }
            set
            {
                _currentEnvironmentBacking = value;
                updateUi();
            }
        }

        private Position _currentPosition;

        private void updateUi()
        {
            // Construct the window titlebar
            var text = _currentFilePath == null ? "(unnamed)" : _currentFilePath;
            text += " — Esoteric IDE";
            if (_anyChanges)
                text += " •";
            if (_currentEnvironment != null)
                text += " (running)";
            Text = text;

            txtSource.ReadOnly = _currentEnvironment != null;
            miGoToCurrentInstruction.Visible = _currentEnvironment != null;
            miStopDebugging.Visible = _currentEnvironment != null;
            miClearInput.Visible = _input != null;
            miSaveWhenRun.Checked = _saveWhenRun;
        }

        private bool canDestroy()
        {
            int result;

            if (_currentEnvironment != null)
            {
                result = DlgMessage.Show("Cancel debugging?", "Esoteric IDE", DlgType.Question, "&Yes", "&No");
                if (result == 1)
                    return false;

                // Tell the executing program to stop
                _currentEnvironment.State = ExecutionState.Stop;
                _currentEnvironment.Continue(true);

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
            _anyChanges = false;
        }

        private void newFile(object _, EventArgs __)
        {
            if (!canDestroy())
                return;
            _currentFilePath = null;
            _anyChanges = false;

            txtSource.Text = "";
            txtExecutionState.Text = "";
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

                txtSource.Text = File.ReadAllText(_currentFilePath = open.FileName);
                txtExecutionState.Text = "";
                txtOutput.Text = "";
                _timerPreviousSource = txtSource.Text;
                _timerPreviousCursorPosition = -1;
                sourceTextboxFixHack();
                _anyChanges = false;
            }
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
                font(ref EsotericIDEProgram.Settings.SourceFontName, ref EsotericIDEProgram.Settings.SourceFontSize, f => txtSource.Font = f);
            else if (sender == miExecutionStateFont)
                font(ref EsotericIDEProgram.Settings.ExecutionStateFontName, ref EsotericIDEProgram.Settings.ExecutionStateFontSize, f => txtExecutionState.Font = f);
            else if (sender == miOutputFont)
                font(ref EsotericIDEProgram.Settings.OutputFontName, ref EsotericIDEProgram.Settings.OutputFontSize, f => txtOutput.Font = f);
        }

        private void font(ref string name, ref float size, Action<Font> set)
        {
            using (var font = new FontDialog { ShowColor = true })
            {
                if (name != null)
                    font.Font = new Font(name, size);
                if (font.ShowDialog() == DialogResult.OK)
                {
                    set(font.Font);
                    name = font.Font.Name;
                    size = font.Font.Size;
                }
            }
        }

        private bool compile(ExecutionState state)
        {
            _currentPosition = null;

            if (_currentEnvironment != null)
            {
                _currentEnvironment.State = state;
                return true;
            }

            if (_saveWhenRun)
                save();

            try
            {
                var input = _input ?? InputBox.GetLine("Please type the input to the program:", "", "Esoteric IDE", "&OK", "&Cancel");
                if (input == null)
                    return false;
                _currentEnvironment = _currentLanguage.Compile(txtSource.Text, input);
                _currentEnvironment.State = state;
                _currentEnvironment.DebuggerBreak += p => { this.Invoke(new Action(() => debuggerBreak(p))); };
                _currentEnvironment.ExecutionFinished += (c, e) => { this.Invoke(new Action(() => executionFinished(c, e))); };
                foreach (int bp in lstBreakpoints.Items)
                    _currentEnvironment.AddBreakpoint(bp);
                _currentEnvironment.BreakpointsChanged += () => { this.Invoke(new Action(() => breakpointsChanged())); };
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
            _currentEnvironment.Continue();
        }

        private void executionFinished(bool canceled, RuntimeError runtimeError)
        {
            txtExecutionState.Text = "(not running)";
            txtOutput.Text = _currentEnvironment.Output.UnifyLineEndings();
            if (canceled && runtimeError == null)
                txtOutput.Text += Environment.NewLine + Environment.NewLine + "Execution stopped.";
            ctTabs.SelectedTab = tabOutput;
            _currentEnvironment = null;
            _currentPosition = null;

            txtSource.Focus();
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
                if (_currentEnvironment != null)
                    foreach (var bp in _currentEnvironment.Breakpoints)
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
            _currentEnvironment.Continue();
        }

        private void debuggerBreak(Position position)
        {
            _currentPosition = position;
            txtExecutionState.Text = _currentEnvironment.DescribeExecutionState();
            txtOutput.Text = _currentEnvironment.Output;
            ctTabs.SelectedTab = tabExecutionState;
            goToCurrentInstruction();
        }

        private void runToCursor(object _, EventArgs __)
        {
            if (!compile(ExecutionState.Debugging))
                return;
            var to = txtSource.SelectionStart;
            if (!_currentEnvironment.Breakpoints.Contains(to))
                _currentEnvironment.AddBreakpoint(to);
            _currentEnvironment.State = ExecutionState.Running;
            _currentEnvironment.Continue();
        }

        private void stopDebugging(object _, EventArgs __)
        {
            if (_currentEnvironment == null)
                return;
            _currentEnvironment.State = ExecutionState.Stop;
            _currentEnvironment.Continue();
        }

        private void exiting(object _, FormClosingEventArgs e)
        {
            EsotericIDEProgram.Settings.LanguageSettings[_currentLanguage.LanguageName] = _currentLanguage.GetSettings();
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
            if (_timerPreviousSource != source)
                _anyChanges = true;
            _timerPreviousSource = source;

            if (cursorPos < 0 || cursorPos > source.Length)
                lblInfo.Text = "";
            else
                lblInfo.Text = _currentLanguage.GetInfo(source, cursorPos);
        }

        private void goToCurrentInstruction(object _ = null, EventArgs __ = null)
        {
            if (_currentEnvironment == null || _currentPosition == null)
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
            _input = InputBox.GetLine("Please type the input to the program:", _input, "Esoteric IDE", "&OK", "&Cancel", useMultilineBox: true) ?? _input;
        }

        private void clearInput(object _, EventArgs __)
        {
            _input = null;
        }

        private void toggleSaveWhenRun(object _, EventArgs __)
        {
            _saveWhenRun = !_saveWhenRun;
        }

        private void viewExecutionState(object _, EventArgs __)
        {
            ctTabs.SelectedTab = tabExecutionState;
            txtExecutionState.Focus();
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

        private void breakpointsKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && !e.Shift && !e.Alt && !e.Control)
            {
                lstBreakpoints.BeginUpdate();
                try
                {
                    var toDelete = lstBreakpoints.SelectedItems.Cast<int>().ToArray();
                    foreach (var del in toDelete)
                        if (_currentEnvironment == null)
                            lstBreakpoints.Items.Remove(del);
                        else
                            _currentEnvironment.RemoveBreakpoint(del);
                }
                finally
                {
                    lstBreakpoints.EndUpdate();
                }
            }
        }

        private void toggleBreakpoint(object _, EventArgs __)
        {
            if (_currentEnvironment == null)
            {
                if (lstBreakpoints.Items.Contains(txtSource.SelectionStart))
                    lstBreakpoints.Items.Remove(txtSource.SelectionStart);
                else
                    lstBreakpoints.Items.Add(txtSource.SelectionStart);
            }
            else
            {
                if (!_currentEnvironment.RemoveBreakpoint(txtSource.SelectionStart))
                    _currentEnvironment.AddBreakpoint(txtSource.SelectionStart);
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

        private void sourceKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A && e.Control && !e.Alt && !e.Shift)
            {
                txtSource.SelectionStart = 0;
                txtSource.SelectionLength = txtSource.TextLength;
            }
        }
    }
}
