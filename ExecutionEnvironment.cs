using System.Collections.Generic;
using System.Text;
using System;
using System.Threading;

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

        protected StringBuilder _output = new StringBuilder();
        public string Output { get { lock (_locker) return _output.ToString(); } }

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

        protected ManualResetEvent _resetEvent = new ManualResetEvent(false);
        protected Thread _runner;

        public void Continue(bool blockUntilFinished = false)
        {
            if (State == ExecutionState.Finished)
            {
                _resetEvent.Reset();
                return;
            }

            if (_runner == null)
            {
                _runner = new Thread(() =>
                {
                    run();
                    _runner = null;
                });
                _runner.Start();
            }

            _resetEvent.Set();
        }

        public virtual void Dispose()
        {
            State = ExecutionState.Stop;
            if (_resetEvent != null)
                ((IDisposable) _resetEvent).Dispose();
        }

        public abstract string DescribeExecutionState();
        protected abstract void run();
    }
}
