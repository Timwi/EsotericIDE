namespace EsotericIDE.Ndim
{
    class DuplicateCommand : NdimCommand
	{
		public DuplicateCommand(Position position) : base(position) { }

		public override bool Execute(NdimEnv ndim)
		{
			int value = ndim.Stack.Pop();
			ndim.Stack.Push(value);
			ndim.Stack.Push(value);
            return false;
        }
        public override string ToString() => "d";
    }
}
