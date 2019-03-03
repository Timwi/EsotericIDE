using EsotericIDE.Runic.math;
using System;


namespace EsotericIDE.Runic.Runes {
	public class RuneConditional : IExecutableRune {
		public bool Execute(Pointer pointer, IRunicContext context) {
			object a = pointer.Pop();
			if(a is ValueType) {
				MathHelper.NumericRelationship r = MathHelper.Compare((ValueType)a, 0);
				if(r != MathHelper.NumericRelationship.EqualTo) {
					int b = (int)MathHelper.GetValue((ValueType)a);
					pointer.SetSkip(Math.Max(1,b));
				}
			}
			else {
				if(a == null) pointer.SetSkip(1);
				else pointer.Push(a);
			}
			return true;
		}

		public IExecutableRune Register() {
			RuneRegistry.ALL_RUNES.Add('?', this);
			return this;
		}
	}
}