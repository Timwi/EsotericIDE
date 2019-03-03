using EsotericIDE.Runic.math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsotericIDE.Runic.Runes {
	class RuneMathFunc : IExecutableRune {
		public bool Execute(Pointer pointer, IRunicContext context) {
			object o = pointer.Pop();
			object v = pointer.Pop();
			if(o is ValueType && v is ValueType) {
				char c = (char)MathHelper.GetValue((ValueType)o);
				double x = MathHelper.GetValue((ValueType)v);
				switch(c) {
					case 'C':
						pointer.Push(Math.Cos(x));
						break;
					case 'S':
						pointer.Push(Math.Sin(x));
						break;
					case 'T':
						pointer.Push(Math.Tan(x));
						break;
					case 'e':
						pointer.Push(Math.Exp(x));
						break;
					case 'l':
						pointer.Push(Math.Log(x));
						break;
					case 'L':
						pointer.Push(Math.Log10(x));
						break;
					case 'f':
						pointer.Push(Math.Floor(x));
						break;
					case 'c':
						pointer.Push(Math.Ceiling(x));
						break;
					case 'r':
						pointer.Push(Math.Round(x));
						break;
					case '|':
						pointer.Push(Math.Abs(x));
						break;
					case 'q':
						pointer.Push(Math.Sqrt(x));
						break;
					case 'i':
						pointer.Push(Math.Asin(x));
						break;
					case 'o':
						pointer.Push(Math.Acos(x));
						break;
					case 'a':
						pointer.Push(Math.Atan(x));
						break;
					case 'R':
						Random rand = new Random();
						pointer.Push(rand.Next((int)x));
						break;
				}
			}
			return true;
		}

		public IExecutableRune Register() {
			RuneRegistry.ALL_RUNES.Add('A', this);
			return this;
		}
	}
}