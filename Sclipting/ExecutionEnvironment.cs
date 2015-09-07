using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace EsotericIDE.Languages
{
    partial class Sclipting
    {
        private sealed class scliptingExecutionEnvironment : ExecutionEnvironment
        {
            public List<object> CurrentStack = new List<object>();
            public List<regexMatch> RegexObjects = new List<regexMatch>();
            private program _program;
            public string Input { get; private set; }

            public scliptingExecutionEnvironment(program program, string input)
            {
                _program = program;
                _runner = null;
                Input = input;
            }

            public void NumericOperation(Func<BigInteger, object> fromInt, Func<double, object> fromDouble)
            {
                var item = Sclipting.ToNumeric(Pop());

                if (item is double)
                    CurrentStack.Add(fromDouble((double) item));
                else
                    CurrentStack.Add(fromInt((BigInteger) item));
            }

            public void NumericOperation(Func<BigInteger, BigInteger, object> intInt, Func<double, double, object> doubleDouble)
            {
                var item2 = Sclipting.ToNumeric(Pop());
                var item1 = Sclipting.ToNumeric(Pop());

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
                if (CurrentStack.Count == 0)
                    throw new InvalidOperationException("Stack underrun (attempt to pop item from empty stack).");
                var i = CurrentStack.Count - 1;
                var item = CurrentStack[i];
                CurrentStack.RemoveAt(i);
                return item;
            }

            public void GenerateOutput()
            {
                var index = CurrentStack.Count;
                while (index > 0 && !(CurrentStack[index - 1] is mark))
                    index--;
                for (; index < CurrentStack.Count; index++)
                    _output.Append(Sclipting.ToString(CurrentStack[index]));
            }

            public override string DescribeExecutionState()
            {
                var sb = new StringBuilder();
                if (RegexObjects.Any())
                {
                    sb.AppendLine("Active regular expression blocks:");
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

            private string describe(object item)
            {
                byte[] b;
                List<object> list;
                function fnc;

                if ((b = item as byte[]) != null)
                    return "Byte array: “{0}” ({1})".Fmt(b.FromUtf8().CLiteralEscape(), Sclipting.ToInt(b));
                else if (item is BigInteger)
                    return "Integer: {0}".Fmt(item);
                else if (item is string)
                    return "String: “{0}”".Fmt(((string) item).CLiteralEscape());
                else if (item is double)
                    return "Float: {0}".Fmt(ExactConvert.ToString((double) item));
                else if (item is mark)
                    return "Mark";
                else if ((list = item as List<object>) != null)
                    return "List ({0} items)".Fmt(list.Count) + list.Select((itm, idx) => Environment.NewLine + describe(itm, idx)).JoinString().Indent(4, false);
                else if ((fnc = item as function) != null)
                    return "Function" + (fnc.CapturedItem != null ? "; capture: " + describe(fnc.CapturedItem) : "; no capture");

                // unknown type of object?
                return "{0} ({1})".Fmt(item, item.GetType().FullName);
            }

            private string describe(object item, int index)
            {
                return "{0,2}.  {1}".Fmt(index, describe(item));
            }

            protected override IEnumerable<Position> GetProgram()
            {
                return _program.Execute(this);
            }
        }

        private sealed class function
        {
            public functionNode FunctionCode;
            public object CapturedItem;
        }
    }
}