using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace EsotericIDE.Ziim
{
    sealed class ZiimEnv : ExecutionEnvironment
    {
        public Node[] Nodes;
        public List<Thread> Threads;
        public string Input;

        public override void UpdateWatch()
        {
            var firstUnsuspended = Threads.IndexOf(t => !t.Suspended);
            _txtWatch.Text = Threads.Select((t, i) =>
                "{0} {1,3}: {2} {3}".Fmt(
                    t.Suspended ? "·" : i == firstUnsuspended ? "→" : " ",
                    i,
                    t.CurrentInstruction.Instruction,
                    t.CurrentValue
                )
            ).JoinString(Environment.NewLine);
        }

        protected override IEnumerable<Position> GetProgram()
        {
            while (true)
            {
                var activeThread = Threads.FirstOrDefault(t => !t.Suspended);
                if (activeThread == null)
                    // No threads left or all threads suspended
                    yield break;

                while (!activeThread.Suspended)
                {
                    if (activeThread.CurrentInstruction == null)
                    {
                        _output.Append(activeThread.CurrentValue.ToOutput());
                        yield break;
                    }

                    yield return activeThread.CurrentInstruction.Position;

                    switch (activeThread.CurrentInstruction.Instruction)
                    {
                        case Instruction.Zero:
                            {
                                activeThread.CurrentValue = Bits.Zero;
                                activeThread.CurrentInstruction = activeThread.CurrentInstruction.PointsTo[0];
                                break;
                            }

                        case Instruction.Stdin:
                            {
                                if (string.IsNullOrEmpty(Input))
                                    activeThread.CurrentValue = Bits.Empty;
                                else if (char.IsSurrogate(Input, 0))
                                {
                                    activeThread.CurrentValue = Bits.FromString(Input.Substring(0, 2));
                                    Input = Input.Substring(2);
                                }
                                else
                                {
                                    activeThread.CurrentValue = Bits.FromString(Input.Substring(0, 1));
                                    Input = Input.Substring(1);
                                }
                                activeThread.CurrentInstruction = activeThread.CurrentInstruction.PointsTo[0];
                                break;
                            }

                        case Instruction.Concat:
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

                        case Instruction.NoOp:
                        case Instruction.Label:
                            {
                                activeThread.CurrentInstruction = activeThread.CurrentInstruction.PointsTo[0];
                                break;
                            }

                        case Instruction.Inverse:
                            {
                                activeThread.CurrentValue = activeThread.CurrentValue.Invert();
                                activeThread.CurrentInstruction = activeThread.CurrentInstruction.PointsTo[0];
                                break;
                            }

                        case Instruction.Splitter:
                            {
                                var newThread = new Thread();
                                newThread.CurrentInstruction = activeThread.CurrentInstruction; // necessary for PrevInstruction to be correct
                                newThread.CurrentInstruction = activeThread.CurrentInstruction.PointsTo[1];
                                newThread.CurrentValue = activeThread.CurrentValue;
                                Threads.Add(newThread);
                                activeThread.CurrentInstruction = activeThread.CurrentInstruction.PointsTo[0];
                                break;
                            }

                        case Instruction.IsZero:
                            {
                                if (activeThread.CurrentValue.IsEmpty)
                                {
                                    Threads.Remove(activeThread);
                                    activeThread.Suspended = true;
                                }
                                var iz = activeThread.CurrentValue.IsFirstBitZero;
                                activeThread.CurrentValue = activeThread.CurrentValue.RemoveFirstBit();
                                activeThread.CurrentInstruction = activeThread.CurrentInstruction.PointsTo[iz ? 1 : 0];
                            }
                            break;

                        case Instruction.IsEmpty:
                            {
                                activeThread.CurrentInstruction = activeThread.CurrentInstruction.PointsTo[activeThread.CurrentValue.IsEmpty ? 0 : 1];
                                activeThread.CurrentValue = Bits.Empty;
                            }
                            break;

                        default:
                            throw new Exception("Unrecognized instruction encountered. (This indicates a bug in the parser, which should have caught this at compile-time.)");
                    }
                }
            }
        }
    }
}
