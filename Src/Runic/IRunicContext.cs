using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsotericIDE.Runic {
	public interface IRunicContext {
		ReadOnlyCollection<Pointer> GetPointers();
		void SpawnPointer(Pointer ptr);

		char GetModifier(int x, int y);
		void SetModifier(int x, int y, char c);
		char GetRune(int x, int y);
		void SetRune(int x, int y, char c);

		void ReadInput(Pointer pointer);
		void WriteOutputs(object o);

		bool IsValidPos(int x, int y);
		void AdvancePointer(Pointer pointer, bool v);

		int GetDelayAmount(char v);
		Direction GetModifiedDirection(char modifier, Direction direction);

		Func<bool> Eval(Pointer pointer, string str, out int size);
	}
}
