using EsotericIDE.Runic.math;
using System;


namespace EsotericIDE.Runic.runes {
	public class RuneMinMana : IExecutableRune {
		public bool Execute(Pointer pointer, IRunicContext context) {
			object o = pointer.Pop();
			if(o is ValueType) {
				ValueType v = (ValueType)o;
				if(MathHelper.Compare(pointer.GetMana(), v) >= 0) {
					return true;
				}
				else {
					pointer.Push(o);
				}
			}
			return false;
		}

		public IExecutableRune Register() {
			RuneRegistry.ALL_RUNES.Add('M', this);
			return this;
		}
	}
}