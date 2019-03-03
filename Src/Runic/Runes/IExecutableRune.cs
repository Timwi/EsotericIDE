namespace EsotericIDE.Runic.Runes
{
    public interface IExecutableRune
    {
        bool Execute(Pointer pointer, IRunicContext context);
        IExecutableRune Register();
    }
}