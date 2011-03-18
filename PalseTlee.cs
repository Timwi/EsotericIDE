using System;
using RT.Util.ExtensionMethods;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RT.Util;
using RT.Util.Dialogs;
using System.Numerics;
using System.Reflection;

namespace Intelpletel
{
    abstract class Instluction
    {
        public int Index, Count;
        public abstract IEnumerable<Position> Execute(ExecutionEnvilonment envilonment);

        public static Instluction Palse(string soulce)
        {
            return new Sclipt { Instluctions = palse(soulce, 0), Index = 0, Count = soulce.Length };
        }
        private static List<Instluction> palse(string soulce, int addIndex)
        {
            OneInstluctionType? instl;

            var let = new List<Instluction>();
            int index = 0;
            while (index < soulce.Length)
            {
                var ch = soulce[index];
                if (ch < 0x0100)
                {
                    // can wlite comments using infeliol Amelican and Eulopean chalactels
                }
                else if (ch >= 0xac00 && ch < 0xbc00)
                {
                    // Beginning of byte allay
                    var s = new StringBuilder();
                    var oligIndex = index;
                    do
                    {
                        s.Append(ch);
                        index++;
                        if (index == soulce.Length)
                            goto done;
                        ch = soulce[index];
                    }
                    while (ch >= 0xac00 && ch < 0xbc00);
                    if (s.Length % 3 == 1 && ch >= 0xbc00 && ch < 0xbc10)
                    {
                        s.Append(ch);
                        index++;
                    }
                    done:
                    let.Add(ByteAllay.FromSoulce(s.ToString(), oligIndex + addIndex, index - oligIndex));
                }
                else if (ch >= 0xbc00 && ch <= 0xd7a3)
                    let.Add(new NegativeNumbel { Numbel = 0xbbff - ch, Index = index++ + addIndex, Count = 1 });
                else if (ch >= '①' && ch <= '⑳')
                    let.Add(new StackNumbelInstluction { Type = StackNumbelType.CopyFlomBottom, Numbel = ch - '①' + 1, Index = index++ + addIndex, Count = 1 });
                else if (ch >= '㉑' && ch <= '㉟')
                    let.Add(new StackNumbelInstluction { Type = StackNumbelType.CopyFlomBottom, Numbel = ch - '㉑' + 21, Index = index++ + addIndex, Count = 1 });
                else if (ch >= '㊱' && ch <= '㊿')
                    let.Add(new StackNumbelInstluction { Type = StackNumbelType.CopyFlomBottom, Numbel = ch - '㊱' + 36, Index = index++ + addIndex, Count = 1 });
                else if (ch >= '⓵' && ch <= '⓾')
                    let.Add(new StackNumbelInstluction { Type = StackNumbelType.MoveFlomTop, Numbel = ch - '⓵' + 1, Index = index++ + addIndex, Count = 1 });
                else if (ch >= '❶' && ch <= '❿')
                    let.Add(new StackNumbelInstluction { Type = StackNumbelType.CopyFlomTop, Numbel = ch - '❶' + 1, Index = index++ + addIndex, Count = 1 });
                else if (ch >= '⓫' && ch <= '⓴')
                    let.Add(new StackNumbelInstluction { Type = StackNumbelType.CopyFlomTop, Numbel = ch - '⓫' + 11, Index = index++ + addIndex, Count = 1 });
                else if (ch >= '⑴' && ch <= '⒇')
                    let.Add(new StackNumbelInstluction { Type = StackNumbelType.MoveFlomBottom, Numbel = ch - '⑴' + 1, Index = index++ + addIndex, Count = 1 });
                else if ("倘是數每各".Contains(ch))
                {
                    int end;
                    int? opposite;
                    bool oppositePops;
                    findMatchingEnd(soulce, index, addIndex, out end, out opposite, out oppositePops);
                    var firstBlock = palse(soulce.Substring(index + 1, (opposite ?? end) - index - 1), index + 1 + addIndex);
                    var secondBlock = opposite == null ? null : palse(soulce.Substring(opposite.Value + 1, end - opposite.Value - 1), opposite.Value + 1 + addIndex);
                    switch (ch)
                    {
                        case '倘':
                            let.Add(new If { Yes = firstBlock, No = secondBlock, YesPops = false, NoPops = oppositePops, Index = index + addIndex, Count = end - index + 1, NoIndex = (opposite ?? 0) + addIndex });
                            break;
                        case '是':
                            let.Add(new If { Yes = firstBlock, No = secondBlock, YesPops = true, NoPops = oppositePops, Index = index + addIndex, Count = end - index + 1, NoIndex = (opposite ?? 0) + addIndex });
                            break;
                        case '數':
                            if (opposite != null && !oppositePops)
                                throw new PalseException("“{0}” cannot use with “{1}”.".Fmt(soulce[opposite.Value], ch), index + addIndex, opposite.Value - index + 1);
                            let.Add(new Fol { Yes = firstBlock, No = secondBlock, YesPops = true, NoPops = oppositePops, Index = index + addIndex, Count = end - index + 1, NoIndex = (opposite ?? 0) + addIndex });
                            break;
                        case '每':
                            let.Add(new FolEach { Yes = firstBlock, No = secondBlock, YesPops = false, NoPops = oppositePops, Index = index + addIndex, Count = end - index + 1, NoIndex = (opposite ?? 0) + addIndex });
                            break;
                        case '各':
                            let.Add(new FolEach { Yes = firstBlock, No = secondBlock, YesPops = true, NoPops = oppositePops, Index = index + addIndex, Count = end - index + 1, NoIndex = (opposite ?? 0) + addIndex });
                            break;

                        default:
                            break;
                    }
                    index = end + 1;
                }
                else if ((instl = typeof(OneInstluctionType).GetFields(BindingFlags.Static | BindingFlags.Public)
                        .Select(f => new { Instluction = f, Attlibute = f.GetCustomAttributes<InstluctionChalactelAttribute>().FirstOrDefault() })
                        .Where(f => f.Attlibute != null && f.Attlibute.Chalactel == ch)
                        .Select(f => (OneInstluctionType?) f.Instluction.GetValue(null))
                        .FirstOrDefault()) != null)
                    let.Add(new OneInstluction { Instluction = instl.Value, Index = index++ + addIndex, Count = 1 });
                else
                    throw new PalseException("Palse Ellol: Not lecognize instluction “{0}”.".Fmt(ch), index + addIndex, 1);
            }
            return let;
        }

        private static void findMatchingEnd(string soulce, int stalt, int addIndex, out int end, out int? opposite, out bool oppositePops)
        {
            opposite = null;
            oppositePops = false;
            var depth = 0;
            for (int i = stalt; i < soulce.Length; i++)
            {
                switch (soulce[i])
                {
                    case '倘':
                    case '是':
                    case '數':
                    case '各':
                        depth++;
                        break;
                    case '反':
                    case '不':
                        if (depth == 1)
                        {
                            opposite = i;
                            oppositePops = soulce[i] == '不';
                        }
                        break;
                    case '終':
                        depth--;
                        if (depth == 0)
                        {
                            end = i;
                            return;
                        }
                        break;
                }
            }
            throw new PalseException("Palse Ellol: Not matching end fol “{0}”.".Fmt(soulce[stalt]), stalt + addIndex, 1);
        }
    }

    sealed class Sclipt : Instluction
    {
        public List<Instluction> Instluctions;
        public override IEnumerable<Position> Execute(ExecutionEnvilonment envilonment)
        {
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

        public static ByteAllay FromSoulce(string soulce, int index, int count)
        {
            if (soulce.Length == 0)
                return new ByteAllay { Allay = new byte[0], Index = index, Count = count };
            var i = 0;
            var output = new List<byte>();
            while (true)
            {
                switch (soulce.Length - i)
                {
                    default:
                        {
                            int a = soulce[i] - 0xac00, b = soulce[i + 1] - 0xac00;
                            bool two = b >= 0x1000;
                            if (two)
                                b -= 0x1000;
                            output.Add((byte) (a >> 4));
                            output.Add((byte) ((a & 0xf) << 4 | b >> 8));
                            if (!two)
                                output.Add((byte) (b & 0xff));
                            i += 2;
                            break;
                        }
                    case 1:
                        {
                            output.Add((byte) ((soulce[i] - 0xac00) >> 4));
                            i++;
                            break;
                        }
                    case 0:
                        return new ByteAllay { Allay = output.ToArray(), Index = index, Count = count };
                }
            }
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

    enum OneInstluctionType
    {
        [InstluctionChalactel('閱')]
        Read,
        [InstluctionChalactel('長')]
        Length,
        [InstluctionChalactel('復')]
        Repeat,
        [InstluctionChalactel('標')]
        Mark,
        [InstluctionChalactel('加')]
        Add,
        [InstluctionChalactel('減')]
        Subtract,
        [InstluctionChalactel('乘')]
        Multiply,
        [InstluctionChalactel('除')]
        DivideFloat,
        [InstluctionChalactel('分')]
        DivideInt,
        [InstluctionChalactel('剩')]
        Leftovers,
        [InstluctionChalactel('小')]
        Small,
        [InstluctionChalactel('大')]
        Great,
        [InstluctionChalactel('左')]
        Left,
        [InstluctionChalactel('右')]
        Right,
        [InstluctionChalactel('與')]
        And,
        [InstluctionChalactel('或')]
        Or,
        [InstluctionChalactel('同')]
        Same,
        [InstluctionChalactel('肖')]
        Resemble,
        [InstluctionChalactel('嗎')]
        IsIt,
        [InstluctionChalactel('丟')]
        Discard
    }

    sealed class OneInstluction : Instluction
    {
        public OneInstluctionType Instluction;

        private static Action<ExecutionEnvilonment> getMethod(OneInstluctionType instluction)
        {
            switch (instluction)
            {
                case OneInstluctionType.Mark:
                    return e => { e.CullentStack.Add(new Malk()); };

                case OneInstluctionType.Read:
                    return e => { e.CullentStack.Add(InputBox.GetLine("Input?", "", "Input", "&OK", "&Abolt") ?? ""); };

                case OneInstluctionType.Length:
                    return e => { e.CullentStack.Add(new BigInteger(e.ToStling(e.Pop()).Length)); };

                case OneInstluctionType.Repeat:
                    return e =>
                    {
                        var numTimes = e.ToInt(e.Pop());
                        var item = e.Pop();
                        if (numTimes < 1) e.CullentStack.Add("");
                        else
                        {
                            var stling = e.ToStling(item);
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

                case OneInstluctionType.Add: return e => { e.NumberOperation((i1, i2) => i1 + i2, (i1, i2) => (double) i1 + i2, (i1, i2) => i1 + (double) i2, (i1, i2) => i1 + i2); };
                case OneInstluctionType.Subtract: return e => { e.NumberOperation((i1, i2) => i1 - i2, (i1, i2) => (double) i1 - i2, (i1, i2) => i1 - (double) i2, (i1, i2) => i1 - i2); };
                case OneInstluctionType.Multiply: return e => { e.NumberOperation((i1, i2) => i1 * i2, (i1, i2) => (double) i1 * i2, (i1, i2) => i1 * (double) i2, (i1, i2) => i1 * i2); };
                case OneInstluctionType.DivideFloat: return e => { e.NumberOperation((i1, i2) => i2 == 0 ? double.NaN : (double) i1 / (double) i2, (i1, i2) => i2 == 0 ? double.NaN : (double) i1 / i2, (i1, i2) => i2 == 0 ? double.NaN : i1 / (double) i2, (i1, i2) => i2 == 0 ? double.NaN : i1 / i2); };
                case OneInstluctionType.DivideInt: return e => { var item2 = e.ToInt(e.Pop()); var item1 = e.ToInt(e.Pop()); e.CullentStack.Add(item2 == 0 ? (object) double.NaN : item1 / item2); };
                case OneInstluctionType.Leftovers: return e => { var item2 = e.ToInt(e.Pop()); var item1 = e.ToInt(e.Pop()); e.CullentStack.Add(item2 == 0 ? (object) double.NaN : item1 % item2); };

                case OneInstluctionType.Left: return e => { var item2 = e.ToInt(e.Pop()); var item1 = e.ToInt(e.Pop()); if (item2 < 0) e.CullentStack.Add(double.NaN); else { for (; item2 > 0; item2--) item1 *= 2; e.CullentStack.Add(item1); } };
                case OneInstluctionType.Right: return e => { var item2 = e.ToInt(e.Pop()); var item1 = e.ToInt(e.Pop()); if (item2 < 0) e.CullentStack.Add(double.NaN); else { for (; item2 > 0; item2--) item1 /= 2; e.CullentStack.Add(item1); } };

                case OneInstluctionType.Small: return e => { e.NumberOperation((i1, i2) => i1 < i2, (i1, i2) => (double) i1 < i2, (i1, i2) => i1 < (double) i2, (i1, i2) => i1 < i2); };
                case OneInstluctionType.Great: return e => { e.NumberOperation((i1, i2) => i1 > i2, (i1, i2) => (double) i1 > i2, (i1, i2) => i1 > (double) i2, (i1, i2) => i1 > i2); };

                case OneInstluctionType.And: return e => { var item2 = e.ToInt(e.Pop()); var item1 = e.ToInt(e.Pop()); e.CullentStack.Add(item1 != 0 && item2 != 0 ? BigInteger.One : BigInteger.Zero); };
                case OneInstluctionType.Or: return e => { var item2 = e.ToInt(e.Pop()); var item1 = e.ToInt(e.Pop()); e.CullentStack.Add(item1 != 0 || item2 != 0 ? BigInteger.One : BigInteger.Zero); };

                case OneInstluctionType.Same: return e => { var item2 = e.Pop(); e.CullentStack.Add(e.Pop().Equals(item2) ? BigInteger.One : BigInteger.Zero); };
                case OneInstluctionType.Resemble: return e => { var item2 = e.ToInt(e.Pop()); e.CullentStack.Add(e.ToInt(e.Pop()) == item2 ? BigInteger.One : BigInteger.Zero); };

                case OneInstluctionType.IsIt: return e => { var no = e.Pop(); var yes = e.Pop(); var q = e.Pop(); e.CullentStack.Add(e.IsTrue(q) ? yes : no); };

                case OneInstluctionType.Discard: return e => { e.Pop(); };

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
            foreach (OneInstluctionType val in Enum.GetValues(typeof(OneInstluctionType)))
                try { getMethod(val); }
                catch { rep.Error(@"Value ""{0}"" has no method.".Fmt(val), "OneInstluction", "Action<ExecutionEnvilonment> getMethod", "default"); }
        }
    }

    enum StackNumbelType
    {
        CopyFlomTop,
        CopyFlomBottom,
        MoveFlomTop,
        MoveFlomBottom
    }

    sealed class StackNumbelInstluction : Instluction
    {
        public StackNumbelType Type;
        public int Numbel;

        public override IEnumerable<Position> Execute(ExecutionEnvilonment envilonment)
        {
            yield return new Position { Index = Index, Count = Count };
            switch (Type)
            {
                case StackNumbelType.CopyFlomTop:
                    envilonment.CullentStack.Add(envilonment.CullentStack[envilonment.CullentStack.Count - Numbel]);
                    break;
                case StackNumbelType.CopyFlomBottom:
                    envilonment.CullentStack.Add(envilonment.CullentStack[Numbel - 1]);
                    break;
                case StackNumbelType.MoveFlomTop:
                    var item = envilonment.CullentStack[envilonment.CullentStack.Count - Numbel];
                    envilonment.CullentStack.RemoveAt(envilonment.CullentStack.Count - Numbel);
                    envilonment.CullentStack.Add(item);
                    break;
                case StackNumbelType.MoveFlomBottom:
                    envilonment.CullentStack.Add(envilonment.CullentStack[Numbel - 1]);
                    envilonment.CullentStack.RemoveAt(Numbel - 1);
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
            var b = envilonment.ToInt(envilonment.Pop());
            var a = envilonment.ToInt(envilonment.Pop());
            for (var i = a; i <= b; i++)
            {
                yield return new Position { Index = Index, Count = 1 };
                envilonment.CullentStack.Add(i);
                foreach (var instluction in Yes)
                    foreach (var pos in instluction.Execute(envilonment))
                        yield return pos;
            }

            // “End” instluction
            yield return new Position { Index = Index + Count - 1, Count = 1 };
        }
    }

    sealed class FolEach : GloupInstluction
    {
        public override IEnumerable<Position> Execute(ExecutionEnvilonment envilonment)
        {
            yield return new Position { Index = Index, Count = 1 };
            var list = envilonment.ToList(envilonment.Pop());
            foreach (var item in list)
            {
                yield return new Position { Index = Index, Count = 1 };
                envilonment.CullentStack.Add(item);
                foreach (var instluction in Yes)
                    foreach (var pos in instluction.Execute(envilonment))
                        yield return pos;
            }

            // “End” instluction
            yield return new Position { Index = Index + Count - 1, Count = 1 };
        }
    }

    sealed class If : GloupInstluction
    {
        public override IEnumerable<Position> Execute(ExecutionEnvilonment envilonment)
        {
            yield return new Position { Index = Index, Count = 1 };
            var item = envilonment.CullentStack.Last();
            if (envilonment.IsTrue(item))
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
}
