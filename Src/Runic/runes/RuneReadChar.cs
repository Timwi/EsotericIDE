using System.Collections;
using System.Collections.Generic;


namespace EsotericIDE.Runic.runes {
	public class RuneReadChar : IExecutableRune {
		public bool Execute(Pointer pointer, IRunicContext context) {
			pointer.SetReadType(Pointer.ReadType.READ_CHAR);
			return true;
		}

		public IExecutableRune Register() {
			RuneRegistry.ALL_RUNES.Add('\'', this);
			return this;
		}
	}
}