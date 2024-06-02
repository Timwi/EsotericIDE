namespace EsotericIDE.Ndim
{
    class DivideCommand : NdimCommand
	{
		public DivideCommand(Position position) : base(position) { }

		public override bool Execute(NdimEnv ndim)
		{
			int a = ndim.Stack.Pop();
			int b = ndim.Stack.Pop();
			ndim.Stack.Push(b / a);
            return false;
        }
        public override string ToString() => "/";
    }
}
