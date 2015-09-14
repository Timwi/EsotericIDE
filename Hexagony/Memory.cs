using System;
using System.Collections.Generic;
using System.Numerics;
using RT.Util;
using RT.Util.ExtensionMethods;
using RT.Util.Text;

namespace EsotericIDE.Hexagony
{
    sealed class Memory
    {
        private Dictionary<Direction, Dictionary<PointAxial, BigInteger>> _edges = new Dictionary<Direction, Dictionary<PointAxial, BigInteger>>();
        private PointAxial _mp = new PointAxial(0, 0);
        private Direction _dir = Direction.East;
        private bool _cw = false;

        public void Reverse() { _cw = !_cw; }

        public void MoveLeft()
        {
            var index = leftIndex;
            _mp = index.Item1;
            _dir = index.Item2;
            _cw = index.Item3;
        }

        public void MoveRight()
        {
            var index = rightIndex;
            _mp = index.Item1;
            _dir = index.Item2;
            _cw = index.Item3;
        }

        public void Set(BigInteger value)
        {
            if (value.IsZero)
                _edges.RemoveSafe(_dir, _mp);
            else
                _edges.AddSafe(_dir, _mp, value);
        }

        public BigInteger Get()
        {
            Dictionary<PointAxial, BigInteger> inner;
            return _edges.TryGetValue(_dir, out inner) ? inner.Get(_mp, BigInteger.Zero) : BigInteger.Zero;
        }

        public BigInteger GetLeft()
        {
            var index = leftIndex;
            Dictionary<PointAxial, BigInteger> inner;
            return _edges.TryGetValue(index.Item2, out inner) ? inner.Get(index.Item1, BigInteger.Zero) : BigInteger.Zero;
        }

        public BigInteger GetRight()
        {
            var index = rightIndex;
            Dictionary<PointAxial, BigInteger> inner;
            return _edges.TryGetValue(index.Item2, out inner) ? inner.Get(index.Item1, BigInteger.Zero) : BigInteger.Zero;
        }

        private Tuple<PointAxial, Direction, bool> leftIndex
        {
            get
            {
                var mp = _mp;
                var dir = _dir;
                var cw = _cw;

                if (dir is NorthEast)
                {
                    mp = cw ? new PointAxial(mp.Q + 1, mp.R - 1) : new PointAxial(mp.Q, mp.R - 1);
                    dir = Direction.SouthEast;
                    cw = !cw;
                }
                else if (dir is East)
                {
                    mp = cw ? new PointAxial(mp.Q, mp.R + 1) : new PointAxial(mp.Q, mp.R);
                    dir = Direction.NorthEast;
                }
                else if (dir is SouthEast)
                {
                    mp = cw ? new PointAxial(mp.Q - 1, mp.R + 1) : new PointAxial(mp.Q, mp.R);
                    dir = Direction.East;
                }

                return Tuple.Create(mp, dir, cw);
            }
        }

        private Tuple<PointAxial, Direction, bool> rightIndex
        {
            get
            {
                var mp = _mp;
                var dir = _dir;
                var cw = _cw;

                if (dir is NorthEast)
                {
                    mp = cw ? new PointAxial(mp.Q, mp.R) : new PointAxial(mp.Q, mp.R - 1);
                    dir = Direction.East;
                }
                else if (dir is East)
                {
                    mp = cw ? new PointAxial(mp.Q, mp.R) : new PointAxial(mp.Q + 1, mp.R - 1);
                    dir = Direction.SouthEast;
                }
                else if (dir is SouthEast)
                {
                    mp = cw ? new PointAxial(mp.Q - 1, mp.R + 1) : new PointAxial(mp.Q, mp.R + 1);
                    dir = Direction.NorthEast;
                    cw = !cw;
                }

                return Tuple.Create(mp, dir, cw);
            }
        }

        public string Describe
        {
            get
            {
                var getX = Ut.Lambda((Direction dir, PointAxial coords) => 4 * coords.Q + 2 * coords.R + (dir is East ? 1 : 0));
                var getY = Ut.Lambda((Direction dir, PointAxial coords) => 2 * coords.R + (dir is NorthEast ? 0 : dir is East ? 1 : 2));

                int minX = getX(_dir, _mp), maxX = minX;
                int minY = getY(_dir, _mp), maxY = minY;
                foreach (var kvp1 in _edges)
                    foreach (var kvp2 in kvp1.Value)
                    {
                        var x = getX(kvp1.Key, kvp2.Key);
                        var y = getY(kvp1.Key, kvp2.Key);
                        minX = Math.Min(minX, x); minY = Math.Min(minY, y);
                        maxX = Math.Max(maxX, x); maxY = Math.Max(maxY, y);
                    }

                var arrow =
                    _dir is NorthEast ? (_cw ? "↘" : "↖") :
                    _dir is East ? (_cw ? "↓" : "↑") :
                    (_cw ? "↙" : "↗");

                var tt = new TextTable { ColumnSpacing = 1, RowSpacing = 1, HorizontalRules = true, VerticalRules = true };

                for (int y = minY; y <= maxY; y++)
                    for (int x = minX; x <= maxX; x++)
                        if ((y % 2 == 0 && x % 2 == 0) ||
                            ((y % 4 + 4) % 4 == 1 && (x % 4 + 4) % 4 == 1) ||
                            ((y % 4 + 4) % 4 == 3 && (x % 4 + 4) % 4 == 3))
                            tt.SetCell(x - minX, y - minY, "0", alignment: HorizontalTextAlignment.Center);

                var found = false;
                foreach (var kvp1 in _edges)
                    foreach (var kvp2 in kvp1.Value)
                    {
                        var x = getX(kvp1.Key, kvp2.Key);
                        var y = getY(kvp1.Key, kvp2.Key);
                        var str = kvp2.Value.ToString();
                        if (kvp1.Key == _dir && kvp2.Key == _mp)
                        {
                            str += arrow;
                            found = true;
                        }
                        tt.SetCell(x - minX, y - minY, str, alignment: HorizontalTextAlignment.Center);
                    }

                if (!found)
                    tt.SetCell(getX(_dir, _mp) - minX, getY(_dir, _mp) - minY, arrow, alignment: HorizontalTextAlignment.Center);

                return tt.ToString();
            }
        }
    }
}
