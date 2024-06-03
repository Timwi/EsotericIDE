namespace EsotericIDE.Ndim
{
    class LessThanCommand : NdimCommand
	{
		public LessThanCommand(Position position) : base(position) { }

		public override void Execute(NdimEnv ndim)
		{
			int a = ndim.Stack.Pop();
			int b = ndim.Stack.Pop();
			if (b < a)
				ndim.Stack.Push(1);
        }

        public override string ToString() => "<";
    }
}
