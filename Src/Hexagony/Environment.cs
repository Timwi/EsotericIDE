using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace EsotericIDE.Hexagony
{
    sealed class HexagonyEnv : ExecutionEnvironment
    {
        private Memory _memory;
        private Grid _grid;
        private readonly string _linearCode;
        private readonly PointAxial[] _ips;
        private readonly Direction[] _ipDirs;
        private int _activeIp = 0;
        private byte[] _input;
        private int _inputIndex = 0;
        private readonly InputMode _inputMode;

        public HexagonyEnv(string source, string input, HexagonySettings settings)
        {
            if (source.StartsWith("❢"))
                _linearCode = source.Substring(1);
            else
            {
                _grid = Grid.Parse(source);
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
            switch (_inputMode = settings.InputMode)
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
            _settings = settings;
            if (!settings.MemoryAnnotations.ContainsKey(settings.LastMemoryAnnotationSet))
                settings.MemoryAnnotations[settings.LastMemoryAnnotationSet] = new Dictionary<Direction, Dictionary<PointAxial, string>>();
            _memory = new Memory(settings.MemoryAnnotations[settings.LastMemoryAnnotationSet]);
        }

        private TextBox _txtIpInfo;
        private ScrollableControl _scroll;
        private Panel _pnlMemory;
        private Bitmap _lastMemoryBitmap;
        private HexagonySettings _settings;

        public override Control InitializeWatchWindow()
        {
            _txtIpInfo = new TextBox { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, Width = 300 };
            _pnlMemory = new Panel();
            _pnlMemory.Paint += paintMemory;
            _scroll = new ScrollableControl { Dock = DockStyle.Fill, AutoScroll = true };
            _scroll.Controls.Add(_pnlMemory);

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill };
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 1));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 1));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            layout.Controls.Add(_scroll, 0, 0);
            layout.Controls.Add(_txtIpInfo, 1, 0);
            return layout;
        }

        private void paintMemory(object _, PaintEventArgs e)
        {
            if (_lastMemoryBitmap != null)
                e.Graphics.DrawImage(_lastMemoryBitmap, 0, 0);
        }

        public override void SetWatchWindowFont(FontSpec font)
        {
            if (_txtIpInfo != null)
            {
                _txtIpInfo.Font = font.Font;
                _txtIpInfo.ForeColor = font.Color;
            }
        }

        public override void UpdateWatch()
        {
            _txtIpInfo.Text = _ips.NullOr(ips => ips.Select((pos, i) => "IP #{0}: {1} ({2}){3}{4}".Fmt(i, pos, _ipDirs[i], _activeIp == i ? " (active)" : null, Environment.NewLine)).JoinString())
                + "Input: " + _input.Skip(_inputIndex).Select(b => b < 32 || b >= 0x7f ? "<{0:X2}>".Fmt(b) : ((char) b).ToString()).JoinString();
            _memory.SetAnnotations(_settings.MemoryAnnotations[_settings.LastMemoryAnnotationSet]);
            _lastMemoryBitmap = _memory.DrawBitmap(_settings, _pnlMemory.Font, _pnlMemory.Font);
            _pnlMemory.Size = _lastMemoryBitmap.Size;
            _pnlMemory.Refresh();
        }

        private Direction dir { get { return _ipDirs[_activeIp]; } }
        private PointAxial coords { get { return _ips[_activeIp]; } }

        protected override IEnumerable<Position> GetProgram()
        {
            var integerFinder = new Regex(@"-?\d+");

            if (_linearCode != null)
            {
                var program = Regex.Matches(_linearCode, @"[^\r\n]+").Cast<Match>()
                    .Select(m => new { Line = Regex.Replace(m.Value.TrimStart('✓'), @"#.*", "").TrimEnd(), Position = new Position(m.Index + 1, m.Length) })
                    .Where(inf => inf.Line.Trim().Length > 0)
                    .ToArray();
                var labels = program.Select(inf => inf.Line.Trim()).Where(l => l.StartsWith("goto ")).Select(l => l.Substring("goto ".Length)).ToHashSet();
                var index = 0;
                while (true)
                {
                    yield return program[index].Position;
                    if (labels.Contains(program[index].Line.Trim()))
                    {
                    }
                    else if (program[index].Line.Trim() == "if > 0")
                    {
                        if (_memory.Get() <= 0)
                        {
                            var indent = program[index].Line.Length - program[index].Line.Trim().Length;
                            do { index++; }
                            while (program[index].Line.Length - program[index].Line.Trim().Length > indent);
                            continue;
                        }
                    }
                    else if (program[index].Line.Trim().StartsWith("goto "))
                    {
                        var newIndex = program.IndexOf(l => l.Line.Trim() == program[index].Line.Trim().Substring("goto ".Length));
                        if (newIndex == -1)
                            throw new InvalidOperationException();
                        index = newIndex;
                    }
                    else
                    {
                        var line = program[index].Line.Trim();
                        for (int lineIndex = 0; lineIndex < line.Length; lineIndex++)
                        {
                            var opcode = line[lineIndex];
                            switch (opcode)
                            {
                                // Annotations
                                case '[':
                                    lineIndex++;
                                    var closePos = line.IndexOf(']', lineIndex);
                                    _memory.Annotate(line.Substring(lineIndex, closePos - lineIndex));
                                    lineIndex = closePos;
                                    break;

                                // NOP
                                case '.': break;

                                // Arithmetic
                                case ')': _memory.Set(_memory.Get() + 1); break;
                                case '(': _memory.Set(_memory.Get() - 1); break;
                                case '+': _memory.Set(_memory.GetLeft() + _memory.GetRight()); break;
                                case '-': _memory.Set(_memory.GetLeft() - _memory.GetRight()); break;
                                case '*': _memory.Set(_memory.GetLeft() * _memory.GetRight()); break;
                                case '~': _memory.Set(-_memory.Get()); break;
                                case ':': _memory.Set(Util.DivideRubyStyle(_memory.GetLeft(), _memory.GetRight())); break;
                                case '%': _memory.Set(Util.ModuloRubyStyle(_memory.GetLeft(), _memory.GetRight())); break;

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
                                        _memory.Set(_input[_inputIndex]);
                                        _inputIndex++;
                                    }
                                    break;

                                case ';':
                                    _output.Append((char) (_memory.Get() % 256));
                                    break;

                                case '?':
                                    _memory.Set(findInteger());
                                    break;

                                case '!':
                                    _output.Append(_memory.Get().ToString());
                                    break;

                                case '@':
                                    yield break;

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
                        }
                    }
                    index++;
                }
            }

            if (_grid.Size == 0)
                yield break;

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
                            _memory.Set(_input[_inputIndex]);
                            _inputIndex++;
                        }
                        break;

                    case ';':
                        _output.Append((char) (_memory.Get() % 256));
                        break;

                    case '?':
                        _memory.Set(findInteger());
                        break;

                    case '!':
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
                    case '$': _ips[_activeIp] += dir.Vector; handleEdges(); break;

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

        public string GetCurrentAnnotation() { return _memory.GetCurrentAnnotation(); }
        public void Annotate(string annotation) { _memory.Annotate(annotation); }

        public void SaveMemoryVisualization(string filename)
        {
            if (_lastMemoryBitmap == null)
                UpdateWatch();
            _lastMemoryBitmap.Save(filename);
        }
    }
}
