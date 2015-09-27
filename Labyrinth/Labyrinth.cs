using EsotericIDE.Labyrinth;
using RT.Util.ExtensionMethods;

namespace EsotericIDE.Languages
{
    sealed class Labyrinth : ProgrammingLanguage
    {
        public override string LanguageName { get { return "Labyrinth"; } }
        public override string DefaultFileExtension { get { return "lab"; } }

        public override ExecutionEnvironment Compile(string source, string input)
        {
            return new LabyrinthEnv(source, input);
        }

        public override string GetInfo(string source, int cursorPosition)
        {
            if (cursorPosition < 0 || cursorPosition >= source.Length)
                return "";

            var ch = source[cursorPosition];
            switch (ch)
            {
                case '"': return "No-op.";
                case '\'': return "No-op.";
                case '@': return "Exit.";

                // Arithmetic
                case ')': return "Increments the top of the stack.";
                case '(': return "Decrements the top of the stack.";
                case '+': return "Pops two values from the stack and pushes their sum.";
                case '-': return "Pops y, pops x, pushes x − y.";
                case '*': return "Pops two values from the stack and pushes their product.";
                case '/': return "Pops y, pops x, pushes x ÷ y (integer division, rounded towards negative infinity).";
                case '%': return "Pops y, pops x, pushes x % y (modulo; the sign of the result is the same as the sign of y).";
                case '`': return "Multiplies the top of the stack by −1.";
                case '&': return "Pops two values from the stack and pushes their bitwise AND.";
                case '|': return "Pops two values from the stack and pushes their bitwise OR.";
                case '$': return "Pops two values from the stack and pushes their bitwise XOR.";
                case '~': return "Pops a value from the stack and pushes its bitwise NOT.";

                // Stack Manipulation
                case ':': return "Duplicates the top of the main stack.";
                case ';': return "Pops the top of the main stack and discards it.";
                case '}': return "Pops the top of the main stack and pushes it onto the auxiliary stack.";
                case '{': return "Pops the top of the auxiliary stack and pushes it onto the main stack.";
                case '=': return "Swaps the tops of both stacks.";
                case '#': return "Pushes the depth of the main stack onto the main stack (not counting the implicit zeroes at the bottom).";

                // I/O
                case ',': return "Read a single character from STDIN and push its byte value. Pushes −1 once EOF is reached.";
                case '?': return "Read and discard from STDIN until a digit, a - or a + is found. Then read as many characters as possible to form a valid (signed) decimal integer and push its value. Pushes 0 once EOF is reached.";
                case '.': return "Pop a value and write the corresponding character to STDOUT.";
                case '!': return "Pop a value and write its decimal representation to STDOUT.";
                case '\\': return "Print a newline/line feed character (0x0A).";

                // Grid Manipulation
                case '<': return "Pop a value and shift the corresponding source row cyclically to the left.";
                case '>': return "Pop a value and shift the corresponding source row cyclically to the right.";
                case '^': return "Pop a value and shift the corresponding source column cyclically up.";
                case 'v': return "Pop a value and shift the corresponding source column cyclically down.";

                // Digits
                case '_': return "Pushes a 0.";
                default:
                    if (ch >= '0' && ch <= '9')
                        return "Multiplies the top of the stack by 10 and then adds {0}.".Fmt(ch);
                    return "";
            }
        }
    }
}
