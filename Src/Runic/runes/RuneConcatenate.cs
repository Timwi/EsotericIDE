using System;
using System.Collections;
using System.Collections.Generic;

namespace EsotericIDE.Runic.runes {
	public class RuneConcatenate : IExecutableRune {
		public bool Execute(Pointer pointer, IRunicContext context) {
			char modifier = context.GetModifier(pointer.position.x, pointer.position.y);
			if(modifier == '͍') {
				StringBuilder result = new StringBuilder();
				bool cont = false;
				do {
					cont = false;
					if(pointer.GetStackSize() > 0) {
						object o = pointer.Pop();
						if(o is char) {
							result.Insert(0, o.ToString());
							//result.Append(o.ToString());
							cont = true;
						}
						else {
							pointer.Push(o);
						}
					}
				} while(cont);
				if(result.Length > 0)
					pointer.Push(result.ToString());
			}
			else {
				object a = pointer.Pop();
				object b = pointer.Pop();
				string x = "";
				string y = "";
				if(a is ValueType || a is string)
					x = a.ToString();
				if(b is ValueType || b is string)
					y = b.ToString();
				pointer.Push(y + x);
			}
			return true;
		}

		public IExecutableRune Register() {
			RuneRegistry.ALL_RUNES.Add('q', this);
			return this;
		}
	}
}