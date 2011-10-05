using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EsotericIDE.Languages
{
    partial class Brainfuck
    {
        private class program
        {
            public static program Parse(string source)
            {
                return new program
                {
                    _nodes = parse(source, 0),
                    _posEnd = new Position(source.Length, 0)
                };
            }

            private static node[] parse(string source, int add)
            {
                var list = new List<node>();
                var index = 0;
                cont:
                while (index < source.Length)
                {
                    if ("+-<>.,".Contains(source[index]))
                        list.Add(new charNode(source[index], new Position(index + add, 1)));
                    else if (source[index] == '[')
                    {
                        var brackets = 1;
                        for (int i = index + 1; i < source.Length; i++)
                        {
                            if (source[i] == '[')
                                brackets++;
                            else if (source[i] == ']')
                            {
                                brackets--;
                                if (brackets == 0)
                                {
                                    list.Add(new loopNode(parse(source.Substring(index + 1, i - index - 1), add + index + 1), new Position(index + add, 1), new Position(i + add, 1)));
                                    index = i + 1;
                                    goto cont;
                                }
                            }
                        }
                        throw new ParseException("Unclosed ‘[’.", index, 1);
                    }
                    else if (source[index] == ']')
                        throw new ParseException("Unmatched closing ‘]’.", index, 1);
                    index++;
                }
                return list.ToArray();
            }

            private program() { }

            private abstract class node
            {
                public abstract IEnumerable<Position> Execute(brainfuckExecutionEnvironment be);
            }
            private sealed class charNode : node
            {
                private char _char;
                private Position _pos;
                public charNode(char ch, Position pos) { _char = ch; _pos = pos; }
                public override IEnumerable<Position> Execute(brainfuckExecutionEnvironment be)
                {
                    yield return _pos;
                    switch (_char)
                    {
                        case '+': be.Inc(); break;
                        case '-': be.Dec(); break;
                        case '<': be.MoveLeft(); break;
                        case '>': be.MoveRight(); break;
                        case '.': be.Output(); break;
                        case ',': be.Input(); break;
                        default:
                            break;
                    }
                }
            }
            private sealed class loopNode : node
            {
                private node[] _inner;
                private Position _posStart;
                private Position _posEnd;
                public loopNode(node[] inner, Position posStart, Position posEnd) { _inner = inner; _posStart = posStart; _posEnd = posEnd; }
                public override IEnumerable<Position> Execute(brainfuckExecutionEnvironment be)
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

            private node[] _nodes;
            private Position _posEnd;
            public IEnumerable<Position> Execute(brainfuckExecutionEnvironment be)
            {
                foreach (var node in _nodes)
                    foreach (var pos in node.Execute(be))
                        yield return pos;
                yield return _posEnd;
            }
        }
    }
}
