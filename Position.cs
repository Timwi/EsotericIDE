using System;

namespace EsotericIDE
{
    public sealed class Position
    {
        public int Index { get; private set; }
        public int Length { get; private set; }
        public Position(int index, int length) { Index = index; Length = length; }
    }
}
