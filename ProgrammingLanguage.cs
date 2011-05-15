using System;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Windows.Forms;
using RT.Util;
using RT.Util.Dialogs;
using RT.Util.ExtensionMethods;
using RT.Util.Lingo;

namespace EsotericIDE
{
    abstract class ProgrammingLanguage
    {
        /// <summary>Returns a unique ID for this language.</summary>
        public abstract string LanguageID { get; }
        /// <summary>Gets the default file extension for programs in this programming language, not including the dot (“.”).</summary>
        public abstract string DefaultFileExtension { get; }
        public abstract ExecutionEnvironment Compile(string source);
        public abstract string GetInfo(string source, int cursorPosition);
        public abstract void InitialiseInsertMenu(ToolStripMenuItem mnuInsert, Func<string> getSelectedText, Action<string> insertText);
    }
}
