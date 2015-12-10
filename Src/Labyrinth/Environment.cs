using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace EsotericIDE.Labyrinth
{
    sealed class LabyrinthEnv : ExecutionEnvironment
    {
        private byte[] _input;
        private int _inputIndex;
        private InputMode _inputMode;
        private char[][] _source;
        private int _curX, _curY;
        private Direction _dir = Direction.Right;
        private Stack<BigInteger> _mainStack = new Stack<BigInteger>();
        private Stack<BigInteger> _auxStack = new Stack<BigInteger>();

        private static string _knownInstructions = "_0123456789)(+-*/%`&|$~:;}{=#,?.!\\<^>v\"'@";

        public LabyrinthEnv(string source, string input, InputMode inputMode)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (input == null)
                throw new ArgumentNullException("input");

            var lines = Regex.Split(source, "\r?\n");
            var longestLine = lines.Max(l => l.Length);
            _source = Ut.NewArray(lines.Length, longestLine, (y, x) => x < lines[y].Length ? lines[y][x] : ' ');

            switch (_inputMode = inputMode)
            {
                case InputMode.Utf8:
                    _input = input.ToUtf8();
                    break;

                case InputMode.Utf16:
                    _input = input.ToUtf16();
                    break;

                case InputMode.Numbers:
                    var m = Regex.Match(input, @"\A(\s*\d+\s*)*\Z", RegexOptions.Singleline);
                    if (!m.Success)
                        throw new InvalidOperationException("The input provided is not a whitespace-separated list of numbers.");
                    _input = m.Groups[1].Captures.Cast<Capture>().Select(c =>
                    {
                        byte b;
                        if (!byte.TryParse(c.Value.Trim(), out b))
                            throw new InvalidOperationException("The number {0} in the input string does not fit in a byte.".Fmt(c.Value.Trim()));
                        return b;
                    }).ToArray();
                    break;

                default:
                    throw new InvalidOperationException("Please choose an input mode from the “Input semantics” menu.");
            }

            _inputIndex = 0;

            for (int y = 0; y < _source.Length; y++)
                for (int x = 0; x < _source[y].Length; x++)
                    if (_knownInstructions.Contains(_source[y][x]))
                    {
                        _curX = x;
                        _curY = y;
                        return;
                    }
        }

        private Direction doTurn(Turn turn) { return (Direction) (((int) _dir + (int) turn) % 4); }

        private bool isPathAt(int x, int y)
        {
            if (x >= 0 && y >= 0 && x < _source[0].Length && y < _source.Length)
                return _knownInstructions.Contains(_source[y][x]);
            return false;
        }

        private bool isPathInDir(Direction dir)
        {
            return isPathAt(
                _curX + (dir == Direction.Left ? -1 : dir == Direction.Right ? 1 : 0),
                _curY + (dir == Direction.Up ? -1 : dir == Direction.Down ? 1 : 0));
        }

        private BigInteger pop() { return _mainStack.Count == 0 ? BigInteger.Zero : _mainStack.Pop(); }
        private void push(BigInteger val) { _mainStack.Push(val); }

        protected override IEnumerable<Position> GetProgram()
        {
            var neighbours = new List<Turn>(4);
            var integerFinder = new Regex(@"-?\d+");

            while (true)
            {
                yield return new Position(_curY * (_source[0].Length + Environment.NewLine.Length) + _curX, _curX == _source[0].Length ? 0 : 1);

                // Execute command
                var opcode = _source[_curY][_curX];
                BigInteger v;
                char c;
                int row, col;
                switch (opcode)
                {
                    case '"': break;
                    case '\'': break;
                    case '@': yield break;

                    // Arithmetic
                    case ')': push(pop() + 1); break;
                    case '(': push(pop() - 1); break;
                    case '+': push(pop() + pop()); break;
                    case '-': v = pop(); push(pop() - v); break;
                    case '*': push(pop() * pop()); break;
                    case '`': push(-pop()); break;
                    case '&': push(pop() & pop()); break;
                    case '|': push(pop() | pop()); break;
                    case '$': push(pop() ^ pop()); break;
                    case '~': push(~pop()); break;

                    case '/':
                    case '%':
                        v = pop();
                        BigInteger rem;
                        var div = Util.DivRemRubyStyle(pop(), v, out rem);
                        push(opcode == '%' ? rem : div);
                        break;

                    // Stack Manipulation
                    case ':': push(_mainStack.Peek()); break;
                    case ';': pop(); break;
                    case '}': _auxStack.Push(pop()); break;
                    case '{': push(_auxStack.Count == 0 ? BigInteger.Zero : _auxStack.Pop()); break;
                    case '=':
                        v = pop();
                        push(_auxStack.Count == 0 ? BigInteger.Zero : _auxStack.Pop());
                        _auxStack.Push(v);
                        break;
                    case '#': push(_mainStack.Count); break;

                    // I/O
                    case ',':
                        if (_inputIndex >= _input.Length)
                            push(BigInteger.MinusOne);
                        else
                        {
                            push(_input[_inputIndex]);
                            _inputIndex++;
                        }
                        break;

                    case '?':
                        push(findInteger());
                        break;

                    case '.': _output.Append(char.ConvertFromUtf32((int) pop())); break;
                    case '!': _output.Append(pop().ToString()); break;
                    case '\\': _output.AppendLine(); break;

                    // Grid Manipulation
                    case '<':
                        row = Util.ModuloRubyStyle((int) (_curY + pop()), _source.Length);
                        c = _source[row][0];
                        for (col = 0; col < _source[row].Length; col++)
                            _source[row][col] = col == _source[row].Length - 1 ? c : _source[row][col + 1];
                        if (row == _curY)
                            _curX = (_curX + _source[0].Length - 1) % _source[0].Length;
                        break;

                    case '>':
                        row = Util.ModuloRubyStyle((int) (_curY + pop()), _source.Length);
                        c = _source[row][_source[row].Length - 1];
                        for (col = _source[row].Length - 1; col >= 0; col--)
                            _source[row][col] = col == 0 ? c : _source[row][col - 1];
                        if (row == _curY)
                            _curX = (_curX + 1) % _source[0].Length;
                        break;

                    case '^':
                        col = Util.ModuloRubyStyle((int) (_curX + pop()), _source[0].Length);
                        c = _source[0][col];
                        for (row = 0; row < _source.Length; row++)
                            _source[row][col] = row == _source.Length - 1 ? c : _source[row + 1][col];
                        if (col == _curX)
                            _curY = (_curY + _source.Length - 1) % _source.Length;
                        break;

                    case 'v':
                        col = Util.ModuloRubyStyle((int) (_curX + pop()), _source[0].Length);
                        c = _source[_source.Length - 1][col];
                        for (row = _source.Length - 1; row >= 0; row--)
                            _source[row][col] = row == 0 ? c : _source[row - 1][col];
                        if (col == _curX)
                            _curY = (_curY + 1) % _source.Length;
                        break;

                    // Digits
                    case '_':
                        push(BigInteger.Zero);
                        break;

                    default:
                        if (opcode >= '0' && opcode <= '9')
                        {
                            push(pop() * 10 + opcode - '0');
                            break;
                        }
                        throw new Exception("Unrecognized instruction: {0}".Fmt(opcode));
                }

                // Determine new movement direction
                // The order of these checks is important for case 2 below
                neighbours.Clear();
                if (isPathInDir(doTurn(Turn.StraightAhead)))
                    neighbours.Add(Turn.StraightAhead);
                if (isPathInDir(doTurn(Turn.Left)))
                    neighbours.Add(Turn.Left);
                if (isPathInDir(doTurn(Turn.Right)))
                    neighbours.Add(Turn.Right);
                if (isPathInDir(doTurn(Turn.Reverse)))
                    neighbours.Add(Turn.Reverse);

                var peek = _mainStack.Count == 0 ? BigInteger.Zero : _mainStack.Peek();
                Turn turn;
                switch (neighbours.Count)
                {
                    case 4:
                        turn = peek < 0 ? Turn.Left : peek > 0 ? Turn.Right : Turn.StraightAhead;
                        break;

                    case 3:
                        turn = peek < 0 ? Turn.Left : peek > 0 ? Turn.Right : Turn.StraightAhead;
                        if (!neighbours.Contains(turn))
                            turn = (Turn) (((int) turn + 2) % 4);
                        break;

                    case 2:
                        turn = neighbours[0] == Turn.Left && neighbours[1] == Turn.Right
                            ? (peek < 0 ? Turn.Left : peek > 0 ? Turn.Right : Rnd.Next(2) == 0 ? Turn.Left : Turn.Right)
                            : neighbours[0];
                        break;

                    case 1:
                        turn = neighbours[0];
                        break;

                    default:
                        goto dontMove;
                }
                _dir = doTurn(turn);

                // Move forward
                switch (_dir)
                {
                    case Direction.Right: _curX++; break;
                    case Direction.Down: _curY++; break;
                    case Direction.Left: _curX--; break;
                    case Direction.Up: _curY--; break;
                }

                dontMove:;
            }
        }

        private BigInteger findInteger()
        {
            var chs = "0123456789+-".Select(c => (byte) c).ToArray();
            while (_inputIndex < _input.Length && !chs.Contains(_input[_inputIndex]))
                _inputIndex++;
            if (_inputIndex == _input.Length)
                return BigInteger.Zero;
            var sb = new StringBuilder();
            if (_input[_inputIndex] == '-' || _input[_inputIndex] == '+')
            {
                if (_input[_inputIndex] == '-')
                    sb.Append('-');
                _inputIndex++;
            }
            if (_inputIndex == _input.Length)
                return BigInteger.Zero;
            while (_inputIndex < _input.Length && _input[_inputIndex] >= '0' && _input[_inputIndex] <= '9')
            {
                sb.Append((char) _input[_inputIndex]);
                _inputIndex++;
            }
            return BigInteger.Parse(sb.ToString());
        }

        public override void UpdateWatch()
        {
            _txtWatch.Text = "Main: {1}{0}Aux: {2}{0}Position: ({3}, {4}) {5}{0}Input: {6}".Fmt(
                /* 0 */ Environment.NewLine,
                /* 1 */ _mainStack.JoinString(", "),
                /* 2 */ _auxStack.JoinString(", "),
                /* 3 */ _curX,
                /* 4 */ _curY,
                /* 5 */ _dir == Direction.Right ? "→" : _dir == Direction.Down ? "↓" : _dir == Direction.Left ? "←" : "↑",
                /* 6 */ _input.Skip(_inputIndex).Select(b => b < 32 || b >= 0x7f ? "<{0:X2}>".Fmt(b) : ((char) b).ToString()).JoinString()
            );
        }

        public override string ModifiedSource { get { return _source.Select(arr => new string(arr)).JoinString(Environment.NewLine); } }
    }
}
