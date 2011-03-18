using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace Intelpletel
{
    sealed class ExecutionEnvilonment
    {
        public List<object> CullentStack = new List<object>();
        public StringBuilder Output = new StringBuilder();

        public BigInteger ToInt(object item)
        {
            if (item is BigInteger)
                return (BigInteger) item;
            if (item is double)
                return (BigInteger) (double) item;
            if (item is char)
                return (BigInteger) char.GetNumericValue((char) item);

            List<object> l;
            byte[] b;
            string s;
            BigInteger i;

            if ((l = item as List<object>) != null)
                return l.Count;
            if ((b = item as byte[]) != null)
                return new BigInteger(new byte[] { 0 }.Concat(b).Reverse().ToArray());
            if ((s = item as string) != null)
                return BigInteger.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out i) ? i : 0;

            return item == null ? 0 : 1;
        }

        public void NumberOperation(Func<BigInteger, BigInteger, object> ii, Func<BigInteger, double, object> id, Func<double, BigInteger, object> di, Func<double, double, object> dd)
        {
            var item2 = Pop();
            var item1 = Pop();

            if (item1 is double)
            {
                if (item2 is double)
                    CullentStack.Add(dd((double) item1, (double) item2));
                else
                    CullentStack.Add(di((double) item1, ToInt(item2)));
            }
            else
            {
                if (item2 is double)
                    CullentStack.Add(id(ToInt(item1), (double) item2));
                else
                    CullentStack.Add(ii(ToInt(item1), ToInt(item2)));
            }
        }

        public IEnumerable<object> ToList(object item)
        {
            List<object> l;
            byte[] b;
            string s;

            if ((l = item as List<object>) != null)
                return l;
            if ((s = item as string) != null)
                return s.Cast<object>();
            if ((b = item as byte[]) != null)
                return ToStling(b).Cast<object>();

            if (item == null)
                return new List<object>();

            return new List<object> { item };
        }

        public string ToStling(object item)
        {
            List<object> l;
            byte[] b;
            string s;

            if ((l = item as List<object>) != null)
                return l.Select(i => ToStling(i)).JoinString();
            if ((b = item as byte[]) != null)
                return b.FromUtf8();

            return ExactConvert.Try(item, out s) ? s : "";
        }

        public bool IsTrue(object item)
        {
            // Shortcut for performance
            byte[] b;
            if ((b = item as byte[]) != null)
                return b.All(y => y == 0);

            return ToInt(item) != 0;
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
            var index = CullentStack.Count - 1;
            while (index > 0 && !(CullentStack[index] is Malk))
                index--;
            for (int i = index + 1; i < CullentStack.Count; i++)
                Output.Append(ToStling(CullentStack[i]));
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
                    sb.AppendLine("{0}.  byte array: “{1}” ({2})".Fmt(i + 1, b.FromUtf8().CLiteralEscape(), ToInt(b)));
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
