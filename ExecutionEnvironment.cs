using System.Collections.Generic;
using System.Text;
using System;

namespace EsotericIDE
{
    enum ExecutionState
    {
        Running,
        Debugging,
        Stop,
        Finished
    }

    abstract class ExecutionEnvironment
    {
        public event Action<Position> DebuggerBreak;
        public event Action ExecutionFinished;

        public string Input;
        public StringBuilder Output = new StringBuilder();
        public volatile ExecutionState State;
        public abstract string DescribeExecutionState();
        public abstract void Run();

        protected void fireDebuggerBreak(Position position) { if (DebuggerBreak != null) DebuggerBreak(position); }
        protected void fireExecutionFinished() { if (ExecutionFinished != null) ExecutionFinished(); }
    }
}
