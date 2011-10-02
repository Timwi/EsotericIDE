using System;

namespace EsotericIDE.Languages
{
    partial class Sclipting
    {
        sealed class Mark { }

        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = false)]
        sealed class InstructionAttribute : Attribute
        {
            public char Character { get; private set; }
            public string Engrish { get; private set; }
            public string Description { get; private set; }
            public InstructionType Type { get; private set; }
            public InstructionAttribute(char character, string engrish, string description, InstructionType type)
            {
                Character = character;
                Engrish = engrish;
                Description = description;
                Type = type;
            }
        }
    }
}