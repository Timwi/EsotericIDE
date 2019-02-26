using EsotericIDE.Runic.math;
using System;
using System.Collections;
using System.Collections.Generic;


namespace EsotericIDE.Runic.runes {
	public class RuneFork : IExecutableRune {
		char c;
		Direction dir;
		public RuneFork(Direction d, char c) {
			this.c = c;
			dir = d;
		}
		public bool Execute(Pointer pointer, IRunicContext context) {
			object o = pointer.Pop();
			if(o is ValueType) {
				ValueType v = (ValueType)o;
				if(MathHelper.Compare(pointer.GetMana(), v) >= 0) {
					int d = (int)MathHelper.GetValue(v);
					pointer.DeductMana(d - 1);
					Pointer ptr = new Pointer(d, dir, pointer.position);
					char modifier = context.GetModifier(pointer.position.x, pointer.position.y);
					if(modifier == '͍') {
						List<object> items = new List<object>();
						while(pointer.GetStackSize() > 0) {
							items.Add(pointer.Pop());
						}
						items.Reverse();
						foreach(object q in items) {
							ptr.Push(q);
							if(modifier != '͍')
								pointer.Push(q);
						}
					}
					context.SpawnPointer(ptr);
					return true;
				}
				else {
					pointer.Push(o);
				}
			}
			return true;
		}

		public IExecutableRune Register() {
			RuneRegistry.ALL_RUNES.Add(c, this);
			return this;
		}
	}
}