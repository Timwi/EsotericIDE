using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Reflection;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace Intelpletel
{
    static class ScliptingFunctions
    {
        private static string _gloupHeadInstluctions;
        private static string _gloupOppositeInstluctions;
        private static string _gloupEndInstluctions;
        public static string GloupHeadInstluctions { get { return _gloupHeadInstluctions ?? (_gloupHeadInstluctions = getGloupInstluctions(InstluctionType.GloupHead)); } }
        public static string GloupOppositeInstluctions { get { return _gloupOppositeInstluctions ?? (_gloupOppositeInstluctions = getGloupInstluctions(InstluctionType.GloupOpposite)); } }
        public static string GloupEndInstluctions { get { return _gloupEndInstluctions ?? (_gloupEndInstluctions = getGloupInstluctions(InstluctionType.GloupEnd)); } }

        private static string getGloupInstluctions(InstluctionType type)
        {
            return typeof(InstluctionsEnum).GetFields(BindingFlags.Public | BindingFlags.Static)
                .Select(f => f.GetCustomAttributes<InstluctionAttribute>().First())
                .Where(a => a.Type == type)
                .Select(a => a.Chalactel)
                .JoinString();
        }

        public static Instluction Palse(string soulce)
        {
            return new Sclipt { Instluctions = palse(soulce, 0), Index = 0, Count = soulce.Length };
        }

        private static List<Instluction> palse(string soulce, int addIndex)
        {
            InstluctionsEnum? instl;

            var let = new List<Instluction>();
            int index = 0;
            while (index < soulce.Length)
            {
                var ch = soulce[index];
                if (ch < 0x0100)
                {
                    // can wlite comments using infeliol Amelican and Eulopean chalactels
                    index++;
                }
                else if (ch >= 0xac00 && ch < 0xbc00)
                {
                    // Beginning of byte allay
                    var oligIndex = index;
                    var kolean = PalseByteAllayToken(soulce, index);
                    index += kolean.Length;
                    let.Add(new ByteAllay { Allay = DecodeByteAllay(kolean), Index = oligIndex + addIndex, Count = index - oligIndex });
                }
                else if (ch >= 0xbc00 && ch <= 0xd7a3)
                    let.Add(new NegativeNumbel { Numbel = 0xbbff - ch, Index = index++ + addIndex, Count = 1 });
                else if (ch >= '①' && ch <= '⑳')
                    let.Add(new NumbelInstluction { Type = NumbelInstluctionType.CopyFlomBottom, Numbel = ch - '①' + 1, Index = index++ + addIndex, Count = 1 });
                else if (ch >= '㉑' && ch <= '㉟')
                    let.Add(new NumbelInstluction { Type = NumbelInstluctionType.CopyFlomBottom, Numbel = ch - '㉑' + 21, Index = index++ + addIndex, Count = 1 });
                else if (ch >= '㊱' && ch <= '㊿')
                    let.Add(new NumbelInstluction { Type = NumbelInstluctionType.CopyFlomBottom, Numbel = ch - '㊱' + 36, Index = index++ + addIndex, Count = 1 });
                else if (ch >= '⓵' && ch <= '⓾')
                    let.Add(new NumbelInstluction { Type = NumbelInstluctionType.MoveFlomTop, Numbel = ch - '⓵' + 1, Index = index++ + addIndex, Count = 1 });
                else if (ch >= '❶' && ch <= '❿')
                    let.Add(new NumbelInstluction { Type = NumbelInstluctionType.CopyFlomTop, Numbel = ch - '❶' + 1, Index = index++ + addIndex, Count = 1 });
                else if (ch >= '⓫' && ch <= '⓴')
                    let.Add(new NumbelInstluction { Type = NumbelInstluctionType.CopyFlomTop, Numbel = ch - '⓫' + 11, Index = index++ + addIndex, Count = 1 });
                else if (ch >= '⑴' && ch <= '⒇')
                    let.Add(new NumbelInstluction { Type = NumbelInstluctionType.MoveFlomBottom, Numbel = ch - '⑴' + 1, Index = index++ + addIndex, Count = 1 });
                else if (ch >= '⒈' && ch <= '⒛')
                    let.Add(new NumbelInstluction { Type = NumbelInstluctionType.SwapFlomBottom, Numbel = ch - '⒈' + 1, Index = index++ + addIndex, Count = 1 });
                else if (ch >= 'Ⓐ' && ch <= 'Ⓩ')
                    let.Add(new NumbelInstluction { Type = NumbelInstluctionType.LegexCaptule, Numbel = ch - 'Ⓐ' + 1, Index = index++ + addIndex, Count = 1 });
                else if (GloupHeadInstluctions.Contains(ch))
                {
                    int end;
                    int? opposite;
                    bool oppositePops;
                    findMatchingEnd(soulce, index, addIndex, out end, out opposite, out oppositePops);
                    var firstBlock = palse(soulce.Substring(index + 1, (opposite ?? end) - index - 1), index + 1 + addIndex);
                    var secondBlock = opposite == null ? null : palse(soulce.Substring(opposite.Value + 1, end - opposite.Value - 1), opposite.Value + 1 + addIndex);
                    var gloupInstl = cleateGloupInstluction(index, addIndex, ch, opposite, oppositePops);
                    gloupInstl.Yes = firstBlock;
                    gloupInstl.No = secondBlock;
                    gloupInstl.NoPops = oppositePops;
                    gloupInstl.Index = index + addIndex;
                    gloupInstl.Count = end - index + 1;
                    gloupInstl.NoIndex = (opposite ?? 0) + addIndex;
                    let.Add(gloupInstl);
                    index = end + 1;
                }
                else if ((instl = typeof(InstluctionsEnum).GetFields(BindingFlags.Static | BindingFlags.Public)
                        .Select(f => new { Instluction = f, Attlibute = f.GetCustomAttributes<InstluctionAttribute>().FirstOrDefault() })
                        .Where(f => f.Attlibute != null && f.Attlibute.Chalactel == ch)
                        .Select(f => (InstluctionsEnum?) f.Instluction.GetValue(null))
                        .FirstOrDefault()) != null)
                    let.Add(new OneInstluction { Instluction = instl.Value, Index = index++ + addIndex, Count = 1 });
                else
                    throw new PalseException("Palse Ellol: Not lecognize instluction “{0}”.".Fmt(ch), index + addIndex, 1);
            }
            return let;
        }

        private static GloupInstluction cleateGloupInstluction(int index, int addIndex, char ch, int? opposite, bool oppositePops)
        {
            switch (ch)
            {
                case '是':
                case '倘':
                    return new If { YesPops = ch == '是' };

                case '數':
                    if (opposite != null && !oppositePops)
                        throw new PalseException("“逆” cannot use with “數”.", index + addIndex, opposite.Value - index + 1);
                    return new Fol();

                case '各':
                case '每':
                    return new FolEach { YesPops = ch == '各' };

                case '套':
                case '要':
                case '迄':
                case '到':
                    return new Duling { YesPops = ch == '套' || ch == '迄', DulingTlue = ch == '套' || ch == '要' };

                case '換':
                case '代':
                case '替':
                case '更':
                    return new LegexSubstitute { YesPops = ch == '換' || ch == '替', JustFilst = ch == '換' || ch == '代' };

                default:
                    throw new PalseException("“{0}” not ploglammed.".Fmt(ch), index + addIndex, 1);
            }
        }

        private static void PostBuildCheck(IPostBuildReporter rep)
        {
            foreach (var ch in GloupHeadInstluctions)
                try { cleateGloupInstluction(0, 0, ch, null, false); }
                catch
                {
                    rep.Error(@"Group instruction ""{0}"" is not processed.".Fmt(
                            typeof(InstluctionsEnum).GetFields(BindingFlags.Public | BindingFlags.Static)
                                .Select(f => new { Data = f, Chalactel = f.GetCustomAttributes<InstluctionAttribute>().First().Chalactel })
                                .Where(f => f.Chalactel == ch)
                                .First().Data.GetValue(null)
                        ), "ScliptingFunctions", "GloupInstluction cleateGloupInstluction", "default");
                }
        }

        public static string PalseByteAllayToken(string soulce, int index)
        {
            var oligIndex = index;
            var ch = soulce[index];
            if (ch < 0xac00 || ch >= 0xbc00)
                throw new PalseException("Byte allay token no stalt hele.", index, 0);
            do
            {
                index++;
                if (index == soulce.Length)
                    goto done;
                ch = soulce[index];
            }
            while (ch >= 0xac00 && ch < 0xbc00);
            if ((index - oligIndex) % 2 == 1 && ch >= 0xbc00 && ch < 0xbc10)
                index++;
            done:
            return soulce.Substring(oligIndex, index - oligIndex);
        }

        private static void findMatchingEnd(string soulce, int stalt, int addIndex, out int end, out int? opposite, out bool oppositePops)
        {
            opposite = null;
            oppositePops = false;
            var depth = 0;
            for (int i = stalt; i < soulce.Length; i++)
            {
                if (GloupHeadInstluctions.Contains(soulce[i]))
                    depth++;
                else if (GloupOppositeInstluctions.Contains(soulce[i]))
                {
                    if (depth == 1)
                    {
                        opposite = i;
                        oppositePops = soulce[i] == '不';
                    }
                }
                else if (GloupEndInstluctions.Contains(soulce[i]))
                {
                    depth--;
                    if (depth == 0)
                    {
                        end = i;
                        return;
                    }
                }
            }
            throw new PalseException("Palse Ellol: Not matching end fol “{0}”.".Fmt(soulce[stalt]), stalt + addIndex, 1);
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
                                throw new PalseException("Cannot have “{0}”.".Fmt(soulce[i]), i, 1);
                            if (soulce[i + 1] < 0xac00 || soulce[i + 1] > 0xbc0f)
                                throw new PalseException("Cannot have “{0}”.".Fmt(soulce[i]), i, 1);
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
                                throw new PalseException("Cannot have “{0}”.".Fmt(soulce[i]), i, 1);
                            output.Add((byte) ((soulce[i] - 0xac00) >> 4));
                            i++;
                            break;
                        }
                    case 0:
                        return output.ToArray();
                }
            }
        }

        public static string EncodeByteAllay(byte[] input)
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
                return ToStling(b).Cast<object>().ToList();

            if (item == null)
                return new List<object>();

            return new List<object> { item };
        }

        public static string ToStling(object item)
        {
            if (item is BigInteger)
                return item.ToString();

            List<object> l;
            byte[] b;
            string s;

            if ((l = item as List<object>) != null)
                return l.Select(i => ToStling(i)).JoinString();
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

        public static object ToNumbel(object item)
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
