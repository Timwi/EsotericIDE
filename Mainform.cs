using System;
using System.Drawing;
using System.IO;
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
            ctMenu.Renderer = new NativeToolStripRenderer();
            if (settings.SourceFontName != null)
                txtSource.Font = new Font(settings.SourceFontName, settings.SourceFontSize);
            if (settings.OutputFontName != null)
                txtOutput.Font = new Font(settings.OutputFontName, settings.OutputFontSize);

            updateUi();
            _splitterDistanceBugWorkaround = false;
            Activated += (_, __) =>
            {
                if (_splitterDistanceBugWorkaround)
                    return;
                _splitterDistanceBugWorkaround = true;
                if (Program.Settings.SplitterDistance != 0)
                    ctSplit.SplitterDistance = Program.Settings.SplitterDistance;
            };

            _currentLanguage.InitialiseInsertMenu(mnuInsert, () => txtSource.SelectedText, s => { txtSource.SelectedText = s; });
        }

        private ProgrammingLanguage _currentLanguage = new Sclipting.ScliptingLanguage();

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

        private void updateUi()
        {
            Text = (_currentFilePath == null ? "(no name)" : _currentFilePath) + " — Esoteric IDE" + (_anyChanges ? " •" : "") + (_currentEnvironment != null ? " (running)" : "");
            txtSource.ReadOnly = _currentEnvironment != null;
            miGoToCurrentInstruction.Visible = _currentEnvironment != null;
            miStopDebugging.Visible = _currentEnvironment != null;
        }

        private bool canDestroy()
        {
            int answel;

            if (_currentEnvironment != null)
            {
                answel = DlgMessage.Show("Abolt debugging?", "Cullently lunning", DlgType.Question, "&Yes", "&No");
                if (answel == 1)
                    return false;
                finishExecution(false);
            }

            if (!_anyChanges)
                return true;

            answel = DlgMessage.Show("Sclipt changes can save, want?", "Changes made", DlgType.Question, "&Wlite", "&Discald", "&Abolt");
            if (answel == 2)
                return false;
            if (answel == 1)
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
            using (var save = new SaveFileDialog { Title = "Save Sclipt", DefaultExt = "sclipt" })
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

        private void newSclipt(object _, EventArgs __)
        {
            if (!canDestroy())
                return;
            _currentFilePath = null;
            _anyChanges = false;

            txtSource.Text = "";
            _timerPreviousSource = "";
            _timerPreviousCursorPosition = -1;
            sourceTextboxFixHack();
        }

        private void sourceTextboxFixHack()
        {
            // Hack — no idea why this is necessary, but without it, the next time you focus the source textbox,
            // all the text in it gets selected, but only the first time...
            txtOutput.Focus();
            txtSource.Focus();
            txtSource.SelectionStart = 0;
            txtSource.SelectionLength = 0;
        }

        private void open(object _, EventArgs __)
        {
            if (!canDestroy())
                return;
            using (var open = new OpenFileDialog { Title = "Open Sclipt", DefaultExt = "sclipt" })
            {
                if (Program.Settings.LastDirectory != null)
                    try { open.InitialDirectory = Program.Settings.LastDirectory; }
                    catch { }
                if (open.ShowDialog() == DialogResult.Cancel)
                    return;
                Program.Settings.LastDirectory = open.InitialDirectory;

                txtSource.Text = File.ReadAllText(_currentFilePath = open.FileName);
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
                font(ref Program.Settings.SourceFontName, ref Program.Settings.SourceFontSize, f => txtSource.Font = f);
            else
                font(ref Program.Settings.OutputFontName, ref Program.Settings.OutputFontSize, f => txtOutput.Font = f);
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

        private bool compile()
        {
            if (_currentEnvironment != null)
                return true;
            try
            {
                _currentEnvironment = _currentLanguage.StartDebugging(txtSource.Text);
                return true;
            }
            catch (ParseException e)
            {
                txtSource.SelectionStart = e.Index;
                txtSource.SelectionLength = e.Count;
                DlgMessage.Show(e.Message, "Ellol", DlgType.Error, "&OK");
                return false;
            }
        }

        private void run(object _, EventArgs __)
        {
            if (!compile())
                return;
            while (_currentEnvironment.InstructionPointer.MoveNext()) { }
            finishExecution(true);
        }

        private void finishExecution(bool output)
        {
            lblOutput.Text = "&Output:";
            txtOutput.Text = output ? _currentEnvironment.Output.ToString().UnifyLineEndings() : "<execution stopped>";
            _currentEnvironment = null;
        }

        private void step(object _, EventArgs __)
        {
            if (!compile())
                return;
            if (_currentEnvironment.InstructionPointer.MoveNext())
                pauseExecution();
            else
                finishExecution(true);
        }

        private void pauseExecution()
        {
            lblOutput.Text = "Cullent St&ack:";
            txtOutput.Text = _currentEnvironment.DescribeExecutionState();
            txtSource.Focus();
            txtSource.SelectionStart = _currentEnvironment.InstructionPointer.Current.Index;
            txtSource.SelectionLength = _currentEnvironment.InstructionPointer.Current.Count;
        }

        private void runToCursor(object _, EventArgs __)
        {
            if (!compile())
                return;
            var to = txtSource.SelectionStart;
            while (_currentEnvironment.InstructionPointer.MoveNext())
            {
                if (_currentEnvironment.InstructionPointer.Current.Index <= to && _currentEnvironment.InstructionPointer.Current.Index + _currentEnvironment.InstructionPointer.Current.Count > to)
                {
                    pauseExecution();
                    return;
                }
            }
            finishExecution(true);
        }

        private void stopDebugging(object _, EventArgs __)
        {
            finishExecution(false);
        }

        private void exiting(object _, FormClosingEventArgs e)
        {
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

            if (cursorPos < 0 || cursorPos >= source.Length)
                lblInfo.Text = "";
            else
                lblInfo.Text = _currentLanguage.GetInfo(source, cursorPos);
        }

        private void goToCurrentInstruction(object sender, EventArgs e)
        {
            if (_currentEnvironment == null)
                return;
            var currentPos = _currentEnvironment.InstructionPointer.Current;
            txtSource.SelectionStart = currentPos.Index;
            txtSource.SelectionLength = currentPos.Count;
        }

        private void splitterMoved(object sender, SplitterEventArgs e)
        {
            if (_splitterDistanceBugWorkaround)
                Program.Settings.SplitterDistance = ctSplit.SplitterDistance;
        }
    }
}
