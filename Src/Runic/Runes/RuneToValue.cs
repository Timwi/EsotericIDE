using EsotericIDE.Runic.math;
using System;
using System.Collections;
using System.Collections.Generic;


namespace EsotericIDE.Runic.Runes {
	public class RuneToValue : IExecutableRune {
		public bool Execute(Pointer pointer, IRunicContext context) {
			object o = pointer.Pop();
			if(o is ValueType) {
				pointer.Push(MathHelper.GetValue((ValueType)o));
			}
			else if(o is string) {
				double d;
				if(double.TryParse((string)o, out d))
					pointer.Push(d);
				else
					pointer.Push(WordDictionary.GetIndex(((string)o).ToLower()));
			}
			return true;
		}

		public IExecutableRune Register() {
			RuneRegistry.ALL_RUNES.Add('n', this);
			return this;
		}
	}
}