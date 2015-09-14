using System.Collections.Generic;

namespace EsotericIDE.Brainfuck
{
    abstract class Node
    {
        public abstract IEnumerable<Position> Execute(BrainfuckEnv be);
    }

    sealed class CharNode : Node
    {
        private char _char;
        private Position _pos;
        public CharNode(char ch, Position pos) { _char = ch; _pos = pos; }
        public override IEnumerable<Position> Execute(BrainfuckEnv be)
        {
            yield return _pos;
            switch (_char)
            {
                case '+': be.Inc(); break;
                case '-': be.Dec(); break;
                case '<': be.MoveLeft(); break;
                case '>': be.MoveRight(); break;
                case '.': be.BfOutput(); break;
                case ',': be.BfInput(); break;
                default:
                    break;
            }
        }
    }

    sealed class LoopNode : Node
    {
        private Node[] _inner;
        private Position _posStart;
        private Position _posEnd;
        public LoopNode(Node[] inner, Position posStart, Position posEnd) { _inner = inner; _posStart = posStart; _posEnd = posEnd; }
        public override IEnumerable<Position> Execute(BrainfuckEnv be)
        {
            yield return _posStart;
            while (be.IsNonZero)
            {
                foreach (var inner in _inner)
                    foreach (var pos in inner.Execute(be))
                        yield return pos;
                yield return _posEnd;
            }
        }
    }


}
