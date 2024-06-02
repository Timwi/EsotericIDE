using System;

namespace EsotericIDE.Ndim
{
    class PowerCommand : NdimCommand
    {
        public PowerCommand(Position position) : base(position) { }

        public override bool Execute(NdimEnv ndim)
        {
            int a = ndim.Stack.Pop();
            int b = ndim.Stack.Pop();
            ndim.Stack.Push((int) Math.Pow(b, a));
            return false;
        }

        public override string ToString() => "^";
    }
}
