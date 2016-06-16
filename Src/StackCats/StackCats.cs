using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using EsotericIDE.StackCats;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace EsotericIDE.Languages
{
    sealed class StackCats : ProgrammingLanguage
    {
        public override string LanguageName { get { return "Stack Cats"; } }
        public override string DefaultFileExtension { get { return "sks"; } }
        public override string GetInfo(string source, int cursorPosition)
        {
            if (cursorPosition < 0 || cursorPosition >= source.Length)
                return "";

            switch (source[cursorPosition])
            {
                case '(': return "If the top is zero or negative, control flow continues after the matching ')'.";
                case ')': return "If the top is zero or negative, control flow continues after the matching '('.";
                case '{': return "Remembers the top. The matching '}' compares this value against its top.";
                case '}': return "If the top differs from the value remembered at the matching '{', control flow continues after the matching '{' (without remembering a new value).";
                case '-': return "Negate the top (i.e. multiply by −1).";
                case '!': return "Take the bitwise NOT of the top (this is equivalent to incrementing and negating).";
                case '*': return "Toggle the least-significant bit of the top. In other words, compute x XOR 1.";
                case '_': return "Pop a, pop b, push b, push b − a.";
                case '^': return "Pop a, pop b, push b, push b XOR a.";
                case ':': return "Swap the top two elements of the stack.";
                case '+': return "Swap the top and third elements of the stack.";
                case '=': return "Swap the top elements of the two adjacent stacks.";
                case '|': return "Reverse all values on the stack down to (and excluding) the first zero from the top.";
                case 'T': return "If the top is non-zero, reverse the entire stack (down to and including the bottommost non-zero value).";
                case '<': return "Move the tape head left one stack.";
                case '>': return "Move the tape head right one stack.";
                case '[': return "Move the tape head left one stack, taking the top with it.";
                case ']': return "Move the tape head right one stack, taking the top with it.";
                case 'I': return "If the top is negative, do [-, if it is positive, do ]-, if it is zero, do nothing.";
                case '/': return "Swap the current stack with the stack to the left, and move the tape head left.";
                case '\\': return "Swap the current stack with the stack to the right, and move the tape head right.";
                case 'X': return "Swap the stacks left and right of the current stack.";
                default: return "";
            }
        }

        public override ExecutionEnvironment Compile(string source, string input)
        {
            source = Regex.Replace(source, @"[\r\n].*", "", RegexOptions.Singleline);
            var origSourceLength = source.Length;
            var translateIndex = Ut.Lambda((int ix) =>
            {
                switch (_settings.ImplicitlyMirror)
                {
                    case MirrorType.Left:
                        return ix < origSourceLength ? origSourceLength - 1 - ix : ix;
                    case MirrorType.Right:
                        return ix > origSourceLength ? origSourceLength - 1 - ix : ix;
                    default:
                        return ix;
                }
            });

            switch (_settings.ImplicitlyMirror)
            {
                case MirrorType.Left:
                    if (origSourceLength == 0 || source[0] != mirror(source[0]))
                        throw new CompileException(@"Compile error: When implicit left-mirroring is enabled, the first instruction must be self-symmetric.", 0, origSourceLength == 0 ? 0 : 1);
                    source = source.Substring(1, source.Length - 1).Select(mirror).Reverse().JoinString() + source;
                    break;
                case MirrorType.Right:
                    if (origSourceLength == 0 || source[origSourceLength - 1] != mirror(source[origSourceLength - 1]))
                        throw new CompileException(@"Compile error: When implicit right-mirroring is enabled, the last instruction must be self-symmetric.", origSourceLength == 0 ? 0 : origSourceLength - 1, origSourceLength == 0 ? 0 : 1);
                    source += source.Substring(0, source.Length - 1).Select(mirror).Reverse().JoinString();
                    break;
                case MirrorType.None:
                    for (int i = 0; i < source.Length / 2; i++)
                        if (source[i] != mirror(source[source.Length - 1 - i]))
                            throw new CompileException(@"Compile error: The program must be a mirror image of itself.", i, source.Length - 2 * i);
                    break;
            }

            var jumpTable = new int[source.Length];
            var jumpStack = new Stack<Tuple<int, char>>();
            var str = @"(){}";
            for (int i = 0; i < source.Length; i++)
            {
                var pos = str.IndexOf(source[i]);
                if (pos == -1)  // not a parenthesis or brace that needs matching
                    continue;
                if ((pos & 1) == 0) // open
                    jumpStack.Push(Tuple.Create(i, str[pos | 1]));
                else    // close
                {
                    if (jumpStack.Count == 0)
                        throw new CompileException($"Compile error: Unmatched '{source[i]}'.", translateIndex(i), 1);
                    var tup = jumpStack.Pop();
                    if (tup.Item2 != source[i])
                        throw new CompileException($"Compile error: '{str[pos ^ 1]}' at index {tup.Item1} does not match '{source[i]}' at index {i}.", translateIndex(tup.Item1), 1);
                    jumpTable[tup.Item1] = i;
                    jumpTable[i] = tup.Item1;
                }
            }

            if (jumpStack.Count > 0)
                jumpStack.Pop().Item1.Apply(index => { throw new CompileException($"Compile error: '{source[index]}' is unmatched.", translateIndex(index), 1); });

            return new SCEnvironment(source, jumpTable, _settings.InputType == IOType.Bytes
                ? input.ToUtf8().Select(b => (BigInteger) b)
                : Regex.Matches(input, @"[-+]?[0-9]+").Cast<Match>().Select(m => BigInteger.Parse(m.Value)));
        }

        private char mirror(char ch)
        {
            var str = @"(){}[]<>\/";
            var pos = str.IndexOf(ch);
            return pos == -1 ? ch : str[pos ^ 1];
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
                    createItem("Input as &bytes", () => { _settings.InputType = IOType.Bytes; }, () => _settings.InputType == IOType.Bytes),
                    createItem("Input as &numbers", () => { _settings.InputType = IOType.Numbers; }, () => _settings.InputType == IOType.Numbers),
                    new ToolStripSeparator(),
                    createItem("Output as b&ytes", () => { _settings.OutputType = IOType.Bytes; }, () => _settings.OutputType == IOType.Bytes),
                    createItem("Output as n&umbers", () => { _settings.OutputType = IOType.Numbers; }, () => _settings.OutputType == IOType.Numbers),
                    new ToolStripSeparator(),
                    createItem("No implicit &mirroring", () => { _settings.ImplicitlyMirror = MirrorType.None; }, () => _settings.ImplicitlyMirror == MirrorType.None),
                    createItem("Implicitly mirror source to the &left (e.g. *|= becomes =|*|=)", () => { _settings.ImplicitlyMirror = MirrorType.Left; }, () => _settings.ImplicitlyMirror == MirrorType.Left),
                    createItem("Implicitly mirror source to the &right (e.g. *|= becomes *|=|*)", () => { _settings.ImplicitlyMirror = MirrorType.Right; }, () => _settings.ImplicitlyMirror == MirrorType.Right)
                )
            );
            update();
            return ret;
        }

        private StackCatsSettings _settings = new StackCatsSettings();

        public override LanguageSettings Settings
        {
            get { return _settings; }
            set { _settings = (StackCatsSettings) value; }
        }
    }
}
