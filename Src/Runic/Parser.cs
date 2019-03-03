using System.Collections.Generic;
using System.Globalization;
using System.Text;
using EsotericIDE.Runic.Math;
using EsotericIDE.Runic.Runes;

namespace EsotericIDE.Runic
{
    internal static class Parser
    {
        public static int inputInd;
        public static string inputStr;

        public static ParseError Parse(string v, string i, out RunicEnvironment context)
        {
            List<Vector2Int> entries = new List<Vector2Int>();
            string code = v;
            inputStr = i;
            inputInd = 0;
            if (!code.Contains(";") && !code.Contains("F") && !code.Contains("@"))
            {
                context = null;
                return new ParseError(ParseErrorType.NO_TERMINATOR, Vector2Int.zero, 0, ';');
            }
            code = code.Replace("\r", string.Empty);
            string[] lines = code.Split('\n');
            int max = 0;
            foreach (string s in lines)
            {
                string s2 = s;
                for (int x = s2.Length - 1; x >= 0; x--)
                {
                    char cat = s2[x];
                    UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(cat);
                    if (uc == UnicodeCategory.NonSpacingMark || uc == UnicodeCategory.EnclosingMark || uc == UnicodeCategory.OtherNotAssigned)
                    {
                        s2 = s2.Remove(x, 1);
                    }
                }
                if (s2.Length > max)
                {
                    max = s2.Length;
                }
            }
            //char[,] runesCodes = new char[max,lines.Length];
            if (lines.Length == 1 && !(code.Contains("$") || code.Contains("@")))
            {
                lines[0] = lines[0] + "@";
                max++;
            }
            IExecutableRune[,] runes = new IExecutableRune[max, lines.Length];
            char[,] modifiers = new char[max, lines.Length];
            for (int y = 0; y < lines.Length; y++)
            {
                int mutateOffset = 0;
                for (int x = 0; x < max; x++)
                {
                    char cat = (x + mutateOffset < lines[y].Length ? lines[y][x + mutateOffset] : ' ');
                    if (cat == '\r') cat = ' ';
                    IExecutableRune r = RuneRegistry.GetRune(cat);
                    modifiers[x, y] = ' ';
                    if (r == null)
                    {
                        runes[x, y] = new RuneCharLiteral(cat);
                    }
                    else
                    {
                        runes[x, y] = r;
                        if (r is RuneEntrySimple /*&& (x == 0 || y == 0 || x == max - 1 || y == lines.Length - 1)*/)
                        {
                            char cat2 = (x + mutateOffset + 1 < lines[y].Length ? lines[y][x + mutateOffset + 1] : ' ');
                            if ((cat == '<' || cat == '>') && cat2 == '̸')
                            {
                                if (cat == '<')
                                    runes[x, y] = RuneRegistry.GetRune('(');
                                if (cat == '>')
                                    runes[x, y] = RuneRegistry.GetRune(')');
                            }
                            else
                            {
                                entries.Add(new Vector2Int(x, y));
                            }
                        }
                    }
                    while (x + mutateOffset + 1 < lines[y].Length)
                    {
                        char mod = lines[y][x + mutateOffset + 1];
                        UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(mod);
                        if (uc == UnicodeCategory.NonSpacingMark || uc == UnicodeCategory.EnclosingMark || uc == UnicodeCategory.OtherNotAssigned)
                        {
                            if (modifiers[x, y] != ' ')
                            {
                                context = null;
                                int p = modifiers.GetLength(0) * y + x;
                                return new ParseError(ParseErrorType.TOO_MANY_MODIFIERS, new Vector2Int(x, y), p, mod);
                            }
                            IExecutableRune rr = runes[x, y];
                            if (rr is RuneReflector || rr is RuneReflectAll || rr is RuneDiagonalReflector || rr is RuneDirection)
                            {
                                context = null;
                                int p = runes.GetLength(0) * y + x;
                                return new ParseError(ParseErrorType.INVALID_MODIFIER, new Vector2Int(x, y), p, mod);
                            }
                            modifiers[x, y] = mod;
                            mutateOffset++;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            if (entries.Count == 0)
            {
                if (lines.Length == 1)
                {
                    entries.Add(new Vector2Int(-1, 0));
                }
                else
                {
                    context = null;
                    return new ParseError(ParseErrorType.NO_ENTRY, Vector2Int.zero, 0, '>');
                }
            }
            context = new RunicEnvironment(v, runes, entries, modifiers);
            context.SetReader(DefaultReader).SetWriter(context.GetDefaultWriter());
            return new ParseError(ParseErrorType.NONE, Vector2Int.zero, 0, ' ');
        }

        public static object DefaultReader()
        {
            StringBuilder sb = new StringBuilder();
            bool forceread = false;
            while (inputInd < inputStr.Length)
            {
                char c = Read();
                if (c == '\r') continue;
                if (!forceread && char.IsWhiteSpace(c)) break;
                if (!forceread && c == '\\')
                {
                    forceread = true;
                }
                else
                {
                    forceread = false;
                    sb.Append(c);
                    //if(context.GetModifier(pointer.position.x, pointer.position.y) == '͍') {
                    //	break;
                    //}
                }
            }
            double d;
            string s = sb.ToString();
            if (double.TryParse(s, out d))
            {
                if (MathHelper.Approximately((float) d, (int) d))
                {
                    return (int) d;
                }
                return (d);
            }
            else if (sb.Length > 1)
            {
                return s;
            }
            else if (sb.Length > 0)
            {
                return s[0];
            }
            return null;
        }

        private static char Read()
        {
            char c = inputStr[inputInd];
            inputInd++;
            return c;
        }
    }
}
