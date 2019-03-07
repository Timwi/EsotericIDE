using System;

namespace EsotericIDE.Sclipting
{
    sealed class Mark { }

    sealed class Function
    {
        public FunctionNode FunctionCode;
        public object CapturedItem;
    }

    [Serializable]
    sealed class DuplicateCharacterException : Exception
    {
        public char Character { get; private set; }
        public DuplicateCharacterException(char chr, string message) : base(message) { Character = chr; }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    sealed class InstructionAttribute : Attribute
    {
        public char Character { get; private set; }
        public string Engrish { get; private set; }
        public string Description { get; private set; }
        public NodeType Type { get; private set; }
        public InstructionGroup Group { get; private set; }
        public InstructionAttribute(char character, string engrish, string description, NodeType type, InstructionGroup group)
        {
            Character = character;
            Engrish = engrish;
            Description = description;
            Type = type;
            Group = group;
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    sealed class InstructionGroupAttribute : Attribute
    {
        public string Label { get; private set; }
        public InstructionGroupAttribute(string label) { Label = label; }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    sealed class ListStringInstructionAttribute : Attribute
    {
        public string MenuLabel { get; private set; }
        public ListStringInstructionAttribute(string menuLabel) { MenuLabel = menuLabel; }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    sealed class SingularListStringInstructionAttribute : Attribute
    {
        public ListStringInstruction Instruction { get; private set; }
        public bool Backwards { get; private set; }
        public SingularListStringInstructionAttribute(ListStringInstruction instruction, bool backwards)
        {
            Instruction = instruction;
            Backwards = backwards;
        }
    }
}