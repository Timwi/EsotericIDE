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

namespace EsotericIDE.Sclipting
{
    abstract class Instruction
    {
        public int Index, Count;
        public abstract IEnumerable<Position> Execute(ScliptingExecutionEnvironment environment);
    }

    sealed class ScliptingProgram : Instruction
    {
        public List<Instruction> Instructions;
        public override IEnumerable<Position> Execute(ScliptingExecutionEnvironment environment)
        {
            environment.CurrentStack.Add(environment.Input);
            foreach (var instruction in Instructions)
                foreach (var position in instruction.Execute(environment))
                    yield return position;
            yield return new Position(Index + Count, 0);
            environment.GenerateOutput();
        }
    }

    sealed class ByteArray : Instruction
    {
        public byte[] Array;
        public override IEnumerable<Position> Execute(ScliptingExecutionEnvironment environment)
        {
            yield return new Position(Index, Count);
            environment.CurrentStack.Add(Array);
        }
    }

    sealed class NegativeNumber : Instruction
    {
        public BigInteger Number;
        public override IEnumerable<Position> Execute(ScliptingExecutionEnvironment environment)
        {
            yield return new Position(Index, Count);
            environment.CurrentStack.Add(Number);
        }
    }

    sealed class SingularInstruction : Instruction
    {
        public ScliptingInstruction ThisInstruction;

        // This is static and returns a delegate so that the post-build check can test it without needing to execute all the instructions
        private static Action<ScliptingExecutionEnvironment> getMethod(ScliptingInstruction instruction)
        {
            switch (instruction)
            {

                // GENERAL

                case ScliptingInstruction.Mark: return e => { e.CurrentStack.Add(new Mark()); };
                case ScliptingInstruction.Discard: return e => { e.Pop(); };


                // STRING/LIST MANIPULATION

                case ScliptingInstruction.Length: return stringListOperation(true, str => (BigInteger) str.Length, list => (BigInteger) list.Count);
                case ScliptingInstruction.Repeat: return repeat;
                case ScliptingInstruction.CombineString: return combineOperation(true);
                case ScliptingInstruction.CombineList: return combineOperation(false);
                case ScliptingInstruction.Excavate: return stringListOperation(true, (s, i) => i < 0 || i >= s.Length ? (object) "" : s[(int) i].ToString(), (l, i) => i < 0 || i >= l.Count ? (object) "" : l[(int) i]);
                case ScliptingInstruction.DigOut: return stringListOperation(false, (s, i) => i < 0 || i >= s.Length ? (object) "" : s[(int) i].ToString(), (l, i) => i < 0 || i >= l.Count ? (object) "" : l[(int) i]);
                case ScliptingInstruction.Reverse: return stringListOperation(true, reverseString, list => { var newList = new List<object>(list); newList.Reverse(); return newList; });


                // STRING MANIPULATION (no lists)

                case ScliptingInstruction.Explain:
                    return e =>
                    {
                        var str = ScliptingLanguage.ToString(e.Pop());
                        e.CurrentStack.Add(str.Length == 0 ? (object) double.NaN : (BigInteger) str[0]);
                    };
                case ScliptingInstruction.Character:
                    return e =>
                    {
                        var codepoint = ScliptingLanguage.ToInt(e.Pop());
                        e.CurrentStack.Add(codepoint < 0 || codepoint > 0x10ffff ? "" : char.ConvertFromUtf32((int) codepoint));
                    };


                // REGULAR EXPRESSIONS

                case ScliptingInstruction.Appear: return e => { e.CurrentStack.Add(e.RegexObjects.Count == 0 ? "" : e.RegexObjects.Last().Match.Value); };
                case ScliptingInstruction.SplitPop: return regexSplit(true);
                case ScliptingInstruction.SplitNoPop: return regexSplit(false);


                // ARITHMETIC

                case ScliptingInstruction.Add: return e => { e.NumericOperation((i1, i2) => i1 + i2, (i1, i2) => i1 + i2); };
                case ScliptingInstruction.Subtract: return e => { e.NumericOperation((i1, i2) => i1 - i2, (i1, i2) => i1 - i2); };
                case ScliptingInstruction.Multiply: return e => { e.NumericOperation((i1, i2) => i1 * i2, (i1, i2) => i1 * i2); };
                case ScliptingInstruction.DivideFloat: return e => { e.NumericOperation((i1, i2) => i2 == 0 ? double.NaN : (double) i1 / (double) i2, (i1, i2) => i2 == 0 ? double.NaN : i1 / i2); };
                case ScliptingInstruction.DivideInt: return e => { var item2 = ScliptingLanguage.ToInt(e.Pop()); var item1 = ScliptingLanguage.ToInt(e.Pop()); e.CurrentStack.Add(item2 == 0 ? (object) double.NaN : item1 / item2); };
                case ScliptingInstruction.Leftovers: return e => { var item2 = ScliptingLanguage.ToInt(e.Pop()); var item1 = ScliptingLanguage.ToInt(e.Pop()); e.CurrentStack.Add(item2 == 0 ? (object) double.NaN : item1 % item2); };
                case ScliptingInstruction.Negative: return e => { var item = e.Pop(); e.CurrentStack.Add(item is double ? (object) -(double) item : -ScliptingLanguage.ToInt(item)); };
                case ScliptingInstruction.Increase: return e => { e.CurrentStack.Add(ScliptingLanguage.ToInt(e.Pop()) + 1); };
                case ScliptingInstruction.Decrease: return e => { e.CurrentStack.Add(ScliptingLanguage.ToInt(e.Pop()) - 1); };
                case ScliptingInstruction.Left: return e => { var item2 = ScliptingLanguage.ToInt(e.Pop()); var item1 = ScliptingLanguage.ToInt(e.Pop()); if (item2 < 0) e.CurrentStack.Add(double.NaN); else { for (; item2 > 0; item2--) item1 *= 2; e.CurrentStack.Add(item1); } };
                case ScliptingInstruction.Right: return e => { var item2 = ScliptingLanguage.ToInt(e.Pop()); var item1 = ScliptingLanguage.ToInt(e.Pop()); var negative = item1 < 0; if (item2 < 0) e.CurrentStack.Add(double.NaN); else { for (; item2 > 0; item2--) { item1 /= 2; if (negative) item1--; } e.CurrentStack.Add(item1); } };
                case ScliptingInstruction.Both: return e => { e.CurrentStack.Add(ScliptingLanguage.ToInt(e.Pop()) & ScliptingLanguage.ToInt(e.Pop())); };
                case ScliptingInstruction.Other: return e => { e.CurrentStack.Add(ScliptingLanguage.ToInt(e.Pop()) | ScliptingLanguage.ToInt(e.Pop())); };
                case ScliptingInstruction.Clever: return e => { e.CurrentStack.Add(ScliptingLanguage.ToInt(e.Pop()) ^ ScliptingLanguage.ToInt(e.Pop())); };


                // LOGIC

                case ScliptingInstruction.Small: return e => { e.NumericOperation((i1, i2) => i1 < i2 ? BigInteger.One : BigInteger.Zero, (i1, i2) => i1 < i2 ? BigInteger.One : BigInteger.Zero); };
                case ScliptingInstruction.Great: return e => { e.NumericOperation((i1, i2) => i1 > i2 ? BigInteger.One : BigInteger.Zero, (i1, i2) => i1 > i2 ? BigInteger.One : BigInteger.Zero); };
                case ScliptingInstruction.And: return e => { var item2 = ScliptingLanguage.ToInt(e.Pop()); var item1 = ScliptingLanguage.ToInt(e.Pop()); e.CurrentStack.Add(item1 != 0 && item2 != 0 ? BigInteger.One : BigInteger.Zero); };
                case ScliptingInstruction.Or: return e => { var item2 = ScliptingLanguage.ToInt(e.Pop()); var item1 = ScliptingLanguage.ToInt(e.Pop()); e.CurrentStack.Add(item1 != 0 || item2 != 0 ? BigInteger.One : BigInteger.Zero); };
                case ScliptingInstruction.OneOfPair: return e => { var item2 = ScliptingLanguage.ToInt(e.Pop()); e.CurrentStack.Add(ScliptingLanguage.ToInt(e.Pop()) != 0 ^ item2 != 0 ? BigInteger.One : BigInteger.Zero); };
                case ScliptingInstruction.Same: return e => { var item2 = e.Pop(); e.CurrentStack.Add(e.Pop().Equals(item2) ? BigInteger.One : BigInteger.Zero); };
                case ScliptingInstruction.Equal: return e => { var item2 = ScliptingLanguage.ToInt(e.Pop()); e.CurrentStack.Add(ScliptingLanguage.ToInt(e.Pop()) == item2 ? BigInteger.One : BigInteger.Zero); };
                case ScliptingInstruction.Resemble: return e => { var item2 = ScliptingLanguage.ToString(e.Pop()); e.CurrentStack.Add(ScliptingLanguage.ToString(e.Pop()) == item2 ? BigInteger.One : BigInteger.Zero); };
                case ScliptingInstruction.IsIt: return e => { var no = e.Pop(); var yes = e.Pop(); e.CurrentStack.Add(ScliptingLanguage.IsTrue(e.Pop()) ? yes : no); };
                case ScliptingInstruction.Power: return e =>
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
                    throw new InternalErrorException("Unknown instruction: “{0}”".Fmt(instruction));
            }
        }

        private static Action<ScliptingExecutionEnvironment> regexSplit(bool pop)
        {
            return e =>
            {
                var regex = ScliptingLanguage.ToString(e.Pop());
                var input = ScliptingLanguage.ToString(e.CurrentStack.Last());
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

        private static void repeat(ScliptingExecutionEnvironment e)
        {
            string str;
            byte[] b;

            var numTimes = (int) ScliptingLanguage.ToInt(e.Pop());
            var item = e.Pop();
            if (numTimes < 1)
                e.CurrentStack.Add("");
            else if ((str = item as string) != null)
            {
                if (numTimes == 1) e.CurrentStack.Add(str);
                else if (numTimes == 2) e.CurrentStack.Add(str + str);
                else if (numTimes == 3) e.CurrentStack.Add(str + str + str);
                else if (numTimes == 4) e.CurrentStack.Add(str + str + str + str);
                else
                {
                    var sb = new StringBuilder();
                    for (; numTimes > 0; numTimes--)
                        sb.Append(str);
                    e.CurrentStack.Add(sb.ToString());
                }
            }
            else if ((b = item as byte[]) != null)
            {
                var newByteArray = new byte[b.Length * numTimes];
                while (--numTimes >= 0)
                    Array.Copy(b, 0, newByteArray, numTimes * b.Length, b.Length);
                e.CurrentStack.Add(newByteArray);
            }
            else
            {
                var list = ScliptingLanguage.ToList(item);
                var newList = new List<object>(list.Count * numTimes);
                while (numTimes-- > 0)
                    newList.AddRange(list);
                e.CurrentStack.Add(newList);
            }
        }

        private static Action<ScliptingExecutionEnvironment> stringListOperation(bool pop, Func<string, object> stringOperation, Func<List<object>, object> listOperation)
        {
            return e =>
            {
                List<object> list;

                var item = pop ? e.Pop() : e.CurrentStack.Last();
                if ((list = item as List<object>) != null)
                    e.CurrentStack.Add(listOperation(list));
                else
                    e.CurrentStack.Add(stringOperation(ScliptingLanguage.ToString(item)));
            };
        }

        private static Action<ScliptingExecutionEnvironment> stringListOperation(bool pop, Func<string, BigInteger, object> stringOperation, Func<List<object>, BigInteger, object> listOperation)
        {
            return e =>
            {
                List<object> list;

                var integer = ScliptingLanguage.ToInt(e.Pop());
                var item = pop ? e.Pop() : e.CurrentStack.Last();
                if ((list = item as List<object>) != null)
                    e.CurrentStack.Add(listOperation(list, integer));
                else
                    e.CurrentStack.Add(stringOperation(ScliptingLanguage.ToString(item), integer));
            };
        }

        private static Action<ScliptingExecutionEnvironment> combineOperation(bool stringify)
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
                    ? (object) items.Select(item => ScliptingLanguage.ToString(item)).JoinString()
                    : (object) items);
            };
        }

        public override IEnumerable<Position> Execute(ScliptingExecutionEnvironment environment)
        {
            yield return new Position(Index, Count);
            getMethod(ThisInstruction)(environment);
        }

        private static void PostBuildCheck(IPostBuildReporter rep)
        {
            var taken = new HashSet<char>();
            foreach (var field in typeof(ScliptingInstruction).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var attr = field.GetCustomAttributes<InstructionAttribute>().First();
                var instr = (ScliptingInstruction) field.GetValue(null);
                if (attr.Type == InstructionType.SingularInstruction)
                {
                    try { getMethod(instr); }
                    catch { rep.Error(@"Instruction ""{0}"" has no method.".Fmt(instr), "SingularInstruction", "Action<ScliptingExecutionEnvironment> getMethod", "default"); }
                }
                var ch = attr.Character;
                if (!taken.Add(ch))
                    rep.Error(@"Same character is used multiple times for the same instruction.".Fmt(instr), "enum ScliptingInstructions", ch.ToString());
            }
        }
    }

    sealed class StackOrRegexInstruction : Instruction
    {
        public StackOrRegexInstructionType Type;
        public int Value;

        public override IEnumerable<Position> Execute(ScliptingExecutionEnvironment environment)
        {
            yield return new Position(Index, Count);
            object item;
            switch (Type)
            {
                case StackOrRegexInstructionType.RegexCapture:
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
                            case StackOrRegexInstructionType.CopyFromTop:
                                environment.CurrentStack.Add(environment.CurrentStack[environment.CurrentStack.Count - Value]);
                                break;
                            case StackOrRegexInstructionType.CopyFromBottom:
                                environment.CurrentStack.Add(environment.CurrentStack[Value - 1]);
                                break;
                            case StackOrRegexInstructionType.MoveFromTop:
                                item = environment.CurrentStack[environment.CurrentStack.Count - Value];
                                environment.CurrentStack.RemoveAt(environment.CurrentStack.Count - Value);
                                environment.CurrentStack.Add(item);
                                break;
                            case StackOrRegexInstructionType.MoveFromBottom:
                                environment.CurrentStack.Add(environment.CurrentStack[Value - 1]);
                                environment.CurrentStack.RemoveAt(Value - 1);
                                break;
                            case StackOrRegexInstructionType.SwapFromBottom:
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

    abstract class BlockInstruction : Instruction
    {
        public List<Instruction> PrimaryBlock;
        public List<Instruction> ElseBlock;
        public bool PrimaryBlockPops, ElseBlockPops;
        public int ElseIndex;
    }

    sealed class ForLoop : BlockInstruction
    {
        public override IEnumerable<Position> Execute(ScliptingExecutionEnvironment environment)
        {
            yield return new Position(Index, 1);
            var b = ScliptingLanguage.ToInt(environment.Pop());
            var a = ScliptingLanguage.ToInt(environment.Pop());
            if (a > b)
            {
                // “End” instruction
                yield return new Position(Index + Count - 1, 1);
            }
            else
            {
                for (var i = a; i <= b; i++)
                {
                    yield return new Position(Index, 1);
                    environment.CurrentStack.Add(i);
                    foreach (var instruction in PrimaryBlock)
                        foreach (var pos in instruction.Execute(environment))
                            yield return pos;
                    // “End” instruction
                    yield return new Position(Index + Count - 1, 1);
                }
            }
        }
    }

    sealed class ForEachLoop : BlockInstruction
    {
        public override IEnumerable<Position> Execute(ScliptingExecutionEnvironment environment)
        {
            yield return new Position(Index, 1);
            var orig = environment.Pop();
            var list = orig is string ? ((string) orig).Select(ch => (object) ch.ToString()).ToList() : ScliptingLanguage.ToList(orig);
            bool any = false;
            foreach (var item in list)
            {
                any = true;
                yield return new Position(Index, 1);
                environment.CurrentStack.Add(item);
                foreach (var instruction in PrimaryBlock)
                    foreach (var pos in instruction.Execute(environment))
                        yield return pos;
                // “End” instruction
                yield return new Position(Index + Count - 1, 1);
            }
            if (!any)
            {
                // “End” instruction
                yield return new Position(Index + Count - 1, 1);
            }
        }
    }

    sealed class If : BlockInstruction
    {
        public override IEnumerable<Position> Execute(ScliptingExecutionEnvironment environment)
        {
            yield return new Position(Index, 1);
            var item = environment.CurrentStack.Last();
            if (ScliptingLanguage.IsTrue(item))
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

    sealed class WhileLoop : BlockInstruction
    {
        public bool WhileTrue;

        public override IEnumerable<Position> Execute(ScliptingExecutionEnvironment environment)
        {
            yield return new Position(Index, 1);
            var item = environment.CurrentStack.Last();
            if (ScliptingLanguage.IsTrue(item) != WhileTrue)
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
                while (ScliptingLanguage.IsTrue(item) == WhileTrue);
                if (PrimaryBlockPops)
                    environment.Pop();
            }

            // “End” instruction
            yield return new Position(Index + Count - 1, 1);
        }
    }

    sealed class RegexSubstitute : BlockInstruction
    {
        public bool FirstMatchOnly;

        public override IEnumerable<Position> Execute(ScliptingExecutionEnvironment environment)
        {
            yield return new Position(Index, 1);
            var regex = ScliptingLanguage.ToString(environment.Pop());
            var input = ScliptingLanguage.ToString(environment.CurrentStack.Last());
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
                    environment.RegexObjects.Add(new RegexMatch(input, regex, offset, m));
                    if (i > 0)
                        yield return new Position(Index, 1);
                    foreach (var instruction in PrimaryBlock)
                        foreach (var position in instruction.Execute(environment))
                            yield return position;
                    yield return new Position(Index + Count - 1, 1);
                    var subst = ScliptingLanguage.ToString(environment.Pop());
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
