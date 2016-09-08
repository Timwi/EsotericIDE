using System;

namespace EsotericIDE.Whitespace
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
    sealed class InstructionAttribute : Attribute
    {
        public InstructionAttribute(string explain, string instr) { Explain = explain; Instr = instr; Arg = null; }
        public InstructionAttribute(string explain, string instr, ArgKind arg) { Explain = explain; Instr = instr; Arg = arg; }
        public string Explain { get; private set; }
        public string Instr { get; private set; }
        public ArgKind? Arg { get; private set; }
    }
}
