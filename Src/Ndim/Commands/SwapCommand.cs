namespace EsotericIDE.Ndim
{
    class SwapCommand : NdimCommand
    {
        public SwapCommand(Position position) : base(position) { }

        public override void Execute(NdimEnv ndim)
        {
            ndim.Stack.Swap();
        }

        public override string ToString() => "sw";
    }
}
