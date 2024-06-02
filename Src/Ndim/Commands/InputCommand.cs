namespace EsotericIDE.Ndim
{
    class InputCommand : NdimCommand
    {
        public InputCommand(Position position) : base(position) { }

        public override bool Execute(NdimEnv ndim)
        {
            ndim.Stack.Push(ndim.GetInput());
            return false;
        }

        public override string ToString() => "inp";
    }
}
