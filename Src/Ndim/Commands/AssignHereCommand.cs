namespace EsotericIDE.Ndim
{
    class AssignHereCommand : NdimCommand
    {
        public AssignHereCommand(Position position) : base(position) { }

        public override void Execute(NdimEnv ndim)
        {
            int value = ndim.Stack.Pop();
            ndim.RegisterCommandOrValue(ndim.Pointer.Position, new Value(value, null));
        }
        public override string ToString() => "=.";
    }
}
