namespace EsotericIDE.Ndim
{
    class JumpCommand : NdimCommand
    {
        public JumpCommand(Position position) : base(position) { }

        public override void Execute(NdimEnv ndim)
        {
            ndim.Pointer.MoveNext();
        }

        public override string ToString() => "j";
    }
}
