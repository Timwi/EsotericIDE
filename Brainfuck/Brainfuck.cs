using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace EsotericIDE.Languages
{
    sealed partial class Brainfuck : ProgrammingLanguage
    {
        private enum ioType
        {
            Numbers,
            Characters
        }

        private enum cellType
        {
            Bytes,
            Int32s,
            UInt32s,
            BigInts
        }

        public override string LanguageName { get { return "Brainfuck"; } }
        public override string DefaultFileExtension { get { return "bf"; } }

        public override ExecutionEnvironment Compile(string source, string input)
        {
            var inputQ = new Queue<BigInteger>();
            switch (_settings.InputType)
            {
                case ioType.Numbers:
                    inputQ.EnqueueRange(input.Split(',').Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => BigInteger.Parse(s)));
                    break;
                case ioType.Characters:
                    inputQ.EnqueueRange(input.Select(ch => (BigInteger) (int) ch));
                    break;
                default:
                    throw new InvalidOperationException();
            }

            switch (_settings.CellType)
            {
                case cellType.Bytes:
                    return new brainfuckEnvironmentBytes(source, inputQ, _settings.OutputType);
                case cellType.Int32s:
                    return new brainfuckEnvironmentInt32(source, inputQ, _settings.OutputType);
                case cellType.UInt32s:
                    return new brainfuckEnvironmentUInt32(source, inputQ, _settings.OutputType);
                case cellType.BigInts:
                    return new brainfuckEnvironmentBigInt(source, inputQ, _settings.OutputType);
                default:
                    throw new InvalidOperationException();
            }
        }

        public override string GetInfo(string source, int cursorPosition)
        {
            if (cursorPosition < 0 || cursorPosition >= source.Length)
                return "";
            switch (source[cursorPosition])
            {
                case '[': return "[: Starts a loop. Jumps to after the matching ‘]’ if the current cell is zero.";
                case ']': return "]: Ends a loop. Jumps to the matching ‘[’ if the current cell is non-zero.";
                case '+': return "+: Increments the value in the current cell.";
                case '-': return "-: Decrements the value in the current cell.";
                case '<': return "<: Moves the pointer one cell to the left.";
                case '>': return ">: Moves the pointer one cell to the right.";
                case '.': return ".: Outputs the contents of the current cell.";
                case ',': return ",: Inputs a value into the current cell.";
            }
            return "";
        }

        public override ToolStripMenuItem[] CreateMenus(Func<string> getSelectedText, Action<string> insertText)
        {
            ToolStripMenuItem[] menus = null;

            var update = Ut.Lambda(() =>
            {
                foreach (var menu in menus.First().DropDownItems.OfType<ToolStripMenuItem>())
                    if (menu.Tag is Func<bool>)
                        menu.Checked = ((Func<bool>) menu.Tag)();
            });

            var createItem = Ut.Lambda((string label, Action action, Func<bool> func) => new ToolStripMenuItem(label, null, (s, e) => { action(); update(); }) { Tag = func });

            menus = Ut.NewArray<ToolStripMenuItem>(
                new ToolStripMenuItem("&Semantics", null,
                    createItem("Input as &numbers", () => { _settings.InputType = ioType.Numbers; }, () => _settings.InputType == ioType.Numbers),
                    createItem("Input as &characters", () => { _settings.InputType = ioType.Characters; }, () => _settings.InputType == ioType.Characters),
                    new ToolStripSeparator(),
                    createItem("Output as nu&mbers", () => { _settings.OutputType = ioType.Numbers; }, () => _settings.OutputType == ioType.Numbers),
                    createItem("Output as c&haracters", () => { _settings.OutputType = ioType.Characters; }, () => _settings.OutputType == ioType.Characters),
                    new ToolStripSeparator(),
                    createItem("Cells are &bytes", () => { _settings.CellType = cellType.Bytes; }, () => _settings.CellType == cellType.Bytes),
                    createItem("Cells are &signed 32-bit integers", () => { _settings.CellType = cellType.Int32s; }, () => _settings.CellType == cellType.Int32s),
                    createItem("Cells are &unsigned 32-bit integers", () => { _settings.CellType = cellType.UInt32s; }, () => _settings.CellType == cellType.UInt32s),
                    createItem("Cells are &arbitrary-size integers", () => { _settings.CellType = cellType.BigInts; }, () => _settings.CellType == cellType.BigInts)
                )
            );
            update();
            return menus;
        }
    }
}
