namespace EsotericIDE.Runic.Runes
{
    public class RuneBlank : IExecutableRune
    {
        public bool Execute(Pointer pointer, IRunicContext context)
        {
            return true;
        }

        public IExecutableRune Register()
        {
            RuneRegistry.ALL_RUNES.Add(' ', this);
            return this;
        }
    }
}