namespace EsotericIDE.Ndim
{
    class InputCommand : NdimCommand
    {
        public InputCommand(Position position) : base(position) { }

        public override void Execute(NdimEnv ndim)
        {
            ndim.Stack.Push(ndim.GetInput());
        }

        public override string ToString() => "inp";
    }
}
