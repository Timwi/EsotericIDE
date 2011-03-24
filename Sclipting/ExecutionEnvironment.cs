using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace EsotericIDE.Sclipting
{
    sealed class ScliptingExecutionEnvironment : ExecutionEnvironment
    {
        public List<object> CurrentStack = new List<object>();
        public Stack<Match> RegexObjects = new Stack<Match>();
        private Translation _tr;
        private ScliptingLanguage _language;

        public ScliptingExecutionEnvironment(string program, Translation tr, ScliptingLanguage language)
        {
            _tr = tr;
            _language = language;
            InstructionPointer = language.Parse(program).Execute(this).GetEnumerator();
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
                Output.Append(ScliptingLanguage.ToString(CurrentStack[index]));
        }

        public override string DescribeExecutionState()
        {
            var sb = new StringBuilder();
            for (int i = CurrentStack.Count - 1; i >= 0; i--)
            {
                var item = CurrentStack[i];
                if (item is byte[])
                {
                    var b = (byte[]) item;
                    sb.AppendLine("{0}.  {3} “{1}” ({2})".Fmt(i + 1, b.FromUtf8().CLiteralEscape(), ScliptingLanguage.ToInt(b), _tr.ByteArray));
                }
                else if (item is BigInteger)
                    sb.AppendLine("{0}.  {2} {1}".Fmt(i + 1, item, _tr.Integer));
                else if (item is string)
                    sb.AppendLine("{0}.  {2} “{1}”".Fmt(i + 1, ((string) item).CLiteralEscape(), _tr.String));
                else if (item is double)
                    sb.AppendLine("{0}.  {2} {1}".Fmt(i + 1, ExactConvert.ToString((double) item), _tr.Float));
                else if (item is char)
                {
                    var c = (char) item;
                    sb.AppendLine("{0}.  {3} ‘{1}’ ({2})".Fmt(i + 1, c.ToString().CLiteralEscape(), (int) c, _tr.Character));
                }
                else if (item is Mark)
                    sb.AppendLine("{0}.  {1}".Fmt(i + 1, _tr.Mark));
                else
                    sb.AppendLine("{0}.  {1} ({2})".Fmt(i + 1, item, item.GetType().FullName));
            }
            if (sb.Length > 0)
                sb.Remove(sb.Length - Environment.NewLine.Length, Environment.NewLine.Length);
            return sb.ToString();
        }
    }
}
