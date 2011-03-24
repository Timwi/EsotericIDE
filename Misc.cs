using System;

namespace EsotericIDE
{
    public sealed class ParseException : Exception
    {
        public int Index { get; private set; }
        public int Count { get; private set; }
        public ParseException(string message, int index, int count) : base(message) { Index = index; Count = count; }
    }

    sealed class Position
    {
        public int Index { get; private set; }
        public int Count { get; private set; }
        public Position(int index, int count) { Index = index; Count = count; }
    }
}
