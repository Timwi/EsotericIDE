namespace EsotericIDE.Ndim
{
    class AssignCommand : NdimCommand
	{
		public AssignCommand(Position position) : base(position) { }

		public override bool Execute(NdimEnv ndim)
		{
			int value = ndim.Stack.Pop();
			ndim.RegisterCommandOrValue(ndim.Pointer.GetRight(), new Value(value, null));
			return false;
		}
        public override string ToString() => "=";
    }
}
