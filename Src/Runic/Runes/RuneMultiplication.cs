using System;
using System.Text;
using EsotericIDE.Runic.Math;

namespace EsotericIDE.Runic.Runes
{
    public class RuneMultiplication : IExecutableRune
    {
        public bool Execute(Pointer pointer, IRunicContext context)
        {
            object a = pointer.Pop();
            object b = pointer.Pop();
            if (a != null && b != null)
            {
                if (a is ValueType && b is ValueType)
                {
                    if (MathHelper.IsInteger((ValueType) a) && MathHelper.IsInteger((ValueType) b))
                    {
                        int c = (int) a * (int) b;
                        pointer.Push(c);
                    }
                    else if (a is Vector3 || b is Vector3)
                    {
                        if (a is Vector3 && b is Vector3)
                        {
                            pointer.Push(Vector3.Cross((Vector3) a, (Vector3) b));
                        }
                        else if (a is Vector3)
                        {
                            pointer.Push(((Vector3) a) * (float) MathHelper.GetValue((ValueType) b));
                        }
                        else if (b is Vector3)
                        {
                            pointer.Push(((Vector3) b) * (float) MathHelper.GetValue((ValueType) a));
                        }
                    }
                    else
                    {
                        double c = MathHelper.GetValue((ValueType) a) * MathHelper.GetValue((ValueType) b);
                        pointer.Push(c);
                    }
                }
                else if (b is string && a is ValueType)
                {
                    StringBuilder sb = new StringBuilder((string) b);
                    int m = (int) MathHelper.GetValue((ValueType) a);
                    for (int i = 1; i < m; i++)
                    {
                        sb.Append((string) b);
                    }
                    pointer.Push(sb.ToString());
                }
            }
            return true;
        }

        public IExecutableRune Register()
        {
            RuneRegistry.ALL_RUNES.Add('*', this);
            return this;
        }
    }
}