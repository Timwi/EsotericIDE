namespace EsotericIDE.Ndim
{
    class GreaterThanCommand : NdimCommand
    {
        public GreaterThanCommand(Position position) : base(position) { }

        public override bool Execute(NdimEnv ndim)
        {
            int a = ndim.Stack.Pop();
            int b = ndim.Stack.Pop();
            if (b > a)
                ndim.Stack.Push(1);
            return false;
        }

        public override string ToString() => ">";
    }
}
