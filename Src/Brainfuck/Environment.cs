using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using RT.Util.Controls;
using RT.Util.ExtensionMethods;

namespace EsotericIDE.Brainfuck
{
    abstract class BrainfuckEnv : ExecutionEnvironment
    {
        private Queue<BigInteger> _input;
        public BrainfuckEnv(Queue<BigInteger> input) { _input = input; }

        public abstract void MoveLeft();
        public abstract void MoveRight();
        public abstract void BfOutput();
        public abstract void Inc();
        public abstract void Dec();
        public abstract bool IsNonZero { get; }
        protected abstract void input(BigInteger input);
        public void BfInput() { input(_input.Count == 0 ? 0 : _input.Dequeue()); }

        public static BrainfuckEnv GetEnvironment(CellType cellType, string source, Queue<BigInteger> input, IOType outputType)
        {
            switch (cellType)
            {
                case CellType.Bytes: return new EnvBytes(source, input, outputType);
                case CellType.Int32s: return new EnvInt32(source, input, outputType);
                case CellType.UInt32s: return new EnvUInt32(source, input, outputType);
                case CellType.BigInts: return new EnvBigInt(source, input, outputType);
                default: throw new InvalidOperationException("Unrecognized cell type.");
            }
        }

        abstract class Env<TCell> : BrainfuckEnv
        {
            private IOType _outputType;
            private bool _everOutput;
            private Program _program;

            protected int _pointer;
            protected TCell[] _cells = new TCell[4];

            public Env(string source, Queue<BigInteger> input, IOType outputType)
                : base(input)
            {
                _outputType = outputType;
                _everOutput = false;
                _pointer = 0;
                _program = Program.Parse(source);
            }

            public sealed override void MoveLeft()
            {
                _pointer--;
                if (_pointer < 0)
                {
                    var nc = new TCell[_cells.Length * 2];
                    Array.Copy(_cells, 0, nc, _cells.Length, _cells.Length);
                    _pointer += _cells.Length;
                    _cells = nc;
                }
            }

            public sealed override void MoveRight()
            {
                _pointer++;
                if (_pointer == _cells.Length)
                {
                    var nc = new TCell[_cells.Length * 2];
                    Array.Copy(_cells, nc, _cells.Length);
                    _cells = nc;
                }
            }

            public sealed override void BfOutput()
            {
                switch (_outputType)
                {
                    case IOType.Numbers:
                        if (_everOutput)
                            _output.Append(", ");
                        _everOutput = true;
                        _output.Append(_cells[_pointer]);
                        break;

                    case IOType.Characters:
                        outputCharacter();
                        break;

                    default:
                        throw new InvalidOperationException();
                }
            }

            protected abstract void outputCharacter();

            protected override IEnumerable<Position> GetProgram() { return _program.Execute(this); }

            private ScrollableControl _scroll;
            private Panel _pnlMemory;
            private Bitmap _lastMemoryBitmap;
            private FontSpec _watchFont;

            public override Control InitializeWatchWindow()
            {
                _pnlMemory = new DoubleBufferedPanel();
                _pnlMemory.Paint += paintMemoryPanel;
                _scroll = new ScrollableControl { Dock = DockStyle.Fill, AutoScroll = true };
                _scroll.Controls.Add(_pnlMemory);
                return _scroll;
            }

            public override void SetWatchWindowFont(FontSpec font)
            {
                _watchFont = font;
                UpdateWatch();
            }

            public override void UpdateWatch()
            {
                const int xPadding = 10;
                const int yPadding = 5;
                const int bottomPadding = 15;

                if (_lastMemoryBitmap == null)
                    _lastMemoryBitmap = new Bitmap(10, 10, PixelFormat.Format32bppArgb);

                var totalWidth = 0;
                var totalHeight = 2 * yPadding + bottomPadding;
                var font = _watchFont == null ? _pnlMemory.Font : _watchFont.Font;

                using (var g = Graphics.FromImage(_lastMemoryBitmap))
                {
                    for (int i = 0; i < _cells.Length; i++)
                        totalWidth += 2 * xPadding + (int) g.MeasureString(_cells[i].ToString(), font).Width;
                    totalHeight += (int) g.MeasureString("Wg", font).Height;
                }

                _lastMemoryBitmap = new Bitmap(totalWidth, totalHeight);
                using (var g = Graphics.FromImage(_lastMemoryBitmap))
                {
                    g.TextRenderingHint = TextRenderingHint.AntiAlias;
                    g.SmoothingMode = SmoothingMode.AntiAlias;

                    g.DrawRectangle(Pens.LightGray, 0, 0, totalWidth - 1, totalHeight - bottomPadding);

                    int x = 0;
                    for (int i = 0; i < _cells.Length; i++)
                    {
                        var str = _cells[i].ToString();
                        g.DrawString(str, font, Brushes.Black, x + xPadding, yPadding);
                        var thisWidth = 2 * xPadding + g.MeasureString(_cells[i].ToString(), font).Width;

                        if (_pointer == i)
                            g.FillPolygon(Brushes.Crimson, new[] { new PointF(x + thisWidth / 2, totalHeight - bottomPadding), new PointF(x + thisWidth / 2 - xPadding, totalHeight - 1), new PointF(x + thisWidth / 2 + xPadding, totalHeight - 1) });

                        x += (int) thisWidth;
                        g.DrawLine(Pens.LightGray, x, 0, x, totalHeight - bottomPadding);
                    }
                }

                _pnlMemory.Size = _lastMemoryBitmap.Size;
                _pnlMemory.Refresh();
            }

            private void paintMemoryPanel(object _, PaintEventArgs e)
            {
                if (_lastMemoryBitmap != null)
                    e.Graphics.DrawImage(_lastMemoryBitmap, 0, 0);
            }
        }

        sealed class EnvBytes : Env<byte>
        {
            public EnvBytes(string source, Queue<BigInteger> input, IOType ot) : base(source, input, ot) { }
            protected override void input(BigInteger input) { _cells[_pointer] = (byte) (input & byte.MaxValue); }
            public override void Inc() { unchecked { _cells[_pointer]++; } }
            public override void Dec() { unchecked { _cells[_pointer]--; } }
            public override bool IsNonZero { get { return _cells[_pointer] != 0; } }
            protected override void outputCharacter() { _output.Append(char.ConvertFromUtf32(_cells[_pointer])); }
        }

        sealed class EnvUInt32 : Env<uint>
        {
            public EnvUInt32(string source, Queue<BigInteger> input, IOType ot) : base(source, input, ot) { }
            protected override void input(BigInteger input) { _cells[_pointer] = (uint) (input & uint.MaxValue); }
            public override void Inc() { unchecked { _cells[_pointer]++; } }
            public override void Dec() { unchecked { _cells[_pointer]--; } }
            public override bool IsNonZero { get { return _cells[_pointer] != 0; } }
            protected override void outputCharacter() { _output.Append(char.ConvertFromUtf32((int) _cells[_pointer])); }
        }

        sealed class EnvInt32 : Env<int>
        {
            public EnvInt32(string source, Queue<BigInteger> input, IOType ot) : base(source, input, ot) { }
            protected override void input(BigInteger input) { _cells[_pointer] = (int) (((input - int.MinValue) & uint.MaxValue) + int.MinValue); }
            public override void Inc() { unchecked { _cells[_pointer]++; } }
            public override void Dec() { unchecked { _cells[_pointer]--; } }
            public override bool IsNonZero { get { return _cells[_pointer] != 0; } }
            protected override void outputCharacter() { _output.Append(char.ConvertFromUtf32(_cells[_pointer])); }
        }

        sealed class EnvBigInt : Env<BigInteger>
        {
            public EnvBigInt(string source, Queue<BigInteger> input, IOType ot) : base(source, input, ot) { }
            protected override void input(BigInteger input) { _cells[_pointer] = input; }
            public override void Inc() { _cells[_pointer]++; }
            public override void Dec() { _cells[_pointer]--; }
            public override bool IsNonZero { get { return !_cells[_pointer].IsZero; } }
            protected override void outputCharacter() { _output.Append(char.ConvertFromUtf32((int) _cells[_pointer])); }
        }
    }
}