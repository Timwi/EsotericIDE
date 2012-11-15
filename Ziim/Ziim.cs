using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace EsotericIDE.Languages
{
    sealed partial class Ziim : ProgrammingLanguage
    {
        public override string LanguageName { get { return "Ziim"; } }
        public override string DefaultFileExtension { get { return "ziim"; } }

        // Single arrow angle combinations
        private int[] _zero = { };
        private int[] _stdin = { 7 };
        private int[] _concat = { 1, 7 };
        private int[] _inverse = { 5 };
        private int[] _noop = { 3 };
        private int[] _label = { 3, 5 };

        // Double arrow angle combinations
        private int[][] _splitter = { new[] { 2 }, new[] { 6 } };
        private int[][] _isZero = { new[] { 3 }, new[] { 7 } };
        private int[][] _isEmpty = { new[] { 1 }, new[] { 5 } };

        private int[] _dx = { -1, 0, 1, 1, 1, 0, -1, -1 };
        private int[] _dy = { -1, -1, -1, 0, 1, 1, 1, 0 };
        private string[] _darr = { "↖⤡", "↑↕", "↗⤢", "→↔", "↘⤡", "↓↕", "↙⤢", "←↔" };

        public override ExecutionEnvironment Compile(string source, string input)
        {
            var lines = source.Replace("\r", "").Split('\n');
            if (lines.Length == 0)
                throw new CompileException("Program does not contain any arrows.", 0, 0);

            var width = lines.Max(line => line.Length);
            var height = lines.Length;
            var chars = lines.Select(line => line + new string(' ', width - line.Length)).ToArray();

            var forEachArrow = Ut.Lambda((Action<int, int, int, bool, int, char> action) =>
            {
                int x = 0, y = 0;
                for (int i = 0; i < source.Length; i++)
                {
                    var ch = source[i];
                    if (ch == '\r')
                        continue;
                    else if (ch == '\n')
                    {
                        y++;
                        x = 0;
                        continue;
                    }
                    if (chars[y][x] != ch)
                        throw new CompileException("Internal parse error: This is a bug in the parser.", i, 1);
                    if (ch != ' ')
                    {
                        var p = "↖↑↗→↘↓↙←".IndexOf(ch);
                        var single = true;
                        if (p == -1)
                        {
                            p = "⤡↕⤢↔".IndexOf(ch);
                            single = false;
                        }
                        if (p == -1)
                            throw new CompileException("Invalid character: “{0}”.".Fmt(ch), i, 1);
                        action(x, y, i, single, p, ch);
                    }
                    x++;
                }
            });

            var followArrow = Ut.Lambda((int ax, int ay, int dx, int dy, Action<int, int> found) =>
            {
                while (true)
                {
                    ax += dx;
                    ay += dy;
                    if (ax < 0 || ax >= width || ay < 0 || ay >= height)
                        return false;
                    if (chars[ay][ax] != ' ')
                    {
                        found(ax, ay);
                        return true;
                    }
                }
            });

            // STEP 1: Determine, for each arrow, which incoming directions have other arrows point at it
            var pointedToBy = new Dictionary<int, Dictionary<int, List<Tuple<int, int, int>>>>();
            forEachArrow((x, y, i, single, p, ch) =>
            {
                for (int dd = 0; dd < 8; dd++)
                    followArrow(x, y, -_dx[dd], -_dy[dd], (fx, fy) =>
                    {
                        if (_darr[dd].Contains(chars[fy][fx]))
                            pointedToBy.AddSafe(x, y, Tuple.Create(fx, fy, (p + 8 - dd) % 8));
                    });
            });

            // STEP 2: Translate each arrow into a ‘node’ instance with the appropriate ‘instruction’
            var nodes = Ut.NewArray<node>(height, width);
            forEachArrow((x, y, i, single, p, ch) =>
            {
                var pointedToFrom = (pointedToBy.ContainsKey(x) && pointedToBy[x].ContainsKey(y) ? pointedToBy[x][y].Select(tup => tup.Item3) : Enumerable.Empty<int>()).Order().ToList();
                nodes[y][x] = new node(x, y, i,
                    single ?
                        pointedToFrom.SequenceEqual(_zero) ? instruction.Zero :
                        pointedToFrom.SequenceEqual(_stdin) ? instruction.Stdin :
                        pointedToFrom.SequenceEqual(_concat) ? instruction.Concat :
                        pointedToFrom.SequenceEqual(_inverse) ? instruction.Inverse :
                        pointedToFrom.SequenceEqual(_noop) ? instruction.NoOp :
                        pointedToFrom.SequenceEqual(_label) ? instruction.Label :
                        Ut.Throw<instruction>(new CompileException("Invalid combination of arrows pointing at arrow.", i, 1))
                    : // double arrow
                        pointedToFrom.SequenceEqual(_splitter[0]) || pointedToFrom.SequenceEqual(_splitter[1]) ? instruction.Splitter :
                        pointedToFrom.SequenceEqual(_isZero[0]) || pointedToFrom.SequenceEqual(_isZero[1]) ? instruction.IsZero :
                        pointedToFrom.SequenceEqual(_isEmpty[0]) || pointedToFrom.SequenceEqual(_isEmpty[1]) ? instruction.IsEmpty :
                        Ut.Throw<instruction>(new CompileException("Invalid combination of arrows pointing at arrow.", i, 1)));
            });

            // STEP 3: For each node, determine which other nodes it is pointing to and are pointing to it
            forEachArrow((x, y, i, single, p, ch) =>
            {
                var pointedToFrom = (pointedToBy.ContainsKey(x) && pointedToBy[x].ContainsKey(y) ? pointedToBy[x][y] : Enumerable.Empty<Tuple<int, int, int>>()).OrderBy(tup => tup.Item3).ToList();
                if (single)
                {
                    if (!followArrow(x, y, _dx[p], _dy[p], (fx, fy) => { nodes[y][x].PointsTo.Add(nodes[fy][fx]); }))
                        nodes[y][x].PointsTo.Add(null);
                }
                else
                {
                    if (!followArrow(x, y, _dx[p], _dy[p], (fx, fy) => { nodes[y][x].PointsTo.Add(nodes[fy][fx]); }))
                        nodes[y][x].PointsTo.Add(null);
                    if (!followArrow(x, y, -_dx[p], -_dy[p], (fx, fy) => { nodes[y][x].PointsTo.Add(nodes[fy][fx]); }))
                        nodes[y][x].PointsTo.Add(null);
                    var pointedToFrom2 = pointedToFrom.Select(tup => tup.Item3);
                    if (pointedToFrom2.SequenceEqual(_isZero[1]) || pointedToFrom2.SequenceEqual(_isEmpty[1]))
                        nodes[y][x].PointsTo.Reverse();
                }
                nodes[y][x].PointedToBy.AddRange(pointedToFrom.Select(tup => nodes[tup.Item2][tup.Item1]));
            });

            var nodesList = nodes.SelectMany(row => row.Where(n => n != null)).ToList();
            return new ziimExecutionEnvironment
            {
                Nodes = nodesList,
                Threads = nodesList.Where(n => n.Instruction == instruction.Zero).Select(n => new thread { CurrentValue = bits.Zero, CurrentInstruction = n, Suspended = false }).ToList(),
                Input = input
            };
        }

        public override string GetInfo(string source, int cursorPosition)
        {
            return null;
        }

        public override ToolStripMenuItem[] CreateMenus(Func<string> getSelectedText, Action<string> insertText)
        {
            return new ToolStripMenuItem[0];
        }
    }
}
