namespace EsotericIDE.Ndim
{
    class IfCommand : NdimCommand
    {
        public IfCommand(int value, Position position) : base(position) { _value = value; }

        private readonly int _value;

        public override void Execute(NdimEnv ndim)
        {
            if (ndim.Stack.Pop() == _value)
                ndim.Pointer.TurnRight();
            else
                ndim.Pointer.TurnLeft();
        }

        public override string ToString() => $"{_value}?";
    }
}
