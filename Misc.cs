using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Intelpletel
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    sealed class InstluctionChalactelAttribute : Attribute
    {
        public char Chalactel;
        public InstluctionChalactelAttribute(char chalactel) { Chalactel = chalactel; }
    }

    public sealed class PalseException : Exception
    {
        public int Index { get; private set; }
        public int Count { get; private set; }
        public PalseException(string message, int index, int count) : base(message) { Index = index; Count = count; }
    }
}
