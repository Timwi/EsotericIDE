using EsotericIDE.Runic.Math;

namespace EsotericIDE.Runic.Runes
{
    public class RuneReverse : IExecutableRune
    {
        public bool Execute(Pointer pointer, IRunicContext context)
        {
            char modifier = context.GetModifier(pointer.position.x, pointer.position.y);
            if (modifier == '̹' || modifier == '͗')
            {
                pointer.ReverseStacksStack();
            }
            else if (modifier == '͍')
            {
                object o = pointer.Pop();
                if (o is string)
                {
                    string s = (string) o;
                    s = s.Reverse();
                    pointer.Push(s);
                }
                else
                {
                    pointer.Push(o);
                }
            }
            else
            {
                pointer.ReverseStack();
            }
            return true;
        }

        public IExecutableRune Register()
        {
            RuneRegistry.ALL_RUNES.Add('r', this);
            return this;
        }
    }
}