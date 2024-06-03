namespace EsotericIDE.Ndim
{
    class PlusCommand : NdimCommand
    {
        public PlusCommand(Position position) : base(position) { }

        public override void Execute(NdimEnv ndim)
        {
            int a = ndim.Stack.Pop();
            int b = ndim.Stack.Pop();
            ndim.Stack.Push(a + b);
        }

        public override string ToString() => "+";
    }
}
