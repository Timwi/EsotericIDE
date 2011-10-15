using System;
using RT.Util.ExtensionMethods;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace EsotericIDE.Languages
{
    partial class Sclipting
    {
        private sealed class regexMatch
        {
            public string Input { get; private set; }
            public string Regex { get; private set; }
            public Match Match { get; private set; }
            public int Offset { get; private set; }
            public regexMatch(string input, string regex, int offset, Match match)
            {
                Input = input;
                Regex = regex;
                Offset = offset;
                Match = match;
            }
            public void AppendDescription(int index, StringBuilder sb)
            {
                sb.AppendLine("{0,2}. {1}".Fmt(index, Regex));
                sb.AppendLine("    {0}".Fmt(Input));
                if (Match != null)
                {
                    sb.Append(new string(' ', Match.Index + Offset + 4));
                    sb.AppendLine(new string('^', Match.Length));
                }
                else
                    sb.AppendLine();
            }
        }
    }
}