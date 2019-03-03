using System;

namespace EsotericIDE.Runic.Runes {
	public class RuneOutput : IExecutableRune {
		bool dumpStack = false;
		char c;
		public RuneOutput(char c, bool fullStack) {
			this.c = c;
			dumpStack = fullStack;
		}
		public bool Execute(Pointer pointer, IRunicContext context) {
			do {
				object o = pointer.Pop();
				if(o != null) {
					context.WriteOutputs(o);
				}
			} while(dumpStack && pointer.GetStackSize() > 0);
			if(dumpStack) pointer.DeductMana(pointer.GetMana());
			return true;
		}

		public IExecutableRune Register() {
			RuneRegistry.ALL_RUNES.Add(c, this);
			return this;
		}
	}
}