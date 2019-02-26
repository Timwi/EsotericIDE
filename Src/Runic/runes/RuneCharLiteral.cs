using System.Collections;
using System.Collections.Generic;


namespace EsotericIDE.Runic.runes {
	public class RuneCharLiteral : IExecutableRune {
		public char c;
		public RuneCharLiteral(char c) {
			this.c = c;
		}
		public bool Execute(Pointer pointer, IRunicContext context) {
			return true;
		}

		public IExecutableRune Register() {
			RuneRegistry.ALL_RUNES.Add(c, this);
			return this;
		}
	}
}