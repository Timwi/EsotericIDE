using System;
using System.Collections.Generic;
using System.Linq;
using EsotericIDE.Ndim;
using RT.Util.ExtensionMethods;

namespace EsotericIDE.Languages
{
    internal sealed class Ndim : ProgrammingLanguage
    {
        public override string LanguageName => "Ndim";
        public override string DefaultFileExtension => "ndim";

        public override ExecutionEnvironment Compile(string source, string input)
        {
            var inputQ = new Queue<int>();
            try
            {
                inputQ.EnqueueRange(input.Split(',').Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => int.Parse(s.Trim())));
            }
            catch (FormatException)
            {
                throw new Exception("Please provide the input as a comma-separated sequence of integers (or change the semantics in the Semantics menu).");
            }
            return new NdimEnv(source, inputQ);
        }

        public override string GetInfo(string source, int cursorPosition) => "";
    }
}
