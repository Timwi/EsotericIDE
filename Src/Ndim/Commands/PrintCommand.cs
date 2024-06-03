namespace EsotericIDE.Ndim
{
    class PrintCommand : NdimCommand
    {
        public PrintCommand(Position position) : base(position) { }

        public override void Execute(NdimEnv ndim)
        {
            ndim.Print(ndim.Stack.Pop().ToString());
        }

        public override string ToString() => "pr";
    }
}
