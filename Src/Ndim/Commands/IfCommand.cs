﻿namespace EsotericIDE.Ndim
{
    class IfCommand : NdimCommand
    {
        public IfCommand(int value, Position position) : base(position) { _value = value; }

        private readonly int _value;

        public override bool Execute(NdimEnv ndim)
        {
            if (ndim.Stack.Pop() == _value)
                ndim.Pointer.TurnRight();
            else
                ndim.Pointer.TurnLeft();
            return true;
        }

        public override string ToString() => "if";
    }
}
