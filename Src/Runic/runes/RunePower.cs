using EsotericIDE.Runic.math;
using System;
using System.Collections;
using System.Collections.Generic;


namespace EsotericIDE.Runic.runes {
	public class RunePower : IExecutableRune {
		public bool Execute(Pointer pointer, IRunicContext context) {
			object a = pointer.Pop();
			object b = pointer.Pop();
			if(a is ValueType && b is ValueType) {
				if(a is Vector3 || b is Vector3) {
				}
				else if(a is char || b is char) {
					double x = MathHelper.GetValue((ValueType)a);
					double y = MathHelper.GetValue((ValueType)b);
					pointer.Push(Math.Pow(y, x));
				}
				else {
					double x = Convert.ToDouble(a);
					double y = Convert.ToDouble(b);
					pointer.Push(Math.Pow(y, x));
				}
			}
			return true;
		}

		public IExecutableRune Register() {
			RuneRegistry.ALL_RUNES.Add('p', this);
			return this;
		}
	}
}