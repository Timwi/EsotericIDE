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

namespace EsotericIDE.Languages
{
    sealed partial class Sclipting : ProgrammingLanguage
    {
        public override string LanguageName { get { return "Sclipting"; } }
        public override string DefaultFileExtension { get { return ".sclipt"; } }

        private static Dictionary<char, instruction> _instructions;
        private static Dictionary<char, nodeType> _instructionTypes;

        public override ExecutionEnvironment Compile(string source, string input)
        {
            return new executionEnvironment(new program { Instructions = parse(source, 0), Index = 0, Count = source.Length }, input);
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
                        goto negNum;
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
                            return "Byte array: {0} = {{ {1} }} = “{2}” = {3}".Fmt(hangeul, array.Select(b => b.ToString("X2")).JoinString(" "), array.FromUtf8().CLiteralEscape(), ToInt(array));
                        }
                        catch (CompileException ce)
                        {
                            return "Error while decoding byte array: " + ce.Message;
                        }
                    }
                    else if (index > cursorPos)
                        throw new InternalErrorException("This cannot happen.");
                }
            }

            negNum:
            if (source[cursorPos] >= 0xbc00 && source[cursorPos] <= 0xd7a3)
                return "Negative number: {0}".Fmt(0xbbff - source[cursorPos]);

            var attribute = typeof(instruction).GetFields(BindingFlags.Static | BindingFlags.Public)
                .Select(f => f.GetCustomAttributes<instructionAttribute>().FirstOrDefault())
                .Where(attr => attr != null && attr.Character == source[cursorPos])
                .FirstOrDefault();
            if (attribute != null)
                return "{3}: {0} ({1}) — {2}".Fmt(attribute.Character, attribute.Engrish, attribute.Description,
                    attribute.Type == nodeType.BlockHead ? "Block head instruction" :
                    attribute.Type == nodeType.BlockElse ? "Block else instruction" :
                    attribute.Type == nodeType.BlockEnd ? "Block end instruction" : "Instruction");

            string description;
            string instructionType = "Stack instruction:";

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
                instructionType = "Regex instruction:";
            }
            else
                return "";
            return "{2} {0} — {1}".Fmt(source[cursorPos], description, instructionType);
        }

        public override ToolStripMenuItem[] CreateMenus(Func<string> getSelectedText, Action<string> insertText)
        {
            var mnuInsert = new ToolStripMenuItem("&Insert");
            var miInsertSingularInstruction = new ToolStripMenuItem("Si&ngular instruction");
            var miInsertBlockInstruction = new ToolStripMenuItem("&Block instruction");
            var miInsertStackInstruction = new ToolStripMenuItem("S&tack instruction");
            var miInsertStackInstructionCopyFromBottom = new ToolStripMenuItem("&Copy from bottom");
            var miInsertStackInstructionMoveFromBottom = new ToolStripMenuItem("&Move from bottom");
            var miInsertStackInstructionSwapFromBottom = new ToolStripMenuItem("&Swap from bottom");
            var miInsertStackInstructionCopyFromTop = new ToolStripMenuItem("C&opy from top");
            var miInsertStackInstructionMoveFromTop = new ToolStripMenuItem("Mo&ve from top");
            var miInsertRegexInstruction = new ToolStripMenuItem("&Regex instruction");
            var miInsertInteger = new ToolStripMenuItem("&Integer...");
            var miInsertString = new ToolStripMenuItem("&String...");
            var miInsertByteArray = new ToolStripMenuItem("Byte &array...");

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

            foreach (var instr in typeof(instruction).GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                var attr = instr.GetCustomAttributes<instructionAttribute>().First();
                var ch = attr.Character;
                var superMenu = (attr.Type == nodeType.SingularNode || attr.Type == nodeType.FunctionExecutionNode) ? miInsertSingularInstruction : miInsertBlockInstruction;
                superMenu.DropDownItems.Add(
                    new ToolStripMenuItem(ch + " &" + attr.Engrish + " — " + attr.Description, null, (_, __) => { insertText(ch.ToString()); })
                );
            }

            return Ut.NewArray<ToolStripMenuItem>(mnuInsert);
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
            var line = InputBox.GetLine("Type an integer to insert (must be greater than −7077):", @default, "Esoteric IDE", "&OK", "&Cancel");
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
                    DlgMessage.Show("The integer you typed is not a valid literal integer for Sclipting. Literal integers must be greater than −7077.", "Esoteric IDE", DlgType.Error, "&OK");
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
            var line = InputBox.GetLine("Type a string to insert (in C-escaped format; backslashes must be escaped):", @default, "Esoteric IDE", "&OK", "&Cancel");
            if (line != null)
                try { insertText(EncodeByteArray(line.CLiteralUnescape().ToUtf8())); }
                catch { DlgMessage.Show("The string you typed is not a valid C-escaped string. Please ensure that your backslashes are escaped.", "Esoteric IDE", DlgType.Error, "&OK"); }
        }

        private void insertByteArray(Func<string> getSelectedText, Action<string> insertText)
        {
            string @default, selected = getSelectedText();
            try { @default = DecodeByteArray(selected).ToHex(); }
            catch { @default = ""; }
            var line = InputBox.GetLine("Type a byte array to insert (in hexdump format; two hexadecimal digits per byte):", @default, "Esoteric IDE", "&OK", "&Cancel");
            if (line != null)
                try { insertText(EncodeByteArray(line.FromHex())); }
                catch { DlgMessage.Show("The text you entered is not valid hexadecimal. Please ensure that you enter an even number of characters 0-9/a-f.", "Esoteric IDE", DlgType.Error, "&OK"); }
        }

        private List<node> parse(string source, int addIndex)
        {
            instruction instruction;
            nodeType type;

            var ret = new List<node>();
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
                        ret.Add(new byteArray { Array = DecodeByteArray(hangeul), Index = origIndex + addIndex, Count = index - origIndex });
                    }
                    catch (CompileException ce)
                    {
                        throw new CompileException(ce.Message, ce.Index + origIndex + addIndex, ce.Length);
                    }
                }
                else if (ch >= 0xbc00 && ch <= 0xd7a3)
                    ret.Add(new negativeNumber { Number = 0xbbff - ch, Index = index++ + addIndex, Count = 1 });
                else if (ch >= '①' && ch <= '⑳')
                    ret.Add(new stackOrRegexNode { Type = stackOrRegexNodeType.CopyFromBottom, Value = ch - '①' + 1, Index = index++ + addIndex, Count = 1 });
                else if (ch >= '㉑' && ch <= '㉟')
                    ret.Add(new stackOrRegexNode { Type = stackOrRegexNodeType.CopyFromBottom, Value = ch - '㉑' + 21, Index = index++ + addIndex, Count = 1 });
                else if (ch >= '㊱' && ch <= '㊿')
                    ret.Add(new stackOrRegexNode { Type = stackOrRegexNodeType.CopyFromBottom, Value = ch - '㊱' + 36, Index = index++ + addIndex, Count = 1 });
                else if (ch >= '⓵' && ch <= '⓾')
                    ret.Add(new stackOrRegexNode { Type = stackOrRegexNodeType.MoveFromTop, Value = ch - '⓵' + 1, Index = index++ + addIndex, Count = 1 });
                else if (ch >= '❶' && ch <= '❿')
                    ret.Add(new stackOrRegexNode { Type = stackOrRegexNodeType.CopyFromTop, Value = ch - '❶' + 1, Index = index++ + addIndex, Count = 1 });
                else if (ch >= '⓫' && ch <= '⓴')
                    ret.Add(new stackOrRegexNode { Type = stackOrRegexNodeType.CopyFromTop, Value = ch - '⓫' + 11, Index = index++ + addIndex, Count = 1 });
                else if (ch >= '⑴' && ch <= '⒇')
                    ret.Add(new stackOrRegexNode { Type = stackOrRegexNodeType.MoveFromBottom, Value = ch - '⑴' + 1, Index = index++ + addIndex, Count = 1 });
                else if (ch >= '⒈' && ch <= '⒛')
                    ret.Add(new stackOrRegexNode { Type = stackOrRegexNodeType.SwapFromBottom, Value = ch - '⒈' + 1, Index = index++ + addIndex, Count = 1 });
                else if (ch >= 'Ⓐ' && ch <= 'Ⓩ')
                    ret.Add(new stackOrRegexNode { Type = stackOrRegexNodeType.RegexCapture, Value = ch - 'Ⓐ' + 1, Index = index++ + addIndex, Count = 1 });
                else if (getInstructionInfo(ch, out instruction, out type))
                {
                    switch (type)
                    {
                        case nodeType.SingularNode:
                            ret.Add(new singularNode { ThisInstruction = instruction, Index = index++ + addIndex, Count = 1 });
                            break;

                        case nodeType.FunctionExecutionNode:
                            ret.Add(new executeFunction { Instruction = instruction, Index = index++ + addIndex, Count = 1 });
                            break;

                        case nodeType.BlockHead:
                            int endIndex;
                            int? elseIndex;
                            bool elsePops;
                            FindMatchingEnd(source, index, addIndex, out endIndex, out elseIndex, out elsePops);
                            var primaryBlock = parse(source.Substring(index + 1, (elseIndex ?? endIndex) - index - 1), index + 1 + addIndex);
                            var elseBlock = elseIndex == null ? null : parse(source.Substring(elseIndex.Value + 1, endIndex - elseIndex.Value - 1), elseIndex.Value + 1 + addIndex);
                            var blockInstr = createBlockNode(instruction, index, addIndex, elseIndex, elsePops);
                            blockInstr.PrimaryBlock = primaryBlock;
                            blockInstr.ElseBlock = elseBlock;
                            blockInstr.ElseBlockPops = elsePops;
                            blockInstr.Index = index + addIndex;
                            blockInstr.Count = endIndex - index + 1;
                            blockInstr.ElseIndex = (elseIndex ?? 0) + addIndex;
                            ret.Add(blockInstr);
                            index = endIndex + 1;
                            break;

                        case nodeType.BlockElse:
                        case nodeType.BlockEnd:
                            throw new CompileException("Else or end instruction encountered without a matching block head instruction.", index + addIndex, 1);

                        default:
                            throw new CompileException("Unrecognised instruction: “{0}”.".Fmt(ch), index + addIndex, 1);
                    }
                }
                else
                    throw new CompileException("Unrecognised instruction: “{0}”.".Fmt(ch), index + addIndex, 1);
            }
            return ret;
        }

        private sealed class duplicateCharacterException : Exception
        {
            public char Character { get; private set; }
            public duplicateCharacterException(char chr, string message) : base(message) { Character = chr; }
        }

        private static void initInstructionsDictionary()
        {
            if (_instructions == null)
            {
                _instructions = new Dictionary<char, instruction>();
                _instructionTypes = new Dictionary<char, nodeType>();
                foreach (var field in typeof(instruction).GetFields(BindingFlags.Static | BindingFlags.Public))
                {
                    var attr = field.GetCustomAttributes<instructionAttribute>().First();
                    if (_instructions.ContainsKey(attr.Character))
                        throw new duplicateCharacterException(attr.Character, "The character “{0}” is used for more than one instruction.".Fmt(attr.Character));
                    _instructions[attr.Character] = (instruction) field.GetValue(null);
                    _instructionTypes[attr.Character] = attr.Type;
                }
            }
        }

        private static bool getInstructionInfo(char character, out instruction instruction, out nodeType type)
        {
            initInstructionsDictionary();
            if (_instructions.TryGetValue(character, out instruction))
            {
                type = _instructionTypes[character];
                return true;
            }
            else
            {
                type = default(nodeType);
                return false;
            }
        }

        private static blockNode createBlockNode(instruction instr, int index, int addIndex, int? elseIndex, bool elsePops)
        {
            switch (instr)
            {
                case instruction.Yes:
                case instruction.If:
                    return new ifBlock { PrimaryBlockPops = instr == instruction.Yes };

                case instruction.Up:
                case instruction.Down:
                    if (elseIndex != null && !elsePops)
                        throw new CompileException("The block else instruction “逆” cannot be used with the block head instruction “數”.", index + addIndex, elseIndex.Value - index + 1);
                    return new forLoop { Backwards = instr == instruction.Down };

                case instruction.Each:
                case instruction.Every:
                    return new forEachLoop { PrimaryBlockPops = instr == instruction.Each };

                case instruction.Loop:
                case instruction.Necessity:
                case instruction.Until:
                case instruction.Arrive:
                    return new whileLoop
                    {
                        PrimaryBlockPops = instr == instruction.Loop || instr == instruction.Until,
                        WhileTrue = instr == instruction.Loop || instr == instruction.Necessity
                    };

                case instruction.ReplaceFirstPop:
                case instruction.ReplaceFirstNoPop:
                case instruction.ReplaceAllPop:
                case instruction.ReplaceAllNoPop:
                    return new regexSubstitute
                    {
                        PrimaryBlockPops = instr == instruction.ReplaceFirstPop || instr == instruction.ReplaceAllPop,
                        FirstMatchOnly = instr == instruction.ReplaceFirstPop || instr == instruction.ReplaceFirstNoPop
                    };

                case instruction.Block:
                case instruction.Capture:
                    if (elseIndex != null)
                        throw new CompileException("The block instruction “塊” cannot have a “不” or “逆” block.", index + addIndex, elseIndex.Value - index + 1);
                    return new functionNode { Capture = instr == instruction.Capture };

                default:
                    throw new CompileException("Instruction “{0}” missing.".Fmt(instr), index + addIndex, 1);
            }
        }

        private static executeFunction createFunctionExecutionNode(instruction instruction, int index, int addIndex)
        {
            switch (instruction)
            {
                case instruction.Initiate:
                case instruction.Handle:
                case instruction.Perform:
                    return new executeFunction { Instruction = instruction };

                default:
                    throw new CompileException("Instruction “{0}” missing.".Fmt(instruction), index + addIndex, 1);
            }
        }


#if DEBUG
        private static void PostBuildCheck(IPostBuildReporter rep)
        {
            try { initInstructionsDictionary(); }
            catch (duplicateCharacterException e)
            {
                rep.Error(@"Same character is used multiple times for the same instruction. (First use here.)".Fmt(e.Character), "enum ScliptingInstructions", e.Character.ToString());
                rep.Error(@"Same character is used multiple times for the same instruction. (Second use here.)".Fmt(e.Character), "enum ScliptingInstructions", e.Character.ToString(), e.Character.ToString());
                return;
            }
            foreach (var instr in _instructionTypes.Where(kvp => kvp.Value == nodeType.BlockHead).Select(kvp => _instructions[kvp.Key]))
            {
                try { createBlockNode(instr, 0, 0, null, false); }
                catch { rep.Error(@"Block instruction ""{0}"" is not processed.".Fmt(instr), "blockNode createBlockNode", "default"); }
            }
            foreach (var instr in _instructionTypes.Where(kvp => kvp.Value == nodeType.FunctionExecutionNode).Select(kvp => _instructions[kvp.Key]))
            {
                try { createFunctionExecutionNode(instr, 0, 0); }
                catch { rep.Error(@"Function execution instruction ""{0}"" is not processed.".Fmt(instr), "executeFunction createFunctionExecutionNode", "default"); }
            }
        }
#endif

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
            instruction instruction;
            nodeType type;
            for (int i = start; i < source.Length; i++)
            {
                if (getInstructionInfo(source[i], out instruction, out type))
                {
                    if (type == nodeType.BlockHead)
                        depth++;
                    else if (type == nodeType.BlockElse && depth == 1)
                    {
                        elseIndex = i;
                        elsePops = source[i] == '不';
                    }
                    else if (type == nodeType.BlockEnd)
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
            throw new CompileException("Block instruction “{0}” is missing a matching end instruction.".Fmt(source[start]), start + addIndex, 1);
        }

        public byte[] DecodeByteArray(string source)
        {
            if (source.Length == 0)
                return new byte[0];
            var i = 0;
            var output = new List<byte>();
            while (true)
            {
                switch (source.Length - i)
                {
                    default:
                        {
                            if (source[i] < 0xac00 || source[i] >= 0xbc00)
                                throw new CompileException("Unexpected character in program: “{0}”.".Fmt(source[i]), i, 1);
                            if (source[i + 1] < 0xac00 || source[i + 1] > 0xbc0f)
                                throw new CompileException("Unexpected character in program: “{0}”.".Fmt(source[i]), i, 1);
                            int a = source[i] - 0xac00, b = source[i + 1] - 0xac00;
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
                            if (source[i] < 0xac00 || source[i] >= 0xbc00)
                                throw new CompileException("Unexpected character in program: “{0}”.".Fmt(source[i]), i, 1);
                            output.Add((byte) ((source[i] - 0xac00) >> 4));
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

        public static string ToString(object item)
        {
            if (item == null || item is mark || item is function)
                return "";
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

            throw new ArgumentException("Unrecognised item type for conversion to string: " + item.GetType().Name);
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
            byte[] b;
            string s;
            BigInteger i;

            if (item == null || item is mark || item is function)
                return 0;
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

            if ((b = item as byte[]) != null)
                return new BigInteger(new byte[] { 0 }.Concat(b).Reverse().ToArray());
            if ((s = item as string) != null)
                return BigInteger.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out i) ? i : 0;

            throw new ArgumentException("Unrecognised item type for conversion to int: " + item.GetType().Name);
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
                    if (num is double)
                    {
                        if (!isDouble)
                        {
                            dbl = (double) bigInt;
                            isDouble = true;
                        }
                        dbl += (double) num;
                    }
                    else if (num is BigInteger)
                    {
                        if (isDouble)
                            dbl += (double) (BigInteger) num;
                        else
                            bigInt += (BigInteger) num;
                    }
                    else
                        throw new InternalErrorException("I expected this item to be either a float or an integer.");
                }
            }
        }
    }
}
