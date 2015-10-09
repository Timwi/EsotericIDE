using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace EsotericIDE.Hexagony
{
    class Grid
    {
        public int Size { get; private set; }
        private char[][] _grid;
        private Position[][] _gridPositions;
        private Position _endPos;

        private Grid(int size, int fileLength, IEnumerable<Tuple<char, Position>> data = null)
        {
            Size = size;
            using (var e = data == null ? null : data.GetEnumerator())
                _grid = Ut.NewArray(2 * size - 1, j => Ut.NewArray(2 * size - 1 - Math.Abs(size - 1 - j), i => e != null && e.MoveNext() ? e.Current.Item1 : '.'));
            _endPos = new Position(fileLength, 0);
            using (var e = data == null ? null : data.GetEnumerator())
                _gridPositions = Ut.NewArray(2 * size - 1, j => Ut.NewArray(2 * size - 1 - Math.Abs(size - 1 - j), i => e != null && e.MoveNext() ? e.Current.Item2 : _endPos));
        }

        public static Grid Parse(string input)
        {
            var bare = input
                .Select((c, i) => Tuple.Create(c, new Position(i, 1)))
                .Where(tup => tup.Item1 != '`' && !char.IsWhiteSpace(tup.Item1))
                .ToArray();
            var size = 1;
            while (3 * size * (size - 1) + 1 < bare.Length)
                size++;
            return new Grid(size, input.Length, bare);
        }

        public char this[PointAxial coords]
        {
            get
            {
                var tup = axial_to_index(coords);
                return tup == null ? '.' : _grid[tup.Item1][tup.Item2];
            }
            set
            {
                var tup = axial_to_index(coords);
                if (tup != null)
                    _grid[tup.Item1][tup.Item2] = value;
            }
        }

        private Tuple<int, int> axial_to_index(PointAxial coords)
        {
            var x = coords.Q;
            var z = coords.R;
            var y = -x - z;
            if (Ut.Max(Math.Abs(x), Math.Abs(y), Math.Abs(z)) >= Size)
                return null;

            var i = z + Size - 1;
            var j = x + Math.Min(i, Size - 1);
            return Tuple.Create(i, j);
        }

        public override string ToString()
        {
            return _grid.Select(line => new string(' ', 2 * Size - line.Length) + line.JoinString(" ")).JoinString(Environment.NewLine);
        }

        public Position GetPosition(PointAxial coords)
        {
            var tup = axial_to_index(coords);
            return tup == null ? _endPos : _gridPositions[tup.Item1][tup.Item2];
        }
    }
}
