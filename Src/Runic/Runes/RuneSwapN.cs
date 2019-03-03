using System;
using System.Collections;
using System.Collections.Generic;

using EsotericIDE.Runic.math;
using System.Linq;

namespace EsotericIDE.Runic.Runes {
	public class RuneSwapN : IExecutableRune {
		public bool Execute(Pointer pointer, IRunicContext context) {
			object o = pointer.Pop();
			char modifier = context.GetModifier(pointer.position.x, pointer.position.y);
			if(o is ValueType) {
				int n = (int)Convert.ToDouble(o);
				if(Math.Abs(n) < 2) return true; //rotating 0 or 1 items will have no effect
				if(modifier == '̹' || modifier == '͗') {
					pointer.SwapNStacksStack(n);
				}
				else if(modifier == '͍') {
					object o2 = pointer.Pop();
					if(o2 is string) {
						string s = (string)o2;
						List<char> list = s.ToCharArray().ToList();
						bool right = n > 0;
						if(n < 0) n *= -1;
						if(list.Count < n) n = list.Count;
						List<char> nlist = new List<char>();
						for(int i = 0; i < n; i++) {
							char a = list[list.Count - 1];
							list.RemoveAt(list.Count - 1);
							nlist.Add(a);
						}
						nlist.Reverse();
						if(right)
							nlist.RotateListRight();
						else
							nlist.RotateListLeft();
						for(int i = 0; i < n; i++) {
							list.Add(nlist[i]);
						}
						s = new string(list.ToArray());
						pointer.Push(s);
					}
					else {
						pointer.Push(o2);
					}
				}
				else {
					bool right = n > 0;
					if(n < 0) n *= -1;
					if(pointer.GetStackSize() < n) n = pointer.GetStackSize();
					List<object> list = new List<object>();
					for(int i = 0; i < n; i++) {
						list.Add(pointer.Pop());
					}
					list.Reverse();
					if(right)
						list.RotateListRight();
					else
						list.RotateListLeft();
					for(int i = 0; i < n; i++) {
						pointer.Push(list[i]);
					}
				}
			}
			return true;
		}

		public IExecutableRune Register() {
			RuneRegistry.ALL_RUNES.Add('s', this);
			return this;
		}
	}
}