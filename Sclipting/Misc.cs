using System;

namespace EsotericIDE.Languages
{
    partial class Sclipting
    {
        private sealed class mark { }

        [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
        private sealed class instructionAttribute : Attribute
        {
            public char Character { get; private set; }
            public string Engrish { get; private set; }
            public string Description { get; private set; }
            public nodeType Type { get; private set; }
            public instructionGroup Group { get; private set; }
            public instructionAttribute(char character, string engrish, string description, nodeType type, instructionGroup group)
            {
                Character = character;
                Engrish = engrish;
                Description = description;
                Type = type;
                Group = group;
            }
        }

        [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
        sealed class instructionGroupAttribute : Attribute
        {
            public string Label { get; private set; }
            public instructionGroupAttribute(string label) { Label = label; }
        }
    }
}