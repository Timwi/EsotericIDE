using EsotericIDE.Runic.Math;

namespace EsotericIDE.Runic.Runes
{
    public class RuneEntrySimple : IExecutableRune
    {
        protected Direction dir;
        protected char c;

        public RuneEntrySimple(Direction d, char c)
        {
            dir = d;
            this.c = c;
        }

        public bool Execute(Pointer pointer, IRunicContext context)
        {
            if (pointer.GetAge() == 0)
            {
                pointer.direction = dir;
                pointer.Merge(new Pointer(10, pointer.direction, Vector2Int.zero));
            }
            return true;
        }

        public IExecutableRune Register()
        {
            RuneRegistry.ALL_RUNES.Add(c, this);
            return this;
        }
    }
}