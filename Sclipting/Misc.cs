using System;

namespace EsotericIDE.Languages
{
    partial class Sclipting
    {
        private sealed class mark { }

        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = false)]
        private sealed class instructionAttribute : Attribute
        {
            public char Character { get; private set; }
            public string Engrish { get; private set; }
            public string Description { get; private set; }
            public nodeType Type { get; private set; }
            public instructionAttribute(char character, string engrish, string description, nodeType type)
            {
                Character = character;
                Engrish = engrish;
                Description = description;
                Type = type;
            }
        }
    }
}