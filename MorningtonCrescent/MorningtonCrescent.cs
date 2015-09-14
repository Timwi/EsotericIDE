using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using EsotericIDE.MorningtonCrescent;

namespace EsotericIDE.Languages
{
    sealed class MorningtonCrescent : ProgrammingLanguage
    {
        public override string LanguageName { get { return "Mornington Crescent"; } }
        public override string DefaultFileExtension { get { return "mcresc"; } }
        public override string GetInfo(string source, int cursorPosition) { return ""; }
        public override System.Windows.Forms.ToolStripMenuItem[] CreateMenus(Func<string> getSelectedText, Action<string> insertText) { return new System.Windows.Forms.ToolStripMenuItem[0]; }

        public override ExecutionEnvironment Compile(string source, string input)
        {
            var sourceLines = new List<Tuple<string, int>>();
            var index = 0;
            Match m;
            while ((m = Regex.Match(source, @"\r?\n")).Success)
            {
                sourceLines.Add(new Tuple<string, int>(source.Substring(0, m.Index), index));
                index += m.Index + m.Length;
                source = source.Substring(m.Index + m.Length);
            }
            if (source.Length > 0)
                sourceLines.Add(new Tuple<string, int>(source, index));
            return new MCEnvironment(sourceLines.ToArray(), input);
        }
    }
}
