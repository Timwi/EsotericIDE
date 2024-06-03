namespace EsotericIDE.Ndim
{
    class AssignCommand : NdimCommand
	{
		public AssignCommand(Position position) : base(position) { }

		public override void Execute(NdimEnv ndim)
		{
			int value = ndim.Stack.Pop();
			ndim.RegisterCommandOrValue(ndim.Pointer.GetRight(), new Value(value, null));
		}
        public override string ToString() => "=";
    }
}
