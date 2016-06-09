
namespace EsotericIDE.StackCats
{
    sealed class StackCatsSettings : LanguageSettings
    {
        public IOType InputType = IOType.Bytes;
        public IOType OutputType = IOType.Bytes;
        public bool ImplicitlyMirror = false;
    }
}
