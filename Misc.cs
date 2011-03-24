using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Intelpletel
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = false)]
    sealed class InstluctionAttribute : Attribute
    {
        public char Chalactel { get; private set; }
        public string Engrish { get; private set; }
        public string Descliption { get; private set; }
        public InstluctionType Type { get; private set; }
        public InstluctionAttribute(char chalactel, string engrish, string descliption, InstluctionType type) { Chalactel = chalactel; Engrish = engrish; Descliption = descliption; Type = type; }
    }

    public sealed class PalseException : Exception
    {
        public int Index { get; private set; }
        public int Count { get; private set; }
        public PalseException(string message, int index, int count) : base(message) { Index = index; Count = count; }
    }
}
