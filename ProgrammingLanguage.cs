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
        public abstract ToolStripMenuItem[] CreateMenus(Func<string> getSelectedText, Action<string> insertText);
        public virtual LanguageSettings GetSettings() { return null; }
        public virtual void SetSettings(LanguageSettings settings) { }

        public override string ToString() { return LanguageName; }
    }
}
