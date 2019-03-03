using System;
using EsotericIDE.Runic.Math;

namespace EsotericIDE.Runic.Runes
{
    public class RuneEval : IExecutableRune
    {
        public bool Execute(Pointer pointer, IRunicContext context)
        {
            object o = pointer.Pop();
            if (o is ValueType)
            {
                int j = (int) MathHelper.GetValue((ValueType) o);
                string s = WordDictionary.GetWord(j);
                if (s != null)
                    pointer.Push(s);
            }
            else if (o is string)
            {
                string str = (string) o;
                Func<bool> execution = context.Eval(pointer, str, out int size);
                if (size > 5)
                {
                    double n = System.Math.Log(size - 5);
                    pointer.DeductMana((int) System.Math.Floor(n * n));
                }
                pointer.Push(execution);
                return false;
            }
            else if (o is Func<bool>)
            {
                Func<bool> isDone = (Func<bool>) o;
                bool ret = isDone();
                if (!ret)
                    pointer.Push(o);
                return ret;
            }
            return true;
        }

        public IExecutableRune Register()
        {
            RuneRegistry.ALL_RUNES.Add('E', this);
            return this;
        }
    }
}