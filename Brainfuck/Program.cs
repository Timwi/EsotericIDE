using System.Collections.Generic;
using System.Linq;

namespace EsotericIDE.Brainfuck
{
    sealed class Program
    {
        public IEnumerable<Position> Execute(BrainfuckEnv be)
        {
            foreach (var node in _nodes)
                foreach (var pos in node.Execute(be))
                    yield return pos;
            yield return _posEnd;
        }

        public static Program Parse(string source)
        {
            return new Program
            {
                _nodes = parse(source, 0),
                _posEnd = new Position(source.Length, 0)
            };
        }

        private Node[] _nodes;
        private Position _posEnd;

        private Program() { }

        private static Node[] parse(string source, int add)
        {
            var list = new List<Node>();
            var index = 0;
            cont:
            while (index < source.Length)
            {
                if ("+-<>.,".Contains(source[index]))
                    list.Add(new CharNode(source[index], new Position(index + add, 1)));
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
                                list.Add(new LoopNode(parse(source.Substring(index + 1, i - index - 1), add + index + 1), new Position(index + add, 1), new Position(i + add, 1)));
                                index = i + 1;
                                goto cont;
                            }
                        }
                    }
                    throw new CompileException("Unclosed ‘[’.", index, 1);
                }
                else if (source[index] == ']')
                    throw new CompileException("Unmatched closing ‘]’.", index, 1);
                index++;
            }
            return list.ToArray();
        }
    }
}
