namespace EsotericIDE.Runic.Runes
{
    public class RuneReadString : IExecutableRune
    {
        public bool Execute(Pointer pointer, IRunicContext context)
        {
            pointer.SetReadType(Pointer.ReadType.READ_STR);
            return true;
        }

        public IExecutableRune Register()
        {
            RuneRegistry.ALL_RUNES.Add('\"', this);
            return this;
        }
    }
}