using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace EsotericIDE
{
    abstract class ProgrammingLanguage
    {
        /// <summary>Returns a unique name for this language.</summary>
        public abstract string LanguageName { get; }
        /// <summary>Gets the default file extension for programs in this programming language, not including the dot (“.”).</summary>
        public abstract string DefaultFileExtension { get; }
        public abstract ExecutionEnvironment Compile(string source, string input);
        public abstract string GetInfo(string source, int cursorPosition);
        public virtual ToolStripMenuItem[] CreateMenus(Func<string> getSelectedText, Action<string> insertText, Func<ExecutionEnvironment> getEnv) { return new ToolStripMenuItem[0]; }
        public virtual LanguageSettings Settings { get { return null; } set { } }

        public override string ToString() { return LanguageName; }
    }
}
