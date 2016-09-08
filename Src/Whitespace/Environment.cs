using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace EsotericIDE.Whitespace
{
    sealed class WhitespaceEnv : ExecutionEnvironment
    {
        private Node[] _program;
        private string _input;
        private Stack<BigInteger> _stack;
        private Dictionary<BigInteger, BigInteger> _heap;
        private Stack<int> _callStack;

        private NumberInputSemantics _numberInputSemantics;
        private CharacterSemantics _outputSemantics;
        private Regex _integerFinder = new Regex(@"-?\d+", RegexOptions.Compiled);

        public WhitespaceEnv(string source, string input, NumberInputSemantics numInputSem, CharacterSemantics outputSem)
        {
            _input = input;
            _stack = new Stack<BigInteger>();
            _heap = new Dictionary<BigInteger, BigInteger>();
            _callStack = new Stack<int>();
            _numberInputSemantics = numInputSem;
            _outputSemantics = outputSem;
            _program = Parse(source);
            // Clipboard.SetText(_program.JoinString(Environment.NewLine));
        }

        public static Node[] Parse(string source, int? throwAtIndex = null)
        {
            var index = 0;
            while (index < source.Length && !" \t\n".Contains(source[index]))
                index++;

            var nodes = new List<Node>();

            var instrs = EnumStrong.GetValues<Instruction>().Select(instr => instr.GetCustomAttribute<InstructionAttribute>().Apply(attr => new
            {
                Instruction = instr,
                Str = attr.Instr,
                Arg = attr.Arg
            })).ToArray();

            while (index < source.Length)
            {
                var startIndex = index;
                var match = instrs
                    .Select(inf =>
                    {
                        var newIndex = index;
                        var instrIndex = 0;
                        while (newIndex < source.Length && instrIndex < inf.Str.Length)
                        {
                            if (" \t\n".Contains(source[newIndex]))
                            {
                                if (source[newIndex] == inf.Str[instrIndex])
                                    instrIndex++;
                                else
                                    break;
                            }
                            newIndex++;
                        }
                        if (instrIndex == inf.Str.Length)
                            return new
                            {
                                Instruction = inf.Instruction,
                                NewIndex = newIndex,
                                Arg = inf.Arg
                            };
                        return null;
                    })
                    .Where(inf => inf != null)
                    .FirstOrDefault();

                if (match == null)
                    throw new CompileException($@"Instruction at index {index} not recognized.", index);

                index = match.NewIndex;
                string labelArg = null;
                BigInteger? numberArg = null;
                List<bool> argBits = null;

                if (match.Arg != null)
                {
                    while (index < source.Length && !" \t\n".Contains(source[index]))
                        index++;
                    var origIndex = index;

                    argBits = new List<bool>();
                    while (index < source.Length && source[index] != '\n')
                    {
                        if (source[index] == ' ')
                            argBits.Add(false);
                        else if (source[index] == '\t')
                            argBits.Add(true);
                        index++;
                    }
                    if (index == source.Length)
                        throw new CompileException("Unterminated number literal or label name.", origIndex);
                    index++;
                    if (match.Arg == ArgKind.Label)
                    {
                        if (argBits.Count >= 8)
                        {
                            var argIndex = 0;
                            var bytes = new List<byte>();
                            while (argIndex < argBits.Count - 8)
                            {
                                bytes.Add((byte) argBits.Skip(argIndex).Take(8).Aggregate(0, (prev, next) => (prev << 1) | (next ? 1 : 0)));
                                argIndex += 8;
                            }
                            bytes.Add((byte) argBits.Skip(argIndex).Aggregate(0, (prev, next) => (prev << 1) | (next ? 1 : 0)));
                            labelArg = bytes.ToArray().FromUtf8();
                        }
                        else
                            labelArg = argBits.Select(b => b ? '1' : '0').JoinString();
                    }
                    else
                    {
                        if (argBits.Count == 0)
                            throw new CompileException("Expected a number (need at least one space or tab before the terminating linefeed).", origIndex);
                        var bi = BigInteger.Zero;
                        for (int i = 1; i < argBits.Count; i++)
                            bi = (bi << 1) | (argBits[i] ? 1 : 0);
                        numberArg = argBits[0] ? -bi : bi;
                    }
                }

                if (throwAtIndex != null && startIndex <= throwAtIndex.Value && index > throwAtIndex.Value)
                    throw new ParseInfoException(match.Instruction, argBits, numberArg, labelArg);
                nodes.Add(new Node(new Position(startIndex, index - startIndex), match.Instruction, numberArg, labelArg));

                while (index < source.Length && !" \t\n".Contains(source[index]))
                    index++;
            }

            nodes.Add(new Node(new Position(source.Length, 0), Instruction.Exit));
            return nodes.ToArray();
        }

        protected override IEnumerable<Position> GetProgram()
        {
            var i = 0;
            while (i != -1)
            {
                yield return _program[i].Position;
                i = execute(i);
            }
        }

        private int execute(int nodeIx)
        {
            var node = _program[nodeIx];
            switch (node.Instruction)
            {
                // Stack Manipulation
                case Instruction.Push: _stack.Push(node.Arg.Value); break;
                case Instruction.Dup: _stack.Push(_stack.Peek()); break;
                case Instruction.Copy: _stack.Push(_stack.Skip((int) node.Arg.Value).First()); break;
                case Instruction.Discard: _stack.Pop(); break;
                case Instruction.Swap:
                    var a = _stack.Pop();
                    var b = _stack.Pop();
                    _stack.Push(a);
                    _stack.Push(b);
                    break;

                case Instruction.Slide:
                    var top = _stack.Pop();
                    for (int i = (int) node.Arg.Value - 1; i >= 0; i--)
                        _stack.Pop();
                    _stack.Push(top);
                    break;

                // Arithmetic
                case Instruction.Add: _stack.Push(_stack.Pop() + _stack.Pop()); break;
                case Instruction.Subtract: _stack.Push(-_stack.Pop() + _stack.Pop()); break;
                case Instruction.Multiply: _stack.Push(_stack.Pop() * _stack.Pop()); break;
                case Instruction.Div: _stack.Pop().Apply(divisor => { _stack.Push(_stack.Pop() / divisor); }); break;
                case Instruction.Modulo: _stack.Pop().Apply(divisor => { _stack.Push(_stack.Pop() % divisor); }); break;

                // Heap Access
                case Instruction.Store: _stack.Pop().Apply(value => { _heap[_stack.Pop()] = value; }); break;
                case Instruction.Retrieve: _stack.Push(_heap.Get(_stack.Pop(), BigInteger.Zero)); break;

                // Flow Control
                case Instruction.Mark: /* does nothing */ break;
                case Instruction.Call: _callStack.Push(nodeIx); return findLabel(node.Label);
                case Instruction.Jump: return findLabel(node.Label);
                case Instruction.Return: return _callStack.Pop() + 1;
                case Instruction.Exit: return -1;

                case Instruction.JumpIfZero:
                    if (_stack.Pop().IsZero)
                        return findLabel(node.Label);
                    break;

                case Instruction.JumpIfNeg:
                    if (_stack.Pop() < 0)
                        return findLabel(node.Label);
                    break;

                // I/O
                case Instruction.OutputNumber: _output.Append(_stack.Pop().ToString()); break;

                case Instruction.OutputChar:
                    var valToOutput = _stack.Pop();
                    if (valToOutput < 0)
                        throw new Exception("Cannot output a negative value as a character.");
                    if (_outputSemantics == CharacterSemantics.Bytewise)
                        _output.Append((char) (int) (valToOutput % 256));
                    else
                        _output.Append(char.ConvertFromUtf32((int) valToOutput));
                    break;

                case Instruction.ReadChar:
                    if (_input.Length == 0)
                        throw new Exception("Attempt to read past the end of the input.");
                    _heap[_stack.Pop()] = _input[0];
                    _input = _input.Substring(1);
                    break;

                case Instruction.ReadNumber:
                    BigInteger inputNumber;
                    if (_numberInputSemantics == NumberInputSemantics.Minimal)
                    {
                        var m = _integerFinder.Match(_input);
                        if (!m.Success)
                            throw new Exception("Input is not a valid number.");
                        inputNumber = BigInteger.Parse(m.Value);
                        _input = _input.Substring(m.Index + m.Length);
                    }
                    else
                    {
                        var pos = _input.IndexOf('\n');
                        var line = pos == -1 ? _input : _input.Substring(0, pos);

                        if (!BigInteger.TryParse(line, out inputNumber))
                        {
                            if (_numberInputSemantics == NumberInputSemantics.LinewiseLenient)
                                inputNumber = BigInteger.Zero;
                            else
                                throw new Exception("Input is not a valid number.");
                        }
                        _input = _input.Substring(pos + 1);
                    }
                    _heap[_stack.Pop()] = inputNumber;
                    break;

                default:
                    throw new Exception("Unrecognized instruction during execution. This indicates a bug in the parser.");
            }

            return nodeIx + 1;
        }

        private int findLabel(string label)
        {
            for (int i = 0; i < _program.Length; i++)
                if (_program[i].Instruction == Instruction.Mark && _program[i].Label == label)
                    return i;
            throw new Exception("Label not found.");
        }

        public override void UpdateWatch()
        {
            _txtWatch.Text = $"Remaining input: {_input}\r\n\r\nStack:\r\n{_stack.JoinString("\r\n")}\r\n\r\nHeap:\r\n{_heap.Select(kvp => $"{kvp.Key} → {kvp.Value}").JoinString("\r\n")}";
        }
    }
}
