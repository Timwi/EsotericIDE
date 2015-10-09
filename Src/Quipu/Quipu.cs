using System.Text.RegularExpressions;
using EsotericIDE.Quipu;
using RT.Util.ExtensionMethods;

namespace EsotericIDE.Languages
{
    sealed class Quipu : ProgrammingLanguage
    {
        public override string LanguageName { get { return "Quipu"; } }
        public override string DefaultFileExtension { get { return "qp"; } }

        public override ExecutionEnvironment Compile(string source, string input)
        {
            return new QuipuEnv(Program.Parse(source), input);
        }

        public override string GetInfo(string source, int cursorPosition)
        {
            var m = Regex.Match(source, @"^\s*""[^""]*""", RegexOptions.Singleline);
            if (m.Success && cursorPosition < m.Length)
                return "";

            int l = cursorPosition, r = cursorPosition;
            do { l--; } while (l >= 0 && source[l] != '\n');
            l++;
            while (r < source.Length && source[r] != '\r' && source[r] != '\n') r++;
            var currentLine = source.Substring(l, r - l);
            var lineIndex = l;

            l = cursorPosition - lineIndex;
            r = cursorPosition - lineIndex;
            do { l--; } while (l >= 0 && currentLine[l] != ' ');
            l++;
            while (r < currentLine.Length && currentLine[r] != ' ') r++;
            var currentInstruction = currentLine.Substring(l, r - l);

            string info;
            if (currentInstruction.StartsWith("'"))
                info = @"Pushes the string ""{0}"" onto the stack (or merges with a previous string, e.g. 'a followed by 'b pushes the string ""ab"" onto the stack).".Fmt(currentInstruction.Substring(1));
            else if ((m = Regex.Match(currentInstruction, @"^(\d)#$")).Success)
                info = @"Pushes the number {0}000 onto the stack (or merges with a previous number, e.g. 1# followed by 2# pushes 12000 onto the stack).".Fmt(m.Groups[1].Value);
            else if ((m = Regex.Match(currentInstruction, @"^(\d)%$")).Success)
                info = @"Pushes the number {0}00 onto the stack (or merges with a previous number, e.g. 1# followed by 2% pushes 1200 onto the stack).".Fmt(m.Groups[1].Value);
            else if ((m = Regex.Match(currentInstruction, @"^(\d)@$")).Success)
                info = @"Pushes the number {0}0 onto the stack (or merges with a previous number, e.g. 1% followed by 2@ pushes 120 onto the stack).".Fmt(m.Groups[1].Value);
            else if ((m = Regex.Match(currentInstruction, @"^(\d)&$")).Success)
                info = @"Pushes the number {0} onto the stack (or merges with a previous number, e.g. 1@ followed by 2& pushes 12 onto the stack).".Fmt(m.Groups[1].Value);
            else
            {
                switch (currentInstruction)
                {
                    case "\\n": info = @"Pushes a newline onto the stack (or merges with a previous string, e.g. 'a followed by \\n pushes the string ""a\\n"" onto the stack)."; break;
                    case "[]": info = "Pushes the value of thread n."; break;
                    case ";;": info = "Noop. Delimiter for numbers and strings."; break;
                    case "\\/": info = "Inputs a value."; break;
                    case "/\\": info = "Outputs a value."; break;
                    case "^^": info = "Pushes the value of the current thread."; break;
                    case "##": info = "Duplicates the last value on the stack."; break;
                    case "++": info = "Adds two values."; break;
                    case "--": info = "Subtracts the last value from the second-last."; break;
                    case "**": info = "Multiplies two values."; break;
                    case "//": info = "Divides the second-last value by the last and pushes the quotient."; break;
                    case "%%": info = "Divides the second-last value by the last and pushes the remainder."; break;
                    case "==": info = "Jumps to thread n (last value) if second-last value is equal to zero."; break;
                    case "<<": info = "Jumps to thread n (last value) if second-last value is less than zero."; break;
                    case "<=": info = "Jumps to thread n (last value) if second-last value is less than or equal to zero."; break;
                    case ">>": info = "Jumps to thread n (last value) if second-last value is greater than zero."; break;
                    case ">=": info = "Jumps to thread n (last value) if second-last value is greater than or equal to zero."; break;
                    case "??": info = "Jumps to thread n (last value)."; break;
                    case "::": info = "Halts the execution."; break;
                    default: return "";
                }
            }
            return currentInstruction + ": " + info;
        }
    }
}
