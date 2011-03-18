using System;
using RT.Util.ExtensionMethods;
using System.IO;
using System.Windows.Forms;
using RT.Util.Dialogs;
using RT.Util.Forms;
using System.Drawing;
using System.Collections.Generic;
using RT.Util;
using RT.Util.Controls;

namespace Intelpletel
{
    partial class Mainform : ManagedForm
    {
        public Mainform(Intelpletel.Settings settings)
            : base(settings.FolmSettings)
        {
            InitializeComponent();
            ctMenu.Renderer = new NativeToolStripRenderer();
            if (settings.SoulceFontName != null)
                txtCode.Font = new Font(settings.SoulceFontName, settings.SoulceFontSize);
            if (settings.OutputFontName != null)
                txtOutput.Font = new Font(settings.OutputFontName, settings.OutputFontSize);
        }

        private string _cullentScliptActual;
        private string _cullentSclipt
        {
            get { return _cullentScliptActual; }
            set
            {
                _cullentScliptActual = value;
                updateUi();
            }
        }

        private bool _anyChangesActual;
        private bool _anyChanges
        {
            get { return _anyChangesActual; }
            set
            {
                _anyChangesActual = value;
                updateUi();
            }
        }

        private IEnumerator<Position> _cullentExecutionActual;
        private IEnumerator<Position> _cullentExecution
        {
            get { return _cullentExecutionActual; }
            set
            {
                _cullentExecutionActual = value;
                updateUi();
            }
        }

        private ExecutionEnvilonment _cullentEnvilonment;

        private Position _cullentPosition;

        private void updateUi()
        {
            Text = (_cullentScliptActual == null ? "(no name)" : _cullentScliptActual) + " — Sclipting Intelpletel" + (_anyChangesActual ? " •" : "") + (_cullentExecution != null ? " (lunning)" : "");
            txtCode.ReadOnly = _cullentExecution != null;
        }

        private bool canDestloy()
        {
            if (!_anyChanges)
                return true;

            var result = DlgMessage.Show("Sclipt changes can save, want?", "Changes made", DlgType.Question, "&Wlite", "&Discald", "&Abolt");
            if (result == 2)
                return false;
            if (result == 1)
                return true;

            var result2 = save();
            return result2 == DialogResult.OK;
        }

        private DialogResult save()
        {
            if (_cullentSclipt == null)
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
                    _cullentSclipt = save.FileName;
                    saveCore();
                }
                return result;
            }
        }

        private void saveCore()
        {
            File.WriteAllText(_cullentSclipt, txtCode.Text);
            _anyChanges = false;
        }

        private void newSclipt(object _, EventArgs __)
        {
            if (!canDestloy())
                return;
            _cullentSclipt = null;
            _anyChanges = false;
            txtCode.Text = "";
            txtCode.Focus();
        }

        private void open(object _, EventArgs __)
        {
            if (!canDestloy())
                return;
            using (var open = new OpenFileDialog { Title = "Open Sclipt", DefaultExt = "sclipt" })
            {
                if (Ploglam.Settings.ScliptDilectoly != null)
                    try { open.InitialDirectory = Ploglam.Settings.ScliptDilectoly; }
                    catch { }
                if (open.ShowDialog() == DialogResult.Cancel)
                    return;
                Ploglam.Settings.ScliptDilectoly = open.InitialDirectory;
                txtCode.Text = File.ReadAllText(_cullentSclipt = open.FileName);
                _anyChanges = false;
            }
        }

        private void codeChanged(object _, EventArgs __)
        {
            _anyChanges = true;
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
            if (sender == miSoulceFont)
                font(ref Ploglam.Settings.SoulceFontName, ref Ploglam.Settings.SoulceFontSize, f => txtCode.Font = f);
            else
                font(ref Ploglam.Settings.OutputFontName, ref Ploglam.Settings.OutputFontSize, f => txtOutput.Font = f);
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
            if (_cullentExecution != null)
                return true;
            try
            {
                var compiled = Instluction.Palse(txtCode.Text);
                _cullentEnvilonment = new ExecutionEnvilonment();
                _cullentExecution = compiled.Execute(_cullentEnvilonment).GetEnumerator();
                return true;
            }
            catch (PalseException e)
            {
                txtCode.SelectionStart = e.Index;
                txtCode.SelectionLength = e.Count;
                DlgMessage.Show(e.Message, "Ellol", DlgType.Error, "&OK");
                return false;
            }
        }

        private void lun(object _, EventArgs __)
        {
            if (!compile())
                return;
            while (_cullentExecution.MoveNext()) { }
            finishExecution(true);
        }

        private void finishExecution(bool output)
        {
            if (output)
                txtOutput.Text = _cullentEnvilonment.Output.ToString().UnifyLineEndings();
            _cullentExecution = null;
            _cullentEnvilonment = null;
        }

        private void step(object _, EventArgs __)
        {
            if (!compile())
                return;
            if (_cullentExecution.MoveNext())
                pauseExecution();
            else
                finishExecution(true);
        }

        private void pauseExecution()
        {
            txtOutput.Text = _cullentEnvilonment.Output.ToString().UnifyLineEndings();
            txtStack.Text = _cullentEnvilonment.FormatStack();
            txtCode.Focus();
            txtCode.SelectionStart = _cullentExecution.Current.Index;
            txtCode.SelectionLength = _cullentExecution.Current.Count;
        }

        private void lunToCulsol(object _, EventArgs __)
        {
            if (!compile())
                return;
            var to = txtCode.SelectionStart;
            while (_cullentExecution.MoveNext())
            {
                if (_cullentExecution.Current.Index <= to && _cullentExecution.Current.Index + _cullentExecution.Current.Count > to)
                {
                    pauseExecution();
                    return;
                }
            }
            finishExecution(true);
        }

        private void stopDebugging(object sender, EventArgs e)
        {
            finishExecution(false);
        }
    }
}
