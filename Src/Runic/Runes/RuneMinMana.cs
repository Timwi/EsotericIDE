using System;
using EsotericIDE.Runic.Math;

namespace EsotericIDE.Runic.Runes
{
    public class RuneMinMana : IExecutableRune
    {
        public bool Execute(Pointer pointer, IRunicContext context)
        {
            var o = pointer.Pop();
            if (o is ValueType)
            {
                ValueType v = (ValueType) o;
                if (MathHelper.Compare(pointer.GetMana(), v) >= 0)
                    return true;
                pointer.Push(o);
            }
            return false;
        }

        public IExecutableRune Register()
        {
            RuneRegistry.ALL_RUNES.Add('M', this);
            return this;
        }
    }
}