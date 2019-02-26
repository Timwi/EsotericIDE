using EsotericIDE.Runic.math;
using EsotericIDE.Runic.runes;
using RT.Util.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsotericIDE.Runic {
	internal sealed class RunicEnvironment : ExecutionEnvironment, IRunicContext {
		private IExecutableRune[,] runes;
		private char[,] modifiers;
		private List<Vector2Int> entries;
		private List<Pointer> newpointers;
		private List<Pointer> pointers;
		private Func<object> inputReader;
		private Action<object> outputWriter;
		private string source;

		public RunicEnvironment(string originalSource, IExecutableRune[,] runes, List<Vector2Int> entryPoints, char[,] modifiers) {
			this.runes = runes;
			this.entries = entryPoints;
			this.modifiers = modifiers;
			this.source = originalSource;
		}

		public RunicEnvironment SetWriter(Action<object> writer) {
			outputWriter = writer;
			return this;
		}

		public bool IsValidPos(int x, int y) {
			return x >= 0 && y >= 0 && x < runes.GetLength(0) && y < runes.GetLength(1);
		}

		public RunicEnvironment SetReader(Func<object> reader) {
			inputReader = reader;
			return this;
		}

		public void Initialize() {
			pointers = new List<Pointer>();
			newpointers = new List<Pointer>();
			foreach(Vector2Int v in entries) {
				Pointer pointer = new Pointer(0, Direction.RIGHT, v);
				if(v.x < 0) {
					RuneRegistry.GetRune('>').Execute(pointer, this);
					pointer.position.x = 0;
					pointer.position.y = 0;
				}
				else if(runes[v.x, v.y].Execute(pointer, null)) {
					AdvancePointer(pointer, true);
				}
				pointers.Add(pointer);
			}
		}

		public void SpawnPointer(Pointer p) {
			newpointers.Add(p);
			//AdvancePointer(p, true);
		}

		public ReadOnlyCollection<Pointer> GetPointers() {
			return pointers.AsReadOnly();
		}

		public bool Tick() {
			foreach(Pointer pointer in pointers) {
				bool delaying = pointer.GetDelayAmt() > 0;
				bool skipping = pointer.isSkipping(true);
				pointer.Execute();
				if(pointer.GetReadType() == Pointer.ReadType.EXECUTE) {
					if(pointer.position.x >= runes.GetLength(0) || pointer.position.y >= runes.GetLength(1)) {
						pointer.DeductMana(pointer.GetMana());
					}
					else if(skipping || delaying || runes[pointer.position.x, pointer.position.y].Execute(pointer, this)) {
						AdvancePointer(pointer, !delaying && !skipping);
					}
				}
				else if(pointer.GetReadType() == Pointer.ReadType.READ_NUM) {
					IExecutableRune r = runes[pointer.position.x, pointer.position.y];
					if(r is RuneNumber) {
						int j = 0;
						int n = ((RuneNumber)r).value;
						if(pointer.GetStackSize() > 0) {
							object o = pointer.Pop();
							if(o is ValueType && MathHelper.IsInteger((ValueType)o)) {
								j = (int)MathHelper.GetValue((ValueType)o);
							}
						}
						pointer.Push(j * 10 + n);
						AdvancePointer(pointer, false);
					}
					else {
						pointer.SetReadType(Pointer.ReadType.EXECUTE);
					}
				}
				else if(pointer.GetReadType() == Pointer.ReadType.READ_CHAR) {
					char c = RuneRegistry.GetRuneChar(runes[pointer.position.x, pointer.position.y]);
					pointer.Push(c);
					c = modifiers[pointer.position.x, pointer.position.y];
					if(c != ' ') {
						pointer.Push(c);
					}
					pointer.SetReadType(Pointer.ReadType.EXECUTE);
					AdvancePointer(pointer, false);
				}
				else if(pointer.GetReadType() == Pointer.ReadType.READ_CHAR_CONTINUOUS) {
					char c = RuneRegistry.GetRuneChar(runes[pointer.position.x, pointer.position.y]);
					if(c.Equals('`')) {
						pointer.SetReadType(Pointer.ReadType.EXECUTE);
					}
					else {
						pointer.Push(c);
						c = modifiers[pointer.position.x, pointer.position.y];
						if(c != ' ') {
							pointer.Push(c);
						}
					}
					AdvancePointer(pointer, false);
				}
				else if(pointer.GetReadType() == Pointer.ReadType.READ_STR) {
					char c = RuneRegistry.GetRuneChar(runes[pointer.position.x, pointer.position.y]);
					if(c.Equals('\"')) {
						pointer.SetReadType(Pointer.ReadType.EXECUTE);
						AdvancePointer(pointer, true);
					}
					else {
						object o = pointer.GetStackSize() > 0 ? pointer.Pop() : null;
						if(o is string || o == null) {
							string s = (string)o + c;
							c = modifiers[pointer.position.x, pointer.position.y];
							if(c != ' ') {
								s += c;
							}
							pointer.Push(s);
						}
						else {
							pointer.Push(o);
							string s = c.ToString();
							c = modifiers[pointer.position.x, pointer.position.y];
							if(c != ' ') {
								s += c;
							}
							pointer.Push(s);
						}
						AdvancePointer(pointer, false);
					}
				}
			}
			pointers.ForEach(x => {
				int xi = pointers.IndexOf(x);
				pointers.ForEach(y => {
					int yi = pointers.IndexOf(y);
					if(xi < yi && x.position == y.position && x.direction == y.direction) {
						x.Merge(y);
						y.DeductMana(y.GetMana());
					}
				});
			});
			pointers.ForEach(x => {
				if(x.GetMana() >= 100 && runes[x.position.x, x.position.y] != RuneRegistry.GetRune(' ')) {
					runes[x.position.x, x.position.y] = RuneRegistry.GetRune(' ');
					x.DeductMana(x.GetMana() / 2);
					Console.Error.WriteLine("Warning: rune at " + x.position + " has burned out!");
				}
			});
			pointers.ForEach(x => {
				if(modifiers[x.position.x, x.position.y] == '̺' || modifiers[x.position.x, x.position.y] == '̪') {
					if(x.GetStackSize() > x.GetMana() + 10) {
						TruncateStack(x, modifiers[x.position.x, x.position.y] == '̪');
					}
				}
			});
			pointers.ForEach(x => {
				if(x.GetStackSize() > x.GetMana() + 10) {
					x.DeductMana(1);
				}
			});
			pointers.AddRange(newpointers);
			newpointers.Clear();
			pointers.RemoveAll(x => x.GetMana() <= 0);
			return pointers.Count > 0;
		}

		private void TruncateStack(Pointer x, bool fromTop) {
			if(!fromTop) {
				x.ReverseStack();
			}
			while(x.GetStackSize() > x.GetMana() + 10) {
				x.Pop();
			}
			if(!fromTop) {
				x.ReverseStack();
			}
		}

		public char GetModifier(int x, int y) {
			return modifiers[x, y];
		}

		public char GetRune(int x, int y) {
			return RuneRegistry.GetRuneChar(runes[x, y]);
		}

		public void AdvancePointer(Pointer pointer, bool readDelayModifier) {
			if(pointer.GetMana() <= 0) return;
			if(pointer.GetDelayAmt() > 0) return;
			if(readDelayModifier) {
				pointer.direction = GetModifiedDirection(modifiers[pointer.position.x, pointer.position.y], pointer.direction);
				int j = GetDelayAmount(modifiers[pointer.position.x, pointer.position.y]);
				pointer.SetDelay(j);
				if(j > 0) return;
			}
			pointer.position.x += DirectionHelper.GetX(pointer.direction);
			pointer.position.y += DirectionHelper.GetY(pointer.direction);
			int width = runes.GetLength(0);
			int height = runes.GetLength(1);
			if(pointer.position.x >= width) {
				pointer.position.x = 0;
			}
			if(pointer.position.y >= height) {
				pointer.position.y = 0;
			}
			if(pointer.position.x < 0) {
				pointer.position.x = width - 1;
			}
			if(pointer.position.y < 0) {
				pointer.position.y = height - 1;
			}
		}

		public int GetDelayAmount(char modifier) {
			return SGetDelayAmount(modifier);
		}

		public static int SGetDelayAmount(char modifier) {
			switch(modifier) {
				case '̇':// ̇
					return 1;
				case '̈':// ̈
					return 2;
				case '̣':// ̣
					return 4;
				case '̤':// ̤
					return 8;
			}
			return 0;
		}

		public Direction GetModifiedDirection(char modifier, Direction original) {
			// a̖a̗a̩a̠a̻ àáa̍āå a̎a͈a̿a͇
			int dir = (int)original;
			Direction ndir;
			switch(modifier) {
				case '͔':
				case '᷾':
					return Direction.LEFT;
				case '͕'://B̂B̌B᷾B͐
				case '͐'://B̬B̭B͔B͕
					return Direction.RIGHT;
				case '̭':
				case '̂':
					return Direction.UP;
				case '̬':
				case '̌':
					return Direction.DOWN;
				case '̖':
				case '̀':
					if(dir <= 1) {
						return DirectionHelper.RotateCCW(original);
					}
					else {
						return DirectionHelper.RotateCW(original);
					}
				case '̗':
				case '́':
					if(dir % 2 == 0) {
						int a = 2 - dir;
						return DirectionHelper.Reflect((Direction)a);
					}
					else {
						int a = 4 - dir;
						return DirectionHelper.Reflect((Direction)a);
					}
				case '̩':
				case '̍':
					ndir = Direction.RIGHT;
					if(original == ndir || original == DirectionHelper.Reflect(ndir)) {
						return DirectionHelper.Reflect(original);
					}
					return original;
				case '̠':
				case '̄':
					ndir = Direction.DOWN;
					if(original == ndir || original == DirectionHelper.Reflect(ndir)) {
						return DirectionHelper.Reflect(original);
					}
					return original;
				case '̻':
				case '̊':
					return DirectionHelper.Reflect(original);
			}
			return original;
		}

		public Func<bool> Eval(Pointer pointer, string code, out int size) {
			RunicEnvironment context;
			ParseError err = Parser.Parse(code, "", out context);

			if(modifiers[pointer.position.x, pointer.position.y] == '͍') {
				context.SetReader(() => {
					if(pointer.GetStackSize() > 0) {
						return pointer.Pop();
					}
					return null;
				});
				context.SetWriter((o) => {
					pointer.Push(o);
				});
			}
			else {
				context.SetWriter((o) => {
					outputWriter(o);
				});
			}
			if(err.type != ParseErrorType.NONE || context == null) {
				size = 0;
				return () => true;
			}
			context.Initialize();
			size = context.runes.GetLength(0) + context.runes.GetLength(1);
			return () => {
				bool continueExecuting = false;
				continueExecuting = context.Tick();
				return !continueExecuting;
			};
		}

		public void ReadInput(Pointer pointer) {
			object o = inputReader();
			if(o != null)
				pointer.Push(o);
		}

		public void WriteOutputs(object o) {
			outputWriter(o);
		}

		public void SetRune(int x, int y, char c) {
			IExecutableRune r = RuneRegistry.GetRune(c);
			if(r == null) {
				runes[x, y] = new RuneCharLiteral(c);
			}
			else {
				runes[x, y] = r;
			}
		}

		public void SetModifier(int x, int y, char c) {
			modifiers[x, y] = c;
		}

		public override void UpdateWatch() {
			_txtWatch.Text = "Position: ({3}, {4}) {5} {1}{0}Mana: {6}{0}{2}{0}Substack depth:{7}".Fmt(
				/* 0 */ Environment.NewLine,
				/* 1 */ _curPointer.isSkipping(false) ? "Skipping" : "",
				/* 2 */ _curPointer.PrintStack(),
				/* 3 */ _curPointer.position.x,
				/* 4 */ _curPointer.position.y,
				/* 5 */ _curPointer.direction,
				/* 6 */ _curPointer.GetMana(),
				/* 7 */ _curPointer.GetStacksStackSize()
			);
		}

		internal Action<object> GetDefaultWriter() {
			return (object x) => {
				_output.Append(x.ToString());
			};
		}

		private Pointer _curPointer;
		protected override IEnumerable<Position> GetProgram() {
			bool continueExecuting = false;
			int counter = 0;
			do {
				foreach(Pointer p in GetPointers()) {
					_curPointer = p;
					yield return new Position(GetLengthFromPos(p.position.x, p.position.y), 1);
				}
				continueExecuting = continueExecuting = Tick();
				counter++;
			} while(continueExecuting && counter < 1000000);
			//yield return null;
		}

		private int GetLengthFromPos(int tx, int ty) {
			int pos = 0;
			int x = 0;
			int y = 0;
			for(int i = 0; i<this.source.Length;i++) {
				pos = i;
				if(y < ty) {
					if(source[i] != '\n') continue;
					else {
						y++;
						continue;
					}
				}
				else if(x <= tx) {
					if(source[i] == '\n') {
						pos--;
						break;
					}
					UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(source[i]);
					if(uc == UnicodeCategory.NonSpacingMark || uc == UnicodeCategory.EnclosingMark || uc == UnicodeCategory.OtherNotAssigned) {
						continue;
					}
					x++;
					continue;
				}
				break;
			}
			return pos-1;
		}
	}
}
