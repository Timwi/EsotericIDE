namespace EsotericIDE.Ndim
{
    class AndCommand : NdimCommand
    {
        public AndCommand(Position position) : base(position) { }
        public override void Execute(NdimEnv ndim)
        {
            int a = ndim.Stack.Pop();
            int b = ndim.Stack.Pop();
            if (a > 0 && b > 0)
                ndim.Stack.Push(1);
        }
        public override string ToString() => "&";
    }
}
