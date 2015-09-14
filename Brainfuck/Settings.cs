
namespace EsotericIDE.Brainfuck
{
    sealed class BrainfuckSettings : LanguageSettings
    {
        public IOType InputType = IOType.Numbers;
        public IOType OutputType = IOType.Numbers;
        public CellType CellType = CellType.BigInts;
    }
}
