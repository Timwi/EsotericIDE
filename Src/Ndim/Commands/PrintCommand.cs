namespace EsotericIDE.Ndim
{
    class PrintCommand : NdimCommand
    {
        public PrintCommand(Position position) : base(position) { }

        public override bool Execute(NdimEnv ndim)
        {
            ndim.Print(ndim.Stack.Pop().ToString());
            return false;
        }

        public override string ToString() => "pr";
    }
}
