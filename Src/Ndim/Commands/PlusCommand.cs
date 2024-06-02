﻿namespace EsotericIDE.Ndim
{
    class PlusCommand : NdimCommand
    {
        public PlusCommand(Position position) : base(position) { }

        public override bool Execute(NdimEnv ndim)
        {
            int a = ndim.Stack.Pop();
            int b = ndim.Stack.Pop();
            ndim.Stack.Push(a + b);
            return false;
        }

        public override string ToString() => "+";
    }
}
