using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EsotericIDE.Languages
{
    partial class Quipu
    {
        private quipuSettings _settings = new quipuSettings
        {
        };

        private sealed class quipuSettings : LanguageSettings
        {
        }

        public override LanguageSettings GetSettings()
        {
            return _settings;
        }

        public override void SetSettings(LanguageSettings settings)
        {
            if (!(settings is quipuSettings))
                throw new InvalidOperationException();
            _settings = (quipuSettings) settings;
        }
    }
}
