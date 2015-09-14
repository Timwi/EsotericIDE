using RT.Util.ExtensionMethods;

namespace EsotericIDE.Hexagony
{
    struct PointAxial
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
    }
}