using EsotericIDE.Runic.math;
using System.Collections;
using System.Collections.Generic;

namespace EsotericIDE.Runic.runes {
	public class RuneSplit : IExecutableRune {
		public bool Execute(Pointer pointer, IRunicContext context) {
			object o = pointer.Pop();
			if(o is string) {
				if(pointer.GetStackSize() > 0) {
					object s = pointer.Pop();
					if(s is char) {
						string[] sp = ((string)o).Split((char)s);
						for(int i = 0; i < sp.Length; i++) {
							pointer.Push(sp[i]);
						}
						return true;
					}
					else {
						pointer.Push(s);
					}
				}

				char[] c = ((string)o).ToCharArray();
				for(int i = 0; i < c.Length; i++) {
					pointer.Push(c[i]);
				}
			}
			else if (o is Vector3) {
				Vector3 v = (Vector3)o;
				pointer.Push(v.x);
				pointer.Push(v.y);
				pointer.Push(v.z);
			}
			else if(o != null) {
				pointer.Push(o.ToString());
				Execute(pointer, context);
			}
			return true;
		}

		public IExecutableRune Register() {
			RuneRegistry.ALL_RUNES.Add('u', this);
			return this;
		}
	}
}