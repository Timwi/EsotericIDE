using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using RT.Util;
using RT.Util.Controls;
using RT.Util.ExtensionMethods;

namespace EsotericIDE.Unreadable
{
    sealed class UnreadableEnv : ExecutionEnvironment
    {
        private Program _program;
        private string _input;
        private Stack<BigInteger?> _stack;
        private Dictionary<BigInteger, BigInteger> _variables;

        public UnreadableEnv(string source, string input)
        {
            _program = Program.Parse(source);
            _input = input;
            _stack = new Stack<BigInteger?>();
            _variables = new Dictionary<BigInteger, BigInteger>();
        }

        protected override IEnumerable<Position> GetProgram()
        {
            return _program.Execute(this);
        }

        public BigInteger? Pop() { return _stack.Pop(); }
        public void Push(BigInteger? value) { _stack.Push(value); }

        private BigInteger popNotNull(string errorMessage)
        {
            var value = _stack.Pop();
            if (value == null)
                throw new Exception(errorMessage);
            return value.Value;
        }

        public void Print()
        {
            var value = popNotNull("Cannot print the result of a void operation.");
            _output.Append(char.ConvertFromUtf32((int) value));
            _stack.Push(value);
        }

        public void One() { _stack.Push(1); }
        public void Inc() { _stack.Push(popNotNull("Cannot increment a void value.") + 1); }
        public void Dec() { _stack.Push(popNotNull("Cannot decrement a void value.") - 1); }

        public void Sequence()
        {
            var value = _stack.Pop();
            _stack.Pop();
            _stack.Push(value);
        }

        public void Set()
        {
            var value = popNotNull("Cannot set a variable to a void value.");
            var variable = popNotNull("Void is not a valid variable identifier.");
            _variables[variable] = value;
            _stack.Push(value);
        }

        public void Get()
        {
            _stack.Push(_variables.Get(popNotNull("Void is not a valid variable identifier."), 0));
        }

        public void Read()
        {
            if (_input.Length == 0)
                _stack.Push(-1);
            else
            {
                _stack.Push(char.ConvertToUtf32(_input, 0));
                _input = _input.Substring(char.IsSurrogate(_input[0]) ? 2 : 1);
            }
        }

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

        private static string charStr(BigInteger value)
        {
            if (value < 0x20 || (value >= 0xD800 && value < 0xE000) || value > 0x10FFFF)
                return null;
            return $"'{char.ConvertFromUtf32((int) value)}'";
        }

        public override void UpdateWatch()
        {
            const int xPadding = 10;
            const int yPadding = 5;
            const int gapWidth = 10;

            if (_lastMemoryBitmap == null)
                _lastMemoryBitmap = new Bitmap(10, 10, PixelFormat.Format32bppArgb);

            // Find some sensible ranges of the memory to show
            const int margin = 5;
            var ranges = new List<Tuple<BigInteger, BigInteger>>();
            foreach (var key in _variables.Keys)
            {
                // Use binary search
                var low = 0;
                var high = ranges.Count - 1;
                var already = false;
                while (high >= low)
                {
                    var mid = (low + high) / 2;
                    var tup = ranges[mid];
                    if (tup.Item2 < key)
                        low = mid + 1;
                    else if (tup.Item1 > key)
                        high = mid - 1;
                    else
                    {
                        already = true;
                        break;
                    }
                }

                if (already)
                    continue;

                // Possibility #1: we’re between two ranges that are less than ‘margin’ away: merge them
                if (high != -1 && low != ranges.Count && (key <= ranges[high].Item2 + margin) && (key >= ranges[low].Item1 - margin))
                {
                    var newItem1 = ranges[high].Item1;
                    var newItem2 = ranges[low].Item2;
                    ranges.RemoveAt(low);
                    ranges[high] = Tuple.Create(newItem1, newItem2);
                }
                // Possibility #2: we’re less than ‘margin’ away from the left range (‘high’)
                else if (high != -1 && (key <= ranges[high].Item2 + margin))
                    ranges[high] = Tuple.Create(ranges[high].Item1, key);
                // Possibility #3: we’re less than ‘margin’ away from the right range (‘low’)
                else if (low != ranges.Count && (key >= ranges[low].Item1 - margin))
                    ranges[low] = Tuple.Create(key, ranges[low].Item2);
                // Possibility #4: we’re no-where near an existing range, so create a new one
                else
                    ranges.Insert(low, Tuple.Create(key, key));
            }

            if (ranges.Count == 0)
                ranges.Add(Tuple.Create(BigInteger.Zero, BigInteger.Zero));

            var font = _watchFont == null ? _pnlMemory.Font : _watchFont.Font;
            var stackText = $"Stack:\r\n{_stack.Select(val => val == null ? "<void>" : val.Value.ToString()).DefaultIfEmpty("<empty>").JoinString("\r\n")}";

            int[] cellWidth = new int[ranges.Sum(r => (int) (r.Item2 - r.Item1) + 1) + ranges.Count - 1];
            BigInteger[] indexes = new BigInteger[cellWidth.Length];
            BigInteger[] values = new BigInteger[cellWidth.Length];

            int[] nature = new int[cellWidth.Length];  // 0 = cell with value; 1 = cell without value; 2 = gap between ranges

            int totalCellWidth = 0;
            int totalCellHeight;
            int totalWidth;
            int totalHeight;
            int top;
            int stackTop;

            using (var g = Graphics.FromImage(_lastMemoryBitmap))
            {
                var stackMeasure = g.MeasureString(stackText, font);

                var lineHeight = (int) g.MeasureString("Wg", font).Height;
                totalCellHeight = 3 * yPadding + lineHeight * 3;
                totalHeight = totalCellHeight + (int) stackMeasure.Height;
                top = yPadding + lineHeight;

                var index = 0;
                foreach (var range in ranges)
                {
                    if (index != 0)
                    {
                        cellWidth[index] = gapWidth;
                        totalCellWidth += gapWidth;
                        nature[index] = 2;
                        index++;
                    }

                    for (var i = range.Item1; i <= range.Item2; i++)
                    {
                        BigInteger value;
                        int width;
                        if (!_variables.TryGetValue(i, out value))
                        {
                            nature[index] = 1;
                            width = (int) g.MeasureString(i.ToString(), font).Width + 2 * xPadding;
                        }
                        else
                        {
                            width = (int) Math.Max(
                                // Row 1: cell index
                                g.MeasureString(i.ToString(), font).Width, Math.Max(
                                // Row 2: cell value (integer)
                                g.MeasureString(value.ToString(), font).Width,
                                // Row 3: cell value (character)
                                charStr(value).NullOr(c => g.MeasureString(c, font).Width) ?? 0)) + 2 * xPadding;
                        }
                        cellWidth[index] = width;
                        indexes[index] = i;
                        values[index] = value;
                        totalCellWidth += width;
                        index++;
                    }
                }

                totalWidth = Math.Max(totalCellWidth, (int) stackMeasure.Width);
            }

            _lastMemoryBitmap = new Bitmap(totalWidth, totalHeight, PixelFormat.Format32bppArgb);

            using (var g = Graphics.FromImage(_lastMemoryBitmap))
            {
                g.TextRenderingHint = TextRenderingHint.AntiAlias;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.White);

                g.DrawString(stackText, font, Brushes.Black, 0, totalCellHeight);

                g.DrawRectangle(Pens.LightGray, 0, top, totalCellWidth - 1, totalCellHeight - top - 1);

                int x = 0;
                var sfTop = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Near };
                var sfBottom = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Far };

                for (int i = 0; i < cellWidth.Length; i++)
                {
                    if (nature[i] == 2)
                        g.FillRectangle(Brushes.Silver, x, top, cellWidth[i], totalCellHeight - top);

                    if (i != 0)
                        g.DrawLine(Pens.LightGray, x, top, x, totalCellHeight);

                    if (nature[i] != 2)
                    {
                        var midX = x + cellWidth[i] / 2;

                        // Row 1: cell index
                        g.DrawString(indexes[i].ToString(), font, Brushes.Black, midX, 0, sfTop);
                        if (nature[i] == 0)
                        {
                            // Row 2: cell value
                            g.DrawString(values[i].ToString(), font, Brushes.Black, midX, top + yPadding, sfTop);
                            // Row 3: cell value as character
                            var str2 = charStr(values[i]);
                            if (str2 != null)
                                g.DrawString(str2, font, Brushes.Black, midX, totalCellHeight - yPadding, sfBottom);
                        }
                    }

                    x += cellWidth[i];
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
}
