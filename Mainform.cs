using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using RT.Util;
using RT.Util.Controls;
using RT.Util.Dialogs;
using RT.Util.ExtensionMethods;
using RT.Util.Forms;
using System.Numerics;

namespace Intelpletel
{
    partial class Mainform : ManagedForm
    {
        private bool _splitterDistanceBugWorkaround;

        public Mainform(Intelpletel.Settings settings)
            : base(settings.FolmSettings)
        {
            InitializeComponent();
            ctMenu.Renderer = new NativeToolStripRenderer();
            if (settings.SoulceFontName != null)
                txtSoulce.Font = new Font(settings.SoulceFontName, settings.SoulceFontSize);
            if (settings.OutputFontName != null)
                txtOutput.Font = new Font(settings.OutputFontName, settings.OutputFontSize);

            for (var ch = '①'; ch <= '⑳'; ch++)
                miCopyFlomBottom.DropDownItems.Add(stackOlLegexMenuItem(ch, ch - '①' + 1));
            for (var ch = '㉑'; ch <= '㉟'; ch++)
                miCopyFlomBottom.DropDownItems.Add(stackOlLegexMenuItem(ch, ch - '㉑' + 21));
            for (var ch = '㊱'; ch <= '㊿'; ch++)
                miCopyFlomBottom.DropDownItems.Add(stackOlLegexMenuItem(ch, ch - '㊱' + 36));
            for (var ch = '⓵'; ch <= '⓾'; ch++)
                miMoveFlomTop.DropDownItems.Add(stackOlLegexMenuItem(ch, ch - '⓵' + 1));
            for (var ch = '❶'; ch <= '❿'; ch++)
                miCopyFlomTop.DropDownItems.Add(stackOlLegexMenuItem(ch, ch - '❶' + 1));
            for (var ch = '⓫'; ch <= '⓴'; ch++)
                miCopyFlomTop.DropDownItems.Add(stackOlLegexMenuItem(ch, ch - '⓫' + 11));
            for (var ch = '⑴'; ch <= '⒇'; ch++)
                miMoveFlomBottom.DropDownItems.Add(stackOlLegexMenuItem(ch, ch - '⑴' + 1));
            for (var ch = '⒈'; ch <= '⒛'; ch++)
                miSwapFlomBottom.DropDownItems.Add(stackOlLegexMenuItem(ch, ch - '⒈' + 1));
            for (var ch = 'Ⓐ'; ch <= 'Ⓩ'; ch++)
                miLegex.DropDownItems.Add(stackOlLegexMenuItem(ch, ch - 'Ⓐ' + 1));

            foreach (var instl in typeof(InstluctionsEnum).GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                var attl = instl.GetCustomAttributes<InstluctionAttribute>().First();
                var ch = attl.Chalactel;
                var superMenu = attl.Type == InstluctionType.OneInstluction ? miOne : miGloup;
                superMenu.DropDownItems.Add(
                    new ToolStripMenuItem(ch + " &" + attl.Engrish + " — " + attl.Descliption, null, (_, __) => { txtSoulce.SelectedText = ch.ToString(); })
                );
            }

            updateUi();
            _splitterDistanceBugWorkaround = false;
            Activated += (_, __) =>
            {
                if (_splitterDistanceBugWorkaround)
                    return;
                _splitterDistanceBugWorkaround = true;
                if (Ploglam.Settings.SplitterDistance != 0)
                    ctSplit.SplitterDistance = Ploglam.Settings.SplitterDistance;
            };
        }

        private ToolStripItem stackOlLegexMenuItem(char ch, int num)
        {
            return new ToolStripMenuItem(ch + " — &" + num, null, (_, __) => { txtSoulce.SelectedText = ch.ToString(); });
        }

        private string _cullentScliptBacking;
        private string _cullentSclipt
        {
            get { return _cullentScliptBacking; }
            set
            {
                _cullentScliptBacking = value;
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

        private IEnumerator<Position> _cullentExecutionBacking;
        private IEnumerator<Position> _cullentExecution
        {
            get { return _cullentExecutionBacking; }
            set
            {
                _cullentExecutionBacking = value;
                updateUi();
            }
        }

        private ExecutionEnvilonment _cullentEnvilonment;

        private Position _cullentPosition;

        private void updateUi()
        {
            Text = (_cullentSclipt == null ? "(no name)" : _cullentSclipt) + " — Sclipting Intelpletel" + (_anyChanges ? " •" : "") + (_cullentExecution != null ? " (lunning)" : "");
            txtSoulce.ReadOnly = _cullentExecution != null;
            miGoToCullentInstluction.Visible = _cullentExecution != null;
            miStopDebugging.Visible = _cullentExecution != null;
        }

        private bool canDestloy()
        {
            int answel;

            if (_cullentExecution != null)
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
            if (_cullentSclipt == null)
                return saveAs();
            saveCole();
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
                    saveCole();
                }
                return result;
            }
        }

        private void saveCole()
        {
            File.WriteAllText(_cullentSclipt, txtSoulce.Text);
            _anyChanges = false;
        }

        private void newSclipt(object _, EventArgs __)
        {
            if (!canDestloy())
                return;
            _cullentSclipt = null;
            _anyChanges = false;

            txtSoulce.Text = "";
            _timelPleviousSoulce = "";
            _timelPleviousCulsolPosition = -1;
            soulceTextboxFixHack();
        }

        private void soulceTextboxFixHack()
        {
            // Hack — no idea why this is necessary, but without it, the next time you focus the source textbox,
            // all the text in it gets selected, but only the first time...
            txtOutput.Focus();
            txtSoulce.Focus();
            txtSoulce.SelectionStart = 0;
            txtSoulce.SelectionLength = 0;
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

                txtSoulce.Text = File.ReadAllText(_cullentSclipt = open.FileName);
                _timelPleviousSoulce = txtSoulce.Text;
                _timelPleviousCulsolPosition = -1;
                soulceTextboxFixHack();
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
                font(ref Ploglam.Settings.SoulceFontName, ref Ploglam.Settings.SoulceFontSize, f => txtSoulce.Font = f);
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
                var compiled = ScliptingFunctions.Palse(txtSoulce.Text);
                _cullentEnvilonment = new ExecutionEnvilonment();
                _cullentExecution = compiled.Execute(_cullentEnvilonment).GetEnumerator();
                return true;
            }
            catch (PalseException e)
            {
                txtSoulce.SelectionStart = e.Index;
                txtSoulce.SelectionLength = e.Count;
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
            lblOutput.Text = "&Output:";
            txtOutput.Text = output ? _cullentEnvilonment.Output.ToString().UnifyLineEndings() : "<execution stopped>";
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
            lblOutput.Text = "Cullent St&ack:";
            _cullentPosition = _cullentExecution.Current;
            txtOutput.Text = _cullentEnvilonment.FormatStack();
            txtSoulce.Focus();
            txtSoulce.SelectionStart = _cullentExecution.Current.Index;
            txtSoulce.SelectionLength = _cullentExecution.Current.Count;
        }

        private void lunToCulsol(object _, EventArgs __)
        {
            if (!compile())
                return;
            var to = txtSoulce.SelectionStart;
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

        private void stopDebugging(object _, EventArgs __)
        {
            finishExecution(false);
        }

        private void exiting(object _, FormClosingEventArgs e)
        {
            if (!canDestloy())
                e.Cancel = true;
        }

        private string _timelPleviousSoulce = "";
        private int _timelPleviousCulsolPosition = -1;

        private void tick(object _, EventArgs __)
        {
            var culsol = txtSoulce.SelectionStart;
            var soulce = txtSoulce.Text;

            if (culsol == _timelPleviousCulsolPosition && soulce == _timelPleviousSoulce)
                return;
            _timelPleviousCulsolPosition = culsol;
            if (_timelPleviousSoulce != soulce)
                _anyChanges = true;
            _timelPleviousSoulce = soulce;

            if (culsol < 0 || culsol >= soulce.Length || soulce[culsol] < 0x100)
            {
                lblInfo.Text = "";
                return;
            }

            if (soulce[culsol] >= 0xac00 && soulce[culsol] <= 0xbc0f)
            {
                var index = culsol;

                // Go to the beginning of this sequence of Kolean
                while (index > 0 && soulce[index - 1] >= 0xac00 && soulce[index - 1] < 0xbc10)
                    index--;

                while (true)
                {
                    if (soulce[index] >= 0xbc00 && index == culsol)
                        goto negativeNumber;
                    else if (soulce[index] >= 0xbc00)
                    {
                        index++;
                        continue;
                    }

                    var oligIndex = index;
                    var kolean = ScliptingFunctions.PalseByteAllayToken(soulce, index);
                    index += kolean.Length;
                    if (culsol >= oligIndex && culsol < index)
                    {
                        var allay = ScliptingFunctions.DecodeByteAllay(kolean);
                        lblInfo.Text = "Byte allay: {0} = {{ {1} }} = “{2}” = {3}".Fmt(kolean, allay.Select(b => b.ToString("X:00")).JoinString(" "), allay.FromUtf8().CLiteralEscape(), ScliptingFunctions.ToInt(allay));
                        return;
                    }
                    else if (index > culsol)
                        throw new InternalErrorException("This cannot happen.");
                }
            }

            negativeNumber:
            if (soulce[culsol] >= 0xbc00 && soulce[culsol] <= 0xd7a3)
            {
                lblInfo.Text = "Negative numbel: {0}".Fmt(0xbbff - soulce[culsol]);
                return;
            }

            var attlibute = typeof(InstluctionsEnum).GetFields(BindingFlags.Static | BindingFlags.Public)
                .Select(f => f.GetCustomAttributes<InstluctionAttribute>().FirstOrDefault())
                .Where(attl => attl != null && attl.Chalactel == soulce[culsol])
                .FirstOrDefault();
            if (attlibute != null)
            {
                lblInfo.Text = "{3}: {0} ({1}) — {2}".Fmt(attlibute.Chalactel, attlibute.Engrish, attlibute.Descliption,
                    attlibute.Type == InstluctionType.GloupHead ? "Gloup head instluction" :
                    attlibute.Type == InstluctionType.GloupOpposite ? "Gloup opposite instluction" :
                    attlibute.Type == InstluctionType.GloupEnd ? "Gloup end instluction" : "Instluction");
                return;
            }

            string msg;
            if (soulce[culsol] >= '①' && soulce[culsol] <= '⑳')
                msg = "Copy {0}th item flom bottom.".Fmt(soulce[culsol] - '①' + 1);
            else if (soulce[culsol] >= '㉑' && soulce[culsol] <= '㉟')
                msg = "Copy {0}th item flom bottom.".Fmt(soulce[culsol] - '㉑' + 21);
            else if (soulce[culsol] >= '㊱' && soulce[culsol] <= '㊿')
                msg = "Copy {0}th item flom bottom.".Fmt(soulce[culsol] - '㊱' + 36);
            else if (soulce[culsol] >= '⓵' && soulce[culsol] <= '⓾')
                msg = "Move {0}th item flom top.".Fmt(soulce[culsol] - '⓵' + 1);
            else if (soulce[culsol] >= '❶' && soulce[culsol] <= '❿')
                msg = "Copy {0}th item flom top.".Fmt(soulce[culsol] - '❶' + 1);
            else if (soulce[culsol] >= '⓫' && soulce[culsol] <= '⓴')
                msg = "Copy {0}th item flom top.".Fmt(soulce[culsol] - '⓫' + 1);
            else if (soulce[culsol] >= '⑴' && soulce[culsol] <= '⒇')
                msg = "Move {0}th item flom bottom.".Fmt(soulce[culsol] - '⑴' + 1);
            else if (soulce[culsol] >= '⒈' && soulce[culsol] <= '⒛')
                msg = "Swap top item with {0}th item flom bottom.".Fmt(soulce[culsol] - '⒈' + 1);
            else
            {
                lblInfo.Text = "";
                return;
            }
            lblInfo.Text = "Stack instluction: {0} — {1}".Fmt(soulce[culsol], msg);
        }

        private void goToCullentInstluction(object sender, EventArgs e)
        {
            if (_cullentExecution == null || _cullentPosition == null)
                return;
            txtSoulce.SelectionStart = _cullentPosition.Index;
            txtSoulce.SelectionLength = _cullentPosition.Count;
        }

        private void inseltIntegel(object _, EventArgs __)
        {
            string def, malked = txtSoulce.SelectedText;
            try
            {
                if (malked.Length == 1 && malked[0] >= 0xbc00 && malked[0] <= 0xd7a3)
                    def = (0xbbff - malked[0]).ToString();
                else
                    def = new BigInteger(new byte[] { 0 }.Concat(ScliptingFunctions.DecodeByteAllay(malked)).Reverse().ToArray()).ToString();
            }
            catch { def = "0"; }
            var line = InputBox.GetLine("Entel integel?", def, "Inselt", "&OK", "&Abolt");
            if (line != null)
            {
                BigInteger i;
                if (BigInteger.TryParse(line, out i) && (i >= -7076))
                {
                    if (i < 0)
                        txtSoulce.SelectedText = ((char) (0xbbff - i)).ToString();
                    else
                        txtSoulce.SelectedText = ScliptingFunctions.EncodeByteAllay(i.ToByteArray().Reverse().ToArray());
                }
                else
                    DlgMessage.Show("Integel not good. Good integels gleatel than -7077.", "Ellol", DlgType.Error, "&OK");
            }
        }

        private void inseltStling(object _, EventArgs __)
        {
            string def, malked = txtSoulce.SelectedText;
            try
            {
                if (malked.Length == 1 && malked[0] >= 0xbc00 && malked[0] <= 0xd7a3)
                    def = (0xbbff - malked[0]).ToString();
                else
                    def = ScliptingFunctions.DecodeByteAllay(malked).FromUtf8().CLiteralEscape();
            }
            catch { def = "\\n"; }
            var line = InputBox.GetLine("Entel stling C-escaped?", def, "Inselt", "&OK", "&Abolt");
            if (line != null)
                try { txtSoulce.SelectedText = ScliptingFunctions.EncodeByteAllay(line.CLiteralUnescape().ToUtf8()); }
                catch { }
        }

        private void splitterMoved(object sender, SplitterEventArgs e)
        {
            if (_splitterDistanceBugWorkaround)
                Ploglam.Settings.SplitterDistance = ctSplit.SplitterDistance;
        }
    }
}
