using System;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Windows.Forms;
using EsotericIDE.Sclipting;
using RT.Util;
using RT.Util.Dialogs;
using RT.Util.ExtensionMethods;

namespace EsotericIDE.Languages
{
    sealed class Sclipting : ProgrammingLanguage
    {
        public override string LanguageName { get { return "Sclipting"; } }
        public override string DefaultFileExtension { get { return "sclipt"; } }

        public override ExecutionEnvironment Compile(string source, string input)
        {
            return new ScliptingEnv(new Program { Instructions = Parser.Parse(source, 0), Index = 0, Count = source.Length }, input);
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
                    var hangeul = Parser.ParseByteArrayToken(source, index);
                    index += hangeul.Length;
                    if (cursorPos >= origIndex && cursorPos < index)
                    {
                        try
                        {
                            var array = ScliptingUtil.DecodeByteArray(hangeul);
                            return "Byte array: {0} = {{ {1} }} = “{2}” = {3}".Fmt(hangeul, array.Select(b => b.ToString("X2")).JoinString(" "), array.FromUtf8().CLiteralEscape(), ScliptingUtil.ToInt(array));
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

            if (ListStringElementNode.Characters.Contains(source[cursorPos]))
                return "List/string manipulation: {0} ({1}) — {2}".Fmt(
                    source[cursorPos],
                    ListStringElementNode.Engrish[ListStringElementNode.Characters.IndexOf(source[cursorPos])],
                    new ListStringElementNode(source[cursorPos], cursorPos).Explain());

            var attribute = typeof(Instruction).GetFields(BindingFlags.Static | BindingFlags.Public)
                .Select(f => f.GetCustomAttributes<InstructionAttribute>().FirstOrDefault())
                .Where(attr => attr != null && attr.Character == source[cursorPos])
                .FirstOrDefault();
            if (attribute != null)
                return "{3}: {0} ({1}) — {2}".Fmt(attribute.Character, attribute.Engrish, attribute.Description,
                    attribute.Type == NodeType.BlockHead ? "Block head instruction" :
                    attribute.Type == NodeType.BlockElse ? "Block else instruction" :
                    attribute.Type == NodeType.BlockEnd ? "Block end instruction" : "Instruction");

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
                instructionType = "Regex instruction:";
                description = "Push content of the {0}th regex capturing parenthesis.".Fmt(source[cursorPos] - 'Ⓐ' + 1);
            }
            else
                return "";
            return "{2} {0} — {1}".Fmt(source[cursorPos], description, instructionType);
        }

        public override ToolStripMenuItem[] CreateMenus(IIde ide)
        {
            var mnuInsert = new ToolStripMenuItem("&Insert");
            var groupMenuItems = typeof(InstructionGroup).GetFields(BindingFlags.Public | BindingFlags.Static)
                .ToDictionary(f => (InstructionGroup) f.GetValue(null), f => new ToolStripMenuItem(f.GetCustomAttributes<InstructionGroupAttribute>().First().Label));

            // Stack manipulation instructions
            var miInsertStackInstructionCopyFromBottom = new ToolStripMenuItem("&Copy from bottom");
            var miInsertStackInstructionMoveFromBottom = new ToolStripMenuItem("&Move from bottom");
            var miInsertStackInstructionSwapFromBottom = new ToolStripMenuItem("&Swap from bottom");
            var miInsertStackInstructionCopyFromTop = new ToolStripMenuItem("C&opy from top");
            var miInsertStackInstructionMoveFromTop = new ToolStripMenuItem("Mo&ve from top");
            var miInsertRegexGroupInstruction = new ToolStripMenuItem("Rege&x captured group");

            var miInsertInteger = new ToolStripMenuItem("&Integer...");
            var miInsertString = new ToolStripMenuItem("&String...");
            var miInsertByteArray = new ToolStripMenuItem("Byte &array...");

            miInsertInteger.Click += (_, __) => { insertInteger(ide); };
            miInsertString.Click += (_, __) => { insertString(ide); };
            miInsertByteArray.Click += (_, __) => { insertByteArray(ide); };

            mnuInsert.DropDownItems.AddRange(groupMenuItems.Values.ToArray());
            mnuInsert.DropDownItems.AddRange(new ToolStripItem[] { miInsertInteger, miInsertString, miInsertByteArray });

            foreach (var instr in typeof(Instruction).GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                var attr = instr.GetCustomAttributes<InstructionAttribute>().First();
                var ch = attr.Character;
                groupMenuItems[attr.Group].DropDownItems.Add(new ToolStripMenuItem(
                    "{0} &{1} — {2}{3}".Fmt(ch, attr.Engrish, attr.Description, attr.Type == NodeType.BlockHead ? " █" : ""),
                    null, (_, __) => { ide.InsertText(ch.ToString()); }
                ));
            }

            groupMenuItems[InstructionGroup.StackManipulation].DropDownItems.Add("-");
            groupMenuItems[InstructionGroup.StackManipulation].DropDownItems.AddRange(Ut.NewArray<ToolStripItem>(
                miInsertStackInstructionCopyFromBottom, miInsertStackInstructionMoveFromBottom, miInsertStackInstructionSwapFromBottom,
                miInsertStackInstructionCopyFromTop, miInsertStackInstructionMoveFromTop));

            groupMenuItems[InstructionGroup.ListStringManipulation].DropDownItems.Add("-");
            groupMenuItems[InstructionGroup.ListStringManipulation].DropDownItems.AddRange(ListStringElementNode.GetMenuItems(ide));

            for (var ch = '①'; ch <= '⑳'; ch++)
                miInsertStackInstructionCopyFromBottom.DropDownItems.Add(stackOrRegexMenuItem(ch, ch - '①' + 1, ide));
            for (var ch = '㉑'; ch <= '㉟'; ch++)
                miInsertStackInstructionCopyFromBottom.DropDownItems.Add(stackOrRegexMenuItem(ch, ch - '㉑' + 21, ide));
            for (var ch = '㊱'; ch <= '㊿'; ch++)
                miInsertStackInstructionCopyFromBottom.DropDownItems.Add(stackOrRegexMenuItem(ch, ch - '㊱' + 36, ide));
            for (var ch = '⓵'; ch <= '⓾'; ch++)
                miInsertStackInstructionMoveFromTop.DropDownItems.Add(stackOrRegexMenuItem(ch, ch - '⓵' + 1, ide));
            for (var ch = '❶'; ch <= '❿'; ch++)
                miInsertStackInstructionCopyFromTop.DropDownItems.Add(stackOrRegexMenuItem(ch, ch - '❶' + 1, ide));
            for (var ch = '⓫'; ch <= '⓴'; ch++)
                miInsertStackInstructionCopyFromTop.DropDownItems.Add(stackOrRegexMenuItem(ch, ch - '⓫' + 11, ide));
            for (var ch = '⑴'; ch <= '⒇'; ch++)
                miInsertStackInstructionMoveFromBottom.DropDownItems.Add(stackOrRegexMenuItem(ch, ch - '⑴' + 1, ide));
            for (var ch = '⒈'; ch <= '⒛'; ch++)
                miInsertStackInstructionSwapFromBottom.DropDownItems.Add(stackOrRegexMenuItem(ch, ch - '⒈' + 1, ide));

            groupMenuItems[InstructionGroup.StringManipulation].DropDownItems.Add("-");
            groupMenuItems[InstructionGroup.StringManipulation].DropDownItems.Add(miInsertRegexGroupInstruction);
            for (var ch = 'Ⓐ'; ch <= 'Ⓩ'; ch++)
                miInsertRegexGroupInstruction.DropDownItems.Add(stackOrRegexMenuItem(ch, ch - 'Ⓐ' + 1, ide));

            return Ut.NewArray<ToolStripMenuItem>(mnuInsert);
        }

        private static ToolStripItem stackOrRegexMenuItem(char ch, int num, IIde ide)
        {
            return new ToolStripMenuItem("{0} — &{1}".Fmt(ch, num), null, (_, __) => { ide.InsertText(ch.ToString()); });
        }

        private void insertInteger(IIde ide)
        {
            string @default, selected = ide.GetSelectedText();
            try
            {
                if (selected.Length == 1 && selected[0] >= 0xbc00 && selected[0] <= 0xd7a3)
                    @default = (0xbbff - selected[0]).ToString();
                else
                    @default = new BigInteger(new byte[] { 0 }.Concat(ScliptingUtil.DecodeByteArray(selected)).Reverse().ToArray()).ToString();
            }
            catch { @default = "0"; }
            var line = InputBox.GetLine("Type an integer to insert (must be greater than −7077):", @default, "Esoteric IDE", "&OK", "&Cancel");
            if (line != null)
            {
                BigInteger i;
                if (BigInteger.TryParse(line, out i) && (i >= -7076))
                {
                    if (i < 0)
                        ide.InsertText(((char) (0xbbff - i)).ToString());
                    else
                        ide.InsertText(ScliptingUtil.EncodeByteArray(i.ToByteArray().Reverse().SkipWhile(b => b == 0).DefaultIfEmpty().ToArray()));
                }
                else
                    DlgMessage.Show("The integer you typed is not a valid literal integer for Sclipting. Literal integers must be greater than −7077.", "Esoteric IDE", DlgType.Error, "&OK");
            }
        }

        private void insertString(IIde ide)
        {
            string @default, selected = ide.GetSelectedText();
            try
            {
                if (selected.Length == 1 && selected[0] >= 0xbc00 && selected[0] <= 0xd7a3)
                    @default = (0xbbff - selected[0]).ToString();
                else
                    @default = ScliptingUtil.DecodeByteArray(selected).FromUtf8().CLiteralEscape();
            }
            catch { @default = "\\n"; }
            var line = InputBox.GetLine("Type a string to insert (in C-escaped format; backslashes must be escaped):", @default, "Esoteric IDE", "&OK", "&Cancel");
            if (line != null)
                try { ide.InsertText(ScliptingUtil.EncodeByteArray(line.CLiteralUnescape().ToUtf8())); }
                catch { DlgMessage.Show("The string you typed is not a valid C-escaped string. Please ensure that your backslashes are escaped.", "Esoteric IDE", DlgType.Error, "&OK"); }
        }

        private void insertByteArray(IIde ide)
        {
            string @default, selected = ide.GetSelectedText();
            try { @default = ScliptingUtil.DecodeByteArray(selected).ToHex(); }
            catch { @default = ""; }
            var line = InputBox.GetLine("Type a byte array to insert (in hexdump format; two hexadecimal digits per byte):", @default, "Esoteric IDE", "&OK", "&Cancel");
            if (line != null)
                try { ide.InsertText(ScliptingUtil.EncodeByteArray(line.FromHex())); }
                catch { DlgMessage.Show("The text you entered is not valid hexadecimal. Please ensure that you enter an even number of characters 0-9/a-f.", "Esoteric IDE", DlgType.Error, "&OK"); }
        }
    }
}
