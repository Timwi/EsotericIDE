using System;

namespace EsotericIDE
{
    public sealed class CompileException : Exception
    {
        public int? Index { get; private set; }
        public int? Length { get; private set; }
        public CompileException(string message, int? index = null, int? length = null)
            : base(message)
        {
            Index = index;
            Length = length;
        }
    }
}
