using System.Collections;
using System;
using EsotericIDE.Runic.math;

namespace EsotericIDE.Runic.runes {
	public class RunePushNewStack : IExecutableRune {
		public bool Execute(Pointer pointer, IRunicContext context) {
			pointer.PushNewStack();
			return true;
		}

		public IExecutableRune Register() {
			RuneRegistry.ALL_RUNES.Add(']', this);
			return this;
		}
	}
}