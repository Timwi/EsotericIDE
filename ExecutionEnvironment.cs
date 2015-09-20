using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public event Action<bool, RuntimeError> ExecutionFinished;
        protected void fireExecutionFinished(bool canceled, RuntimeError error) { if (ExecutionFinished != null) ExecutionFinished(canceled, error); }

        public event Action BreakpointsChanged;

        // This StringBuilder can be safely appended to in derived classes.
        protected StringBuilder _output = new StringBuilder();
        // This property is only called when the esolang is not executing, so there is no race condition.
        public string Output { get { return _output.ToString(); } }

        // This List is accessed from derived classes as well as the WinForms code. Therefore, always lock on _locker when accessing it.
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
                    if (State == ExecutionState.Finished)
                    {
                        _resetEvent.Reset();
                        return;
                    }

                    try
                    {
                        Run();
                    }
                    catch (Exception e)
                    {
                        var type = e.GetType();
                        var error = new RuntimeError(null, e.Message + (type != typeof(Exception) ? " (" + type.Name + ")" : ""));
                        fireExecutionFinished(true, error);
                        State = ExecutionState.Finished;
                        _resetEvent.Reset();
                    }

                    _runner = null;
                });
                _runner.Start();
            }

            _resetEvent.Set();
            if (blockUntilFinished)
                while (State != ExecutionState.Finished)
                    _resetEvent.WaitOne();
        }

        public virtual void Dispose()
        {
            State = ExecutionState.Stop;
            if (_resetEvent != null)
                ((IDisposable) _resetEvent).Dispose();
        }

        public abstract string DescribeExecutionState();

        protected virtual IEnumerable<Position> GetProgram()
        {
            throw new NotImplementedException("Your Environment must override either the Run() method or the GetProgram() method.");
        }

        protected virtual void Run()
        {
            using (var instructionPointer = GetProgram().GetEnumerator())
            {
                bool canceled = false;
                RuntimeError error = null;
                try
                {
                    while (instructionPointer.MoveNext())
                    {
                        switch (State)
                        {
                            case ExecutionState.Running:
                                lock (_locker)
                                    if (_breakpoints.Any(bp => bp >= instructionPointer.Current.Index && bp < instructionPointer.Current.Index + Math.Max(instructionPointer.Current.Length, 1)))
                                        goto case ExecutionState.Debugging;
                                continue;
                            case ExecutionState.Debugging:
                                fireDebuggerBreak(instructionPointer.Current);
                                _resetEvent.Reset();
                                _resetEvent.WaitOne();
                                if (State == ExecutionState.Stop)
                                    goto case ExecutionState.Stop;
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
                    var type = e.GetType();
                    error = new RuntimeError(instructionPointer.Current, e.Message + (type != typeof(Exception) ? " (" + type.Name + ")" : ""));
                }

                finished:
                fireExecutionFinished(canceled, error);
                State = ExecutionState.Finished;
                _resetEvent.Reset();
            }
        }
    }
}
