namespace EsotericIDE.Ndim
{
    class ToggleEatCommand : NdimCommand
    {
        public ToggleEatCommand(Position position) : base(position) { }

        public override void Execute(NdimEnv ndim)
        {
            ndim.EatMode = !ndim.EatMode;
        }

        public override string ToString() => "eat";
    }
}
