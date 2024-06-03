namespace EsotericIDE.Ndim
{
    class MinusCommand : NdimCommand
    {
        public MinusCommand(Position position) : base(position) { }

        public override void Execute(NdimEnv ndim)
        {
            int a = ndim.Stack.Pop();
            int b = ndim.Stack.Pop();
            ndim.Stack.Push(b - a);
        }

        public override string ToString() => "-";
    }
}
