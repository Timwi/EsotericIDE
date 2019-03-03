using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsotericIDE.Runic.Runes {
	public class RuneReadInput : IExecutableRune {
		public bool Execute(Pointer pointer, IRunicContext context) {
			context.ReadInput(pointer);
			return true;
		}

		public IExecutableRune Register() {
			RuneRegistry.ALL_RUNES.Add('i', this);
			return this;
		}
	}
}