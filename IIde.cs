using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace EsotericIDE
{
    interface IIde
    {
        // Editing
        string GetSource();
        void ReplaceSource(string newSource);
        string GetSelectedText();
        void InsertText(string text);

        ExecutionEnvironment GetEnvironment();
    }
}
