using System;
using EsotericIDE.Runic.Math;

namespace EsotericIDE.Runic.Runes
{
    public class RuneToChar : IExecutableRune
    {
        public bool Execute(Pointer pointer, IRunicContext context)
        {
            object o = pointer.Pop();
            if (o is ValueType)
            {
                ValueType v = (ValueType) o;
                int i = (int) MathHelper.GetValue(v);
                char c = (char) i;
                pointer.Push(c);
            }
            else if (o is string)
            {
                string s = (string) o;
                if (s.Length == 1)
                {
                    pointer.Push(s[0]);
                }
            }
            return true;
        }

        public IExecutableRune Register()
        {
            RuneRegistry.ALL_RUNES.Add('k', this);
            return this;
        }
    }
}