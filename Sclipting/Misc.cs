using System;

namespace EsotericIDE.Languages
{
    partial class Sclipting
    {
        private sealed class mark { }

        [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
        sealed class instructionAttribute : Attribute
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

        [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
        sealed class listStringInstructionAttribute : Attribute
        {
            public string MenuLabel { get; private set; }
            public listStringInstructionAttribute(string menuLabel) { MenuLabel = menuLabel; }
        }

        [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
        sealed class singularListStringInstructionAttribute : Attribute
        {
            public ListStringInstruction Instruction { get; private set; }
            public bool Backwards { get; private set; }
            public singularListStringInstructionAttribute(ListStringInstruction instruction, bool backwards)
            {
                Instruction = instruction;
                Backwards = backwards;
            }
        }
    }
}