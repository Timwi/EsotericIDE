using System;
using System.Collections.Generic;

namespace EsotericIDE.Unreadable
{
    sealed class Program
    {
        private node[] _nodes;
        private Position _posEnd;

        public IEnumerable<Position> Execute(UnreadableEnv env)
        {
            foreach (var node in _nodes)
            {
                foreach (var pos in node.Execute(env))
                    yield return pos;
            }
            yield return _posEnd;
        }

        public static Program Parse(string source)
        {
            var index = 0;
            var nodes = new List<node>();
            while (index < source.Length)
                nodes.Add(parse(source, ref index));

            return new Program
            {
                _nodes = nodes.ToArray(),
                _posEnd = new Position(source.Length, 0)
            };
        }

        private static node parse(string source, ref int index)
        {
            if (index >= source.Length)
                throw new CompileException("Further instructions expected.", index, 0);
            if (source[index] != '\'')
                throw new CompileException("Apostrophe expected.", index, 1);
            var origIndex = index;
            index++;
            while (index < source.Length && source[index] == '"')
                index++;

            int numArgs;
            Action<UnreadableEnv> action;

            switch (index - origIndex)
            {
                case 1:
                    throw new CompileException("Quotation mark expected.", index, 1);

                case 2: numArgs = 1; action = env => { env.Print(); }; break;
                case 3: numArgs = 1; action = env => { env.Inc(); }; break;
                case 4: numArgs = 0; action = env => { env.One(); }; break;
                case 5: numArgs = 2; action = env => { env.Sequence(); }; break;
                case 6: return new whileNode(parse(source, ref index), parse(source, ref index), new Position(origIndex, index - origIndex));
                case 7: numArgs = 2; action = env => { env.Set(); }; break;
                case 8: numArgs = 1; action = env => { env.Get(); }; break;
                case 9: numArgs = 1; action = env => { env.Dec(); }; break;
                case 10: return new ifNode(parse(source, ref index), parse(source, ref index), parse(source, ref index), new Position(origIndex, index - origIndex));
                case 11: numArgs = 0; action = env => { env.Read(); }; break;
                case 12: numArgs = 0; action = env => { env.State = ExecutionState.Debugging; }; break;

                default:
                    throw new CompileException("Too many quotation marks.", index, 1);
            }

            var args = new node[numArgs];
            for (int i = 0; i < numArgs; i++)
                args[i] = parse(source, ref index);
            return new simpleNode(args, action, new Position(origIndex, index - origIndex));
        }

        private Program() { }

        private abstract class node
        {
            public abstract IEnumerable<Position> Execute(UnreadableEnv env);
        }

        private sealed class simpleNode : node
        {
            private node[] _args;
            private Position _pos;
            private Action<UnreadableEnv> _executor;
            public simpleNode(node[] args, Action<UnreadableEnv> executor, Position pos) { _args = args; _executor = executor; _pos = pos; }
            public override IEnumerable<Position> Execute(UnreadableEnv env)
            {
                foreach (var arg in _args)
                    foreach (var pos in arg.Execute(env))
                        yield return pos;
                yield return _pos;
                _executor(env);
            }
        }

        private sealed class whileNode : node
        {
            private node _argCond;
            private node _argCode;
            private Position _pos;
            public whileNode(node argCond, node argCode, Position pos) { _argCond = argCond; _argCode = argCode; _pos = pos; }
            public override IEnumerable<Position> Execute(UnreadableEnv env)
            {
                env.Push(null);

                again:

                // Evaluate the condition
                foreach (var pos in _argCond.Execute(env))
                    yield return pos;

                yield return _pos;
                var value = env.Pop();
                if (value == null)
                    throw new Exception("While condition cannot be void.");

                if (!value.Value.IsZero)
                {
                    // Discard result from previous iteration
                    env.Pop();

                    // Execute the code
                    foreach (var pos in _argCode.Execute(env))
                        yield return pos;

                    yield return _pos;
                    goto again;
                }

                // Last result is now on the stack
                yield return _pos;
            }
        }

        private sealed class ifNode : node
        {
            private node _argCond;
            private node _argYCode;
            private node _argNCode;
            private Position _pos;
            public ifNode(node argCond, node argYCode, node argNCode, Position pos) { _argCond = argCond; _argYCode = argYCode; _argNCode = argNCode; _pos = pos; }
            public override IEnumerable<Position> Execute(UnreadableEnv env)
            {
                // Evaluate the condition
                foreach (var pos in _argCond.Execute(env))
                    yield return pos;

                yield return _pos;
                var value = env.Pop();
                if (value == null)
                    throw new Exception("If condition cannot be void.");

                var codeToExecute = value.Value.IsZero ? _argNCode : _argYCode;

                // Execute the code
                foreach (var pos in codeToExecute.Execute(env))
                    yield return pos;

                // Last result is now on the stack
                yield return _pos;
            }
        }
    }
}
