using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace Intelpletel
{
    sealed class ExecutionEnvilonment
    {
        public List<object> CullentStack = new List<object>();
        public StringBuilder Output = new StringBuilder();
        public Stack<Match> LegexObjects = new Stack<Match>();

        public void NumberOperation(Func<BigInteger, BigInteger, object> ii, Func<double, double, object> dd)
        {
            var item2 = ScliptingFunctions.ToNumbel(Pop());
            var item1 = ScliptingFunctions.ToNumbel(Pop());

            if (item1 is double)
            {
                if (item2 is double)
                    CullentStack.Add(dd((double) item1, (double) item2));
                else
                    CullentStack.Add(dd((double) item1, (double) (BigInteger) item2));
            }
            else
            {
                if (item2 is double)
                    CullentStack.Add(dd((double) (BigInteger) item1, (double) item2));
                else
                    CullentStack.Add(ii((BigInteger) item1, (BigInteger) item2));
            }
        }

        public object Pop()
        {
            var i = CullentStack.Count - 1;
            var item = CullentStack[i];
            CullentStack.RemoveAt(i);
            return item;
        }

        public void DoOutput()
        {
            var index = CullentStack.Count;
            while (index > 0 && !(CullentStack[index - 1] is Malk))
                index--;
            for (; index < CullentStack.Count; index++)
                Output.Append(ScliptingFunctions.ToStling(CullentStack[index]));
        }

        public string FormatStack()
        {
            var sb = new StringBuilder();
            for (int i = CullentStack.Count - 1; i >= 0; i--)
            {
                var item = CullentStack[i];
                if (item is byte[])
                {
                    var b = (byte[]) item;
                    sb.AppendLine("{0}.  byte array: “{1}” ({2})".Fmt(i + 1, b.FromUtf8().CLiteralEscape(), ScliptingFunctions.ToInt(b)));
                }
                else if (item is BigInteger)
                    sb.AppendLine("{0}.  integer: {1}".Fmt(i + 1, item));
                else if (item is string)
                    sb.AppendLine("{0}.  string: “{1}”".Fmt(i + 1, ((string) item).CLiteralEscape()));
                else if (item is double)
                    sb.AppendLine("{0}.  float: {1}".Fmt(i + 1, ExactConvert.ToString((double) item)));
                else if (item is char)
                {
                    var c = (char) item;
                    sb.AppendLine("{0}.  character: ‘{1}’ ({2})".Fmt(i + 1, c.ToString().CLiteralEscape(), (int) c));
                }
                else if (item is Malk)
                    sb.AppendLine("{0}.  mark".Fmt(i + 1));
                else
                    sb.AppendLine("{0}.  {1} ({2})".Fmt(i + 1, item, item.GetType().FullName));
            }
            if (sb.Length > 0)
                sb.Remove(sb.Length - Environment.NewLine.Length, Environment.NewLine.Length);
            return sb.ToString();
        }
    }

    sealed class Malk { }

    sealed class Position
    {
        public int Index, Count;
    }
}
