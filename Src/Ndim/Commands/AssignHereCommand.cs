namespace EsotericIDE.Ndim
{
    class AssignHereCommand : NdimCommand
    {
        public AssignHereCommand(Position position) : base(position) { }

        public override bool Execute(NdimEnv ndim)
        {
            int value = ndim.Stack.Pop();
            ndim.RegisterCommandOrValue(ndim.Pointer.Position, new Value(value, null));
            return false;
        }
        public override string ToString() => "=.";
    }
}
