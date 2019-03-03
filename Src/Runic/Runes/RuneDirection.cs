namespace EsotericIDE.Runic.Runes
{
    public class RuneDirection : IExecutableRune
    {
        Direction dir;
        char c;
        public RuneDirection(Direction dir, char c)
        {
            this.dir = dir;
            this.c = c;
        }
        public bool Execute(Pointer pointer, IRunicContext context)
        {
            pointer.direction = dir;
            return true;
        }

        public IExecutableRune Register()
        {
            RuneRegistry.ALL_RUNES.Add(c, this);
            return this;
        }
    }
}