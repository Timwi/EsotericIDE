using System.Collections.Generic;

namespace EsotericIDE.Ziim
{
    sealed class Node
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Index { get; private set; }
        public Position Position { get; private set; }
        public Instruction Instruction { get; private set; }
        public List<Node> PointsTo { get; private set; }
        public List<Node> PointedToBy { get; private set; }
        public Node(int x, int y, int index, Instruction instr)
        {
            X = x;
            Y = y;
            Index = index;
            Instruction = instr;
            PointsTo = new List<Node>();
            PointedToBy = new List<Node>();
            Position = new Position(Index, 1);
        }
    }
}
