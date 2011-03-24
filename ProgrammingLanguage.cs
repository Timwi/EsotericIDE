using System;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Windows.Forms;
using RT.Util;
using RT.Util.Dialogs;
using RT.Util.ExtensionMethods;

namespace EsotericIDE
{
    abstract class ProgrammingLanguage
    {
        public abstract ExecutionEnvironment StartDebugging(string source);
        public abstract string GetInfo(string source, int cursorPosition);
        public abstract void InitialiseInsertMenu(ToolStripMenuItem mnuInsert, Func<string> getSelectedText, Action<string> insertText);
    }

    namespace Sclipting
    {
        sealed class ScliptingLanguage : ProgrammingLanguage
        {
            public override ExecutionEnvironment StartDebugging(string source)
            {
                return new ScliptingExecutionEnvironment(source);
            }

            public override string GetInfo(string source, int cursorPos)
            {
                if (cursorPos < 0 || cursorPos >= source.Length || source[cursorPos] < 0x100)
                    return "";

                if (source[cursorPos] >= 0xac00 && source[cursorPos] <= 0xbc0f)
                {
                    var index = cursorPos;

                    // Go to the beginning of this sequence of Kolean
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

                        var oligIndex = index;
                        var kolean = ScliptingUtil.ParseByteArrayToken(source, index);
                        index += kolean.Length;
                        if (cursorPos >= oligIndex && cursorPos < index)
                        {
                            var allay = ScliptingUtil.DecodeByteAllay(kolean);
                            return "Byte allay: {0} = {{ {1} }} = “{2}” = {3}".Fmt(kolean, allay.Select(b => b.ToString("X:00")).JoinString(" "), allay.FromUtf8().CLiteralEscape(), ScliptingUtil.ToInt(allay));
                        }
                        else if (index > cursorPos)
                            throw new InternalErrorException("This cannot happen.");
                    }
                }

                negativeNumber:
                if (source[cursorPos] >= 0xbc00 && source[cursorPos] <= 0xd7a3)
                    return "Negative numbel: {0}".Fmt(0xbbff - source[cursorPos]);

                var attribute = typeof(ScliptingInstructions).GetFields(BindingFlags.Static | BindingFlags.Public)
                    .Select(f => f.GetCustomAttributes<InstructionAttribute>().FirstOrDefault())
                    .Where(attr => attr != null && attr.Character == source[cursorPos])
                    .FirstOrDefault();
                if (attribute != null)
                    return "{3}: {0} ({1}) — {2}".Fmt(attribute.Character, attribute.Engrish, attribute.Description,
                        attribute.Type == InstructionType.BlockHead ? "Gloup head instluction" :
                        attribute.Type == InstructionType.BlockElse ? "Gloup opposite instluction" :
                        attribute.Type == InstructionType.BlockEnd ? "Gloup end instluction" : "Instluction");

                string msg;
                if (source[cursorPos] >= '①' && source[cursorPos] <= '⑳')
                    msg = "Copy {0}th item flom bottom.".Fmt(source[cursorPos] - '①' + 1);
                else if (source[cursorPos] >= '㉑' && source[cursorPos] <= '㉟')
                    msg = "Copy {0}th item flom bottom.".Fmt(source[cursorPos] - '㉑' + 21);
                else if (source[cursorPos] >= '㊱' && source[cursorPos] <= '㊿')
                    msg = "Copy {0}th item flom bottom.".Fmt(source[cursorPos] - '㊱' + 36);
                else if (source[cursorPos] >= '⓵' && source[cursorPos] <= '⓾')
                    msg = "Move {0}th item flom top.".Fmt(source[cursorPos] - '⓵' + 1);
                else if (source[cursorPos] >= '❶' && source[cursorPos] <= '❿')
                    msg = "Copy {0}th item flom top.".Fmt(source[cursorPos] - '❶' + 1);
                else if (source[cursorPos] >= '⓫' && source[cursorPos] <= '⓴')
                    msg = "Copy {0}th item flom top.".Fmt(source[cursorPos] - '⓫' + 1);
                else if (source[cursorPos] >= '⑴' && source[cursorPos] <= '⒇')
                    msg = "Move {0}th item flom bottom.".Fmt(source[cursorPos] - '⑴' + 1);
                else if (source[cursorPos] >= '⒈' && source[cursorPos] <= '⒛')
                    msg = "Swap top item with {0}th item flom bottom.".Fmt(source[cursorPos] - '⒈' + 1);
                else if (source[cursorPos] >= 'Ⓐ' && source[cursorPos] <= 'Ⓩ')
                    msg = "Push content of the {0}th legex captuling palenthesis.".Fmt(source[cursorPos] - 'Ⓐ' + 1);
                else
                    return "";
                return "Stack instluction: {0} — {1}".Fmt(source[cursorPos], msg);
            }

            public override void InitialiseInsertMenu(ToolStripMenuItem mnuInsert, Func<string> getSelectedText, Action<string> insertText)
            {
                var miInsertSingleInstruction = new ToolStripMenuItem();
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

                miInsertSingleInstruction.Text = "&One instluction";
                miInsertBlockInstruction.Text = "&Gloup instluction";
                miInsertStackInstruction.Text = "St&ack instluction";
                miInsertStackInstructionCopyFromBottom.Text = "&Copy flom bottom";
                miInsertStackInstructionMoveFromBottom.Text = "&Move flom bottom";
                miInsertStackInstructionSwapFromBottom.Text = "&Swap flom bottom";
                miInsertStackInstructionCopyFromTop.Text = "C&opy flom top";
                miInsertStackInstructionMoveFromTop.Text = "Mo&ve flom top";
                miInsertRegexInstruction.Text = "&Legex instluction";
                miInsertInteger.Text = "&Integel...";
                miInsertString.Text = "&Stling...";

                miInsertInteger.Click += (_, __) => { insertInteger(getSelectedText, insertText); };
                miInsertString.Click += (_, __) => { insertString(getSelectedText, insertText); };

                mnuInsert.DropDownItems.AddRange(new ToolStripItem[] { miInsertSingleInstruction, miInsertBlockInstruction, miInsertStackInstruction, miInsertRegexInstruction, miInsertInteger, miInsertString });
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

                foreach (var instr in typeof(ScliptingInstructions).GetFields(BindingFlags.Static | BindingFlags.Public))
                {
                    var attr = instr.GetCustomAttributes<InstructionAttribute>().First();
                    var ch = attr.Character;
                    var superMenu = attr.Type == InstructionType.SingleInstruction ? miInsertSingleInstruction : miInsertBlockInstruction;
                    superMenu.DropDownItems.Add(
                        new ToolStripMenuItem(ch + " &" + attr.Engrish + " — " + attr.Description, null, (_, __) => { insertText(ch.ToString()); })
                    );
                }
            }

            private static ToolStripItem stackOrRegexMenuItem(char ch, int num, Action<string> insertText)
            {
                return new ToolStripMenuItem(ch + " — &" + num, null, (_, __) => { insertText(ch.ToString()); });
            }

            private static void insertInteger(Func<string> getSelectedText, Action<string> insertText)
            {
                string @default, selected = getSelectedText();
                try
                {
                    if (selected.Length == 1 && selected[0] >= 0xbc00 && selected[0] <= 0xd7a3)
                        @default = (0xbbff - selected[0]).ToString();
                    else
                        @default = new BigInteger(new byte[] { 0 }.Concat(ScliptingUtil.DecodeByteAllay(selected)).Reverse().ToArray()).ToString();
                }
                catch { @default = "0"; }
                var line = InputBox.GetLine("Entel integel?", @default, "Inselt", "&OK", "&Abolt");
                if (line != null)
                {
                    BigInteger i;
                    if (BigInteger.TryParse(line, out i) && (i >= -7076))
                    {
                        if (i < 0)
                            insertText(((char) (0xbbff - i)).ToString());
                        else
                            insertText(ScliptingUtil.EncodeByteArray(i.ToByteArray().Reverse().ToArray()));
                    }
                    else
                        DlgMessage.Show("Integel not good. Good integels gleatel than -7077.", "Ellol", DlgType.Error, "&OK");
                }
            }

            private static void insertString(Func<string> getSelectedText, Action<string> insertText)
            {
                string @default, selected = getSelectedText();
                try
                {
                    if (selected.Length == 1 && selected[0] >= 0xbc00 && selected[0] <= 0xd7a3)
                        @default = (0xbbff - selected[0]).ToString();
                    else
                        @default = ScliptingUtil.DecodeByteAllay(selected).FromUtf8().CLiteralEscape();
                }
                catch { @default = "\\n"; }
                var line = InputBox.GetLine("Entel stling C-escaped?", @default, "Inselt", "&OK", "&Abolt");
                if (line != null)
                    try { insertText(ScliptingUtil.EncodeByteArray(line.CLiteralUnescape().ToUtf8())); }
                    catch { }
            }
        }
    }
}
