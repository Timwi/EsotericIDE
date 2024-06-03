namespace EsotericIDE.Ndim
{
    class DuplicateCommand : NdimCommand
	{
		public DuplicateCommand(Position position) : base(position) { }

		public override void Execute(NdimEnv ndim)
		{
			int value = ndim.Stack.Pop();
			ndim.Stack.Push(value);
			ndim.Stack.Push(value);
        }
        public override string ToString() => "d";
    }
}
