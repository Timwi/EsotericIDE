using System;
using System.Collections.Generic;
using System.Numerics;

namespace EsotericIDE.Whitespace
{
    sealed class ParseInfoException : Exception
    {
        public Instruction Instruction { get; private set; }
        public List<bool> RawArg { get; private set; }
        public BigInteger? NumberArg { get; private set; }
        public string LabelArg { get; private set; }

        public ParseInfoException(Instruction instr, List<bool> rawArg, BigInteger? numberArg, string labelArg)
        {
            Instruction = instr;
            RawArg = rawArg;
            NumberArg = numberArg;
            LabelArg = labelArg;
        }
    }
}
