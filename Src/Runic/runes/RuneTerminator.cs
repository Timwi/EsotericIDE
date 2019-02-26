

namespace EsotericIDE.Runic.runes {
	internal class RuneTerminator : IExecutableRune {
		public bool Execute(Pointer pointer, IRunicContext context) {
			pointer.DeductMana(pointer.GetMana());
			return true;
		}

		public IExecutableRune Register() {
			RuneRegistry.ALL_RUNES.Add(';',this);
			return this;
		}
	}
}