namespace EsotericIDE.Ndim
{
    internal class PopCommand : NdimCommand
    {
        public PopCommand(Position position) : base(position) { }

        public override void Execute(NdimEnv ndim)
        {
            ndim.Stack.Pop();
        }

        public override string ToString() => "pop";
    }
}
