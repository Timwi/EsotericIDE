namespace EsotericIDE.Runic.Runes
{
    public class RuneReadCharContinuous : IExecutableRune
    {
        public bool Execute(Pointer pointer, IRunicContext context)
        {
            pointer.SetReadType(Pointer.ReadType.READ_CHAR_CONTINUOUS);
            return true;
        }

        public IExecutableRune Register()
        {
            RuneRegistry.ALL_RUNES.Add('`', this);
            return this;
        }
    }
}