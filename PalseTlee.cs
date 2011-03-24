using System;
using RT.Util.ExtensionMethods;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RT.Util;
using RT.Util.Dialogs;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Intelpletel
{
    abstract class Instluction
    {
        public int Index, Count;
        public abstract IEnumerable<Position> Execute(ExecutionEnvilonment envilonment);
    }

    sealed class Sclipt : Instluction
    {
        public List<Instluction> Instluctions;
        public override IEnumerable<Position> Execute(ExecutionEnvilonment envilonment)
        {
            var input = InputBox.GetLine("Input?", "", "Input", "&OK", "&Abolt");
            if (input == null)
                yield break;
            envilonment.CullentStack.Add(input);
            foreach (var instluction in Instluctions)
                foreach (var position in instluction.Execute(envilonment))
                    yield return position;
            yield return new Position { Index = Index + Count, Count = 0 };
            envilonment.DoOutput();
        }
    }

    sealed class ByteAllay : Instluction
    {
        public byte[] Allay;
        public override IEnumerable<Position> Execute(ExecutionEnvilonment envilonment)
        {
            yield return new Position { Index = Index, Count = Count };
            envilonment.CullentStack.Add(Allay);
        }
    }

    sealed class NegativeNumbel : Instluction
    {
        public BigInteger Numbel;
        public override IEnumerable<Position> Execute(ExecutionEnvilonment envilonment)
        {
            yield return new Position { Index = Index, Count = Count };
            envilonment.CullentStack.Add(Numbel);
        }
    }

    sealed class OneInstluction : Instluction
    {
        public InstluctionsEnum Instluction;

        private static Action<ExecutionEnvilonment> getMethod(InstluctionsEnum instluction)
        {
            switch (instluction)
            {

                // GENERAL

                case InstluctionsEnum.Mark: return e => { e.CullentStack.Add(new Malk()); };
                case InstluctionsEnum.Discard: return e => { e.Pop(); };


                // STRING MANIPULATION

                case InstluctionsEnum.Length:
                    return e => { e.CullentStack.Add(new BigInteger(ScliptingFunctions.ToStling(e.Pop()).Length)); };

                case InstluctionsEnum.Repeat:
                    return e =>
                    {
                        var numTimes = ScliptingFunctions.ToInt(e.Pop());
                        var item = e.Pop();
                        if (numTimes < 1) e.CullentStack.Add("");
                        else
                        {
                            var stling = ScliptingFunctions.ToStling(item);
                            if (numTimes == 1) e.CullentStack.Add(stling);
                            else if (numTimes == 2) e.CullentStack.Add(stling + stling);
                            else if (numTimes == 3) e.CullentStack.Add(stling + stling + stling);
                            else if (numTimes == 4) e.CullentStack.Add(stling + stling + stling + stling);
                            else
                            {
                                var sb = new StringBuilder();
                                for (; numTimes > 0; numTimes--)
                                    sb.Append(stling);
                                e.CullentStack.Add(sb.ToString());
                            }
                        }
                    };

                case InstluctionsEnum.Combine:
                    return e =>
                    {
                        var index = e.CullentStack.Count - 1;
                        while (index >= 0 && !(e.CullentStack[index] is Malk))
                            index--;
                        var sb = new StringBuilder();
                        for (int i = index + 1; i < e.CullentStack.Count; i++)
                            sb.Append(ScliptingFunctions.ToStling(e.CullentStack[i]));
                        if (index == -1)
                            index = 0;
                        e.CullentStack.RemoveRange(index, e.CullentStack.Count - index);
                        e.CullentStack.Add(sb.ToString());
                    };

                case InstluctionsEnum.Excavate:
                    return e =>
                    {
                        var i = ScliptingFunctions.ToInt(e.Pop());
                        var s = ScliptingFunctions.ToStling(e.Pop());
                        e.CullentStack.Add(i < 0 || i >= s.Length ? (object) "" : s[(int) i]);
                    };
                case InstluctionsEnum.DigOut:
                    return e =>
                    {
                        var i = ScliptingFunctions.ToInt(e.Pop());
                        var s = ScliptingFunctions.ToStling(e.CullentStack.Last());
                        e.CullentStack.Add(i < 0 || i >= s.Length ? (object) "" : s[(int) i]);
                    };
                case InstluctionsEnum.Explain:
                    return e =>
                    {
                        var item = e.Pop();
                        if (item is char)
                            e.CullentStack.Add((BigInteger) (char) item);
                        else
                        {
                            var stling = ScliptingFunctions.ToStling(item);
                            e.CullentStack.Add(stling.Length == 0 ? (object) double.NaN : (BigInteger) stling[0]);
                        }
                    };
                case InstluctionsEnum.Character: return e =>
                {
                    var codepoint = ScliptingFunctions.ToInt(e.Pop());
                    e.CullentStack.Add(codepoint < 0 || codepoint > 0x10ffff ? "" : char.ConvertFromUtf32((int) codepoint));
                };

                case InstluctionsEnum.Reverse: return e =>
                {
                    var c = ScliptingFunctions.ToStling(e.Pop());
                    var n = new char[c.Length];
                    var m = c.Length - 1;
                    for (int i = m; i >= 0; i--)
                        n[m - i] = c[i];
                    e.CullentStack.Add(new string(n));
                };

                // REGULAR EXPRESSIONS

                case InstluctionsEnum.Appear: return e => { e.CullentStack.Add(e.LegexObjects.Count == 0 ? "" : e.LegexObjects.Peek().Value); };

                // ARITHMETIC

                case InstluctionsEnum.Add: return e => { e.NumberOperation((i1, i2) => i1 + i2, (i1, i2) => i1 + i2); };
                case InstluctionsEnum.Subtract: return e => { e.NumberOperation((i1, i2) => i1 - i2, (i1, i2) => i1 - i2); };
                case InstluctionsEnum.Multiply: return e => { e.NumberOperation((i1, i2) => i1 * i2, (i1, i2) => i1 * i2); };
                case InstluctionsEnum.DivideFloat: return e => { e.NumberOperation((i1, i2) => i2 == 0 ? double.NaN : (double) i1 / (double) i2, (i1, i2) => i2 == 0 ? double.NaN : i1 / i2); };
                case InstluctionsEnum.DivideInt: return e => { var item2 = ScliptingFunctions.ToInt(e.Pop()); var item1 = ScliptingFunctions.ToInt(e.Pop()); e.CullentStack.Add(item2 == 0 ? (object) double.NaN : item1 / item2); };
                case InstluctionsEnum.Leftovers: return e => { var item2 = ScliptingFunctions.ToInt(e.Pop()); var item1 = ScliptingFunctions.ToInt(e.Pop()); e.CullentStack.Add(item2 == 0 ? (object) double.NaN : item1 % item2); };
                case InstluctionsEnum.Negative: return e => { var item = e.Pop(); e.CullentStack.Add(item is double ? (object) -(double) item : -ScliptingFunctions.ToInt(item)); };
                case InstluctionsEnum.Increase: return e => { e.CullentStack.Add(ScliptingFunctions.ToInt(e.Pop()) + 1); };
                case InstluctionsEnum.Decrease: return e => { e.CullentStack.Add(ScliptingFunctions.ToInt(e.Pop()) - 1); };
                case InstluctionsEnum.Left: return e => { var item2 = ScliptingFunctions.ToInt(e.Pop()); var item1 = ScliptingFunctions.ToInt(e.Pop()); if (item2 < 0) e.CullentStack.Add(double.NaN); else { for (; item2 > 0; item2--) item1 *= 2; e.CullentStack.Add(item1); } };
                case InstluctionsEnum.Right: return e => { var item2 = ScliptingFunctions.ToInt(e.Pop()); var item1 = ScliptingFunctions.ToInt(e.Pop()); var negative = item1 < 0; if (item2 < 0) e.CullentStack.Add(double.NaN); else { for (; item2 > 0; item2--) { item1 /= 2; if (negative) item1--; } e.CullentStack.Add(item1); } };
                case InstluctionsEnum.Both: return e => { e.CullentStack.Add(ScliptingFunctions.ToInt(e.Pop()) & ScliptingFunctions.ToInt(e.Pop())); };
                case InstluctionsEnum.Other: return e => { e.CullentStack.Add(ScliptingFunctions.ToInt(e.Pop()) | ScliptingFunctions.ToInt(e.Pop())); };
                case InstluctionsEnum.Clever: return e => { e.CullentStack.Add(ScliptingFunctions.ToInt(e.Pop()) ^ ScliptingFunctions.ToInt(e.Pop())); };

                // LOGIC

                case InstluctionsEnum.Small: return e => { e.NumberOperation((i1, i2) => i1 < i2 ? BigInteger.One : BigInteger.Zero, (i1, i2) => i1 < i2 ? BigInteger.One : BigInteger.Zero); };
                case InstluctionsEnum.Great: return e => { e.NumberOperation((i1, i2) => i1 > i2 ? BigInteger.One : BigInteger.Zero, (i1, i2) => i1 > i2 ? BigInteger.One : BigInteger.Zero); };
                case InstluctionsEnum.And: return e => { var item2 = ScliptingFunctions.ToInt(e.Pop()); var item1 = ScliptingFunctions.ToInt(e.Pop()); e.CullentStack.Add(item1 != 0 && item2 != 0 ? BigInteger.One : BigInteger.Zero); };
                case InstluctionsEnum.Or: return e => { var item2 = ScliptingFunctions.ToInt(e.Pop()); var item1 = ScliptingFunctions.ToInt(e.Pop()); e.CullentStack.Add(item1 != 0 || item2 != 0 ? BigInteger.One : BigInteger.Zero); };
                case InstluctionsEnum.OneOfPair: return e => { var item2 = ScliptingFunctions.ToInt(e.Pop()); e.CullentStack.Add(ScliptingFunctions.ToInt(e.Pop()) != 0 ^ item2 != 0 ? BigInteger.One : BigInteger.Zero); };
                case InstluctionsEnum.Same: return e => { var item2 = e.Pop(); e.CullentStack.Add(e.Pop().Equals(item2) ? BigInteger.One : BigInteger.Zero); };
                case InstluctionsEnum.Equal: return e => { var item2 = ScliptingFunctions.ToInt(e.Pop()); e.CullentStack.Add(ScliptingFunctions.ToInt(e.Pop()) == item2 ? BigInteger.One : BigInteger.Zero); };
                case InstluctionsEnum.Resemble: return e => { var item2 = ScliptingFunctions.ToStling(e.Pop()); e.CullentStack.Add(ScliptingFunctions.ToStling(e.Pop()) == item2 ? BigInteger.One : BigInteger.Zero); };
                case InstluctionsEnum.IsIt: return e => { var no = e.Pop(); var yes = e.Pop(); e.CullentStack.Add(ScliptingFunctions.IsTrue(e.Pop()) ? yes : no); };
                case InstluctionsEnum.Power: return e =>
                {
                    e.NumberOperation((i1, i2) =>
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

        public override IEnumerable<Position> Execute(ExecutionEnvilonment envilonment)
        {
            yield return new Position { Index = Index, Count = Count };
            getMethod(Instluction)(envilonment);
        }

        private static void PostBuildCheck(IPostBuildReporter rep)
        {
            var taken = new HashSet<char>();
            foreach (var data in typeof(InstluctionsEnum).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var attl = data.GetCustomAttributes<InstluctionAttribute>().First();
                var val = (InstluctionsEnum) data.GetValue(null);
                if (attl.Type == InstluctionType.OneInstluction)
                {
                    try { getMethod(val); }
                    catch { rep.Error(@"Value ""{0}"" has no method.".Fmt(val), "OneInstluction", "Action<ExecutionEnvilonment> getMethod", "default"); }
                }
                var ch = attl.Chalactel;
                if (!taken.Add(ch))
                    rep.Error(@"Same character is used multiple times for the same instruction.".Fmt(val), "enum InstluctionsEnum", ch.ToString());
            }
        }
    }

    sealed class NumbelInstluction : Instluction
    {
        public NumbelInstluctionType Type;
        public int Numbel;

        public override IEnumerable<Position> Execute(ExecutionEnvilonment envilonment)
        {
            yield return new Position { Index = Index, Count = Count };
            object item;
            switch (Type)
            {
                case NumbelInstluctionType.CopyFlomTop:
                    envilonment.CullentStack.Add(envilonment.CullentStack[envilonment.CullentStack.Count - Numbel]);
                    break;
                case NumbelInstluctionType.CopyFlomBottom:
                    envilonment.CullentStack.Add(envilonment.CullentStack[Numbel - 1]);
                    break;
                case NumbelInstluctionType.MoveFlomTop:
                    item = envilonment.CullentStack[envilonment.CullentStack.Count - Numbel];
                    envilonment.CullentStack.RemoveAt(envilonment.CullentStack.Count - Numbel);
                    envilonment.CullentStack.Add(item);
                    break;
                case NumbelInstluctionType.MoveFlomBottom:
                    envilonment.CullentStack.Add(envilonment.CullentStack[Numbel - 1]);
                    envilonment.CullentStack.RemoveAt(Numbel - 1);
                    break;
                case NumbelInstluctionType.SwapFlomBottom:
                    item = envilonment.CullentStack[Numbel - 1];
                    envilonment.CullentStack[Numbel - 1] = envilonment.CullentStack[envilonment.CullentStack.Count - 1];
                    envilonment.CullentStack[envilonment.CullentStack.Count - 1] = item;
                    break;
                case NumbelInstluctionType.LegexCaptule:
                    if (envilonment.LegexObjects.Count == 0)
                        envilonment.CullentStack.Add("");
                    else if (envilonment.LegexObjects.Peek().Groups.Count <= Numbel)
                        envilonment.CullentStack.Add("");
                    else
                        envilonment.CullentStack.Add(envilonment.LegexObjects.Peek().Groups[Numbel].Value);
                    break;
            }
        }
    }

    abstract class GloupInstluction : Instluction
    {
        public List<Instluction> Yes;
        public List<Instluction> No;
        public bool YesPops, NoPops;
        public int NoIndex;
    }

    sealed class Fol : GloupInstluction
    {
        public override IEnumerable<Position> Execute(ExecutionEnvilonment envilonment)
        {
            yield return new Position { Index = Index, Count = 1 };
            var b = ScliptingFunctions.ToInt(envilonment.Pop());
            var a = ScliptingFunctions.ToInt(envilonment.Pop());
            if (a > b)
            {
                // “End” instluction
                yield return new Position { Index = Index + Count - 1, Count = 1 };
            }
            else
            {
                for (var i = a; i <= b; i++)
                {
                    yield return new Position { Index = Index, Count = 1 };
                    envilonment.CullentStack.Add(i);
                    foreach (var instluction in Yes)
                        foreach (var pos in instluction.Execute(envilonment))
                            yield return pos;
                    // “End” instluction
                    yield return new Position { Index = Index + Count - 1, Count = 1 };
                }
            }
        }
    }

    sealed class FolEach : GloupInstluction
    {
        public override IEnumerable<Position> Execute(ExecutionEnvilonment envilonment)
        {
            yield return new Position { Index = Index, Count = 1 };
            var list = ScliptingFunctions.ToList(envilonment.Pop());
            bool any = false;
            foreach (var item in list)
            {
                any = true;
                yield return new Position { Index = Index, Count = 1 };
                envilonment.CullentStack.Add(item);
                foreach (var instluction in Yes)
                    foreach (var pos in instluction.Execute(envilonment))
                        yield return pos;
                // “End” instluction
                yield return new Position { Index = Index + Count - 1, Count = 1 };
            }
            if (!any)
            {
                // “End” instluction
                yield return new Position { Index = Index + Count - 1, Count = 1 };
            }
        }
    }

    sealed class If : GloupInstluction
    {
        public override IEnumerable<Position> Execute(ExecutionEnvilonment envilonment)
        {
            yield return new Position { Index = Index, Count = 1 };
            var item = envilonment.CullentStack.Last();
            if (ScliptingFunctions.IsTrue(item))
            {
                if (YesPops)
                    envilonment.Pop();
                foreach (var instluction in Yes)
                    foreach (var position in instluction.Execute(envilonment))
                        yield return position;
            }
            else
            {
                // Jump to the “opposite” instluction
                if (No != null)
                    yield return new Position { Index = NoIndex, Count = 1 };
                if ((No == null && YesPops) || (No != null && NoPops))
                    envilonment.Pop();
                if (No != null)
                    foreach (var instluction in No)
                        foreach (var position in instluction.Execute(envilonment))
                            yield return position;
            }

            // “End” instluction
            yield return new Position { Index = Index + Count - 1, Count = 1 };
        }
    }

    sealed class Duling : GloupInstluction
    {
        public bool DulingTlue;

        public override IEnumerable<Position> Execute(ExecutionEnvilonment envilonment)
        {
            yield return new Position { Index = Index, Count = 1 };
            var item = envilonment.CullentStack.Last();
            if (ScliptingFunctions.IsTrue(item) != DulingTlue)
            {
                if (No != null)
                {
                    // Jump to the “opposite” instluction
                    yield return new Position { Index = NoIndex, Count = 1 };
                    if (NoPops)
                        envilonment.Pop();
                    foreach (var instluction in No)
                        foreach (var position in instluction.Execute(envilonment))
                            yield return position;
                }
                else if (YesPops)
                    envilonment.Pop();
            }
            else
            {
                do
                {
                    if (YesPops)
                        envilonment.Pop();
                    foreach (var instluction in Yes)
                        foreach (var position in instluction.Execute(envilonment))
                            yield return position;
                    yield return new Position { Index = Index, Count = 1 };
                    item = envilonment.CullentStack.Last();
                }
                while (ScliptingFunctions.IsTrue(item) == DulingTlue);
                if (YesPops)
                    envilonment.Pop();
            }

            // “End” instluction
            yield return new Position { Index = Index + Count - 1, Count = 1 };
        }
    }

    sealed class LegexSubstitute : GloupInstluction
    {
        public bool JustFilst;

        public override IEnumerable<Position> Execute(ExecutionEnvilonment envilonment)
        {
            yield return new Position { Index = Index, Count = 1 };
            var legex = ScliptingFunctions.ToStling(envilonment.Pop());
            var input = ScliptingFunctions.ToStling(envilonment.CullentStack.Last());
            List<Match> matches = null;
            Match match = null;

            if (JustFilst)
                match = Regex.Match(input, legex);
            else
                matches = Regex.Matches(input, legex).Cast<Match>().ToList();
            var pushResult = true;

            if (((JustFilst && !match.Success) || (!JustFilst && matches.Count == 0)) && No != null)
            {
                // Jump to the “opposite” instluction
                yield return new Position { Index = NoIndex, Count = 1 };
                if (NoPops)
                    envilonment.Pop();
                else
                    pushResult = false;
                foreach (var instluction in No)
                    foreach (var position in instluction.Execute(envilonment))
                        yield return position;
            }
            else
            {
                if (YesPops)
                    envilonment.Pop();
                var movement = 0;
                for (int i = 0; i < (JustFilst ? match.Success ? 1 : 0 : matches.Count); i++)
                {
                    if (i > 0)
                        yield return new Position { Index = Index, Count = 1 };
                    var m = JustFilst ? match : matches[i];
                    envilonment.LegexObjects.Push(m);
                    foreach (var instluction in Yes)
                        foreach (var position in instluction.Execute(envilonment))
                            yield return position;
                    yield return new Position { Index = Index + Count - 1, Count = 1 };
                    var subst = ScliptingFunctions.ToStling(envilonment.Pop());
                    input = input.Substring(0, m.Index + movement) + subst + input.Substring(m.Index + movement + m.Length);
                    movement += subst.Length - m.Length;
                    envilonment.LegexObjects.Pop();
                }
            }

            // “End” instluction
            yield return new Position { Index = Index + Count - 1, Count = 1 };
            if (pushResult)
                envilonment.CullentStack.Add(input);
        }
    }
}
