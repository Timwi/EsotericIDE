using System;

namespace EsotericIDE.Ndim
{
    class PowerCommand : NdimCommand
    {
        public PowerCommand(Position position) : base(position) { }

        public override void Execute(NdimEnv ndim)
        {
            int a = ndim.Stack.Pop();
            int b = ndim.Stack.Pop();
            ndim.Stack.Push((int) Math.Pow(b, a));
        }

        public override string ToString() => "^";
    }
}
