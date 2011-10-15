using System.Collections.Generic;

namespace EsotericIDE.Languages
{
    partial class Ziim
    {
        private enum instruction
        {
            Zero,
            Stdin,
            Concat,
            NoOp,
            Inverse,
            Label,
            Splitter,
            Producer,
            IsZero,
            IsEmpty
        }

        private sealed class node
        {
            public int X { get; private set; }
            public int Y { get; private set; }
            public int Index { get; private set; }
            public instruction Instruction { get; private set; }
            public List<node> PointsTo { get; private set; }
            public List<node> PointedToBy { get; private set; }
            public node(int x, int y, int index, instruction instr)
            {
                X = x;
                Y = y;
                Index = index;
                Instruction = instr;
                PointsTo = new List<node>();
                PointedToBy = new List<node>();
            }
        }

        private sealed class thread
        {
            private node _currentInstruction, _prevInstruction;
            public node CurrentInstruction
            {
                get { return _currentInstruction; }
                set { _prevInstruction = _currentInstruction; _currentInstruction = value; }
            }
            public node PrevInstruction { get { return _prevInstruction; } }
            public bits CurrentValue;
            public bool Suspended;
            public int Role;
        }
    }
}
