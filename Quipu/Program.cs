using System;
using System.Numerics;
using RT.Util;
using RT.Util.ExtensionMethods;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace EsotericIDE.Languages
{
    partial class Quipu
    {
        private class program
        {
            public static program Parse(string source)
            {
                var sourceLength = source.Length;
                int index = 0;
                var m = Regex.Match(source, @"^\s*""[^""]*""", RegexOptions.Singleline);
                if (m.Success)
                {
                    source = source.Substring(m.Length);
                    index += m.Length;
                }
                m = Regex.Match(source, @"^\s*\n(.*?)\s*$", RegexOptions.Singleline);
                if (m.Success)
                {
                    source = m.Groups[1].Value;
                    index += m.Groups[1].Index;
                }
                var sourceLines = new List<Tuple<string, int>>();
                while ((m = Regex.Match(source, @"\r?\n")).Success)
                {
                    sourceLines.Add(new Tuple<string, int>(source.Substring(0, m.Index), index));
                    index += m.Index + 2;
                    source = source.Substring(m.Index + m.Length);
                }
                sourceLines.Add(new Tuple<string, int>(source, index));
                var threads = new List<thread>();
                var threadIndex = 0;

                while (true)
                {
                    var curThread = new thread();
                    var indentation = sourceLines[0].Item1.TakeWhile(char.IsWhiteSpace).Count();
                    BigInteger? prevInstructionWasThousands = null;
                    BigInteger? prevInstructionWasHundreds = null;
                    BigInteger? prevInstructionWasTens = null;
                    BigInteger? prevInstructionWasUnits = null;
                    string prevInstructionWasString = null;

                    for (int i = 0; i < sourceLines.Count; i++)
                    {
                        var instructionStr = sourceLines[i].Item1.SubstringSafe(indentation, 3);
                        if (string.IsNullOrWhiteSpace(instructionStr))
                            break;
                        if ((instructionStr.Length != 3 || instructionStr[2] != ' ') && instructionStr.Length != 2)
                            throw new CompileException("Instructions must be two characters long.");
                        instructionStr = instructionStr.Substring(0, 2);

                        var position = new Position(sourceLines[i].Item2 + threadIndex + indentation, 2);

                        Func<quipuExecutionEnvironment, int?> instruction = null;
                        BigInteger? instructionIsThousands = null;
                        BigInteger? instructionIsHundreds = null;
                        BigInteger? instructionIsTens = null;
                        BigInteger? instructionIsUnits = null;
                        string instructionIsString = null;
                        bool removeLast = false;

                        if (instructionStr.StartsWith("'") || instructionStr == "\\n")
                        {
                            var actualStr = instructionStr == "\\n" ? Environment.NewLine : instructionStr.Substring(1);
                            if (prevInstructionWasString != null)
                            {
                                var str = (instructionIsString = prevInstructionWasString + actualStr);
                                instruction = qee => { qee.Stack.Push(str); return null; };
                                removeLast = true;
                            }
                            else
                            {
                                var str = (instructionIsString = actualStr);
                                instruction = qee => { qee.Stack.Push(str); return null; };
                            }
                        }

                        else if ((m = Regex.Match(instructionStr, @"^(\d)#$")).Success)
                        {
                            if (prevInstructionWasThousands != null)
                            {
                                var num = 10 * prevInstructionWasThousands.Value + 1000 * Convert.ToInt32(m.Groups[1].Value);
                                instruction = qee => { qee.Stack.Push(num); return null; };
                                instructionIsThousands = num;
                                removeLast = true;
                            }
                            else
                            {
                                var num = 1000 * BigInteger.Parse(m.Groups[1].Value);
                                instruction = qee => { qee.Stack.Push(num); return null; };
                                instructionIsThousands = num;
                            }
                        }

                        else if ((m = Regex.Match(instructionStr, @"^(\d)%$")).Success)
                        {
                            if (prevInstructionWasThousands != null)
                            {
                                var num = prevInstructionWasThousands.Value + 100 * Convert.ToInt32(m.Groups[1].Value);
                                instruction = qee => { qee.Stack.Push(num); return null; };
                                instructionIsHundreds = num;
                                removeLast = true;
                            }
                            else
                            {
                                var num = 100 * BigInteger.Parse(m.Groups[1].Value);
                                instruction = qee => { qee.Stack.Push(num); return null; };
                                instructionIsHundreds = num;
                            }
                        }

                        else if ((m = Regex.Match(instructionStr, @"^(\d)@$")).Success)
                        {
                            if (prevInstructionWasThousands != null || prevInstructionWasHundreds != null)
                            {
                                var num = (prevInstructionWasThousands ?? prevInstructionWasHundreds).Value + 10 * Convert.ToInt32(m.Groups[1].Value);
                                instruction = qee => { qee.Stack.Push(num); return null; };
                                instructionIsTens = num;
                                removeLast = true;
                            }
                            else
                            {
                                var num = 10 * BigInteger.Parse(m.Groups[1].Value);
                                instruction = qee => { qee.Stack.Push(num); return null; };
                                instructionIsTens = num;
                            }
                        }

                        else if ((m = Regex.Match(instructionStr, @"^(\d)&$")).Success)
                        {
                            if (prevInstructionWasThousands != null || prevInstructionWasHundreds != null || prevInstructionWasTens != null)
                            {
                                var num = (prevInstructionWasThousands ?? prevInstructionWasHundreds ?? prevInstructionWasTens).Value + Convert.ToInt32(m.Groups[1].Value);
                                instruction = qee => { qee.Stack.Push(num); return null; };
                                instructionIsUnits = num;
                                removeLast = true;
                            }
                            else
                            {
                                var num = BigInteger.Parse(m.Groups[1].Value);
                                instruction = qee => { qee.Stack.Push(num); return null; };
                                instructionIsUnits = num;
                            }
                        }

                        else
                        {
                            Func<BigInteger, BigInteger, BigInteger> operation = null;
                            Func<BigInteger, bool> jump = null;

                            switch (instructionStr)
                            {
                                case "[]":
                                    instruction = qee =>
                                    {
                                        var item = qee.Stack.Pop();
                                        if (!(item is BigInteger))
                                            throw new InvalidOperationException("The value on the stack is not a number.");
                                        var itemBi = (BigInteger) item;
                                        if (itemBi < 0 || itemBi >= qee.ThreadValues.Count)
                                            throw new InvalidOperationException("The value on the stack is not a valid thread number.");
                                        qee.Stack.Push(qee.ThreadValues[(int) itemBi]);
                                        return null;
                                    };
                                    break;

                                case "/\\":
                                    instruction = qee =>
                                    {
                                        if (qee.Stack.Count < 1)
                                            throw new InvalidOperationException("Stack underflow.");
                                        qee.AddOutput(qee.Stack.Peek().ToString());
                                        return null;
                                    };
                                    break;

                                case "##":
                                    instruction = qee =>
                                    {
                                        if (qee.Stack.Count < 1)
                                            throw new InvalidOperationException("Stack underflow.");
                                        var item = qee.Stack.Pop();
                                        qee.Stack.Push(item);
                                        qee.Stack.Push(item);
                                        return null;
                                    };
                                    break;

                                case "++": operation = (a, b) => a + b; goto case "%%";
                                case "--": operation = (a, b) => b - a; goto case "%%";
                                case "**": operation = (a, b) => a * b; goto case "%%";
                                case "//": operation = (a, b) => b / a; goto case "%%";
                                case "%%":
                                    if (operation == null)
                                        operation = (a, b) => b % a;
                                    instruction = qee =>
                                    {
                                        // This operation is not supposed to pop from the stack
                                        var item1 = qee.GetNumber();
                                        var item2 = qee.GetNumber();
                                        qee.Stack.Push(item2);
                                        qee.Stack.Push(item1);
                                        qee.Stack.Push(operation(item1, item2));
                                        return null;
                                    };
                                    break;


                                case "==": jump = b => b == 0; goto case ">=";
                                case "<<": jump = b => b < 0; goto case ">=";
                                case "<=": jump = b => b <= 0; goto case ">=";
                                case ">>": jump = b => b > 0; goto case ">=";
                                case ">=":
                                    if (jump == null)
                                        jump = b => b >= 0;

                                    instruction = qee =>
                                    {
                                        var threadNum = qee.GetNumber();
                                        var item = qee.GetNumber();
                                        if (threadNum < 0 || threadNum >= qee.ThreadValues.Count)
                                            throw new InvalidOperationException("Invalid thread number.");
                                        qee.Stack.Push(item);
                                        return jump(item) ? (int?) (int) threadNum : null;
                                    };
                                    break;


                                case "??":
                                    instruction = qee =>
                                    {
                                        var threadNum = qee.GetNumber();
                                        if (threadNum < 0 || threadNum >= qee.ThreadValues.Count)
                                            throw new InvalidOperationException("Invalid thread number.");
                                        return (int) threadNum;
                                    };
                                    break;

                                case "\\/": instruction = qee => { qee.Stack.Push(qee.Input.ReadLine() ?? ""); return null; }; break;
                                case "^^": instruction = qee => { qee.Stack.Push(qee.ThreadValues[qee.CurrentThread]); return null; }; break;
                                case ";;": instruction = qee => { return null; }; break;
                                case "::": instruction = qee => { return -1; }; break;
                            }
                        }

                        if (instruction == null)
                            throw new CompileException("Instruction is not recognized.", position.Index, position.Length);

                        if (removeLast)
                            curThread.Knots.RemoveAt(curThread.Knots.Count - 1);
                        curThread.Knots.Add(new knot(instruction, new Position(sourceLines[i].Item2 + threadIndex + indentation, 2)));

                        prevInstructionWasString = instructionIsString;
                        prevInstructionWasThousands = instructionIsThousands;
                        prevInstructionWasHundreds = instructionIsHundreds;
                        prevInstructionWasTens = instructionIsTens;
                        prevInstructionWasUnits = instructionIsUnits;
                    }

                    if (curThread.Knots.Count == 0)
                        break;
                    threads.Add(curThread);
                    for (int i = 0; i < sourceLines.Count; i++)
                        sourceLines[i] = new Tuple<string, int>(sourceLines[i].Item1.SubstringSafe(indentation + 2), sourceLines[i].Item2);
                    threadIndex += indentation + 2;
                }

                return new program(threads.ToArray(), sourceLength);
            }

            private program(thread[] threads, int sourceLength) { Threads = threads; SourceLength = sourceLength; }

            public thread[] Threads { get; private set; }
            public int SourceLength { get; private set; }

            public IEnumerable<Position> Execute(quipuExecutionEnvironment qee)
            {
                // Empty program: exit immediately
                if (qee.ThreadValues.Count == 0)
                    yield break;

                while (true)
                {
                    var instr = Threads[qee.CurrentThread].Knots[qee.CurrentKnot];
                    yield return instr.Position;
                    var result = instr.Instruction(qee);
                    if (result != null)
                    {
                        if (result.Value == -1)
                            yield break;
                        if (result.Value < -1 || result.Value >= Threads.Length)
                            throw new InvalidOperationException(@"Jump to invalid thread {0}.".Fmt(result));
                        qee.ThreadValues[qee.CurrentThread] = qee.Stack.Pop();
                        qee.CurrentThread = result.Value;
                        qee.CurrentKnot = 0;
                        qee.Stack.Clear();
                        qee.Stack.Push(qee.ThreadValues[result.Value]);
                    }
                    else
                    {
                        qee.CurrentKnot++;
                        if (qee.CurrentKnot >= Threads[qee.CurrentThread].Knots.Count)
                        {
                            qee.ThreadValues[qee.CurrentThread] = qee.Stack.Pop();
                            qee.CurrentThread++;
                            if (qee.CurrentThread >= Threads.Length)
                                break;
                            qee.CurrentKnot = 0;
                            qee.Stack.Clear();
                            qee.Stack.Push(qee.ThreadValues[qee.CurrentThread]);
                        }
                    }
                }

                yield return new Position(SourceLength, 0);
            }
        }

        private sealed class thread
        {
            public List<knot> Knots = new List<knot>();
        }

        private sealed class knot
        {
            public Func<quipuExecutionEnvironment, int?> Instruction { get; private set; }
            public Position Position { get; private set; }
            public knot(Func<quipuExecutionEnvironment, int?> instruction, Position position)
            {
                if (instruction == null)
                    throw new ArgumentNullException("instruction");
                if (position == null)
                    throw new ArgumentNullException("position");
                Instruction = instruction;
                Position = position;
            }
        }
    }
}
