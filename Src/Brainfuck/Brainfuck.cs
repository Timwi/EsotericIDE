using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using EsotericIDE.Brainfuck;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace EsotericIDE.Languages
{
    sealed partial class Brainfuck : ProgrammingLanguage
    {
        public override string LanguageName { get { return "Brainfuck"; } }
        public override string DefaultFileExtension { get { return "bf"; } }

        public override ExecutionEnvironment Compile(string source, string input)
        {
            var inputQ = new Queue<BigInteger>();
            switch (_settings.InputType)
            {
                case IOType.Numbers:
                    try
                    {
                        inputQ.EnqueueRange(input.Split(',').Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => BigInteger.Parse(s)));
                    }
                    catch (FormatException)
                    {
                        throw new Exception("Please provide the input as a comma-separated sequence of integers (or change the semantics in the Semantics menu).");
                    }
                    break;
                case IOType.Characters:
                    inputQ.EnqueueRange(input.Select(ch => (BigInteger) (int) ch));
                    break;
                default:
                    throw new InvalidOperationException();
            }
            return BrainfuckEnv.GetEnvironment(_settings.CellType, source, inputQ, _settings.OutputType);
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

        public override ToolStripMenuItem[] CreateMenus(IIde ide)
        {
            var menuItems = new List<Tuple<ToolStripMenuItem, Func<bool>>>();

            var update = Ut.Lambda(() =>
            {
                foreach (var tuple in menuItems)
                    tuple.Item1.Checked = tuple.Item2();
            });

            var createItem = Ut.Lambda((string label, Action action, Func<bool> checkedFunc) =>
            {
                var menuItem = new ToolStripMenuItem(label, null, delegate { action(); update(); });
                menuItems.Add(new Tuple<ToolStripMenuItem, Func<bool>>(menuItem, checkedFunc));
                return menuItem;
            });

            var ret = Ut.NewArray<ToolStripMenuItem>(
                new ToolStripMenuItem("&Semantics", null,
                    createItem("Input as &numbers", () => { _settings.InputType = IOType.Numbers; }, () => _settings.InputType == IOType.Numbers),
                    createItem("Input as &characters", () => { _settings.InputType = IOType.Characters; }, () => _settings.InputType == IOType.Characters),
                    new ToolStripSeparator(),
                    createItem("Output as nu&mbers", () => { _settings.OutputType = IOType.Numbers; }, () => _settings.OutputType == IOType.Numbers),
                    createItem("Output as c&haracters", () => { _settings.OutputType = IOType.Characters; }, () => _settings.OutputType == IOType.Characters),
                    new ToolStripSeparator(),
                    createItem("Cells are &bytes", () => { _settings.CellType = CellType.Bytes; }, () => _settings.CellType == CellType.Bytes),
                    createItem("Cells are &signed 32-bit integers", () => { _settings.CellType = CellType.Int32s; }, () => _settings.CellType == CellType.Int32s),
                    createItem("Cells are &unsigned 32-bit integers", () => { _settings.CellType = CellType.UInt32s; }, () => _settings.CellType == CellType.UInt32s),
                    createItem("Cells are &arbitrary-size integers", () => { _settings.CellType = CellType.BigInts; }, () => _settings.CellType == CellType.BigInts)
                )
            );
            update();
            return ret;
        }

        private BrainfuckSettings _settings = new BrainfuckSettings();

        public override LanguageSettings Settings
        {
            get => _settings;
            set { _settings = (BrainfuckSettings) value; }
        }
    }
}
