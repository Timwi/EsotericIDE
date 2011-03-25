using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Windows.Forms;
using RT.Util;
using RT.Util.Dialogs;
using RT.Util.ExtensionMethods;
using RT.Util.Lingo;

namespace EsotericIDE.Sclipting
{
    sealed class ScliptingLanguage : ProgrammingLanguage
    {
        public override string DefaultFileExtension { get { return ".sclipt"; } }

        private Translation _tr;

        public ScliptingLanguage()
        {
            var language = Program.Settings.Language;
            _tr = Lingo.LoadTranslationOrDefault<Translation>("EsotericIDE", ref language);
        }

        private static Dictionary<char, ScliptingInstruction> _instructions;
        private static Dictionary<char, InstructionType> _instructionTypes;

        public override ExecutionEnvironment StartDebugging(string source)
        {
            return new ScliptingExecutionEnvironment(source, _tr, this);
        }

        public override string GetInfo(string source, int cursorPos)
        {
            if (cursorPos < 0 || cursorPos >= source.Length || source[cursorPos] < 0x100)
                return "";

            if (source[cursorPos] >= 0xac00 && source[cursorPos] <= 0xbc0f)
            {
                var index = cursorPos;

                // Go to the beginning of this sequence of Hangeul
                while (index > 0 && source[index - 1] >= 0xac00 && source[index - 1] < 0xbc10)
                    index--;

                while (true)
                {
                    if (source[index] >= 0xbc00 && index == cursorPos)
                        goto negativeNumber;
                    else if (source[index] >= 0xbc00)
                    {
                        index++;
                        continue;
                    }

                    var origIndex = index;
                    var hangeul = ParseByteArrayToken(source, index);
                    index += hangeul.Length;
                    if (cursorPos >= origIndex && cursorPos < index)
                    {
                        try
                        {
                            var array = DecodeByteArray(hangeul);
                            return "{4} {0} = {{ {1} }} = “{2}” = {3}".Fmt(hangeul, array.Select(b => b.ToString("X2")).JoinString(" "), array.FromUtf8().CLiteralEscape(), ToInt(array), _tr.ByteArray);
                        }
                        catch (ParseException pe)
                        {
                            return "Error while decoding byte array: " + pe.Message;
                        }
                    }
                    else if (index > cursorPos)
                        throw new InternalErrorException("This cannot happen.");
                }
            }

            negativeNumber:
            if (source[cursorPos] >= 0xbc00 && source[cursorPos] <= 0xd7a3)
                return "{1} {0}".Fmt(0xbbff - source[cursorPos], _tr.NegativeNumber);

            var attribute = typeof(ScliptingInstruction).GetFields(BindingFlags.Static | BindingFlags.Public)
                .Select(f => f.GetCustomAttributes<InstructionAttribute>().FirstOrDefault())
                .Where(attr => attr != null && attr.Character == source[cursorPos])
                .FirstOrDefault();
            if (attribute != null)
                return "{3}: {0} ({1}) — {2}".Fmt(attribute.Character, attribute.Engrish, attribute.Description,
                    attribute.Type == InstructionType.BlockHead ? _tr.BlockHeadInstruction :
                    attribute.Type == InstructionType.BlockElse ? _tr.BlockElseInstruction :
                    attribute.Type == InstructionType.BlockEnd ? _tr.BlockEndInstruction : _tr.SingularInstruction);

            string description;
            string instructionType = _tr.StackInstruction.Translation;
#warning TODO: Lingo and ordinal numbers?
            if (source[cursorPos] >= '①' && source[cursorPos] <= '⑳')
                description = "Copy {0}th item from bottom.".Fmt(source[cursorPos] - '①' + 1);
            else if (source[cursorPos] >= '㉑' && source[cursorPos] <= '㉟')
                description = "Copy {0}th item from bottom.".Fmt(source[cursorPos] - '㉑' + 21);
            else if (source[cursorPos] >= '㊱' && source[cursorPos] <= '㊿')
                description = "Copy {0}th item from bottom.".Fmt(source[cursorPos] - '㊱' + 36);
            else if (source[cursorPos] >= '⓵' && source[cursorPos] <= '⓾')
                description = "Move {0}th item from top.".Fmt(source[cursorPos] - '⓵' + 1);
            else if (source[cursorPos] >= '❶' && source[cursorPos] <= '❿')
                description = "Copy {0}th item from top.".Fmt(source[cursorPos] - '❶' + 1);
            else if (source[cursorPos] >= '⓫' && source[cursorPos] <= '⓴')
                description = "Copy {0}th item from top.".Fmt(source[cursorPos] - '⓫' + 1);
            else if (source[cursorPos] >= '⑴' && source[cursorPos] <= '⒇')
                description = "Move {0}th item from bottom.".Fmt(source[cursorPos] - '⑴' + 1);
            else if (source[cursorPos] >= '⒈' && source[cursorPos] <= '⒛')
                description = "Swap top item with {0}th item from bottom.".Fmt(source[cursorPos] - '⒈' + 1);
            else if (source[cursorPos] >= 'Ⓐ' && source[cursorPos] <= 'Ⓩ')
            {
                description = "Push content of the {0}th regex capturing parenthesis.".Fmt(source[cursorPos] - 'Ⓐ' + 1);
                instructionType = _tr.RegexInstruction.Translation;
            }
            else
                return "";
            return "{2} {0} — {1}".Fmt(source[cursorPos], description, instructionType);
        }

        public override void InitialiseInsertMenu(ToolStripMenuItem mnuInsert, Func<string> getSelectedText, Action<string> insertText)
        {
            var miInsertSingularInstruction = new ToolStripMenuItem();
            var miInsertBlockInstruction = new ToolStripMenuItem();
            var miInsertStackInstruction = new ToolStripMenuItem();
            var miInsertStackInstructionCopyFromBottom = new ToolStripMenuItem();
            var miInsertStackInstructionMoveFromBottom = new ToolStripMenuItem();
            var miInsertStackInstructionSwapFromBottom = new ToolStripMenuItem();
            var miInsertStackInstructionCopyFromTop = new ToolStripMenuItem();
            var miInsertStackInstructionMoveFromTop = new ToolStripMenuItem();
            var miInsertRegexInstruction = new ToolStripMenuItem();
            var miInsertInteger = new ToolStripMenuItem();
            var miInsertString = new ToolStripMenuItem();
            var miInsertByteArray = new ToolStripMenuItem();

            miInsertSingularInstruction.Text = _tr.InsertMenu.SingularInstruction;
            miInsertBlockInstruction.Text = _tr.InsertMenu.BlockInstruction;
            miInsertStackInstruction.Text = _tr.InsertMenu.StackInstruction;
            miInsertStackInstructionCopyFromBottom.Text = _tr.InsertMenu.StackInstructionCopyFromBottom;
            miInsertStackInstructionMoveFromBottom.Text = _tr.InsertMenu.StackInstructionMoveFromBottom;
            miInsertStackInstructionSwapFromBottom.Text = _tr.InsertMenu.StackInstructionSwapFromBottom;
            miInsertStackInstructionCopyFromTop.Text = _tr.InsertMenu.StackInstructionCopyFromTop;
            miInsertStackInstructionMoveFromTop.Text = _tr.InsertMenu.StackInstructionMoveFromTop;
            miInsertRegexInstruction.Text = _tr.InsertMenu.RegexInstruction;
            miInsertInteger.Text = _tr.InsertMenu.Integer;
            miInsertString.Text = _tr.InsertMenu.String;
            miInsertByteArray.Text = _tr.InsertMenu.ByteArray;

            miInsertInteger.Click += (_, __) => { insertInteger(getSelectedText, insertText); };
            miInsertString.Click += (_, __) => { insertString(getSelectedText, insertText); };
            miInsertByteArray.Click += (_, __) => { insertByteArray(getSelectedText, insertText); };

            mnuInsert.DropDownItems.AddRange(new ToolStripItem[] { miInsertSingularInstruction, miInsertBlockInstruction, miInsertStackInstruction, miInsertRegexInstruction, miInsertInteger, miInsertString, miInsertByteArray });
            miInsertStackInstruction.DropDownItems.AddRange(new ToolStripItem[] { miInsertStackInstructionCopyFromBottom, miInsertStackInstructionMoveFromBottom, miInsertStackInstructionSwapFromBottom, miInsertStackInstructionCopyFromTop, miInsertStackInstructionMoveFromTop });

            for (var ch = '①'; ch <= '⑳'; ch++)
                miInsertStackInstructionCopyFromBottom.DropDownItems.Add(stackOrRegexMenuItem(ch, ch - '①' + 1, insertText));
            for (var ch = '㉑'; ch <= '㉟'; ch++)
                miInsertStackInstructionCopyFromBottom.DropDownItems.Add(stackOrRegexMenuItem(ch, ch - '㉑' + 21, insertText));
            for (var ch = '㊱'; ch <= '㊿'; ch++)
                miInsertStackInstructionCopyFromBottom.DropDownItems.Add(stackOrRegexMenuItem(ch, ch - '㊱' + 36, insertText));
            for (var ch = '⓵'; ch <= '⓾'; ch++)
                miInsertStackInstructionMoveFromTop.DropDownItems.Add(stackOrRegexMenuItem(ch, ch - '⓵' + 1, insertText));
            for (var ch = '❶'; ch <= '❿'; ch++)
                miInsertStackInstructionCopyFromTop.DropDownItems.Add(stackOrRegexMenuItem(ch, ch - '❶' + 1, insertText));
            for (var ch = '⓫'; ch <= '⓴'; ch++)
                miInsertStackInstructionCopyFromTop.DropDownItems.Add(stackOrRegexMenuItem(ch, ch - '⓫' + 11, insertText));
            for (var ch = '⑴'; ch <= '⒇'; ch++)
                miInsertStackInstructionMoveFromBottom.DropDownItems.Add(stackOrRegexMenuItem(ch, ch - '⑴' + 1, insertText));
            for (var ch = '⒈'; ch <= '⒛'; ch++)
                miInsertStackInstructionSwapFromBottom.DropDownItems.Add(stackOrRegexMenuItem(ch, ch - '⒈' + 1, insertText));
            for (var ch = 'Ⓐ'; ch <= 'Ⓩ'; ch++)
                miInsertRegexInstruction.DropDownItems.Add(stackOrRegexMenuItem(ch, ch - 'Ⓐ' + 1, insertText));

            foreach (var instr in typeof(ScliptingInstruction).GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                var attr = instr.GetCustomAttributes<InstructionAttribute>().First();
                var ch = attr.Character;
                var superMenu = attr.Type == InstructionType.SingularInstruction ? miInsertSingularInstruction : miInsertBlockInstruction;
                superMenu.DropDownItems.Add(
                    new ToolStripMenuItem(ch + " &" + attr.Engrish + " — " + attr.Description, null, (_, __) => { insertText(ch.ToString()); })
                );
            }
        }

        private static ToolStripItem stackOrRegexMenuItem(char ch, int num, Action<string> insertText)
        {
            return new ToolStripMenuItem(ch + " — &" + num, null, (_, __) => { insertText(ch.ToString()); });
        }

        private void insertInteger(Func<string> getSelectedText, Action<string> insertText)
        {
            string @default, selected = getSelectedText();
            try
            {
                if (selected.Length == 1 && selected[0] >= 0xbc00 && selected[0] <= 0xd7a3)
                    @default = (0xbbff - selected[0]).ToString();
                else
                    @default = new BigInteger(new byte[] { 0 }.Concat(DecodeByteArray(selected)).Reverse().ToArray()).ToString();
            }
            catch { @default = "0"; }
            var line = InputBox.GetLine(_tr.InsertMenu.IntegerPrompt, @default, "Esoteric IDE", Program.Tr.Ok, Program.Tr.Cancel);
            if (line != null)
            {
                BigInteger i;
                if (BigInteger.TryParse(line, out i) && (i >= -7076))
                {
                    if (i < 0)
                        insertText(((char) (0xbbff - i)).ToString());
                    else
                        insertText(EncodeByteArray(i.ToByteArray().Reverse().ToArray()));
                }
                else
                    DlgMessage.Show(_tr.InsertMenu.IntegerError, "Esoteric IDE", DlgType.Error, Program.Tr.Ok);
            }
        }

        private void insertString(Func<string> getSelectedText, Action<string> insertText)
        {
            string @default, selected = getSelectedText();
            try
            {
                if (selected.Length == 1 && selected[0] >= 0xbc00 && selected[0] <= 0xd7a3)
                    @default = (0xbbff - selected[0]).ToString();
                else
                    @default = DecodeByteArray(selected).FromUtf8().CLiteralEscape();
            }
            catch { @default = "\\n"; }
            var line = InputBox.GetLine(_tr.InsertMenu.StringPrompt, @default, "Esoteric IDE", Program.Tr.Ok, Program.Tr.Cancel);
            if (line != null)
                try { insertText(EncodeByteArray(line.CLiteralUnescape().ToUtf8())); }
                catch { DlgMessage.Show(_tr.InsertMenu.StringError, "Esoteric IDE", DlgType.Error, Program.Tr.Ok); }
        }

        private void insertByteArray(Func<string> getSelectedText, Action<string> insertText)
        {
            string @default, selected = getSelectedText();
            try { @default = DecodeByteArray(selected).ToHex(); }
            catch { @default = ""; }
            var line = InputBox.GetLine(_tr.InsertMenu.ByteArrayPrompt, @default, "Esoteric IDE", Program.Tr.Ok, Program.Tr.Cancel);
            if (line != null)
                try { insertText(EncodeByteArray(line.FromHex())); }
                catch { DlgMessage.Show(_tr.InsertMenu.ByteArrayError, "Esoteric IDE", DlgType.Error, Program.Tr.Ok); }
        }

        public ScliptingProgram Parse(string source)
        {
            return new ScliptingProgram { Instructions = parse(source, 0), Index = 0, Count = source.Length };
        }

        private List<Instruction> parse(string source, int addIndex)
        {
            ScliptingInstruction instruction;
            InstructionType type;

            var ret = new List<Instruction>();
            int index = 0;
            while (index < source.Length)
            {
                var ch = source[index];
                if (ch < 0x0100)
                {
                    // can wlite comments using infeliol Amelican and Eulopean chalactels
                    index++;
                }
                else if (ch >= 0xac00 && ch < 0xbc00)
                {
                    // Beginning of byte allay
                    var origIndex = index;
                    var hangeul = ParseByteArrayToken(source, index);
                    index += hangeul.Length;
                    try
                    {
                        ret.Add(new ByteArray { Array = DecodeByteArray(hangeul), Index = origIndex + addIndex, Count = index - origIndex });
                    }
                    catch (ParseException pe)
                    {
                        throw new ParseException(pe.Message, pe.Index + origIndex + addIndex, pe.Count);
                    }
                }
                else if (ch >= 0xbc00 && ch <= 0xd7a3)
                    ret.Add(new NegativeNumber { Number = 0xbbff - ch, Index = index++ + addIndex, Count = 1 });
                else if (ch >= '①' && ch <= '⑳')
                    ret.Add(new StackOrRegexInstruction { Type = StackOrRegexInstructionType.CopyFromBottom, Value = ch - '①' + 1, Index = index++ + addIndex, Count = 1 });
                else if (ch >= '㉑' && ch <= '㉟')
                    ret.Add(new StackOrRegexInstruction { Type = StackOrRegexInstructionType.CopyFromBottom, Value = ch - '㉑' + 21, Index = index++ + addIndex, Count = 1 });
                else if (ch >= '㊱' && ch <= '㊿')
                    ret.Add(new StackOrRegexInstruction { Type = StackOrRegexInstructionType.CopyFromBottom, Value = ch - '㊱' + 36, Index = index++ + addIndex, Count = 1 });
                else if (ch >= '⓵' && ch <= '⓾')
                    ret.Add(new StackOrRegexInstruction { Type = StackOrRegexInstructionType.MoveFromTop, Value = ch - '⓵' + 1, Index = index++ + addIndex, Count = 1 });
                else if (ch >= '❶' && ch <= '❿')
                    ret.Add(new StackOrRegexInstruction { Type = StackOrRegexInstructionType.CopyFromTop, Value = ch - '❶' + 1, Index = index++ + addIndex, Count = 1 });
                else if (ch >= '⓫' && ch <= '⓴')
                    ret.Add(new StackOrRegexInstruction { Type = StackOrRegexInstructionType.CopyFromTop, Value = ch - '⓫' + 11, Index = index++ + addIndex, Count = 1 });
                else if (ch >= '⑴' && ch <= '⒇')
                    ret.Add(new StackOrRegexInstruction { Type = StackOrRegexInstructionType.MoveFromBottom, Value = ch - '⑴' + 1, Index = index++ + addIndex, Count = 1 });
                else if (ch >= '⒈' && ch <= '⒛')
                    ret.Add(new StackOrRegexInstruction { Type = StackOrRegexInstructionType.SwapFromBottom, Value = ch - '⒈' + 1, Index = index++ + addIndex, Count = 1 });
                else if (ch >= 'Ⓐ' && ch <= 'Ⓩ')
                    ret.Add(new StackOrRegexInstruction { Type = StackOrRegexInstructionType.RegexCapture, Value = ch - 'Ⓐ' + 1, Index = index++ + addIndex, Count = 1 });
                else if (getInstructionInfo(ch, out instruction, out type))
                {
                    if (type == InstructionType.SingularInstruction)
                        ret.Add(new SingularInstruction { ThisInstruction = instruction, Index = index++ + addIndex, Count = 1 });
                    else if (type == InstructionType.BlockHead)
                    {
                        int endIndex;
                        int? elseIndex;
                        bool elsePops;
                        FindMatchingEnd(source, index, addIndex, out endIndex, out elseIndex, out elsePops);
                        var primaryBlock = parse(source.Substring(index + 1, (elseIndex ?? endIndex) - index - 1), index + 1 + addIndex);
                        var elseBlock = elseIndex == null ? null : parse(source.Substring(elseIndex.Value + 1, endIndex - elseIndex.Value - 1), elseIndex.Value + 1 + addIndex);
                        var blockInstr = createBlockInstruction(instruction, index, addIndex, elseIndex, elsePops, _tr);
                        blockInstr.PrimaryBlock = primaryBlock;
                        blockInstr.ElseBlock = elseBlock;
                        blockInstr.ElseBlockPops = elsePops;
                        blockInstr.Index = index + addIndex;
                        blockInstr.Count = endIndex - index + 1;
                        blockInstr.ElseIndex = (elseIndex ?? 0) + addIndex;
                        ret.Add(blockInstr);
                        index = endIndex + 1;
                    }
                    else
                        throw new ParseException(_tr.ParseErrors.UnexpectedEndOfBlock, index + addIndex, 1);
                }
                else
                    throw new ParseException(_tr.ParseErrors.UnrecognisedInstruction.Fmt(ch), index + addIndex, 1);
            }
            return ret;
        }

        private static void initInstructionsDictionary()
        {
            if (_instructions == null)
            {
                _instructions = new Dictionary<char, ScliptingInstruction>();
                _instructionTypes = new Dictionary<char, InstructionType>();
                foreach (var field in typeof(ScliptingInstruction).GetFields(BindingFlags.Static | BindingFlags.Public))
                {
                    var attr = field.GetCustomAttributes<InstructionAttribute>().First();
                    _instructions[attr.Character] = (ScliptingInstruction) field.GetValue(null);
                    _instructionTypes[attr.Character] = attr.Type;
                }
            }
        }

        private static bool getInstructionInfo(char character, out ScliptingInstruction instruction, out InstructionType type)
        {
            initInstructionsDictionary();
            if (_instructions.TryGetValue(character, out instruction))
            {
                type = _instructionTypes[character];
                return true;
            }
            else
            {
                type = default(InstructionType);
                return false;
            }
        }

        private static BlockInstruction createBlockInstruction(ScliptingInstruction instruction, int index, int addIndex, int? elseIndex, bool elsePops, Translation tr)
        {
            switch (instruction)
            {
                case ScliptingInstruction.Yes:
                case ScliptingInstruction.If:
                    return new If { PrimaryBlockPops = instruction == ScliptingInstruction.Yes };

                case ScliptingInstruction.Count:
                    if (elseIndex != null && !elsePops)
                        throw new ParseException(tr.ParseErrors.ForLoopCannotUseNonPopElse, index + addIndex, elseIndex.Value - index + 1);
                    return new ForLoop();

                case ScliptingInstruction.Each:
                case ScliptingInstruction.Every:
                    return new ForEachLoop { PrimaryBlockPops = instruction == ScliptingInstruction.Each };

                case ScliptingInstruction.Loop:
                case ScliptingInstruction.Necessity:
                case ScliptingInstruction.Until:
                case ScliptingInstruction.Arrive:
                    return new WhileLoop
                    {
                        PrimaryBlockPops = instruction == ScliptingInstruction.Loop || instruction == ScliptingInstruction.Until,
                        WhileTrue = instruction == ScliptingInstruction.Loop || instruction == ScliptingInstruction.Necessity
                    };

                case ScliptingInstruction.ReplaceFirstPop:
                case ScliptingInstruction.ReplaceFirstNoPop:
                case ScliptingInstruction.ReplaceAllPop:
                case ScliptingInstruction.ReplaceAllNoPop:
                    return new RegexSubstitute
                    {
                        PrimaryBlockPops = instruction == ScliptingInstruction.ReplaceFirstPop || instruction == ScliptingInstruction.ReplaceAllPop,
                        FirstMatchOnly = instruction == ScliptingInstruction.ReplaceFirstPop || instruction == ScliptingInstruction.ReplaceFirstNoPop
                    };

                default:
                    throw new ParseException(tr.ParseErrors.MissingInstruction.Fmt(instruction), index + addIndex, 1);
            }
        }

        private static void PostBuildCheck(IPostBuildReporter rep)
        {
            var tr = new Translation();
            initInstructionsDictionary();
            foreach (var instr in _instructionTypes.Where(kvp => kvp.Value == InstructionType.BlockHead).Select(kvp => _instructions[kvp.Key]))
                try { createBlockInstruction(instr, 0, 0, null, false, tr); }
                catch { rep.Error(@"Block instruction ""{0}"" is not processed.".Fmt(instr), "ScliptingLanguage", "BlockInstruction createBlockInstruction", "default"); }
        }

        public static string ParseByteArrayToken(string source, int index)
        {
            var origIndex = index;
            var ch = source[index];
            if (ch < 0xac00 || ch >= 0xbc00)
                throw new InternalErrorException("A byte array token does not start here.");
            do
            {
                index++;
                if (index == source.Length)
                    goto done;
                ch = source[index];
            }
            while (ch >= 0xac00 && ch < 0xbc00);
            if ((index - origIndex) % 2 == 1 && ch >= 0xbc00 && ch < 0xbc10)
                index++;
            done:
            return source.Substring(origIndex, index - origIndex);
        }

        public void FindMatchingEnd(string source, int start, int addIndex, out int endIndex, out int? elseIndex, out bool elsePops)
        {
            elseIndex = null;
            elsePops = false;
            var depth = 0;
            ScliptingInstruction instruction;
            InstructionType type;
            for (int i = start; i < source.Length; i++)
            {
                if (getInstructionInfo(source[i], out instruction, out type))
                {
                    if (type == InstructionType.BlockHead)
                        depth++;
                    else if (type == InstructionType.BlockElse && depth == 1)
                    {
                        elseIndex = i;
                        elsePops = source[i] == '不';
                    }
                    else if (type == InstructionType.BlockEnd)
                    {
                        depth--;
                        if (depth == 0)
                        {
                            endIndex = i;
                            return;
                        }
                    }
                }
            }
            throw new ParseException(_tr.ParseErrors.MissingEnd.Fmt(source[start]), start + addIndex, 1);
        }

        public byte[] DecodeByteArray(string soulce)
        {
            if (soulce.Length == 0)
                return new byte[0];
            var i = 0;
            var output = new List<byte>();
            while (true)
            {
                switch (soulce.Length - i)
                {
                    default:
                        {
                            if (soulce[i] < 0xac00 || soulce[i] >= 0xbc00)
                                throw new ParseException(_tr.ParseErrors.UnexpectedCharacter.Fmt(soulce[i]), i, 1);
                            if (soulce[i + 1] < 0xac00 || soulce[i + 1] > 0xbc0f)
                                throw new ParseException(_tr.ParseErrors.UnexpectedCharacter.Fmt(soulce[i]), i, 1);
                            int a = soulce[i] - 0xac00, b = soulce[i + 1] - 0xac00;
                            bool two = b >= 0x1000;
                            output.Add((byte) (a >> 4));
                            output.Add((byte) ((a & 0xf) << 4 | (two ? b - 0x1000 : b >> 8)));
                            if (!two)
                                output.Add((byte) (b & 0xff));
                            i += 2;
                            break;
                        }
                    case 1:
                        {
                            if (soulce[i] < 0xac00 || soulce[i] >= 0xbc00)
                                throw new ParseException(_tr.ParseErrors.UnexpectedCharacter.Fmt(soulce[i]), i, 1);
                            output.Add((byte) ((soulce[i] - 0xac00) >> 4));
                            i++;
                            break;
                        }
                    case 0:
                        return output.ToArray();
                }
            }
        }

        public static string EncodeByteArray(byte[] input)
        {
            if (input.Length == 0)
                return "";
            var i = 0;
            var output = "";
            while (true)
            {
                switch (input.Length - i)
                {
                    default:
                        {
                            byte a = input[i], b = input[i + 1], c = input[i + 2];
                            output += (char) (0xac00 + (a << 4 | b >> 4));
                            output += (char) (0xac00 + ((b & 0xf) << 8 | c));
                            i += 3;
                            break;
                        }
                    case 2:
                        {
                            byte a = input[i], b = input[i + 1];
                            output += (char) (0xac00 + (a << 4 | b >> 4));
                            output += (char) (0xbc00 + (b & 0xf));
                            i += 2;
                            break;
                        }
                    case 1:
                        {
                            byte a = input[i];
                            output += (char) (0xac00 + (a << 4));
                            i++;
                            break;
                        }
                    case 0:
                        return output;
                }
            }
        }

        public static List<object> ToList(object item)
        {
            List<object> l;
            byte[] b;
            string s;

            if ((l = item as List<object>) != null)
                return l;
            if ((s = item as string) != null)
                return s.Cast<object>().ToList();
            if ((b = item as byte[]) != null)
                return b.Select(byt => (object) (BigInteger) byt).ToList();

            if (item == null)
                return new List<object>();

            return new List<object> { item };
        }

        public static string ToString(object item)
        {
            if (item is BigInteger)
                return ((BigInteger) item).ToString(CultureInfo.InvariantCulture);
            if (item is double)
                return ((double) item).ToString(CultureInfo.InvariantCulture);

            string s;
            byte[] b;
            List<object> l;

            if ((s = item as string) != null)
                return s;
            if ((b = item as byte[]) != null)
                return b.FromUtf8();
            if ((l = item as List<object>) != null)
                return l.Select(i => ToString(i)).JoinString();

            return "";
        }

        public static bool IsTrue(object item)
        {
            // Shortcut for performance
            byte[] b;
            if ((b = item as byte[]) != null)
                return b.Any(y => y != 0);

            return ToInt(item) != 0;
        }

        public static object ToNumeric(object item)
        {
            List<object> l;
            if ((l = item as List<object>) != null)
                return recursiveListSum(l);

            if (item is double || item is BigInteger)
                return item;

            double dbl;
            if (item is string && ((string) item).Contains('.') && double.TryParse((string) item, NumberStyles.Float, CultureInfo.InvariantCulture, out dbl))
                return dbl;

            return ToInt(item);
        }

        public static BigInteger ToInt(object item)
        {
            List<object> l;
            if ((l = item as List<object>) != null)
                item = recursiveListSum(l);

            if (item is BigInteger)
                return (BigInteger) item;
            if (item is double)
            {
                var d = (double) item;
                if (double.IsNaN(d) || double.IsInfinity(d))
                    return BigInteger.Zero;
                return (BigInteger) d;
            }

            byte[] b;
            string s;
            BigInteger i;

            if ((b = item as byte[]) != null)
                return new BigInteger(new byte[] { 0 }.Concat(b).Reverse().ToArray());
            if ((s = item as string) != null)
                return BigInteger.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out i) ? i : 0;

            return item == null ? 0 : 1;
        }

        public static object recursiveListSum(List<object> list)
        {
            BigInteger bigInt = 0;
            double dbl = 0;
            bool isDouble = false;
            recursiveListSum(list, ref bigInt, ref dbl, ref isDouble);
            return isDouble ? (object) dbl : bigInt;
        }

        public static void recursiveListSum(List<object> list, ref BigInteger bigInt, ref double dbl, ref bool isDouble)
        {
            List<object> l;

            foreach (var item in list)
            {
                if ((l = item as List<object>) != null)
                    recursiveListSum(l, ref bigInt, ref dbl, ref isDouble);
                else
                {
                    var num = ToNumeric(item);
                    if (item is double)
                    {
                        if (!isDouble)
                        {
                            dbl = (double) bigInt;
                            isDouble = true;
                        }
                        dbl += (double) item;
                    }
                    else if (item is BigInteger)
                    {
                        if (isDouble)
                            dbl += (double) (BigInteger) item;
                        else
                            bigInt += (BigInteger) item;
                    }
                    else
                        throw new InternalErrorException("I expected this item to be either a float or an integer.");
                }
            }
        }
    }
}
