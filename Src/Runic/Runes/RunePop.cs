

namespace EsotericIDE.Runic.Runes {
	public class RunePop : IExecutableRune {
		public bool Execute(Pointer pointer, IRunicContext context) {
			char modifier = context.GetModifier(pointer.position.x, pointer.position.y);
			if(modifier == '̹' || modifier == '͗') {
				pointer.PopDiscardStack();
			}
			else {
				pointer.Pop();
			}
			return true;
		}

		public IExecutableRune Register() {
			RuneRegistry.ALL_RUNES.Add('~', this);
			return this;
		}
	}
}