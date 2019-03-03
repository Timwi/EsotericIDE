namespace EsotericIDE.Runic.Runes
{
    public class RuneLength : IExecutableRune
    {
        public bool Execute(Pointer pointer, IRunicContext context)
        {
            char modifier = context.GetModifier(pointer.position.x, pointer.position.y);
            if (modifier == '̹' || modifier == '͗')
            {
                pointer.Push(pointer.GetStacksStackSize());
            }
            else if (modifier == '͍')
            {
                object o = pointer.Pop();
                if (o is string)
                {
                    string s = (string) o;
                    pointer.Push(s.Length);
                }
                else
                {
                    pointer.Push(o);
                }
            }
            else
            {
                pointer.Push(pointer.GetStackSize());
            }
            return true;
        }

        public IExecutableRune Register()
        {
            RuneRegistry.ALL_RUNES.Add('l', this);
            return this;
        }
    }
}