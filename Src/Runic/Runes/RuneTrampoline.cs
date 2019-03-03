namespace EsotericIDE.Runic.Runes
{
    public class RuneTrampoline : IExecutableRune
    {
        public bool Execute(Pointer pointer, IRunicContext context)
        {
            pointer.SetSkip(1);
            return true;
        }

        public IExecutableRune Register()
        {
            RuneRegistry.ALL_RUNES.Add('!', this);
            return this;
        }
    }
}