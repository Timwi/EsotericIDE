using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace EsotericIDE.Sclipting
{
    abstract class Node
    {
        public int Index, Count;
        public abstract IEnumerable<Position> Execute(ScliptingEnv environment);
    }

    sealed class Program : Node
    {
        public List<Node> Instructions;
        public override IEnumerable<Position> Execute(ScliptingEnv environment)
        {
            environment.CurrentStack.Add(environment.Input);
            foreach (var instruction in Instructions)
                foreach (var position in instruction.Execute(environment))
                    yield return position;
            yield return new Position(Index + Count, 0);
            environment.GenerateOutput();
        }
    }

    sealed class ByteArray : Node
    {
        public byte[] Array;
        public override IEnumerable<Position> Execute(ScliptingEnv environment)
        {
            yield return new Position(Index, Count);
            environment.CurrentStack.Add(Array);
        }
    }

    sealed class NegativeNumber : Node
    {
        public BigInteger Number;
        public override IEnumerable<Position> Execute(ScliptingEnv environment)
        {
            yield return new Position(Index, Count);
            environment.CurrentStack.Add(Number);
        }
    }

    sealed class SingularNode : Node
    {
        public Instruction ThisInstruction;

        // This is static and returns a delegate so that the post-build check can test it without needing to execute all the instructions
        private static Action<ScliptingEnv> getMethod(Instruction instr)
        {
            switch (instr)
            {

                // GENERAL

                case Instruction.Discard: return e => { e.Pop(); };
                case Instruction.Abandon: return e => { e.Pop(); e.Pop(); };


                // STRING/LIST MANIPULATION

                case Instruction.Lack: return e => { e.CurrentStack.Add(new List<object>()); };
                case Instruction.Empty: return e => { e.CurrentStack.Add(""); };
                case Instruction.Length: return stringListOperation(true, str => (BigInteger) str.Length, list => (BigInteger) list.Count);
                case Instruction.Long: return stringListOperation(false, str => (BigInteger) str.Length, list => (BigInteger) list.Count);
                case Instruction.Repeat: return repeat(false);
                case Instruction.Extend: return repeat(true);
                case Instruction.RepeatIntoList: return repeatIntoList(false);
                case Instruction.Stretch: return repeatIntoList(true);
                case Instruction.Mark: return e => { e.CurrentStack.Add(new Mark()); };
                case Instruction.CombineString: return combineOperation(true);
                case Instruction.CombineList: return combineOperation(false);
                case Instruction.Combine: return combine(false);
                case Instruction.Blend: return combine(true);
                case Instruction.Child: return sublist(true);
                case Instruction.Part: return sublist(false);
                case Instruction.Start1: return beginningOrEnd(false, false, true);
                case Instruction.Start2: return beginningOrEnd(false, false, false);
                case Instruction.Begin: return beginningOrEnd(false, true, true);
                case Instruction.Beginning: return beginningOrEnd(false, true, false);
                case Instruction.Final: return beginningOrEnd(true, false, true);
                case Instruction.Tail: return beginningOrEnd(true, false, false);
                case Instruction.EndStr: return beginningOrEnd(true, true, true);
                case Instruction.Stop: return beginningOrEnd(true, true, false);
                case Instruction.Reverse: return stringListOperation(true, reverseString,
                    list => { var newList = new List<object>(list); newList.Reverse(); return newList; });
                case Instruction.Sort: return stringListOperation(true, sortString(StringComparer.InvariantCultureIgnoreCase),
                    list => { var newList = new List<object>(list); newList.Sort((a, b) => ScliptingUtil.ToString(a).CompareTo(ScliptingUtil.ToString(b))); return newList; });
                case Instruction.Arrange: return stringListOperation(true, sortString(StringComparer.Ordinal),
                    list => { var newList = new List<object>(list); newList.Sort((a, b) => ScliptingUtil.ToInt(a).CompareTo(ScliptingUtil.ToInt(b))); return newList; });
                case Instruction.Assemble: return assemble;
                case Instruction.Mad: return e => { var listOrString = e.Pop(); e.CurrentStack.Add(randomListOrString(ScliptingUtil.ToInt(e.Pop()), listOrString)); };
                case Instruction.Silly: return e => { var length = ScliptingUtil.ToInt(e.Pop()); e.CurrentStack.Add(randomListOrString(length, e.Pop())); };
                case Instruction.Shuffle: return stringListOperation(true, s => s.ToList().Shuffle().JoinString(), list => list.ToList().Shuffle().ToList());


                // STRING MANIPULATION

                case Instruction.Explain: return e =>
                {
                    var str = ScliptingUtil.ToString(e.Pop());
                    e.CurrentStack.Add(str.Length == 0 ? (object) double.NaN : (BigInteger) str[0]);
                };
                case Instruction.Character: return e =>
                {
                    var codepoint = ScliptingUtil.ToInt(e.Pop());
                    e.CurrentStack.Add(codepoint < 0 || codepoint > 0x10ffff ? "" : char.ConvertFromUtf32((int) codepoint));
                };
                case Instruction.ChangeRegex:
                case Instruction.ChangeCs:
                case Instruction.ChangeCi: return e =>
                {
                    var replacement = ScliptingUtil.ToString(e.Pop());
                    var needle = ScliptingUtil.ToString(e.Pop());
                    var haystack = ScliptingUtil.ToString(e.Pop());
                    e.CurrentStack.Add(
                        instr == Instruction.ChangeRegex
                            ? Regex.Replace(haystack, needle, replacement)
                            : instr == Instruction.ChangeCs
                                ? haystack.Replace(needle, replacement)
                                : haystack.Replace(needle, replacement, StringComparison.InvariantCultureIgnoreCase)
                    );
                };
                case Instruction.Appear: return e => { e.CurrentStack.Add(e.RegexObjects.Count == 0 ? "" : e.RegexObjects.Last().Match.Value); };
                case Instruction.SplitPop: return regexSplit(true);
                case Instruction.SplitNoPop: return regexSplit(false);
                case Instruction.Big: return recursiveStringOperation(s => s.ToUpperInvariant());
                case Instruction.Tiny: return recursiveStringOperation(s => s.ToLowerInvariant());
                case Instruction.Title: return recursiveStringOperation(CultureInfo.InvariantCulture.TextInfo.ToTitleCase);
                case Instruction.Crazy: return e => { e.CurrentStack.Add(Rnd.GenerateString((int) ScliptingUtil.ToInt(e.Pop()), "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz")); };
                case Instruction.Insane: return e => { e.CurrentStack.Add(Rnd.GenerateString((int) ScliptingUtil.ToInt(e.Pop()))); };


                // ARITHMETIC

                case Instruction.Add: return e => { e.NumericOperation((i1, i2) => i1 + i2, (i1, i2) => i1 + i2); };
                case Instruction.Subtract: return e => { e.NumericOperation((i1, i2) => i1 - i2, (i1, i2) => i1 - i2); };
                case Instruction.Reduce: return e => { e.NumericOperation((i1, i2) => i2 - i1, (i1, i2) => i2 - i1); };
                case Instruction.Multiply: return e => { e.NumericOperation((i1, i2) => i1 * i2, (i1, i2) => i1 * i2); };
                case Instruction.DivideFloat: return e => { e.NumericOperation((i1, i2) => i2 == 0 ? double.NaN : (double) i1 / (double) i2, (i1, i2) => i2 == 0 ? double.NaN : i1 / i2); };
                case Instruction.DivideInt: return e => { var item2 = ScliptingUtil.ToInt(e.Pop()); var item1 = ScliptingUtil.ToInt(e.Pop()); e.CurrentStack.Add(item2 == 0 ? (object) double.NaN : item1 / item2); };
                case Instruction.Leftovers: return e => { var item2 = ScliptingUtil.ToInt(e.Pop()); var item1 = ScliptingUtil.ToInt(e.Pop()); e.CurrentStack.Add(item2 == 0 ? (object) double.NaN : item1 % item2); };
                case Instruction.Double: return e => { e.NumericOperation(i => 2 * i, i => 2 * i); };
                case Instruction.Half: return e => { e.NumericOperation(i => (double) i / 2, i => i / 2); };
                case Instruction.Separate: return e => { e.CurrentStack.Add(ScliptingUtil.ToInt(e.Pop()) >> 1); };
                case Instruction.Negative: return e => { e.NumericOperation(i => -i, i => -i); };
                case Instruction.Correct: return e => { e.NumericOperation(i => BigInteger.Abs(i), i => Math.Abs(i)); };
                case Instruction.Increase: return e => { e.CurrentStack.Add(ScliptingUtil.ToInt(e.Pop()) + 1); };
                case Instruction.Decrease: return e => { e.CurrentStack.Add(ScliptingUtil.ToInt(e.Pop()) - 1); };
                case Instruction.Left: return e => { var b = ScliptingUtil.ToInt(e.Pop()); var a = ScliptingUtil.ToInt(e.Pop()); e.CurrentStack.Add(b < 0 ? a >> (int) -b : a << (int) b); };
                case Instruction.Right: return e => { var b = ScliptingUtil.ToInt(e.Pop()); var a = ScliptingUtil.ToInt(e.Pop()); e.CurrentStack.Add(b < 0 ? a << (int) -b : a >> (int) b); };
                case Instruction.Both: return e => { e.CurrentStack.Add(ScliptingUtil.ToInt(e.Pop()) & ScliptingUtil.ToInt(e.Pop())); };
                case Instruction.Other: return e => { e.CurrentStack.Add(ScliptingUtil.ToInt(e.Pop()) | ScliptingUtil.ToInt(e.Pop())); };
                case Instruction.Clever: return e => { e.CurrentStack.Add(ScliptingUtil.ToInt(e.Pop()) ^ ScliptingUtil.ToInt(e.Pop())); };
                case Instruction.BitwiseNot: return e => { e.CurrentStack.Add(~ScliptingUtil.ToInt(e.Pop())); };
                case Instruction.Gnaw: return gnaw(false);
                case Instruction.Bite: return gnaw(true);
                case Instruction.Chaotic: return e => { e.CurrentStack.Add(generateRandomInteger(0, BigInteger.One << 32)); };
                case Instruction.Disarray: return e => { e.CurrentStack.Add(generateRandomInteger(0, ScliptingUtil.ToInt(e.Pop()))); };
                case Instruction.Wild1: return e => { var max = ScliptingUtil.ToInt(e.Pop()); e.CurrentStack.Add(generateRandomInteger(ScliptingUtil.ToInt(e.Pop()), max)); };
                case Instruction.Chaos: return e => { e.CurrentStack.Add(Rnd.NextDouble()); };
                case Instruction.Wild2: return e => { e.CurrentStack.Add(Rnd.NextDouble(0, ScliptingUtil.ToFloat(e.Pop()))); };
                case Instruction.Wild3: return e => { var max = ScliptingUtil.ToFloat(e.Pop()); e.CurrentStack.Add(Rnd.NextDouble(ScliptingUtil.ToFloat(e.Pop()), max)); };
                case Instruction.Number: return e => { e.CurrentStack.Add(Math.Log(ScliptingUtil.ToFloat(e.Pop()))); };
                case Instruction.Position: return e => { e.CurrentStack.Add(Math.Log10(ScliptingUtil.ToFloat(e.Pop()))); };
                case Instruction.Level: return e => { e.CurrentStack.Add(Math.Log(ScliptingUtil.ToFloat(e.Pop())) / Math.Log(2)); };
                case Instruction.Circle1: return e => { e.CurrentStack.Add(Math.Truncate(ScliptingUtil.ToFloat(e.Pop()))); };
                case Instruction.Circle2: return e => { var item = ScliptingUtil.ToFloat(e.Pop()); e.CurrentStack.Add(item < 0 ? Math.Floor(item) : Math.Ceiling(item)); };
                case Instruction.Sphere: return e => { e.CurrentStack.Add(Math.Floor(ScliptingUtil.ToFloat(e.Pop()))); };
                case Instruction.Surround1: return e => { e.CurrentStack.Add(Math.Ceiling(ScliptingUtil.ToFloat(e.Pop()))); };
                case Instruction.Surround2: return e => { e.CurrentStack.Add(Math.Round(ScliptingUtil.ToFloat(e.Pop()), MidpointRounding.AwayFromZero)); };
                case Instruction.Wheel: return e => { e.CurrentStack.Add(Math.Round(ScliptingUtil.ToFloat(e.Pop()), MidpointRounding.ToEven)); };


                // LOGIC

                case Instruction.Small: return e => { e.NumericOperation((i1, i2) => i1 < i2 ? BigInteger.One : BigInteger.Zero, (i1, i2) => i1 < i2 ? BigInteger.One : BigInteger.Zero); };
                case Instruction.Great: return e => { e.NumericOperation((i1, i2) => i1 > i2 ? BigInteger.One : BigInteger.Zero, (i1, i2) => i1 > i2 ? BigInteger.One : BigInteger.Zero); };
                case Instruction.Less: return e => { e.NumericOperation((i1, i2) => i1 <= i2 ? BigInteger.One : BigInteger.Zero, (i1, i2) => i1 <= i2 ? BigInteger.One : BigInteger.Zero); };
                case Instruction.Overflow: return e => { e.NumericOperation((i1, i2) => i1 >= i2 ? BigInteger.One : BigInteger.Zero, (i1, i2) => i1 >= i2 ? BigInteger.One : BigInteger.Zero); };
                case Instruction.And: return e => { var item2 = ScliptingUtil.ToInt(e.Pop()); var item1 = ScliptingUtil.ToInt(e.Pop()); e.CurrentStack.Add(item1 != 0 && item2 != 0 ? BigInteger.One : BigInteger.Zero); };
                case Instruction.Or: return e => { var item2 = ScliptingUtil.ToInt(e.Pop()); var item1 = ScliptingUtil.ToInt(e.Pop()); e.CurrentStack.Add(item1 != 0 || item2 != 0 ? BigInteger.One : BigInteger.Zero); };
                case Instruction.OneOfPair: return e => { var item2 = ScliptingUtil.ToInt(e.Pop()); e.CurrentStack.Add(ScliptingUtil.ToInt(e.Pop()) != 0 ^ item2 != 0 ? BigInteger.One : BigInteger.Zero); };
                case Instruction.Not: return e => { var item = ScliptingUtil.ToInt(e.Pop()); e.CurrentStack.Add(item == 0 ? BigInteger.One : BigInteger.Zero); };
                case Instruction.Same: return e => { var item2 = e.Pop(); e.CurrentStack.Add(e.Pop().Equals(item2) ? BigInteger.One : BigInteger.Zero); };
                case Instruction.Equal: return e => { var item2 = ScliptingUtil.ToInt(e.Pop()); e.CurrentStack.Add(ScliptingUtil.ToInt(e.Pop()) == item2 ? BigInteger.One : BigInteger.Zero); };
                case Instruction.Resemble: return e => { var item2 = ScliptingUtil.ToString(e.Pop()); e.CurrentStack.Add(ScliptingUtil.ToString(e.Pop()) == item2 ? BigInteger.One : BigInteger.Zero); };
                case Instruction.Different1: return e => { var item2 = e.Pop(); e.CurrentStack.Add(e.Pop().Equals(item2) ? BigInteger.Zero : BigInteger.One); };
                case Instruction.Different2: return e => { var item2 = ScliptingUtil.ToInt(e.Pop()); e.CurrentStack.Add(ScliptingUtil.ToInt(e.Pop()) == item2 ? BigInteger.Zero : BigInteger.One); };
                case Instruction.Different3: return e => { var item2 = ScliptingUtil.ToString(e.Pop()); e.CurrentStack.Add(ScliptingUtil.ToString(e.Pop()) == item2 ? BigInteger.Zero : BigInteger.One); };
                case Instruction.IsIt: return e => { var no = e.Pop(); var yes = e.Pop(); e.CurrentStack.Add(ScliptingUtil.IsTrue(e.Pop()) ? yes : no); };
                case Instruction.Power: return e => { e.NumericOperation((i1, i2) => i2 < 0 ? (object) Math.Pow((double) i1, (double) i2) : BigInteger.Pow(i1, (int) i2), (i1, i2) => Math.Pow(i1, i2)); };
                case Instruction.Flat: return e => { e.NumericOperation(i => i * i, d => d * d); };
                case Instruction.Root: return e => { e.NumericOperation(i => Math.Sqrt((double) i), d => Math.Sqrt(d)); };


                default:
                    throw new InternalErrorException("Unknown instruction: “{0}”".Fmt(instr));
            }
        }

        private static Action<ScliptingEnv> beginningOrEnd(bool end, bool countingBackwards, bool pop)
        {
            return e =>
            {
                var index = (int) ScliptingUtil.ToInt(e.Pop());
                var item = pop ? e.Pop() : e.CurrentStack.Last();
                var list = (item as List<object>) ?? (ScliptingUtil.ToString(item).Select(ch => (object) ch.ToString()).ToList());
                var result = new List<object>();
                result.AddRange(end
                    ? list.Skip(countingBackwards ? index : list.Count - 1 - index)
                    : list.Take(countingBackwards ? list.Count - 1 - index : index));
                e.CurrentStack.Add(item is List<object> ? (object) result : result.JoinString());
            };
        }

        private static object randomListOrString(BigInteger length, object listOrString)
        {
            var list = (listOrString as List<object>) ?? (ScliptingUtil.ToString(listOrString).Select(ch => (object) ch.ToString()).ToList());
            var result = new List<object>();
            var intLength = (int) length;
            while (intLength-- > 0)
                result.Add(list[Rnd.Next(list.Count)]);
            return listOrString is List<object> ? (object) result : result.JoinString();
        }

        private static BigInteger generateRandomInteger(BigInteger min, BigInteger max)
        {
            if (max < min)
                return min;
            tryAgain:
            var range = max - min;
            var bits = 0;
            while (range > BigInteger.One << bits)
                bits++;
            var randomBits = Rnd.NextBytes(bits / 8 + 1);
            randomBits[randomBits.Length - 1] &= (byte) ((1 << (bits % 8)) - 1);
            var randomInteger = new BigInteger(randomBits);
            if (randomInteger >= range)
                goto tryAgain;
            return randomInteger + min;
        }

        private static Action<ScliptingEnv> recursiveStringOperation(Func<string, string> func)
        {
            Func<object, object> recurse = null;
            recurse = (object item) =>
            {
                var list = item as List<object>;
                if (list != null)
                    return list.Select(recurse).ToList();
                return func(ScliptingUtil.ToString(item));
            };
            return e =>
            {
                var item = e.Pop();
                e.CurrentStack.Add(recurse(item));
            };
        }

        private static Action<ScliptingEnv> gnaw(bool reversed)
        {
            return e =>
            {
                var b = (int) ScliptingUtil.ToInt(e.Pop());
                var a = ScliptingUtil.ToInt(e.Pop());
                ScliptingUtil.FlipIf(reversed,
                    () => e.CurrentStack.Add(a & ((BigInteger.One << b) - BigInteger.One)),
                    () => e.CurrentStack.Add(a >> b));
            };
        }

        private static Func<string, string> sortString(StringComparer comparer)
        {
            return s =>
            {
                var list = s.Select(c => c.ToString()).ToList();
                list.Sort(comparer);
                return new string(list.Select(st => st[0]).ToArray());
            };
        }

        private static Action<ScliptingEnv> regexSplit(bool pop)
        {
            return e =>
            {
                var regex = ScliptingUtil.ToString(e.Pop());
                var input = ScliptingUtil.ToString(e.CurrentStack.Last());
                if (pop)
                    e.Pop();
                e.CurrentStack.Add(new List<object>(Regex.Split(input, regex, RegexOptions.Singleline)));
            };
        }

        private static string reverseString(string input)
        {
            var newArr = new char[input.Length];
            var len = input.Length - 1;
            for (int i = len; i >= 0; i--)
                newArr[len - i] = input[i];
            return new string(newArr);
        }

        private static Action<ScliptingEnv> repeat(bool reversedArguments)
        {
            return e =>
            {
                List<object> list;
                byte[] b;
                int numTimes;
                object item;

                var item1 = e.Pop();
                var item2 = e.Pop();

                if (reversedArguments)
                {
                    numTimes = (int) ScliptingUtil.ToInt(item2);
                    item = item1;
                }
                else
                {
                    numTimes = (int) ScliptingUtil.ToInt(item1);
                    item = item2;
                }
                if (numTimes < 1)
                    e.CurrentStack.Add("");
                else if ((list = item as List<object>) != null)
                {
                    var newList = new List<object>(list.Count * numTimes);
                    while (numTimes-- > 0)
                        newList.AddRange(list);
                    e.CurrentStack.Add(newList);
                }
                else if ((b = item as byte[]) != null)
                {
                    var newByteArray = new byte[b.Length * numTimes];
                    while (--numTimes >= 0)
                        Array.Copy(b, 0, newByteArray, numTimes * b.Length, b.Length);
                    e.CurrentStack.Add(newByteArray);
                }
                else
                    e.CurrentStack.Add(ScliptingUtil.ToString(item).Repeat(numTimes));
            };
        }

        private static Action<ScliptingEnv> repeatIntoList(bool reversedArguments)
        {
            return e =>
            {
                int numTimes;
                object item;

                var item1 = e.Pop();
                var item2 = e.Pop();

                if (reversedArguments)
                {
                    numTimes = (int) ScliptingUtil.ToInt(item2);
                    item = item1;
                }
                else
                {
                    numTimes = (int) ScliptingUtil.ToInt(item1);
                    item = item2;
                }
                if (numTimes < 1)
                    e.CurrentStack.Add(new List<object>());
                else
                {
                    var newList = new List<object>(numTimes);
                    while (numTimes-- > 0)
                        newList.Add(item);
                    e.CurrentStack.Add(newList);
                }
            };
        }

        private static Action<ScliptingEnv> stringListOperation(bool pop, Func<string, object> stringOperation, Func<List<object>, object> listOperation)
        {
            return e =>
            {
                List<object> list;

                var item = pop ? e.Pop() : e.CurrentStack.Last();
                if ((list = item as List<object>) != null)
                    e.CurrentStack.Add(listOperation(list));
                else
                    e.CurrentStack.Add(stringOperation(ScliptingUtil.ToString(item)));
            };
        }

        private static Action<ScliptingEnv> stringListOperation(bool pop, Func<string, BigInteger, object> stringOperation, Func<List<object>, BigInteger, object> listOperation)
        {
            return e =>
            {
                List<object> list;

                var integer = ScliptingUtil.ToInt(e.Pop());
                var item = pop ? e.Pop() : e.CurrentStack.Last();
                if ((list = item as List<object>) != null)
                    e.CurrentStack.Add(listOperation(list, integer));
                else
                    e.CurrentStack.Add(stringOperation(ScliptingUtil.ToString(item), integer));
            };
        }

        private static Action<ScliptingEnv> combine(bool reverse)
        {
            return e =>
            {
                var item2 = e.Pop();
                var item1 = e.Pop();
                if (reverse) { var t = item1; item1 = item2; item2 = t; }
                if (item1 is List<object> && item2 is List<object>)
                    e.CurrentStack.Add(new List<object>(((List<object>) item1).Concat((List<object>) item2)));
                else
                    e.CurrentStack.Add(ScliptingUtil.ToString(item1) + ScliptingUtil.ToString(item2));
            };
        }

        private static Action<ScliptingEnv> sublist(bool pop)
        {
            return e =>
            {
                var length = (int) ScliptingUtil.ToInt(e.Pop());
                var index = (int) ScliptingUtil.ToInt(e.Pop());
                var item = pop ? e.Pop() : e.CurrentStack.Last();
                var list = item as List<object>;
                if (list != null)
                {
                    var result = new List<object>();
                    for (var i = (int) index; i < (int) length; i++)
                        result.Add(i >= 0 && i < list.Count ? list[i] : "");
                    e.CurrentStack.Add(result);
                }
                else
                {
                    var str = ScliptingUtil.ToString(item);
                    var result = new StringBuilder();
                    for (var i = index; i < index + length; i++)
                        result.Append(i >= 0 && i < str.Length ? str[i] : ' ');
                    e.CurrentStack.Add(result.ToString());
                }
            };
        }

        private static void assemble(ScliptingEnv e)
        {
            var separator = ScliptingUtil.ToString(e.Pop());
            var item = e.Pop();
            var list = item is List<object> ? ((List<object>) item).Select(obj => ScliptingUtil.ToString(obj)) : ScliptingUtil.ToString(item).Select(ch => (object) ch.ToString());
            e.CurrentStack.Add(list.JoinString(separator));
        }

        private static Action<ScliptingEnv> insert(bool replace)
        {
            return e =>
            {
                var item = e.Pop();
                var bigInteger = ScliptingUtil.ToInt(e.Pop());
                if (bigInteger < 0 || bigInteger > int.MaxValue)
                    return;
                var listOrString = e.Pop();

                var integer = (int) bigInteger;
                var list = listOrString as List<object>;
                if (list != null)
                {
                    var newList = new List<object>(list);
                    if (replace)
                    {
                        while (integer >= newList.Count)
                            newList.Add("");
                        newList[integer] = item;
                    }
                    else
                    {
                        while (integer > newList.Count)
                            newList.Add("");
                        newList.Insert(integer, item);
                    }
                    e.CurrentStack.Add(newList);
                }
                else
                {
                    // assume string
                    var itemAsStr = ScliptingUtil.ToString(item);
                    var itemAsChar = itemAsStr == "" ? ' ' : itemAsStr[0];
                    var input = ScliptingUtil.ToString(listOrString);

                    if (input.Length < integer)
                        input += new string(' ', integer - input.Length) + itemAsChar;
                    else if (input.Length == integer)
                        input += itemAsChar;
                    else if (replace)
                        input = input.Substring(0, integer) + itemAsChar + input.Substring(integer + 1);
                    else
                        input = input.Substring(0, integer) + itemAsChar + input.Substring(integer);
                    e.CurrentStack.Add(input);
                }
            };
        }

        private static void annihilate(ScliptingEnv e)
        {
            var bigInteger = ScliptingUtil.ToInt(e.Pop());
            if (bigInteger < 0 || bigInteger > int.MaxValue)
                return;
            var listOrString = e.Pop();

            var integer = (int) bigInteger;
            var list = listOrString as List<object>;
            if (list != null)
            {
                var newList = new List<object>(list);
                if (integer < newList.Count)
                    newList.RemoveAt(integer);
                e.CurrentStack.Add(newList);
            }
            else
            {
                // assume string
                var input = ScliptingUtil.ToString(listOrString);
                // make sure we push the original object (not convert to string) if out of range
                e.CurrentStack.Add(integer >= input.Length ? listOrString : input.Substring(0, integer) + input.Substring(integer + 1));
            }
        }

        private static Action<ScliptingEnv> combineOperation(bool stringify)
        {
            return e =>
            {
                var index = e.CurrentStack.Count - 1;
                while (index >= 0 && !(e.CurrentStack[index] is Mark))
                    index--;
                var items = new List<object>(Enumerable.Range(index + 1, e.CurrentStack.Count - index - 1).Select(i => e.CurrentStack[i]));
                if (index == -1)
                    index = 0;
                e.CurrentStack.RemoveRange(index, e.CurrentStack.Count - index);
                e.CurrentStack.Add(stringify
                    ? (object) items.Select(item => ScliptingUtil.ToString(item)).JoinString()
                    : (object) items);
            };
        }

        public override IEnumerable<Position> Execute(ScliptingEnv environment)
        {
            yield return new Position(Index, Count);
            getMethod(ThisInstruction)(environment);
        }

#if DEBUG
        private static void PostBuildCheck(IPostBuildReporter rep)
        {
            foreach (var field in typeof(Instruction).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var attr = field.GetCustomAttributes<InstructionAttribute>().First();
                var instr = (Instruction) field.GetValue(null);
                if (attr.Type == NodeType.SingularNode && !field.IsDefined<SingularListStringInstructionAttribute>())
                {
                    try { getMethod(instr); }
                    catch { rep.Error(@"Instruction ""{0}"" has no method.".Fmt(instr), "singularNode", "getMethod", "default"); }
                }
            }
        }
#endif
    }

    sealed class StackOrRegexNode : Node
    {
        public StackOrRegexNodeType Type;
        public int Value;

        public override IEnumerable<Position> Execute(ScliptingEnv environment)
        {
            yield return new Position(Index, Count);
            object item;
            switch (Type)
            {
                case StackOrRegexNodeType.RegexCapture:
                    if (environment.RegexObjects.Count == 0 || environment.RegexObjects.Last().Match.Groups.Count <= Value)
                        environment.CurrentStack.Add("");
                    else
                        environment.CurrentStack.Add(environment.RegexObjects.Last().Match.Groups[Value].Value);
                    break;
                default:
                    if (environment.CurrentStack.Count < Value)
                        environment.CurrentStack.Add("");
                    else
                    {
                        switch (Type)
                        {
                            case StackOrRegexNodeType.CopyFromTop:
                                environment.CurrentStack.Add(environment.CurrentStack[environment.CurrentStack.Count - Value]);
                                break;
                            case StackOrRegexNodeType.CopyFromBottom:
                                environment.CurrentStack.Add(environment.CurrentStack[Value - 1]);
                                break;
                            case StackOrRegexNodeType.MoveFromTop:
                                item = environment.CurrentStack[environment.CurrentStack.Count - Value];
                                environment.CurrentStack.RemoveAt(environment.CurrentStack.Count - Value);
                                environment.CurrentStack.Add(item);
                                break;
                            case StackOrRegexNodeType.MoveFromBottom:
                                environment.CurrentStack.Add(environment.CurrentStack[Value - 1]);
                                environment.CurrentStack.RemoveAt(Value - 1);
                                break;
                            case StackOrRegexNodeType.SwapFromBottom:
                                item = environment.CurrentStack[Value - 1];
                                environment.CurrentStack[Value - 1] = environment.CurrentStack[environment.CurrentStack.Count - 1];
                                environment.CurrentStack[environment.CurrentStack.Count - 1] = item;
                                break;
                        }
                    }
                    break;
            }
        }
    }

    public enum ListStringInstruction
    {
        [ListStringInstruction("Retrieve item/character from list/string (pop)")]
        RetrievePop,
        [ListStringInstruction("Retrieve item/character from list/string (no pop)")]
        RetrieveNoPop,
        [ListStringInstruction("Insert item/character in list/string")]
        Insert,
        [ListStringInstruction("Delete item/character from list/string")]
        Delete,
        [ListStringInstruction("Retrieve and delete item/character from list/string")]
        RetrieveDelete,
        [ListStringInstruction("Replace item/character in list/string")]
        Replace,
        [ListStringInstruction("Exchange item/character in list/string")]
        Exchange
    }

    sealed class ListStringElementNode : Node
    {
        public ListStringInstruction Instruction { get; private set; }
        public int ListStringIndex { get; private set; }
        public bool GetIndexFromStack { get; private set; }
        public bool Backwards { get; private set; }

        public const string Characters =
            @"一二三四五六七八九十乾兌離震巽坎艮坤陰陽" +   // RetrievePop
            @"壹貳叁肆伍陸柒捌玖拾首跟副矩手蟜週蛛貓指" +   // RetrieveNoPop
            @"氫氦鋰鈹硼碳氮氧氟氖鈉鎂鋁矽磷硫氯氬鉀鈣" +   // Insert
            @"鈧鈦釩鉻錳鐵鈷鎳銅鋅鎵鍺砷硒溴氪銣鍶釔鋯" +   // Delete
            @"鈮鉬鎝釕銠鈀銀鎘銦錫銻碲碘氙銫鋇鑭鈰鐠釹" +   // RetrieveDelete
            @"鉕釤銪釓鋱鏑鈥鉺銩鐿鎦鉿鉭鎢錸鋨銥鉑金汞" +   // Replace
            @"鉈鉛鉍釙砈氡鍅鐳錒釷鏷鈾錼鈽鋂鋦鉳鉲鑀鐨";     // Exchange

        public static readonly string[] Engrish = Ut.NewArray(
            "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten",
            "sky", "lake", "fire", "thunder", "wind", "water", "mountain", "earth", "yin", "yang",
            "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten",
            "head", "follow", "supplement", "square", "hand", "insect", "week", "spider", "cat", "finger",
            "hydrogen", "helium", "lithium", "beryllium", "boron", "carbon", "nitrogen", "oxygen", "fluorine", "neon",
            "sodium", "magnesium", "aluminium", "silicon", "phosphorus", "sulfur", "chlorine", "argon", "potassium", "calcium",
            "scandium", "titanium", "vanadium", "chromium", "manganese", "iron", "cobalt", "nickel", "copper", "zinc",
            "gallium", "germanium", "arsenic", "selenium", "bromine", "krypton", "rubidium", "strontium", "yttrium", "zirconium",
            "niobium", "molybdenum", "technetium", "ruthenium", "rhodium", "palladium", "silver", "cadmium", "indium", "tin",
            "antimony", "tellurium", "iodine", "xenon", "caesium", "barium", "lanthanum", "cerium", "praseodymium", "neodymium",
            "promethium", "samarium", "europium", "gadolinium", "terbium", "dysprosium", "holmium", "erbium", "thulium", "ytterbium",
            "lutetium", "hafnium", "tantalum", "tungsten", "rhenium", "osmium", "iridium", "platinum", "gold", "mercury",
            "thallium", "lead", "bismuth", "polonium", "astatine", "radon", "francium", "radium", "actinium", "thorium",
            "protactinium", "uranium", "neptunium", "plutonium", "americium", "curium", "berkelium", "californium", "einsteinium", "fermium");

#if DEBUG
        private static void PostBuildCheck(IPostBuildReporter rep)
        {
            if (Characters.Length != Engrish.Length)
                rep.Error("listStringElementNode: Number of characters ({0}) does not equal number of Engrish terms ({1}).".Fmt(Characters.Length, Engrish.Length), "class listStringElementNode", "Characters");
            var numInstr = EnumStrong.GetValues<ListStringInstruction>().Length;
            if (Characters.Length != 20 * numInstr)
                rep.Error("listStringElementNode: Number of characters ({0}) does not equal 20 times the number of list/string instructions ({1}).".Fmt(Characters.Length, numInstr), "class listStringElementNode", "Engrish");
        }
#endif

        public ListStringElementNode(char ch, int sourceIndex)
        {
            ListStringIndex = Characters.IndexOf(ch);
            if (ListStringIndex == -1)
                throw new ArgumentException("The specified character is not a valid list/string element retrieval instruction.", "ch");
            Instruction = (ListStringInstruction) (ListStringIndex / 20);
            ListStringIndex %= 20;
            Backwards = ListStringIndex >= 10;
            if (Backwards)
                ListStringIndex -= 10;
            GetIndexFromStack = false;
            Index = sourceIndex;
            Count = 1;
        }

        public ListStringElementNode(ListStringInstruction instruction, bool backwards, int sourceIndex)
        {
            Instruction = instruction;
            Backwards = backwards;
            GetIndexFromStack = true;
            Index = sourceIndex;
            Count = 1;
        }

        public override IEnumerable<Position> Execute(ScliptingEnv environment)
        {
            yield return new Position(Index, Count);

            switch (Instruction)
            {
                case ListStringInstruction.RetrievePop:
                case ListStringInstruction.RetrieveNoPop:
                    {
                        if (GetIndexFromStack)
                            ListStringIndex = (int) ScliptingUtil.ToInt(environment.Pop());
                        var listOrString = Instruction == ListStringInstruction.RetrievePop ? environment.Pop() : environment.CurrentStack.Last();
                        operation(listOrString, false, false,
                            (s, i) => { environment.CurrentStack.Add(i >= 0 && i < s.Length ? s[i].ToString() : ""); },
                            (l, i) => { environment.CurrentStack.Add(i >= 0 && i < l.Count ? l[i] : ""); });
                    }
                    break;
                case ListStringInstruction.Insert:
                    {
                        var item = environment.Pop();
                        if (GetIndexFromStack)
                            ListStringIndex = (int) ScliptingUtil.ToInt(environment.Pop());
                        var listOrString = environment.Pop();
                        operation(listOrString, true, false,
                            (s, i) =>
                            {
                                var j = Backwards ? i + 1 : i;
                                var itemAsString = ScliptingUtil.ToString(item);
                                var itemAsChar = itemAsString.Length == 0 ? ' ' : itemAsString[0];
                                environment.CurrentStack.Add(s.Substring(0, j) + itemAsChar + s.Substring(j));
                            },
                            (l, i) =>
                            {
                                l.Insert(Backwards ? i + 1 : i, item);
                                environment.CurrentStack.Add(l);
                            });
                    }
                    break;
                case ListStringInstruction.Delete:
                    {
                        if (GetIndexFromStack)
                            ListStringIndex = (int) ScliptingUtil.ToInt(environment.Pop());
                        var listOrString = environment.Pop();
                        operation(listOrString, false, false,
                            (s, i) =>
                            {
                                // make sure we push the original object (not convert to string) if out of range
                                environment.CurrentStack.Add(i >= 0 && i < s.Length ? s.Remove(i, 1) : listOrString);
                            },
                            (l, i) =>
                            {
                                if (i >= 0 && i < l.Count)
                                    l.RemoveAt(i);
                                environment.CurrentStack.Add(l);
                            });
                    }
                    break;
                case ListStringInstruction.RetrieveDelete:
                    {
                        if (GetIndexFromStack)
                            ListStringIndex = (int) ScliptingUtil.ToInt(environment.Pop());
                        var listOrString = environment.Pop();
                        operation(listOrString, false, false,
                            (s, i) =>
                            {
                                var ch = (i >= 0 && i < s.Length ? s[i].ToString() : "");
                                // make sure we push the original object (not convert to string) if out of range
                                environment.CurrentStack.Add(i >= 0 && i < s.Length ? s.Remove(i, 1) : listOrString);
                                environment.CurrentStack.Add(ch);
                            },
                            (l, i) =>
                            {
                                var item = i >= 0 && i < l.Count ? l[i] : "";
                                if (i >= 0 && i < l.Count)
                                    l.RemoveAt(i);
                                environment.CurrentStack.Add(l);
                                environment.CurrentStack.Add(item);
                            });
                    }
                    break;
                case ListStringInstruction.Replace:
                    {
                        var item = environment.Pop();
                        if (GetIndexFromStack)
                            ListStringIndex = (int) ScliptingUtil.ToInt(environment.Pop());
                        var listOrString = environment.Pop();
                        operation(listOrString, false, true,
                            (s, i) =>
                            {
                                var itemAsString = ScliptingUtil.ToString(item);
                                var itemAsChar = itemAsString.Length == 0 ? ' ' : itemAsString[0];
                                environment.CurrentStack.Add(s.Substring(0, i) + itemAsChar + s.Substring(i + 1));
                            },
                            (l, i) =>
                            {
                                l[i] = item;
                                environment.CurrentStack.Add(l);
                            });
                    }
                    break;
                case ListStringInstruction.Exchange:
                    {
                        var item = environment.Pop();
                        if (GetIndexFromStack)
                            ListStringIndex = (int) ScliptingUtil.ToInt(environment.Pop());
                        var listOrString = environment.Pop();
                        operation(listOrString, false, true,
                            (s, i) =>
                            {
                                var itemAsString = ScliptingUtil.ToString(item);
                                var itemAsChar = itemAsString.Length == 0 ? ' ' : itemAsString[0];
                                var prevChar = s[i].ToString();
                                environment.CurrentStack.Add(s.Substring(0, i) + itemAsChar + s.Substring(i + 1));
                                environment.CurrentStack.Add(prevChar);
                            },
                            (l, i) =>
                            {
                                environment.CurrentStack.Add(l);
                                environment.CurrentStack.Add(l[i]);
                                l[i] = item;    // sneaky: modify list after putting it on the stack...
                            });
                    }
                    break;

                default:
                    throw new InvalidOperationException("Invalid list/string manipulation instruction encountered.");
            }
        }

        private void operation(object listOrString, bool padExcl, bool padIncl, Action<string, int> stringOperation, Action<List<object>, int> listOperation)
        {
            var list = listOrString as List<object>;
            if (list != null)
            {
                var index = Backwards ? list.Count - 1 - ListStringIndex : ListStringIndex;
                var newList = new List<object>(list);
                if (Backwards && (padExcl || padIncl))
                {
                    while (index < (padExcl ? -1 : 0))
                    {
                        newList.Insert(0, "");
                        index++;
                    }
                }
                else if (!Backwards)
                {
                    while (padExcl ? index > newList.Count : padIncl ? index >= newList.Count : false)
                        newList.Add("");
                }
                listOperation(newList, index);
            }
            else
            {
                var str = ScliptingUtil.ToString(listOrString);
                var index = Backwards ? str.Length - 1 - ListStringIndex : ListStringIndex;
                var padding =
                    !Backwards && padExcl && index > str.Length ? new string(' ', index - str.Length) :
                    !Backwards && padIncl && index >= str.Length ? new string(' ', index + 1 - str.Length) :
                    index < 0 && (padExcl || padIncl) ? new string(' ', -index) : null;
                stringOperation(Backwards ? padding + str : str + padding, index < 0 && (padExcl || padIncl) ? 0 : index);
            }
        }

        public string Explain()
        {
            string ordinal;
            switch (ListStringIndex)
            {
                case 0: ordinal = Backwards ? "last" : "first"; break;
                case 1: ordinal = Backwards ? "second-last" : "second"; break;
                case 2: ordinal = Backwards ? "third-last" : "third"; break;
                default: ordinal = (ListStringIndex + 1) + (Backwards ? "th-last" : "th"); break;
            }

            return (
                Instruction == ListStringInstruction.RetrievePop ? "Retrieve {0} item/character from list/string (pop)." :
                Instruction == ListStringInstruction.RetrieveNoPop ? "Retrieve {0} item/character from list/string (no pop)." :
                Instruction == ListStringInstruction.Insert ? "Insert item/character at {0} position in list/string." :
                Instruction == ListStringInstruction.Delete ? "Delete {0} item/character from list/string." :
                Instruction == ListStringInstruction.RetrieveDelete ? "Retrieve and delete {0} item/character from list/string." :
                Instruction == ListStringInstruction.Replace ? "Replace {0} item/character in list/string." :
                Instruction == ListStringInstruction.Exchange ? "Exchange {0} item/character in list/string." : null)
                .Fmt(ordinal);
        }

        public static ToolStripItem[] GetMenuItems(IIde ide)
        {
            return typeof(ListStringInstruction).GetFields(BindingFlags.Static | BindingFlags.Public)
                .Select((field, fi) =>
                {
                    var item = new ToolStripMenuItem(field.GetCustomAttributes<ListStringInstructionAttribute>().First().MenuLabel);
                    item.DropDownItems.AddRange(Enumerable.Range(0, 20)
                        .Select(i => new { Ch = Characters[fi * 20 + i], Engrish = Engrish[fi * 20 + i] })
                        .Select(inf => new ToolStripMenuItem("{0} &{1} — {2}".Fmt(inf.Ch, inf.Engrish, new ListStringElementNode(inf.Ch, 0).Explain()), null, (_, __) => { ide.InsertText(inf.Ch.ToString()); }))
                        .ToArray());
                    return item;
                })
                .ToArray();
        }
    }

    abstract class BlockNode : Node
    {
        public List<Node> PrimaryBlock;
        public List<Node> ConditionBlock;
        public List<Node> ElseBlock;
        public bool PrimaryBlockPops, ElseBlockPops;
        public int ElseIndex;
        public int ConditionEndIndex;

        public static BlockNode Create(char instrCh, Instruction instr, int index, int addIndex, int? conditionEndIndex, int? elseIndex, bool elsePops)
        {
            // Only few instructions allow a conditionEndIndex, so check that first.
            switch (instr)
            {
                case Instruction.Loop:
                case Instruction.Necessity:
                case Instruction.Until:
                case Instruction.Arrive:
                case Instruction.Full:
                case Instruction.BeFull:
                    break;

                default:
                    if (conditionEndIndex != null)
                        throw new CompileException("The condition-block instruction “況” cannot be used with the block head instruction “{0}”.".Fmt(instrCh), index + addIndex, conditionEndIndex.Value - index + 1);
                    break;
            }

            switch (instr)
            {
                case Instruction.Yes: return new IfBlock { PrimaryBlockPops = true, Condition = Condition.True };
                case Instruction.If: return new IfBlock { PrimaryBlockPops = false, Condition = Condition.True };
                case Instruction.NotPop: return new IfBlock { PrimaryBlockPops = true, Condition = Condition.False };
                case Instruction.NotNoPop: return new IfBlock { PrimaryBlockPops = false, Condition = Condition.False };
                case Instruction.Enough: return new IfBlock { PrimaryBlockPops = true, Condition = Condition.NonEmpty };
                case Instruction.Contain: return new IfBlock { PrimaryBlockPops = false, Condition = Condition.NonEmpty };

                case Instruction.Loop: return new WhileLoop { PrimaryBlockPops = true, Condition = Condition.True };
                case Instruction.Necessity: return new WhileLoop { PrimaryBlockPops = false, Condition = Condition.True };
                case Instruction.Until: return new WhileLoop { PrimaryBlockPops = true, Condition = Condition.False };
                case Instruction.Arrive: return new WhileLoop { PrimaryBlockPops = false, Condition = Condition.False };
                case Instruction.Full: return new WhileLoop { PrimaryBlockPops = true, Condition = Condition.NonEmpty };
                case Instruction.BeFull: return new WhileLoop { PrimaryBlockPops = false, Condition = Condition.NonEmpty };

                case Instruction.Up:
                case Instruction.Down:
                    if (elseIndex != null && !elsePops)
                        throw new CompileException("The block else instruction “逆” cannot be used with the block head instruction “數”.", index + addIndex, elseIndex.Value - index + 1);
                    return new ForLoop { Backwards = instr == Instruction.Down };

                case Instruction.Each: return new ForEachLoop { PrimaryBlockPops = true };
                case Instruction.Every: return new ForEachLoop { PrimaryBlockPops = false };

                case Instruction.ReplaceRegexFirstPop:
                case Instruction.ReplaceRegexFirstNoPop:
                case Instruction.ReplaceRegexAllPop:
                case Instruction.ReplaceRegexAllNoPop:
                case Instruction.ReplaceCsSubstrFirstPop:
                case Instruction.ReplaceCsSubstrFirstNoPop:
                case Instruction.ReplaceCsSubstrAllPop:
                case Instruction.ReplaceCsSubstrAllNoPop:
                case Instruction.ReplaceCiSubstrFirstPop:
                case Instruction.ReplaceCiSubstrFirstNoPop:
                case Instruction.ReplaceCiSubstrAllPop:
                case Instruction.ReplaceCiSubstrAllNoPop:
                    return new RegexSubstitute(instr);

                case Instruction.Block:
                    if (elseIndex != null)
                        throw new CompileException("The block instruction “塊” cannot have a “不” or “逆” block.", index + addIndex, elseIndex.Value - index + 1);
                    return new FunctionNode { Capture = false };
                case Instruction.Capture:
                    if (elseIndex != null)
                        throw new CompileException("The block instruction “掳” cannot have a “不” or “逆” block.", index + addIndex, elseIndex.Value - index + 1);
                    return new FunctionNode { Capture = true };

                case Instruction.Snap: return new SplitLoop { Backward = false, PrimaryBlockPops = true };
                case Instruction.Break: return new SplitLoop { Backward = true, PrimaryBlockPops = true };
                case Instruction.Rupture: return new SplitLoop { Backward = false, PrimaryBlockPops = false };
                case Instruction.Sever: return new SplitLoop { Backward = true, PrimaryBlockPops = false };

                default:
                    throw new CompileException("Instruction “{0}” missing.".Fmt(instr), index + addIndex, 1);
            }
        }
    }

    sealed class FunctionNode : BlockNode
    {
        public bool Capture;
        public override IEnumerable<Position> Execute(ScliptingEnv environment)
        {
            yield return new Position(Index, Count);
            environment.CurrentStack.Add(new Function { FunctionCode = this, CapturedItem = Capture ? environment.Pop() : null });
        }
    }

    sealed class ExecuteFunction : Node
    {
        public Instruction Instruction;

        public override IEnumerable<Position> Execute(ScliptingEnv environment)
        {
            yield return new Position(Index, 1);
            var item = Instruction == Instruction.Perform ? environment.CurrentStack.Last() : environment.Pop();
            var fnc = item as Function;
            if (fnc != null)
            {
                yield return new Position(fnc.FunctionCode.Index, 1);
                if (fnc.CapturedItem != null)
                    environment.CurrentStack.Add(fnc.CapturedItem);
                foreach (var instr in fnc.FunctionCode.PrimaryBlock)
                    foreach (var pos in instr.Execute(environment))
                        yield return pos;
                yield return new Position(fnc.FunctionCode.Index + fnc.FunctionCode.Count - 1, 1);
            }
            if (Instruction == Instruction.Handle)
            {
                yield return new Position(Index, 1);
                environment.CurrentStack.Add(item);
            }
        }

        public static ExecuteFunction Create(Instruction instr, int index, int addIndex)
        {
            switch (instr)
            {
                case Instruction.Initiate:
                case Instruction.Handle:
                case Instruction.Perform:
                    return new ExecuteFunction { Instruction = instr };

                default:
                    throw new CompileException("Instruction “{0}” missing.".Fmt(instr), index + addIndex, 1);
            }
        }
    }

    sealed class ForLoop : BlockNode
    {
        public bool Backwards;
        public override IEnumerable<Position> Execute(ScliptingEnv environment)
        {
            yield return new Position(Index, 1);
            var b = ScliptingUtil.ToInt(environment.Pop());
            var a = ScliptingUtil.ToInt(environment.Pop());
            if (Backwards ? (a < b) : (a > b))
            {
                // “Else” block
                if (ElseBlock != null)
                {
                    yield return new Position(ElseIndex, 1);
                    foreach (var instr in ElseBlock)
                        foreach (var pos in instr.Execute(environment))
                            yield return pos;
                }
                // “End” instruction
                yield return new Position(Index + Count - 1, 1);
            }
            else
            {
                for (var i = a; Backwards ? (i >= b) : (i <= b); i += Backwards ? -1 : 1)
                {
                    yield return new Position(Index, 1);
                    environment.CurrentStack.Add(i);
                    foreach (var instr in PrimaryBlock)
                        foreach (var pos in instr.Execute(environment))
                            yield return pos;
                    // “End” instruction
                    yield return new Position(Index + Count - 1, 1);
                }
            }
        }
    }

    sealed class ForEachLoop : BlockNode
    {
        public override IEnumerable<Position> Execute(ScliptingEnv environment)
        {
            yield return new Position(Index, 1);
            var orig = environment.CurrentStack.Last();
            var list =
                orig is List<object> ? (List<object>) orig :
                orig is byte[] ? ((byte[]) orig).Select(b => (object) (BigInteger) b) :
                ScliptingUtil.ToString(orig).Select(ch => (object) ch.ToString());
            bool any = false;
            foreach (var item in list)
            {
                if (!any && PrimaryBlockPops)
                    environment.Pop();
                any = true;
                yield return new Position(Index, 1);
                environment.CurrentStack.Add(item);
                foreach (var instr in PrimaryBlock)
                    foreach (var pos in instr.Execute(environment))
                        yield return pos;
                // “End” instruction
                yield return new Position(Index + Count - 1, 1);
            }
            if (!any)
            {
                // Jump to the “else” instruction
                if (ElseBlock != null)
                    yield return new Position(ElseIndex, 1);
                if ((ElseBlock == null && PrimaryBlockPops) || (ElseBlock != null && ElseBlockPops))
                    environment.Pop();
                if (ElseBlock != null)
                    foreach (var instr in ElseBlock)
                        foreach (var pos in instr.Execute(environment))
                            yield return pos;
                // “End” instruction
                yield return new Position(Index + Count - 1, 1);
            }
        }
    }

    enum Condition
    {
        True,
        False,
        NonEmpty
    }

    abstract class ConditionalBlockNode : BlockNode
    {
        public Condition Condition;

        protected bool SatisfiesCondition(object item)
        {
            switch (Condition)
            {
                case Condition.True: return ScliptingUtil.IsTrue(item);
                case Condition.False: return !ScliptingUtil.IsTrue(item);
                case Condition.NonEmpty: return ScliptingUtil.IsNonEmpty(item);
                default: return false;
            }
        }
    }

    sealed class IfBlock : ConditionalBlockNode
    {
        public override IEnumerable<Position> Execute(ScliptingEnv environment)
        {
            yield return new Position(Index, 1);
            var item = environment.CurrentStack.Last();
            if (SatisfiesCondition(item))
            {
                if (PrimaryBlockPops)
                    environment.Pop();
                foreach (var instruction in PrimaryBlock)
                    foreach (var position in instruction.Execute(environment))
                        yield return position;
            }
            else
            {
                // Jump to the “else” instruction
                if (ElseBlock != null)
                    yield return new Position(ElseIndex, 1);
                if ((ElseBlock == null && PrimaryBlockPops) || (ElseBlock != null && ElseBlockPops))
                    environment.Pop();
                if (ElseBlock != null)
                    foreach (var instruction in ElseBlock)
                        foreach (var position in instruction.Execute(environment))
                            yield return position;
            }

            // “End” instruction
            yield return new Position(Index + Count - 1, 1);
        }
    }

    sealed class WhileLoop : ConditionalBlockNode
    {
        public override IEnumerable<Position> Execute(ScliptingEnv environment)
        {
            yield return new Position(Index, 1);

            if (ConditionBlock != null)
            {
                foreach (var instruction in ConditionBlock)
                    foreach (var position in instruction.Execute(environment))
                        yield return position;
                yield return new Position(ConditionEndIndex, 1);
            }

            var item = environment.CurrentStack.Last();
            if (!SatisfiesCondition(item))
            {
                if (ElseBlock != null)
                {
                    // Jump to the “else” instruction
                    yield return new Position(ElseIndex, 1);
                    if (ElseBlockPops)
                        environment.Pop();
                    foreach (var instruction in ElseBlock)
                        foreach (var position in instruction.Execute(environment))
                            yield return position;
                }
                else if (PrimaryBlockPops)
                    environment.Pop();
            }
            else
            {
                do
                {
                    if (PrimaryBlockPops)
                        environment.Pop();
                    foreach (var instruction in PrimaryBlock)
                        foreach (var position in instruction.Execute(environment))
                            yield return position;
                    yield return new Position(Index, 1);
                    if (ConditionBlock != null)
                    {
                        foreach (var instruction in ConditionBlock)
                            foreach (var position in instruction.Execute(environment))
                                yield return position;
                        yield return new Position(ConditionEndIndex, 1);
                    }
                    item = environment.CurrentStack.Last();
                }
                while (SatisfiesCondition(item));
                if (PrimaryBlockPops)
                    environment.Pop();
            }

            // “End” instruction
            yield return new Position(Index + Count - 1, 1);
        }
    }

    sealed class SplitLoop : BlockNode
    {
        public bool Backward;

        public override IEnumerable<Position> Execute(ScliptingEnv environment)
        {
            yield return new Position(Index, 1);
            var item = environment.CurrentStack.Last();
            var isList = item is List<object>;
            var list = isList ? ((List<object>) item).ToList() : (ScliptingUtil.ToString(item).Select(ch => (object) ch.ToString()).ToList());
            var newList = new List<object>();

            if (list.Count == 0 && ElseBlock != null)
            {
                // Jump to the “else” instruction
                yield return new Position(ElseIndex, 1);
                if (ElseBlockPops)
                    environment.Pop();
                foreach (var instruction in ElseBlock)
                    foreach (var position in instruction.Execute(environment))
                        yield return position;
                // “End” instruction
                yield return new Position(Index + Count - 1, 1);
            }
            else
            {
                if (PrimaryBlockPops)
                    environment.Pop();
                while (list.Count > 0)
                {
                    yield return new Position(Index, 1);
                    var index = Backward ? list.Count - 1 : 0;
                    environment.CurrentStack.Add(list[index]);
                    foreach (var instruction in PrimaryBlock)
                        foreach (var position in instruction.Execute(environment))
                            yield return position;
                    yield return new Position(Index + Count - 1, 1);
                    var subItem = environment.Pop();
                    if (ScliptingUtil.IsTrue(subItem))
                        break;
                    newList.Insert(Backward ? 0 : newList.Count, list[index]);
                    list.RemoveAt(index);
                }

                ScliptingUtil.FlipIf(Backward, 
                    () => environment.CurrentStack.Add(isList ? (object) newList : ScliptingUtil.ToString(newList)),
                    () => environment.CurrentStack.Add(isList ? (object) list : ScliptingUtil.ToString(list)));
            }
        }
    }

    sealed class RegexSubstitute : BlockNode
    {
        private bool _firstMatchOnly;
        private enum matchType { Regex, CaseSensitiveSubstring, CaseInsensitiveSubstring };
        private matchType _matchType;

        public RegexSubstitute(Instruction instr)
        {
            PrimaryBlockPops =
                instr == Instruction.ReplaceRegexFirstPop ||
                instr == Instruction.ReplaceRegexAllPop ||
                instr == Instruction.ReplaceCsSubstrFirstPop ||
                instr == Instruction.ReplaceCsSubstrAllPop ||
                instr == Instruction.ReplaceCiSubstrFirstPop ||
                instr == Instruction.ReplaceCiSubstrAllPop;

            _firstMatchOnly =
                instr == Instruction.ReplaceRegexFirstPop ||
                instr == Instruction.ReplaceRegexFirstNoPop ||
                instr == Instruction.ReplaceCsSubstrFirstPop ||
                instr == Instruction.ReplaceCsSubstrFirstNoPop ||
                instr == Instruction.ReplaceCiSubstrFirstPop ||
                instr == Instruction.ReplaceCiSubstrFirstNoPop;

            _matchType =
                instr == Instruction.ReplaceRegexFirstPop ||
                instr == Instruction.ReplaceRegexFirstNoPop ||
                instr == Instruction.ReplaceRegexAllPop ||
                instr == Instruction.ReplaceRegexAllNoPop ? matchType.Regex :
                instr == Instruction.ReplaceCsSubstrFirstPop ||
                instr == Instruction.ReplaceCsSubstrFirstNoPop ||
                instr == Instruction.ReplaceCsSubstrAllPop ||
                instr == Instruction.ReplaceCsSubstrAllNoPop ? matchType.CaseSensitiveSubstring : matchType.CaseInsensitiveSubstring;
        }

        public override IEnumerable<Position> Execute(ScliptingEnv environment)
        {
            yield return new Position(Index, 1);

            var regex = ScliptingUtil.ToString(environment.Pop());
            switch (_matchType)
            {
                case matchType.Regex: break;
                case matchType.CaseSensitiveSubstring: regex = Regex.Escape(regex); break;
                case matchType.CaseInsensitiveSubstring: regex = "(?i:{0})".Fmt(Regex.Escape(regex)); break;
            }

            var input = ScliptingUtil.ToString(environment.CurrentStack.Last());
            List<Match> matches = null;
            Match match = null;

            if (_firstMatchOnly)
                match = Regex.Match(input, regex, RegexOptions.Singleline);
            else
                matches = Regex.Matches(input, regex, RegexOptions.Singleline).Cast<Match>().ToList();
            var pushResult = true;

            if (((_firstMatchOnly && !match.Success) || (!_firstMatchOnly && matches.Count == 0)) && ElseBlock != null)
            {
                // Jump to the “else” instruction
                yield return new Position(ElseIndex, 1);
                if (ElseBlockPops)
                    environment.Pop();
                else
                    pushResult = false;
                foreach (var instruction in ElseBlock)
                    foreach (var position in instruction.Execute(environment))
                        yield return position;
            }
            else
            {
                if (PrimaryBlockPops)
                    environment.Pop();
                var offset = 0;
                for (int i = 0; i < (_firstMatchOnly ? match.Success ? 1 : 0 : matches.Count); i++)
                {
                    var m = _firstMatchOnly ? match : matches[i];
                    environment.RegexObjects.Add(new RegexMatch(input, regex, offset, m));
                    if (i > 0)
                        yield return new Position(Index, 1);
                    foreach (var instruction in PrimaryBlock)
                        foreach (var position in instruction.Execute(environment))
                            yield return position;
                    yield return new Position(Index + Count - 1, 1);
                    var subst = ScliptingUtil.ToString(environment.Pop());
                    input = input.Substring(0, m.Index + offset) + subst + input.Substring(m.Index + offset + m.Length);
                    offset += subst.Length - m.Length;
                    environment.RegexObjects.RemoveAt(environment.RegexObjects.Count - 1);
                }
            }

            // “End” instruction
            environment.RegexObjects.Add(new RegexMatch(input, regex, 0, null));
            yield return new Position(Index + Count - 1, 1);
            environment.RegexObjects.RemoveAt(environment.RegexObjects.Count - 1);
            if (pushResult)
                environment.CurrentStack.Add(input);
        }
    }
}
