using EsotericIDE.Runic.math;
using System;
using System.Collections;
using System.Collections.Generic;


namespace EsotericIDE.Runic.runes {
	public class RuneModulo : IExecutableRune {
		public bool Execute(Pointer pointer, IRunicContext context) {
			object a = pointer.Pop();
			object b = pointer.Pop();
			if(a != null && b != null) {
				if(a is ValueType && b is ValueType) {
					if(MathHelper.IsInteger((ValueType)a) && MathHelper.IsInteger((ValueType)b)) {
						int c = (int)MathHelper.GetValue((ValueType)b) % (int)MathHelper.GetValue((ValueType)a);
						pointer.Push(c);
					}
					else {
						double d = MathHelper.GetValue((ValueType)a);
						if(MathHelper.Approximately((float)d, 0)) {
							pointer.DeductMana(pointer.GetMana());
							return true;
						}
						double c = MathHelper.GetValue((ValueType)b) % d;
						pointer.Push(c);
					}
				}
				else if(a is ValueType && b is string) {
					int c = (int)MathHelper.GetValue((ValueType)a);
					if(c < 0)
						c = ((string)b).Length + c;
					pointer.Push(((string)b)[c]);
				}
				else {

				}
			}
			return true;
		}

		public IExecutableRune Register() {
			RuneRegistry.ALL_RUNES.Add('%', this);
			return this;
		}
	}
}