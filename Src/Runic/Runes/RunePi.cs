using System.Collections;
using System.Collections.Generic;


namespace EsotericIDE.Runic.Runes {
	public class RunePi : IExecutableRune {
		public double value;
		char c;

		public RunePi(double v, char c) {
			value = v;
			this.c = c;
		}

		public bool Execute(Pointer pointer, IRunicContext context) {
			pointer.Push(value);
			return true;
		}

		public IExecutableRune Register() {
			RuneRegistry.ALL_RUNES.Add(c, this);
			return this;
		}
	}
}