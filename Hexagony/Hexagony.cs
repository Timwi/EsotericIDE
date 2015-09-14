using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using EsotericIDE.Hexagony;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace EsotericIDE.Languages
{
    sealed class Hexagony : ProgrammingLanguage
    {
        public override string LanguageName { get { return "Hexagony"; } }
        public override string DefaultFileExtension { get { return "hex"; } }

        public override ExecutionEnvironment Compile(string source, string input)
        {
            return new HexagonyEnv(source, input);
        }

        public override string GetInfo(string source, int cursorPosition)
        {
            if (cursorPosition >= source.Length)
                return "";

            switch (source[cursorPosition])
            {
                case '.': return "No-op.";
                case '@': return "Terminates the program.";
                case ')': return "Increments the current memory edge.";
                case '(': return "Decrements the current memory edge.";
                case '+': return "Sets the current memory edge to the sum of the left and right neighbours.";
                case '-': return "Sets the current memory edge to the difference of the left and right neighbours (left − right).";
                case '*': return "Sets the current memory edge to the product of the left and right neighbours.";
                case ':': return "Sets the current memory edge to the quotient of the left and right neighbours (left ÷ right, rounded towards negative infinity).";
                case '%': return "Sets the current memory edge to the modulo of the left and right neighbours (left % right, the sign of the result is the same as the sign of right).";
                case '~': return "Multiplies the current memory edge by −1.";

                case ',': return "Reads a single character from STDIN and sets the current memory edge to its Unicode codepoint. Returns -1 once EOF is reached.";
                case '?': return "Reads and discards from STDIN until a digit, a - or a + is found. Then reads as many characters as possible to form a valid (signed) decimal integer and sets the current memory edge to its value. Returns 0 once EOF is reached.";
                case ';': return "Interprets the current memory edge as a codepoint and writes the corresponding Unicode character to STDOUT.";
                case '!': return "Writes the decimal representation of the current memory edge to STDOUT.";

                case '_':
                case '|':
                case '/':
                case '\\': return "Mirror.";

                case '<':
                case '>': return "Mirror/branch.";

                case '[': return "Switches to the previous instruction pointer (wrapping around from 0 to 5).";
                case ']': return "Switches to the next instruction pointer (wrapping around from 5 to 0).";
                case '#': return "Takes the current memory edge modulo 6 and switches to the instruction pointer with that index.";

                case '{': return "Moves the memory pointer to the left neighbour.";
                case '}': return "Moves the memory pointer to the right neighbour.";
                case '"': return "Moves the memory pointer backwards to the left. This is equivalent to =}=.";
                case '\'': return "Moves the memory pointer backwards to the right. This is equivalent to ={=.";
                case '=': return "Reverses the direction of the memory pointer (this doesn’t affect the current memory edge, but changes which edges are considered the left and right neighbour).";
                case '^': return "Moves the memory pointer to the left neighbour if the current edge is zero or negative and to the right neighbour if it’s positive.";
                case '&': return "Copies the value of left neighbour into the current edge if the current edge is zero or negative and the value of the right neighbour if it’s positive.";

                default:
                    if (source[cursorPosition] >= '0' && source[cursorPosition] <= '9')
                        return "Multiply by 10 and add/subtract {0}.".Fmt(source[cursorPosition]);
                    if ((source[cursorPosition] >= 'a' && source[cursorPosition] <= 'z') || (source[cursorPosition] >= 'A' && source[cursorPosition] <= 'Z'))
                        return "Write ASCII code of {0} (= {1}) to memory.".Fmt(source[cursorPosition], (int) source[cursorPosition]);
                    return "";
            }
        }
    }
}
