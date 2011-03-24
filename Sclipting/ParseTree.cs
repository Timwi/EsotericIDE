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
            var input = InputBox.GetLine("Input?", "", "Input", "&OK", "&Abolt");
            if (input == null)
                yield break;
            environment.CurrentStack.Add(input);
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

    sealed class SingleInstruction : Instruction
    {
        public ScliptingInstructions ThisInstruction;

        private static Action<ScliptingExecutionEnvironment> getMethod(ScliptingInstructions instruction)
        {
            switch (instruction)
            {

                // GENERAL

                case ScliptingInstructions.Mark: return e => { e.CurrentStack.Add(new Mark()); };
                case ScliptingInstructions.Discard: return e => { e.Pop(); };


                // STRING MANIPULATION

                case ScliptingInstructions.Length:
                    return e => { e.CurrentStack.Add(new BigInteger(ScliptingUtil.ToString(e.Pop()).Length)); };

                case ScliptingInstructions.Repeat:
                    return e =>
                    {
                        var numTimes = ScliptingUtil.ToInt(e.Pop());
                        var item = e.Pop();
                        if (numTimes < 1) e.CurrentStack.Add("");
                        else
                        {
                            var stling = ScliptingUtil.ToString(item);
                            if (numTimes == 1) e.CurrentStack.Add(stling);
                            else if (numTimes == 2) e.CurrentStack.Add(stling + stling);
                            else if (numTimes == 3) e.CurrentStack.Add(stling + stling + stling);
                            else if (numTimes == 4) e.CurrentStack.Add(stling + stling + stling + stling);
                            else
                            {
                                var sb = new StringBuilder();
                                for (; numTimes > 0; numTimes--)
                                    sb.Append(stling);
                                e.CurrentStack.Add(sb.ToString());
                            }
                        }
                    };

                case ScliptingInstructions.Combine:
                    return e =>
                    {
                        var index = e.CurrentStack.Count - 1;
                        while (index >= 0 && !(e.CurrentStack[index] is Mark))
                            index--;
                        var sb = new StringBuilder();
                        for (int i = index + 1; i < e.CurrentStack.Count; i++)
                            sb.Append(ScliptingUtil.ToString(e.CurrentStack[i]));
                        if (index == -1)
                            index = 0;
                        e.CurrentStack.RemoveRange(index, e.CurrentStack.Count - index);
                        e.CurrentStack.Add(sb.ToString());
                    };

                case ScliptingInstructions.Excavate:
                    return e =>
                    {
                        var i = ScliptingUtil.ToInt(e.Pop());
                        var s = ScliptingUtil.ToString(e.Pop());
                        e.CurrentStack.Add(i < 0 || i >= s.Length ? (object) "" : s[(int) i]);
                    };
                case ScliptingInstructions.DigOut:
                    return e =>
                    {
                        var i = ScliptingUtil.ToInt(e.Pop());
                        var s = ScliptingUtil.ToString(e.CurrentStack.Last());
                        e.CurrentStack.Add(i < 0 || i >= s.Length ? (object) "" : s[(int) i]);
                    };
                case ScliptingInstructions.Explain:
                    return e =>
                    {
                        var item = e.Pop();
                        if (item is char)
                            e.CurrentStack.Add((BigInteger) (char) item);
                        else
                        {
                            var stling = ScliptingUtil.ToString(item);
                            e.CurrentStack.Add(stling.Length == 0 ? (object) double.NaN : (BigInteger) stling[0]);
                        }
                    };
                case ScliptingInstructions.Character: return e =>
                {
                    var codepoint = ScliptingUtil.ToInt(e.Pop());
                    e.CurrentStack.Add(codepoint < 0 || codepoint > 0x10ffff ? "" : char.ConvertFromUtf32((int) codepoint));
                };

                case ScliptingInstructions.Reverse: return e =>
                {
                    var c = ScliptingUtil.ToString(e.Pop());
                    var n = new char[c.Length];
                    var m = c.Length - 1;
                    for (int i = m; i >= 0; i--)
                        n[m - i] = c[i];
                    e.CurrentStack.Add(new string(n));
                };

                // REGULAR EXPRESSIONS

                case ScliptingInstructions.Appear: return e => { e.CurrentStack.Add(e.RegexObjects.Count == 0 ? "" : e.RegexObjects.Peek().Value); };

                // ARITHMETIC

                case ScliptingInstructions.Add: return e => { e.NumericOperation((i1, i2) => i1 + i2, (i1, i2) => i1 + i2); };
                case ScliptingInstructions.Subtract: return e => { e.NumericOperation((i1, i2) => i1 - i2, (i1, i2) => i1 - i2); };
                case ScliptingInstructions.Multiply: return e => { e.NumericOperation((i1, i2) => i1 * i2, (i1, i2) => i1 * i2); };
                case ScliptingInstructions.DivideFloat: return e => { e.NumericOperation((i1, i2) => i2 == 0 ? double.NaN : (double) i1 / (double) i2, (i1, i2) => i2 == 0 ? double.NaN : i1 / i2); };
                case ScliptingInstructions.DivideInt: return e => { var item2 = ScliptingUtil.ToInt(e.Pop()); var item1 = ScliptingUtil.ToInt(e.Pop()); e.CurrentStack.Add(item2 == 0 ? (object) double.NaN : item1 / item2); };
                case ScliptingInstructions.Leftovers: return e => { var item2 = ScliptingUtil.ToInt(e.Pop()); var item1 = ScliptingUtil.ToInt(e.Pop()); e.CurrentStack.Add(item2 == 0 ? (object) double.NaN : item1 % item2); };
                case ScliptingInstructions.Negative: return e => { var item = e.Pop(); e.CurrentStack.Add(item is double ? (object) -(double) item : -ScliptingUtil.ToInt(item)); };
                case ScliptingInstructions.Increase: return e => { e.CurrentStack.Add(ScliptingUtil.ToInt(e.Pop()) + 1); };
                case ScliptingInstructions.Decrease: return e => { e.CurrentStack.Add(ScliptingUtil.ToInt(e.Pop()) - 1); };
                case ScliptingInstructions.Left: return e => { var item2 = ScliptingUtil.ToInt(e.Pop()); var item1 = ScliptingUtil.ToInt(e.Pop()); if (item2 < 0) e.CurrentStack.Add(double.NaN); else { for (; item2 > 0; item2--) item1 *= 2; e.CurrentStack.Add(item1); } };
                case ScliptingInstructions.Right: return e => { var item2 = ScliptingUtil.ToInt(e.Pop()); var item1 = ScliptingUtil.ToInt(e.Pop()); var negative = item1 < 0; if (item2 < 0) e.CurrentStack.Add(double.NaN); else { for (; item2 > 0; item2--) { item1 /= 2; if (negative) item1--; } e.CurrentStack.Add(item1); } };
                case ScliptingInstructions.Both: return e => { e.CurrentStack.Add(ScliptingUtil.ToInt(e.Pop()) & ScliptingUtil.ToInt(e.Pop())); };
                case ScliptingInstructions.Other: return e => { e.CurrentStack.Add(ScliptingUtil.ToInt(e.Pop()) | ScliptingUtil.ToInt(e.Pop())); };
                case ScliptingInstructions.Clever: return e => { e.CurrentStack.Add(ScliptingUtil.ToInt(e.Pop()) ^ ScliptingUtil.ToInt(e.Pop())); };

                // LOGIC

                case ScliptingInstructions.Small: return e => { e.NumericOperation((i1, i2) => i1 < i2 ? BigInteger.One : BigInteger.Zero, (i1, i2) => i1 < i2 ? BigInteger.One : BigInteger.Zero); };
                case ScliptingInstructions.Great: return e => { e.NumericOperation((i1, i2) => i1 > i2 ? BigInteger.One : BigInteger.Zero, (i1, i2) => i1 > i2 ? BigInteger.One : BigInteger.Zero); };
                case ScliptingInstructions.And: return e => { var item2 = ScliptingUtil.ToInt(e.Pop()); var item1 = ScliptingUtil.ToInt(e.Pop()); e.CurrentStack.Add(item1 != 0 && item2 != 0 ? BigInteger.One : BigInteger.Zero); };
                case ScliptingInstructions.Or: return e => { var item2 = ScliptingUtil.ToInt(e.Pop()); var item1 = ScliptingUtil.ToInt(e.Pop()); e.CurrentStack.Add(item1 != 0 || item2 != 0 ? BigInteger.One : BigInteger.Zero); };
                case ScliptingInstructions.OneOfPair: return e => { var item2 = ScliptingUtil.ToInt(e.Pop()); e.CurrentStack.Add(ScliptingUtil.ToInt(e.Pop()) != 0 ^ item2 != 0 ? BigInteger.One : BigInteger.Zero); };
                case ScliptingInstructions.Same: return e => { var item2 = e.Pop(); e.CurrentStack.Add(e.Pop().Equals(item2) ? BigInteger.One : BigInteger.Zero); };
                case ScliptingInstructions.Equal: return e => { var item2 = ScliptingUtil.ToInt(e.Pop()); e.CurrentStack.Add(ScliptingUtil.ToInt(e.Pop()) == item2 ? BigInteger.One : BigInteger.Zero); };
                case ScliptingInstructions.Resemble: return e => { var item2 = ScliptingUtil.ToString(e.Pop()); e.CurrentStack.Add(ScliptingUtil.ToString(e.Pop()) == item2 ? BigInteger.One : BigInteger.Zero); };
                case ScliptingInstructions.IsIt: return e => { var no = e.Pop(); var yes = e.Pop(); e.CurrentStack.Add(ScliptingUtil.IsTrue(e.Pop()) ? yes : no); };
                case ScliptingInstructions.Power: return e =>
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
                    throw new InvalidOperationException("Instluction not know what?!");
            }
        }

        public override IEnumerable<Position> Execute(ScliptingExecutionEnvironment environment)
        {
            yield return new Position(Index, Count);
            getMethod(ThisInstruction)(environment);
        }

        private static void PostBuildCheck(IPostBuildReporter rep)
        {
            var taken = new HashSet<char>();
            foreach (var field in typeof(ScliptingInstructions).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var attr = field.GetCustomAttributes<InstructionAttribute>().First();
                var instr = (ScliptingInstructions) field.GetValue(null);
                if (attr.Type == InstructionType.SingleInstruction)
                {
                    try { getMethod(instr); }
                    catch { rep.Error(@"Instruction ""{0}"" has no method.".Fmt(instr), "SingleInstruction", "Action<ExecutionEnvironment> getMethod", "default"); }
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
                case StackOrRegexInstructionType.RegexCapture:
                    if (environment.RegexObjects.Count == 0)
                        environment.CurrentStack.Add("");
                    else if (environment.RegexObjects.Peek().Groups.Count <= Value)
                        environment.CurrentStack.Add("");
                    else
                        environment.CurrentStack.Add(environment.RegexObjects.Peek().Groups[Value].Value);
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
            var b = ScliptingUtil.ToInt(environment.Pop());
            var a = ScliptingUtil.ToInt(environment.Pop());
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
            var list = ScliptingUtil.ToList(environment.Pop());
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
            if (ScliptingUtil.IsTrue(item))
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
            if (ScliptingUtil.IsTrue(item) != WhileTrue)
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
                while (ScliptingUtil.IsTrue(item) == WhileTrue);
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
            var regex = ScliptingUtil.ToString(environment.Pop());
            var input = ScliptingUtil.ToString(environment.CurrentStack.Last());
            List<Match> matches = null;
            Match match = null;

            if (FirstMatchOnly)
                match = Regex.Match(input, regex);
            else
                matches = Regex.Matches(input, regex).Cast<Match>().ToList();
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
                    if (i > 0)
                        yield return new Position(Index, 1);
                    var m = FirstMatchOnly ? match : matches[i];
                    environment.RegexObjects.Push(m);
                    foreach (var instruction in PrimaryBlock)
                        foreach (var position in instruction.Execute(environment))
                            yield return position;
                    yield return new Position(Index + Count - 1, 1);
                    var subst = ScliptingUtil.ToString(environment.Pop());
                    input = input.Substring(0, m.Index + offset) + subst + input.Substring(m.Index + offset + m.Length);
                    offset += subst.Length - m.Length;
                    environment.RegexObjects.Pop();
                }
            }

            // “End” instruction
            yield return new Position(Index + Count - 1, 1);
            if (pushResult)
                environment.CurrentStack.Add(input);
        }
    }
}
