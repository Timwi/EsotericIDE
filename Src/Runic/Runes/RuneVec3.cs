using EsotericIDE.Runic.math;
using System;
using System.Collections;
using System.Collections.Generic;


namespace EsotericIDE.Runic.Runes {
	public class RuneVec3 : IExecutableRune {
		public bool Execute(Pointer pointer, IRunicContext context) {
			object a = pointer.Pop();
			object b = pointer.Pop();
			object c = pointer.Pop();
			float x = 0, y = 0, z = 0;
			if(a is ValueType)
				x = (float)Convert.ToDouble(a);
			if(b is ValueType)
				y = (float)Convert.ToDouble(b);
			if(c is ValueType)
				z = (float)Convert.ToDouble(c);
			pointer.Push(new Vector3(x, y, z));
			return true;
		}

		public IExecutableRune Register() {
			RuneRegistry.ALL_RUNES.Add('V', this);
			return this;
		}
	}
}