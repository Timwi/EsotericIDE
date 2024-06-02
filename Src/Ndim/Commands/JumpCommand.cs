namespace EsotericIDE.Ndim
{
    class JumpCommand : NdimCommand
    {
        public JumpCommand(Position position) : base(position) { }

        public override bool Execute(NdimEnv ndim)
        {
            ndim.Pointer.MoveNext();
            ndim.JumpPosition = ndim.Pointer.Position;
            return false;
        }

        public override string ToString() => "j";
    }
}
