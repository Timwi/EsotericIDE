using EsotericIDE.Runic.math;
using System;


namespace EsotericIDE.Runic.Runes {
	internal class RuneSubtraction : IExecutableRune {

		public bool Execute(Pointer pointer, IRunicContext context) {
			object a = pointer.Pop();
			object b = pointer.Pop();
			if(a != null && b != null) {
				if(a is ValueType && b is ValueType) {
					if(MathHelper.IsInteger((ValueType)a) && MathHelper.IsInteger((ValueType)b)) {
						int c = (int)b - (int)a;
						pointer.Push(c);
					}
					else if(a is Vector3 && b is Vector3) {
						pointer.Push(((Vector3)b - (Vector3)a));
					}
					else {
						double c = MathHelper.GetValue((ValueType)b) - MathHelper.GetValue((ValueType)a);
						pointer.Push(c);
					}
				}
				else if(a is ValueType && b is string) {
					int n = (int)MathHelper.GetValue((ValueType)a);
					string s = (string)b;
					if(n > 0) {
						string second = s.Substring(0, s.Length - n);
						pointer.Push(second);
					}
					else if(n < 0) {
						n *= -1;
						string first = s.Substring(0, n);
						string second = s.Substring(n, s.Length - n);
						pointer.Push(second);
					}
					else {
						pointer.Push(s);
					}
				}
				else {

				}
			}
			return true;
		}

		public IExecutableRune Register() {
			RuneRegistry.ALL_RUNES.Add('-', this);
			return this;
		}
	}
}