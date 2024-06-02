namespace EsotericIDE.Ndim
{
    class OrCommand : NdimCommand
    {
        public OrCommand(Position position) : base(position) { }

        public override bool Execute(NdimEnv ndim)
        {
            int a = ndim.Stack.Pop();
            int b = ndim.Stack.Pop();
            if (a > 0 || b > 0)
                ndim.Stack.Push(1);
            return false;
        }

        public override string ToString() => "|";
    }
}
