using EsotericIDE.Runic.Runes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EsotericIDE.Runic {
	sealed class RunicEnchantments : ProgrammingLanguage {
		public override string LanguageName { get { return "Runic"; } }

		public override string DefaultFileExtension { get { return "rune"; } }
		
		public override ToolStripMenuItem[] CreateMenus(IIde ide) {
			RuneRegistry.Initialize();
			//if(EsotericIDEProgram.Settings.SourceFont == null)
			//	EsotericIDEProgram.Settings.SourceFont = new FontSpec("Lucida Console", 12, FontStyle.Regular, Color.Black);
			return new ToolStripMenuItem[0];
		}

		public override ExecutionEnvironment Compile(string source, string input) {
			RunicEnvironment ctx;
			ParseError err = Parser.Parse(source, input, out ctx);
			if(err.type != ParseErrorType.NONE) {
				StringBuilder sb = new StringBuilder("Parser error: ");
				sb.Append(err.type.ToString());
				sb.Append(" at position ");
				sb.Append(err.pos);
				sb.Append(", character '");
				UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(err.character);
				if(uc == UnicodeCategory.NonSpacingMark || uc == UnicodeCategory.EnclosingMark) {
					sb.Append('◌');
				}
				sb.Append(err.character);
				sb.Append("'");
				throw new CompileException(sb.ToString(), err.pos1d, 1);
			}
			ctx.Initialize();
			return ctx;
		}

		public override string GetInfo(string source, int cursorPosition) {
			if(cursorPosition < 0 || cursorPosition >= source.Length)
				return "";

			var ch = source[cursorPosition];
			var modifier = cursorPosition+1 >= source.Length ? ' ' : source[cursorPosition+1];
			UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(modifier);
			if(!(uc == UnicodeCategory.NonSpacingMark || uc == UnicodeCategory.EnclosingMark || uc == UnicodeCategory.OtherNotAssigned)) {
				modifier = ' ';
			}
			string modifierText = "";
			string str = DirectionModifierText(modifier);
			string returnStr = "";
			bool modFlag = false;
			bool substackFlag = false;
			if(modifier == ' ') {

			}
			else if(str.Length > 0) {
				modifierText = " Then " + str;
			}
			else if(RunicEnvironment.SGetDelayAmount(modifier) > 0) {
				modifierText = " Then delay " + RunicEnvironment.SGetDelayAmount(modifier) + " ticks.";
			}
			else if(modifier == '̪' || modifier == '̺') {
				modifierText = " Then truncate stack, keeping the " + ((modifier == '̪')?"bottom":"top");
			}
			else if (modifier == '͏') {
				modifierText = " +Blank Modifier";
			}
			else if(modifier == '͗' || modifier == '̹') {
				substackFlag = true;
			}
			else {
				modFlag = true;
			}
			switch(ch) {
				// Misc
				case ';': returnStr = "Exit."; break;

				// Arithmetic
				case '+': returnStr = "Pops two values from the stack and pushes their sum."; break;
				case '-': returnStr = "Pops y, pops x, pushes x − y."; break;
				case '*': returnStr = "Pops two values from the stack and pushes their product."; break;
				case ',': returnStr = "Pops y, pops x, pushes x ÷ y (if resulting value is approximately an integer value, integer result, otherwise double)."; break;
				case '%': returnStr = "Pops y, pops x, pushes x % y (modulo; the sign of the result is the same as the sign of y)."; break;
				case 'X': returnStr = (modFlag?"Miltiplies":"Divides")+" the top of the stack by 10."; break;
				case 'C': returnStr = (modFlag ? "Miltiplies" : "Divides") + " the top of the stack by 100."; break;
				case 'Y': returnStr = (modFlag ? "Miltiplies" : "Divides") + " the top of the stack by 1000."; break;
				case 'p': returnStr = "Pops y, pops x, pushes x^y."; break;
				case 'A': returnStr = "Pops a character x, performs a math operation on a popped value y."; break;
				case 'Z': returnStr = (modFlag ? "Boolean not.":"Multiplies the top of the stack by -1."); break;

				// Entries
				case '>':
					returnStr = (modFlag ? "Pops y, pops x, pushes 1 if x ≤ y, else 0." : "Entry point."); break;
				case '<':
					returnStr = (modFlag? "Pops y, pops x, pushes 1 if x ≥ y, else 0." : "Entry point."); break;
				case 'v':
				case '^':
					returnStr = "Entry point."; break;

				// Input/Output
				case 'i': returnStr = "Pushes a space-separated value read from input. As a double or integer, if numeric, else string or character. If the input is 1 character, then char."; break;
				case '$': returnStr = "Pops a value and prints it."; break;
				case '@': returnStr = "While the stack is not empty, pop a value and print it. Finally, terminate."; break;

				// Mana
				case 'm': returnStr = "Pushes the IP's current mana value to the stack."; break;
				case 'M': returnStr = "Peeks at the top value of the stack. If the IP's mana is greater or equal to this value, the value is popped an the IP advances. Otherwise nothing happens."; break;
				case 'F': returnStr = "IP fizzles 1 mana."; break;

				// Stack Manipulation
				case ':': returnStr = "Duplicates " + (substackFlag?"the entire stack as a new substack": "the top of the stack."); break;
				case '~': returnStr = "Pops the top of the stack and discards it."; break;
				case '}': returnStr = "Rotates " + (substackFlag?"all substacks": (modFlag ? "a popped string" : "the stack")) + " ("+(modFlag ? "end moves to front" :"top moves to the bottom")+")."; break;
				case '{': returnStr = "Rotates " + (substackFlag ? "all substacks" : (modFlag ? "a popped string" : "the stack")) + " (" + (modFlag ? "end moves to front" : "top moves to the bottom") + ")."; break;
				case 'S': returnStr = "Swaps the " + (modFlag?"last two":"top two")+ " " + (substackFlag?"substacks": (modFlag ? "characters from a popped string" : "values on the stack")) + "."; break;
				case 's': returnStr = "Pops x, then rotates the top"+(modFlag ? "last" : "top")+" |x| " + (substackFlag ? "substacks" : "values on the stack") + ". If x is positive, the top moves to the bottom, else the bottom moves to the top. These values are then put back on the stack."; break;
				case 'r': returnStr = "Reverses " + (substackFlag ? "the substacks stack" : (modFlag ? "a popped string" : "the stack")) + "."; break;
				case 'l': returnStr = "Pushes the size of " + (substackFlag ? "the substacks stack" : (modFlag ? "a popped string" : "the stack")) + " to the stack."; break;
				case 'o': returnStr = (modFlag ? "Sorts the string by character.":"Continuously pops values off the top of the stack until a non-numeric value is found (which is replaced). Sorts the popped values and pushes them back to the stack, smallest on top. Costs n-10 (min 1) mana where n is the number of items sorted."); break;
				case 'T': returnStr = "If this is the only IP on this cell or it has no stack, the IP does nothing and does not advance. Otherwise, it pops a value x (as int), then pops x items off its stack and transfers these values (in the same order) to all other IPs on this instruction after which all IPs will advance. Stack underflow still transfers the items that did exist but the source IP is terminated."; break;

				// Substacks
				case '[': returnStr = "Pop x off the stack and create a new stack, moving x (maximum the current stack) values from the old stack onto the new one (maintaining the same order). The new stack is completely isolated from the previous one, and an arbitrary amount of stacks can be created on top of each other. Costs 1 mana (unless x 0)."; break;
				case ']': returnStr = "Pushes the current stack back to the top of the next most underlying substack (maintaining the current order). If there is no underlying substack, then the stack is cleared."; break;

				// Flow Control
				case '/':
				case '\\':
				case '_':
				case '|':
				case '#':
					returnStr = "Changes the direction of an IP, reflecting it."; break;
				case 'U':
				case 'D':
				case 'L':
				case 'R':
				case '↑':
				case '↓':
				case '←':
				case '→':
					returnStr = "Changes the direction of an IP to the indicated direction."; break;

				// Logic
				case '=':
					returnStr = "Pops y, pops x, pushes 1 if x " + (modFlag ? "≠" : "=") + " y, else 0."; break;
				case ')':
					returnStr = "Pops y, pops x, pushes 1 if x " + (modFlag ? "≤" : ">") + " y, else 0."; break;
				case '(':
					returnStr = "Pops y, pops x, pushes 1 if x " + (modFlag ? "≥" : "<") + " y, else 0."; break;

				// Skips and Delays
				case 'y': returnStr = "IP waits 1 cycle before advancing."; break;
				case '!': returnStr = "IP skips the next instruction."; break;
				case '?': returnStr = "Pops x. IP skips the next x instructions. If x < 0, then 1 instruction is skipped."; break;

				// Vectors
				case 'V': returnStr = "Pops x, y, z and pushes a Vector3 composed of those values."; break;
				case 'j': returnStr = "Pops two Vector3s and pushes their relative distance."; break;

				// Literals
				case '\'': returnStr = "Next instruction is read as a character literal."; break;
				case '`': returnStr = "Pointer enters continuous character literal reading mode."; break;
				case '"': returnStr = "Pointer enters continuous string literal reading mode."; break;
				case '‘':
				case '´': returnStr = "Pointer enters continuous number reading mode."; break;
				case 'P': returnStr = "Pushes pi to the stack."; break;
				case 'ɩ':
				case 'í':  returnStr = "Pushes -1 to the stack."; break;
				case 'é': returnStr = "Pushes e to the stack."; break;
				case 'µ': returnStr = "Pushes 0.000001 to the stack."; break;
				case '0': returnStr = "Pushes 0 to the stack."; break;
				case '1': returnStr = "Pushes 1 to the stack."; break;
				case '2': returnStr = "Pushes 2 to the stack."; break;
				case '3': returnStr = "Pushes 3 to the stack."; break;
				case '4': returnStr = "Pushes 4 to the stack."; break;
				case '5': returnStr = "Pushes 5 to the stack."; break;
				case '6': returnStr = "Pushes 6 to the stack."; break;
				case '7': returnStr = "Pushes 7 to the stack."; break;
				case '8': returnStr = "Pushes 8 to the stack."; break;
				case '9': returnStr = "Pushes 9 to the stack."; break;
				case 'a': returnStr = "Pushes 10 to the stack."; break;
				case 'b': returnStr = "Pushes 11 to the stack."; break;
				case 'c': returnStr = "Pushes 12 to the stack."; break;
				case 'd': returnStr = "Pushes 13 to the stack."; break;
				case 'e': returnStr = "Pushes 14 to the stack."; break;
				case 'f': returnStr = "Pushes 15 to the stack."; break;

				// Conversions
				case 'k': returnStr = "Pops a value x and pushes (char)x."; break;
				case 'n': returnStr = "Pops x and pushes (double)x or double.parse(x)."; break;
				case 'q': returnStr = "Pops x, pops y, concatenates them into the string \"yx\"."; break;
				case 'u': returnStr = "Pops x. If x is a string, peek y. If y is a char, pop y and pushes the results of x.Split(y). Otherwise decompose the string into individual characters. If x is a Vector3, it is decomposed into doubles."; break;

				// Functions
				case 'I':
				case 'J':
				case 'H':
				case 'K':
				case '↥':
				case '↧':
				case '↤':
				case '↦':
					returnStr = "Pop x. Spawns a new pointer in the indicated direction (IJHK:↥↧↤↦:UDLR) with x mana. Costs the current IP (x-1) mana." + (substackFlag? " The active IP's current stack will be duplicated to the new IP's stack." : ""); break;
				case 'B': returnStr = "Pops y, pops x," + (modFlag?" ":"pushes (X,Y) where (X,Y) is where the IP would normally advance to, ")+"and the IP teleports to (x,y)."; break;
				case 'E':
					returnStr = "Pops a string and Evaluates it as Runic code, where intput/output is " + (modFlag?"from the eval'ing IP's stack":"standard in/out") +" Costs an amount of mana equal to ln(size-5)^2, where size is the length of the string to be Evaluated (min 0). If the value popped is numerical, instead a string from the Word Dictionary is pushed to the stack."; break;
				case 'w': returnStr = (modFlag? "Pops x, pops y, (if available, else IP's location is used). Reflectively reads the char at (x,y) in the current program." : "Pops a char c, pops x, pops y, (if available, else IP's location is used). Writes c into the current program at (x,y).") + "Costs 1 mana."; break;

				// Board edges
				case '\n':
				case '\r':
					return "";
				default: returnStr = "NOP"; break;
			}

			return returnStr + modifierText;
		}

		public static string DirectionModifierText(char modifier) {
			switch(modifier) {
				case '͔':
				case '᷾':
					return "Change direction: LEFT";
				case '͕':
				case '͐':
					return "Change direction: RIGHT";
				case '̭':
				case '̂':
					return "Change direction: UP";
				case '̬':
				case '̌':
					return "Change direction: DOWN";
				case '̖':
				case '̀':
					return "Change direction: Reflect Diagonal \\";
				case '̗':
				case '́':
					return "Change direction: Reflect Diagonal /";
				case '̩':
				case '̍':
					return "Change direction: Reflect Horizontal";
				case '̠':
				case '̄':
					return "Change direction: Reflect Vertical";
				case '̻':
				case '̊':
					return "Change direction: Reflect Orthogonal";
			}
			return "";
		}
	}
}
