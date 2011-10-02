using System;

namespace EsotericIDE
{
    public sealed class ParseException : Exception
    {
        public int Index { get; private set; }
        public int Length { get; private set; }
        public ParseException(string message, int index, int length) : base(message) { Index = index; Length = length; }
    }

    public sealed class Position
    {
        public int Index { get; private set; }
        public int Length { get; private set; }
        public Position(int index, int length) { Index = index; Length = length; }
    }
}
