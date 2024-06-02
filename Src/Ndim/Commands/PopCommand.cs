namespace EsotericIDE.Ndim
{
    internal class PopCommand : NdimCommand
    {
        public PopCommand(Position position) : base(position) { }

        public override bool Execute(NdimEnv ndim)
        {
            ndim.Stack.Pop();
            return false;
        }

        public override string ToString() => "pop";
    }
}
