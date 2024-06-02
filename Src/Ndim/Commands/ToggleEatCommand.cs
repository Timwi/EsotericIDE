namespace EsotericIDE.Ndim
{
    class ToggleEatCommand : NdimCommand
    {
        public ToggleEatCommand(Position position) : base(position) { }

        public override bool Execute(NdimEnv ndim)
        {
            ndim.EatMode = !ndim.EatMode;
            return false;
        }

        public override string ToString() => "eat";
    }
}
