using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace EsotericIDE.Languages
{
    partial class Ziim
    {
        private sealed class executionEnvironment : ExecutionEnvironment
        {
            public List<node> Nodes;
            public List<thread> Threads;
            public string Input;

            public override string DescribeExecutionState()
            {
                var firstUnsuspended = Threads.IndexOf(t => !t.Suspended);
                return Threads.Select((t, i) =>
                    "{0} {1,3}: {2} {3}".Fmt(
                        t.Suspended ? "·" : i == firstUnsuspended ? "→" : " ",
                        i,
                        t.CurrentInstruction.Instruction,
                        t.CurrentValue
                    )
                ).JoinString(Environment.NewLine);
            }

            protected override void run()
            {
                if (State == ExecutionState.Finished)
                {
                    _resetEvent.Reset();
                    return;
                }

                RuntimeException exception = null;

                while (State == ExecutionState.Debugging || State == ExecutionState.Running)
                {
                    var activeThread = Threads.FirstOrDefault(t => !t.Suspended);
                    if (activeThread == null)
                        // No threads left or all threads suspended
                        break;

                    if (activeThread.CurrentInstruction == null)
                    {
                        _output.Append(activeThread.CurrentValue.ToOutput());
                        goto finished;
                    }

                    lock (_locker)
                        if (_breakpoints != null && _breakpoints.Contains(activeThread.CurrentInstruction.Index))
                            State = ExecutionState.Debugging;

                    if (State == ExecutionState.Debugging)
                    {
                        fireDebuggerBreak(new Position(activeThread.CurrentInstruction.Index, 1));
                        _resetEvent.Reset();
                        _resetEvent.WaitOne();
                    }

                    switch (activeThread.CurrentInstruction.Instruction)
                    {
                        case instruction.Zero:
                            {
                                activeThread.CurrentValue = bits.Zero;
                                activeThread.CurrentInstruction = activeThread.CurrentInstruction.PointsTo[0];
                                break;
                            }

                        case instruction.Stdin:
                            {
                                activeThread.CurrentValue = bits.FromString(Input);
                                activeThread.CurrentInstruction = activeThread.CurrentInstruction.PointsTo[0];
                                break;
                            }

                        case instruction.Concat:
                            {
                                activeThread.Role = activeThread.CurrentInstruction.PointedToBy.IndexOf(activeThread.PrevInstruction);
                                Ut.Assert(activeThread.Role == 0 || activeThread.Role == 1);
                                var otherThread = Threads.FirstOrDefault(t => t.Suspended && t.CurrentInstruction == activeThread.CurrentInstruction && t.Role == (activeThread.Role ^ 1));
                                if (otherThread != null)
                                {
                                    activeThread.CurrentValue = activeThread.Role == 0 ? activeThread.CurrentValue.Concat(otherThread.CurrentValue) : otherThread.CurrentValue.Concat(activeThread.CurrentValue);
                                    activeThread.CurrentInstruction = activeThread.CurrentInstruction.PointsTo[0];
                                    Threads.Remove(otherThread);
                                }
                                else
                                    activeThread.Suspended = true;
                                break;
                            }

                        case instruction.NoOp:
                        case instruction.Label:
                            {
                                activeThread.CurrentInstruction = activeThread.CurrentInstruction.PointsTo[0];
                                break;
                            }

                        case instruction.Inverse:
                            {
                                activeThread.CurrentValue = activeThread.CurrentValue.Invert();
                                activeThread.CurrentInstruction = activeThread.CurrentInstruction.PointsTo[0];
                                break;
                            }

                        case instruction.Splitter:
                            {
                                var newThread = new thread();
                                newThread.CurrentInstruction = activeThread.CurrentInstruction; // necessary for PrevInstruction to be correct
                                newThread.CurrentInstruction = activeThread.CurrentInstruction.PointsTo[1];
                                newThread.CurrentValue = activeThread.CurrentValue;
                                Threads.Add(newThread);
                                activeThread.CurrentInstruction = activeThread.CurrentInstruction.PointsTo[0];
                                break;
                            }

                        case instruction.Producer:
                            {
                                activeThread.Role = activeThread.CurrentInstruction.PointedToBy.IndexOf(activeThread.PrevInstruction);
                                Ut.Assert(activeThread.Role == 0 || activeThread.Role == 1);
                                var dataThread = Threads.FirstOrDefault(t => t.Suspended && t.CurrentInstruction == activeThread.CurrentInstruction && t.Role == 0);

                                if (dataThread == null)
                                {
                                    activeThread.Suspended = true;
                                    if (activeThread.Role == 0)
                                        foreach (var thr in Threads)
                                            if (thr.Suspended && thr.CurrentInstruction == activeThread.CurrentInstruction && thr.Role == 1)
                                            {
                                                thr.CurrentValue = activeThread.CurrentValue;
                                                thr.CurrentInstruction = thr.CurrentInstruction.PointsTo[0];
                                                thr.Suspended = false;
                                            }
                                }
                                else if (activeThread.Role == 0)
                                {
                                    exception = new RuntimeException(new Position(activeThread.CurrentInstruction.Index, 1), "Data channel is already initialised.");
                                    goto finished;
                                }
                                else
                                {
                                    activeThread.CurrentValue = dataThread.CurrentValue;
                                    activeThread.CurrentInstruction = activeThread.CurrentInstruction.PointsTo[0];
                                }
                                break;
                            }

                        case instruction.IsZero:
                            {
                                if (activeThread.CurrentValue.IsEmpty)
                                    Threads.Remove(activeThread);
                                var iz = activeThread.CurrentValue.IsFirstBitZero;
                                activeThread.CurrentValue = activeThread.CurrentValue.RemoveFirstBit();
                                activeThread.CurrentInstruction = activeThread.CurrentInstruction.PointsTo[iz ? 1 : 0];
                            }
                            break;

                        case instruction.IsEmpty:
                            {
                                activeThread.CurrentInstruction = activeThread.CurrentInstruction.PointsTo[activeThread.CurrentValue.IsEmpty ? 0 : 1];
                                activeThread.CurrentValue = bits.Empty;
                            }
                            break;

                        default:
                            exception = new RuntimeException(new Position(activeThread.CurrentInstruction.Index, 1), "Unrecognised instruction encountered. (This indicates a bug in the parser, which should have caught this at compile-time.)");
                            goto finished;
                    }
                }

                finished:
                fireExecutionFinished(State == ExecutionState.Stop || exception != null, exception);
                State = ExecutionState.Finished;
                _resetEvent.Reset();
            }
        }
    }
}
