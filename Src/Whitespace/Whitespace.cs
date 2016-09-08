using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using EsotericIDE.Whitespace;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace EsotericIDE.Languages
{
    sealed class Whitespace : ProgrammingLanguage
    {
        public override string LanguageName { get { return "Whitespace"; } }
        public override string DefaultFileExtension { get { return "ws"; } }

        public override ExecutionEnvironment Compile(string source, string input)
        {
            return new WhitespaceEnv(source, input, _settings.NumberInputSemantics, _settings.OutputSemantics);
        }

        public override string GetInfo(string source, int cursorPosition)
        {
            try
            {
                WhitespaceEnv.Parse(source, cursorPosition);
                return "";
            }
            catch (ParseInfoException pie)
            {
                const string lf = "⏎";
                const string tab = "⭾";
                const string space = "␣";

                var attr = pie.Instruction.GetCustomAttribute<InstructionAttribute>();
                return attr.Instr.Select(ch => ch == ' ' ? space : ch == '\n' ? lf : tab).JoinString() +
                    pie.RawArg.NullOr(argBits => " " + argBits.Select(b => b ? tab : space).JoinString() + lf) + " — " +
                    pie.Instruction +
                    pie.NumberArg.NullOr(num => " " + num) +
                    pie.LabelArg.NullOr(lbl => $@" ""{lbl}""") + " — " +
                    attr.Explain.Fmt(
                        pie.NumberArg,
                        pie.NumberArg.NullOr(num => (num % 100) / 10 == 1 ? "th" : num % 10 == 1 ? "st" : num % 10 == 2 ? "nd" : num % 10 == 3 ? "rd" : "th"),
                        pie.LabelArg.NullOr(lbl => $@"""{lbl}"""));
            }
            catch (Exception e)
            {
                return e.Message;
            }
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

            var ret = Ut.NewArray(
                new ToolStripMenuItem("&Semantics", null,
                    createItem("Input &numbers line-wise, 0 if not a number", () => { _settings.NumberInputSemantics =  NumberInputSemantics.LinewiseLenient; }, () => _settings.NumberInputSemantics == NumberInputSemantics.LinewiseLenient),
                    createItem("Input n&umbers line-wise, error if not a number", () => { _settings.NumberInputSemantics = NumberInputSemantics.LinewiseStrict; }, () => _settings.NumberInputSemantics == NumberInputSemantics.LinewiseStrict),
                    createItem("Input nu&mbers by skipping non-numerical text, error at EOF", () => { _settings.NumberInputSemantics = NumberInputSemantics.Minimal; }, () => _settings.NumberInputSemantics == NumberInputSemantics.Minimal),
                    new ToolStripSeparator(),
                    createItem("Output &characters as bytes", () => { _settings.OutputSemantics = CharacterSemantics.Bytewise; }, () => _settings.OutputSemantics == CharacterSemantics.Bytewise),
                    createItem("Output c&haracters as Unicode", () => { _settings.OutputSemantics = CharacterSemantics.Unicode; }, () => _settings.OutputSemantics == CharacterSemantics.Unicode)
                )
            );
            update();
            return ret;
        }

        private WhitespaceSettings _settings = new WhitespaceSettings();

        public override LanguageSettings Settings
        {
            get { return _settings; }
            set { _settings = (WhitespaceSettings) value; }
        }
    }
}
