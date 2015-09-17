using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace EsotericIDE.Hexagony
{
    sealed class HexagonyEnv : ExecutionEnvironment
    {
        private Memory _memory;
        private Grid _grid;
        private PointAxial[] _ips;
        private Direction[] _ipDirs;
        private int _activeIp = 0;
        private string _input;
        private int _inputIndex = 0;

        public HexagonyEnv(string source, string input)
        {
            _grid = Grid.Parse(source);
            _memory = new Memory();
            _input = input;
            _inputIndex = 0;
            _ips = Ut.NewArray(
                new PointAxial(0, -_grid.Size + 1),
                new PointAxial(_grid.Size - 1, -_grid.Size + 1),
                new PointAxial(_grid.Size - 1, 0),
                new PointAxial(0, _grid.Size - 1),
                new PointAxial(-_grid.Size + 1, _grid.Size - 1),
                new PointAxial(-_grid.Size + 1, 0));
            _ipDirs = Ut.NewArray(
                Direction.East,
                Direction.SouthEast,
                Direction.SouthWest,
                Direction.West,
                Direction.NorthWest,
                Direction.NorthEast);
        }

        public override string DescribeExecutionState()
        {
            return _ips.Select((pos, i) => "IP #{0}: {1} ({2}){3}{4}".Fmt(i, pos, _ipDirs[i], _activeIp == i ? " (active)" : null, Environment.NewLine)).JoinString() + Environment.NewLine + _memory.Describe;
        }

        private Direction dir { get { return _ipDirs[_activeIp]; } }
        private PointAxial coords { get { return _ips[_activeIp]; } }

        protected override IEnumerable<Position> GetProgram()
        {
            if (_grid.Size == 0)
                yield break;

            var integerFinder = new Regex(@"-?\d+");

            while (true)
            {
                yield return _grid.GetPosition(coords);

                // Execute the current instruction
                var newIp = _activeIp;
                var opcode = _grid[coords];
                switch (opcode)
                {
                    // NOP
                    case '.': break;

                    // Terminate
                    case '@': yield break;

                    // Arithmetic
                    case ')': _memory.Set(_memory.Get() + 1); break;
                    case '(': _memory.Set(_memory.Get() - 1); break;
                    case '+': _memory.Set(_memory.GetLeft() + _memory.GetRight()); break;
                    case '-': _memory.Set(_memory.GetLeft() - _memory.GetRight()); break;
                    case '*': _memory.Set(_memory.GetLeft() * _memory.GetRight()); break;
                    case '~': _memory.Set(-_memory.Get()); break;

                    case ':':
                    case '%':
                        var leftVal = _memory.GetLeft();
                        var rightVal = _memory.GetRight();
                        BigInteger rem;
                        var div = BigInteger.DivRem(leftVal, rightVal, out rem);
                        // The semantics of integer division and modulo are different in Hexagony because the 
                        // reference interpreter was written in Ruby. Account for this discrepancy.
                        if (rem != 0 && (leftVal < 0 ^ rightVal < 0))
                        {
                            rem += rightVal;
                            div--;
                        }
                        _memory.Set(opcode == ':' ? div : rem);
                        break;

                    // Memory manipulation
                    case '{': _memory.MoveLeft(); break;
                    case '}': _memory.MoveRight(); break;
                    case '=': _memory.Reverse(); break;
                    case '"': _memory.Reverse(); _memory.MoveRight(); _memory.Reverse(); break;
                    case '\'': _memory.Reverse(); _memory.MoveLeft(); _memory.Reverse(); break;
                    case '^':
                        if (_memory.Get() > 0)
                            _memory.MoveRight();
                        else
                            _memory.MoveLeft();
                        break;
                    case '&':
                        if (_memory.Get() > 0)
                            _memory.Set(_memory.GetRight());
                        else
                            _memory.Set(_memory.GetLeft());
                        break;

                    // I/O
                    case ',':
                        if (_inputIndex >= _input.Length)
                            _memory.Set(BigInteger.MinusOne);
                        else
                        {
                            _memory.Set(char.ConvertToUtf32(_input, _inputIndex));
                            _inputIndex += char.IsSurrogate(_input, _inputIndex) ? 2 : 1;
                        }
                        break;

                    case ';':
                        lock (_output)
                            _output.Append(char.ConvertFromUtf32((int) _memory.Get()));
                        break;

                    case '?':
                        var match = integerFinder.Match(_input, _inputIndex);
                        _memory.Set(match.Success ? BigInteger.Parse(match.Value) : BigInteger.Zero);
                        _inputIndex = match.Index + match.Length;
                        break;

                    case '!':
                        lock (_output)
                            _output.Append(_memory.Get().ToString());
                        break;

                    // Control flow
                    case '_': _ipDirs[_activeIp] = dir.ReflectAtUnderscore; break;
                    case '|': _ipDirs[_activeIp] = dir.ReflectAtPipe; break;
                    case '/': _ipDirs[_activeIp] = dir.ReflectAtSlash; break;
                    case '\\': _ipDirs[_activeIp] = dir.ReflectAtBackslash; break;
                    case '<': _ipDirs[_activeIp] = dir.ReflectAtLessThan(_memory.Get() > 0); break;
                    case '>': _ipDirs[_activeIp] = dir.ReflectAtGreaterThan(_memory.Get() > 0); break;
                    case ']': newIp = (_activeIp + 1) % 6; break;
                    case '[': newIp = (_activeIp + 5) % 6; break;
                    case '#': newIp = ((int) (_memory.Get() % 6) + 6) % 6; break;
                    case '$': _ips[_activeIp] += dir.Vector; break;

                    // Digits and letters
                    default:
                        if (opcode >= '0' && opcode <= '9')
                        {
                            var opVal = opcode - '0';
                            var memVal = _memory.Get();
                            _memory.Set(memVal * 10 + (memVal < 0 ? -opVal : opVal));
                        }
                        else if ((opcode >= 'a' && opcode <= 'z') || (opcode >= 'A' && opcode <= 'Z'))
                            _memory.Set(opcode);
                        else
                            throw new Exception("'{0}' is not a recognized instruction.".Fmt(opcode));
                        break;
                }

                _ips[_activeIp] += dir.Vector;
                handleEdges();
                _activeIp = newIp;
            }
        }

        private void handleEdges()
        {
            if (_grid.Size == 1)
            {
                _ips[_activeIp] = new PointAxial(0, 0);
                return;
            }

            var x = coords.Q;
            var z = coords.R;
            var y = -x - z;

            if (Ut.Max(Math.Abs(x), Math.Abs(y), Math.Abs(z)) < _grid.Size)
                return;

            var xBigger = Math.Abs(x) >= _grid.Size;
            var yBigger = Math.Abs(y) >= _grid.Size;
            var zBigger = Math.Abs(z) >= _grid.Size;

            // Move the pointer back to the hex near the edge
            _ips[_activeIp] -= dir.Vector;

            // If two values are still in range, we are wrapping around an edge (not a corner).
            if (!xBigger && !yBigger)
                _ips[_activeIp] = new PointAxial(coords.Q + coords.R, -coords.R);
            else if (!yBigger && !zBigger)
                _ips[_activeIp] = new PointAxial(-coords.Q, coords.Q + coords.R);
            else if (!zBigger && !xBigger)
                _ips[_activeIp] = new PointAxial(-coords.R, -coords.Q);
            else
            {
                // If two values are out of range, we navigated into a corner.
                // We teleport to a location that depends on the current memory value.
                var isPositive = _memory.Get() > 0;

                if ((!xBigger && !isPositive) || (!yBigger && isPositive))
                    _ips[_activeIp] = new PointAxial(coords.Q + coords.R, -coords.R);
                else if ((!yBigger && !isPositive) || (!zBigger && isPositive))
                    _ips[_activeIp] = new PointAxial(-coords.Q, coords.Q + coords.R);
                else if ((!zBigger && !isPositive) || (!xBigger && isPositive))
                    _ips[_activeIp] = new PointAxial(-coords.R, -coords.Q);
            }
        }
    }
}
