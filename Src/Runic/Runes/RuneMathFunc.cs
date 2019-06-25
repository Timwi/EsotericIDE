using System;
using EsotericIDE.Runic.Math;

namespace EsotericIDE.Runic.Runes
{
    class RuneMathFunc : IExecutableRune
    {
        Random rand = new Random();
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
                        pointer.Push(rand.Next((int) x));
                        break;
                    case '+':
                        char[] digits = x.ToString().ToCharArray();
                        int sum = 0;
                        foreach(char d in digits)
                        {
                            sum += (d-48);
                        }
                        pointer.Push(sum);
                        break;
                    case '*':
                        digits = x.ToString().ToCharArray();
                        sum = 1;
                        foreach(char d in digits)
                        {
                            sum *= (d - 48);
                        }
                        pointer.Push(sum);
                        break;
                    case '!':
                        int number = (int)x;
                        if(number == 0)
                        {
                            pointer.Push(1);
                            break;
                        }
                        double fact = number;
                        for(int i = number - 1; i >= 1; i--)
                        {
                            fact *= i;
                        }
                        pointer.Push(fact);
                        break;
                    case '‼':
                        number = (int)x;
                        if(number == 0)
                        {
                            pointer.Push(1);
                            break;
                        }
                        fact = number;
                        for(int i = number - 2; i >= 1; i-=2)
                        {
                            fact *= i;
                        }
                        pointer.Push(fact);
                        break;
                    case 'P':
                        pointer.Push(IsPrime((int)x) ? 1 : 0);
                        break;
                }
            }
            return true;
        }

        private bool IsPrime(int number) {
            if(number < 2)
                return false;
            if(number == 2)
                return true;
            if(number % 2 == 0)
                return false;
            if(number % 3 == 0)
                return false;
            double sq = System.Math.Sqrt(number);
            for(int i = 5; i <= sq; i += 6)
            {
                if(number % i == 0)
                    return false;
				if(number % (i+2) == 0)
					return false;
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