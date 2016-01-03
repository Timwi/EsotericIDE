using EsotericIDE.Unreadable;

namespace EsotericIDE.Languages
{
    sealed class Unreadable : ProgrammingLanguage
    {
        public override string LanguageName { get { return "Unreadable"; } }
        public override string DefaultFileExtension { get { return "unr"; } }

        public override ExecutionEnvironment Compile(string source, string input)
        {
            return new UnreadableEnv(source, input);
        }

        public override string GetInfo(string source, int cursorPosition)
        {
            while (cursorPosition >= 0 && cursorPosition < source.Length && source[cursorPosition] == '"')
                cursorPosition--;

            if (cursorPosition < 0 || cursorPosition >= source.Length || source[cursorPosition] != '\'')
                return "";

            var i = cursorPosition + 1;
            while (i < source.Length && source[i] == '"')
                i++;

            switch (i - cursorPosition)
            {
                case 2: return "'\"X — Print X as a Unicode character and return X.";
                case 3: return "'\"\"X — Return X + 1.";
                case 4: return "'\"\"\" — Return 1.";
                case 5: return "'\"\"\"\"XY — Do both X and Y and return what Y returned.";
                case 6: return "'\"\"\"\"\"XY — Do Y while X is not 0 and return the last result.";
                case 7: return "'\"\"\"\"\"\"XY — Set variable number X to Y and return Y.";
                case 8: return "'\"\"\"\"\"\"\"X — Return the value of variable X, or 0 if the variable has never been assigned.";
                case 9: return "'\"\"\"\"\"\"\"\"X — Return X − 1.";
                case 10: return "'\"\"\"\"\"\"\"\"\"XYZ — If X is not 0, do Y, else do Z. Return what Y/Z returned.";
                case 11: return "'\"\"\"\"\"\"\"\"\"\" — Return a single Unicode character which has been read from stdin, or -1 if stdin is exhausted.";
                case 12: return "'\"\"\"\"\"\"\"\"\"\"\" — Debugger break.";
                default: return "";
            }
        }
    }
}
