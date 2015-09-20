using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using EsotericIDE.Hexagony;
using RT.Util;
using RT.Util.Dialogs;
using RT.Util.ExtensionMethods;

namespace EsotericIDE.Languages
{
    sealed class Hexagony : ProgrammingLanguage
    {
        public override string LanguageName { get { return "Hexagony"; } }
        public override string DefaultFileExtension { get { return "hex"; } }

        public override ExecutionEnvironment Compile(string source, string input)
        {
            return new HexagonyEnv(source, input, _settings);
        }

        public override string GetInfo(string source, int cursorPosition)
        {
            if (cursorPosition >= source.Length)
                return "";

            switch (source[cursorPosition])
            {
                case '.': return "No-op.";
                case '@': return "Terminates the program.";
                case ')': return "Increments the current memory edge.";
                case '(': return "Decrements the current memory edge.";
                case '+': return "Sets the current memory edge to the sum of the left and right neighbours.";
                case '-': return "Sets the current memory edge to the difference of the left and right neighbours (left − right).";
                case '*': return "Sets the current memory edge to the product of the left and right neighbours.";
                case ':': return "Sets the current memory edge to the quotient of the left and right neighbours (left ÷ right, rounded towards negative infinity).";
                case '%': return "Sets the current memory edge to the modulo of the left and right neighbours (left % right, the sign of the result is the same as the sign of right).";
                case '~': return "Multiplies the current memory edge by −1.";

                case ',': return "Reads a single character from STDIN and sets the current memory edge to its Unicode codepoint. Returns -1 once EOF is reached.";
                case '?': return "Reads and discards from STDIN until a digit, a - or a + is found. Then reads as many characters as possible to form a valid (signed) decimal integer and sets the current memory edge to its value. Returns 0 once EOF is reached.";
                case ';': return "Interprets the current memory edge as a codepoint and writes the corresponding Unicode character to STDOUT.";
                case '!': return "Writes the decimal representation of the current memory edge to STDOUT.";

                case '_':
                case '|':
                case '/':
                case '\\': return "Mirror.";

                case '<':
                case '>': return "Mirror/branch. 60° right turn if positive, 60° left turn if zero or negative.";

                case '[': return "Switches to the previous instruction pointer (wrapping around from 0 to 5).";
                case ']': return "Switches to the next instruction pointer (wrapping around from 5 to 0).";
                case '#': return "Takes the current memory edge modulo 6 and switches to the instruction pointer with that index.";

                case '{': return "Moves the memory pointer to the left neighbour.";
                case '}': return "Moves the memory pointer to the right neighbour.";
                case '"': return "Moves the memory pointer backwards to the left. This is equivalent to =}=.";
                case '\'': return "Moves the memory pointer backwards to the right. This is equivalent to ={=.";
                case '=': return "Reverses the direction of the memory pointer (this doesn’t affect the current memory edge, but changes which edges are considered the left and right neighbour).";
                case '^': return "Moves the memory pointer to the left neighbour if the current edge is zero or negative and to the right neighbour if it’s positive.";
                case '&': return "Copies the value of left neighbour into the current edge if the current edge is zero or negative and the value of the right neighbour if it’s positive.";

                default:
                    if (source[cursorPosition] >= '0' && source[cursorPosition] <= '9')
                        return "Multiply by 10 and add/subtract {0}.".Fmt(source[cursorPosition]);
                    if ((source[cursorPosition] >= 'a' && source[cursorPosition] <= 'z') || (source[cursorPosition] >= 'A' && source[cursorPosition] <= 'Z'))
                        return "Write ASCII code of {0} (= {1}) to memory.".Fmt(source[cursorPosition], (int) source[cursorPosition]);
                    return "";
            }
        }

        private HexagonySettings _settings = new HexagonySettings();

        public override LanguageSettings Settings
        {
            get { return _settings; }
            set { _settings = value == null ? new HexagonySettings() : (HexagonySettings) value; }
        }

        public override ToolStripMenuItem[] CreateMenus(Func<string> getSelectedText, Action<string> insertText, Func<ExecutionEnvironment> getEnv)
        {
            ToolStripMenuItem valueColorItem = null;
            ToolStripMenuItem annotationColorItem = null;
            ToolStripMenuItem annotationSets = null;

            Action updateEnvironment = () =>
            {
                var env = getEnv() as HexagonyEnv;
                if (env != null)
                    env.UpdateWatch();
            };

            var createAnnotationSet = Ut.Lambda((bool copyFromCurrent) =>
            {
                string newName = null;
                while (true)
                {
                    newName = InputBox.GetLine("Enter new name for this annotation set:", newName, "Create new annotation set", "&OK", "&Cancel");
                    if (newName == null)
                        break;
                    if (!_settings.MemoryAnnotations.ContainsKey(newName))
                    {
                        _settings.MemoryAnnotations[newName] = copyFromCurrent
                            ? new Dictionary<Direction, Dictionary<PointAxial, string>>(_settings.MemoryAnnotations[_settings.LastMemoryAnnotationSet])
                            : new Dictionary<Direction, Dictionary<PointAxial, string>>();
                        _settings.LastMemoryAnnotationSet = newName;
                    }
                    DlgMessage.Show("The specified annotation set name already exists. Annotation set names must be unique.", "Error", DlgType.Error);
                }
            });

            Action updateAnnotationSets = null;
            updateAnnotationSets = () =>
            {
                annotationSets.DropDownItems.Clear();
                foreach (var kvp in _settings.MemoryAnnotations)
                {
                    var setName = kvp.Key;
                    var super = new ToolStripMenuItem(setName);
                    annotationSets.DropDownItems.Add(super);
                    if (setName == _settings.LastMemoryAnnotationSet)
                        super.Checked = true;

                    super.DropDownItems.AddRange(Ut.NewArray(
                        new ToolStripMenuItem("&Switch to this set", null, delegate
                        {
                            _settings.LastMemoryAnnotationSet = setName;
                            updateAnnotationSets();
                            updateEnvironment();
                        }) { Checked = setName == _settings.LastMemoryAnnotationSet },
                        new ToolStripMenuItem("&Rename this set...", null, delegate
                        {
                            var newName = InputBox.GetLine("Enter new name for this annotation set:", setName, "Rename annotation set", "&OK", "&Cancel");
                            if (newName != null && newName != setName)
                            {
                                _settings.MemoryAnnotations[newName] = _settings.MemoryAnnotations[setName];
                                _settings.MemoryAnnotations.Remove(setName);
                                if (_settings.LastMemoryAnnotationSet == setName)
                                    _settings.LastMemoryAnnotationSet = newName;
                                updateAnnotationSets();
                            }
                        }),
                        new ToolStripMenuItem("&Delete this set", null, delegate
                        {
                            _settings.MemoryAnnotations.Remove(setName);
                            _settings.LastMemoryAnnotationSet = _settings.MemoryAnnotations.Keys.FirstOrDefault("(default)");
                            if (!_settings.MemoryAnnotations.ContainsKey(_settings.LastMemoryAnnotationSet))
                                _settings.MemoryAnnotations[_settings.LastMemoryAnnotationSet] = new Dictionary<Direction, Dictionary<PointAxial, string>>();
                            updateEnvironment();
                        })
                    ));
                }
                annotationSets.DropDownItems.Add("-");
                annotationSets.DropDownItems.Add(new ToolStripMenuItem("&Create new annotation set...", null, delegate { createAnnotationSet(false); }));
                annotationSets.DropDownItems.Add(new ToolStripMenuItem("C&opy current annotation set...", null, delegate { createAnnotationSet(true); }));
            };

            var menu = new ToolStripMenuItem("&Memory visualization", null, Ut.NewArray(

                // Unused mnemonics: cdefhijkmqruwxy

                new ToolStripMenuItem("&Background color...", null, delegate { color(getEnv, _settings.MemoryBackgroundColor, c => { _settings.MemoryBackgroundColor = c; }, (c, e) => { e.UpdateWatch(); }); }),
                new ToolStripMenuItem("Grid color (&zeros)...", null, delegate { color(getEnv, _settings.MemoryGridZeroColor, c => { _settings.MemoryGridZeroColor = c; }, (c, e) => { e.UpdateWatch(); }); }),
                new ToolStripMenuItem("&Grid color (non-zeros)...", null, delegate { color(getEnv, _settings.MemoryGridNonZeroColor, c => { _settings.MemoryGridNonZeroColor = c; }, (c, e) => { e.UpdateWatch(); }); }),
                new ToolStripMenuItem("&Pointer color...", null, delegate { color(getEnv, _settings.MemoryPointerColor, c => { _settings.MemoryPointerColor = c; }, (c, e) => { e.UpdateWatch(); }); }),
                new ToolStripMenuItem("&Value font...", null, delegate { font(getEnv, _settings.MemoryValueFont, Color.CornflowerBlue, f => { _settings.MemoryValueFont = f; valueColorItem.Enabled = true; }, (f, e) => { e.UpdateWatch(); }); }),
                (valueColorItem = new ToolStripMenuItem("Va&lue color...", null, delegate { color(getEnv, _settings.MemoryValueFont.Color, c => { _settings.MemoryValueFont = _settings.MemoryValueFont.SetColor(c); }, (c, e) => { e.UpdateWatch(); }); }) { Enabled = _settings.MemoryValueFont != null }),
                new ToolStripMenuItem("Ann&otation font...", null, delegate { font(getEnv, _settings.MemoryAnnotationFont, Color.ForestGreen, f => { _settings.MemoryAnnotationFont = f; annotationColorItem.Enabled = true; }, (f, e) => { e.UpdateWatch(); }); }),
                (annotationColorItem = new ToolStripMenuItem("Anno&tation color...", null, delegate { color(getEnv, _settings.MemoryAnnotationFont.Color, c => { _settings.MemoryAnnotationFont = _settings.MemoryAnnotationFont.SetColor(c); }, (c, e) => { e.UpdateWatch(); }); }) { Enabled = _settings.MemoryAnnotationFont != null }),
                new ToolStripMenuItem("&Annotate current edge...", null, checkDebugging(getEnv, env =>
                {
                    var newAnnotation = InputBox.GetLine("Enter annotation:", env.GetCurrentAnnotation(), "Annotate edge", "&OK", "&Cancel");
                    if (newAnnotation != null)
                    {
                        env.Annotate(newAnnotation);
                        env.UpdateWatch();
                    }
                })),
                (annotationSets = new ToolStripMenuItem("A&nnotation sets")),
                new ToolStripMenuItem("&Save memory as PNG...", null, checkDebugging(getEnv, env =>
                {
                    using (var dlg = new SaveFileDialog { DefaultExt = "png", Filter = "PNG files (*.png)|*.png", OverwritePrompt = true, Title = "Save memory visualization to file" })
                        if (dlg.ShowDialog() == DialogResult.OK)
                            env.SaveMemoryVisualization(dlg.FileName);
                }))
            ));

            updateAnnotationSets();
            return new[] { menu };
        }

        private EventHandler checkDebugging(Func<ExecutionEnvironment> getEnv, Action<HexagonyEnv> action)
        {
            return (_, __) =>
            {
                var env = getEnv() as HexagonyEnv;
                if (env == null || env.State != ExecutionState.Debugging)
                    DlgMessage.Show("Program must be running but stopped in the debugger.", "Error", DlgType.Error);
                else
                    action(env);
            };
        }

        private void color(Func<ExecutionEnvironment> getEnv, Color orig, Action<Color> setColor1, Action<Color, HexagonyEnv> setColor2)
        {
            using (var colorDlg = new ColorDialog { Color = orig })
                if (colorDlg.ShowDialog() == DialogResult.OK)
                {
                    setColor1(colorDlg.Color);
                    var env = getEnv() as HexagonyEnv;
                    if (env != null)
                        setColor2(colorDlg.Color, env);
                }
        }

        private void font(Func<ExecutionEnvironment> getEnv, FontSpec prevFont, Color defaultColor, Action<FontSpec> setFont1, Action<FontSpec, HexagonyEnv> setFont2)
        {
            using (var fontDlg = new FontDialog())
            {
                if (prevFont != null)
                    fontDlg.Font = prevFont.Font;
                if (fontDlg.ShowDialog() == DialogResult.OK)
                {
                    var spec = new FontSpec(fontDlg.Font, defaultColor);
                    setFont1(spec);
                    var env = getEnv() as HexagonyEnv;
                    if (env != null)
                        setFont2(spec, env);
                }
            }
        }
    }
}
