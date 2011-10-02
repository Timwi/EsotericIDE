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

    abstract class ExecutionEnvironment : IDisposable
    {
        protected object _locker = new object();

        public event Action<Position> DebuggerBreak;
        protected void fireDebuggerBreak(Position position) { if (DebuggerBreak != null) DebuggerBreak(position); }

        public event Action<bool, RuntimeException> ExecutionFinished;
        protected void fireExecutionFinished(bool canceled, RuntimeException exception) { if (ExecutionFinished != null) ExecutionFinished(canceled, exception); }

        public event Action BreakpointsChanged;

        public string Input;

        protected StringBuilder _output = new StringBuilder();
        public string Output { get { lock (_locker) { return _output.ToString(); } } }

        protected List<int> _breakpoints = new List<int>();
        public int[] Breakpoints { get { lock (_locker) return _breakpoints.ToArray(); } }

        public void AddBreakpoint(int index)
        {
            lock (_locker) { _breakpoints.Add(index); }
            if (BreakpointsChanged != null) BreakpointsChanged();
        }
        public bool RemoveBreakpoint(int index)
        {
            bool ret;
            lock (_locker) { ret = _breakpoints.Remove(index); }
            if (BreakpointsChanged != null && ret) BreakpointsChanged();
            return ret;
        }
        public void ClearBreakpoints()
        {
            lock (_locker) { _breakpoints.Clear(); }
            if (BreakpointsChanged != null) BreakpointsChanged();
        }

        public volatile ExecutionState State = ExecutionState.Debugging;

        public abstract string DescribeExecutionState();
        public abstract void Continue(bool blockUntilFinished = false);
        public abstract void Dispose();
    }
}
