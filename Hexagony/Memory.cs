using System;
using System.Collections.Generic;
using System.Numerics;
using RT.Util.ExtensionMethods;

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
    }
}
