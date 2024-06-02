namespace EsotericIDE.Ndim
{
    class SwapCommand : NdimCommand
    {
        public SwapCommand(Position position) : base(position) { }

        public override bool Execute(NdimEnv ndim)
        {
            ndim.Stack.Swap();
            return false;
        }

        public override string ToString() => "sw";
    }
}
