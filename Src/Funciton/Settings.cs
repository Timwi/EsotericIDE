using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using RT.Util;
using RT.Util.ExtensionMethods;
using RT.Util.Forms;

namespace EsotericIDE.Languages
{
    partial class Funciton
    {
        private funcitonSettings _settings = new funcitonSettings
        {
            AdditionalSourceFiles = new string[0],
            InputType = inputType.InterpretAsString,
            AnalyzeFunction = null,
            TraceFunctions = null
        };

        private enum inputType
        {
            InterpretAsString,
            InterpretAsInteger
        }

        private sealed class funcitonSettings : LanguageSettings
        {
            public string[] AdditionalSourceFiles;
            public inputType InputType;
            public string AnalyzeFunction;
            public string[] TraceFunctions;
            public ManagedForm.Settings FormSettings = new ManagedForm.Settings();
        }

        public override LanguageSettings GetSettings()
        {
            return _settings;
        }

        public override void SetSettings(LanguageSettings settings)
        {
            if (!(settings is funcitonSettings))
                throw new InvalidOperationException();
            _settings = (funcitonSettings) settings;
        }
    }
}
