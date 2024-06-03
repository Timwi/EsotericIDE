namespace EsotericIDE.Ndim
{
    class NotCommand : NdimCommand
    {
        public NotCommand(Position position) : base(position) { }

        public override void Execute(NdimEnv ndim)
        {
            int a = ndim.Stack.Pop();
            if (a > 0)
                ndim.Stack.Push(0);
            else
                ndim.Stack.Push(1);
        }

        public override string ToString() => "!";
    }
}
