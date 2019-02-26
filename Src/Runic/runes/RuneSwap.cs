using System.Collections.Generic;
using System.Linq;

namespace EsotericIDE.Runic.runes {
	public class RuneSwap : IExecutableRune {
		public bool Execute(Pointer pointer, IRunicContext context) {
			char modifier = context.GetModifier(pointer.position.x, pointer.position.y);
			if(modifier == '̹' || modifier == '͗') {
				pointer.SwapStacksStack();
			}
			else if(modifier == '͍') {
				object o = pointer.Pop();
				if(o is string) {
					string s = (string)o;
					List<char> list = s.ToCharArray().ToList();
					char a = list[list.Count - 1];
					list.RemoveAt(list.Count - 1);
					char b = list[list.Count - 1];
					list.RemoveAt(list.Count - 1);
					list.Add(a);
					list.Add(b);
					pointer.Push(new string(list.ToArray()));
				}
				else {
					pointer.Push(o);
				}
			}
			else {
				if(pointer.GetStackSize() < 2) return true;
				object a = pointer.Pop();
				object b = pointer.Pop();
				pointer.Push(a);
				pointer.Push(b);
			}
			return true;
		}

		public IExecutableRune Register() {
			RuneRegistry.ALL_RUNES.Add('S', this);
			return this;
		}
	}
}