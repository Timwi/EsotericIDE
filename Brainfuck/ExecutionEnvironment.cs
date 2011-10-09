using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using RT.Util.ExtensionMethods;

namespace EsotericIDE.Languages
{
    partial class Brainfuck
    {
        private abstract class brainfuckExecutionEnvironment : ExecutionEnvironment
        {
            private Queue<BigInteger> _input;
            public brainfuckExecutionEnvironment(Queue<BigInteger> input) { _input = input; }

            public abstract void MoveLeft();
            public abstract void MoveRight();
            public abstract void BfOutput();
            public abstract void Inc();
            public abstract void Dec();
            public abstract bool IsNonZero { get; }
            protected abstract void input(BigInteger input);
            public void BfInput() { input(_input.Count == 0 ? 0 : _input.Dequeue()); }
        }

        private abstract class brainfuckExecutionEnvironment<TCell> : brainfuckExecutionEnvironment
        {
            private ioType _outputType;
            private bool _everOutput;
            private program _program;

            protected int _pointer;
            protected TCell[] _cells = new TCell[4];

            public brainfuckExecutionEnvironment(string source, Queue<BigInteger> input, ioType outputType)
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

            protected override void run()
            {
                if (State == ExecutionState.Finished)
                {
                    _resetEvent.Reset();
                    return;
                }

                using (var instructionPointer = _program.Execute(this).GetEnumerator())
                {
                    bool canceled = false;
                    RuntimeException exception = null;
                    try
                    {
                        while (instructionPointer.MoveNext())
                        {
                            lock (_locker)
                                if (_breakpoints.Any(bp => bp >= instructionPointer.Current.Index && bp < instructionPointer.Current.Index + Math.Max(instructionPointer.Current.Length, 1)))
                                    State = ExecutionState.Debugging;

                            switch (State)
                            {
                                case ExecutionState.Debugging:
                                    fireDebuggerBreak(instructionPointer.Current);
                                    _resetEvent.Reset();
                                    _resetEvent.WaitOne();
                                    continue;
                                case ExecutionState.Running:
                                    continue;
                                case ExecutionState.Stop:
                                    canceled = true;
                                    goto finished;
                                case ExecutionState.Finished:
                                    goto finished;
                                default:
                                    throw new InvalidOperationException("Execution state has invalid value: " + State);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        exception = new RuntimeException(instructionPointer.Current, e.Message + " (" + e.GetType().Name + ")");
                    }

                    finished:
                    fireExecutionFinished(canceled, exception);
                    State = ExecutionState.Finished;
                    _resetEvent.Reset();
                }
            }
        }

        private sealed class brainfuckEnvironmentBytes : brainfuckExecutionEnvironment<byte>
        {
            public brainfuckEnvironmentBytes(string source, Queue<BigInteger> input, ioType ot) : base(source, input, ot) { }
            protected override void input(BigInteger input) { _cells[_pointer] = (byte) (input & byte.MaxValue); }
            public override void Inc() { unchecked { _cells[_pointer]++; } }
            public override void Dec() { unchecked { _cells[_pointer]--; } }
            public override bool IsNonZero { get { return _cells[_pointer] != 0; } }
            protected override void outputCharacter() { _output.Append(char.ConvertFromUtf32(_cells[_pointer])); }
        }
        private sealed class brainfuckEnvironmentUInt32 : brainfuckExecutionEnvironment<uint>
        {
            public brainfuckEnvironmentUInt32(string source, Queue<BigInteger> input, ioType ot) : base(source, input, ot) { }
            protected override void input(BigInteger input) { _cells[_pointer] = (uint) (input & uint.MaxValue); }
            public override void Inc() { unchecked { _cells[_pointer]++; } }
            public override void Dec() { unchecked { _cells[_pointer]--; } }
            public override bool IsNonZero { get { return _cells[_pointer] != 0; } }
            protected override void outputCharacter() { _output.Append(char.ConvertFromUtf32((int) _cells[_pointer])); }
        }
        private sealed class brainfuckEnvironmentInt32 : brainfuckExecutionEnvironment<int>
        {
            public brainfuckEnvironmentInt32(string source, Queue<BigInteger> input, ioType ot) : base(source, input, ot) { }
            protected override void input(BigInteger input) { _cells[_pointer] = (int) (((input - int.MinValue) & uint.MaxValue) + int.MinValue); }
            public override void Inc() { unchecked { _cells[_pointer]++; } }
            public override void Dec() { unchecked { _cells[_pointer]--; } }
            public override bool IsNonZero { get { return _cells[_pointer] != 0; } }
            protected override void outputCharacter() { _output.Append(char.ConvertFromUtf32(_cells[_pointer])); }
        }
        private sealed class brainfuckEnvironmentBigInt : brainfuckExecutionEnvironment<BigInteger>
        {
            public brainfuckEnvironmentBigInt(string source, Queue<BigInteger> input, ioType ot) : base(source, input, ot) { }
            protected override void input(BigInteger input) { _cells[_pointer] = input; }
            public override void Inc() { unchecked { _cells[_pointer]++; } }
            public override void Dec() { unchecked { _cells[_pointer]--; } }
            public override bool IsNonZero { get { return _cells[_pointer] != 0; } }
            protected override void outputCharacter() { _output.Append(char.ConvertFromUtf32((int) _cells[_pointer])); }
        }
    }
}
