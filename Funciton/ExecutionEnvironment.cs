using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace EsotericIDE.Languages
{
    partial class Funciton
    {
        private sealed class executionEnvironment : ExecutionEnvironment
        {
            private FuncitonProgram _program;
            private string[] _traceFunctions;
            private Stack<FuncitonFunction.Node> _evaluationStack;
            private FuncitonFunction.Node _currentNode;

            public executionEnvironment(FuncitonProgram program, string[] traceFunctions)
            {
                _program = program;
                _traceFunctions = traceFunctions;
            }

            public override string DescribeExecutionState()
            {
                return "";
            }

            protected override void run()
            {
                // A larger initial capacity than this does not improve performance
                _evaluationStack = new Stack<FuncitonFunction.Node>(1024);

                // Should have only one output
                _currentNode = _program.OutputNodes.Single(o => o != null);

                var canceled = false;
                RuntimeError error = null;

                while (true)
                {
                    if (nextStep())
                        State = ExecutionState.Finished;

                    // In case another thread sets State to Debugging or Running now
                    if (_currentNode == null)
                        goto finished;

                    switch (State)
                    {
                        case ExecutionState.Debugging:
                            fireDebuggerBreak(_currentNode.Position);
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

                finished:
                fireExecutionFinished(canceled, error);
                State = ExecutionState.Finished;
                _resetEvent.Reset();
            }

            private bool nextStep()
            {
                var next = _currentNode.NextToEvaluate(_traceFunctions, this);

                // small performance optimisation (saves a push and a pop for every literal)
                while (next is FuncitonFunction.LiteralNode)
                {
                    _currentNode.PreviousSubresult = next.Result;
                    next = _currentNode.NextToEvaluate(_traceFunctions, this);
                }

                if (next != null)
                {
                    _evaluationStack.Push(_currentNode);
                    _currentNode = next;
                }
                else
                {
                    if (_evaluationStack.Count == 0)
                    {
                        lock (_locker)
                            _output.Append(FuncitonHelper.IntegerToString(_currentNode.Result));
                        _currentNode = null;
                        return true;
                    }
                    var lastResult = _currentNode.Result;
                    _currentNode = _evaluationStack.Pop();
                    _currentNode.PreviousSubresult = lastResult;
                }

                return false;
            }

            public void AddTraceLine(string traceLine)
            {
                _output.AppendLine(traceLine);
            }
        }

        private sealed class analysisExecutionEnvironment : ExecutionEnvironment
        {
            private string _analysis;
            public analysisExecutionEnvironment(string analysis) { _analysis = analysis; }
            public override string DescribeExecutionState() { return ""; }

            protected override void run()
            {
                _output.Append(_analysis);
                State = ExecutionState.Finished;
                _resetEvent.Reset();
            }
        }
    }
}
