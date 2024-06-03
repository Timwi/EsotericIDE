namespace EsotericIDE.Ndim
{
    class EndCommand : NdimCommand
	{
		public EndCommand(Position position) : base(position) { }

		public override void Execute(NdimEnv ndim)
		{
            ndim.Ended = true;
        }

        public override string ToString() => "X";
    }
}
