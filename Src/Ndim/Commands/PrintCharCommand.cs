namespace EsotericIDE.Ndim
{
    class PrintCharCommand : NdimCommand
    {
        public PrintCharCommand(Position position) : base(position) { }

        public override void Execute(NdimEnv ndim)
        {
            ndim.Print(char.ConvertFromUtf32(ndim.Stack.Pop()));
        }

        public override string ToString() => "pc";
    }
}
