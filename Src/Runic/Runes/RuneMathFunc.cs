using System;
using EsotericIDE.Runic.Math;

namespace EsotericIDE.Runic.Runes
{
    class RuneMathFunc : IExecutableRune
    {
        public bool Execute(Pointer pointer, IRunicContext context)
        {
            object o = pointer.Pop();
            object v = pointer.Pop();
            if (o is ValueType && v is ValueType)
            {
                char c = (char) MathHelper.GetValue((ValueType) o);
                double x = MathHelper.GetValue((ValueType) v);
                switch (c)
                {
                    case 'C':
                        pointer.Push(System.Math.Cos(x));
                        break;
                    case 'S':
                        pointer.Push(System.Math.Sin(x));
                        break;
                    case 'T':
                        pointer.Push(System.Math.Tan(x));
                        break;
                    case 'e':
                        pointer.Push(System.Math.Exp(x));
                        break;
                    case 'l':
                        pointer.Push(System.Math.Log(x));
                        break;
                    case 'L':
                        pointer.Push(System.Math.Log10(x));
                        break;
                    case 'f':
                        pointer.Push(System.Math.Floor(x));
                        break;
                    case 'c':
                        pointer.Push(System.Math.Ceiling(x));
                        break;
                    case 'r':
                        pointer.Push(System.Math.Round(x));
                        break;
                    case '|':
                        pointer.Push(System.Math.Abs(x));
                        break;
                    case 'q':
                        pointer.Push(System.Math.Sqrt(x));
                        break;
                    case 'i':
                        pointer.Push(System.Math.Asin(x));
                        break;
                    case 'o':
                        pointer.Push(System.Math.Acos(x));
                        break;
                    case 'a':
                        pointer.Push(System.Math.Atan(x));
                        break;
                    case 'R':
                        Random rand = new Random();
                        pointer.Push(rand.Next((int) x));
                        break;
                }
            }
            return true;
        }

        public IExecutableRune Register()
        {
            RuneRegistry.ALL_RUNES.Add('A', this);
            return this;
        }
    }
}