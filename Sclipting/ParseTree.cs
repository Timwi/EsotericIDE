using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace EsotericIDE.Languages
{
    partial class Sclipting
    {
        private abstract class node
        {
            public int Index, Count;
            public abstract IEnumerable<Position> Execute(scliptingExecutionEnvironment environment);
        }

        private sealed class program : node
        {
            public List<node> Instructions;
            public override IEnumerable<Position> Execute(scliptingExecutionEnvironment environment)
            {
                environment.CurrentStack.Add(environment.Input);
                foreach (var instruction in Instructions)
                    foreach (var position in instruction.Execute(environment))
                        yield return position;
                yield return new Position(Index + Count, 0);
                environment.GenerateOutput();
            }
        }

        private sealed class byteArray : node
        {
            public byte[] Array;
            public override IEnumerable<Position> Execute(scliptingExecutionEnvironment environment)
            {
                yield return new Position(Index, Count);
                environment.CurrentStack.Add(Array);
            }
        }

        private sealed class negativeNumber : node
        {
            public BigInteger Number;
            public override IEnumerable<Position> Execute(scliptingExecutionEnvironment environment)
            {
                yield return new Position(Index, Count);
                environment.CurrentStack.Add(Number);
            }
        }

        private sealed class singularNode : node
        {
            public instruction ThisInstruction;

            // This is static and returns a delegate so that the post-build check can test it without needing to execute all the instructions
            private static Action<scliptingExecutionEnvironment> getMethod(instruction instr)
            {
                switch (instr)
                {

                    // GENERAL

                    case instruction.Mark: return e => { e.CurrentStack.Add(new mark()); };
                    case instruction.Discard: return e => { e.Pop(); };


                    // STRING/LIST MANIPULATION

                    case instruction.Lack: return e => { e.CurrentStack.Add(new List<object>()); };
                    case instruction.Empty: return e => { e.CurrentStack.Add(""); };
                    case instruction.Length: return stringListOperation(true, str => (BigInteger) str.Length, list => (BigInteger) list.Count);
                    case instruction.Long: return stringListOperation(false, str => (BigInteger) str.Length, list => (BigInteger) list.Count);
                    case instruction.Repeat: return repeat(false);
                    case instruction.Extend: return repeat(true);
                    case instruction.RepeatIntoList: return repeatIntoList(false);
                    case instruction.Stretch: return repeatIntoList(true);
                    case instruction.CombineString: return combineOperation(true);
                    case instruction.CombineList: return combineOperation(false);
                    case instruction.Combine: return combine(false);
                    case instruction.Blend: return combine(true);
                    case instruction.Reverse: return stringListOperation(true, reverseString,
                        list => { var newList = new List<object>(list); newList.Reverse(); return newList; });
                    case instruction.Sort: return stringListOperation(true, sortString(StringComparer.InvariantCultureIgnoreCase),
                        list => { var newList = new List<object>(list); newList.Sort((a, b) => Sclipting.ToString(a).CompareTo(Sclipting.ToString(b))); return newList; });
                    case instruction.Arrange: return stringListOperation(true, sortString(StringComparer.Ordinal),
                        list => { var newList = new List<object>(list); newList.Sort((a, b) => Sclipting.ToInt(a).CompareTo(Sclipting.ToInt(b))); return newList; });
                    case instruction.Assemble: return assemble;


                    // STRING MANIPULATION

                    case instruction.Explain: return e =>
                    {
                        var str = Sclipting.ToString(e.Pop());
                        e.CurrentStack.Add(str.Length == 0 ? (object) double.NaN : (BigInteger) str[0]);
                    };
                    case instruction.Character: return e =>
                    {
                        var codepoint = Sclipting.ToInt(e.Pop());
                        e.CurrentStack.Add(codepoint < 0 || codepoint > 0x10ffff ? "" : char.ConvertFromUtf32((int) codepoint));
                    };
                    case instruction.ChangeRegex:
                    case instruction.ChangeCs:
                    case instruction.ChangeCi: return e =>
                    {
                        var replacement = Sclipting.ToString(e.Pop());
                        var needle = Sclipting.ToString(e.Pop());
                        var haystack = Sclipting.ToString(e.Pop());
                        e.CurrentStack.Add(
                            instr == instruction.ChangeRegex
                                ? Regex.Replace(haystack, needle, replacement)
                                : instr == instruction.ChangeCs
                                    ? haystack.Replace(needle, replacement)
                                    : haystack.Replace(needle, replacement, StringComparison.InvariantCultureIgnoreCase)
                        );
                    };
                    case instruction.Appear: return e => { e.CurrentStack.Add(e.RegexObjects.Count == 0 ? "" : e.RegexObjects.Last().Match.Value); };
                    case instruction.SplitPop: return regexSplit(true);
                    case instruction.SplitNoPop: return regexSplit(false);
                    case instruction.Big: return recursiveStringOperation(s => s.ToUpperInvariant());
                    case instruction.Tiny: return recursiveStringOperation(s => s.ToLowerInvariant());
                    case instruction.Title: return recursiveStringOperation(CultureInfo.InvariantCulture.TextInfo.ToTitleCase);


                    // ARITHMETIC

                    case instruction.Add: return e => { e.NumericOperation((i1, i2) => i1 + i2, (i1, i2) => i1 + i2); };
                    case instruction.Subtract: return e => { e.NumericOperation((i1, i2) => i1 - i2, (i1, i2) => i1 - i2); };
                    case instruction.Reduce: return e => { e.NumericOperation((i1, i2) => i2 - i1, (i1, i2) => i2 - i1); };
                    case instruction.Multiply: return e => { e.NumericOperation((i1, i2) => i1 * i2, (i1, i2) => i1 * i2); };
                    case instruction.DivideFloat: return e => { e.NumericOperation((i1, i2) => i2 == 0 ? double.NaN : (double) i1 / (double) i2, (i1, i2) => i2 == 0 ? double.NaN : i1 / i2); };
                    case instruction.DivideInt: return e => { var item2 = Sclipting.ToInt(e.Pop()); var item1 = Sclipting.ToInt(e.Pop()); e.CurrentStack.Add(item2 == 0 ? (object) double.NaN : item1 / item2); };
                    case instruction.Leftovers: return e => { var item2 = Sclipting.ToInt(e.Pop()); var item1 = Sclipting.ToInt(e.Pop()); e.CurrentStack.Add(item2 == 0 ? (object) double.NaN : item1 % item2); };
                    case instruction.Double: return e => { e.NumericOperation(i => 2 * i, i => 2 * i); };
                    case instruction.Half: return e => { e.NumericOperation(i => (double) i / 2, i => i / 2); };
                    case instruction.Separate: return e => { e.CurrentStack.Add(Sclipting.ToInt(e.Pop()) >> 1); };
                    case instruction.Negative: return e => { e.NumericOperation(i => -i, i => -i); };
                    case instruction.Correct: return e => { e.NumericOperation(i => BigInteger.Abs(i), i => Math.Abs(i)); };
                    case instruction.Increase: return e => { e.CurrentStack.Add(Sclipting.ToInt(e.Pop()) + 1); };
                    case instruction.Decrease: return e => { e.CurrentStack.Add(Sclipting.ToInt(e.Pop()) - 1); };
                    case instruction.Left: return e => { var b = Sclipting.ToInt(e.Pop()); var a = Sclipting.ToInt(e.Pop()); e.CurrentStack.Add(b < 0 ? a >> (int) -b : a << (int) b); };
                    case instruction.Right: return e => { var b = Sclipting.ToInt(e.Pop()); var a = Sclipting.ToInt(e.Pop()); e.CurrentStack.Add(b < 0 ? a << (int) -b : a >> (int) b); };
                    case instruction.Both: return e => { e.CurrentStack.Add(Sclipting.ToInt(e.Pop()) & Sclipting.ToInt(e.Pop())); };
                    case instruction.Other: return e => { e.CurrentStack.Add(Sclipting.ToInt(e.Pop()) | Sclipting.ToInt(e.Pop())); };
                    case instruction.Clever: return e => { e.CurrentStack.Add(Sclipting.ToInt(e.Pop()) ^ Sclipting.ToInt(e.Pop())); };
                    case instruction.BitwiseNot: return e => { e.CurrentStack.Add(~Sclipting.ToInt(e.Pop())); };
                    case instruction.Gnaw: return gnaw(false);
                    case instruction.Bite: return gnaw(true);


                    // LOGIC

                    case instruction.Small: return e => { e.NumericOperation((i1, i2) => i1 < i2 ? BigInteger.One : BigInteger.Zero, (i1, i2) => i1 < i2 ? BigInteger.One : BigInteger.Zero); };
                    case instruction.Great: return e => { e.NumericOperation((i1, i2) => i1 > i2 ? BigInteger.One : BigInteger.Zero, (i1, i2) => i1 > i2 ? BigInteger.One : BigInteger.Zero); };
                    case instruction.Less: return e => { e.NumericOperation((i1, i2) => i1 <= i2 ? BigInteger.One : BigInteger.Zero, (i1, i2) => i1 <= i2 ? BigInteger.One : BigInteger.Zero); };
                    case instruction.Overflow: return e => { e.NumericOperation((i1, i2) => i1 >= i2 ? BigInteger.One : BigInteger.Zero, (i1, i2) => i1 >= i2 ? BigInteger.One : BigInteger.Zero); };
                    case instruction.And: return e => { var item2 = Sclipting.ToInt(e.Pop()); var item1 = Sclipting.ToInt(e.Pop()); e.CurrentStack.Add(item1 != 0 && item2 != 0 ? BigInteger.One : BigInteger.Zero); };
                    case instruction.Or: return e => { var item2 = Sclipting.ToInt(e.Pop()); var item1 = Sclipting.ToInt(e.Pop()); e.CurrentStack.Add(item1 != 0 || item2 != 0 ? BigInteger.One : BigInteger.Zero); };
                    case instruction.OneOfPair: return e => { var item2 = Sclipting.ToInt(e.Pop()); e.CurrentStack.Add(Sclipting.ToInt(e.Pop()) != 0 ^ item2 != 0 ? BigInteger.One : BigInteger.Zero); };
                    case instruction.Not: return e => { var item = Sclipting.ToInt(e.Pop()); e.CurrentStack.Add(item == 0 ? BigInteger.One : BigInteger.Zero); };
                    case instruction.Same: return e => { var item2 = e.Pop(); e.CurrentStack.Add(e.Pop().Equals(item2) ? BigInteger.One : BigInteger.Zero); };
                    case instruction.Equal: return e => { var item2 = Sclipting.ToInt(e.Pop()); e.CurrentStack.Add(Sclipting.ToInt(e.Pop()) == item2 ? BigInteger.One : BigInteger.Zero); };
                    case instruction.Resemble: return e => { var item2 = Sclipting.ToString(e.Pop()); e.CurrentStack.Add(Sclipting.ToString(e.Pop()) == item2 ? BigInteger.One : BigInteger.Zero); };
                    case instruction.Different1: return e => { var item2 = e.Pop(); e.CurrentStack.Add(e.Pop().Equals(item2) ? BigInteger.Zero : BigInteger.One); };
                    case instruction.Different2: return e => { var item2 = Sclipting.ToInt(e.Pop()); e.CurrentStack.Add(Sclipting.ToInt(e.Pop()) == item2 ? BigInteger.Zero : BigInteger.One); };
                    case instruction.Different3: return e => { var item2 = Sclipting.ToString(e.Pop()); e.CurrentStack.Add(Sclipting.ToString(e.Pop()) == item2 ? BigInteger.Zero : BigInteger.One); };
                    case instruction.IsIt: return e => { var no = e.Pop(); var yes = e.Pop(); e.CurrentStack.Add(Sclipting.IsTrue(e.Pop()) ? yes : no); };
                    case instruction.Power: return e =>
                    {
                        e.NumericOperation((i1, i2) =>
                        {
                            if (i2 < 0)
                                return Math.Pow((double) i1, (double) i2);
                            if (i2 == 0)
                                return BigInteger.One;
                            var bi = BigInteger.One;
                            while (i2 > int.MaxValue)
                            {
                                bi *= BigInteger.Pow(i1, int.MaxValue);
                                i2 -= int.MaxValue;
                            }
                            return bi * BigInteger.Pow(i1, (int) i2);
                        }, (i1, i2) => Math.Pow(i1, i2));
                    };


                    default:
                        throw new InternalErrorException("Unknown instruction: “{0}”".Fmt(instr));
                }
            }

            private static Action<scliptingExecutionEnvironment> recursiveStringOperation(Func<string, string> func)
            {
                Func<object, object> recurse = null;
                recurse = (object item) =>
                {
                    var list = item as List<object>;
                    if (list != null)
                        return list.Select(recurse).ToList();
                    return func(Sclipting.ToString(item));
                };
                return e =>
                {
                    var item = e.Pop();
                    e.CurrentStack.Add(recurse(item));
                };
            }

            private static Action<scliptingExecutionEnvironment> gnaw(bool reversed)
            {
                return e =>
                {
                    var b = (int) Sclipting.ToInt(e.Pop());
                    var a = Sclipting.ToInt(e.Pop());
                    Sclipting.FlipIf(reversed,
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

            private static Action<scliptingExecutionEnvironment> regexSplit(bool pop)
            {
                return e =>
                {
                    var regex = Sclipting.ToString(e.Pop());
                    var input = Sclipting.ToString(e.CurrentStack.Last());
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

            private static Action<scliptingExecutionEnvironment> repeat(bool reversedArguments)
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
                        numTimes = (int) Sclipting.ToInt(item2);
                        item = item1;
                    }
                    else
                    {
                        numTimes = (int) Sclipting.ToInt(item1);
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
                        e.CurrentStack.Add(Sclipting.ToString(item).Repeat(numTimes));
                };
            }

            private static Action<scliptingExecutionEnvironment> repeatIntoList(bool reversedArguments)
            {
                return e =>
                {
                    int numTimes;
                    object item;

                    var item1 = e.Pop();
                    var item2 = e.Pop();

                    if (reversedArguments)
                    {
                        numTimes = (int) Sclipting.ToInt(item2);
                        item = item1;
                    }
                    else
                    {
                        numTimes = (int) Sclipting.ToInt(item1);
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

            private static Action<scliptingExecutionEnvironment> stringListOperation(bool pop, Func<string, object> stringOperation, Func<List<object>, object> listOperation)
            {
                return e =>
                {
                    List<object> list;

                    var item = pop ? e.Pop() : e.CurrentStack.Last();
                    if ((list = item as List<object>) != null)
                        e.CurrentStack.Add(listOperation(list));
                    else
                        e.CurrentStack.Add(stringOperation(Sclipting.ToString(item)));
                };
            }

            private static Action<scliptingExecutionEnvironment> stringListOperation(bool pop, Func<string, BigInteger, object> stringOperation, Func<List<object>, BigInteger, object> listOperation)
            {
                return e =>
                {
                    List<object> list;

                    var integer = Sclipting.ToInt(e.Pop());
                    var item = pop ? e.Pop() : e.CurrentStack.Last();
                    if ((list = item as List<object>) != null)
                        e.CurrentStack.Add(listOperation(list, integer));
                    else
                        e.CurrentStack.Add(stringOperation(Sclipting.ToString(item), integer));
                };
            }

            private static Action<scliptingExecutionEnvironment> combine(bool reverse)
            {
                return e =>
                {
                    var item2 = e.Pop();
                    var item1 = e.Pop();
                    if (reverse) { var t = item1; item1 = item2; item2 = t; }
                    if (item1 is List<object> && item2 is List<object>)
                        e.CurrentStack.Add(new List<object>(((List<object>) item1).Concat((List<object>) item2)));
                    else
                        e.CurrentStack.Add(Sclipting.ToString(item1) + Sclipting.ToString(item2));
                };
            }

            private static void assemble(scliptingExecutionEnvironment e)
            {
                var separator = Sclipting.ToString(e.Pop());
                var item = e.Pop();
                var list = item is List<object> ? ((List<object>) item).Select(obj => Sclipting.ToString(obj)) : Sclipting.ToString(item).Select(ch => (object) ch.ToString());
                e.CurrentStack.Add(list.JoinString(separator));
            }

            private static Action<scliptingExecutionEnvironment> insert(bool replace)
            {
                return e =>
                {
                    var item = e.Pop();
                    var bigInteger = Sclipting.ToInt(e.Pop());
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
                        var itemAsStr = Sclipting.ToString(item);
                        var itemAsChar = itemAsStr == "" ? ' ' : itemAsStr[0];
                        var input = Sclipting.ToString(listOrString);

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

            private static void annihilate(scliptingExecutionEnvironment e)
            {
                var bigInteger = Sclipting.ToInt(e.Pop());
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
                    var input = Sclipting.ToString(listOrString);
                    // make sure we push the original object (not convert to string) if out of range
                    e.CurrentStack.Add(integer >= input.Length ? listOrString : input.Substring(0, integer) + input.Substring(integer + 1));
                }
            }

            private static Action<scliptingExecutionEnvironment> combineOperation(bool stringify)
            {
                return e =>
                {
                    var index = e.CurrentStack.Count - 1;
                    while (index >= 0 && !(e.CurrentStack[index] is mark))
                        index--;
                    var items = new List<object>(Enumerable.Range(index + 1, e.CurrentStack.Count - index - 1).Select(i => e.CurrentStack[i]));
                    if (index == -1)
                        index = 0;
                    e.CurrentStack.RemoveRange(index, e.CurrentStack.Count - index);
                    e.CurrentStack.Add(stringify
                        ? (object) items.Select(item => Sclipting.ToString(item)).JoinString()
                        : (object) items);
                };
            }

            public override IEnumerable<Position> Execute(scliptingExecutionEnvironment environment)
            {
                yield return new Position(Index, Count);
                getMethod(ThisInstruction)(environment);
            }

#if DEBUG
            private static void PostBuildCheck(IPostBuildReporter rep)
            {
                foreach (var field in typeof(instruction).GetFields(BindingFlags.Public | BindingFlags.Static))
                {
                    var attr = field.GetCustomAttributes<instructionAttribute>().First();
                    var instr = (instruction) field.GetValue(null);
                    if (attr.Type == nodeType.SingularNode && !field.IsDefined<singularListStringInstructionAttribute>())
                    {
                        try { getMethod(instr); }
                        catch { rep.Error(@"Instruction ""{0}"" has no method.".Fmt(instr), "singularNode", "getMethod", "default"); }
                    }
                }
            }
#endif
        }

        private sealed class stackOrRegexNode : node
        {
            public stackOrRegexNodeType Type;
            public int Value;

            public override IEnumerable<Position> Execute(scliptingExecutionEnvironment environment)
            {
                yield return new Position(Index, Count);
                object item;
                switch (Type)
                {
                    case stackOrRegexNodeType.RegexCapture:
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
                                case stackOrRegexNodeType.CopyFromTop:
                                    environment.CurrentStack.Add(environment.CurrentStack[environment.CurrentStack.Count - Value]);
                                    break;
                                case stackOrRegexNodeType.CopyFromBottom:
                                    environment.CurrentStack.Add(environment.CurrentStack[Value - 1]);
                                    break;
                                case stackOrRegexNodeType.MoveFromTop:
                                    item = environment.CurrentStack[environment.CurrentStack.Count - Value];
                                    environment.CurrentStack.RemoveAt(environment.CurrentStack.Count - Value);
                                    environment.CurrentStack.Add(item);
                                    break;
                                case stackOrRegexNodeType.MoveFromBottom:
                                    environment.CurrentStack.Add(environment.CurrentStack[Value - 1]);
                                    environment.CurrentStack.RemoveAt(Value - 1);
                                    break;
                                case stackOrRegexNodeType.SwapFromBottom:
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
            [listStringInstruction("Retrieve item/character from list/string (pop)")]
            RetrievePop,
            [listStringInstruction("Retrieve item/character from list/string (no pop)")]
            RetrieveNoPop,
            [listStringInstruction("Insert item/character in list/string")]
            Insert,
            [listStringInstruction("Delete item/character from list/string")]
            Delete,
            [listStringInstruction("Retrieve and delete item/character from list/string")]
            RetrieveDelete,
            [listStringInstruction("Replace item/character in list/string")]
            Replace,
            [listStringInstruction("Exchange item/character in list/string")]
            Exchange
        }

        private sealed class listStringElementNode : node
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

            public listStringElementNode(char ch, int sourceIndex)
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

            public listStringElementNode(ListStringInstruction instruction, bool backwards, int sourceIndex)
            {
                Instruction = instruction;
                Backwards = backwards;
                GetIndexFromStack = true;
                Index = sourceIndex;
                Count = 1;
            }

            public override IEnumerable<Position> Execute(scliptingExecutionEnvironment environment)
            {
                yield return new Position(Index, Count);

                switch (Instruction)
                {
                    case ListStringInstruction.RetrievePop:
                    case ListStringInstruction.RetrieveNoPop:
                        {
                            if (GetIndexFromStack)
                                ListStringIndex = (int) Sclipting.ToInt(environment.Pop());
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
                                ListStringIndex = (int) Sclipting.ToInt(environment.Pop());
                            var listOrString = environment.Pop();
                            operation(listOrString, true, false,
                                (s, i) =>
                                {
                                    var j = Backwards ? i + 1 : i;
                                    var itemAsString = Sclipting.ToString(item);
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
                                ListStringIndex = (int) Sclipting.ToInt(environment.Pop());
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
                                ListStringIndex = (int) Sclipting.ToInt(environment.Pop());
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
                                ListStringIndex = (int) Sclipting.ToInt(environment.Pop());
                            var listOrString = environment.Pop();
                            operation(listOrString, false, true,
                                (s, i) =>
                                {
                                    var itemAsString = Sclipting.ToString(item);
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
                                ListStringIndex = (int) Sclipting.ToInt(environment.Pop());
                            var listOrString = environment.Pop();
                            operation(listOrString, false, true,
                                (s, i) =>
                                {
                                    var itemAsString = Sclipting.ToString(item);
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
                    var str = Sclipting.ToString(listOrString);
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

            public static ToolStripItem[] GetMenuItems(Action<string> insertText)
            {
                return typeof(ListStringInstruction).GetFields(BindingFlags.Static | BindingFlags.Public)
                    .Select((field, fi) =>
                    {
                        var item = new ToolStripMenuItem(field.GetCustomAttributes<listStringInstructionAttribute>().First().MenuLabel);
                        item.DropDownItems.AddRange(Enumerable.Range(0, 20)
                            .Select(i => new { Ch = Characters[fi * 20 + i], Engrish = Engrish[fi * 20 + i] })
                            .Select(inf => new ToolStripMenuItem("{0} &{1} — {2}".Fmt(inf.Ch, inf.Engrish, new listStringElementNode(inf.Ch, 0).Explain()), null, (_, __) => { insertText(inf.Ch.ToString()); }))
                            .ToArray());
                        return item;
                    })
                    .ToArray();
            }
        }

        private abstract class blockNode : node
        {
            public List<node> PrimaryBlock;
            public List<node> ElseBlock;
            public bool PrimaryBlockPops, ElseBlockPops;
            public int ElseIndex;
        }

        private sealed class functionNode : blockNode
        {
            public bool Capture;
            public override IEnumerable<Position> Execute(scliptingExecutionEnvironment environment)
            {
                yield return new Position(Index, Count);
                environment.CurrentStack.Add(new function { FunctionCode = this, CapturedItem = Capture ? environment.Pop() : null });
            }
        }

        private sealed class executeFunction : node
        {
            public instruction Instruction;

            public override IEnumerable<Position> Execute(scliptingExecutionEnvironment environment)
            {
                yield return new Position(Index, 1);
                var item = Instruction == instruction.Perform ? environment.CurrentStack.Last() : environment.Pop();
                var fnc = item as function;
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
                if (Instruction == instruction.Handle)
                {
                    yield return new Position(Index, 1);
                    environment.CurrentStack.Add(item);
                }
            }
        }

        private sealed class forLoop : blockNode
        {
            public bool Backwards;
            public override IEnumerable<Position> Execute(scliptingExecutionEnvironment environment)
            {
                yield return new Position(Index, 1);
                var b = Sclipting.ToInt(environment.Pop());
                var a = Sclipting.ToInt(environment.Pop());
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

        private sealed class forEachLoop : blockNode
        {
            public override IEnumerable<Position> Execute(scliptingExecutionEnvironment environment)
            {
                yield return new Position(Index, 1);
                var orig = environment.CurrentStack.Last();
                var list =
                    orig is List<object> ? (List<object>) orig :
                    orig is byte[] ? ((byte[]) orig).Select(b => (object) (BigInteger) b) :
                    Sclipting.ToString(orig).Select(ch => (object) ch.ToString());
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

        private enum condition
        {
            True,
            False,
            NonEmpty
        }

        private abstract class conditionalBlockNode : blockNode
        {
            public condition Condition;

            protected bool SatisfiesCondition(object item)
            {
                switch (Condition)
                {
                    case condition.True: return Sclipting.IsTrue(item);
                    case condition.False: return !Sclipting.IsTrue(item);
                    case condition.NonEmpty: return Sclipting.IsNonEmpty(item);
                    default: return false;
                }
            }
        }

        private sealed class ifBlock : conditionalBlockNode
        {
            public override IEnumerable<Position> Execute(scliptingExecutionEnvironment environment)
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

        private sealed class whileLoop : conditionalBlockNode
        {
            public override IEnumerable<Position> Execute(scliptingExecutionEnvironment environment)
            {
                yield return new Position(Index, 1);
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

        private sealed class splitLoop : blockNode
        {
            public bool Backward;

            public override IEnumerable<Position> Execute(scliptingExecutionEnvironment environment)
            {
                yield return new Position(Index, 1);
                var item = environment.CurrentStack.Last();
                var isList = item is List<object>;
                var list = isList ? ((List<object>) item).ToList() : (Sclipting.ToString(item).Select(ch => (object) ch.ToString()).ToList());
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
                        if (Sclipting.IsTrue(subItem))
                            break;
                        newList.Insert(Backward ? 0 : newList.Count, list[index]);
                        list.RemoveAt(index);
                    }

                    Action action1 = () => environment.CurrentStack.Add(isList ? (object) newList : Sclipting.ToString(newList));
                    Action action2 = () => environment.CurrentStack.Add(isList ? (object) list : Sclipting.ToString(list));
                    Sclipting.FlipIf(Backward, action1, action2);
                }
            }
        }

        private sealed class regexSubstitute : blockNode
        {
            private bool _firstMatchOnly;
            private enum matchType { Regex, CaseSensitiveSubstring, CaseInsensitiveSubstring };
            private matchType _matchType;

            public regexSubstitute(instruction instr)
            {
                PrimaryBlockPops =
                    instr == instruction.ReplaceRegexFirstPop ||
                    instr == instruction.ReplaceRegexAllPop ||
                    instr == instruction.ReplaceCsSubstrFirstPop ||
                    instr == instruction.ReplaceCsSubstrAllPop ||
                    instr == instruction.ReplaceCiSubstrFirstPop ||
                    instr == instruction.ReplaceCiSubstrAllPop;

                _firstMatchOnly =
                    instr == instruction.ReplaceRegexFirstPop ||
                    instr == instruction.ReplaceRegexFirstNoPop ||
                    instr == instruction.ReplaceCsSubstrFirstPop ||
                    instr == instruction.ReplaceCsSubstrFirstNoPop ||
                    instr == instruction.ReplaceCiSubstrFirstPop ||
                    instr == instruction.ReplaceCiSubstrFirstNoPop;

                _matchType =
                    instr == instruction.ReplaceRegexFirstPop ||
                    instr == instruction.ReplaceRegexFirstNoPop ||
                    instr == instruction.ReplaceRegexAllPop ||
                    instr == instruction.ReplaceRegexAllNoPop ? matchType.Regex :
                    instr == instruction.ReplaceCsSubstrFirstPop ||
                    instr == instruction.ReplaceCsSubstrFirstNoPop ||
                    instr == instruction.ReplaceCsSubstrAllPop ||
                    instr == instruction.ReplaceCsSubstrAllNoPop ? matchType.CaseSensitiveSubstring : matchType.CaseInsensitiveSubstring;
            }

            public override IEnumerable<Position> Execute(scliptingExecutionEnvironment environment)
            {
                yield return new Position(Index, 1);

                var regex = Sclipting.ToString(environment.Pop());
                switch (_matchType)
                {
                    case matchType.Regex: break;
                    case matchType.CaseSensitiveSubstring: regex = Regex.Escape(regex); break;
                    case matchType.CaseInsensitiveSubstring: regex = "(?i:{0})".Fmt(Regex.Escape(regex)); break;
                }

                var input = Sclipting.ToString(environment.CurrentStack.Last());
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
                        environment.RegexObjects.Add(new regexMatch(input, regex, offset, m));
                        if (i > 0)
                            yield return new Position(Index, 1);
                        foreach (var instruction in PrimaryBlock)
                            foreach (var position in instruction.Execute(environment))
                                yield return position;
                        yield return new Position(Index + Count - 1, 1);
                        var subst = Sclipting.ToString(environment.Pop());
                        input = input.Substring(0, m.Index + offset) + subst + input.Substring(m.Index + offset + m.Length);
                        offset += subst.Length - m.Length;
                        environment.RegexObjects.RemoveAt(environment.RegexObjects.Count - 1);
                    }
                }

                // “End” instruction
                environment.RegexObjects.Add(new regexMatch(input, regex, 0, null));
                yield return new Position(Index + Count - 1, 1);
                environment.RegexObjects.RemoveAt(environment.RegexObjects.Count - 1);
                if (pushResult)
                    environment.CurrentStack.Add(input);
            }
        }
    }
}
