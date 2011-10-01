using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace EsotericIDE.Sclipting
{
    sealed class ScliptingExecutionEnvironment : ExecutionEnvironment
    {
        public List<object> CurrentStack = new List<object>();
        public List<RegexMatch> RegexObjects = new List<RegexMatch>();
        private Translation _tr;
        private ScliptingLanguage _language;
        private ScliptingProgram _program;
        private ManualResetEvent _resetEvent = new ManualResetEvent(false);
        private Thread _runner;

        public ScliptingExecutionEnvironment(string program, Translation tr, ScliptingLanguage language)
        {
            _tr = tr;
            _language = language;
            _program = language.Parse(program);
            _runner = null;
        }

        public void NumericOperation(Func<BigInteger, BigInteger, object> intInt, Func<double, double, object> doubleDouble)
        {
            var item2 = ScliptingLanguage.ToNumeric(Pop());
            var item1 = ScliptingLanguage.ToNumeric(Pop());

            if (item1 is double)
            {
                if (item2 is double)
                    CurrentStack.Add(doubleDouble((double) item1, (double) item2));
                else
                    CurrentStack.Add(doubleDouble((double) item1, (double) (BigInteger) item2));
            }
            else
            {
                if (item2 is double)
                    CurrentStack.Add(doubleDouble((double) (BigInteger) item1, (double) item2));
                else
                    CurrentStack.Add(intInt((BigInteger) item1, (BigInteger) item2));
            }
        }

        public object Pop()
        {
            var i = CurrentStack.Count - 1;
            var item = CurrentStack[i];
            CurrentStack.RemoveAt(i);
            return item;
        }

        public void GenerateOutput()
        {
            var index = CurrentStack.Count;
            while (index > 0 && !(CurrentStack[index - 1] is Mark))
                index--;
            for (; index < CurrentStack.Count; index++)
                _output.Append(ScliptingLanguage.ToString(CurrentStack[index]));
        }

        public override string DescribeExecutionState()
        {
            var sb = new StringBuilder();
            if (RegexObjects.Any())
            {
                sb.AppendLine(_tr.ActiveRegexBlocks);
                for (int i = 0; i < RegexObjects.Count; i++)
                    RegexObjects[i].AppendDescription(i, sb);
                sb.AppendLine();
            }

            for (int i = CurrentStack.Count - 1; i >= 0; i--)
                sb.AppendLine(describe(CurrentStack[i], i + 1));
            if (sb.Length > 0)
                sb.Remove(sb.Length - Environment.NewLine.Length, Environment.NewLine.Length);
            return sb.ToString();
        }

        private string describe(object item, int index)
        {
            byte[] b;
            List<object> list;

            if ((b = item as byte[]) != null)
                return "{0,2}.  {3} “{1}” ({2})".Fmt(index, b.FromUtf8().CLiteralEscape(), ScliptingLanguage.ToInt(b), _tr.ByteArray);
            else if (item is BigInteger)
                return "{0,2}.  {2} {1}".Fmt(index, item, _tr.Integer);
            else if (item is string)
                return "{0,2}.  {2} “{1}”".Fmt(index, ((string) item).CLiteralEscape(), _tr.String);
            else if (item is double)
                return "{0,2}.  {2} {1}".Fmt(index, ExactConvert.ToString((double) item), _tr.Float);
            else if (item is Mark)
                return "{0,2}.  {1}".Fmt(index, _tr.Mark);
            else if ((list = item as List<object>) != null)
                return "{0,2}.  list ({1} items)".Fmt(index, list.Count) + list.Select((itm, idx) => Environment.NewLine + describe(itm, idx)).JoinString().Indent(4, false);

            // unknown type of object?
            return "{0,2}.  {1} ({2})".Fmt(index, item, item.GetType().FullName);
        }

        private void run()
        {
            if (State == ExecutionState.Finished)
            {
                _resetEvent.Reset();
                return;
            }

            using (var instructionPointer = _program.Execute(this).GetEnumerator())
            {
                bool canceled = false;
                while (instructionPointer.MoveNext())
                {
                    bool breakHere = false;
                    lock (_breakpoints)
                        breakHere = _breakpoints.Any(bp => bp >= instructionPointer.Current.Index && bp < instructionPointer.Current.Index + Math.Max(instructionPointer.Current.Count, 1));
                    if (breakHere || State == ExecutionState.Debugging)
                    {
                        fireDebuggerBreak(instructionPointer.Current);
                        _resetEvent.Reset();
                        _resetEvent.WaitOne();
                        continue;
                    }

                    if (State == ExecutionState.Running)
                        continue;
                    else
                    {
                        if (State == ExecutionState.Stop)
                            canceled = true;
                        goto finished;
                    }
                }

                finished:
                fireExecutionFinished(canceled);
                State = ExecutionState.Finished;
                _resetEvent.Reset();
                _runner = null;
            }
        }

        public override void Continue(bool blockUntilFinished = false)
        {
            if (State == ExecutionState.Finished)
            {
                _resetEvent.Reset();
                return;
            }

            if (_runner == null)
            {
                _runner = new Thread(run);
                _runner.Start();
            }

            _resetEvent.Set();
        }

        public override void Dispose()
        {
            State = ExecutionState.Stop;
            if (_resetEvent != null)
                ((IDisposable) _resetEvent).Dispose();
        }
    }
}