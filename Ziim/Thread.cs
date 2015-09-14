
namespace EsotericIDE.Ziim
{
    sealed class Thread
    {
        private Node _currentInstruction, _prevInstruction;
        public Node CurrentInstruction
        {
            get { return _currentInstruction; }
            set { _prevInstruction = _currentInstruction; _currentInstruction = value; }
        }
        public Node PrevInstruction { get { return _prevInstruction; } }
        public Bits CurrentValue;
        public bool Suspended;
        public int Role;
    }
}
