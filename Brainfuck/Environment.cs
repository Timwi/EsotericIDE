using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using RT.Util.ExtensionMethods;

namespace EsotericIDE.Languages
{
    partial class Brainfuck
    {
        private abstract class brainfuckEnv : ExecutionEnvironment
        {
            private Queue<BigInteger> _input;
            public brainfuckEnv(Queue<BigInteger> input) { _input = input; }

            public abstract void MoveLeft();
            public abstract void MoveRight();
            public abstract void BfOutput();
            public abstract void Inc();
            public abstract void Dec();
            public abstract bool IsNonZero { get; }
            protected abstract void input(BigInteger input);
            public void BfInput() { input(_input.Count == 0 ? 0 : _input.Dequeue()); }
        }

        private abstract class brainfuckEnv<TCell> : brainfuckEnv
        {
            private ioType _outputType;
            private bool _everOutput;
            private program _program;

            protected int _pointer;
            protected TCell[] _cells = new TCell[4];

            public brainfuckEnv(string source, Queue<BigInteger> input, ioType outputType)
                : base(input)
            {
                _outputType = outputType;
                _everOutput = false;
                _pointer = 0;
                _program = program.Parse(source);
            }

            public override string DescribeExecutionState()
            {
                return _cells.Cast<object>().Select((cell, i) => (_pointer == i ? "→ " : "  ") + cell).JoinString(Environment.NewLine);
            }

            public sealed override void MoveLeft()
            {
                _pointer--;
                if (_pointer < 0)
                {
                    var nc = new TCell[_cells.Length * 2];
                    Array.Copy(_cells, 0, nc, _cells.Length, _cells.Length);
                    _pointer += _cells.Length;
                    _cells = nc;
                }
            }

            public sealed override void MoveRight()
            {
                _pointer++;
                if (_pointer == _cells.Length)
                {
                    var nc = new TCell[_cells.Length * 2];
                    Array.Copy(_cells, nc, _cells.Length);
                    _cells = nc;
                }
            }

            public sealed override void BfOutput()
            {
                switch (_outputType)
                {
                    case ioType.Numbers:
                        if (_everOutput)
                            _output.Append(", ");
                        _everOutput = true;
                        _output.Append(_cells[_pointer]);
                        break;

                    case ioType.Characters:
                        outputCharacter();
                        break;

                    default:
                        throw new InvalidOperationException();
                }
            }

            protected abstract void outputCharacter();

            protected override IEnumerable<Position> GetProgram() { return _program.Execute(this); }
        }

        private sealed class brainfuckEnvBytes : brainfuckEnv<byte>
        {
            public brainfuckEnvBytes(string source, Queue<BigInteger> input, ioType ot) : base(source, input, ot) { }
            protected override void input(BigInteger input) { _cells[_pointer] = (byte) (input & byte.MaxValue); }
            public override void Inc() { unchecked { _cells[_pointer]++; } }
            public override void Dec() { unchecked { _cells[_pointer]--; } }
            public override bool IsNonZero { get { return _cells[_pointer] != 0; } }
            protected override void outputCharacter() { _output.Append(char.ConvertFromUtf32(_cells[_pointer])); }
        }
        private sealed class brainfuckEnvUInt32 : brainfuckEnv<uint>
        {
            public brainfuckEnvUInt32(string source, Queue<BigInteger> input, ioType ot) : base(source, input, ot) { }
            protected override void input(BigInteger input) { _cells[_pointer] = (uint) (input & uint.MaxValue); }
            public override void Inc() { unchecked { _cells[_pointer]++; } }
            public override void Dec() { unchecked { _cells[_pointer]--; } }
            public override bool IsNonZero { get { return _cells[_pointer] != 0; } }
            protected override void outputCharacter() { _output.Append(char.ConvertFromUtf32((int) _cells[_pointer])); }
        }
        private sealed class brainfuckEnvInt32 : brainfuckEnv<int>
        {
            public brainfuckEnvInt32(string source, Queue<BigInteger> input, ioType ot) : base(source, input, ot) { }
            protected override void input(BigInteger input) { _cells[_pointer] = (int) (((input - int.MinValue) & uint.MaxValue) + int.MinValue); }
            public override void Inc() { unchecked { _cells[_pointer]++; } }
            public override void Dec() { unchecked { _cells[_pointer]--; } }
            public override bool IsNonZero { get { return _cells[_pointer] != 0; } }
            protected override void outputCharacter() { _output.Append(char.ConvertFromUtf32(_cells[_pointer])); }
        }
        private sealed class brainfuckEnvBigInt : brainfuckEnv<BigInteger>
        {
            public brainfuckEnvBigInt(string source, Queue<BigInteger> input, ioType ot) : base(source, input, ot) { }
            protected override void input(BigInteger input) { _cells[_pointer] = input; }
            public override void Inc() { unchecked { _cells[_pointer]++; } }
            public override void Dec() { unchecked { _cells[_pointer]--; } }
            public override bool IsNonZero { get { return _cells[_pointer] != 0; } }
            protected override void outputCharacter() { _output.Append(char.ConvertFromUtf32((int) _cells[_pointer])); }
        }
    }
}
