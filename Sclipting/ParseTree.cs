using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using RT.Util;
using RT.Util.Dialogs;
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

                    case instruction.Length: return stringListOperation(true, str => (BigInteger) str.Length, list => (BigInteger) list.Count);
                    case instruction.Repeat: return repeat;
                    case instruction.CombineString: return combineOperation(true);
                    case instruction.CombineList: return combineOperation(false);
                    case instruction.Insert: return insert;
                    case instruction.Combine: return combine;
                    case instruction.Reverse: return stringListOperation(true, reverseString,
                        list => { var newList = new List<object>(list); newList.Reverse(); return newList; });
                    case instruction.Sort: return stringListOperation(true, sortString(StringComparer.InvariantCultureIgnoreCase),
                        list => { var newList = new List<object>(list); newList.Sort((a, b) => Sclipting.ToString(a).CompareTo(Sclipting.ToString(b))); return newList; });
                    case instruction.Arrange: return stringListOperation(true, sortString(StringComparer.Ordinal),
                        list => { var newList = new List<object>(list); newList.Sort((a, b) => Sclipting.ToInt(a).CompareTo(Sclipting.ToInt(b))); return newList; });

                    case instruction.Excavate:
                    case instruction.DigOut:
                        return stringListOperation(instr == instruction.Excavate,
                            (s, i) => i < 0 || i >= s.Length ? (object) "" : s[(int) i].ToString(),
                            (l, i) => i < 0 || i >= l.Count ? (object) "" : l[(int) i]);
                    case instruction.Dig:
                    case instruction.Collect:
                        return stringListOperation(instr == instruction.Dig,
                            (s, i) => i < 0 || i >= s.Length ? (object) "" : s[s.Length - 1 - (int) i].ToString(),
                            (l, i) => i < 0 || i >= l.Count ? (object) "" : l[l.Count - 1 - (int) i]);



                    // STRING MANIPULATION (no lists)

                    case instruction.Explain:
                        return e =>
                        {
                            var str = Sclipting.ToString(e.Pop());
                            e.CurrentStack.Add(str.Length == 0 ? (object) double.NaN : (BigInteger) str[0]);
                        };
                    case instruction.Character:
                        return e =>
                        {
                            var codepoint = Sclipting.ToInt(e.Pop());
                            e.CurrentStack.Add(codepoint < 0 || codepoint > 0x10ffff ? "" : char.ConvertFromUtf32((int) codepoint));
                        };


                    // REGULAR EXPRESSIONS

                    case instruction.Appear: return e => { e.CurrentStack.Add(e.RegexObjects.Count == 0 ? "" : e.RegexObjects.Last().Match.Value); };
                    case instruction.SplitPop: return regexSplit(true);
                    case instruction.SplitNoPop: return regexSplit(false);


                    // ARITHMETIC

                    case instruction.Add: return e => { e.NumericOperation((i1, i2) => i1 + i2, (i1, i2) => i1 + i2); };
                    case instruction.Subtract: return e => { e.NumericOperation((i1, i2) => i1 - i2, (i1, i2) => i1 - i2); };
                    case instruction.Multiply: return e => { e.NumericOperation((i1, i2) => i1 * i2, (i1, i2) => i1 * i2); };
                    case instruction.DivideFloat: return e => { e.NumericOperation((i1, i2) => i2 == 0 ? double.NaN : (double) i1 / (double) i2, (i1, i2) => i2 == 0 ? double.NaN : i1 / i2); };
                    case instruction.DivideInt: return e => { var item2 = Sclipting.ToInt(e.Pop()); var item1 = Sclipting.ToInt(e.Pop()); e.CurrentStack.Add(item2 == 0 ? (object) double.NaN : item1 / item2); };
                    case instruction.Leftovers: return e => { var item2 = Sclipting.ToInt(e.Pop()); var item1 = Sclipting.ToInt(e.Pop()); e.CurrentStack.Add(item2 == 0 ? (object) double.NaN : item1 % item2); };
                    case instruction.Negative: return e => { var item = e.Pop(); e.CurrentStack.Add(item is double ? (object) -(double) item : -Sclipting.ToInt(item)); };
                    case instruction.Correct: return e => { var item = e.Pop(); e.CurrentStack.Add(item is double ? (object) Math.Abs((double) item) : BigInteger.Abs(Sclipting.ToInt(item))); };
                    case instruction.Increase: return e => { e.CurrentStack.Add(Sclipting.ToInt(e.Pop()) + 1); };
                    case instruction.Decrease: return e => { e.CurrentStack.Add(Sclipting.ToInt(e.Pop()) - 1); };
                    case instruction.Left: return e => { var item2 = Sclipting.ToInt(e.Pop()); var item1 = Sclipting.ToInt(e.Pop()); if (item2 < 0) e.CurrentStack.Add(double.NaN); else { for (; item2 > 0; item2--) item1 *= 2; e.CurrentStack.Add(item1); } };
                    case instruction.Right: return e => { var item2 = Sclipting.ToInt(e.Pop()); var item1 = Sclipting.ToInt(e.Pop()); var negative = item1 < 0; if (item2 < 0) e.CurrentStack.Add(double.NaN); else { for (; item2 > 0; item2--) { item1 /= 2; if (negative) item1--; } e.CurrentStack.Add(item1); } };
                    case instruction.Both: return e => { e.CurrentStack.Add(Sclipting.ToInt(e.Pop()) & Sclipting.ToInt(e.Pop())); };
                    case instruction.Other: return e => { e.CurrentStack.Add(Sclipting.ToInt(e.Pop()) | Sclipting.ToInt(e.Pop())); };
                    case instruction.Clever: return e => { e.CurrentStack.Add(Sclipting.ToInt(e.Pop()) ^ Sclipting.ToInt(e.Pop())); };


                    // LOGIC

                    case instruction.Small: return e => { e.NumericOperation((i1, i2) => i1 < i2 ? BigInteger.One : BigInteger.Zero, (i1, i2) => i1 < i2 ? BigInteger.One : BigInteger.Zero); };
                    case instruction.Great: return e => { e.NumericOperation((i1, i2) => i1 > i2 ? BigInteger.One : BigInteger.Zero, (i1, i2) => i1 > i2 ? BigInteger.One : BigInteger.Zero); };
                    case instruction.Less: return e => { e.NumericOperation((i1, i2) => i1 <= i2 ? BigInteger.One : BigInteger.Zero, (i1, i2) => i1 <= i2 ? BigInteger.One : BigInteger.Zero); };
                    case instruction.Overflow: return e => { e.NumericOperation((i1, i2) => i1 >= i2 ? BigInteger.One : BigInteger.Zero, (i1, i2) => i1 >= i2 ? BigInteger.One : BigInteger.Zero); };
                    case instruction.And: return e => { var item2 = Sclipting.ToInt(e.Pop()); var item1 = Sclipting.ToInt(e.Pop()); e.CurrentStack.Add(item1 != 0 && item2 != 0 ? BigInteger.One : BigInteger.Zero); };
                    case instruction.Or: return e => { var item2 = Sclipting.ToInt(e.Pop()); var item1 = Sclipting.ToInt(e.Pop()); e.CurrentStack.Add(item1 != 0 || item2 != 0 ? BigInteger.One : BigInteger.Zero); };
                    case instruction.OneOfPair: return e => { var item2 = Sclipting.ToInt(e.Pop()); e.CurrentStack.Add(Sclipting.ToInt(e.Pop()) != 0 ^ item2 != 0 ? BigInteger.One : BigInteger.Zero); };
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

            private static void repeat(scliptingExecutionEnvironment e)
            {
                List<object> list;
                byte[] b;

                var numTimes = (int) Sclipting.ToInt(e.Pop());
                var item = e.Pop();
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

            private static void combine(scliptingExecutionEnvironment e)
            {
                var item2 = e.Pop();
                var item1 = e.Pop();
                if (item1 is List<object> && item2 is List<object>)
                    e.CurrentStack.Add(new List<object>(((List<object>) item1).Concat((List<object>) item2)));
                else
                    e.CurrentStack.Add(Sclipting.ToString(item1) + Sclipting.ToString(item2));
            }

            private static void insert(scliptingExecutionEnvironment e)
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
                    while (integer >= newList.Count)
                        newList.Add("");
                    newList[integer] = item;
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
                    else
                        input = input.Substring(0, integer) + itemAsChar + input.Substring(integer + 1);
                    e.CurrentStack.Add(input);
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
                var taken = new HashSet<char>();
                foreach (var field in typeof(instruction).GetFields(BindingFlags.Public | BindingFlags.Static))
                {
                    var attr = field.GetCustomAttributes<instructionAttribute>().First();
                    var instr = (instruction) field.GetValue(null);
                    if (attr.Type == nodeType.SingularNode)
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

        private sealed class ifBlock : blockNode
        {
            public override IEnumerable<Position> Execute(scliptingExecutionEnvironment environment)
            {
                yield return new Position(Index, 1);
                var item = environment.CurrentStack.Last();
                if (Sclipting.IsTrue(item))
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

        private sealed class whileLoop : blockNode
        {
            public bool WhileTrue;

            public override IEnumerable<Position> Execute(scliptingExecutionEnvironment environment)
            {
                yield return new Position(Index, 1);
                var item = environment.CurrentStack.Last();
                if (Sclipting.IsTrue(item) != WhileTrue)
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
                    while (Sclipting.IsTrue(item) == WhileTrue);
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
            public bool FirstMatchOnly;

            public override IEnumerable<Position> Execute(scliptingExecutionEnvironment environment)
            {
                yield return new Position(Index, 1);
                var regex = Sclipting.ToString(environment.Pop());
                var input = Sclipting.ToString(environment.CurrentStack.Last());
                List<Match> matches = null;
                Match match = null;

                if (FirstMatchOnly)
                    match = Regex.Match(input, regex, RegexOptions.Singleline);
                else
                    matches = Regex.Matches(input, regex, RegexOptions.Singleline).Cast<Match>().ToList();
                var pushResult = true;

                if (((FirstMatchOnly && !match.Success) || (!FirstMatchOnly && matches.Count == 0)) && ElseBlock != null)
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
                    for (int i = 0; i < (FirstMatchOnly ? match.Success ? 1 : 0 : matches.Count); i++)
                    {
                        var m = FirstMatchOnly ? match : matches[i];
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
