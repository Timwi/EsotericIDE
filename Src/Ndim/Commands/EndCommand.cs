namespace EsotericIDE.Ndim
{
    class EndCommand : NdimCommand
	{
		public EndCommand(Position position) : base(position) { }

		public override bool Execute(NdimEnv ndim)
		{
            ndim.Ended = true;
            return false;
        }

        public override string ToString() => "X";
    }
}
