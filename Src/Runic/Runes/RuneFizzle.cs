namespace EsotericIDE.Runic.Runes
{
    public class RuneFizzle : IExecutableRune
    {
        public bool Execute(Pointer pointer, IRunicContext context)
        {
            pointer.DeductMana(1);
            return true;
        }

        public IExecutableRune Register()
        {
            RuneRegistry.ALL_RUNES.Add('F', this);
            return this;
        }
    }
}