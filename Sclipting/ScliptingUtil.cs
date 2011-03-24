using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Reflection;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace EsotericIDE.Sclipting
{
    static class ScliptingUtil
    {
        private static string _blockHeadInstructions;
        private static string _blockElseInstructions;
        private static string _blockEndInstructions;
        public static string BlockHeadInstructions { get { return _blockHeadInstructions ?? (_blockHeadInstructions = getBlockInstructions(InstructionType.BlockHead)); } }
        public static string BlockElseInstructions { get { return _blockElseInstructions ?? (_blockElseInstructions = getBlockInstructions(InstructionType.BlockElse)); } }
        public static string BlockEndInstructions { get { return _blockEndInstructions ?? (_blockEndInstructions = getBlockInstructions(InstructionType.BlockEnd)); } }

        private static string getBlockInstructions(InstructionType type)
        {
            return typeof(ScliptingInstructions).GetFields(BindingFlags.Public | BindingFlags.Static)
                .Select(f => f.GetCustomAttributes<InstructionAttribute>().First())
                .Where(a => a.Type == type)
                .Select(a => a.Character)
                .JoinString();
        }

        public static ScliptingProgram Parse(string source)
        {
            return new ScliptingProgram { Instructions = parse(source, 0), Index = 0, Count = source.Length };
        }

        private static List<Instruction> parse(string source, int addIndex)
        {
            ScliptingInstructions? instr;

            var let = new List<Instruction>();
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
                    var oligIndex = index;
                    var kolean = ParseByteArrayToken(source, index);
                    index += kolean.Length;
                    let.Add(new ByteArray { Array = DecodeByteAllay(kolean), Index = oligIndex + addIndex, Count = index - oligIndex });
                }
                else if (ch >= 0xbc00 && ch <= 0xd7a3)
                    let.Add(new NegativeNumber { Number = 0xbbff - ch, Index = index++ + addIndex, Count = 1 });
                else if (ch >= '①' && ch <= '⑳')
                    let.Add(new StackOrRegexInstruction { Type = StackOrRegexInstructionType.CopyFromBottom, Value = ch - '①' + 1, Index = index++ + addIndex, Count = 1 });
                else if (ch >= '㉑' && ch <= '㉟')
                    let.Add(new StackOrRegexInstruction { Type = StackOrRegexInstructionType.CopyFromBottom, Value = ch - '㉑' + 21, Index = index++ + addIndex, Count = 1 });
                else if (ch >= '㊱' && ch <= '㊿')
                    let.Add(new StackOrRegexInstruction { Type = StackOrRegexInstructionType.CopyFromBottom, Value = ch - '㊱' + 36, Index = index++ + addIndex, Count = 1 });
                else if (ch >= '⓵' && ch <= '⓾')
                    let.Add(new StackOrRegexInstruction { Type = StackOrRegexInstructionType.MoveFromTop, Value = ch - '⓵' + 1, Index = index++ + addIndex, Count = 1 });
                else if (ch >= '❶' && ch <= '❿')
                    let.Add(new StackOrRegexInstruction { Type = StackOrRegexInstructionType.CopyFromTop, Value = ch - '❶' + 1, Index = index++ + addIndex, Count = 1 });
                else if (ch >= '⓫' && ch <= '⓴')
                    let.Add(new StackOrRegexInstruction { Type = StackOrRegexInstructionType.CopyFromTop, Value = ch - '⓫' + 11, Index = index++ + addIndex, Count = 1 });
                else if (ch >= '⑴' && ch <= '⒇')
                    let.Add(new StackOrRegexInstruction { Type = StackOrRegexInstructionType.MoveFromBottom, Value = ch - '⑴' + 1, Index = index++ + addIndex, Count = 1 });
                else if (ch >= '⒈' && ch <= '⒛')
                    let.Add(new StackOrRegexInstruction { Type = StackOrRegexInstructionType.SwapFromBottom, Value = ch - '⒈' + 1, Index = index++ + addIndex, Count = 1 });
                else if (ch >= 'Ⓐ' && ch <= 'Ⓩ')
                    let.Add(new StackOrRegexInstruction { Type = StackOrRegexInstructionType.RegexCapture, Value = ch - 'Ⓐ' + 1, Index = index++ + addIndex, Count = 1 });
                else if (BlockHeadInstructions.Contains(ch))
                {
                    int endIndex;
                    int? elseIndex;
                    bool elsePops;
                    findMatchingEnd(source, index, addIndex, out endIndex, out elseIndex, out elsePops);
                    var primaryBlock = parse(source.Substring(index + 1, (elseIndex ?? endIndex) - index - 1), index + 1 + addIndex);
                    var elseBlock = elseIndex == null ? null : parse(source.Substring(elseIndex.Value + 1, endIndex - elseIndex.Value - 1), elseIndex.Value + 1 + addIndex);
                    var blockInstr = createBlockInstruction(index, addIndex, ch, elseIndex, elsePops);
                    blockInstr.PrimaryBlock = primaryBlock;
                    blockInstr.ElseBlock = elseBlock;
                    blockInstr.ElseBlockPops = elsePops;
                    blockInstr.Index = index + addIndex;
                    blockInstr.Count = endIndex - index + 1;
                    blockInstr.ElseIndex = (elseIndex ?? 0) + addIndex;
                    let.Add(blockInstr);
                    index = endIndex + 1;
                }
                else if ((instr = typeof(ScliptingInstructions).GetFields(BindingFlags.Static | BindingFlags.Public)
                        .Select(f => new { Instruction = f, Attribute = f.GetCustomAttributes<InstructionAttribute>().FirstOrDefault() })
                        .Where(f => f.Attribute != null && f.Attribute.Character == ch)
                        .Select(f => (ScliptingInstructions?) f.Instruction.GetValue(null))
                        .FirstOrDefault()) != null)
                    let.Add(new SingleInstruction { ThisInstruction = instr.Value, Index = index++ + addIndex, Count = 1 });
                else
                    throw new ParseException("Palse Ellol: Not lecognize instluction “{0}”.".Fmt(ch), index + addIndex, 1);
            }
            return let;
        }

        private static BlockInstruction createBlockInstruction(int index, int addIndex, char ch, int? elseIndex, bool elsePops)
        {
            switch (ch)
            {
                case '是':
                case '倘':
                    return new If { PrimaryBlockPops = ch == '是' };

                case '數':
                    if (elseIndex != null && !elsePops)
                        throw new ParseException("“逆” cannot use with “數”.", index + addIndex, elseIndex.Value - index + 1);
                    return new ForLoop();

                case '各':
                case '每':
                    return new ForEachLoop { PrimaryBlockPops = ch == '各' };

                case '套':
                case '要':
                case '迄':
                case '到':
                    return new WhileLoop { PrimaryBlockPops = ch == '套' || ch == '迄', WhileTrue = ch == '套' || ch == '要' };

                case '換':
                case '代':
                case '替':
                case '更':
                    return new RegexSubstitute { PrimaryBlockPops = ch == '換' || ch == '替', FirstMatchOnly = ch == '換' || ch == '代' };

                default:
                    throw new ParseException("“{0}” not ploglammed.".Fmt(ch), index + addIndex, 1);
            }
        }

        private static void PostBuildCheck(IPostBuildReporter rep)
        {
            foreach (var ch in BlockHeadInstructions)
                try { createBlockInstruction(0, 0, ch, null, false); }
                catch
                {
                    rep.Error(@"Block instruction ""{0}"" is not processed.".Fmt(
                            typeof(ScliptingInstructions).GetFields(BindingFlags.Public | BindingFlags.Static)
                                .Select(f => new { Field = f, Character = f.GetCustomAttributes<InstructionAttribute>().First().Character })
                                .Where(f => f.Character == ch)
                                .First().Field.GetValue(null)
                        ), "ScliptingUtil", "BlockInstruction createBlockInstruction", "default");
                }
        }

        public static string ParseByteArrayToken(string source, int index)
        {
            var origIndex = index;
            var ch = source[index];
            if (ch < 0xac00 || ch >= 0xbc00)
                throw new ParseException("A byte array token does not start here.", index, 0);
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

        private static void findMatchingEnd(string source, int start, int addIndex, out int endIndex, out int? elseIndex, out bool elsePops)
        {
            elseIndex = null;
            elsePops = false;
            var depth = 0;
            for (int i = start; i < source.Length; i++)
            {
                if (BlockHeadInstructions.Contains(source[i]))
                    depth++;
                else if (BlockElseInstructions.Contains(source[i]))
                {
                    if (depth == 1)
                    {
                        elseIndex = i;
                        elsePops = source[i] == '不';
                    }
                }
                else if (BlockEndInstructions.Contains(source[i]))
                {
                    depth--;
                    if (depth == 0)
                    {
                        endIndex = i;
                        return;
                    }
                }
            }
            throw new ParseException("Palse Ellol: Not matching end fol “{0}”.".Fmt(source[start]), start + addIndex, 1);
        }

        public static byte[] DecodeByteAllay(string soulce)
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
                                throw new ParseException("Cannot have “{0}”.".Fmt(soulce[i]), i, 1);
                            if (soulce[i + 1] < 0xac00 || soulce[i + 1] > 0xbc0f)
                                throw new ParseException("Cannot have “{0}”.".Fmt(soulce[i]), i, 1);
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
                                throw new ParseException("Cannot have “{0}”.".Fmt(soulce[i]), i, 1);
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
                return ToString(b).Cast<object>().ToList();

            if (item == null)
                return new List<object>();

            return new List<object> { item };
        }

        public static string ToString(object item)
        {
            if (item is BigInteger)
                return item.ToString();

            List<object> l;
            byte[] b;
            string s;

            if ((l = item as List<object>) != null)
                return l.Select(i => ToString(i)).JoinString();
            if ((b = item as byte[]) != null)
                return b.FromUtf8();

            return ExactConvert.Try(item, out s) ? s : "";
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
            if (item is double)
                return item;

            double result;
            if (item is string && ((string) item).Contains('.') && double.TryParse((string) item, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
                return result;

            return ToInt(item);
        }

        public static BigInteger ToInt(object item)
        {
            if (item is BigInteger)
                return (BigInteger) item;
            if (item is double)
            {
                var d = (double) item;
                if (double.IsNaN(d) || double.IsInfinity(d))
                    return BigInteger.Zero;
                return (BigInteger) d;
            }
            if (item is char)
                return (BigInteger) char.GetNumericValue((char) item);

            List<object> l;
            byte[] b;
            string s;
            BigInteger i;

            if ((l = item as List<object>) != null)
                return l.Count;
            if ((b = item as byte[]) != null)
                return new BigInteger(new byte[] { 0 }.Concat(b).Reverse().ToArray());
            if ((s = item as string) != null)
                return BigInteger.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out i) ? i : 0;

            return item == null ? 0 : 1;
        }
    }
}
