namespace EsotericIDE.Ndim
{
    class PrintCharCommand : NdimCommand
    {
        public PrintCharCommand(Position position) : base(position) { }

        public override bool Execute(NdimEnv ndim)
        {
            ndim.Print(char.ConvertFromUtf32(ndim.Stack.Pop()));
            return false;
        }

        public override string ToString() => "pc";
    }
}
