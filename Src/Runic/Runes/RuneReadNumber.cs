namespace EsotericIDE.Runic.Runes
{
    public class RuneReadNumber : IExecutableRune
    {
        char c;
        public RuneReadNumber(char c)
        {
            this.c = c;
        }
        public bool Execute(Pointer pointer, IRunicContext context)
        {
            pointer.SetReadType(Pointer.ReadType.READ_NUM);
            return true;
        }

        public IExecutableRune Register()
        {
            RuneRegistry.ALL_RUNES.Add(c, this);
            return this;
        }
    }
}