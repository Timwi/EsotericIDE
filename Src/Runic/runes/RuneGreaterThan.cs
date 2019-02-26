using EsotericIDE.Runic.math;
using System;
using System.Collections;
using System.Collections.Generic;


namespace EsotericIDE.Runic.runes {
	public class RuneGreaterThan : IExecutableRune {
		public bool Execute(Pointer pointer, IRunicContext context) {
			object a = pointer.Pop();
			object b = pointer.Pop();
			if(a is ValueType && b is ValueType) {
				MathHelper.NumericRelationship q = MathHelper.Compare((ValueType)b, (ValueType)a);
				bool r = q == MathHelper.NumericRelationship.GreaterThan;
				char modifier = context.GetModifier(pointer.position.x, pointer.position.y);
				if(modifier == '̸' || modifier == '͍') {
					r = !r;
				}
				pointer.Push(r ? 1 : 0);
			}
			return true;
		}

		public IExecutableRune Register() {
			RuneRegistry.ALL_RUNES.Add(')', this);
			return this;
		}
	}
}