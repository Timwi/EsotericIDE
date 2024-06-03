using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace EsotericIDE.Ndim
{
    sealed class NdimEnv : ExecutionEnvironment
    {
        private readonly Queue<int> _input;

        private readonly int _dimensions;
        private readonly Dictionary<Coordinate, NdimCommand> _space;
        private readonly NdimStack _stack;
        private readonly NdimPointer _pointer;
        private readonly Position _posEnd;

        public int Dimensions => _dimensions;
        public NdimStack Stack => _stack;
        public NdimPointer Pointer => _pointer;
        public bool Ended = false;
        public Coordinate? JumpPosition;
        public bool EatMode = false;

        public NdimEnv(string source, Queue<int> input)
        {
            _space = parse(source, out _dimensions);
            _input = input;
            _stack = new NdimStack();
            _pointer = new NdimPointer(_dimensions);
            _posEnd = new Position(source.Length, 0);
        }

        private static Dictionary<Coordinate, NdimCommand> parse(string source, out int dimensions)
        {
            var commandRegex = new Regex(@"^\s*(?<everything>
                (?:
                    (?:(?<dim>\d+)dim\s*;\s*)|
                    (?<comment>//[^\n]*\n)|
                    (?<cmd>
                        (?:
                            (?<chp>-?\d+)|
                            (?<noparam>\?|duplicate|jump|pop|swap|\+|-|\*|/|\^|<|>|&|\||!|input|assign|assignHere|toggleEat|printChar|print|end)|
                            (?:\#(?<push>\d))|
                            (?:if\s+(?<if>-?\d+))
                        )
                        \s*<\s*(?<coord>(-?\d+)\s*(,\s*-?\d+\s*)*)>)\s*;\s*
                    )
                )
            ", RegexOptions.IgnorePatternWhitespace);

            var prevSource = "";
            var firstLine = true;
            var space = new Dictionary<Coordinate, NdimCommand>();
            dimensions = -1;

            while (!string.IsNullOrWhiteSpace(source))
            {
                int line() => prevSource.Count(ch => ch == '\n') + 1;
                Match m = commandRegex.Match(source);
                if (!m.Success)
                    throw new Exception($"Syntax error in line {prevSource.Count(ch => ch == '\n') + 1}");

                if (m.Groups["comment"].Success)
                {
                    // comments are okay
                }
                else if (m.Groups["dim"].Success)
                {
                    if (!firstLine)
                        throw new Exception($"Line {line()} contains a number-of-dimensions command, but that must be at the top of the program.");
                    if (!int.TryParse(m.Groups["dim"].Value, out dimensions) || dimensions < 1)
                        throw new Exception($"Line {line()} contains a number-of-dimensions command that specifies an invalid number of dimensions (must be between 1 and 2147483647.");
                }
                else if (m.Groups["cmd"].Success)
                {
                    if (firstLine)
                        throw new Exception($"Line {line()} contains a command before the number-of-dimensions command.");

                    var coordVector = m.Groups["coord"].Value.Split(',').Select(x => int.Parse(x.Trim())).ToArray();
                    if (coordVector.Length != dimensions)
                        throw new Exception($"Line {line()} vector is not {dimensions} dimensional");
                    var coord = new Coordinate(coordVector);

                    NdimCommand cmd;
                    var pos = new Position(prevSource.Length + m.Groups["cmd"].Index, m.Groups["cmd"].Length);
                    if (m.Groups["chp"].Success)
                    {
                        if (!int.TryParse(m.Groups["chp"].Value, out int dir) && dir != 0 && Math.Abs(dir) <= dimensions)
                            throw new Exception($"Line {line()}: invalid pointer direction");
                        cmd = new ChangePointerDirectionCommand(dir, pos);
                    }
                    else if (m.Groups["noparam"].Success)
                    {
                        // \?|duplicate|jump|pop|swap|\+|-|\*|/|\^|<|>|&|\||!|input|assign|assignHere|toggleEat|printChar|print|end
                        switch (m.Groups["noparam"].Value)
                        {
                            case "?": cmd = new ChangePointerDirectionRandomCommand(pos); break;
                            case "duplicate": cmd = new DuplicateCommand(pos); break;
                            case "jump": cmd = new JumpCommand(pos); break;
                            case "pop": cmd = new PopCommand(pos); break;
                            case "swap": cmd = new SwapCommand(pos); break;
                            case "+": cmd = new PlusCommand(pos); break;
                            case "-": cmd = new MinusCommand(pos); break;
                            case "*": cmd = new MultiplyCommand(pos); break;
                            case "/": cmd = new DivideCommand(pos); break;
                            case "^": cmd = new PowerCommand(pos); break;
                            case "<": cmd = new LessThanCommand(pos); break;
                            case ">": cmd = new GreaterThanCommand(pos); break;
                            case "&": cmd = new AndCommand(pos); break;
                            case "|": cmd = new OrCommand(pos); break;
                            case "!": cmd = new NotCommand(pos); break;
                            case "input": cmd = new InputCommand(pos); break;
                            case "assign": cmd = new AssignCommand(pos); break;
                            case "assignHere": cmd = new AssignHereCommand(pos); break;
                            case "toggleEat": cmd = new ToggleEatCommand(pos); break;
                            case "printChar": cmd = new PrintCharCommand(pos); break;
                            case "print": cmd = new PrintCommand(pos); break;
                            case "end": cmd = new EndCommand(pos); break;
                            default: throw new Exception("Internal error.");
                        }
                    }
                    else if (m.Groups["push"].Success)
                    {
                        if (!int.TryParse(m.Groups["push"].Value, out var pushNum))
                            throw new Exception($"Line {line()}: invalid number to push.");
                        cmd = new PushCommand(pushNum, pos);
                    }
                    else if (m.Groups["if"].Success)
                    {
                        if (!int.TryParse(m.Groups["if"].Value, out var ifNum))
                            throw new Exception($"Line {line()}: invalid if number.");
                        cmd = new IfCommand(ifNum, pos);
                    }
                    else
                        throw new Exception("Internal error.");

                    if (space.ContainsKey(coord))
                        throw new Exception($"Line {line()}: re-use of duplicate coordinate: <{coord}>.");
                    space.Add(coord, cmd);
                }

                firstLine = false;
                prevSource += source.Substring(0, m.Index + m.Length);
                source = source.Substring(m.Index + m.Length);
            }
            return space;
        }

        public void RegisterCommandOrValue(Coordinate coord, NdimCommand command)
        {
            _space[coord] = command;
        }

        protected override IEnumerable<Position> GetProgram()
        {
            KeyValuePair<Coordinate, NdimCommand>[] candidateCmds = null;
            int ix = -1;

            void refreshCandidates()
            {
                var axis = Math.Abs(Pointer.Direction) - 1;
                candidateCmds = _space.Where(kvp => kvp.Key.Vector.All((c, ax) => ax == axis || c == Pointer.Position.Vector[ax])).OrderBy(kvp => kvp.Key.Vector[axis]).ToArray();
                ix = Pointer.Direction > 0
                    ? candidateCmds.IndexOf(kvp => kvp.Key.Vector[axis] >= Pointer.Position.Vector[axis])
                    : candidateCmds.LastIndexOf(kvp => kvp.Key.Vector[axis] <= Pointer.Position.Vector[axis]);
                if (ix == -1)
                    ix = Pointer.Direction > 0 ? 0 : candidateCmds.Length - 1;
            }
            refreshCandidates();

            while (!Ended)
            {
                Pointer.SetPosition(candidateCmds[ix].Key);
                yield return candidateCmds[ix].Value.Position ?? _posEnd;
                if (JumpPosition == null || JumpPosition.Value != candidateCmds[ix].Key)
                {
                    if (candidateCmds[ix].Value.Execute(this))
                        refreshCandidates();
                }
                else
                    JumpPosition = null;
                ix = Pointer.Direction > 0 ? (ix + 1) % candidateCmds.Length : (ix + candidateCmds.Length - 1) % candidateCmds.Length;
            }
        }

        public void Print(string value)
        {
            _output.Append(value);
        }

        public override void UpdateWatch()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Stack: {Stack}");
            sb.AppendLine($"Position: {Pointer.Position}");
            sb.AppendLine($"Direction: {Pointer.Direction}");

            for (var d1 = 0; d1 < _dimensions; d1++)
                for (var d2 = d1 + 1; d2 < _dimensions; d2++)
                {
                    var blah = _space
                        .Where(kvp => kvp.Key.Vector.All((c, ax) => ax == d1 || ax == d2 || c == Pointer.Position.Vector[ax]))
                        .ToDictionary(kvp => kvp.Key, kvp => Pointer.Position == kvp.Key ? $"<{kvp.Value}>" : kvp.Value.ToString());
                    var minX = blah.Min(kvp => kvp.Key.Vector[d1]);
                    var maxX = blah.Max(kvp => kvp.Key.Vector[d1]);
                    var minY = blah.Min(kvp => kvp.Key.Vector[d2]);
                    var maxY = blah.Max(kvp => kvp.Key.Vector[d2]);
                    var widths = Enumerable.Range(minX, maxX - minX + 1).Select(x => blah.Where(kvp => kvp.Key.Vector[d1] == x).DefaultIfEmpty().Max(kvp => (kvp.Value ?? " ").Length)).ToArray();
                    for (var y = minY; y <= maxY; y++)
                        sb.AppendLine(Enumerable.Range(minX, maxX - minX + 1).Select(x => (blah.TryGetValue(Pointer.Position.WithValue(d1, x).WithValue(d2, y), out var str) ? str : " ").PadRight(widths[x - minX], ' ')).JoinString("|"));
                    sb.AppendLine();
                }
            _txtWatch.Text = sb.ToString();
        }

        public int GetInput()
        {
            if (_input.Count == 0)
                throw new Exception("Input is exhausted.");
            return _input.Dequeue();
        }
    }
}
