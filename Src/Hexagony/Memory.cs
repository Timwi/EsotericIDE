using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Numerics;
using RT.Util;
using RT.Util.Drawing;
using RT.Util.ExtensionMethods;
using RT.Util.Text;

namespace EsotericIDE.Hexagony
{
    sealed class Memory
    {
        private Dictionary<Direction, Dictionary<PointAxial, BigInteger>> _edges = new Dictionary<Direction, Dictionary<PointAxial, BigInteger>>();
        private Dictionary<Direction, Dictionary<PointAxial, string>> _annotations;
        private PointAxial _mp = new PointAxial(0, 0);
        private Direction _dir = Direction.East;
        private bool _cw = false;

        public Memory(Dictionary<Direction, Dictionary<PointAxial, string>> annotations)
        {
            if (annotations == null)
                throw new ArgumentNullException("annotations");
            _annotations = annotations;
        }

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

        public Bitmap DrawBitmap(HexagonySettings settings, Font defaultValueFont, Font defaultAnnotationFont)
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
            foreach (var kvp1 in _annotations)
                foreach (var kvp2 in kvp1.Value)
                {
                    var x = getX(kvp1.Key, kvp2.Key);
                    var y = getY(kvp1.Key, kvp2.Key);
                    minX = Math.Min(minX, x); minY = Math.Min(minY, y);
                    maxX = Math.Max(maxX, x); maxY = Math.Max(maxY, y);
                }
            minX -= 3; minY -= 3; maxX += 3; maxY += 3;

            const int xFactor = 20, yFactor = 34;

            return GraphicsUtil.DrawBitmap((maxX - minX) * xFactor, (maxY - minY) * yFactor, g =>
            {
                g.Clear(settings.MemoryBackgroundColor);

                using (var unusedEdgePen = new Pen(settings.MemoryGridZeroColor))
                using (var usedEdgePen = new Pen(settings.MemoryGridNonZeroColor, 2f))
                using (var pointerBrush = new SolidBrush(settings.MemoryPointerColor))
                using (var valueBrush = new SolidBrush(settings.MemoryValueFont.NullOr(f => f.Color) ?? Color.CornflowerBlue))
                using (var annotationBrush = new SolidBrush(settings.MemoryAnnotationFont.NullOr(f => f.Color) ?? Color.ForestGreen))
                using (var valueFont = settings.MemoryValueFont.NullOr(f => f.Font))
                using (var annotationFont = settings.MemoryAnnotationFont.NullOr(f => f.Font))
                {
                    var sfValue = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Near };
                    var sfAnnotation = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Far };

                    for (int y = minY; y <= maxY; y++)
                        for (int x = minX; x <= maxX; x++)
                        {
                            if (!((y % 2 == 0 && x % 2 == 0) ||
                                ((y % 4 + 4) % 4 == 1 && (x % 4 + 4) % 4 == 1) ||
                                ((y % 4 + 4) % 4 == 3 && (x % 4 + 4) % 4 == 3)))
                                continue;

                            var dir = Direction.East;
                            var mp = new PointAxial(0, 0);

                            if (y % 2 != 0)
                            {
                                dir = Direction.East;
                                mp = new PointAxial((x - y) / 4, (y - 1) / 2);
                            }
                            else if ((x - y) % 4 == 0)
                            {
                                dir = Direction.NorthEast;
                                mp = new PointAxial((x - y) / 4, y / 2);
                            }
                            else
                            {
                                dir = Direction.SouthEast;
                                mp = new PointAxial((x - y + 2) / 4, (y - 2) / 2);
                            }

                            var xx = (x - minX) * xFactor;
                            var yy = (y - minY) * yFactor;
                            var hasValue = _edges.ContainsKeys(dir, mp);

                            using (var tr = new GraphicsTransformer(g).Rotate((dir is NorthEast ? -60 : dir is SouthEast ? 60 : 0) + (_cw ? 180 : 0)).Translate(xx, yy))
                            {
                                g.DrawLine(hasValue ? usedEdgePen : unusedEdgePen, 0, yFactor * -.68f, 0, yFactor * .68f);
                                if (dir == _dir && mp == _mp)
                                    g.FillPolygon(pointerBrush,
                                        new[] { new PointF(0, yFactor * -.68f), new PointF(3, yFactor * .68f), new PointF(-3, yFactor * .68f) });
                            }
                            using (var tr = new GraphicsTransformer(g).Rotate((dir is NorthEast ? 30 : dir is SouthEast ? -30 : -90)).Translate(xx, yy))
                            {
                                if (hasValue)
                                {
                                    var str = _edges[dir][mp].ToString();
                                    // Show printable ISO-8859-1 characters
                                    if (_edges[dir][mp] >= 0x20 && _edges[dir][mp] <= 0xff && _edges[dir][mp] != 0x7f)
                                        try { str += " '" + char.ConvertFromUtf32((int) _edges[dir][mp]) + "'"; }
                                        catch { }
                                    g.DrawString(str, valueFont ?? defaultValueFont, valueBrush, 0, 0, sfValue);
                                }
                                var annotation = _annotations.Get(dir, mp, null);
                                if (!string.IsNullOrWhiteSpace(annotation))
                                    g.DrawString(annotation, annotationFont ?? defaultAnnotationFont, annotationBrush, 0, 2, sfAnnotation);
                            }
                        }
                }
            });
        }

        public void Annotate(string annotation)
        {
            if (string.IsNullOrWhiteSpace(annotation))
                _annotations.RemoveSafe(_dir, _mp);
            else
                _annotations.AddSafe(_dir, _mp, annotation);
        }

        public string GetCurrentAnnotation()
        {
            return _annotations.Get(_dir, _mp, "");
        }

        public void SetAnnotations(Dictionary<Direction, Dictionary<PointAxial, string>> annotations)
        {
            _annotations = annotations;
        }
    }
}
