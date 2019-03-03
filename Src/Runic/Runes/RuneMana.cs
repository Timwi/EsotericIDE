namespace EsotericIDE.Runic.Runes
{
    public class RuneMana : IExecutableRune
    {
        public bool Execute(Pointer pointer, IRunicContext context)
        {
            pointer.Push(pointer.GetMana());
            return true;
        }

        public IExecutableRune Register()
        {
            RuneRegistry.ALL_RUNES.Add('m', this);
            return this;
        }
    }
}