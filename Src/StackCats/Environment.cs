using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using RT.Util.ExtensionMethods;

namespace EsotericIDE.StackCats
{
    sealed class SCEnvironment : ExecutionEnvironment
    {
        private string _program;
        private int[] _jumpTable;
        private List<Stack<BigInteger>> _tape;
        private int _pointer;
        private Stack<BigInteger> _remember;

        public SCEnvironment(string program, int[] jumpTable, IEnumerable<BigInteger> input)
        {
            _program = program;
            _jumpTable = jumpTable;
            _tape = new List<Stack<BigInteger>> { input.Concat(BigInteger.MinusOne).Reverse().ToStack() };
            _remember = new Stack<BigInteger>();
            _pointer = 0;
        }

        protected override IEnumerable<Position> GetProgram()
        {
            int index = 0;

            while (index < _program.Length)
            {
                var instruction = _program[index];
                yield return new Position(index, 1);

                var top = _tape[_pointer].PeekSafe(BigInteger.Zero);
                bool doNeg = false;
                switch (instruction)
                {
                    case '(':
                    case ')':
                        if (top <= 0)
                            index = _jumpTable[index];
                        break;

                    case '{':
                        _remember.Push(top);
                        break;

                    case '}':
                        if (top != _remember.Pop())
                            index = _jumpTable[index];
                        break;

                    case '-': _tape[_pointer].PopAndPush(-top); break;
                    case '!': _tape[_pointer].PopAndPush(~top); break;
                    case '*': _tape[_pointer].PopAndPush(top ^ 1); break;
                    case '_':
                        _tape[_pointer].PopSafe();
                        var top2 = _tape[_pointer].PeekSafe(BigInteger.Zero);
                        _tape[_pointer].Push(top2 - top);
                        break;
                    case '^':
                        _tape[_pointer].PopSafe();
                        var top3 = _tape[_pointer].PeekSafe(BigInteger.Zero);
                        _tape[_pointer].Push(top3 ^ top);
                        break;

                    case ':':
                        _tape[_pointer].PopSafe();
                        var top4 = _tape[_pointer].PopSafe(BigInteger.Zero);
                        _tape[_pointer].Push(top);
                        _tape[_pointer].Push(top4);
                        break;
                    case '+':
                        _tape[_pointer].PopSafe();
                        var top5 = _tape[_pointer].PopSafe(BigInteger.Zero);
                        var top6 = _tape[_pointer].PopSafe(BigInteger.Zero);
                        _tape[_pointer].Push(top);
                        _tape[_pointer].Push(top5);
                        _tape[_pointer].Push(top6);
                        break;

                    case '=':
                        ensureLeft();
                        var left = _tape[_pointer - 1].PeekSafe(BigInteger.Zero);
                        var right = _tape[_pointer + 1].PeekSafe(BigInteger.Zero);
                        _tape[_pointer - 1].PopAndPush(right);
                        _tape[_pointer + 1].PopAndPush(left);
                        break;

                    case '|':
                        var values = new List<BigInteger>();
                        while (!top.IsZero)
                        {
                            values.Add(top);
                            _tape[_pointer].Pop();
                            top = _tape[_pointer].PeekSafe(BigInteger.Zero);
                        }
                        foreach (var value in values)
                            _tape[_pointer].Push(value);
                        break;

                    case 'T':
                        if (!top.IsZero)
                        {
                            var vals = _tape[_pointer].ToList();
                            while (vals[vals.Count - 1].IsZero)
                                vals.RemoveAt(vals.Count - 1);
                            _tape[_pointer] = vals.ToStack();
                        }
                        break;

                    case '<':
                        ensureLeft();
                        _pointer--;
                        break;
                    case '>':
                        _pointer++;
                        if (_pointer >= _tape.Count)
                            _tape.Add(new Stack<BigInteger>());
                        break;

                    case '[':
                        _tape[_pointer].PopSafe();
                        ensureLeft();
                        _pointer--;
                        _tape[_pointer].Push(top);
                        if (doNeg)
                            goto case '-';
                        break;
                    case ']':
                        _tape[_pointer].PopSafe();
                        _pointer++;
                        _tape[_pointer].Push(top);
                        if (doNeg)
                            goto case '-';
                        break;

                    case 'I':
                        doNeg = true;
                        if (top < 0)
                            goto case '[';
                        else if (top > 0)
                            goto case ']';
                        break;

                    case '/':
                        ensureLeft();
                        var t = _tape[_pointer];
                        _tape[_pointer] = _tape[_pointer - 1];
                        _pointer--;
                        _tape[_pointer] = t;
                        break;

                    case '\\':
                        var u = _tape[_pointer];
                        _pointer++;
                        if (_pointer >= _tape.Count)
                            _tape.Add(new Stack<BigInteger>());
                        _tape[_pointer - 1] = _tape[_pointer];
                        _tape[_pointer] = u;
                        break;

                    case 'X':
                        ensureLeft();
                        var v = _tape[_pointer - 1];
                        _tape[_pointer - 1] = _tape[_pointer + 1];
                        _tape[_pointer + 1] = v;
                        break;

                    default:
                        throw new InvalidOperationException($"Unrecognized Stack Cats instruction: '{instruction}'.");
                }
                index++;
            }

            yield return new Position(_program.Length, 0);
            _output.Append(_tape[_pointer].IgnoreMinusOneAtEnd().Select(bi => (byte) ((bi % 256 + 256) % 256)).ToArray().FromUtf8());
        }

        private void ensureLeft()
        {
            if (_pointer > 0)
                return;
            _tape.Insert(0, new Stack<BigInteger>());
            _pointer = 1;
        }

        public override void UpdateWatch()
        {
            _txtWatch.Text = _tape.Select((stack, i) => $"{(i == _pointer ? "→" : " ")} {stack.JoinString(", ")}").JoinString("\r\n");
        }

        public override string ModifiedSource { get { return _program; } }
    }

    static class SCExtensions
    {
        public static T PeekSafe<T>(this Stack<T> stack, T defaultValue)
        {
            return stack.Count == 0 ? defaultValue : stack.Peek();
        }

        public static void PopSafe<T>(this Stack<T> stack)
        {
            if (stack.Count > 0)
                stack.Pop();
        }

        public static T PopSafe<T>(this Stack<T> stack, T defaultValue)
        {
            if (stack.Count > 0)
                return stack.Pop();
            return defaultValue;
        }

        public static void PopAndPush<T>(this Stack<T> stack, T value)
        {
            if (stack.Count > 0)
                stack.Pop();
            stack.Push(value);
        }

        public static IEnumerable<BigInteger> IgnoreMinusOneAtEnd(this IEnumerable<BigInteger> source)
        {
            BigInteger lastValue = default(BigInteger);
            var haveValue = false;
            foreach (var value in source)
            {
                if (haveValue)
                    yield return lastValue;
                lastValue = value;
                haveValue = true;
            }
            if (haveValue && lastValue != BigInteger.MinusOne)
                yield return lastValue;
        }
    }
}
