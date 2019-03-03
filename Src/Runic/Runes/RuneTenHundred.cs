using System;

namespace EsotericIDE.Runic.Runes
{
    public class RuneTenHundred : IExecutableRune
    {
        int value;
        char c;
        private RuneMultiplication multi = new RuneMultiplication();
        private RuneDivision divide = new RuneDivision();
        public RuneTenHundred(int v, char c)
        {
            value = v;
            this.c = c;
        }
        public bool Execute(Pointer pointer, IRunicContext context)
        {
            object o = pointer.Pop();
            if (o is ValueType)
            {
                char modifier = context.GetModifier(pointer.position.x, pointer.position.y);
                if (modifier == '͍')
                {
                    pointer.Push(o);
                    pointer.Push(value);
                    divide.Execute(pointer, context);
                }
                else
                {
                    pointer.Push(o);
                    pointer.Push(value);
                    multi.Execute(pointer, context);
                }
            }
            return true;
        }

        public IExecutableRune Register()
        {
            RuneRegistry.ALL_RUNES.Add(c, this);
            return this;
        }
    }
}