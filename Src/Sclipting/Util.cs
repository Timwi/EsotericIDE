using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Reflection;
using RT.PostBuild;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace EsotericIDE.Sclipting
{
    static class ScliptingUtil
    {
        private static Dictionary<char, Instruction> _instructions;
        private static Dictionary<char, NodeType> _instructionTypes;
        private static Dictionary<char, SingularListStringInstructionAttribute> _instructionListStringTypes;

        private static void initInstructionsDictionary()
        {
            if (_instructions == null)
            {
                _instructions = new Dictionary<char, Instruction>();
                _instructionTypes = new Dictionary<char, NodeType>();
                _instructionListStringTypes = new Dictionary<char, SingularListStringInstructionAttribute>();
                foreach (var field in typeof(Instruction).GetFields(BindingFlags.Static | BindingFlags.Public))
                {
                    var attr = field.GetCustomAttributes<InstructionAttribute>().First();
                    if (_instructions.ContainsKey(attr.Character))
                        throw new DuplicateCharacterException(attr.Character, "The character U+{0:X4} is used for more than one instruction ({1} and {2}).".Fmt((int) attr.Character, _instructions[attr.Character], field.GetValue(null)));
                    if (ListStringElementNode.Characters.Contains(attr.Character))
                        throw new DuplicateCharacterException(attr.Character, "The character U+{0:X4} is used for an instruction ({1}) and a list/string element retrieval instruction.".Fmt((int) attr.Character, field.GetValue(null)));
                    _instructions[attr.Character] = (Instruction) field.GetValue(null);
                    _instructionTypes[attr.Character] = attr.Type;
                    var lsAttr = field.GetCustomAttributes<SingularListStringInstructionAttribute>().FirstOrDefault();
                    if (lsAttr != null)
                        _instructionListStringTypes[attr.Character] = lsAttr;
                }
            }
        }

        public static double ToFloat(object item)
        {
            if (item is double)
                return (double) item;
            double result;
            if (item is string && double.TryParse((string) item, out result))
                return result;
            return (double) ToInt(item);
        }

        public static BigInteger ToInt(object item)
        {
            List<object> l;
            byte[] b;
            string s;
            BigInteger i;

            if (item == null || item is Mark || item is Function)
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

            throw new ArgumentException("Unrecognised item type for conversion to int: " + item.GetType().Name, "item");
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

        public static string ToString(object item)
        {
            if (item == null || item is Mark || item is Function)
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

            throw new ArgumentException("Unrecognised item type for conversion to string: " + item.GetType().Name, "item");
        }

        public static bool IsTrue(object item)
        {
            // Shortcut for performance
            byte[] b;
            if ((b = item as byte[]) != null)
                return b.Any(y => y != 0);

            return ToInt(item) != 0;
        }

        public static bool IsNonEmpty(object item)
        {
            List<object> list;
            if ((list = item as List<object>) != null)
                return list.Count != 0;

            // Shortcut for performance
            byte[] b;
            if ((b = item as byte[]) != null)
                return b.Length != 0;

            return ToString(item).Length != 0;
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

        public static void FlipIf(bool doFlip, Action first, Action second)
        {
            if (doFlip) { second(); first(); }
            else { first(); second(); }
        }

        public static bool GetInstructionInfo(char character, out ListStringInstruction listStringType, out bool backwards)
        {
            initInstructionsDictionary();
            SingularListStringInstructionAttribute attr;
            if (_instructionListStringTypes.TryGetValue(character, out attr))
            {
                listStringType = attr.Instruction;
                backwards = attr.Backwards;
                return true;
            }
            else
            {
                listStringType = default(ListStringInstruction);
                backwards = default(bool);
                return false;
            }
        }

        public static bool GetInstructionInfo(char character, out Instruction instruction, out NodeType type)
        {
            initInstructionsDictionary();
            if (_instructions.TryGetValue(character, out instruction))
            {
                type = _instructionTypes[character];
                return true;
            }
            else
            {
                type = default(NodeType);
                return false;
            }
        }

        public static byte[] DecodeByteArray(string source)
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

#if DEBUG
        private static void PostBuildCheck(IPostBuildReporter rep)
        {
            try { initInstructionsDictionary(); }
            catch (DuplicateCharacterException e)
            {
                rep.Error(e.Message, "enum instruction", e.Character.ToString());
                return;
            }
            foreach (var instrKvp in _instructionTypes)
            {
                if (instrKvp.Value == NodeType.BlockHead)
                {
                    try { BlockNode.Create(instrKvp.Key, _instructions[instrKvp.Key], 0, 0, null, null, false); }
                    catch { rep.Error(@"Block instruction ""{0}"" is not processed.".Fmt(instrKvp), "blockNode createBlockNode", "default"); }
                }
                else if (instrKvp.Value == NodeType.FunctionExecutionNode)
                {
                    try { ExecuteFunction.Create(_instructions[instrKvp.Key], 0, 0); }
                    catch { rep.Error(@"Function execution instruction ""{0}"" is not processed.".Fmt(instrKvp), "executeFunction createFunctionExecutionNode", "default"); }
                }
            }
        }
#endif
    }
}
