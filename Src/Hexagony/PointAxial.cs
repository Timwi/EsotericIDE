using System;
using RT.Util.ExtensionMethods;

namespace EsotericIDE.Hexagony
{
    struct PointAxial : IEquatable<PointAxial>
    {
        public int Q { get; private set; }
        public int R { get; private set; }
        public PointAxial(int q, int r) : this() { Q = q; R = r; }

        public static PointAxial operator +(PointAxial a, PointAxial b)
        {
            return new PointAxial(a.Q + b.Q, a.R + b.R);
        }

        public static PointAxial operator -(PointAxial a, PointAxial b)
        {
            return new PointAxial(a.Q - b.Q, a.R - b.R);
        }

        public override string ToString()
        {
            return "({0}, {1})".Fmt(Q, R);
        }

        public static bool operator ==(PointAxial a, PointAxial b)
        {
            return a.Q == b.Q && a.R == b.R;
        }

        public static bool operator !=(PointAxial a, PointAxial b)
        {
            return a.Q != b.Q || a.R != b.R;
        }

        public override int GetHashCode()
        {
            return unchecked(Q * 24567 + R * 47);
        }

        public override bool Equals(object obj)
        {
            return obj is PointAxial && this == (PointAxial) obj;
        }

        public bool Equals(PointAxial other)
        {
            return this == other;
        }
    }
}