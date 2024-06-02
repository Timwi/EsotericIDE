namespace EsotericIDE.Ndim
{
    class Value : NdimCommand
    {
        public Value(int value, Position position) : base(position) { _value = value; }

        private readonly int _value;

        public override bool Execute(NdimEnv ndim)
        {
            ndim.Stack.Push(_value);
            return false;
        }

        public override string ToString() => _value.ToString();
    }
}
