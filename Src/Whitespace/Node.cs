using System;
using System.Collections.Generic;
using System.Numerics;
using RT.Util.ExtensionMethods;

namespace EsotericIDE.Whitespace
{
    sealed class Node
    {
        public Position Position { get; private set; }
        public Instruction Instruction { get; private set; }
        public BigInteger? Arg { get; private set; }
        public string Label { get; private set; }

        public Node(Position pos, Instruction instruction, BigInteger? arg = null, string label = null)
        {
            Position = pos;
            Instruction = instruction;
            Arg = arg;
            Label = label;
        }

        public override string ToString()
        {
            var ret = Instruction.ToString();
            if (Arg != null)
                ret += $" {Arg}";
            if (Label != null)
                ret += $@" ""{Label}""";
            return ret;
        }
    }
}
