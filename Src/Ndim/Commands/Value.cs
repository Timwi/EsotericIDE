﻿namespace EsotericIDE.Ndim
{
    class Value : NdimCommand
    {
        public Value(int value, Position position) : base(position) { _value = value; }

        private readonly int _value;

        public override void Execute(NdimEnv ndim)
        {
            ndim.Stack.Push(_value);
        }

        public override string ToString() => _value.ToString();
    }
}
