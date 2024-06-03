using System;

namespace EsotericIDE.Ndim
{
    class ChangePointerDirectionCommand : NdimCommand
    {
        public ChangePointerDirectionCommand(int newDirection, Position position) : base(position) { _newDirection = newDirection; }

        private readonly int _newDirection;

        public override void Execute(NdimEnv ndim)
        {
            ndim.Pointer.SetDirection(_newDirection);
        }
        public override string ToString() => $"{(_newDirection < 0 ? "←" : "→")}{Math.Abs(_newDirection)}";
    }
}
